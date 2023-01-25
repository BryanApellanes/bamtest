using System.IO;

namespace Bam.Net.Testing.Automation
{
    public class AutomationPageDebugInfo
    {
        public FileInfo ScreenShot { get; set; }
        public string Message{ get; set; }
        public AutomationPage AutomationPage{ get; set; }
    }
}
