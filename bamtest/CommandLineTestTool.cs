using Bam.Console;
using Bam.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Bam.Test.Integration;
using Bam.Test.Specification;
using Bam.Test.Unit;

namespace Bam.Test
{
    [Obsolete("Use UnitTestMenuContainer instead")]
    public class CommandLineTestTool : CommandLineTool
    {
        static CommandLineTestTool()
        {
            GetUnitTestRunListeners = () => new List<ITestRunListener<UnitTestMethod>>();
            GetSpecTestRunListeners = () => new List<ITestRunListener<SpecTestMethod>>();
            //InitLogger();
        }

        protected internal static Func<IEnumerable<ITestRunListener<UnitTestMethod>>> GetUnitTestRunListeners
        {
            get;
            set;
        }

        protected internal static Func<IEnumerable<ITestRunListener<SpecTestMethod>>> GetSpecTestRunListeners
        {
            get;
            set;
        }

        public static void RunSpecTests()
        {
            if (Arguments.Contains("group"))
            {
                RunSpecTestGroup(Assembly.GetEntryAssembly(), Arguments["group"]);
            }
            else
            {
                RunAllSpecTests(Assembly.GetEntryAssembly());
            }
        }

        public static void RunUnitTests()
        {
            if (Arguments.Contains("group"))
            {
                RunUnitTestGroup(Assembly.GetEntryAssembly(), Arguments["group"]);
            }
            else
            {
                RunAllUnitTests(Assembly.GetEntryAssembly());
            }
        }

        public static EventHandler DefaultPassedHandler;
        public static EventHandler DefaultFailedHandler;

        public static void RunAllSpecTests(Assembly assembly, ILogger logger = null, EventHandler passedHandler = null, EventHandler failedHandler = null)
        {
            passedHandler = passedHandler ?? DefaultPassedHandler;
            failedHandler = failedHandler ?? DefaultFailedHandler;
            ITestRunner<SpecTestMethod> runner = GetSpecTestRunner(assembly, logger);
            AttachHandlers<SpecTestMethod>(passedHandler, failedHandler, runner);
            AttachSpecTestRunListeners(runner);
            runner.RunAllTests();
        }

        public static void RunSpecTestGroup(Assembly assembly, string testGroup, ILogger logger = null, EventHandler passedHandler = null, EventHandler failedHandler = null)
        {
            passedHandler = passedHandler ?? DefaultPassedHandler;
            failedHandler = failedHandler ?? DefaultFailedHandler;
            ITestRunner<SpecTestMethod> runner = GetSpecTestRunner(assembly, logger);
            AttachHandlers<SpecTestMethod>(passedHandler, failedHandler, runner);
            AttachSpecTestRunListeners(runner);
            runner.RunTestGroup(testGroup);
        }

        public static void RunAllUnitTests(Assembly assembly, ILogger logger = null, EventHandler passedHandler = null, EventHandler failedHandler = null)
        {
            passedHandler = passedHandler ?? DefaultPassedHandler;
            failedHandler = failedHandler ?? DefaultFailedHandler;
            ITestRunner<UnitTestMethod> runner = GetUnitTestRunner(assembly, logger);
            AttachHandlers<UnitTestMethod>(passedHandler, failedHandler, runner);
            AttachUnitTestRunListeners(runner);
            runner.RunAllTests();
        }

        public static void RunUnitTestGroup(Assembly assembly, string testGroup, ILogger logger = null, EventHandler passedHandler = null, EventHandler failedHandler = null)
        {
            passedHandler = passedHandler ?? DefaultPassedHandler;
            failedHandler = failedHandler ?? DefaultFailedHandler;
            ITestRunner<UnitTestMethod> runner = GetUnitTestRunner(assembly, logger);
            AttachHandlers<UnitTestMethod>(passedHandler, failedHandler, runner);
            AttachUnitTestRunListeners(runner);
            runner.RunTestGroup(testGroup);
        }

        public static void RunIntegrationTests()
        {
            IntegrationTestRunnerObsolete.RunIntegrationTests(Assembly.GetEntryAssembly());
        }

        public static void UnitTestMenu(Assembly assembly, CommandLine.ConsoleMenu[] otherMenus, string header)
        {
            System.Console.WriteLine(header);
            ITestRunner<UnitTestMethod> runner = GetUnitTestRunner(assembly, Log.Default);
            ShowActions(runner.GetTests());
            System.Console.WriteLine();
            System.Console.WriteLine("Q to quit\ttype all to run all tests.");
            string answer = ShowSelectedMenuOrReturnAnswer(otherMenus);
            System.Console.WriteLine();

            try
            {
                answer = answer.Trim().ToLowerInvariant();
                runner.RunSpecifiedTests(answer);
            }
            catch (Exception ex)
            {
                Error("An error occurred running tests", ex);
            }

            if (Confirm("Return to the Test menu? [y][N]"))
            {
                UnitTestMenu(assembly, otherMenus, header);
            }
            else
            {
                Exit(0);
            }
        }
        
