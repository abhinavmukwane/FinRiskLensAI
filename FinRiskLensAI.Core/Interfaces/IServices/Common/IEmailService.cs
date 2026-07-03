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
        /// </summary>
        Task<bool> SendLoginOtpAsync(string toEmail, string otpCode, string? recipientName = null, int expiryMinutes = 10, CancellationToken ct = default);
    }
}
