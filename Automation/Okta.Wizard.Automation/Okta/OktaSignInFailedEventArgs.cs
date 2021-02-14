// <copyright file="OktaSignInFailedEventArgs.cs" company="Okta, Inc">
// Copyright (c) 2020 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using System;

namespace Okta.Wizard.Automation.Okta
{
    public class OktaSignInFailedEventArgs : EventArgs
    {
        public UserSignInCredentials UserSignInCredentials { get; set; }
        public string Message{ get; set; }
    }
}
