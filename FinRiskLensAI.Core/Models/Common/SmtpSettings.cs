using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinRiskLensAI.Core.Models.Common
{
    /// <summary>
    /// Bound from the "Smtp" section of appsettings.json (see Program.cs).
    /// </summary>
    public class SmtpSettings
    {
        public string Host { get; set; } = string.Empty;

        public int Port { get; set; } = 25;

        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string FromEmail { get; set; } = string.Empty;

        public string FromName { get; set; } = "FinRiskLensAI";

        /// <summary>
        /// In System.Net.Mail this means STARTTLS on the given port
        /// (the "TLS" option in mail clients), not implicit SSL/465.
        /// </summary>
        public bool EnableSsl { get; set; } = false;

        /// <summary>
        /// Accept a certificate whose only validation failure is a host-name
        /// mismatch (shared hosting: connecting to finrisklensai.com but the
        /// server presents *.webhostbox.net). The certificate chain must still
        /// be valid and trusted — expired/self-signed certs are still rejected.
        /// </summary>
        public bool AllowCertificateNameMismatch { get; set; } = false;

        public int TimeoutSeconds { get; set; } = 30;
    }
}
