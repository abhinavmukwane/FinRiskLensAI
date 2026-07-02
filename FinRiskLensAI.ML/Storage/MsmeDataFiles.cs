namespace FinRiskLensAI.ML.Storage
{
    /// <summary>
    /// Naming convention for files inside an MSME's blob folder. Deterministic names
    /// let the engine route each file to the right extractor without guessing.
    /// </summary>
    public static class MsmeDataFiles
    {
        public const string Manifest = "_manifest.json";
        public const string Result = "result.json";

        public const string Udyam = "udyam.json";
        public const string Itr = "itr.json";
        public const string Aa = "aa.json";
        public const string GstTaxpayer = "gst_taxpayer.json";
        public const string Epfo = "epfo.json";

        // Monthly GST files carry a MMyyyy period suffix, e.g. gstr3b_012026.json
        public const string Gstr3bPrefix = "gstr3b_";
        public const string Gstr1SummaryPrefix = "gstr1_summary_";
        public const string Gstr1B2bPrefix = "gstr1_b2b_";

        private static readonly string[] KnownFixed = { Udyam, Itr, Aa, GstTaxpayer, Epfo, Manifest, Result };
        private static readonly string[] KnownPrefixes = { Gstr3bPrefix, Gstr1SummaryPrefix, Gstr1B2bPrefix };

        public static bool IsKnown(string fileName)
            => KnownFixed.Contains(fileName, StringComparer.OrdinalIgnoreCase)
            || KnownPrefixes.Any(p => fileName.StartsWith(p, StringComparison.OrdinalIgnoreCase))
            && fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase);
    }
}
