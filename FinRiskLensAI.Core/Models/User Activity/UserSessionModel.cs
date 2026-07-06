namespace FinRiskLensAI.Core.Models.User_Activity
{
    /// <summary>
    /// Lightweight snapshot of the logged-in MSME user, sourced from
    /// t_UserRegistration at login time and kept in the HTTP session so any
    /// page can read the current user's identifiers without another DB hit.
    /// This is a plain DTO — it is NOT an EF entity and maps to no table.
    /// </summary>
    public class UserSessionModel
    {
        public int UserRegistrationID { get; set; }

        public int? MsmeEnquiryID { get; set; }

        public string? NameOfEnterprise { get; set; }

        public string? Email { get; set; }

        public string? MobileNumber { get; set; }

        public string? UdyamNumber { get; set; }

        public string? GstinNumber { get; set; }

        public string? PanNumber { get; set; }

        /// <summary>Client IP captured once at login.</summary>
        public string? ClientIP { get; set; }
    }
}
