namespace BamTest
{
    /// <summary>
    /// The MSBuild properties bamtest passes to the builds it starts with <c>dotnet run --project</c>.
    /// </summary>
    /// <remarks>
    /// A test project built this way is built outside <c>bamtk.sln</c>, so the solution's switches don't apply.
    /// <see cref="PlatformTargetsArgument"/> tells a project with platform targets (bam.maui's Android, iOS,
    /// Mac Catalyst and Windows builds) to build only its plain target, which needs no workload. It is passed as a
    /// property on the command line, never set in the environment: an environment variable would reach every
    /// build that inherits it, an app's included (bam.maui ignores one and warns, BAMMAUI001).
    /// </remarks>
    public static class TestBuildProperties
    {
        /// <summary>Turns platform targets off for the test project's build.</summary>
        /// <remarks>The long form: on <c>dotnet run</c>, <c>-p</c> has meant <c>--project</c>.</remarks>
        public const string PlatformTargetsArgument = "--property:BamPlatformTargets=false";

        /// <summary>The property's name, which bamtest keeps out of the environment of the builds it starts.</summary>
        public const string PlatformTargetsVariable = "BamPlatformTargets";

        /// <summary>The arguments of <c>dotnet</c> for running one test project.</summary>
        /// <param name="csprojPath">Full path to the test project.</param>
        /// <param name="testSwitch">The test switch without leading dashes (e.g. "ut").</param>
        public static string RunArguments(string csprojPath, string testSwitch)
        {
            return $"run --project \"{csprojPath}\" {PlatformTargetsArgument} -- --{testSwitch}";
        }

        /// <summary>The arguments of the coverage tool for running one test project under it.</summary>
        /// <param name="collectArguments">The coverage tool's own arguments, ending before the command it runs.</param>
        /// <param name="csprojPath">Full path to the test project.</param>
        /// <param name="testSwitch">The test switch without leading dashes (e.g. "ut").</param>
        public static string CoveredRunArguments(string collectArguments, string csprojPath, string testSwitch)
        {
            return $"{collectArguments} -- dotnet {RunArguments(csprojPath, testSwitch)}";
        }
    }
}
