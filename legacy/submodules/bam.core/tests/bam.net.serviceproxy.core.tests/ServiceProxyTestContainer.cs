/*
	Copyright © Bryan Apellanes 2015  
*/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using Bam.Net.CommandLine;
using Bam.Net.Configuration;
using Bam.Net.CoreServices;
using Bam.Net.Data;
using Bam.Net.Data.SQLite;
using Bam.Net.Encryption;
using Bam.Net.Incubation;
using Bam.Net.Server;
using Bam.Net.Server.ServiceProxy;
using Bam.Net.ServiceProxy.Encryption;
using Bam.Net.Services;
using Bam.Net.Testing;
using Bam.Net.Testing.Unit;
using Bam.Net.UserAccounts;
using Bam.Net.UserAccounts.Data;
using Bam.Net.Web;
using NSubstitute;
using Org.BouncyCastle.Crypto;
using Type = System.Type;

namespace Bam.Net.ServiceProxy.Tests
{
    [Serializable]
    public partial class ServiceProxyTestContainer : CommandLineTool
    {
        public static void PreInit()
        {
            #region expand for PreInit help
            // To accept custom command line arguments you may use            
            /*
             * AddValidArgument(string argumentName, bool allowNull)
            */

            // All arguments are assumed to be name value pairs in the format
            // /name:value unless allowNull is true then only the name is necessary.

            // to access arguments and values you may use the protected member
            // arguments. Example:

            /*
             * arguments.Contains(argName); // returns true if the specified argument name was passed in on the command line
             * arguments[argName]; // returns the specified value associated with the named argument
             */

            // the arguments protected member is not available in PreInit() (this method)
            #endregion
			AddValidArgument("t", true, description: "run all tests");
			DefaultMethod = typeof(ServiceProxyTestContainer).GetMethod("Start");
		}

		public static void Start()
		{
			if (Arguments.Contains("t"))
			{
				RunAllUnitTests(typeof(ServiceProxyTestContainer).Assembly);
			}
			else
			{
				Interactive();
			}
		}

        class TestApiKeyClient: EncryptedServiceProxyClient<string>
        {
            public TestApiKeyClient() : base("") { }

            public void TestStartSession()
            {
                _ = base.StartClientSessionAsync(new Instant());
            }
        }

        [AfterEachUnitTest]
        public void StopServersAfterUnitTests()
        {
            ServiceProxyTestHelpers.StopServers();
        }

        [UnitTest]
        public void ApiKeyProviderShouldNotBeNull()
        {
            TestApiKeyClient client = new TestApiKeyClient();
            Expect.IsNotNull(((ApiHmacKeyResolver)client.ApiHmacKeyResolver).ApiHmacKeyProvider, "ApiKeyProvider was null");
        }

        [UnitTest]
        public void SecureServiceProxyClientBaseAddressShouldBeSet()
        {
            string baseAddress = "http://localhost:8989/";
            EncryptedServiceProxyClient<Echo> sspc = new EncryptedServiceProxyClient<Echo>(baseAddress);
            Expect.AreEqual(baseAddress, sspc.BaseAddress);
        }

        [UnitTest]
        public void PostShouldFireEvents()
        {
            BamAppServer server;
            EncryptedServiceProxyClient<Echo> sspc;
            ServiceProxyTestHelpers.StartTestServerGetEchoClient(out server, out sspc);

            bool? firedIngEvent = false;
            bool? firedEdEvent = false;
            
            sspc.PostStarted += (s, a) =>
            {
                firedIngEvent = true;
            };
            sspc.PostComplete += (s, assembly) =>
            {
                firedEdEvent = true;
            };
            try
            {
                sspc.ReceivePostResponseAsync("TestStringParameter", "monkey").Wait(); 
            }
            catch (Exception ex)
            {
                server.Stop();
                Expect.Fail(ex.Message);
            }

            server.Stop();

            Expect.IsTrue(firedIngEvent.Value, "Posting event didn't fire");
            Expect.IsTrue(firedEdEvent.Value, "Posted event didn't fire");
        }

