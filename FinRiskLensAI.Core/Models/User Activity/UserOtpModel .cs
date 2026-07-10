
using FinRiskLensAI.Core.Models.Common;
using FinRiskLensAI.Core.Models.Onboarding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinRiskLensAI.Core.Models.User_Activity
{
    public class UserOtpModel : AuditableEntity
    {
        public int UserOtpID { get; set; }

        public int? UserRegistrationID { get; set; }

        public int? MsmeEnquiryID { get; set; }

        public string? MobileNumber { get; set; }

        public string? Email { get; set; }

        public string? OTP { get; set; }

        public virtual UserRegistrationModel UserRegistration { get; set; }

        public virtual MsmeEnquiry? MsmeEnquiry { get; set; }
    }
}
