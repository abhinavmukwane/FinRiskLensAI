using FinRiskLensAI.Core.Models.Common;

namespace FinRiskLensAI.Core.Models.Onboarding
{
    /// <summary>
    /// Cache of the Udyam Aadhaar API response. First lookup for a UAN hits the API
    /// and saves here (full raw JSON in <see cref="Payload"/>); subsequent lookups
    /// for the same UAN are served from this table without calling the API again.
    /// </summary>
    public class MsmeEnquiry : AuditableEntity
    {
        public int MsmeEnquiryID { get; set; }

        public string ClientId { get; set; } = string.Empty;

        /// <summary>Udyam Registration Number, e.g. UDYAM-MH-20-0033382 — the cache lookup key.</summary>
        public string Uan { get; set; } = string.Empty;

        public string? CertificateUrl { get; set; }

        // ── main_details (enterprise_type_list intentionally excluded — lives only in Payload)
        public string NameOfEnterprise { get; set; } = string.Empty;
        public string? MajorActivity { get; set; }
        public string? SocialCategory { get; set; }
        public DateTime? DateOfCommencement { get; set; }
        public string? DicName { get; set; }
        public string? State { get; set; }
        public DateTime? AppliedDate { get; set; }
        public string? Flat { get; set; }
        public string? NameOfBuilding { get; set; }
        public string? Road { get; set; }
        public string? Village { get; set; }
        public string? Block { get; set; }
        public string? City { get; set; }
        public string? Pin { get; set; }
        public string? MobileNumber { get; set; }
        public string? Email { get; set; }
        public string? OrganizationType { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfIncorporation { get; set; }
        public string? MsmeDfo { get; set; }
        public DateTime? RegistrationDate { get; set; }

        /// <summary>The complete Udyam API response JSON, stored as-is.</summary>
        public string Payload { get; set; } = string.Empty;

        public ICollection<MsmeLocation> Locations { get; set; } = new List<MsmeLocation>();
        public ICollection<MsmeNicCode> NicCodes { get; set; } = new List<MsmeNicCode>();
    }
}
