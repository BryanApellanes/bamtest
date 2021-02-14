// <copyright file="PageActionSequenceShould.cs" company="Okta, Inc">
// Copyright (c) 2020 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Okta.Wizard.Automation.Okta;
using System;
using System.Collections.Generic;

namespace Okta.Wizard.Automation.Tests.Integration
{
    [TestClass]
    public class PageActionSequenceShould
    {
        public const string ERROR_SELECTOR = "#form1 > div.o-form-content.o-form-theme.clearfix > div.o-form-error-container.o-form-has-errors > div";
        public const string TEST_SIGNIN_URL = "https://dev-6437470.okta.com/";

        [TestMethod]
        public void RaiseSuccessEvent()
        {
            try
            {
                PageActionSequence pageActionSequence = new PageActionSequence("Enter Credentials")
                    .AddStep("enter username", async (page) =>
                    {
                        await page.KeysAsync(Selectors.SignInUserName, $"{nameof(RaiseSuccessEvent)}_Test");
                    })
                    .AddStep("enter password", async (page) =>
                    {
                        await page.KeysAsync(Selectors.SignInPassword, "testpassword");
                    });

                bool? successRaised = false;
                pageActionSequence.Success += (sender, args) =>
                {
                    PageActionSequenceEventArgs pasea = (PageActionSequenceEventArgs)args;
                    pasea.PageActionSequence.Should().Be(pageActionSequence);
                    successRaised = true;
                };

                PageActionSequenceExecutionResult result = pageActionSequence.ExecuteAsync(TEST_SIGNIN_URL).Result;
                List<PageActionResult> results = result.Results;
                successRaised.Should().BeTrue();
            }
            finally
            {
                AutomationPage.TryCloseBrowser();
            }
        }

        [TestMethod]
        public void RaiseFailureEvent()
        {
            try
            {
                PageActionSequence pageActionSequence = new PageActionSequence($"{nameof(RaiseFailureEvent)}_Test")
                    .AddStep("enter username", async (page) =>
                    {
                        await page.KeysAsync(Selectors.SignInUserName, $"{nameof(RaiseFailureEvent)}_Test");
                    })
                    .AddStep("enter password and throw", async (page) =>
                    {
                        await page.KeysAsync(Selectors.SignInPassword, "test password");
                        throw new Exception("Testing page action sequence failure");
                    })
                    .AddStep("shouldn't get here", async (page) =>
                    {
                        await page.ClickAsync(Selectors.SignInSubmit);
                    });

                bool? failureRaised = false;
                pageActionSequence.Failure += (sender, args) => failureRaised = true;
                PageActionSequenceExecutionResult result = pageActionSequence.ExecuteAsync(TEST_SIGNIN_URL).Result;
                List<PageActionResult> results = result.Results;

                pageActionSequence.Steps.Count.Should().Be(3);
                results.Count.Should().Be(2);
                failureRaised.Should().BeTrue();

            }
            finally
            {
                AutomationPage.TryCloseBrowser();
            }
        }

        [TestMethod]
        public void HaveExecutionResultAfterExecution()
        {
            try
            {
                PageActionSequence pageActionSequence = new PageActionSequence($"{nameof(HaveExecutionResultAfterExecution)}_Test")
                    .AddStep("enter username", async (page) =>
                    {
                        await page.KeysAsync(Selectors.SignInUserName, $"{nameof(RaiseFailureEvent)}_Test");
                    })
                    .AddStep("enter password and throw", async (page) =>
                    {
                        await page.KeysAsync(Selectors.SignInPassword, "test password");
                    });

                PageActionSequenceExecutionResult result = pageActionSequence.ExecuteAsync(TEST_SIGNIN_URL).Result;
                (pageActionSequence.ExecutionResult == result).Should().BeTrue();
                pageActionSequence.HasExecuted.Should().BeTrue();

            }
            finally
            {
                AutomationPage.TryCloseBrowser();
            }
        }

        [TestMethod]
        public void RecognizeSignInFailure()
        {
            try
            {
                PageActionSequence pageActionSequence = new PageActionSequence("Fail to login")
                    .AddStep("enter username", async (page) =>
                    {
                        await page.KeysAsync("#okta-signin-username", "bad username");
                    })
                    .AddStep("enter password", async (page) =>
                    {
                        await page.KeysAsync("#okta-signin-password", "bad password");
                    })
                    .AddNavigationStep("click sign in button", async (page) =>
                    {
                        await page.ClickAsync("#okta-signin-submit");
                    })
                    .AddStep("assert login failed", async (page) =>
                    {
                        await page.WaitForElementAsync(ERROR_SELECTOR);
                        await page.AssertElementIsPresentAsync(ERROR_SELECTOR);
                    });
                pageActionSequence.Debug = true;

                pageActionSequence.ScreenShotsDirectory = "/tmp/";
                PageActionSequenceExecutionResult result = pageActionSequence.ExecuteAsync(TEST_SIGNIN_URL).Result;
                List<PageActionResult> results = result.Results;
                results.Count.Should().Be(4);
                results[0].Succeeded.Should().BeTrue();
                results[1].Succeeded.Should().BeTrue();
                results[2].Succeeded.Should().BeTrue();
                results[3].Succeeded.Should().BeTrue();
            }
            finally
            {
                AutomationPage.TryCloseBrowser();
            }
        }

