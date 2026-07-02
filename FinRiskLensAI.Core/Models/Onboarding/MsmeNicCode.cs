using FinRiskLensAI.Core.Models.Common;

namespace FinRiskLensAI.Core.Models.Onboarding
{
    /// <summary>One row per entry in the Udyam response's nic_code array.</summary>
    public class MsmeNicCode : AuditableEntity
    {
        public Guid MsmeEnquiryId { get; set; }
        public MsmeEnquiry? MsmeEnquiry { get; set; }

        public string? Nic2Digit { get; set; }
        public string? Nic4Digit { get; set; }
        public string? Nic5Digit { get; set; }
        public string? ActivityType { get; set; }
        public DateTime? AddedOn { get; set; }
    }
}
