// <copyright file="SignInFailedException.cs" company="Okta, Inc">
// Copyright (c) 2020 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

namespace Okta.Wizard.Automation
{
    public class SignInFailedException : Exception 
    {
        public SignInFailedException(IEnumerable<PageActionResult> pageAssertionResults)
        : base(string.Join("\r\n", pageAssertionResults.Select(par => par.ToString()).ToArray()))
        { }
    }
}
