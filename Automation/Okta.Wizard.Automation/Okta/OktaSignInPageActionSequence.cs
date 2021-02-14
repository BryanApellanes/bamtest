// <copyright file="OktaSignInPageActionSequence.cs" company="Okta, Inc">
// Copyright (c) 2020 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using System;

namespace Okta.Wizard.Automation.Okta
{
    public class OktaSignInPageActionSequence : PageActionSequence
    {
        public OktaSignInPageActionSequence() : this(UserSignInCredentials.FromEnvironmentVariables()) 
        {
        }

        public OktaSignInPageActionSequence(UserSignInCredentials userSignInCredentials): base("Sign In To Okta")
        {
            UserSignInCredentials = userSignInCredentials;
            StartUrl = UserSignInCredentials.SignInUrl;
            Executed += (sender, args) =>
            {
                if(!this.Succeeded)
                {
                    SignInFailed?.Invoke(this, new OktaSignInFailedEventArgs { UserSignInCredentials = userSignInCredentials, Message = SignInFailureMessage() });
                }
            };

            this.AddStep("enter username", (page) =>
            {
                page.KeysAsync(Selectors.SignInUserName, userSignInCredentials.UserName).Wait();
            })
            .AddStep("enter password", (page) =>
            {
                page.KeysAsync(Selectors.SignInPassword, userSignInCredentials.Password).Wait();
            })
            .AddNavigationStep("click sign in button", (page) =>
            {
                page.ClickAsync(Selectors.SignInSubmit).Wait();
                page.AssertElementIsNotPresentAsync(Selectors.LoginError, 500).Wait();
                page.WaitForNavigationAsync(NavigationOptions).Wait();
            });
        }

        public UserSignInCredentials UserSignInCredentials { get; set; }

        /// <summary>
        /// The event that is raised when sign in fails.
        /// </summary>
        public event EventHandler SignInFailed;

        public string SignInFailureMessage()
        {
            if(this.HasExecuted && this.HasErrors())
            {
                return Page.GetElementTextAsync(Selectors.LoginError).Result;
            }
            return string.Empty;
        }
    }
}
