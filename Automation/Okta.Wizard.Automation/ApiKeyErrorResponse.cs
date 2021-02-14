// <copyright file="ApiKeyErrorResponse.cs" company="Okta, Inc">
// Copyright (c) 2020 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using DevEx.Internal;
using Okta.Wizard.Automation.Okta;
using System.Collections.Generic;
using System.IO;

namespace Okta.Wizard.Automation
{
    public class ApiKeyErrorResponse : Serializable
    {
        static ApiKeyErrorResponse()
        {
            FilePath = Path.Combine(OktaWizardConfig.OktaWizardHome, "aker.json");
        }

        public static string FilePath { get; set; }

        public OktaSignInFailedEventArgs OktaSignInFailedEventArgs { get; set; }
        public List<PageActionResult> PageActionResults{ get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public bool FailureOccurred => PageActionResults != null;

        public void Save()
        {
            File.WriteAllText(FilePath, ToJson(true));
        }
    }
}
