using FinRiskLensAI.Core.Interfaces.IServices.Common;
using FinRiskLensAI.Services.Implementation.Common;
using Microsoft.AspNetCore.Mvc;

namespace FinRiskLensAI.Controllers
{
    public class ReportController : Controller
    {
        private readonly IEmailService _emailService;

        public ReportController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public IActionResult FinancialReport()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SendReportReadyEmail(string udyamNumber, CancellationToken ct)
        {
            // TODO: replace with real fetch — score, band, metrics, recipient
            //var report = await GetFinancialReportDataAsync(udyamNumber, ct);

            //var sent = await _emailService.SendReportReadyEmailAsync(
            //    toEmail: report.OwnerEmail,
            //    recipientName: report.OwnerName,
            //    businessName: report.BusinessName,
            //    financialHealthScore: report.Score,
            //    riskBand: report.RiskBand,
            //    reportDate: report.GeneratedOn.ToString("dd MMM yyyy"),
            //    reportUrl: Url.Action("ViewReport", "Report", new { id = report.ReportId }, Request.Scheme)!,
            //    metric1Label: "Revenue Vitality", metric1Value: $"{report.RevenueVitality}/100",
            //    metric2Label: "Cash Flow Health", metric2Value: $"{report.CashFlowHealth}/100",
            //    metric3Label: "Compliance Quotient", metric3Value: $"{report.ComplianceQuotient}/100",
            //    theme: "theme1",
            //    ct: ct);

            //return sent ? Common.SuccessResponse("Report email sent.") : Common.ErrorResponse("Failed to send report email.");

            var sent = await _emailService.SendReportReadyEmailAsync(
            toEmail: "owner@example.com",
            recipientName: "Owner Name",
            businessName: "Business Name",
            financialHealthScore: 742,
            riskBand: "Low Risk",
            reportDate: DateTime.UtcNow.ToString("dd MMM yyyy"),
            reportUrl: Url.Action("ViewReport", "Report", new { id = udyamNumber }, Request.Scheme)!,
            metric1Label: "Revenue Vitality", metric1Value: "82/100",
            metric2Label: "Cash Flow Health", metric2Value: "76/100",
            metric3Label: "Compliance Quotient", metric3Value: "91/100",
            theme: "theme1",
            ct: ct);

            return sent
                ? Json(new { success = true, message = "Report email sent." })
                : Json(new { success = false, message = "Failed to send report email." });

        }

    }
}
