using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FinRiskLensAI.Services.Templates
{
    /// <summary>
    /// Branded HTML template for the login/registration OTP email.
    /// Table-based layout with inline styles so it renders correctly in
    /// Outlook / Gmail / mobile clients; palette matches the dashboard
    /// (maroon #57001d / rose #974354).
    /// </summary>
    public static class LoginOtpEmailTemplate
    {
        public static string Subject(string otpCode) => $"{otpCode} is your FinRiskLensAI verification code";

        public static string Build(string otpCode, string? recipientName, int expiryMinutes)
        {
            var greeting = string.IsNullOrWhiteSpace(recipientName)
                ? "Hello,"
                : $"Hello {WebUtility.HtmlEncode(recipientName.Trim())},";
            var otp = WebUtility.HtmlEncode(otpCode);

            return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
<meta charset=""utf-8"" />
<meta name=""viewport"" content=""width=device-width, initial-scale=1"" />
<title>FinRiskLensAI verification code</title>
</head>
<body style=""margin:0;padding:0;background-color:#f6f1f3;"">
  <!-- preheader (hidden preview text) -->
  <div style=""display:none;max-height:0;overflow:hidden;mso-hide:all;"">
    Your FinRiskLensAI one-time password is {otp}. It expires in {expiryMinutes} minutes.
  </div>

  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#f6f1f3;padding:32px 12px;"">
    <tr>
      <td align=""center"">
        <table role=""presentation"" width=""560"" cellpadding=""0"" cellspacing=""0"" style=""width:560px;max-width:100%;"">

          <!-- header band -->
          <tr>
            <td style=""background-color:#57001d;border-radius:16px 16px 0 0;padding:26px 36px;"">
              <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"">
                <tr>
                  <td style=""font-family:Arial,Helvetica,sans-serif;font-size:20px;font-weight:bold;color:#ffffff;letter-spacing:.3px;"">
                    FinRiskLens<span style=""color:#e8b7c3;"">AI</span>
                  </td>
                  <td align=""right"" style=""font-family:Arial,Helvetica,sans-serif;font-size:11px;color:#e8b7c3;text-transform:uppercase;letter-spacing:.12em;"">
                    MSME Financial Health
                  </td>
                </tr>
              </table>
            </td>
          </tr>

          <!-- body card -->
          <tr>
            <td style=""background-color:#ffffff;padding:36px;border:1px solid #ead9df;border-top:0;border-radius:0 0 16px 16px;"">

              <p style=""margin:0 0 6px;font-family:Arial,Helvetica,sans-serif;font-size:18px;font-weight:bold;color:#57001d;"">
                Your one-time password
              </p>
              <p style=""margin:0 0 22px;font-family:Arial,Helvetica,sans-serif;font-size:14px;line-height:22px;color:#5c4a51;"">
                {greeting}<br />
                Use the code below to sign in to your FinRiskLensAI account. Enter it on the
                verification screen to continue.
              </p>

              <!-- OTP box -->
              <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"">
                <tr>
                  <td align=""center"" style=""background-color:#faf5f7;border:1px dashed #d8c3ca;border-radius:12px;padding:22px 12px;"">
                    <span style=""font-family:'Courier New',Courier,monospace;font-size:34px;font-weight:bold;letter-spacing:12px;color:#57001d;"">{otp}</span>
                  </td>
                </tr>
              </table>

              <p style=""margin:18px 0 0;font-family:Arial,Helvetica,sans-serif;font-size:13px;line-height:20px;color:#974354;"">
                &#9200;&nbsp;This code expires in <strong>{expiryMinutes} minutes</strong> and can be used only once.
              </p>

              <!-- divider -->
              <hr style=""border:none;border-top:1px solid #f0e2e7;margin:26px 0;"" />

              <p style=""margin:0;font-family:Arial,Helvetica,sans-serif;font-size:12px;line-height:19px;color:#8a7580;"">
                <strong style=""color:#57001d;"">Didn't request this?</strong>
                You can safely ignore this email — no changes will be made to your account.
                FinRiskLensAI will never call or message you asking for this code. Do not share
                it with anyone, including bank or support staff.
              </p>
            </td>
          </tr>

          <!-- footer -->
          <tr>
            <td align=""center"" style=""padding:22px 12px;font-family:Arial,Helvetica,sans-serif;font-size:11px;line-height:17px;color:#a08a92;"">
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
