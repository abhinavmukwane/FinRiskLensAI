using FinRiskLensAI.Core.Models.Common;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FinRiskLensAI.Core.Interfaces.IServices.Common
{
    /// <summary>
    /// Universal page-aware chatbot. Any screen can ask a question, passing the
    /// screen's full data as <c>screenContext</c> (the controller caches it per
    /// page so it only has to travel from the browser once, on the first
    /// question) plus the rolling conversation history.
    /// </summary>
    public interface IChatbotService
    {
        Task<string> AskAsync(string? screenContext,
                              IReadOnlyList<ChatTurnModel>? history,
                              string question,
                              CancellationToken ct = default);
    }
}
