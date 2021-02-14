// <copyright file="DeleteApiTokenPageActionSequence.cs" company="Okta, Inc">
// Copyright (c) 2020 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using PuppeteerSharp;
using System.Collections.Generic;

namespace Okta.Wizard.Automation.Okta
{
    public class DeleteApiTokenPageActionSequence : ReadApiTokenNamesPageActionSequence
    {
        public DeleteApiTokenPageActionSequence(string tokenNameToDelete) : this(UserSignInCredentials.FromEnvironmentVariables(), tokenNameToDelete) 
        { 
        }

        public DeleteApiTokenPageActionSequence(UserSignInCredentials userSignInCredentials, string tokenNameToDelete) : base(userSignInCredentials) 
        {
            TokenNameToDelete = tokenNameToDelete;
            this.AddStep($"click revoke button for {TokenNameToDelete}", (page) =>
            {
                /// find the index of the token to delete and click the related button
                if (TokenNameExists(TokenNameToDelete))
                {
                    TokenWasPresent = true;
                    int tokenIndex = GetTokenIndex(TokenNameToDelete);
                    ElementHandle[] revokeButtons = page.QuerySelectorAllAsync(Selectors.RevokeButtons).Result;
                    revokeButtons[tokenIndex].ClickAsync();
                }
            }, Tags.Action)
            .AddStep($"confirm token revocation for {TokenNameToDelete}", (page) =>
            {
                if(TokenNameExists(TokenNameToDelete))
                {
                    if (!page.WaitForElementAsync(Selectors.ConfirmRevokeApiTokenButton).Result)
                    {
                        throw new DeleteTokenFailedException(TokenNameToDelete, "Failed to confirm revocation of API token");
                    }
                    page.ClickAsync(Selectors.ConfirmRevokeApiTokenButton);
                }
            }, Tags.Action);
        }

        public string TokenNameToDelete{ get; set; }
        public bool TokenWasPresent { get; set; }

        protected int GetTokenIndex(string tokenName)
        {
            for (int i = 0; i < TokenNames.Length; i++)
            {
                if (TokenNames[i].Equals(tokenName))
                {
                    return i;
                }
            }
            return -1;
        }

        protected bool TokenNameExists(string tokenName)
        {
            return new List<string>(TokenNames).Contains(tokenName);
        }
    }
}
