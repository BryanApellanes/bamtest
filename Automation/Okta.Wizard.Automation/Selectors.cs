// <copyright file="Selectors.cs" company="Okta, Inc">
// Copyright (c) 2020 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

namespace Okta.Wizard.Automation.Okta
{
    public static class Selectors
    {
        public const string SignInUserName = "#okta-signin-username";
        public const string SignInPassword = "#okta-signin-password";
        public const string SignInSubmit = "#okta-signin-submit";
        public const string LogoutLink = "#logout-link";
        public const string LoginError = "#form1 > div.o-form-content.o-form-theme.clearfix > div.o-form-error-container.o-form-has-errors > div";
        public const string NavAdminApi = "#nav-admin-api";
        public const string TokenDeviceName = ".token-device-name";
        public const string CreateTokenButton = "#token-list > div > div.outside.data-list-toolbar.clearfix > a";
        public const string SubmitCreateTokenButton = "input[value='Create Token']";
        public const string CreateTokenError = ".okta-form-input-error";
        public const string ApiToken = "input[name=apiToken]";
        public const string ConfirmApiTokenButton = "input[value='OK, got it']";
        public const string RevokeButtons = "[data-type=btn-revoke] a";
        public const string ConfirmRevokeApiTokenButton = "input[value=Revoke]";
    }
}
