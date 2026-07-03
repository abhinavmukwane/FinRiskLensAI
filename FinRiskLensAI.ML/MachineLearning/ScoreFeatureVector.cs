using FinRiskLensAI.ML.Models;
using Microsoft.ML.Data;

namespace FinRiskLensAI.ML.MachineLearning
{
    /// <summary>
    /// The flat numeric row fed to the LightGBM calibration model. Field order defines
    /// the feature index used by permutation importance — keep <see cref="FeatureNames"/> in sync.
    /// </summary>
    public class ScoreFeatureVector
    {
        public const int Count = 15;

        public static readonly string[] FeatureNames =
        {
            "GstTurnoverTrend", "GstFilingRegularity", "GstB2bShare",
            "ItrIncomeTrend", "ItrFilingTimeliness",
            "InflowVolatility", "DaysCashOnHand", "BounceRate",
            "UpiShare", "RepeatPayerRatio",
            "BusinessVintage", "EmiToInflowRatio",
            "CreditNoteRatio", "PurchaseCoverage", "HsnDiversity"
        };

        [VectorType(Count)]
        public float[] Features { get; set; } = new float[Count];

        public float Label { get; set; }

        public static ScoreFeatureVector FromFeatureSet(MsmeFeatureSet f) => new()
        {
            Features = new[]
            {
                // Missing sources feed neutral values, not zeros — absence of a
                // source must not read as bad behavior (same rule as the dimensions)
                (float)f.GstTurnoverTrendSlope,                                   // -1..1
                f.HasGst ? (float)f.GstFilingRegularity : 0.5f,                   // 0..1
                (float)f.GstB2bShare,                                             // 0..1
                (float)f.ItrIncomeTrendSlope,                                     // -1..1
                f.HasItr ? (float)f.ItrFilingTimeliness : 0.5f,                   // 0..1
                (float)Math.Clamp(f.AaInflowVolatility, 0, 2),                    // 0..2
                (float)Math.Clamp(f.AaDaysCashOnHand / 180.0, 0, 1),              // 0..1 (capped at ~6 months)
                (float)Math.Clamp(f.AaBounceCount / 6.0, 0, 1),                   // 0..1
                (float)f.AaUpiTxnShare,                                           // 0..1
                (float)f.AaRepeatPayerRatio,                                      // 0..1
                (float)Math.Clamp(f.BusinessVintageMonths / 120.0, 0, 1),         // 0..1 (capped at 10y)
                (float)f.AaEmiToInflowRatio,                                      // 0..1
                (float)f.GstCreditNoteRatio,                                      // 0..1 (share of turnover reversed)
                (float)Math.Clamp(f.GstPurchaseToSalesRatio / 1.5, 0, 1),         // 0..1 (0.5 ≈ healthy 0.75 ratio)
                (float)Math.Clamp(f.GstHsnProductCount / 15.0, 0, 1)              // 0..1 (15+ product lines = max)
            }
        };
    }
}
