using FinRiskLensAI.Common;
using FinRiskLensAI.Core.Interfaces;
using FinRiskLensAI.Core.Interfaces.IServices.AccountAggregator;
using FinRiskLensAI.Core.Interfaces.IServices.Common;
using FinRiskLensAI.Core.Models.Storage;
using FinRiskLensAI.Core.Models.Universal;
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
        private readonly IAccountAggregatorService _aaService;
        private readonly IIpRiskService _ipRisk;

        public DashboardController(IBlobAnalysisService analysis, IMsmeDataStore store, ILogger<DashboardController> logger, IAccountAggregatorService AAService, IIpRiskService ipRisk)
        {
            _analysis = analysis;
            _store = store;
            _logger = logger;
            _aaService = AAService;
            _ipRisk = ipRisk;
        }

        /// <summary>
        /// Source-IP security audit for the "Secure Connection IP" popup. Reads
        /// the client IP captured in session at login and hands it to the single
        /// IP risk service (currently sourced from m_StaticResponces.IPResponce;
        /// the Signzy API once the production key is live). Called only when the
        /// popup opens — never during login.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetIpVerificationDetail()
        {
            var user = HttpContext.Session.GetCurrentUser();
            var ip = user?.ClientIP;
            var uan = user?.UdyamNumber;

            var json = await _ipRisk.GetIpRiskScoreAsync(ip ?? string.Empty, uan ?? string.Empty, HttpContext.RequestAborted);

            var vm = string.IsNullOrWhiteSpace(json)
                ? new IpVerificationViewModel()
                : IpVerificationViewModel.FromJson(json);

            if (!vm.HasData)
                vm = IpVerificationViewModel.Demo();

            if (!string.IsNullOrWhiteSpace(ip))
                vm.Ip = ip;

            return Json(new { status = vm.HasData, data = vm });
        }

        /// <summary>
        /// Kicks off the Finvu AA consent journey for the logged-in MSME and
        /// returns the consent URL for the client to open in a new tab.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> InitiateAAFetch()
        {
            var user = HttpContext.Session.GetCurrentUser();
            if (user == null || string.IsNullOrWhiteSpace(user.UdyamNumber))
                return Json(new { status = false, message = "Your session has expired. Please log in again." });

            if (string.IsNullOrWhiteSpace(user.MobileNumber))
                return Json(new { status = false, message = "No mobile number is linked to your account." });

            var aaId = user.MobileNumber + "@finvu";
            try
            {
                var url = await _aaService.CreateConsentRequest(user.UdyamNumber, aaId);
                if (string.IsNullOrWhiteSpace(url))
                    return Json(new { status = false, message = "Could not initiate the Account Aggregator consent. Please try again." });

                return Json(new { status = true, url, message = "Consent request created — opening the Account Aggregator." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AA consent initiation failed for {Uan}", user.UdyamNumber);
                return Json(new { status = false, message = "Something went wrong while contacting the Account Aggregator." });
            }
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
    }
}
