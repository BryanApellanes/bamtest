// <copyright file="OktaSignInPageActionSequenceShould.cs" company="Okta, Inc">
// Copyright (c) 2020 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Okta.Wizard.Automation.Okta;
using System;

namespace Okta.Wizard.Automation.Tests.Integration
{
    [TestClass]
    public class OktaSignInPageActionSequenceShould
    {
        [TestMethod]
        public void SignInSuccessfully()
        {
            try
            {
                OktaSignInPageActionSequence signInPageActionSequence = new OktaSignInPageActionSequence();
                bool? signInSucceeded = false;
                bool? signInExecuted = false;
                signInPageActionSequence.Success += (sender, args) => signInSucceeded = true;
                signInPageActionSequence.Executed += (sender, args) => signInExecuted = true;

                signInPageActionSequence.ExecuteAsync().Wait();

                signInExecuted.Should().BeTrue();
                signInSucceeded.Should().BeTrue();
            }
            finally
            {
                AutomationPage.TryCloseBrowser();
            }
        }

        [TestMethod]
        public void HaveMessageOnFailure()
        {
            try
            {
                UserSignInCredentials userSignInCredentials = new UserSignInCredentials { SignInUrl = TestData.SignInUrl, UserName = "invalid", Password = "bad password" };
                OktaSignInPageActionSequence signInPageActionSequence = new OktaSignInPageActionSequence(userSignInCredentials);
                bool? signInSucceeded = false;
                bool? signInExecuted = false;
                bool? signInError = false;
                bool? signInFailed = false;
                signInPageActionSequence.Success += (sender, args) => signInSucceeded = true;
                signInPageActionSequence.Executed += (sender, args) => signInExecuted = true;
                signInPageActionSequence.Error += (sender, args) => signInError = true;
                signInPageActionSequence.SignInFailed += (sender, args) => 
                {
                    OktaSignInFailedEventArgs oktaSignInFailedEventArgs = (OktaSignInFailedEventArgs)args;
                    oktaSignInFailedEventArgs.Message.Should().Be(signInPageActionSequence.SignInFailureMessage());
                    signInFailed = true;
                };

                signInPageActionSequence.ExecuteAsync().Wait();

                signInExecuted.Should().BeTrue();
                signInSucceeded.Should().BeFalse();
                signInError.Should().BeTrue();
                signInFailed.Should().BeTrue();

                string signInFailureMessage = signInPageActionSequence.SignInFailureMessage();
                signInFailureMessage.Should().NotBeNullOrEmpty();
                signInFailureMessage.Should().Be("Unable to sign in");
                Console.WriteLine(signInFailureMessage);
            }
            finally
            {
                AutomationPage.TryCloseBrowser();
            }
        }
    }
}
