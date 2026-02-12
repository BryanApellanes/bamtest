using System;
using System.Collections.Generic;
using System.Linq;

namespace BamTest
{
    public class BamTestSummary
    {
        public BamTestSummary()
        {
            Results = new List<TestProjectResult>();
        }

        public List<TestProjectResult> Results { get; set; }
        public int TotalPassed => Results.Sum(r => r.Passed);
        public int TotalFailed => Results.Sum(r => r.Failed);
        public bool AllPassed => Results.All(r => r.Success);

        public void PrintSummary()
        {
            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine("  bamtest Aggregate Summary");
            Console.WriteLine("========================================");

            foreach (var result in Results)
            {
                string status = result.Success ? "PASS" : "FAIL";
                Console.WriteLine($"  [{status}] {result.ProjectName} - {result.Passed} passed, {result.Failed} failed ({result.Duration.TotalSeconds:F1}s)");
            }

            Console.WriteLine("----------------------------------------");
            Console.WriteLine($"  Total: {TotalPassed} passed, {TotalFailed} failed across {Results.Count} project(s)");
            Console.WriteLine($"  Result: {(AllPassed ? "ALL PASSED" : "FAILURES DETECTED")}");

            var coverageFiles = Results.Where(r => r.CoverageOutputPath != null).ToList();
            if (coverageFiles.Count > 0)
            {
                Console.WriteLine("----------------------------------------");
                Console.WriteLine("  Coverage reports:");
                foreach (var r in coverageFiles)
                {
                    Console.WriteLine($"    {r.ProjectName}: {r.CoverageOutputPath}");
                }
            }

            Console.WriteLine("========================================");
            Console.WriteLine();
        }
    }
}