        [UnitTest]
        public void PostShouldBeCancelable()
        {
            BamAppServer server;
            EncryptedServiceProxyClient<Echo> sspc;
            ServiceProxyTestHelpers.StartTestServerGetEchoClient(out server, out sspc);

            bool? firedIngEvent = false;
            bool? firedEdEvent = false;
            sspc.PostStarted += (s, a) =>
            {
                firedIngEvent = true;
                a.CancelInvoke = true; // should cancel the call
            };
            sspc.PostComplete += (s,a) =>
            {
                firedEdEvent = true;
            };

            try
            {
                sspc.ReceivePostResponseAsync("TestStringParameter", "monkey").Wait();
            }
            catch (Exception ex)
            {
                server.Stop();
                Expect.Fail(ex.Message);
            }

            server.Stop();
            
            Expect.IsTrue(firedIngEvent.Value, "Posting didn't fire");
            Expect.IsFalse(firedEdEvent.Value, "Posted fired but should not have");
        }


        [UnitTest]
        public void GetShouldFireEvents()
        {
            BamAppServer server;
            EncryptedServiceProxyClient<Echo> sspc;
            ServiceProxyTestHelpers.StartTestServerGetEchoClient(out server, out sspc);

            bool? firedIngEvent = false;
            bool? firedEdEvent = false;

            sspc.GetStarted += (s, a) =>
            {
                firedIngEvent = true;
            };
            sspc.GetComplete += (s, a) =>
            {
                firedEdEvent = true;
            };
            try
            {
                sspc.ReceiveGetResponseAsync("TestStringParameter", "monkey").Wait();
            }
            catch (Exception ex)
            {
                server.Stop();
                Expect.Fail(ex.Message);
            }
            
            server.Stop();

            Expect.IsTrue(firedIngEvent.Value, "Getting event didn't fire");
            Expect.IsTrue(firedEdEvent.Value, "Got event didn't fire");
        }

        [UnitTest]
        public void SecureServiceProxyInvokeShouldFireSessionStarting()
        {
            BamAppServer server;
            EncryptedServiceProxyClient<Echo> sspc;
            ServiceProxyTestHelpers.StartSecureChannelTestServerGetEchoClient(out server, out sspc);
            bool? sessionStartingCalled = false;
            sspc.SessionStarting += (c) =>
            {
                sessionStartingCalled = true;
            };

            sspc.InvokeServiceMethod<string>("Send", new object[] { "banana" });
            server.Stop();
            Expect.IsTrue(sessionStartingCalled.Value, "SessionStarting did not fire");
        }

        [UnitTest]
        public void SecureServiceProxyInvokeShouldEstablishSessionIfSecureChannelServerRegistered()
        {
            BamAppServer server;
            EncryptedServiceProxyClient<Echo> testSecureServiceProxyClient;
            ServiceProxyTestHelpers.StartSecureChannelTestServerGetEchoClient(out server, out testSecureServiceProxyClient);

            Expect.IsFalse(testSecureServiceProxyClient.IsSessionStarted);
            string value = "InputValue_".RandomLetters(8);
            string result = testSecureServiceProxyClient.InvokeServiceMethod<string>("Send", new object[] { value });
            server.Stop();

            string msg = testSecureServiceProxyClient.StartSessionException != null ? testSecureServiceProxyClient.StartSessionException.Message : string.Empty;
            Expect.IsNull(testSecureServiceProxyClient.StartSessionException, "SessionStartException: {0}"._Format(msg));
            Expect.IsTrue(testSecureServiceProxyClient.IsSessionStarted, "Session was not established");

            server.Stop();
        }

        [UnitTest]
        public void SecureServiceProxyInvokeShouldSucceed()
        {
            BamAppServer server;
            EncryptedServiceProxyClient<Echo> encryptedServiceProxyClient;
            ServiceProxyTestHelpers.StartSecureChannelTestServerGetEchoClient(out server, out encryptedServiceProxyClient);

            string value = "InputValue_".RandomLetters(8);
            string result = encryptedServiceProxyClient.InvokeServiceMethod<string>("Send", new object[] { value });
            server.Stop();
            Expect.AreEqual(value, result);
        }

