using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FinRiskLensAI.Core.Interfaces.IServices.Common
{
    public interface IEmailService
    {
        /// <summary>
        /// Sends an arbitrary HTML email. Returns true when the SMTP server accepted the message.
        /// </summary>
        Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default);

        /// <summary>
        /// Sends the login/registration OTP email using the branded HTML template.
        /// <paramref name="theme"/> is the UI theme active when the OTP was requested
        /// ("theme1" = IDBI teal/orange, "theme2" = classic maroon) so the email matches it.
        /// </summary>
        Task<bool> SendLoginOtpAsync(string toEmail, string otpCode, string? recipientName = null, int expiryMinutes = 10, string? theme = null, CancellationToken ct = default);

        Task<bool> SendReportReadyEmailAsync(string toEmail,string? recipientName,string businessName,int financialHealthScore,string riskBand, string reportDate,
       string reportUrl, string metric1Label, string metric1Value, string metric2Label, string metric2Value, string metric3Label, string metric3Value, string? theme = null, CancellationToken ct = default);
    }
}
