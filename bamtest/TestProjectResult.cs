using System;

namespace BamTest
{
    public class TestProjectResult
    {
        public string ProjectPath { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public int ExitCode { get; set; }
        public int Passed { get; set; }
        public int Failed { get; set; }
        public TimeSpan Duration { get; set; }
        public bool Success => ExitCode == 0 && Failed == 0;
    }
}
