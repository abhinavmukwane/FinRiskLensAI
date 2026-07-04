namespace FinRiskLensAI.ML.Storage
{
    public enum MsmeDataStatus { Collecting, Ready, Processing, Completed, Failed }

    /// <summary>
    /// The control file (_manifest.json) in each MSME folder. Declares which source
    /// files the folder is expected to contain so the engine knows when collection
    /// is complete — it never guesses from what happens to be present.
    /// </summary>
    public class MsmeDataManifest
    {
        public string MsmeRef { get; set; } = string.Empty;   // Udyam number
        public ExpectedFiles Expected { get; set; } = new();
        public MsmeDataStatus Status { get; set; } = MsmeDataStatus.Collecting;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public string? Error { get; set; }

        public class ExpectedFiles
        {
            public bool Udyam { get; set; } = true;
            public bool Itr { get; set; } = true;
            public bool Aa { get; set; } = true;
            public bool GstTaxpayer { get; set; }
            /// <summary>Expected number of monthly GSTR-3B files (0 = GST not expected).</summary>
            public int GstMonths { get; set; }
            public bool Epfo { get; set; }
        }
    }
}
