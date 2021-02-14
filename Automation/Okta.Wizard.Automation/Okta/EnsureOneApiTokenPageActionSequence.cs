// <copyright file="EnsureOneApiTokenPageActionSequence.cs" company="Okta, Inc">
// Copyright (c) 2020 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Okta.Wizard.Automation.Okta
{
    public class EnsureOneApiTokenPageActionSequence : PageActionSequence
    {
        public EnsureOneApiTokenPageActionSequence(string tokenName) : this(UserSignInCredentials.FromEnvironmentVariables(), tokenName)
        {
        }

        public EnsureOneApiTokenPageActionSequence(UserSignInCredentials userSignInCredentials, string tokenName) : base($"Ensure Only One Api Token Named ({tokenName}) Exists")
        {
            UserSignInCredentials = userSignInCredentials;
            TokenName = tokenName;
            CreateApiTokenPageActionSequence = new CreateApiTokenPageActionSequence(userSignInCredentials, tokenName);
            DeleteApiTokenPageActionSequence = new DeleteApiTokenPageActionSequence(userSignInCredentials, tokenName);
            ReadApiTokenNamesPageActionSequence = new ReadApiTokenNamesPageActionSequence(userSignInCredentials);

            ReadApiTokenNamesPageActionSequence.SignInPageActionSequence.SignInFailed += (sender, args) => SignInFailed?.Invoke(this, args);

            ReadApiTokenNamesPageActionSequence.Executed += (sender, args) => ReadExecuted?.Invoke(this, args);
            ReadApiTokenNamesPageActionSequence.Success += (sender, args) => ReadSucceeded?.Invoke(this, args);
            
            DeleteApiTokenPageActionSequence.Executed += (sender, args) => DeleteExecuted?.Invoke(this, args);
            DeleteApiTokenPageActionSequence.Success += (sender, args) =>
            {
                DeleteSucceeded?.Invoke(this, args);
                ExistingTokenDeleted?.Invoke(this, args);
            };

            CreateApiTokenPageActionSequence.Executed += (sender, args) => CreateExecuted?.Invoke(this, args);
            CreateApiTokenPageActionSequence.Success += (sender, args) => CreateSucceeded?.Invoke(this, args);
        }

        public override PageActionSequence EnableDebug(string screenshotsDirectory = null)
        {
            ReadApiTokenNamesPageActionSequence.EnableDebug(screenshotsDirectory);
            DeleteApiTokenPageActionSequence.EnableDebug(screenshotsDirectory);
            CreateApiTokenPageActionSequence.EnableDebug(screenshotsDirectory);
            return base.EnableDebug(screenshotsDirectory);
        }

        public UserSignInCredentials UserSignInCredentials { get; set; }
        public OktaSignInPageActionSequence SignInPageActionSequence 
        {
            get
            {
                return ReadApiTokenNamesPageActionSequence.SignInPageActionSequence;
            }
        }

        /// <summary>
        /// Gets or sets the sequence used to create an api token.
        /// </summary>
        public CreateApiTokenPageActionSequence CreateApiTokenPageActionSequence { get; set; }

        /// <summary>
        /// Gets or sets the sequence used to delete an api token.
        /// </summary>
        public DeleteApiTokenPageActionSequence DeleteApiTokenPageActionSequence { get; set; }

        /// <summary>
        /// Gets or sets the sequence used to read api tokens.
        /// </summary>
        public ReadApiTokenNamesPageActionSequence ReadApiTokenNamesPageActionSequence { get; set; }

        /// <summary>
        /// The event that is raised when sign in failed.
        /// </summary>
        public event EventHandler SignInFailed;

        /// <summary>
        /// The event that is raised when reading api tokens completes.
        /// </summary>
        public event EventHandler ReadExecuted;

        /// <summary>
        /// The event that is raised when reading api tokens succeeds.
        /// </summary>
        public event EventHandler ReadSucceeded;

        /// <summary>
        /// The event that is raised when a token by the specified name already exists.  The existing token is deleted and recreated by this action sequence.
        /// </summary>
        public event EventHandler FoundExistingToken;

        /// <summary>
        /// The event that is raised when the DeleteApiTokenPageActionSequence succeeds.
        /// </summary>
        public event EventHandler ExistingTokenDeleted;

        /// <summary>
        /// The event that is raised when DeleteApiTokenPageActionSequence finishes execution.
        /// </summary>
        public event EventHandler DeleteExecuted;

        /// <summary>
        /// The event that is raised when DeleteApiTokenPageActionSequence succeeds;
        /// </summary>
        public event EventHandler DeleteSucceeded;

        /// <summary>
        /// The event that is raised when the CreateApiTokenPageActionSequence finishes execution.
        /// </summary>
        public event EventHandler CreateExecuted;

        /// <summary>
        /// The event that is raised when the CreateApiTokenPageActionSequence succeeds.
        /// </summary>
        public event EventHandler CreateSucceeded;

        /// <summary>
        /// The event that is raised when a token is created.
        /// </summary>
        public event EventHandler TokenCreated;

        public override async Task<PageActionSequenceExecutionResult> ExecuteAsync(ReExecutionStrategy reExecutionStrategy = ReExecutionStrategy.ForErrors, bool continueOnFailure = false)
        {
            PageActionSequenceExecutionResult readResult = await ReadApiTokenNamesPageActionSequence.ExecuteAsync(reExecutionStrategy, continueOnFailure);
            IAutomationPage page = readResult.PageActionSequence.Page;
            DeleteApiTokenPageActionSequence.TokenNames = ReadApiTokenNamesPageActionSequence.TokenNames;

            if(readResult.Success && ReadApiTokenNamesPageActionSequence.TokenExists(TokenName))
            {
                FoundExistingToken?.Invoke(this, new PageActionSequenceEventArgs(this));
                List<PageAction> actionSteps = DeleteApiTokenPageActionSequence.GetTaggedSteps(Tags.Action);
                PageActionSequenceExecutionResult deleteResult = DeleteApiTokenPageActionSequence.ExecuteAsync(page, actionSteps).Result;
                if(deleteResult.Success)
                {
                    ExistingTokenDeleted?.Invoke(this, new PageActionSequenceEventArgs(this));
                }
                page = deleteResult.PageActionSequence.Page;               
            }
            if (readResult.Success == false)
            {
                ExecutionResult = readResult;
                OnFailure(this, new PageActionSequenceEventArgs(this) { Results = ReadApiTokenNamesPageActionSequence?.ExecutionResult?.Results });
                return ReadApiTokenNamesPageActionSequence.ExecutionResult;
            }

            ExecutionResult = await CreateApiTokenPageActionSequence.ExecuteAsync(page, Tags.Action);
            TokenValue = CreateApiTokenPageActionSequence.CreatedApiToken;
            if(ExecutionResult.Success)
            {
                TokenCreated?.Invoke(this, new TokenCreatedEventArgs { TokenName = TokenName, TokenValue = TokenValue });
            }
            return ExecutionResult;
        }

        public string TokenName { get; set; }
        public string TokenValue{ get; set; }
    }
}