        [UnitTest]
        public void InstantDiffTest()
        {
            Instant now = new Instant();
            Instant then = new Instant(DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(2)));
            Instant fromNow = new Instant(DateTime.UtcNow.AddMinutes(4));

            int thenDiff = now.DiffInMilliseconds(then);
            int fromNowDiff = now.DiffInMilliseconds(fromNow);

            Expect.IsGreaterThan(thenDiff, 0);
            Expect.IsGreaterThan(fromNowDiff, 0);

            Message.PrintLine("Then Diff: {0}", thenDiff);
            Message.PrintLine("From Now Diff: {0}", fromNowDiff);

            TimeSpan thenSpan = TimeSpan.FromMilliseconds(thenDiff);
            TimeSpan fromNowSpan = TimeSpan.FromMilliseconds(fromNowDiff);

            Message.PrintLine("Then minutes diff: {0}", thenSpan.Minutes);
            Message.PrintLine("From now minutes diff: {0}", fromNowSpan.Minutes);
        }

        [UnitTest]
        public void InstantToStringParse()
        {
            Instant normalNow = new Instant(DateTime.Now);
            string toStringed = normalNow.ToString();
            Message.PrintLine("ToStringed: {0}", toStringed);
            Instant parsed = Instant.FromString(toStringed);

            Expect.AreEqual(0, parsed.DiffInMilliseconds(normalNow));
        }

        [UnitTest]
        public async void StartSession()
        {
            BamAppServer server;
            EncryptedServiceProxyClient<Echo> sspc;
            ServiceProxyTestHelpers.StartSecureChannelTestServerGetEchoClient(out server, out sspc);

            await sspc.StartClientSessionAsync(new Instant());
            server.Stop();
        }
        
        [UnitTest]
        public void GetShouldBeCancelable()
        {
            BamAppServer server;
            EncryptedServiceProxyClient<Echo> sspc;
            ServiceProxyTestHelpers.StartTestServerGetEchoClient(out server, out sspc);

            bool? firedIngEvent = false;
            bool? firedEdEvent = false;
            sspc.GetStarted += (s, a) =>
            {
                firedIngEvent = true;
                a.CancelInvoke = true; // should cancel the call
            };
            sspc.GetComplete += (s, a) =>
            {
                firedEdEvent = true;
            };

            try
            {
                sspc.ReceiveGetResponseAsync("TestStringParameter", "monkey").Wait();
            }
            catch (Exception ex)
            {
                server.Stop();
                Expect.Fail(ex.Message);
            }

            server.Stop();

            Expect.IsTrue(firedIngEvent.Value, "Getting didn't fire");
            Expect.IsFalse(firedEdEvent.Value, "Got fired but should not have");
        }


        class TestExecutionRequest: ServiceProxyInvocation
        {
            public TestExecutionRequest(string c, string m, string f)
                : base(c, m)
            {
                WebServiceRegistry inc = new WebServiceRegistry();
                inc.Set(new Echo());
                WebServiceRegistry = inc;
                InvocationTarget = new Echo();
                MethodInfo = typeof(Echo).GetMethod(m);
                Request = Substitute.For<IRequest>();
                Request.Url.Returns(new Uri($"http://localhost/{c}/{m}.{f}"));
                Request.QueryString.Returns(new NameValueCollection());
            }

            public bool Called { get; set; }
            public override ServiceProxyInvocationValidationResult Validate()
            {
                Called = true;
                return new ServiceProxyInvocationValidationResult();
            }
        }

        [UnitTest]
        public void ExecuteShouldCallValidate()
        {
            TestExecutionRequest to = new TestExecutionRequest("Echo", "Send", "json");
            Expect.IsFalse(to.Called);
            to.Execute();
            Expect.IsTrue(to.Called);
        }

        [UnitTest]
        public void MissingClassShouldReturnClassNotSpecifiedResult()
        {
            ServiceProxyInvocation er = new ServiceProxyInvocation(null, "TestMethod");
            ServiceProxyInvocationValidationResult result = er.Validate();
            Expect.IsFalse(result.Success);
            Expect.IsTrue(result.ValidationFailures.ToList().Contains(ValidationFailures.ClassNameNotSpecified));
            OutFormat(result.Message);
        }

