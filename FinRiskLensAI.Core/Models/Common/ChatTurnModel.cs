namespace FinRiskLensAI.Core.Models.Common
{
    /// <summary>One prior exchange turn in a chatbot conversation.</summary>
    public class ChatTurnModel
    {
        /// <summary>"user" or "assistant" — anything else is dropped server-side.</summary>
        public string Role { get; set; } = "user";

        public string Content { get; set; } = string.Empty;
    }
}
