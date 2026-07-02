using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinRiskLensAI.Core.Models.AccountAggregator
{
    public class AccAggreTokenModel
    {
        public int TokenID { get; set; }
        public string rid { get; set; }
        public string ts { get; set; }
        public string token { get; set; }
        public System.DateTime UpdatedOn { get; set; }
    }
}
