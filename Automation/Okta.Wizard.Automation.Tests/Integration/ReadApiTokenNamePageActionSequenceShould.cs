// <copyright file="ReadApiTokenNamePageActionSequenceShould.cs" company="Okta, Inc">
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
    public class ReadApiTokenNamePageActionSequenceShould
    {

        [TestMethod]
        public void ReadApiTokensPage()
        {
            try
            {
                PageActionSequence.DefaultNavigationOptions = AutomationPage.OriginalTimeoutNavigationOptions;

                string expectedPath = "/admin/access/api/tokens";

                ReadApiTokenNamesPageActionSequence readApiTokenNamesPageActionSequence = new ReadApiTokenNamesPageActionSequence();
                readApiTokenNamesPageActionSequence.EnableDebug();
                bool? successEventFired = false;
                readApiTokenNamesPageActionSequence.Success += (sender, args) => successEventFired = true;

                PageActionSequenceExecutionResult result = readApiTokenNamesPageActionSequence.ExecuteAsync().Result;

                result.HasFailures.Should().BeFalse();
                readApiTokenNamesPageActionSequence.Succeeded.Should().BeTrue();
                successEventFired.Should().BeTrue();
                readApiTokenNamesPageActionSequence.Page.IsAtPathAsync(expectedPath).Result.Should().BeTrue();

                readApiTokenNamesPageActionSequence.TokenNames.Should().NotBeNull();
                readApiTokenNamesPageActionSequence.TokenNames.Length.Should().BeGreaterThan(0);

                Console.WriteLine("*** Found tokens");
                foreach (string name in readApiTokenNamesPageActionSequence.TokenNames)
                {
                    Console.WriteLine(name);
                }

            }
            finally
            {
                AutomationPage.TryCloseBrowser();
            }
        }
    }
}
