// <copyright file="UserSignInEventArgs.cs" company="Okta, Inc">
// Copyright (c) 2020 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;

namespace Okta.Wizard
{
    public class UserSignInEventArgs : EventArgs
    {
        public UserSignInInfo UserSignInInfo{ get; set; }
        public List<PageActionResult> SequenceResults{ get; set; }
        public string Message => Exception.Message;
        public Exception Exception{ get; set; }
    }
}
