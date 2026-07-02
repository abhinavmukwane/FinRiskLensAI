using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinRiskLensAI.Core.Models.AccountAggregator
{
    public class FinInfoReqRespModel
    {
        public class FinInfoRespModel
        {
            public Header header { get; set; }
            public Body body { get; set; }
        }

        public class Header
        {
            public string rid { get; set; }
            public DateTime ts { get; set; }
            public string channelId { get; set; }
        }

        public class Body
        {
            public string ver { get; set; }
            public string timestamp { get; set; }
            public string txnid { get; set; }
            public string consentId { get; set; }
            public string sessionId { get; set; }
            public string consentHandleId { get; set; }
        }
    }
}
