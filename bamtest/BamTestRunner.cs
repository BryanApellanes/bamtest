using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Bam.Test;

namespace BamTest
{
    public class BamTestRunner
    {
        private readonly TestProjectDiscovery _projectDiscovery;
        private readonly TestAssemblyDiscovery _assemblyDiscovery;
        private readonly TestProjectRunner _projectRunner;
        private readonly TestAssemblyRunner _assemblyRunner;

        public BamTestRunner()
        {
            _projectDiscovery = new TestProjectDiscovery();
            _assemblyDiscovery = new TestAssemblyDiscovery();
            _projectRunner = new TestProjectRunner();
            _assemblyRunner = new TestAssemblyRunner();
        }

        /// <summary>
        /// Discover test projects based on the provided options and run them all.
        /// </summary>
        /// <param name="testSwitch">The test switch (e.g., "ut", "it", "spec").</param>
        /// <param name="slnPath">Optional path to a .sln file.</param>
        /// <param name="dirPath">Optional directory path to scan for *.tests.csproj.</param>
        /// <param name="assemblyDirPath">Optional directory path to scan for pre-built test assemblies.</param>
        public BamTestSummary RunAll(string testSwitch, string? slnPath = null, string? dirPath = null, string? assemblyDirPath = null, CoverageOptions? coverage = null)
        {
            var summary = new BamTestSummary();

            if (!string.IsNullOrEmpty(assemblyDirPath))
            {
                RunAssemblies(assemblyDirPath, testSwitch, summary, coverage);
            }
            else
            {
                List<string> projects = DiscoverProjects(slnPath, dirPath);
                RunProjects(projects, testSwitch, summary, coverage);
            }

            if (coverage != null)
            {
                MergeCoverageReports(summary, coverage);
            }

            summary.PrintSummary();
            return summary;
        }

        /// <summary>
        /// Run a single test project by path.
        /// </summary>
        public BamTestSummary RunSingle(string csprojPath, string testSwitch, CoverageOptions? coverage = null)
        {
            var summary = new BamTestSummary();
            var result = _projectRunner.Run(csprojPath, testSwitch, coverage);
            summary.Results.Add(result);
            summary.PrintSummary();
            return summary;
        }

        /// <summary>
        /// Discover test projects based on discovery options.
        /// </summary>
        public List<string> DiscoverProjects(string? slnPath = null, string? dirPath = null)
        {
            if (!string.IsNullOrEmpty(slnPath))
            {
                Console.WriteLine($"Discovering test projects from solution: {slnPath}");
                return _projectDiscovery.FromSolution(slnPath);
            }

            if (!string.IsNullOrEmpty(dirPath))
            {
                Console.WriteLine($"Discovering test projects from directory: {dirPath}");
                return _projectDiscovery.FromDirectory(dirPath);
            }

            Console.WriteLine("Auto-discovering test projects...");
            return _projectDiscovery.AutoDiscover();
        }

        private void RunProjects(List<string> projects, string testSwitch, BamTestSummary summary, CoverageOptions? coverage = null)
        {
            if (projects.Count == 0)
            {
                Console.WriteLine("No test projects discovered.");
                return;
            }

            Console.WriteLine($"Discovered {projects.Count} test project(s):");
            for (int i = 0; i < projects.Count; i++)
            {
                Console.WriteLine($"  {i + 1}. {projects[i]}");
            }
            Console.WriteLine();

            foreach (string project in projects)
            {
                var result = _projectRunner.Run(project, testSwitch, coverage);
                summary.Results.Add(result);
            }
        }

        private void RunAssemblies(string assemblyDir, string testSwitch, BamTestSummary summary, CoverageOptions? coverage = null)
        {
            List<string> assemblies = _assemblyDiscovery.FromDirectory(assemblyDir);
            if (assemblies.Count == 0)
            {
                Console.WriteLine($"No test assemblies found in: {assemblyDir}");
                return;
            }

            Console.WriteLine($"Discovered {assemblies.Count} test assembly(ies):");
            for (int i = 0; i < assemblies.Count; i++)
            {
                Console.WriteLine($"  {i + 1}. {assemblies[i]}");
            }
            Console.WriteLine();

            foreach (string assembly in assemblies)
            {
                var result = _assemblyRunner.Run(assembly, testSwitch, coverage);
                summary.Results.Add(result);
            }
        }

        private void MergeCoverageReports(BamTestSummary summary, CoverageOptions coverage)
        {
            var coverageFiles = summary.Results
                .Where(r => !string.IsNullOrEmpty(r.CoverageOutputPath) && File.Exists(r.CoverageOutputPath))
                .Select(r => r.CoverageOutputPath!)
                .ToList();

            if (coverageFiles.Count == 0)
            {
                Console.WriteLine("No coverage files to merge.");
                return;
            }

            string mergedFile = $"bamtk.coverage.{CoverageOptions.GetExtensionForFormat(coverage.Format)}";
            string inputFiles = string.Join(" ", coverageFiles.Select(f => $"\"{f}\""));
            string mergeArgs = $"merge --output \"{mergedFile}\" --output-format {coverage.Format} {inputFiles}";

            Console.WriteLine();
            Console.WriteLine($"Merging {coverageFiles.Count} coverage report(s)...");

            var startInfo = new ProcessStartInfo
            {
                FileName = CoverageOptions.ToolName,
                Arguments = mergeArgs,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };

            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    Console.WriteLine(e.Data);
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    Console.Error.WriteLine(e.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                summary.MergedCoverageOutputPath = Path.GetFullPath(mergedFile);
                Console.WriteLine($"Merged coverage report: {summary.MergedCoverageOutputPath}");
            }
            else
            {
                Console.Error.WriteLine("Failed to merge coverage reports.");
            }
        }
    }
}
