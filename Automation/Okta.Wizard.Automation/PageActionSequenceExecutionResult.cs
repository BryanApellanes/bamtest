// <copyright file="PageActionSequenceExecutionResult.cs" company="Okta, Inc">
// Copyright (c) 2020 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Okta.Wizard.Automation
{
    public class PageActionSequenceExecutionResult
    {
        public PageActionSequenceExecutionResult() { }
        public PageActionSequenceExecutionResult(PageActionSequence pageActionSequence, List<PageActionResult> results)
        {
            PageActionSequence = pageActionSequence;
            Results = results;
        }
        public PageActionSequence PageActionSequence { get; set; }
        public List<PageActionResult> Results { get; set; }
        public bool Success => !HasFailures;
        public bool HasFailures => Results.Any(result => result.Succeeded == false);
        public List<PageActionResult> GetFailures()
        {
            return Results.Where(result => result.Succeeded == false).ToList();
        }
        public Dictionary<string, string> GetFailureMessages()
        {
            Dictionary<string, string> results = new Dictionary<string, string>();
            foreach(PageActionResult result in GetFailures())
            {
                results.Add(result.StepName, result.Message);
            }
            return results;
        }

        public void ScreenShot(string fileName)
        {
            PageActionSequence.Page.ScreenshotAsync(Path.Combine(PageActionSequence.ScreenShotsDirectory, fileName));
        }
    }
}
