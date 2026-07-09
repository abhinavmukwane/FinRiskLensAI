using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FinRiskLensAI.Services.Templates
{
    public static class ReportReadyEmail
    {
        public static string Subject(string businessName) => $"Your FinRiskLensAI Financial Health Report is ready — {businessName}";

        /// <summary>Email-safe colour palette for one theme.</summary>
        private sealed record Palette(
            string Primary,         // header band, headings
            string AccentSoft,      // "AI" span + header right label (on the dark band)
            string PageBg,          // page background behind the card
            string CardBorder,      // body card hairline
            string ScoreBoxBg,      // score box background
            string ScoreBoxBorder,  // score box dashed border
            string ScoreText,       // big score number
            string BandBadgeBg,     // risk-band pill background
            string BandBadgeText,   // risk-band pill text
            string Note,            // sub-note under score
            string Divider,         // hr colour
            string BodyText,        // paragraph text
            string SubtleText,      // small print
            string ButtonBg,        // CTA button background
            string ButtonText,      // CTA button text
            string MetricLabel,     // metric card label
            string MetricValue,     // metric card value
            string FooterText);     // footer line

        private static readonly Palette Theme1 = new(
            Primary: "#00836c", AccentSoft: "#FCD9B8", PageBg: "#eef4f5",
            CardBorder: "#cfe4e8", ScoreBoxBg: "#FDF1E5", ScoreBoxBorder: "#F0B683",
            ScoreText: "#f37021", BandBadgeBg: "#e8f5f0", BandBadgeText: "#00836c",
            Note: "#B35F0C", Divider: "#dceaed", BodyText: "#4a5b5f",
            SubtleText: "#75898e", ButtonBg: "#00836c", ButtonText: "#ffffff",
            MetricLabel: "#75898e", MetricValue: "#1f3335", FooterText: "#8aa0a5");

        private static readonly Palette Theme2 = new(
            Primary: "#57001d", AccentSoft: "#e8b7c3", PageBg: "#f6f1f3",
            CardBorder: "#ead9df", ScoreBoxBg: "#faf5f7", ScoreBoxBorder: "#d8c3ca",
            ScoreText: "#57001d", BandBadgeBg: "#f3e3e8", BandBadgeText: "#57001d",
            Note: "#974354", Divider: "#f0e2e7", BodyText: "#5c4a51",
            SubtleText: "#8a7580", ButtonBg: "#57001d", ButtonText: "#f7d9a0",
            MetricLabel: "#8a7580", MetricValue: "#3a2129", FooterText: "#a08a92");

        /// <summary>
        /// Builds the "report ready" notification email.
        /// </summary>
        /// <param name="recipientName">Contact / owner name.</param>
        /// <param name="businessName">MSME / business display name.</param>
        /// <param name="financialHealthScore">0–1000 composite score.</param>
        /// <param name="riskBand">e.g. "Low Risk", "Moderate Risk".</param>
        /// <param name="reportDate">Date the report was generated (display string).</param>
        /// <param name="reportUrl">Link to view/download the report (or PDF endpoint).</param>
        /// <param name="metric1Label">e.g. "Revenue Vitality".</param>
        /// <param name="metric1Value">e.g. "82/100".</param>
        /// <param name="metric2Label">e.g. "Cash Flow Health".</param>
        /// <param name="metric2Value">e.g. "76/100".</param>
        /// <param name="metric3Label">e.g. "Compliance Quotient".</param>
        /// <param name="metric3Value">e.g. "91/100".</param>
        /// <param name="theme">"theme1" (teal/orange) or "theme2" (maroon/gold). Defaults to theme1.</param>
        public static string Build(
            string? recipientName,
            string businessName,
            int financialHealthScore,
            string riskBand,
            string reportDate,
            string reportUrl,
            string metric1Label, string metric1Value,
            string metric2Label, string metric2Value,
            string metric3Label, string metric3Value,
            string? theme = null)
        {
            var p = string.Equals(theme?.Trim(), "theme2", StringComparison.OrdinalIgnoreCase) ? Theme2 : Theme1;

            var greeting = string.IsNullOrWhiteSpace(recipientName)
                ? "Hello,"
                : $"Hello {WebUtility.HtmlEncode(recipientName.Trim())},";

            var biz = WebUtility.HtmlEncode(businessName);
            var band = WebUtility.HtmlEncode(riskBand);
            var date = WebUtility.HtmlEncode(reportDate);
            var url = WebUtility.HtmlEncode(reportUrl);

            return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
<meta charset=""utf-8"" />
<meta name=""viewport"" content=""width=device-width, initial-scale=1"" />
<title>Your FinRiskLensAI Financial Health Report</title>
</head>
<body style=""margin:0;padding:0;background-color:{p.PageBg};"">
  <!-- preheader (hidden preview text) -->
  <div style=""display:none;max-height:0;overflow:hidden;mso-hide:all;"">
    {biz}'s Financial Health Score is {financialHealthScore}/1000 ({band}). View the full report inside.
  </div>

  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:{p.PageBg};padding:32px 12px;"">
    <tr>
      <td align=""center"">
        <table role=""presentation"" width=""600"" cellpadding=""0"" cellspacing=""0"" style=""width:600px;max-width:100%;"">

          <!-- header band -->
          <tr>
            <td style=""background-color:{p.Primary};border-radius:16px 16px 0 0;padding:26px 36px;"">
              <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"">
                <tr>
                  <td style=""font-family:Arial,Helvetica,sans-serif;font-size:20px;font-weight:bold;color:#ffffff;letter-spacing:.3px;"">
                    FinRiskLens<span style=""color:{p.AccentSoft};"">AI</span>
                  </td>
                  <td align=""right"" style=""font-family:Arial,Helvetica,sans-serif;font-size:11px;color:{p.AccentSoft};text-transform:uppercase;letter-spacing:.12em;"">
                    MSME Financial Health
                  </td>
                </tr>
              </table>
            </td>
          </tr>

          <!-- body card -->
          <tr>
            <td style=""background-color:#ffffff;padding:36px;border:1px solid {p.CardBorder};border-top:0;border-radius:0 0 16px 16px;"">

              <p style=""margin:0 0 6px;font-family:Arial,Helvetica,sans-serif;font-size:18px;font-weight:bold;color:{p.Primary};"">
                Your Financial Health Report is ready
              </p>
              <p style=""margin:0 0 22px;font-family:Arial,Helvetica,sans-serif;font-size:14px;line-height:22px;color:{p.BodyText};"">
                {greeting}<br />
                The Financial Health assessment for <strong>{biz}</strong> was generated on {date}.
                Here's a quick summary before you view the full report.
              </p>

              <!-- score box -->
              <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"">
                <tr>
                  <td align=""center"" style=""background-color:{p.ScoreBoxBg};border:1px dashed {p.ScoreBoxBorder};border-radius:12px;padding:26px 12px;"">
                    <span style=""display:block;font-family:Arial,Helvetica,sans-serif;font-size:11px;font-weight:bold;letter-spacing:.12em;text-transform:uppercase;color:{p.SubtleText};margin-bottom:8px;"">
                      Financial Health Score
                    </span>
                    <span style=""font-family:'Courier New',Courier,monospace;font-size:44px;font-weight:bold;color:{p.ScoreText};"">{financialHealthScore}</span>
                    <span style=""font-family:Arial,Helvetica,sans-serif;font-size:16px;color:{p.SubtleText};"">/1000</span>
                    <br />
                    <span style=""display:inline-block;margin-top:12px;padding:6px 16px;border-radius:999px;background-color:{p.BandBadgeBg};color:{p.BandBadgeText};font-family:Arial,Helvetica,sans-serif;font-size:12px;font-weight:bold;letter-spacing:.02em;"">
                      {band}
                    </span>
                  </td>
                </tr>
              </table>

              <!-- metric mini-cards -->
              <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""margin-top:18px;"">
                <tr>
                  <td width=""33.33%"" align=""center"" style=""background-color:{p.PageBg};border-radius:10px;padding:14px 8px;"">
                    <span style=""display:block;font-family:Arial,Helvetica,sans-serif;font-size:10px;text-transform:uppercase;letter-spacing:.06em;color:{p.MetricLabel};margin-bottom:4px;"">{WebUtility.HtmlEncode(metric1Label)}</span>
                    <span style=""font-family:Arial,Helvetica,sans-serif;font-size:15px;font-weight:bold;color:{p.MetricValue};"">{WebUtility.HtmlEncode(metric1Value)}</span>
                  </td>
                  <td width=""4""></td>
                  <td width=""33.33%"" align=""center"" style=""background-color:{p.PageBg};border-radius:10px;padding:14px 8px;"">
                    <span style=""display:block;font-family:Arial,Helvetica,sans-serif;font-size:10px;text-transform:uppercase;letter-spacing:.06em;color:{p.MetricLabel};margin-bottom:4px;"">{WebUtility.HtmlEncode(metric2Label)}</span>
                    <span style=""font-family:Arial,Helvetica,sans-serif;font-size:15px;font-weight:bold;color:{p.MetricValue};"">{WebUtility.HtmlEncode(metric2Value)}</span>
                  </td>
                  <td width=""4""></td>
                  <td width=""33.33%"" align=""center"" style=""background-color:{p.PageBg};border-radius:10px;padding:14px 8px;"">
                    <span style=""display:block;font-family:Arial,Helvetica,sans-serif;font-size:10px;text-transform:uppercase;letter-spacing:.06em;color:{p.MetricLabel};margin-bottom:4px;"">{WebUtility.HtmlEncode(metric3Label)}</span>
                    <span style=""font-family:Arial,Helvetica,sans-serif;font-size:15px;font-weight:bold;color:{p.MetricValue};"">{WebUtility.HtmlEncode(metric3Value)}</span>
                  </td>
                </tr>
              </table>

              <!-- CTA button -->
              <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""margin-top:26px;"">
                <tr>
                  <td align=""center"">
                    <a href=""{url}"" target=""_blank""
                       style=""display:inline-block;background-color:{p.ButtonBg};color:{p.ButtonText};font-family:Arial,Helvetica,sans-serif;font-size:14px;font-weight:bold;text-decoration:none;padding:14px 34px;border-radius:8px;"">
                      View Full Report
                    </a>
                  </td>
                </tr>
              </table>

              <p style=""margin:18px 0 0;font-family:Arial,Helvetica,sans-serif;font-size:13px;line-height:20px;color:{p.Note};text-align:center;"">
                &#128196;&nbsp;A PDF copy is also available for download from the report page.
              </p>

              <!-- divider -->
              <hr style=""border:none;border-top:1px solid {p.Divider};margin:26px 0;"" />

              <p style=""margin:0;font-family:Arial,Helvetica,sans-serif;font-size:12px;line-height:19px;color:{p.SubtleText};"">
                <strong style=""color:{p.Primary};"">About this score.</strong>
                The Financial Health Score blends Revenue Vitality, Cash Flow Health, Transaction Trustworthiness,
                Compliance Quotient, Business Stability and Debt Serviceability using data sourced from GST, ITR,
                Account Aggregator and EPFO records. Scores and bands are advisory and map to indicative loan products.
              </p>
            </td>
          </tr>

          <!-- footer -->
          <tr>
            <td align=""center"" style=""padding:22px 12px;font-family:Arial,Helvetica,sans-serif;font-size:11px;line-height:17px;color:{p.FooterText};"">
              This is an automated message from FinRiskLensAI &middot; please do not reply.<br />
              &copy; {DateTime.UtcNow.Year} FinRiskLensAI &middot; MSME Financial Health Score Platform
            </td>
          </tr>

        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
        }
    }
}
