using FinRiskLensAI.Core.Models.Scoring;

namespace FinRiskLensAI.Core.Interfaces
{
    /// <summary>Raw source payloads for one MSME, exactly as returned by the upstream APIs.</summary>
    public class RiskAnalysisRequest
    {
        public string? UdyamJson { get; set; }
        public string? ItrJson { get; set; }
        public string? AaJson { get; set; }
        public string? GstTaxpayerJson { get; set; }
        public List<string> Gstr3bJsons { get; set; } = new();          // one per month, up to 12
        public List<string> Gstr1SummaryJsons { get; set; } = new();    // one per month, up to 12
        public List<string> Gstr1B2bJsons { get; set; } = new();        // B2B / e-invoice payloads
        public List<string> Gstr1CdnrJsons { get; set; } = new();       // credit/debit notes
        public List<string> Gstr1HsnJsons { get; set; } = new();        // HSN summaries
        public List<string> Gstr2aB2bJsons { get; set; } = new();       // inward (purchase) invoices
        // EPFO payload slot — source arrives later, wired but unused for now
        public string? EpfoJson { get; set; }
    }

    public interface IRiskScoringService
    {
        RiskAnalysisResult Analyze(RiskAnalysisRequest request);
    }
}
