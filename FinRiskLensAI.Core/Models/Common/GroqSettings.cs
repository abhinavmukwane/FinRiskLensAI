namespace FinRiskLensAI.Core.Models.Common
{
    /// <summary>
    /// Groq LLM API settings (appsettings.json "Groq" section), bound once at
    /// startup like SmtpSettings. Powers the universal in-app chatbot.
    /// </summary>
    public class GroqSettings
    {
        public string BaseUrl { get; set; } = "https://api.groq.com/openai/v1/chat/completions";
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "meta-llama/llama-4-scout-17b-16e-instruct";
        public double Temperature { get; set; } = 0.4;
        public int MaxTokens { get; set; } = 400;
    }
}
