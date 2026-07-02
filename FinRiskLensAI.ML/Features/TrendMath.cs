namespace FinRiskLensAI.ML.Features
{
    /// <summary>Small shared numeric helpers for feature engineering.</summary>
    public static class TrendMath
    {
        /// <summary>
        /// Least-squares slope of a series, normalized by its mean and clamped to -1..1,
        /// so "revenue doubled over the window" ≈ +1 and "halved" ≈ -1.
        /// </summary>
        public static double NormalizedSlope(IReadOnlyList<double> series)
        {
            if (series.Count < 2) return 0;
            var mean = series.Average();
            if (mean <= 0) return 0;

            double n = series.Count;
            double sumX = 0, sumY = 0, sumXy = 0, sumXx = 0;
            for (int i = 0; i < series.Count; i++)
            {
                sumX += i; sumY += series[i];
                sumXy += i * series[i]; sumXx += (double)i * i;
            }
            var slope = (n * sumXy - sumX * sumY) / Math.Max(1e-9, n * sumXx - sumX * sumX);

            // slope per period relative to mean level, scaled by window length
            var normalized = slope * (series.Count - 1) / mean;
            return Math.Clamp(normalized, -1, 1);
        }

        /// <summary>Coefficient of variation (stddev / mean); 0 when the series is empty or flat-zero.</summary>
        public static double CoefficientOfVariation(IReadOnlyList<double> series)
        {
            if (series.Count == 0) return 0;
            var mean = series.Average();
            if (mean <= 0) return 0;
            var variance = series.Sum(v => (v - mean) * (v - mean)) / series.Count;
            return Math.Sqrt(variance) / mean;
        }
    }
}
