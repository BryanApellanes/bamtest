// <copyright file="ApiKeyErrorResponseShould.cs" company="Okta, Inc">
// Copyright (c) 2020 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Msg = Okta.Wizard.Messages;

namespace Okta.Wizard.Automation.Tests.Unit
{
    [TestClass]
    public class ApiKeyErrorResponseShould
    {
        [TestMethod]
        public void Load()
        {
            ApiKeyErrorResponse apiKeyErrorResponse = new ApiKeyErrorResponse
            {
                OktaSignInFailedEventArgs = new Okta.OktaSignInFailedEventArgs
                {
                    UserSignInCredentials = new UserSignInCredentials
                    {
                        SignInUrl = "test-sign-in-url",
                        UserName = "test-useranme",
                        Password = "test-password"
                    },
                    Message = "test message"
                },
                PageActionResults = new List<PageActionResult>
                {
                    new PageActionResult{ Message = "page action result 1"},
                    new PageActionResult{ Message = "page action result 2"},
                },
                Message = "test api key error message",
                StackTrace = "test api key error stack trace"
            };

            apiKeyErrorResponse.FailureOccurred.Should().BeTrue();

            apiKeyErrorResponse.Save();

            Msg.ApiKeyErrorResponse apiKeyErrorResponseMsg = Msg.ApiKeyErrorResponse.Load();
            apiKeyErrorResponseMsg.FailureOccurred.Should().BeTrue();
            apiKeyErrorResponseMsg.PageActionResults.Count.Should().Be(2);
            apiKeyErrorResponseMsg.Message.Should().Be(apiKeyErrorResponse.Message);
            apiKeyErrorResponseMsg.StackTrace.Should().Be(apiKeyErrorResponse.StackTrace);
            apiKeyErrorResponseMsg.OktaSignInFailedEventArgs.Count.Should().Be(2);
            apiKeyErrorResponseMsg.OktaSignInFailedEventArgs.ContainsKey("UserSignInCredentials").Should().BeTrue();
            apiKeyErrorResponseMsg.OktaSignInFailedEventArgs.ContainsKey("Message").Should().BeTrue();
            apiKeyErrorResponseMsg.OktaSignInFailedEventArgs["UserSignInCredentials"].GetType().Should().Be(typeof(JObject));
        }
    }
}