        class TestClass
        {
            public void TestMethod(object param)
            {

            }

            public string ShouldWork()
            {
                return "Yay";
            }

            [Local]
            public virtual void LocalMethod()
            {
            }
        }

        [UnitTest]
        public void MissingMethodShouldReturnMethodNotSpecified()
        {
            ServiceProxySystem.Register<TestClass>();
            ServiceProxyInvocation er = new ServiceProxyInvocation("TestClass", "");
            ServiceProxyInvocationValidationResult result = er.Validate();
            Expect.IsFalse(result.Success);
            Expect.IsTrue(result.ValidationFailures.ToList().Contains(ValidationFailures.MethodNameNotSpecified));
            Message.PrintLine(result.Message);
        }

        [UnitTest]
        public void LocalMethodShouldNotValidateIfNotLoopback()
        {
            ServiceProxySystem.Register<TestClass>();
            IRequest request = Substitute.For<IRequest>();
            request.QueryString.Returns(new NameValueCollection());
            request.UserHostAddress.Returns("192.168.0.80:80");
            IResponse response = Substitute.For<IResponse>();
            IHttpContext context = Substitute.For<IHttpContext>();
            context.Request.Returns(request);
            context.Response.Returns(response);
            ServiceProxyInvocation er = new ServiceProxyInvocation("TestClass", "LocalMethod") { Context = context };
            ServiceProxyInvocationValidationResult result = er.Validate();
            Expect.IsFalse(result.Success);
            Expect.IsTrue(result.ValidationFailures.ToList().Contains(ValidationFailures.RemoteExecutionNotAllowed));
        }

        [UnitTest]
        public void LocalMethodShouldValidateIfLocalClient()
        {
            ServiceProxySystem.Register<TestClass>();
            IRequest request = Substitute.For<IRequest>();
            request.QueryString.Returns(new NameValueCollection());
            request.UserHostAddress.Returns("127.0.0.1:80");
            IResponse response = Substitute.For<IResponse>();
            IHttpContext context = Substitute.For<IHttpContext>();
            context.Request.Returns(request);
            context.Response.Returns(response);
            ServiceProxyInvocation er = new ServiceProxyInvocation("TestClass", "LocalMethod") { Context = context };
            ServiceProxyInvocationValidationResult result = er.Validate();
            Expect.IsTrue(result.Success);
        }

        [UnitTest]
        public void UnregisteredClassShoudReturnClassNotRegistered()
        {
			ServiceProxySystem.Unregister<TestClass>();
            ServiceProxyInvocation er = new ServiceProxyInvocation("TestClass", "ShouldWork");
            ServiceProxyInvocationValidationResult result = er.Validate();
            Expect.IsFalse(result.Success);
            Expect.IsTrue(result.ValidationFailures.ToList().Contains(ValidationFailures.ClassNotRegistered));
            Message.PrintLine(result.Message);
        }

        [UnitTest]
        public void MethodNotFoundShouldBeReturned()
        {
            ServiceProxySystem.Register<TestClass>();
            ServiceProxyInvocation er = new ServiceProxyInvocation("TestClass", "MissingMethod");
            ServiceProxyInvocationValidationResult result = er.Validate();
            Expect.IsFalse(result.Success);
            Expect.IsTrue(result.ValidationFailures.ToList().Contains(ValidationFailures.MethodNotFound));
            Message.PrintLine(result.Message);
        }

        [UnitTest]
        public void ParameterCountMismatchShouldBeReturned()
        {
            ServiceProxySystem.Register<TestClass>();
            ServiceProxyInvocation er = new ServiceProxyInvocation("TestClass", "TestMethod");
            er.Arguments = new ServiceProxyInvocationArgument[] { null, null };
            ServiceProxyInvocationValidationResult result = er.Validate();
            Expect.IsFalse(result.Success);
            Expect.IsTrue(result.ValidationFailures.ToList().Contains(ValidationFailures.ParameterCountMismatch));
            Message.PrintLine(result.Message);
        }

