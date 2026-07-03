using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinRiskLensAI.Core.Models.Onboarding
{
    public class UdyamDetailsModel
    {
        public string UdyamNumber { get; set; }
        public string EnterpriseName { get; set; }
        public string OrganizationType { get; set; }
        public string EnterpriseType { get; set; }
        public DateTime? DateOfIncorporation { get; set; }

        // Contact
        public string Email { get; set; }
        public string Mobile { get; set; }

        // Location
        public string Address { get; set; }
        public string Village { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string State { get; set; }
        public string PinCode { get; set; }

        // NIC Details
        public string NicCode { get; set; }
        public string NicDescription { get; set; }
    }
}
