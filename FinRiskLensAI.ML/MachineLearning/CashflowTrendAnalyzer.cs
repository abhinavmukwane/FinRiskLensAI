using FinRiskLensAI.ML.Features;
using Microsoft.ML;
using Microsoft.ML.Transforms.TimeSeries;

namespace FinRiskLensAI.ML.MachineLearning
{
    /// <summary>
    /// Derives a trend feature from the monthly bank-inflow series using ML.NET SSA
    /// time-series forecasting (per Doc/03_SCORING_ENGINE.md "Trend overlay"). Falls
    /// back to a least-squares slope when the series is too short for SSA.
    /// </summary>
    public class CashflowTrendAnalyzer
    {
        private class SeriesRow { public float Value { get; set; } }
        private class ForecastRow
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Local — bound by ML.NET
            public float[] Forecast { get; set; } = Array.Empty<float>();
        }

        /// <summary>Returns a normalized trend slope in -1..1 (positive = inflows accelerating).</summary>
        public double ComputeTrend(IReadOnlyList<double> monthlySeries)
        {
            if (monthlySeries.Count < 3) return 0;

            // SSA needs a reasonably long series; below 8 points use the analytic slope.
            if (monthlySeries.Count < 8)
                return TrendMath.NormalizedSlope(monthlySeries);

            try
            {
                var ml = new MLContext(seed: 42);
                var data = ml.Data.LoadFromEnumerable(monthlySeries.Select(v => new SeriesRow { Value = (float)v }));

                var windowSize = Math.Max(2, monthlySeries.Count / 3);
                var pipeline = ml.Forecasting.ForecastBySsa(
                    outputColumnName: nameof(ForecastRow.Forecast),
                    inputColumnName: nameof(SeriesRow.Value),
                    windowSize: windowSize,
                    seriesLength: monthlySeries.Count,
                    trainSize: monthlySeries.Count,
                    horizon: 3);

                var model = pipeline.Fit(data);
                using var engine = model.CreateTimeSeriesEngine<SeriesRow, ForecastRow>(ml);
                var forecast = engine.Predict().Forecast;

                var mean = monthlySeries.Average();
                if (mean <= 0 || forecast.Length == 0) return TrendMath.NormalizedSlope(monthlySeries);

                // Forecast average vs recent average, relative to level → same -1..1 scale
                var recent = monthlySeries.TakeLast(3).Average();
                var slope = (forecast.Average() - recent) / mean;
                return Math.Clamp(slope, -1, 1);
            }
            catch
            {
                // SSA can be picky about series shape — the analytic slope is an acceptable stand-in.
                return TrendMath.NormalizedSlope(monthlySeries);
            }
        }
    }
}
