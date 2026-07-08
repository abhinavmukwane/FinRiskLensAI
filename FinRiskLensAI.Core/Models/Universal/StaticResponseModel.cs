using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinRiskLensAI.Core.Models.Universal
{
    public class StaticResponseModel
    {
        public int StaticResponcesID { get; set; }

        public string? UdyamNumber { get; set; }

        public string? UdyamNumberResponce { get; set; }

        public string? PanNumberResponce { get; set; }

        public string? GSTNumberResponce { get; set; }

        public string? GST2BResponce { get; set; }

        public string? GST3BResponce { get; set; }

        public string? ITRNumberResponce { get; set; }

        public string? IPResponce { get; set; }

        public string? MCAResponce { get; set; }

        public string? DINResponce { get; set; }
    }
}
