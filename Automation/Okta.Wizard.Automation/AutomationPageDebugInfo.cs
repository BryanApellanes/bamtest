// <copyright file="AutomationPageDebugInfo.cs" company="Okta, Inc">
// Copyright (c) 2020 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using System.IO;

namespace Okta.Wizard.Automation
{
    public class AutomationPageDebugInfo
    {
        public FileInfo ScreenShot { get; set; }
        public string Message{ get; set; }
        public AutomationPage AutomationPage{ get; set; }
    }
}
