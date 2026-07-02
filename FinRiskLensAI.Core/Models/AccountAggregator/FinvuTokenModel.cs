using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinRiskLensAI.Core.Models.AccountAggregator
{
    public class FinvuTokenModel
    {
        public class Header
        {
            public string rid { get; set; }
            public string ts { get; set; }
            public string channelId { get; set; }
        }

        public class Body
        {
            public string token { get; set; }
        }

        public class TokenResponse
        {
            public Header header { get; set; }
            public Body body { get; set; }
        }

    }
}
