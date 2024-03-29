﻿using Bam.Net.CommandLine;
using Bam.Net.ExceptionHandling;
using Bam.Net.Logging;
using Bam.Net.Testing.Specification;
using Bam.Net.Testing.Unit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Net.Testing
{
    public abstract class TestRunner<TTestMethod> : Loggable, ITestRunner<TTestMethod> where TTestMethod : TestMethod
    {
        static readonly Dictionary<Type, Func<Assembly, ILogger, ITestRunner<TTestMethod>>> _factory;
        static TestRunner()
        {
            _factory = new Dictionary<Type, Func<Assembly, ILogger, ITestRunner<TTestMethod>>>
            {
                { typeof(UnitTestMethod), (assembly, logger) => (ITestRunner<TTestMethod>)new UnitTestRunner(assembly, logger) },
                { typeof(SpecTestMethod), (assembly, logger )=> (ITestRunner<TTestMethod>)new SpecTestRunner(assembly, logger) }
                // TODO: 
                // refactor Integration test handling to match established convention for Unit and Spec then 
                // add IntegrationTest here
            };
        }

        readonly Lazy<List<TTestMethod>> _tests;
        public TestRunner(Assembly assembly, TestMethodProvider<TTestMethod> testMethodProvider, ILogger logger = null)
        {
            Assembly = assembly;
            TestMethodProvider = testMethodProvider;
            TestSummary = new TestRunnerSummary();
            IsolateMethodCalls = true;
            logger = logger ?? Log.Default;

            _tests = new Lazy<List<TTestMethod>>(() => TestMethodProvider.GetTests());
            
            Subscribe(logger);
            AttachBeforeHandlers();
            AttachAfterHandlers();
        }

        public static ITestRunner<TTestMethod> Create(Assembly assembly, ILogger logger = null)
        {
            if (!_factory.ContainsKey(typeof(TTestMethod)))
            {
                Args.Throw<InvalidOperationException>("Invalid TTestMethod type specified: {0}", typeof(TTestMethod).Name);
            }
            return _factory[typeof(TTestMethod)](assembly, logger);
        }

        public string Tag { get; set; }

        public event EventHandler TestIgnored;

        public event EventHandler TestPassed;
        public event EventHandler TestFailed;

        public event EventHandler TestsStarting;
        public event EventHandler TestsFinished;
        public event EventHandler TestStarting;
        public event EventHandler TestFinished;

        public event EventHandler TestsDiscovered;
        public event EventHandler NoTestsDiscovered;

        public event EventHandler InvalidTestNumberSpecified;

        public Action<Exception> BeforeAllExceptionHandler { get; set; }
        public Action<Exception> BeforeEachExceptionHandler { get; set; }

        public Action<Exception> AfterAllExceptionHandler { get; set; }
        public Action<Exception> AfterEachExceptionHandler { get; set; }

        public TestRunnerSummary TestSummary { get; set; }
        
        public TestMethodProvider<TTestMethod> TestMethodProvider { get; set; }

        public ISetupMethodProvider SetupMethodProvider { get; set; }

        public ITeardownMethodProvider TeardownMethodProvider { get; set; }

        public bool IsolateMethodCalls { get; set; }
        public void RunAllTests()
        {
            List<TTestMethod> tests = TestMethodProvider.GetTests();
            RunTestList(tests);
        }

        public void RunTestGroup(string testGroup)
        {
            Args.ThrowIfNullOrEmpty(testGroup, "testGroup");
            List<TTestMethod> tests = TestMethodProvider.GetTests(testGroup);
            RunTestList(tests);
        }

        private void RunTestList(List<TTestMethod> tests)
        {
            if (tests.Count == 0)
            {
                FireEvent(NoTestsDiscovered);
                return;
            }

            FireEvent(TestsDiscovered, new TestsDiscoveredEventArgs<TTestMethod>
            {
                Assembly = Assembly, 
                TestRunner = this, 
                Tests = tests.Select(t => (TestMethod) t).ToList()
            });
            FireEvent(TestsStarting, new TestEventArgs<TTestMethod> {TestRunner = this, Assembly = Assembly});
            foreach (TTestMethod test in tests)
            {
                RunTest(test);
            }

            FireEvent(TestsFinished, new TestEventArgs<TTestMethod> {TestRunner = this, Assembly = Assembly});
        }

        /// <summary>
        /// Run the tests specified by testIdentifier which should be in the 
        /// following forms: N1,N2,N3... or N1-Nn or N or "all"
        /// </summary>
        /// <param name="testIdentifier"></param>
        public void RunSpecifiedTests(string testIdentifier)
        {
            string[] individualTests = testIdentifier.DelimitSplit(",", true);
            string[] range = testIdentifier.DelimitSplit("-", true);
            if (testIdentifier.Equals("all", StringComparison.InvariantCultureIgnoreCase))
            {
                RunAllTests();
            }
            else if (range.Length == 2)
            {
                RunTestRange(range[0], range[1]);
            }
            else if (individualTests.Length > 1)
            {
                RunTestSet(individualTests);
            }
            else
            {
                FireEvent(TestsStarting, new TestEventArgs<TTestMethod> { TestRunner = this, Assembly = Assembly });
                RunTest(testIdentifier.Trim());
                FireEvent(TestsFinished, new TestEventArgs<TTestMethod> { TestRunner = this, Assembly = Assembly });
            }
        }

        public void RunTestRange(string fromNumber, string toNumber)
        {
            if(IsInRange(fromNumber, out int fromInt) && IsInRange(toNumber, out int toInt))
            {
                FireEvent(TestsStarting, new TestEventArgs<TTestMethod> { TestRunner = this, Assembly = Assembly });
                for (int i = fromInt; i <= toInt; i++)
                {
                    RunTest(Tests[i-1]);
                }
                FireEvent(TestsFinished, new TestEventArgs<TTestMethod> { TestRunner = this, Assembly = Assembly });
            }
        }

        public void RunTestSet(string[] testNumbers)
        {
            FireEvent(TestsStarting, new TestEventArgs<TTestMethod> { TestRunner = this, Assembly = Assembly });
            foreach (string testNumber in testNumbers)
            {
                RunTest(testNumber);
            }
            FireEvent(TestsFinished, new TestEventArgs<TTestMethod> { TestRunner = this, Assembly = Assembly });
        }

        public void RunTest(string testNumber)
        {
            if (IsInRange(testNumber, out int selectedNumber))
            {
                RunTest(Tests[selectedNumber - 1]);
            }
            else
            {
                FireEvent(InvalidTestNumberSpecified, new TestEventArgs<TTestMethod> { TestRunner = this });
            }
        }

        [DebuggerStepThrough]
        public virtual void RunTest(TestMethod test)
        {
            if (test.Attribute is UnitTestAttribute testAttribute)
            {
                if (testAttribute.Ignore)
                {
                    FireEvent(TestIgnored, new TestIgnoredEventArgs(testAttribute));
                    return;
                }
            }
            TestEventArgs<TTestMethod> args = FireTestStarting(test);
            try
            {
                InvokeTest(test, IsolateMethodCalls);
                TestSummary.PassedTests.Add(test);
                FireEvent(TestPassed, args);
            }
            catch (ReflectionTypeLoadException rtle)
            {
                FireTestFailed(test, rtle);
            }
            catch (Exception ex)
            {
                FireTestFailed(test, ex.GetBaseException());
            }
            FireEvent(TestFinished, args);
        }
        
        protected void FireTestFailed(TestMethod test, ReflectionTypeLoadException rtle)
        {
            Exception ex = new ReflectionTypeLoadAggregateException(rtle);
            FireTestFailed(test, ex);
        }

        protected void FireTestFailed(TestMethod test, Exception ex)
        {
            TestSummary.FailedTests.Add(new FailedTest { Test = test, Exception = ex });
            FireEvent(TestFailed, new TestExceptionEventArgs(test, ex));
        }

        protected TestEventArgs<TTestMethod> FireTestStarting(TestMethod test)
        {
            TestEventArgs<TTestMethod> args = new TestEventArgs<TTestMethod> { Test = test, TestRunner = this, Tag = Tag, Assembly = test.Method.DeclaringType.Assembly };
            FireEvent(TestStarting, args);
            return args;
        }

        protected Assembly Assembly { get; set; }

        protected List<TTestMethod> Tests => _tests.Value;

        protected bool IsInRange(string testNumber, out int selectedNumber)
        {
            return int.TryParse(testNumber, out selectedNumber) && (selectedNumber - 1) > -1 && (selectedNumber - 1) < Tests.Count;
        }

        [DebuggerStepThrough]
        protected internal static void InvokeTest(ConsoleMethod consoleMethod, bool isolateMethodCalls = true)
        {
            MethodInfo invokeTarget = typeof(ConsoleMethod).GetMethod("Invoke");
            if (consoleMethod.Method.IsStatic)
            {
                if (isolateMethodCalls)
                {
                    CommandLineInterface.InvokeInSeparateAppDomain(invokeTarget, consoleMethod);
                }
                else
                {
                    CommandLineInterface.InvokeInCurrentAppDomain(invokeTarget, consoleMethod);
                }
            }
            else
            {
                string typeName = consoleMethod.Method.DeclaringType.Name;
                ConstructorInfo ctor = consoleMethod.Method.DeclaringType.GetConstructor(Type.EmptyTypes);
                if (ctor == null)
                    ExceptionExtensions.ThrowInvalidOperation("The declaring type {0} of method {1} does not have a parameterless constructor, test cannot be run.", typeName, consoleMethod.Method.Name);

                object instance = ctor.Invoke(null);
                instance.IsNotNull($"Unable to instantiate declaring type {typeName} of method {consoleMethod.Method.Name}");

                consoleMethod.Provider = instance;
                if (isolateMethodCalls)
                {
                    CommandLineInterface.InvokeInSeparateAppDomain(invokeTarget, consoleMethod);
                }
                else
                {
                    CommandLineInterface.InvokeInCurrentAppDomain(invokeTarget, consoleMethod);
                }
            }
        }

        readonly object _attachBeforeHandlersLock = new object();
        bool _beforeHandlersAreAttached;
        protected void AttachBeforeHandlers()
        {
            lock (_attachBeforeHandlersLock)
            {
                if (!_beforeHandlersAreAttached)
                {
                    TestsStarting += (o, e) =>
                    {
                        GetBeforeAllMethods().Each(cim =>
                        {
                            cim.TryInvoke(BeforeAllExceptionHandler);
                        });
                    };
                    TestStarting += (o, e) =>
                    {
                        GetBeforeEachMethods().Each(cim =>
                        {
                            cim.TryInvoke(BeforeEachExceptionHandler);
                        });
                    };
                    _beforeHandlersAreAttached = true;
                }
            }
        }

        protected List<ConsoleMethod> GetBeforeAllMethods()
        {
            if(SetupMethodProvider != null)
            {
                return SetupMethodProvider.GetBeforeAllMethods(Assembly) ?? new List<ConsoleMethod>();
            }
            return new List<ConsoleMethod>();
        }

        protected List<ConsoleMethod> GetBeforeEachMethods()
        {
            if(SetupMethodProvider != null)
            {
                return SetupMethodProvider.GetBeforeEachMethods(Assembly) ?? new List<ConsoleMethod>();
            }
            return new List<ConsoleMethod>();
        }

        readonly object _attachAfterHandlersLock = new object();
        bool _afterHandlersAreAttached;
        protected void AttachAfterHandlers()
        {
            lock (_attachAfterHandlersLock)
            {
                if (!_afterHandlersAreAttached)
                {
                    TestsFinished += (o, e) =>
                    {
                        GetAfterAllMethods().Each(cim =>
                        {
                            cim.TryInvoke(AfterAllExceptionHandler);
                        });
                    };
                    TestFinished += (o, e) =>
                    {
                        GetAfterEachMethods().Each(cim =>
                        {
                            cim.TryInvoke(AfterEachExceptionHandler);
                        });
                    };
                    _afterHandlersAreAttached = true;
                }
            }
        }

        protected List<ConsoleMethod> GetAfterAllMethods()
        {
            if(TeardownMethodProvider != null)
            {
                return TeardownMethodProvider.GetAfterAllMethods(Assembly) ?? new List<ConsoleMethod>();
            }
            return new List<ConsoleMethod>();
        }

        protected List<ConsoleMethod> GetAfterEachMethods()
        {
            if(TeardownMethodProvider != null)
            {
                return TeardownMethodProvider.GetAfterEachMethods(Assembly) ?? new List<ConsoleMethod>();
            }
            return new List<ConsoleMethod>();
        }

        public List<TTestMethod> GetTests()
        {
            return Tests;
        }
    }
}
