using PuppeteerSharp;
using System.Threading.Tasks;

namespace Bam.Net.Testing.Automation
{
    public interface IPage
    {
        string Name{ get; set; }

        Task<Response> GoToAsync(string url, int? timeout = null, WaitUntilNavigation[] waitUntil = null);

        Task ScreenshotAsync(string file);
        string Url{ get; set; }
    }
}