        [UnitTest]
        public void ExecuteShouldSucceed()
        {
            ServiceProxySystem.Register<TestClass>();
            ServiceProxyInvocation invocation = new ServiceProxyInvocation("TestClass", "ShouldWork");
            ServiceProxyInvocationValidationResult result = invocation.Validate();            
            Expect.IsTrue(result.Success);
            invocation.Execute();
            Expect.IsTrue(invocation.Result.Equals("Yay"));
            Message.PrintLine(invocation.Result.ToString());
        }

        [UnitTest]
        public void ShouldHaveEncryptAndApiKeyRequired()
        {
            Type type = typeof(ApiKeyRequiredEcho);
            Expect.IsTrue(type.HasCustomAttributeOfType<EncryptAttribute>());
            Expect.IsTrue(type.HasCustomAttributeOfType<ApiHmacKeyRequiredAttribute>());
        }

        public class InvalidKeyProvider: ApiHmacKeyProvider
        {
            public override string GetApplicationClientId(IApplicationNameProvider nameProvider)
            {
                return "InvalidClientId_".RandomLetters(8);
            }

            public override string GetApplicationApiHmacKey(string applicationClientId, int index)
            {
                return "InvalidApiKey_".RandomLetters(4);
            }
        }

        [UnitTest]
        public void SecureServiceProxyInvokeWithInvalidTokenShouldFail()
        {
			CleanUp();
            BamAppServer server;
            ServiceProxyTestHelpers.StartSecureChannelTestServerGetApiKeyRequiredEchoClient(out server, out EncryptedServiceProxyClient<ApiKeyRequiredEcho> sspc);
            
            string value = "InputValue_".RandomLetters(8);
            bool? thrown = false;
            sspc.InvocationException += (client, ex) =>
            {
                thrown = true;
            };

            ApiHmacKeyResolver resolver = new ApiHmacKeyResolver(new InvalidKeyProvider());
            sspc.ApiHmacKeyResolver = resolver;
            string result = sspc.InvokeServiceMethod<string>("Send", new object[] { value });            

            thrown.Value.IsTrue();
			CleanUp();
        }

        [UnitTest]
        public void StaticApiKeyProviderShouldAlwaysReturnSameKey()
        {
            string id = "TheClientIdGoesHere";
            string key = "The Api Key Goes Here";
            StaticApiKeyProvider apiKeyProvider = new StaticApiKeyProvider(id, key);

            ApiHmacKeyInfo keyInfo = apiKeyProvider.GetApiHmacKeyInfo(new DefaultConfigurationApplicationNameProvider());
            
            Expect.AreEqual(id, keyInfo.ApplicationClientId);
            Expect.AreEqual(key, keyInfo.ApiHmacKey);
        }
		
