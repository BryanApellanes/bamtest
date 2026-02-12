using System.Reflection;
using Bam.Console;
using Bam.Logging;
using Bam.Test;

namespace BamTest
{
    /// <summary>
    /// Custom ITestSwitchExecutor that runs tests across multiple projects out-of-process,
    /// rather than running tests in the current assembly.
    /// </summary>
    public class BamTestSwitchExecutor : ITestSwitchExecutor
    {
        public bool ExecuteTestSwitches(Assembly assembly, ILogger logger, IParsedArguments arguments)
        {
            CoverageOptions? coverage = CoverageOptions.FromArguments(arguments);
            if (coverage != null && !CoverageOptions.IsToolInstalled())
            {
                logger.Error("{0} is not installed. Install with: dotnet tool install --global {0}", CoverageOptions.ToolName, CoverageOptions.ToolName);
                BamConsoleContext.Exit(1);
            }

            bool executed = false;
            string[] testSwitches = { "ut", "spec", "it" };

            foreach (string sw in testSwitches)
            {
                if (arguments.Contains(sw))
                {
                    var runner = new BamTestRunner();

                    string? slnPath = null;
                    string? dirPath = null;
                    string? assemblyDir = null;

                    if (arguments.Contains("sln", out string? slnVal))
                    {
                        slnPath = slnVal;
                    }
                    if (arguments.Contains("dir", out string? dirVal))
                    {
                        dirPath = dirVal;
                    }
                    if (arguments.Contains("assemblyDir", out string? asmVal))
                    {
                        assemblyDir = asmVal;
                    }

                    var summary = runner.RunAll(sw, slnPath, dirPath, assemblyDir, coverage);
                    executed = true;

                    if (!summary.AllPassed)
                    {
                        BamConsoleContext.Exit(1);
                    }
                }
            }

            return executed;
        }
    }
}
