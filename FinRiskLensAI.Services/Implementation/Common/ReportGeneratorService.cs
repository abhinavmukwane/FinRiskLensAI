using PuppeteerSharp;
using PuppeteerSharp.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinRiskLensAI.Services.Implementation.Common
{
    public class ReportGeneratorService
    {
        //public ReportGeneratorService()
        //{
        //    new BrowserFetcher().DownloadAsync().GetAwaiter().GetResult();
        //}

        public async Task<byte[]> ConvertHtmlToPdfAsync(string html)
        {
            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true
            });

            await using var page = await browser.NewPageAsync();

            await page.SetContentAsync(html);

            return await page.PdfDataAsync(new PdfOptions
            {
                Format = PaperFormat.A4,
                PrintBackground = true,
                MarginOptions = new MarginOptions
                {
                    Top = "15mm",
                    Bottom = "15mm",
                    Left = "10mm",
                    Right = "10mm"
                }
            });
        }
    }
}
