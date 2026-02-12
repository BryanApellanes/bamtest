using Bam.Console;
using BamTest;

namespace Bam.Application
{
    class Program
    {
        static void Main(string[] args)
        {
            BamConsoleContext.Current.AddValidArgument("sln", description: "Path to solution file for test project discovery.");
            BamConsoleContext.Current.AddValidArgument("dir", description: "Directory to scan for *.tests.csproj files.");
            BamConsoleContext.Current.AddValidArgument("assemblyDir", false, true, description: "Directory to scan for pre-built *tests.dll assemblies.");

            // Override the default TestSwitchExecutor (from bam.test) with one that
            // runs tests across multiple projects out-of-process.
            BamConsoleContext.Current.ServiceRegistry.Set<ITestSwitchExecutor>(new BamTestSwitchExecutor());

            BamConsoleContext.Current.Main(args);
        }
    }
}
