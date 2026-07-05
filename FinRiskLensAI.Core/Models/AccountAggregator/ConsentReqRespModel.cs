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
            public string encryptedRequest { get; set; }
            public string requestDate { get; set; }
            public string encryptedFiuId { get; set; }
            public string ConsentHandle { get; set; }
            public string url { get; set; }
        }
    }
}
