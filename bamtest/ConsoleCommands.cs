using System;
using System.Collections.Generic;
using Bam.Console;
using Bam.DependencyInjection;
using Bam.Services;
using Bam.Shell;

namespace BamTest
{
    [ConsoleMenu("bamtest options")]
    public class ConsoleCommands : ConsoleMenuContainer
    {
        public ConsoleCommands(ServiceRegistry serviceRegistry) : base(serviceRegistry)
        {
        }

        [ConsoleCommand("Run all unit tests", "Run all unit tests")]
        [MenuItem("Run All Unit Tests", "Run all unit tests")]
        public void RunAllUnitTests()
        {
            RunAllTests("ut");
        }

        [ConsoleCommand("Run unit test")]
        [MenuItem("Run Unit Test")]
        public void RunUnitTest()
        {
            RunSelectedTest("ut");
        }

        [ConsoleCommand("Run all spec tests", "Run all spec tests")]
        [MenuItem("Run All Spec Tests", "Run all spec tests")]
        public void RunAllSpecTests()
        {
            RunAllTests("spec");
        }

        [ConsoleCommand("Run spec test", "Run spec test")]
        [MenuItem("Run Spec Test", "Run spec test")]
        public void RunSpecTest()
        {
            RunSelectedTest("spec");
        }

        [ConsoleCommand("Run all integration tests", "Run all integration tests")]
        [MenuItem("Run All Integration Tests", "Run all integration tests")]
        public void RunAllIntegrationTests()
        {
            RunAllTests("it");
        }

        [ConsoleCommand("Run integration test", "Run integration test")]
        [MenuItem("Run Integration Test", "Run integration test")]
        public void RunIntegrationTest()
        {
            RunSelectedTest("it");
        }

        private void RunAllTests(string testSwitch)
        {
            var runner = new BamTestRunner();
            IParsedArguments args = BamConsoleContext.Current.Arguments;

            string? slnPath = null;
            string? dirPath = null;
            string? assemblyDir = null;

            if (args.Contains("sln", out string? slnVal))
            {
                slnPath = slnVal;
            }
            if (args.Contains("dir", out string? dirVal))
            {
                dirPath = dirVal;
            }
            if (args.Contains("assemblyDir", out string? asmVal))
            {
                assemblyDir = asmVal;
            }

            runner.RunAll(testSwitch, slnPath, dirPath, assemblyDir);
        }

        private void RunSelectedTest(string testSwitch)
        {
            var runner = new BamTestRunner();
            IParsedArguments args = BamConsoleContext.Current.Arguments;

            string? slnPath = null;
            string? dirPath = null;

            if (args.Contains("sln", out string? slnVal))
            {
                slnPath = slnVal;
            }
            if (args.Contains("dir", out string? dirVal))
            {
                dirPath = dirVal;
            }

            List<string> projects = runner.DiscoverProjects(slnPath, dirPath);
            if (projects.Count == 0)
            {
                Console.WriteLine("No test projects found.");
                return;
            }

            Console.WriteLine("Select a test project:");
            for (int i = 0; i < projects.Count; i++)
            {
                Console.WriteLine($"  {i + 1}. {System.IO.Path.GetFileNameWithoutExtension(projects[i])}");
            }
            Console.Write("Enter number: ");

            string? input = Console.ReadLine();
            if (int.TryParse(input, out int selection) && selection >= 1 && selection <= projects.Count)
            {
                runner.RunSingle(projects[selection - 1], testSwitch);
            }
            else
            {
                Console.WriteLine("Invalid selection.");
            }
        }
    }
}