        [UnitTest]
        public void SecureServiceProxyInvokeWithApiKeyShouldSucceed()
        {
			CleanUp();
            string methodName = MethodBase.GetCurrentMethod().Name;

            ServiceProxyTestHelpers.CreateServer(out string baseAddress, out BamAppServer server);
            ServiceProxyTestHelpers.Servers.Add(server); // makes sure it gets stopped after test run
            EncryptedServiceProxyClient<ApiKeyRequiredEcho> sspc = new EncryptedServiceProxyClient<ApiKeyRequiredEcho>(baseAddress);

            IApplicationNameProvider nameProvider = null; // TODO: fix this using a substitute from NSubstitute //new TestApplicationNameProvider(methodName);
            IApiHmacKeyProvider keyProvider = null; // TODO: fix this using a substitute from NSubstitute // new LocalApiKeyProvider();
            ApiHmacKeyResolver keyResolver = new ApiHmacKeyResolver(keyProvider, nameProvider);

            SecureChannel channel = new SecureChannel() {ApiHmacKeyResolver = keyResolver};

            server.AddCommonService<SecureChannel>(channel);
            server.AddCommonService<ApiKeyRequiredEcho>();

            server.Start();

            string value = "InputValue_".RandomLetters(8);
            bool? thrown = false;
            sspc.InvocationException += (client, ex) =>
            {
                thrown = true;
            };

            sspc.ApiHmacKeyResolver = keyResolver;
            string result = sspc.InvokeServiceMethod<string>("Send", new object[] { value });
            
            thrown.Value.IsFalse("Exception was thrown");
            Expect.AreEqual(value, result);
			CleanUp();
        }

/*
        [UnitTest]
        public void ApiKey_ExecutionRequestShouldValidateApiKey()
        {
			RegisterDb();
            ServiceProxySystem.Register<ApiKeyRequiredEcho>();
            string testName = MethodBase.GetCurrentMethod().Name;
            IUserResolver mockUserResolver = Substitute.For<IUserResolver>();
            mockUserResolver.GetUser(Arg.Any<IHttpContext>()).Returns("testUser");
            LocalApiKeyManager.Default.UserResolver = mockUserResolver;
            
            IApplicationNameProvider nameProvider = new TestApplicationNameProvider(testName.RandomLetters(6));
            IApiKeyProvider keyProvider = new LocalApiKeyProvider();

            ServiceProxyInvocation er = new ServiceProxyInvocation("ApiKeyRequiredEcho", "Send")
            {
                ApiKeyResolver = new ApiKeyResolver(keyProvider, nameProvider),
                Request = new ServiceProxyTestHelpers.JsonTestRequest()
            };

            string data = ApiArgumentEncoder.ArgumentsToJsonArgsMember("some random data"); // TODO: fix this //ApiArgumentEncoder.ParametersToJsonParamsObjectString("some random data");
           // er.InputString = data;

            ServiceProxyInvocationValidationResult result = er.Validate();
            result.Success.IsFalse();
            List<ValidationFailures> failures = new List<ValidationFailures>(result.ValidationFailures);
            failures.Contains(ValidationFailures.InvalidApiKeyToken).IsTrue();
        }*/

/*        [UnitTest]
        public void ApiKey_ExecutionRequestValidationShouldSucceedIfGoodToken()
        {
			Prepare();
            ServiceProxySystem.Register<ApiKeyRequiredEcho>();

            string methodName = MethodBase.GetCurrentMethod().Name;
            IApplicationNameProvider nameProvider = new TestApplicationNameProvider(methodName);
            IApiKeyProvider keyProvider = new LocalApiKeyProvider();

            string className = "ApiKeyRequiredEcho";
            string method= "Send";
            string data = ApiArgumentEncoder.ArgumentsToJsonArgumentsArray("some random data").ToJson();
            ServiceProxyInvocation er = new ServiceProxyInvocation(className, method)
            {
                //ArgumentsAsJsonArrayOfJsonStrings = data,
                ApiKeyResolver = new ApiKeyResolver(keyProvider, nameProvider),
                Request = new ServiceProxyTestHelpers.FormUrlEncodedTestRequest()
            };

            er.ApiKeyResolver.SetKeyToken(er.Request.Headers, ApiArgumentEncoder.GetStringToHash(className, method, data));

            ServiceProxyInvocationValidationResult result = er.Validate();
            Expect.IsTrue(result.Success);
        }*/

/*        [UnitTest]
        public void ApiKey_ExecutionRequestValidationShouldFailIfBadToken()
        {
			RegisterDb();
            ServiceProxySystem.Register<ApiKeyRequiredEcho>();

            IUserResolver mockUserResolver = Substitute.For<IUserResolver>();
            mockUserResolver.GetUser(Arg.Any<IHttpContext>()).Returns("testUser");
            LocalApiKeyManager.Default.UserResolver = mockUserResolver;
            
            string methodName = MethodBase.GetCurrentMethod().Name;
            IApplicationNameProvider nameProvider = new TestApplicationNameProvider(methodName.RandomLetters(4));
            IApiKeyProvider keyProvider = new LocalApiKeyProvider();

            ServiceProxyInvocation spi = new ServiceProxyInvocation("ApiKeyRequiredEcho", "Send")
            {
                ApiKeyResolver = new ApiKeyResolver(keyProvider, nameProvider),
                Request = new ServiceProxyTestHelpers.JsonTestRequest()
            };
            string data = ApiArgumentEncoder.ArgumentsToJsonArgsMember("some random data");
            //er.InputString = data;
            ApiKeyResolver resolver = new ApiKeyResolver(keyProvider, nameProvider);
            resolver.SetKeyToken(spi.Request.Headers, data);

            spi.Request.Headers[Headers.KeyToken] = "bad token value";

            ServiceProxyInvocationValidationResult result = spi.Validate();
            
            Expect.IsFalse(result.Success, "Validation should have failed");
            List<ValidationFailures> failures = new List<ValidationFailures>(result.ValidationFailures);            
            Expect.IsTrue(failures.Contains(ValidationFailures.InvalidApiKeyToken), "ValidationFailure should have been InvalidApiKeyToken");
        }*/
/*
        [UnitTest]
        public void ShouldBeAbleToCreateApplication()
        {
			RegisterDb();
            Expect.IsTrue(_registeredDb.Value);
            ApplicationCreateResult result = CreateTestApp();
            Expect.AreEqual(ApplicationCreateStatus.Success, result.Status);
            Expect.IsNotNull(result.Application);
            Expect.IsNullOrEmpty(result.Message);
        }

        [UnitTest]
        public void ShouldReturnNameInUseIfAppNameInUse()
        {
			RegisterDb();
            ApplicationCreateResult first = CreateTestApp();
            ApplicationCreateResult second = Secure.Application.Create(first.Application.Name);
            Expect.AreEqual(ApplicationCreateStatus.NameInUse, second.Status);
        }*/
/*
        private static ApplicationCreateResult CreateTestApp()
        {
            LocalApiKeyManager.Default.UserResolver = new TestUserResolver();
            ApplicationCreateResult result = Secure.Application.Create("Test_AppName_".RandomLetters(6));
            return result;
        }*/

/*        [UnitTest]
        public void ApplicationShouldHaveKey()
        {
			RegisterDb();
            Expect.IsTrue(_registeredDb.Value);
            ApplicationCreateResult result = CreateTestApp();
            Expect.IsNotNull(result.Application);
            Expect.IsTrue(result.Application.ApiKeysByApplicationId.Count > 0);
        }*/

