using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinRiskLensAI.Core.Models.Onboarding
{
    public class SaveMsmeResultModel
    {
        public bool Status { get; set; }
        public bool AlreadyRegistered { get; set; }
        public int MsmeEnquiryID { get; set; }
        public string Message { get; set; }
    }
}
