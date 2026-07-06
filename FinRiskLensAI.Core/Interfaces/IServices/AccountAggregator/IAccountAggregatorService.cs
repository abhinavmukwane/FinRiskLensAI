using FinRiskLensAI.Core.Models.AccountAggregator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FinRiskLensAI.Core.Models.AccountAggregator.ConsentStatusRespModel;
using static FinRiskLensAI.Core.Models.AccountAggregator.FinInfoReqRespModel;
using static FinRiskLensAI.Core.Models.AccountAggregator.FinInfoReqStatusModel;

namespace FinRiskLensAI.Core.Interfaces.IServices.AccountAggregator
{
    public interface IAccountAggregatorService
    {
        Task GenerateFinvuToken();
        Task<string> CreateConsentRequest(string transactionId, string AAID);
        Task<AAConsentReqModel> GetAAConsentData(string transactionId);
        Task<ConsentDetailsById> CheckConsentStatus(string consentHandle, string AAID);
        Task<FinInfoRespModel> FinancialInfoRequest(string consentHandle, string AAID, string consentId, string dateFrom, string dateTo);
        Task<FinInfoRespStatusModel> FinancialInfoReqStatus(string consentHandle, string AAID, string consentId, string sessionId);
        Task<string> FinancialInfoFetch(string AAID, string consentId, string sessionId);

        /// <summary>
        /// Full FI pipeline for an ACTIVE consent: FI request → status → fetch,
        /// then stores the fetched bank data as aa.json in the MSME's blob folder
        /// (overwrites any existing file). Returns true only if data was stored.
        /// </summary>
        Task<bool> FetchAndStoreFinancialData(string trnxid, ConsentDetailsById consentDetails);
    }
}
