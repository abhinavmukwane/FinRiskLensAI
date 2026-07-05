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

        /// <summary>
        /// The single common accessor for any cached response column (Udyam, IP,
        /// PAN, GST, ITR…) — returns that column's raw JSON, or null when absent.
        /// </summary>
        Task<string?> GetStaticCommonResponce(string udyamNumber, StaticResponseType type);

        /// <summary>
        /// Same as <see cref="GetStaticCommonResponce(string, StaticResponseType)"/>
        /// but deserialized into <typeparamref name="T"/>; null when absent/unparseable.
        /// </summary>
        Task<T?> GetStaticCommonResponce<T>(string udyamNumber, StaticResponseType type) where T : class;
    }
}
