// <copyright file="AutomationPageShould.cs" company="Okta, Inc">
// Copyright (c) 2020 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Okta.Wizard.Automation.Tests.Integration
{
    [TestClass]
    public class AutomationPageShould
    {
        [TestMethod]
        public async Task GetElementText()
        {
            try
            {
                AutomationPage page = AutomationPage.Open("https://okta.com");
                string text = await page.GetElementTextAsync("#myokta-signin");
                text.Should().Be("Sign In");
            }
            finally
            {
                AutomationPage.TryCloseBrowser();
            }
        }

        [TestMethod]
        public async Task BeOnSpecifiedPath()
        {
            try
            {
                AutomationPage page = AutomationPage.Open("https://www.okta.com/contact-sales/");
                (await page.IsAtPathAsync("/contact-sales")).Should().BeTrue();
            }
            finally
            {
                AutomationPage.TryCloseBrowser();
            }
        }

        [TestMethod]
        public async Task GoToPath()
        {
            try
            {
                AutomationPage page = AutomationPage.Open("https://okta.com");

                await page.GoToPathAsync("/contact-sales");
                (await page.IsAtPathAsync("/contact-sales")).Should().BeTrue();
            }
            finally
            {
                AutomationPage.TryCloseBrowser();
            }
        }

        [TestMethod]
        public async Task GetAllElementText()
        {
            try
            {
                AutomationPage page = AutomationPage.Open("https://okta.com");
                string[] results = await page.GetAllElementTextAsync(".utility__menu > ul > li");
                results.Should().NotBeNull();
                results.Length.Should().Be(5);

                results[0].Should().Be("Privacy Policy");
                results[1].Should().Be("Security");
                results[2].Should().Be("FAQ");
                results[3].Should().Be("Sitemap");
                results[4].Should().Be("Visit our Developer Site");
            }
            finally
            {
                AutomationPage.TryCloseBrowser();
            }
        }

        [TestMethod]
        public async Task GetElementValue()
        {
            try
            {
                AutomationPage page = AutomationPage.Open("https://developer.okta.com");
                string value = await page.GetElementValueAsync("#okta-signin-username");

                value.Should().Be("leia@rebelalliance.io");
            }
            finally
            {
                AutomationPage.TryCloseBrowser();
            }
        }
    }
}
