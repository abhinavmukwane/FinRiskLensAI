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
    }
}
