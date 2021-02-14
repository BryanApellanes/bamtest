// <copyright file="ReadApiTokenNamesPageActionSequence.cs" company="Okta, Inc">
// Copyright (c) 2020 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Okta.Wizard.Automation.Okta
{
    public class ReadApiTokenNamesPageActionSequence : OpenApiTokensPageActionSequence
    {
        public ReadApiTokenNamesPageActionSequence() : this(UserSignInCredentials.FromEnvironmentVariables())
        {
        }

        public ReadApiTokenNamesPageActionSequence(UserSignInCredentials userSignInCredentials) : base(userSignInCredentials)
        {
            Name = "Read Api token names";
            AddStep("Read Api token names", async (page) =>
            {
                TokenNames = await GetTokenNamesAsync(page);
            });
        }

        public string[] TokenNames { get; set; }

        public async Task<string[]> GetTokenNamesAsync(IAutomationPage automationPage)
        {
            return await automationPage.GetAllElementTextAsync(Selectors.TokenDeviceName);
        }

        public bool TokenExists(string tokenName)
        {
            return new List<string>(TokenNames).Contains(tokenName);
        }
    }
}
