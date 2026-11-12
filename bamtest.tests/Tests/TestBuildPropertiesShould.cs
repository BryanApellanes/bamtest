using Bam.DependencyInjection;
using Bam.Test;
using BamTest;

namespace Bam.Test.Runner.Tests;

/// <summary>
/// Covers bamtest#3: the builds bamtest starts with <c>dotnet run</c> get <c>--property:BamPlatformTargets=false</c>,
/// with and without coverage, and nothing goes into the environment.
/// </summary>
[UnitTestMenu("Test build properties should", Selector = "tbprop")]
public class TestBuildPropertiesShould : UnitTestMenuContainer
{
    public TestBuildPropertiesShould(ServiceRegistry serviceRegistry) : base(serviceRegistry)
    {
    }

    [UnitTest]
    public void TurnPlatformTargetsOffForEveryRun()
    {
        After.Setup(reg => reg.Set(new RunCase("C:\\src\\a b\\x.tests.csproj", "ut")))
        .When<RunCase>("builds the arguments for a plain run and a coverage run", (run, reg) =>
        {
            string plain = TestBuildProperties.RunArguments(run.Project, run.Switch);
            string covered = TestBuildProperties.CoveredRunArguments("collect --output \"x.coverage.xml\" --output-format cobertura", run.Project, run.Switch);
            return new ArgumentsOutcome(plain, covered);
        })
        .TheTest
        .ShouldPass<ArgumentsOutcome>((because, outcome) =>
        {
            because.ItsTrue("a plain run passes the property before the test switch, got " + outcome.Plain,
                outcome.Plain == "run --project \"C:\\src\\a b\\x.tests.csproj\" --property:BamPlatformTargets=false -- --ut");
            because.ItsTrue("a coverage run passes it to the dotnet run it wraps, got " + outcome.Covered,
                outcome.Covered == "collect --output \"x.coverage.xml\" --output-format cobertura -- dotnet run --project \"C:\\src\\a b\\x.tests.csproj\" --property:BamPlatformTargets=false -- --ut");
            because.ItsTrue("the long form, since -p on dotnet run has meant --project", !outcome.Plain.Contains(" -p:"));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void StartEveryTestProjectRunWithThePropertyAndNotTheVariable()
    {
        string? before = Environment.GetEnvironmentVariable(TestBuildProperties.PlatformTargetsVariable);
        After.Setup(reg => reg.Set(new RunCase("C:\\src\\x.tests.csproj", "ut")))
        .When<RunCase>("creates the start info for a plain and a coverage run while bamtest's own environment has the variable", (run, reg) =>
        {
            try
            {
                Environment.SetEnvironmentVariable(TestBuildProperties.PlatformTargetsVariable, "false");
                System.Diagnostics.ProcessStartInfo plain = TestProjectRunner.CreateStartInfo(run.Project, run.Switch, null);
                System.Diagnostics.ProcessStartInfo covered = TestProjectRunner.CreateStartInfo(run.Project, run.Switch, "collect --output \"x.coverage.xml\" --output-format cobertura");
                return new StartOutcome(
                    plain.FileName,
                    plain.Arguments,
                    covered.FileName,
                    covered.Arguments,
                    plain.Environment.Keys.Any(key => string.Equals(key, TestBuildProperties.PlatformTargetsVariable, StringComparison.OrdinalIgnoreCase)),
                    covered.Environment.Keys.Any(key => string.Equals(key, TestBuildProperties.PlatformTargetsVariable, StringComparison.OrdinalIgnoreCase)),
                    plain.Environment.ContainsKey("PATH") || plain.Environment.ContainsKey("Path"),
                    Environment.GetEnvironmentVariable(TestBuildProperties.PlatformTargetsVariable));
            }
            finally
            {
                Environment.SetEnvironmentVariable(TestBuildProperties.PlatformTargetsVariable, before);
            }
        })
        .TheTest
        .ShouldPass<StartOutcome>((because, outcome) =>
        {
            because.ItsTrue("a plain run is dotnet, got " + outcome.PlainFile, outcome.PlainFile == "dotnet");
            because.ItsTrue("with the property, got " + outcome.PlainArguments, outcome.PlainArguments == "run --project \"C:\\src\\x.tests.csproj\" --property:BamPlatformTargets=false -- --ut");
            because.ItsTrue("a coverage run is the coverage tool, got " + outcome.CoveredFile, outcome.CoveredFile == "dotnet-coverage");
            because.ItsTrue("wrapping the same dotnet run, got " + outcome.CoveredArguments, outcome.CoveredArguments == "collect --output \"x.coverage.xml\" --output-format cobertura -- dotnet run --project \"C:\\src\\x.tests.csproj\" --property:BamPlatformTargets=false -- --ut");
            because.ItsTrue("the plain child does not inherit the variable", !outcome.PlainHasVariable);
            because.ItsTrue("nor the coverage child", !outcome.CoveredHasVariable);
            because.ItsTrue("the rest of the environment is inherited", outcome.KeepsPath);
            because.ItsTrue("bamtest's own environment is left alone, got " + outcome.Own, outcome.Own == "false");
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    private sealed record RunCase(string Project, string Switch);

    private sealed record StartOutcome(string PlainFile, string PlainArguments, string CoveredFile, string CoveredArguments, bool PlainHasVariable, bool CoveredHasVariable, bool KeepsPath, string? Own);

    private sealed record ArgumentsOutcome(string Plain, string Covered);
}
