// <copyright file="IPage.cs" company="Okta, Inc">
// Copyright (c) 2020 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using PuppeteerSharp;
using System.Threading.Tasks;

namespace Okta.Wizard.Automation
{
    public interface IPage
    {
        string Name{ get; set; }

        Task<Response> GoToAsync(string url, int? timeout = null, WaitUntilNavigation[] waitUntil = null);

        Task ScreenshotAsync(string file);
        string Url{ get; set; }
    }
}
