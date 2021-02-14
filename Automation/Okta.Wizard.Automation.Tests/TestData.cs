// <copyright file="TestData.cs" company="Okta, Inc">
// Copyright (c) 2020 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.IO;

namespace Okta.Wizard.Automation.Tests
{
    public class TestData
    {
        public static string SignInUrl = "https://dev-6437470.okta.com/";
        public static string UserName = "testUserName"; // only for unit testing, not a valid user
        public static string Password = "testPassword"; // only for unit testing, not a valid password

        public static string UserCredentialsInputFilePath = Path.Combine(OktaWizardConfig.OktaWizardHome, "testUserCredentials.ejson");
        public static string OktaApiTokenOutputFilePath = Path.Combine(Environment.GetEnvironmentVariable("TMP") ?? "/tmp", "Okta", "oktaApiToken.ejson");
    }
}
