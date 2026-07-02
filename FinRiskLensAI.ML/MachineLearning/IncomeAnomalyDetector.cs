using FinRiskLensAI.ML.Models;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace FinRiskLensAI.ML.MachineLearning
{
    /// <summary>
    /// Cross-source income validation (Doc/03_SCORING_ENGINE.md "Fraud / anomaly signal"):
    /// RandomizedPca anomaly detection over the three independent income reads —
    /// GST-declared turnover, ITR-declared income, actual bank credits — plus their
    /// pairwise ratios. Trained once on synthetic consistent profiles.
    /// </summary>
    public class IncomeAnomalyDetector
    {
        private const int FeatureCount = 6;
        private readonly Lazy<(MLContext Ml, ITransformer Model)> _trained;

        public IncomeAnomalyDetector() => _trained = new(Train);

        private class Row
        {
            [VectorType(FeatureCount)]
            public float[] Features { get; set; } = new float[FeatureCount];
        }

        private class Prediction
        {
            public bool PredictedLabel { get; set; }
            public float Score { get; set; }
        }

        public AnomalyCheckResult Check(MsmeFeatureSet features)
        {
            var result = new AnomalyCheckResult
            {
                GstMonthlyTurnover = features.GstMonthlyAvgTurnover,
                ItrMonthlyIncome = features.ItrMonthlyAvgIncome,
                BankMonthlyCredits = features.AaMonthlyAvgBankCredit
            };

            // Need at least two of the three signals to say anything meaningful.
            var available = new[] { features.GstMonthlyAvgTurnover, features.ItrMonthlyAvgIncome, features.AaMonthlyAvgBankCredit }
                .Count(v => v > 0);
            if (available < 2)
            {
                result.Note = "Cross-source income check skipped — fewer than two income signals available.";
                return result;
            }

            var (ml, model) = _trained.Value;
            var engine = ml.Model.CreatePredictionEngine<Row, Prediction>(model);
            var prediction = engine.Predict(new Row
            {
                Features = BuildVector(features.GstMonthlyAvgTurnover, features.ItrMonthlyAvgIncome, features.AaMonthlyAvgBankCredit)
            });

            result.AnomalyScore = prediction.Score;
            result.IsAnomalous = prediction.PredictedLabel;
            if (result.IsAnomalous)
                result.Note = "Large unexplained gap between GST turnover, ITR income and bank credits — "
                            + "data-integrity signal for officer review, not a creditworthiness deduction.";
            return result;
        }

        /// <summary>Log-scaled levels + pairwise ratios, so scale drops out and disagreement dominates.</summary>
        private static float[] BuildVector(double gst, double itr, double bank)
        {
            static float Log(double v) => (float)Math.Log10(Math.Max(1, v));
            static float Ratio(double a, double b) => (float)Math.Clamp((a + 1) / (b + 1), 0.01, 100);

            return new[]
            {
                Log(gst), Log(itr), Log(bank),
                Ratio(gst, itr), Ratio(gst, bank), Ratio(itr, bank)
            };
        }

        private static (MLContext, ITransformer) Train()
        {
            var ml = new MLContext(seed: 42);
            var random = new Random(42);

            // Consistent profiles: the three signals agree within business-normal spread
            // (bank credits usually a bit above GST turnover; ITR income below turnover).
            var rows = new List<Row>();
            for (int i = 0; i < 600; i++)
            {
                var scale = Math.Pow(10, 4 + random.NextDouble() * 3);      // 10k .. 10Cr monthly
                var gst = scale * (0.8 + random.NextDouble() * 0.4);
                var itr = gst * (0.05 + random.NextDouble() * 0.5);         // income share of turnover
                var bank = gst * (0.7 + random.NextDouble() * 0.8);
                rows.Add(new Row { Features = BuildVector(gst, itr, bank) });
            }

            var data = ml.Data.LoadFromEnumerable(rows);
            var pipeline = ml.Transforms.NormalizeMinMax(nameof(Row.Features))
                .Append(ml.AnomalyDetection.Trainers.RandomizedPca(
                    featureColumnName: nameof(Row.Features), rank: 3));

            return (ml, pipeline.Fit(data));
        }
    }
}
