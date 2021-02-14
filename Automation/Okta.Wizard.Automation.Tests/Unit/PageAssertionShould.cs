// <copyright file="PageAssertionShould.cs" company="Okta, Inc">
// Copyright (c) 2020 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using NSubstitute;
using FluentAssertions;

namespace Okta.Wizard.Automation.Tests.Unit
{
    [TestClass]
    public class PageAssertionShould
    {
        [TestMethod]
        public void PassForMatchingPath()
        {
            string path = "/path/to/a/resource";
            Uri testUri = new Uri($"https://any.cxm{path}");
            PageAssertion pageAssertion = new PagePathAssertion($"{nameof(PassForMatchingPath)}_Test", path);
            bool? passedEventRaised = false;
            pageAssertion.AssertionPassed += (sender, args) => passedEventRaised = true;
            IAutomationPage page = Substitute.For<IAutomationPage>();
            page.Url.Returns(testUri.ToString());
            PageAssertionResult result = pageAssertion.Execute(page);
            result.Passed.Should().BeTrue();
            passedEventRaised.Should().BeTrue();
        }

        [TestMethod]
        public void DetermineElementPresenceBySelector()
        {
            PageAssertion pageAssertion = new PageAssertion($"{nameof(DetermineElementPresenceBySelector)}_Test", async (page) => 
            {
                return new PageAssertionResult(page, await page.IsPresentAsync(".header")); // assert that there is an element matching the selector
            });
            AutomationPage page = AutomationPage.Open("https://okta.com");
            PageAssertionResult result = pageAssertion.Execute(page);
            result.Passed.Should().BeTrue();
            Console.WriteLine("passed");
        }
    }
}
