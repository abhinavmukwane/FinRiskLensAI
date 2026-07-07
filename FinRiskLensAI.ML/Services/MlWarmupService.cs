using System.Diagnostics;
using FinRiskLensAI.ML.MachineLearning;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FinRiskLensAI.ML.Services
{
    /// <summary>
    /// Trains the lazy ML.NET models (LightGBM calibration + PCA anomaly detector)
    /// once at application startup, off the request path. Without this the FIRST
    /// /analyze request trains them inline — on a constrained/shared host that spike
    /// was slow enough to trip the reverse-proxy timeout or recycle the app pool,
    /// dropping the connection ("TypeError: Failed to fetch") before result.json was
    /// written. Runs on a background thread so app startup isn't delayed; the models'
    /// Lazy&lt;T&gt; makes this safe even if an early request races the warmup.
    /// </summary>
    public class MlWarmupService : IHostedService
    {
        private readonly ScoreCalibrationModel _calibration;
        private readonly IncomeAnomalyDetector _anomaly;
        private readonly ILogger<MlWarmupService> _logger;

        public MlWarmupService(ScoreCalibrationModel calibration, IncomeAnomalyDetector anomaly, ILogger<MlWarmupService> logger)
        {
            _calibration = calibration;
            _anomaly = anomaly;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _ = Task.Run(() =>
            {
                try
                {
                    var sw = Stopwatch.StartNew();
                    _calibration.Warmup();
                    _anomaly.Warmup();
                    _logger.LogInformation("ML models warmed up in {ElapsedMs} ms", sw.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ML warmup failed — the first /analyze will train inline");
                }
            }, CancellationToken.None);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
