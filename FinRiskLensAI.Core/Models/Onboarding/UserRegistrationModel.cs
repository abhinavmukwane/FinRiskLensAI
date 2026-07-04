using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinRiskLensAI.Core.Models.Common;

namespace FinRiskLensAI.Core.Models.Onboarding
{
    public class UserRegistrationModel : AuditableEntity
    {
        public int UserRegistrationID { get; set; }

        public string? MobileNumber { get; set; }

        public string? Email { get; set; }

        public string? UdyamNumber { get; set; }

        public string? GstinNumber { get; set; }

        public string? PanNumber { get; set; }
        
        public int? MsmeEnquiryID { get; set; }

        public virtual MsmeEnquiry? MsmeEnquiry { get; set; }
    }
}
