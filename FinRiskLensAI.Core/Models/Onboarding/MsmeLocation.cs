using FinRiskLensAI.Core.Models.Common;

namespace FinRiskLensAI.Core.Models.Onboarding
{
    /// <summary>One row per entry in the Udyam response's location_of_plant_details array.</summary>
    public class MsmeLocation : AuditableEntity
    {
        public Guid MsmeEnquiryId { get; set; }
        public MsmeEnquiry? MsmeEnquiry { get; set; }

        public string? UnitName { get; set; }
        public string? Line1 { get; set; }
        public string? Building { get; set; }
        public string? Village { get; set; }
        public string? Street { get; set; }
        public string? Road { get; set; }
        public string? City { get; set; }
        public string? Pin { get; set; }
        public string? State { get; set; }
        public string? District { get; set; }
    }
}
