using FinRiskLensAI.ML.Models;
using FinRiskLensAI.ML.Storage;

namespace FinRiskLensAI.ML.Services
{
    public class MsmeDataStatusReport
    {
        public string Uan { get; set; } = string.Empty;
        public MsmeDataManifest? Manifest { get; set; }
        public List<string> FilesPresent { get; set; } = new();
        public List<string> FilesMissing { get; set; } = new();
        public bool ReadyForAnalysis { get; set; }
        public bool ResultAvailable { get; set; }
    }

    public interface IBlobAnalysisService
    {
        Task SaveManifestAsync(string uan, MsmeDataManifest.ExpectedFiles expected, CancellationToken ct = default);
        Task<MsmeDataStatusReport> GetStatusAsync(string uan, CancellationToken ct = default);
        /// <summary>Runs the full AI analysis from the MSME's blob folder once all expected files are present.</summary>
        Task<RiskAnalysisResult> AnalyzeAsync(string uan, bool force = false, CancellationToken ct = default);
        Task<RiskAnalysisResult?> GetResultAsync(string uan, CancellationToken ct = default);
    }
}
