using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuppeteerSharp;

namespace WebsiteWatcher
{
    public class PdfCreatorService
    {
        public async Task<Stream> ConvertPageToPdfAsAsync(string url)
        {
            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();
            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
            await using var page = await browser.NewPageAsync();
            await page.GoToAsync(url);
            await page.EvaluateExpressionAsync("document.fonts.ready");
            var result = await page.PdfStreamAsync();
            result.Position = 0;
            return result;

        }
    }
}