        [UnitTest]
        public void TimeGenerateVsNewGuid()
        {
            string generatedId;
            TimeSpan generateTime = Exec.Time(() =>
            {
                generatedId = ServiceProxySystem.GenerateSecureRandomString();
            });

            string guid;
            TimeSpan guidTime = Exec.Time(() =>
            {
                guid = Guid.NewGuid().ToString();
            });

            string sha256;
            TimeSpan guidTimeSha256 = Exec.Time(() =>
            {
                sha256 = Guid.NewGuid().ToString().Sha256();
            });

            Message.PrintLine("Generate took: {0}", generateTime.ToString());
            Message.PrintLine("-NewGuid took: {0}", guidTime.ToString());
            Message.PrintLine("HashGuid took: {0}", guidTimeSha256.ToString());
        }

        [ConsoleAction("Output Keys")]
        public void GetPublicKeyXml()
        {
            OutLine("Public Key", ConsoleColor.Cyan);
            OutLine(RsaKeyFile.Default.PublicKeyXml, ConsoleColor.Green);
            OutLine("Private Key", ConsoleColor.Cyan);
            OutLine(RsaKeyFile.Default.PrivateKeyXml, ConsoleColor.Yellow);
        }

        [ConsoleAction]
        public void LookAtGeneratedRsaPemKeys()
        {
            AsymmetricCipherKeyPair keys = Rsa.GenerateKeyPair(RsaKeyLength._1024);
            OutLine(keys.Public.ToPem());
        }

        [ConsoleAction]
        public void OutputDaoUsersContextAssemblyQualifiedName()
        {
            string name = typeof(UserAccountsContext).AssemblyQualifiedName;
            Out(name);
            name.SafeWriteToFile("./UserAccountsContext", true);
            "notepad ./UserAccountsContext".Run();
        }

        static bool? _registeredDb;
        public static void RegisterDb()
        {
            SQLiteDatabase db = new SQLiteDatabase();
            //Db.For<Secure.Application>(db);
            Db.For<Account>(UserAccountsDatabase.Default);
            //Db.TryEnsureSchema<Secure.Application>(db);
            //SQLiteRegistrar.Register<Secure.Application>();
            _registeredDb = true;
        }
    }

}
