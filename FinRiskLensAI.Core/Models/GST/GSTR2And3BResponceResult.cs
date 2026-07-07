using System;

namespace FinRiskLensAI.Core.Models.GST
{
    /// <summary>
    /// Read-only projection returned by IGSTR2And3BResponceService.GetResponces() —
    /// only the GSTR-2B/3B response payloads and lookup metadata, never the stored
    /// request payloads or audit actor.
    /// </summary>
    public class GSTR2And3BResponceResult
    {
        public string GSTR2BResponseData { get; set; }

        public string GSTR3BResponseData { get; set; }

        public string? GSTINNumber { get; set; }

        public string? FilingPeriod { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
