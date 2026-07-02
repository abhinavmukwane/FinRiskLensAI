using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinRiskLensAI.Core.Models.AccountAggregator
{
    public class ConsentStatusRespModel
    {
        public class ConsentStatusModel
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
            public string consentStatus { get; set; }
            public string consentId { get; set; }
        }
    }
}
