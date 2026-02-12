using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Bam.Test;

namespace BamTest
{
    public class TestAssemblyRunner
    {
        private static readonly Regex SummaryRegex = new Regex(
            @"Test Summary:\s*(\d+)\s*passed,\s*(\d+)\s*failed",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Run a pre-built test assembly out-of-process via: dotnet &lt;assembly.dll&gt; --&lt;testSwitch&gt;
        /// Streams output to console in real-time and parses the test summary line.
        /// </summary>
        /// <param name="assemblyPath">Full path to the test assembly (.dll or .exe).</param>
        /// <param name="testSwitch">The test switch without leading dashes (e.g., "ut", "it", "spec").</param>
        public TestProjectResult Run(string assemblyPath, string testSwitch, CoverageOptions? coverage = null)
        {
            string assemblyName = Path.GetFileNameWithoutExtension(assemblyPath);
            Console.WriteLine();
            Console.WriteLine($"--- Running {assemblyName} (--{testSwitch}) ---");

            var result = new TestProjectResult
            {
                ProjectPath = assemblyPath,
                ProjectName = assemblyName
            };

            string fileName;
            string arguments;

            if (coverage != null)
            {
                string coverageFile = $"{assemblyName}.coverage.{GetExtensionForFormat(coverage.Format)}";
                fileName = CoverageOptions.ToolName;
                arguments = $"collect --output \"{coverageFile}\" --output-format {coverage.Format} -- dotnet \"{assemblyPath}\" --{testSwitch}";
                result.CoverageOutputPath = Path.GetFullPath(coverageFile);
            }
            else
            {
                fileName = "dotnet";
                arguments = $"\"{assemblyPath}\" --{testSwitch}";
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            var stopwatch = Stopwatch.StartNew();

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

            stopwatch.Stop();
            result.Duration = stopwatch.Elapsed;

            string status = result.Success ? "PASS" : "FAIL";
            Console.WriteLine($"--- {assemblyName}: [{status}] {result.Passed} passed, {result.Failed} failed ({result.Duration.TotalSeconds:F1}s) ---");
            Console.WriteLine();

            return result;
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

        private static string GetExtensionForFormat(string format)
        {
            return format.ToLowerInvariant() switch
            {
                "cobertura" => "cobertura.xml",
                "xml" => "xml",
                _ => format
            };
        }
    }
}
