using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinRiskLensAI.Core.Models.Common
{
    /// <summary>
    /// Audit timestamps/actors only. Each entity declares its own int identity
    /// primary key named &lt;EntityName&gt;ID (e.g. MsmeEnquiryID) — no shared Id base.
    /// </summary>
    public abstract class AuditableEntity
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
