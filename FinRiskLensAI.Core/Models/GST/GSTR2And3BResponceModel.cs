using System;

namespace FinRiskLensAI.Core.Models.GST
{
    /// <summary>
    /// Stored GST GSTR-2B / GSTR-3B request+response payloads for an MSME.
    /// Plain entity (not AuditableEntity) — it keeps its own <see cref="CreatedDate"/>
    /// and integer <see cref="CreatedBy"/>, mirroring the AAConsentReqModel convention.
    /// C# property names cannot start with a digit, so the four data properties are
    /// prefixed with "GSTR" and mapped to the 2B*/3B* SQL columns in configuration.
    /// </summary>
    public class GSTR2And3BResponceModel
    {
        public int Id { get; set; }

        public string UdyamNumber { get; set; }

        public string? GSTINNumber { get; set; }

        public string? FilingPeriod { get; set; }

        public string GSTR2BRequestData { get; set; }   // → 2BRequestData

        public string GSTR3BRequestData { get; set; }   // → 3BRequestData

        public string GSTR2BResponseData { get; set; }  // → 2BResponseData

        public string GSTR3BResponseData { get; set; }  // → 3BResponseData

        public DateTime CreatedDate { get; set; }

        public int? CreatedBy { get; set; }
    }
}
