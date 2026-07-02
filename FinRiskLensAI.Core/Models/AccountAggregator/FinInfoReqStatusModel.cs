using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinRiskLensAI.Core.Models.AccountAggregator
{
    public class FinInfoReqStatusModel
    {
        public class FinInfoRespStatusModel
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
            public string fiRequestStatus { get; set; }
            public List<Account> accounts { get; set; }
        }

        public class Account
        {
            public string linkRefNumber { get; set; }
            public string status { get; set; }
        }
    }
}
