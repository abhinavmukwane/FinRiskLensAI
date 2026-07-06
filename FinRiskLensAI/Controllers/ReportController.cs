using FinRiskLensAI.Core.Interfaces.IServices.Common;
using FinRiskLensAI.Services.Implementation.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Playwright;
using Rotativa.AspNetCore;

namespace FinRiskLensAI.Controllers
{
    public class ReportController : Controller
    {
        private readonly ReportGeneratorService _repService;
        private readonly IViewRenderService _viewRenderService;

        public ReportController(
          ReportGeneratorService pdfService,
          IViewRenderService viewRenderService)
        {
            _repService = pdfService;
            _viewRenderService = viewRenderService;

        }

        public IActionResult FinancialReport()
        {
            return View();
        }

        public async Task<IActionResult> DownloadFinancialReport()
        {
            string html = await _viewRenderService.RenderViewToStringAsync(
                this,
                "FinancialReport",
                null);

            byte[] pdf = await _repService.ConvertHtmlToPdfAsync(html);

            return File(pdf, "application/pdf", "FinancialReport.pdf");
        }


        //[HttpGet]
        //public async Task<IActionResult> DownloadFinancialReportPdf()
        //{
        //    var baseUrl = $"{Request.Scheme}://{Request.Host}";
        //    var reportUrl = $"{baseUrl}/Report/FinancialReport"; // your actual route

        //    using var playwright = await Playwright.CreateAsync();
        //    await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        //    {
        //        Headless = true
        //    });

        //    var page = await browser.NewPageAsync();

        //    // if FinancialReport requires auth, forward the request cookie:
        //    if (Request.Cookies.Count > 0)
        //    {
        //        var context = page.Context;
        //        await context.AddCookiesAsync(Request.Cookies.Select(c => new Cookie
        //        {
        //            Name = c.Key,
        //            Value = c.Value,
        //            Url = baseUrl
        //        }));
        //    }

        //    await page.GotoAsync(reportUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        //    await page.WaitForTimeoutAsync(1200); // let Chart.js finish drawing before the snapshot
        //    await page.EmulateMediaAsync(new PageEmulateMediaOptions { Media = Media.Print });

        //    var pdfBytes = await page.PdfAsync(new PagePdfOptions
        //    {
        //        Format = "A4",
        //        PrintBackground = true,
        //        Margin = new Margin { Top = "14mm", Bottom = "16mm", Left = "10mm", Right = "10mm" }
        //    });

        //    return File(pdfBytes, "application/pdf", "FinancialHealthReport.pdf");
        //}
    }
}
