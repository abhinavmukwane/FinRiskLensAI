using FinRiskLensAI.Core.Models.Common;

namespace FinRiskLensAI.Core.Models.Admin
{
    public class AdmLogin : AuditableEntity
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Admin";
        public bool IsActive { get; set; } = true;
        public DateTime? LastLoginAt { get; set; }
    }
}
