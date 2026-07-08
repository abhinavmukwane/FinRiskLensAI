using System.Collections.Generic;

namespace FinRiskLensAI.Models
{
    public class McaDetailsViewModel
    {
        public bool HasData { get; set; }
        public string? LoadError { get; set; }

        public string CompanyName { get; set; } = "";
        public string Cin { get; set; } = "";
        public string Status { get; set; } = "";
        public string CompanyClass { get; set; } = "";
        public string Category { get; set; } = "";
        public string RocCode { get; set; } = "";
        public string RegisteredAddress { get; set; } = "";
        public string Listed { get; set; } = "";
        public string AuthorizedCapital { get; set; } = "0";
        public string PaidUpCapital { get; set; } = "0";
        public string IncorporationDate { get; set; } = "";

        public List<McaDirector> Directors { get; set; } = new();
        public List<McaCharge> Charges { get; set; } = new();

        public long TotalOpenChargeAmount { get; set; }
        public int OpenChargeCount { get; set; }
    }

    public class McaDirector
    {
        public string Din { get; set; } = "";
        public string Name { get; set; } = "";
        public string StartDate { get; set; } = "";
    }

    public class McaCharge
    {
        public string Asset { get; set; } = "";
        public long Amount { get; set; }
        public string Created { get; set; } = "";
        public string Modified { get; set; } = "";
        public string Status { get; set; } = "";
    }
}
