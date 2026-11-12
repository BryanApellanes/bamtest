using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Bam.Test;

namespace BamTest
{
    public class TestProjectRunner
    {
        private static readonly Regex SummaryRegex = new Regex(
            @"Test Summary:\s*(\d+)\s*passed,\s*(\d+)\s*failed",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Run a test project out-of-process via: dotnet run --project &lt;csproj&gt; --property:BamPlatformTargets=false -- --&lt;testSwitch&gt;
        /// Streams output to console in real-time and parses the test summary line.
        /// </summary>
        /// <param name="csprojPath">Full path to the .csproj file.</param>
        /// <param name="testSwitch">The test switch without leading dashes (e.g., "ut", "it", "spec").</param>
        public TestProjectResult Run(string csprojPath, string testSwitch, CoverageOptions? coverage = null)
        {
            string projectName = Path.GetFileNameWithoutExtension(csprojPath);
            Console.WriteLine();
            Console.WriteLine($"--- Running {projectName} (--{testSwitch}) ---");

            var result = new TestProjectResult
            {
                ProjectPath = csprojPath,
                ProjectName = projectName
            };

            string? collectArguments = null;
            string? settingsPath = null;

            if (coverage != null)
            {
                string coverageFile = $"{projectName}.coverage.{CoverageOptions.GetExtensionForFormat(coverage.Format)}";
                settingsPath = coverage.GenerateSettingsFile();
                string settingsArg = settingsPath != null ? $" --settings \"{settingsPath}\"" : "";
                collectArguments = $"collect --output \"{coverageFile}\" --output-format {coverage.Format}{settingsArg}";
                result.CoverageOutputPath = Path.GetFullPath(coverageFile);
            }

            ProcessStartInfo startInfo = CreateStartInfo(csprojPath, testSwitch, collectArguments);

            var stopwatch = Stopwatch.StartNew();

            try
            {
                using (var process = new Process { StartInfo = startInfo })
                {
                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data != null)
                        {
                            Console.WriteLine(e.Data);
                            ParseSummaryLine(e.Data, result);
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

                    result.ExitCode = process.ExitCode;
                }
            }
            finally
            {
                CoverageOptions.CleanupSettingsFile(settingsPath);
            }

            stopwatch.Stop();
            result.Duration = stopwatch.Elapsed;

            string status = result.Success ? "PASS" : "FAIL";
            Console.WriteLine($"--- {projectName}: [{status}] {result.Passed} passed, {result.Failed} failed ({result.Duration.TotalSeconds:F1}s) ---");
            Console.WriteLine();

            return result;
        }

        /// <summary>
        /// The start info for one test project run: <c>dotnet run</c> with <see cref="TestBuildProperties.PlatformTargetsArgument"/>,
        /// under the coverage tool when <paramref name="collectArguments"/> is given.
        /// </summary>
        /// <param name="csprojPath">Full path to the test project.</param>
        /// <param name="testSwitch">The test switch without leading dashes (e.g. "ut").</param>
        /// <param name="collectArguments">The coverage tool's own arguments, or null to run without coverage.</param>
        /// <remarks>
        /// A <c>BamPlatformTargets</c> in bamtest's own environment is taken out of the child's (any letter case): the
        /// property on the command line is what counts, and an inherited variable would only make bam.maui warn
        /// (BAMMAUI001). bamtest's own environment is not changed.
        /// </remarks>
        public static ProcessStartInfo CreateStartInfo(string csprojPath, string testSwitch, string? collectArguments)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = collectArguments is null ? "dotnet" : CoverageOptions.ToolName,
                Arguments = collectArguments is null
                    ? TestBuildProperties.RunArguments(csprojPath, testSwitch)
                    : TestBuildProperties.CoveredRunArguments(collectArguments, csprojPath, testSwitch),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            foreach (string name in startInfo.Environment.Keys.Where(key => string.Equals(key, TestBuildProperties.PlatformTargetsVariable, StringComparison.OrdinalIgnoreCase)).ToList())
            {
                startInfo.Environment.Remove(name);
            }
            return startInfo;
        }

        private void ParseSummaryLine(string line, TestProjectResult result)
        {
            var match = SummaryRegex.Match(line);
            if (match.Success)
            {
                result.Passed = int.Parse(match.Groups[1].Value);
                result.Failed = int.Parse(match.Groups[2].Value);
            }
        }
    }
}
