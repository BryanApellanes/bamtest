// <copyright file="OpenApiTokensPageActionSequence.cs" company="Okta, Inc">
// Copyright (c) 2020 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using System.Threading.Tasks;

namespace Okta.Wizard.Automation.Okta
{
    public class OpenApiTokensPageActionSequence : PageActionSequence
    {
        public OpenApiTokensPageActionSequence() : this(UserSignInCredentials.FromEnvironmentVariables()) 
        { 
        }

        public OpenApiTokensPageActionSequence(UserSignInCredentials userSignInCredentials) : base("Open Api Tokens Page") 
        {
            SignInPageActionSequence = new OktaSignInPageActionSequence(userSignInCredentials);
            SignInPageActionSequence.Success += (sender, args) =>
             {
                 PageActionSequenceEventArgs pageActionSequenceEventArgs = (PageActionSequenceEventArgs)args;
                 ExecuteAsync(pageActionSequenceEventArgs.Page).Wait();                 
             };
            AddNavigationStep("open api token page", async (page) =>
            {
                await page.ClickAsync(Selectors.NavAdminApi);
                await page.WaitForNavigationAsync();
            });
        }

        public OktaSignInPageActionSequence SignInPageActionSequence{ get; set; }

        public override PageActionSequence EnableDebug(string screenshotsDirectory = null)
        {
            SignInPageActionSequence.EnableDebug(screenshotsDirectory);
            return base.EnableDebug(screenshotsDirectory);
        }

        public override async Task<PageActionSequenceExecutionResult> ExecuteAsync(ReExecutionStrategy reExecutionStrategy = ReExecutionStrategy.ForErrors, bool continueOnFailure = false)
        {
            if(!SignInPageActionSequence.HasExecuted)
            {
                await SignInPageActionSequence.ExecuteAsync(reExecutionStrategy, continueOnFailure);
            }

            if (SignInPageActionSequence.Succeeded)
            {
                PageActionSequenceExecutionResult result = await ExecuteAsync(SignInPageActionSequence.Page, continueOnFailure);
                result.Results.InsertRange(0, SignInPageActionSequence.ExecutionResult.Results);
                return result;
            }
            else
            {
                return SignInPageActionSequence.ExecutionResult;
            }
        }
    }
}
