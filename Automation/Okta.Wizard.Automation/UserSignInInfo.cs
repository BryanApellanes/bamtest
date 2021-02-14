// <copyright file="UserSignInInfo.cs" company="Okta, Inc">
// Copyright (c) 2020 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using Okta.Wizard.Automation.Okta;
using Okta.Wizard.Internal;

namespace Okta.Wizard
{
    public class UserSignInInfo
    {
        public UserSignInInfo() :
        this(Deserialize.FromEnvironmentVariables<UserSignInCredentials>())
        {
        }

        public UserSignInInfo(UserSignInCredentials userSignInCredentials)
        {
            UserSignInCredentials = userSignInCredentials;
            UserNameInputSelector = Selectors.SignInUserName;
            PasswordInputSelector = Selectors.SignInPassword;
            SubmitSelector = Selectors.SignInSubmit;
        }

        public UserSignInCredentials UserSignInCredentials{ get; set; }
        public string UserNameInputSelector{ get; set; }
        public string PasswordInputSelector{ get; set; }
        public string SubmitSelector{ get; set; }
    }
}
