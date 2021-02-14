// <copyright file="EnsureOneApiTokenPageActionSequenceShould.cs" company="Okta, Inc">
// Copyright (c) 2020 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Okta.Wizard.Automation.Okta;
using System;
using System.Collections.Generic;
using System.Text;

namespace Okta.Wizard.Automation.Tests.Integration
{
    [TestClass]
    public class EnsureOneApiTokenPageActionSequenceShould
    {
        [TestMethod]
        public void RaiseSignInFailedEvent()
        {
            try
            {
                UserSignInCredentials userSignInCredentials = new UserSignInCredentials { SignInUrl = "https://dev-6437470.okta.com/", UserName = "invalid", Password = "bad password" };
                EnsureOneApiTokenPageActionSequence ensureOneApiTokenPageActionSequence = new EnsureOneApiTokenPageActionSequence(userSignInCredentials, $"{nameof(EnsureOneApiTokenPageActionSequenceShould)}_{nameof(RaiseSignInFailedEvent)}_Test");
                bool? signInFailed = false;
                string message = string.Empty;
                ensureOneApiTokenPageActionSequence.SignInFailed += (sender, args) =>
                {
                    OktaSignInFailedEventArgs signInFailedEventArgs = (OktaSignInFailedEventArgs)args;
                    signInFailed = true;
                    message = signInFailedEventArgs.Message;
                };

                ensureOneApiTokenPageActionSequence.ExecuteAsync().Wait();

                signInFailed.Should().BeTrue();
                message.Should().NotBeNullOrEmpty();
                message.Should().Be("Unable to sign in");
            }
            finally
            {
                AutomationPage.TryCloseBrowser();
            }
        }

        [TestMethod]
        public void Succeed()
        {
            try
            {
                string tokenName = $"{nameof(EnsureOneApiTokenPageActionSequenceShould)}_{nameof(Succeed)}_Test";
                EnsureOneApiTokenPageActionSequence ensureOneApiTokenPageActionSequence = new EnsureOneApiTokenPageActionSequence(tokenName);
                ensureOneApiTokenPageActionSequence.EnableDebug();

                bool? readExecuted = false;
                bool? readSucceeded = false;
                bool? foundExistingToken = false;
                bool? existingTokenDeleted = false;
                bool? deleteExecuted = false;
                bool? deleteSucceeded = false;
                bool? tokenCreated = false;
                ensureOneApiTokenPageActionSequence.ReadExecuted += (sender, args) => readExecuted = true;
                ensureOneApiTokenPageActionSequence.ReadSucceeded += (sender, args) => readSucceeded = true;
                ensureOneApiTokenPageActionSequence.FoundExistingToken += (sender, args) => foundExistingToken = true;
                ensureOneApiTokenPageActionSequence.ExistingTokenDeleted += (sender, args) => existingTokenDeleted = true;
                ensureOneApiTokenPageActionSequence.TokenCreated += (sender, args) =>
                {
                    TokenCreatedEventArgs tokenCreatedEventArgs = (TokenCreatedEventArgs)args;
                    tokenCreatedEventArgs.TokenName.Should().Be(tokenName);
                    tokenCreatedEventArgs.TokenValue.Should().NotBeNullOrEmpty();
                    tokenCreated = true;
                };

                ensureOneApiTokenPageActionSequence.DeleteExecuted += (sender, args) => deleteExecuted = true;
                ensureOneApiTokenPageActionSequence.DeleteSucceeded += (sender, args) => deleteSucceeded = true;

                ensureOneApiTokenPageActionSequence.ExecuteAsync().Wait();

                readExecuted.Should().BeTrue();
                readSucceeded.Should().BeTrue();
                if (foundExistingToken == true)
                {
                    deleteExecuted.Should().BeTrue();
                    existingTokenDeleted.Should().BeTrue();
                }

                if (deleteExecuted == true)
                {
                    deleteSucceeded.Should().BeTrue();
                }
                ensureOneApiTokenPageActionSequence.Succeeded.Should().BeTrue();
                tokenCreated.Should().BeTrue();
                ensureOneApiTokenPageActionSequence.TokenValue.Should().NotBeNullOrEmpty();
            }
            finally
            {
                AutomationPage.TryCloseBrowser();
            }
        }
    }
}
