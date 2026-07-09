using FinRiskLensAI.Common;
using FinRiskLensAI.Core.Interfaces;
using FinRiskLensAI.Core.Interfaces.IServices.Common;
using FinRiskLensAI.Core.Models.Storage;
using FinRiskLensAI.Services.Implementation.Common;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Utilities.Collections;

namespace FinRiskLensAI.Controllers
{
    public class ReportController : Controller
    {
        private readonly IEmailService _emailService;
        private readonly IBlobAnalysisService _analysis;
        private readonly IMsmeDataStore _store;
        public ReportController(IEmailService emailService, IBlobAnalysisService analysis, IMsmeDataStore store)
        {
            _emailService = emailService; _analysis = analysis;
            _store = store;
        }

        public IActionResult FinancialReport()
        {
            return View();
        }

        //[HttpPost]
        //public async Task<IActionResult> SendReportEmail(string? uan, CancellationToken ct)
        //{
        //    uan ??= HttpContext.Session.GetCurrentUser()?.UdyamNumber;

        //    if (string.IsNullOrWhiteSpace(uan))
        //        return Json(new { success = false, message = "No Udyam number found for this account." });

        //    uan = uan.Trim();

        //    // Score must already exist for this UAN
        //    var result = await _analysis.GetResultAsync(uan, ct);
        //    if (result == null)
        //        return Json(new { success = false, message = "No financial health result found for this Udyam number." });

        //    // Enterprise name + recipient details from the stored Udyam payload
        //    string? enterpriseName = null;
        //    string? recipientEmail = null;
        //    string? recipientName = null;

        //    var udyamJson = await _store.DownloadAsync(uan, MsmeDataFiles.Udyam, ct);
        //    if (udyamJson != null)
        //    {
        //        var udyam = JObject.Parse(udyamJson);
        //        enterpriseName = udyam.SelectToken("main_details.name_of_enterprise")?.Value<string>();
        //        recipientEmail = udyam.SelectToken("main_details.email")?.Value<string>()
        //                          ?? udyam.SelectToken("contact_details.email")?.Value<string>();
        //        recipientName = udyam.SelectToken("main_details.applicant_name")?.Value<string>();
        //    }

        //    // Fall back to the logged-in user's own email if Udyam payload has none
        //    recipientEmail ??= HttpContext.Session.GetCurrentUser()?.Email;

        //    if (string.IsNullOrWhiteSpace(recipientEmail))
        //        return Json(new { success = false, message = "No email address found to send the report to." });

        //    var reportUrl = Url.Action("Download", "FinancialHealthReport", new { uan }, Request.Scheme)!;

        //    var sent = await _emailService.SendReportReadyEmailAsync(
        //        toEmail: recipientEmail,
        //        recipientName: recipientName,
        //        businessName: enterpriseName ?? uan,
        //        financialHealthScore: (int)Math.Round(result.OverallScore),
        //        riskBand: result.ScoreBand.ToString(),
        //        reportDate: result.ComputedAt.ToString("dd MMM yyyy"),
        //        reportUrl: reportUrl,
        //        metric1Label: "Monthly Balance", metric1Value: $"{result.BankAnalysis.AverageMonthlyBalance:0}/100",
        //        metric2Label: "Profit Margin", metric2Value: $"{result.Financials.NetProfitMargin:0}/100",
        //        metric3Label: "Compliance Quotient", metric3Value: $"{result.Financials.BusinessTurnover:0}/100",
        //        theme: "theme1",
        //        ct: ct);

        //    return Json(new { success = sent, message = sent ? "Report email sent." : "Failed to send report email." });
        //}

    }
}
