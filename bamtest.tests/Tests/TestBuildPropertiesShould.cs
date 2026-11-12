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

    private sealed record RunCase(string Project, string Switch);

    private sealed record ArgumentsOutcome(string Plain, string Covered);
}
