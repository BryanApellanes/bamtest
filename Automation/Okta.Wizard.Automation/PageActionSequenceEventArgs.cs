// <copyright file="PageActionSequenceEventArgs.cs" company="Okta, Inc">
// Copyright (c) 2020 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;

namespace Okta.Wizard.Automation
{
    public class PageActionSequenceEventArgs : EventArgs
    {
        public PageActionSequenceEventArgs(PageActionSequence pageActionSequence)
        {
            PageActionSequence = pageActionSequence;
        }
        public IAutomationPage Page => PageActionSequence.Page;
        public PageActionSequence PageActionSequence{ get; set; }
        public PageActionResult PageActionResult{ get; set; }
        public PageAction PageAction{ get; set; }
        public List<PageActionResult> Results{ get; set; }
        public Exception Exception { get; set; }
    }
}
