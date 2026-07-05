using FinRiskLensAI.Common;
using FinRiskLensAI.Core.Interfaces;
using FinRiskLensAI.Core.Models.Storage;
using FinRiskLensAI.Models;
using FinRiskLensAI.Utility;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace FinRiskLensAI.Controllers
{
    [CustDashboardAuthorize]
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

        public IActionResult CustDashboard()
        {
            return View();
        }

        /// <summary>
        /// Seeds the logged-in MSME's blob folder with the DUMMY-DATA GST template,
        /// rewriting the template filer's GSTIN/PAN to the user's so the copied
        /// returns validate as theirs. No-op if the folder already has GST files.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SeedFinancialData(CancellationToken ct)
        {
            var user = HttpContext.Session.GetCurrentUser();
            var uan = user?.UdyamNumber;
            if (string.IsNullOrWhiteSpace(uan) || string.IsNullOrWhiteSpace(user!.GstinNumber))
                return Json(new { status = false, message = "No Udyam/GSTIN in session." });

            // Already seeded? Don't copy again.
            var existing = await _store.ListFilesAsync(uan, ct);
            if (existing.Any(f => f.StartsWith("gstr", StringComparison.OrdinalIgnoreCase)))
                return Json(new { status = true, copied = 0, message = "GST data already present." });

            const string template = "dummy-data";
            var files = await _store.ListFilesAsync(template, ct);
            if (files.Count == 0)
                return Json(new { status = false, message = "DUMMY-DATA template folder is empty." });

            // Template filer's GSTIN (top-level of any gstr3b file); PAN is chars 2..11 of the GSTIN.
            var probe = await _store.DownloadAsync(template, files.First(f => f.StartsWith("gstr3b")), ct);
            var srcGstin = probe != null ? JObject.Parse(probe).SelectToken("$..gstin")?.Value<string>() : null;
            if (string.IsNullOrEmpty(srcGstin) || srcGstin.Length < 15)
                return Json(new { status = false, message = "Could not read template GSTIN." });

            var srcPan = srcGstin.Substring(2, 10);
            var userGstin = user.GstinNumber;
            var userPan = userGstin.Length >= 15 ? userGstin.Substring(2, 10) : (user.PanNumber ?? srcPan);

            int copied = 0;
            foreach (var f in files)
            {
                var content = await _store.DownloadAsync(template, f, ct);
                if (content == null) continue;
                content = content.Replace(srcGstin, userGstin).Replace(srcPan, userPan);
                await _store.UploadAsync(uan, f, content, ct);
                copied++;
            }

            _logger.LogInformation("Seeded {Count} GST files into {Uan} from template.", copied, uan);
            return Json(new { status = true, copied, message = $"Copied {copied} GST files." });
        }

        /// <summary>
        /// Financial Health Card — renders the ML risk analysis result for the
        /// logged-in MSME only. The UAN is taken from the authenticated session
        /// (never from the request), so a user can only ever see their own card.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> FinancialHealthCard(CancellationToken ct)
        {
            var uan = HttpContext.Session.GetCurrentUser()?.UdyamNumber;
            var model = new FinancialHealthCardViewModel { Uan = uan?.Trim() };

            if (string.IsNullOrWhiteSpace(model.Uan))
            {
                // Logged in but no Udyam number on the account yet — nothing to show.
                model.LoadError = "No Udyam number is linked to your account yet, so there is no Financial Health Card to display.";
                return View(model);
            }

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
                model.LoadError = "Could not load your Financial Health Card right now. Please try again later.";
            }

            return View(model);
        }

        // UdyamDetails moved to UdyamController (/Udyam/UdyamDetails), backed by
        // the common IStaticResponseService over m_StaticResponces.
    }
}