        protected static void ShowActions<TConsoleMethod>(List<TConsoleMethod> actions) where TConsoleMethod : ConsoleMethod
        {
            for (int i = 1; i <= actions.Count; i++)
            {
                ConsoleMethod consoleMethod = actions[i - 1];
                string menuOption = consoleMethod.Information;
                System.Console.WriteLine("{0}. {1}", i, menuOption);
            }
        }
        protected internal static ITestRunner<SpecTestMethod> GetSpecTestRunner(Assembly assembly, ILogger logger)
        {
            return GetTestRunner<SpecTestMethod>(assembly, logger);
        }

        protected internal static ITestRunner<UnitTestMethod> GetUnitTestRunner(Assembly assembly, ILogger logger)
        {
            return GetTestRunner<UnitTestMethod>(assembly, logger);
        }

        protected internal static ITestRunner<TTestMethod> GetTestRunner<TTestMethod>(Assembly assembly, ILogger logger) where TTestMethod : TestMethod
        {
            ITestRunner<TTestMethod> runner = TestRunner<TTestMethod>.Create(assembly, logger);
            if (Arguments != null && Arguments.Contains("tag"))
            {
                runner.Tag = Arguments["tag"];
            }
            runner.NoTestsDiscovered += (o, e) => Message.PrintLine("No tests were found in {0}", ConsoleColor.Yellow, assembly.FullName);
            runner.TestsDiscovered += (o, e) =>
            {
                TestsDiscoveredEventArgs<TTestMethod> args = (TestsDiscoveredEventArgs<TTestMethod>)e;
                Message.PrintLine("Running all tests in {0}", ConsoleColor.Green, args.Assembly.FullName);
                Message.PrintLine("\tFound {0} tests", ConsoleColor.Cyan, args.Tests.Count);
            };
            runner.TestPassed += (o, e) =>
            {
                TestEventArgs<TTestMethod> args = (TestEventArgs<TTestMethod>)e;
                Pass(args.Test.Information);
            };
            runner.TestFailed += (o, t) =>
            {
                TestExceptionEventArgs args = (TestExceptionEventArgs)t;
                Message.PrintLine("Test Failed: ({0})", ConsoleColor.Red, args.TestMethod.ToString());
                Message.PrintLine(args.Exception.Message, ConsoleColor.Magenta);
                Message.PrintLine();
                Message.PrintLine(args.Exception.StackTrace, ConsoleColor.Red);
                Message.PrintLine("---", ConsoleColor.Red);
            };
            runner.TestsFinished += (o, e) =>
            {
                TestEventArgs<TTestMethod> args = (TestEventArgs<TTestMethod>)e;
                TestRunnerSummary summary = args.TestRunner.TestSummary;

                Message.PrintLine("********", ConsoleColor.Blue);
                if (summary.FailedTests.Count > 0)
                {
                    Message.PrintLine("({0}) tests passed", ConsoleColor.Green, summary.PassedTests.Count);
                    Message.PrintLine("({0}) tests failed", ConsoleColor.Red, summary.FailedTests.Count);
                    StringBuilder failedTests = new StringBuilder();
                    summary.FailedTests.ForEach(cim =>
                    {
                        MethodInfo method = cim.Test.Method;
                        Type type = method.DeclaringType;
                        string testIdentifier = $"{type.Namespace}.{type.Name}.{method.Name}";
                        failedTests.AppendFormat("\t{0}: ({1}) => {2}\r\n", testIdentifier, cim.Test.Information, cim.Exception?.Message ?? "[no message]");
                    });
                    Message.PrintLine("FAILED TESTS: \r\n {0})", new ConsoleColorCombo(ConsoleColor.Yellow, ConsoleColor.Red), failedTests.ToString());
                }
                else
                {
                    Message.PrintLine("All ({0}) tests passed", ConsoleColor.Green, ConsoleColor.Black, summary.PassedTests.Count);
                }
                Message.PrintLine("********", ConsoleColor.Blue, ConsoleColor.Black);
            };
            return runner;
        }

        private static void AttachSpecTestRunListeners(ITestRunner<SpecTestMethod> runner)
        {
            foreach (ITestRunListener<SpecTestMethod> listener in GetSpecTestRunListeners())
            {
                listener.Tag = runner.Tag;
                listener.Listen(runner);
            }
        }

        private static void AttachUnitTestRunListeners(ITestRunner<UnitTestMethod> runner)
        {
            foreach (ITestRunListener<UnitTestMethod> listener in GetUnitTestRunListeners())
            {
                listener.Tag = runner.Tag;
                listener.Listen(runner);
            }
        }

        private static void AttachHandlers<TTestMethod>(EventHandler passedHandler, EventHandler failedHandler, ITestRunner<TTestMethod> runner) where TTestMethod : TestMethod
        {
            if (passedHandler != null)
            {
                runner.TestPassed += passedHandler;
            }
            if (failedHandler != null)
            {
                runner.TestFailed += failedHandler;
            }

            if (DefaultPassedHandler != null)
            {
                runner.TestPassed += DefaultPassedHandler;
            }

            if (DefaultFailedHandler != null)
            {
                runner.TestFailed += DefaultFailedHandler;
            }
        }
    }
}
