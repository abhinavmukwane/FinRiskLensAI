namespace FinRiskLensAI.ML.MachineLearning
{
    /// <summary>
    /// Generates synthetic MSME training rows spanning the borrower spectrum, per the
    /// scoring doc's synthetic-data note (IDBI sandbox data lands July 22 — swap then).
    /// The label is a domain-informed score formula plus noise, so LightGBM learns a
    /// credible non-linear mapping the calibration layer can blend with the rules engine.
    /// </summary>
    public class SyntheticProfileGenerator
    {
        public IEnumerable<ScoreFeatureVector> Generate(int count, int seed = 42)
        {
            var random = new Random(seed);
            for (int i = 0; i < count; i++)
            {
                var f = new float[ScoreFeatureVector.Count];
                f[0] = (float)(random.NextDouble() * 2 - 1);       // GST turnover trend
                f[1] = (float)random.NextDouble();                 // GST filing regularity
                f[2] = (float)random.NextDouble();                 // B2B share
                f[3] = (float)(random.NextDouble() * 2 - 1);       // ITR income trend
                f[4] = (float)random.NextDouble();                 // ITR timeliness
                f[5] = (float)(random.NextDouble() * 2);           // inflow volatility (CV)
                f[6] = (float)random.NextDouble();                 // days cash on hand (normalized)
                f[7] = (float)random.NextDouble();                 // bounce rate (normalized)
                f[8] = (float)random.NextDouble();                 // UPI share
                f[9] = (float)random.NextDouble();                 // repeat payer ratio
                f[10] = (float)random.NextDouble();                // vintage (normalized)
                f[11] = (float)random.NextDouble();                // EMI-to-inflow

                yield return new ScoreFeatureVector { Features = f, Label = Label(f, random) };
            }
        }

        /// <summary>Domain formula ≈ the six-dimension weighting, on a 0-1000 scale, with noise.</summary>
        private static float Label(float[] f, Random random)
        {
            double revenue = 0.5 + 0.35 * f[0] + 0.15 * f[3];                // trends
            double cashflow = 0.55 + 0.25 * f[6] - 0.20 * Math.Min(1, f[5]) - 0.25 * f[7];
            double txnTrust = 0.30 + 0.35 * f[8] + 0.35 * f[9];
            double compliance = 0.60 * f[1] + 0.40 * f[4];
            double stability = 0.30 + 0.55 * f[10] + 0.15 * f[2];
            double debt = 0.85 - 0.70 * f[11];

            var score = 1000 * (0.25 * revenue + 0.20 * cashflow + 0.15 * txnTrust
                              + 0.15 * compliance + 0.15 * stability + 0.10 * debt);
            score += random.NextDouble() * 60 - 30;                          // noise
            return (float)Math.Clamp(score, 0, 1000);
        }
    }
}
