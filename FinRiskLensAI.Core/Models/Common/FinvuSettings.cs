namespace FinRiskLensAI.Core.Models.Common
{
    /// <summary>
    /// Bound from the "Finvu" section of appsettings.json — the Account
    /// Aggregator (Finvu/FinFactor) API base URL and login credentials.
    /// </summary>
    public class FinvuSettings
    {
        public string FinvuApi { get; set; } = string.Empty;
        public string AaUserId { get; set; } = string.Empty;
        public string AaPassword { get; set; } = string.Empty;
        public string FinvuChannelId { get; set; } = string.Empty;

        /// <summary>
        /// Consent-callback URL Finvu redirects the user back to. Leave empty to
        /// derive it from the incoming request (works local + published); set it
        /// only to override the auto-detected host (e.g. behind a reverse proxy).
        /// </summary>
        public string CallbackUrl { get; set; } = string.Empty;
    }
}