        [TestMethod]
        public void GoToApiTokensPage()
        {
            try
            {
                bool? signInSequenceSuccessEventFired = false;
                bool? goToApiTokensSuccessEventFired = false;
                string expectedPath = "/admin/access/api/tokens";
                OktaSignInPageActionSequence signInSequence = new OktaSignInPageActionSequence(UserSignInCredentials.FromEnvironmentVariables());

                PageActionSequence goToApiTokens = new PageActionSequence("Go to api tokens page")
                    .AddNavigationStep("open api tokens page", async (page) =>
                    {
                        await page.ClickAsync("#nav-admin-api");
                        await page.WaitForNavigationAsync();
                    });

                signInSequence.EnableDebug();
                goToApiTokens.EnableDebug();

                signInSequence.Success += (sender, args) =>
                {
                    PageActionSequenceEventArgs pageActionSequenceEventArgs = (PageActionSequenceEventArgs)args;
                    goToApiTokens.ExecuteAsync(pageActionSequenceEventArgs.Page).Wait();
                    signInSequenceSuccessEventFired = true;
                };

                goToApiTokens.Success += (sender, args) =>
                {
                    PageActionSequenceEventArgs pageActionSequenceEventArgs = (PageActionSequenceEventArgs)args;
                    pageActionSequenceEventArgs.Page.AssertIsAtPathAsync(expectedPath).Wait();
                    goToApiTokensSuccessEventFired = true;
                };

                PageActionSequenceExecutionResult signInResult = signInSequence.ExecuteAsync().Result;

                signInSequenceSuccessEventFired.Should().BeTrue();
                goToApiTokensSuccessEventFired.Should().BeTrue();


            }
            finally
            {
                AutomationPage.TryCloseBrowser();
            }
        }

        [TestMethod]
        public void CreateAndDeleteApiToken()
        {
            try
            {
                PageActionSequence.DefaultNavigationOptions = AutomationPage.OriginalTimeoutNavigationOptions;
                string testTokenName = $"{nameof(CreateAndDeleteApiToken)}_TestToken";

                string expectedPath = "/admin/access/api/tokens";

                CreateApiTokenPageActionSequence createApiTokenPageActionSequence = new CreateApiTokenPageActionSequence(testTokenName);
                createApiTokenPageActionSequence.EnableDebug();
                bool? createSuccessEventFired = false;
                createApiTokenPageActionSequence.Success += (sender, args) => createSuccessEventFired = true;

                PageActionSequenceExecutionResult createResult = createApiTokenPageActionSequence.ExecuteAsync().Result;

                createResult.HasFailures.Should().BeFalse();
                createApiTokenPageActionSequence.Succeeded.Should().BeTrue();
                createSuccessEventFired.Should().BeTrue();
                createApiTokenPageActionSequence.Page.IsAtPathAsync(expectedPath).Result.Should().BeTrue();

                createApiTokenPageActionSequence.TokenNames.Should().NotBeNull();
                createApiTokenPageActionSequence.TokenNames.Length.Should().BeGreaterThan(0);
                createApiTokenPageActionSequence.CreatedApiToken.Should().NotBeNullOrEmpty();

                DeleteApiTokenPageActionSequence deleteApiTokenPageActionSequence = new DeleteApiTokenPageActionSequence(testTokenName);
                deleteApiTokenPageActionSequence.EnableDebug();
                bool? deleteSuccessEventFired = false;
                deleteApiTokenPageActionSequence.Success += (sender, args) => deleteSuccessEventFired = true;

                PageActionSequenceExecutionResult deleteResult = deleteApiTokenPageActionSequence.ExecuteAsync(createApiTokenPageActionSequence.Page).Result;

                deleteResult.HasFailures.Should().BeFalse();
                deleteApiTokenPageActionSequence.Succeeded.Should().BeTrue();
                deleteSuccessEventFired.Should().BeTrue();
                deleteApiTokenPageActionSequence.Page.IsAtPathAsync(expectedPath).Result.Should().BeTrue();

            }
            finally
            {
                AutomationPage.TryCloseBrowser();
            }
        }

        [TestMethod]
        public void GetTaggedSteps()
        {
            PageActionSequence pageActionSequence = new PageActionSequence();
            pageActionSequence.AddStep("test validation step", (page) => { }, Tags.Validation);
            pageActionSequence.AddStep("test action step 1", (page) => { }, Tags.Action);
            pageActionSequence.AddStep("test action step 2", (page) => { }, Tags.Action);
            pageActionSequence.AddStep("submit", (page) => { }, Tags.Submit);

            List<PageAction> actionSteps = pageActionSequence.GetTaggedSteps(Tags.Action);
            actionSteps.Should().NotBeNull();
            actionSteps.Count.Should().Be(2);
            actionSteps[0].Name.Should().Be("test action step 1");
            actionSteps[1].Name.Should().Be("test action step 2");
        }
    }
}
