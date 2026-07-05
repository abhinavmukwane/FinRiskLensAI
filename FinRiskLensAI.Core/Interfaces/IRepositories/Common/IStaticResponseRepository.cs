using FinRiskLensAI.Core.Models;
using FinRiskLensAI.Core.Models.Universal;
using System.Threading.Tasks;

namespace FinRiskLensAI.Core.Interfaces.IRepositories.Common
{
    public interface IStaticResponseRepository
    {
        /// <summary>Fetches the full m_StaticResponces row (all cached API responses) for a Udyam number.</summary>
        Task<ResultModel<StaticResponseModel>> FetchByUdyamNumber(string udyamNumber);
    }
}
