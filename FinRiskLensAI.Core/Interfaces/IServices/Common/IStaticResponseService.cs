using FinRiskLensAI.Core.Models;
using FinRiskLensAI.Core.Models.Onboarding;
using FinRiskLensAI.Core.Models.Universal;
using System.Threading.Tasks;

namespace FinRiskLensAI.Core.Interfaces.IServices.Common
{
    /// <summary>
    /// Common access point for the cached API responses in m_StaticResponces.
    /// Use this everywhere a screen or analysis needs a stored payload for a
    /// Udyam number (the caller supplies the UAN, typically from session).
    /// </summary>
    public interface IStaticResponseService
    {
        /// <summary>The full row — every cached JSON response for the Udyam number.</summary>
        Task<ResultModel<StaticResponseModel>> FetchResponses(string udyamNumber);

        /// <summary>One cached response column's raw JSON, or null when absent.</summary>
        Task<string?> FetchResponse(string udyamNumber, StaticResponseType type);

        /// <summary>The Udyam response deserialized into the strongly-typed model, or null when absent/unparseable.</summary>
        Task<UdyamResponseModel?> FetchUdyamResponse(string udyamNumber);
    }
}
