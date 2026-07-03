using FinRiskLensAI.ML.Services;
using FinRiskLensAI.ML.Storage;
using FinRiskLensAI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace FinRiskLensAI.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IBlobAnalysisService _analysis;
        private readonly IMsmeDataStore _store;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IBlobAnalysisService analysis, IMsmeDataStore store, ILogger<DashboardController> logger)
        {
            _analysis = analysis;
            _store = store;
            _logger = logger;
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        /// <summary>
        /// Financial Health Card — renders the ML risk analysis result for an MSME
        /// from its blob folder (result.json). ?uan=UDYAM-XX-XX-XXXXXXX
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> FinancialHealthCard(string? uan, CancellationToken ct)
        {
            var model = new FinancialHealthCardViewModel { Uan = uan?.Trim() };
            if (string.IsNullOrWhiteSpace(model.Uan))
                return View(model);

            try
            {
                model.Status = await _analysis.GetStatusAsync(model.Uan, ct);
                model.Result = await _analysis.GetResultAsync(model.Uan, ct);

                // Enterprise name for the card header, straight from the stored Udyam payload
                var udyamJson = await _store.DownloadAsync(model.Uan, MsmeDataFiles.Udyam, ct);
                if (udyamJson != null)
                    model.EnterpriseName = JObject.Parse(udyamJson)
                        .SelectToken("main_details.name_of_enterprise")?.Value<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed loading Financial Health Card for {Uan}", model.Uan);
                model.LoadError = "Could not load data for this Udyam number. Check the number and try again.";
            }

            return View(model);
        }
    }
}
