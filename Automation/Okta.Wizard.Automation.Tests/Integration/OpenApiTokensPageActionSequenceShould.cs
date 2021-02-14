// <copyright file="OpenApiTokensPageActionSequenceShould.cs" company="Okta, Inc">
// Copyright (c) 2020 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Okta.Wizard.Automation.Okta;

namespace Okta.Wizard.Automation.Tests.Integration
{
    [TestClass]
    public class OpenApiTokensPageActionSequenceShould
    {

        [TestMethod]
        public void GoToApiTokensPage()
        {
            try
            {
                PageActionSequence.DefaultNavigationOptions = AutomationPage.OriginalTimeoutNavigationOptions;

                string expectedPath = "/admin/access/api/tokens";

                OpenApiTokensPageActionSequence openApiTokensPageActionSequence = new OpenApiTokensPageActionSequence();
                openApiTokensPageActionSequence.EnableDebug();
                bool? successEventFired = false;
                openApiTokensPageActionSequence.Success += (sender, args) => successEventFired = true;

                PageActionSequenceExecutionResult result = openApiTokensPageActionSequence.ExecuteAsync().Result;

                result.HasFailures.Should().BeFalse();
                openApiTokensPageActionSequence.Succeeded.Should().BeTrue();
                successEventFired.Should().BeTrue();
                openApiTokensPageActionSequence.Page.IsAtPathAsync(expectedPath).Result.Should().BeTrue();

            }
            finally
            {
                AutomationPage.TryCloseBrowser();
            }
        }
    }
}
