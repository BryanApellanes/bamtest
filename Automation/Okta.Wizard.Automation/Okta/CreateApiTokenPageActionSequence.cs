// <copyright file="CreateApiTokenPageActionSequence.cs" company="Okta, Inc">
// Copyright (c) 2020 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Okta.Wizard.Automation.Okta
{
    public class CreateApiTokenPageActionSequence : PageActionSequence
    {
        public CreateApiTokenPageActionSequence(string tokenNameToCreate) : this(UserSignInCredentials.FromEnvironmentVariables(), tokenNameToCreate)
        {
        }
        public CreateApiTokenPageActionSequence(UserSignInCredentials userSignInCredentials, string tokenName) : this(userSignInCredentials, "default", tokenName)
        {
            Category = GetDefaultCateogry();
        }

        public CreateApiTokenPageActionSequence(UserSignInCredentials userSignInCredentials, string category, string tokenName): base("Create Api Token Page Action Sequence")
        {
            Category = category;
            ReadApiTokenNamesPageActionSequence = new ReadApiTokenNamesPageActionSequence(userSignInCredentials);
            ReadApiTokenNamesPageActionSequence.Success += (sender, args) =>
            {
                PageActionSequenceEventArgs pageActionSequenceEventArgs = (PageActionSequenceEventArgs)args;
                TokenNames = ReadApiTokenNamesPageActionSequence.TokenNames;
            };

            UserSignInCredentials = userSignInCredentials;
            Name = $"Create Api token {tokenName}";
            this.AddStep("verify token name not in use", (page) => 
            {
                if (new List<string>(ReadApiTokenNamesPageActionSequence.TokenNames).Contains(tokenName))
                {
                    throw new InvalidOperationException($"The specified token name is already in use: {tokenName}");
                };
            }, Tags.Validation, Tags.Throws)
            .AddStep("click create token button", (page) =>
            {                
                page.ClickAsync(Selectors.CreateTokenButton).Wait();
            }, Tags.Action, Tags.Click)
            .AddStep("enter token name", (page) => 
            {
                page.KeysAsync(tokenName).Wait();                
            }, Tags.Action, Tags.Keyboard)
            .AddStep("click submit create token button", async (page) =>
            {
                await page.ClickAsync(Selectors.SubmitCreateTokenButton);
                await page.AssertElementIsNotPresentAsync(Selectors.CreateTokenError);
            }, Tags.Action, Tags.Click, Tags.Submit)
            .AddStep("read api token", async (page) =>
            {
                CreatedApiToken = await page.GetElementValueAsync(Selectors.ApiToken);
                await page.ClickAsync(Selectors.ConfirmApiTokenButton);
            }, Tags.Action, Tags.Read);
        }

        public override PageActionSequence EnableDebug(string screenshotsDirectory = null)
        {
            ReadApiTokenNamesPageActionSequence.EnableDebug(screenshotsDirectory);
            return base.EnableDebug(screenshotsDirectory);
        }

        public ReadApiTokenNamesPageActionSequence ReadApiTokenNamesPageActionSequence { get; set; }
        public UserSignInCredentials UserSignInCredentials { get; set; }
        public string[] TokenNames { get; set; }

        public string CreatedApiToken{ get; set; }

        public override async Task<PageActionSequenceExecutionResult> ExecuteAsync(ReExecutionStrategy reExecutionStrategy = ReExecutionStrategy.ForErrors, bool continueOnFailure = false)
        {
            if(!ReadApiTokenNamesPageActionSequence.HasExecuted)
            {
                await ReadApiTokenNamesPageActionSequence.ExecuteAsync(reExecutionStrategy, continueOnFailure);
            }

            PageActionSequenceExecutionResult result = await base.ExecuteAsync(ReadApiTokenNamesPageActionSequence.Page, continueOnFailure);
            result.Results.InsertRange(0, ReadApiTokenNamesPageActionSequence.ExecutionResult.Results);
            return result;
        }
    }
}
