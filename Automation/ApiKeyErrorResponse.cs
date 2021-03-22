using Bam.Net.Testing.Automation;
using System.Collections.Generic;
using System.IO;
using Bam.Net;

namespace Bam.Net.Testing.Automation
{
    public class ApiKeyErrorResponse 
    {
        static ApiKeyErrorResponse()
        {
            FilePath = Path.Combine(BamProfile.Data, $"{nameof(ApiKeyErrorResponse)}.json");
        }

        public static string FilePath { get; set; }

        public List<PageActionResult> PageActionResults{ get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public bool FailureOccurred => PageActionResults != null;

        public void Save()
        {
            File.WriteAllText(FilePath, this.ToJson(true));
        }
    }
}
