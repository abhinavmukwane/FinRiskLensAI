using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinRiskLensAI.Core.Models.AccountAggregator
{
    public class AAConsentReqModel
    {
        public int ConsentReqId { get; set; }
        public string transactionId { get; set; }
        public string rid { get; set; }
        public string ts { get; set; }
        public string channelId { get; set; }
        public string encryptedRequest { get; set; }
        public string requestDate { get; set; }
        public string encryptedFiuId { get; set; }
        public string consentHandle { get; set; }
        public string url { get; set; }
        public System.DateTime CreatedOn { get; set; }
    }
}
