using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinRiskLensAI.Core.Models.AccountAggregator
{
    public class ConsentReqRespModel
    {
        public class ConsentRespModel
        {
            public Header header { get; set; }
            public Body body { get; set; }
        }

        public class Header
        {
            public string rid { get; set; }
            public string ts { get; set; }
            public string channelId { get; set; }
        }

        public class Body
        {
            public string custId { get; set; }
            public string consentHandle { get; set; }
            public string consentPurpose { get; set; }
            public string consentDescription { get; set; }
            public string requestDate { get; set; }
            public string consentStatus { get; set; }
            public string requestSessionId { get; set; }
            public string requestConsentId { get; set; }
            public string dateTimeRangeFrom { get; set; }
            public string dateTimeRangeTo { get; set; }
            public string aaId { get; set; }
            public string fetchType { get; set; }
        }
    }
}
