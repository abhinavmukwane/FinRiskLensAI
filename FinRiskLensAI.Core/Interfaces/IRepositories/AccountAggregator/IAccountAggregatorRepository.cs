using FinRiskLensAI.Core.Models;
using FinRiskLensAI.Core.Models.AccountAggregator;

namespace FinRiskLensAI.Core.Interfaces.IRepositories.AccountAggregator
{
    public interface IAccountAggregatorRepository
    {
        Task<ResultModel<AccAggreTokenModel>> AddAAToken(AccAggreTokenModel entity);
        Task<ResultModel<AccAggreTokenModel>> UpdateAAToken(AccAggreTokenModel entity);
        AccAggreTokenModel GetAAToken();
        bool CheckAATokenValidity();
        Task<ResultModel<AAConsentReqModel>> AddAAConsentRequest(AAConsentReqModel entity);
        Task<AAConsentReqModel> GetAAConsentRequest(string transactionId);
    }
}
