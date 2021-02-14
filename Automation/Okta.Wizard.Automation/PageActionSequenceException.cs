// <copyright file="PageActionSequenceException.cs" company="Okta, Inc">
// Copyright (c) 2020 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using System;

namespace Okta.Wizard.Automation
{
    public class PageActionSequenceException: Exception
    {
        public PageActionSequenceException(PageActionSequence pageActionSequence, string message) : base(message)
        {
            PageActionSequence = pageActionSequence;
        }

        public PageActionSequence PageActionSequence { get; set; }
    }
}
