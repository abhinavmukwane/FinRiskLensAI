using System.Threading;
using System.Threading.Tasks;

namespace FinRiskLensAI.Core.Interfaces.IServices.Common
{
    /// <summary>
    /// The single service responsible for the Signzy IP risk-score lookup.
    /// Accepts an IP address and returns the raw JSON response. The production
    /// HTTP call is implemented; while the production Authorization key is
    /// unavailable it temporarily returns the stored response from
    /// m_StaticResponces.IPResponce (see SignzySettings.UseApi).
    /// </summary>
    public interface IIpRiskService
    {
        Task<string?> GetIpRiskScoreAsync(string ip, string udyamNumber, CancellationToken ct = default);
    }
}
