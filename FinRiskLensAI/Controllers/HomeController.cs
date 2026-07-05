using System.Diagnostics;
using FinRiskLensAI.Core.Interfaces.IServices.AccountAggregator;
using FinRiskLensAI.Models;
using Microsoft.AspNetCore.Mvc;

namespace FinRiskLensAI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAccountAggregatorService _aaService;

        public HomeController(ILogger<HomeController> logger, IAccountAggregatorService aaService)
        {
            _logger = logger;
            _aaService = aaService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Finvu AA end-user redirect landing page. Finvu redirects to
        /// /Home/AAConsentCallback/{trnxid}?ecres=...&amp;resdate=...&amp;fi=...
        /// The transaction id arrives as a route segment; a present <c>ecres</c>
        /// (encrypted consent response) means the journey completed successfully.
        /// </summary>
        [HttpGet("Home/AAConsentCallback/{trnxid?}")]
        public async Task<IActionResult> AAConsentCallback(
            string? trnxid, string? ecres, string? resdate, string? fi)
        {
            var accepted = new[] { "ACTIVE", "ACCEPTED", "APPROVED", "READY", "SUCCESS" };

            string? consentStatus = null;
            string? consentHandle = null;

            // Confirm the outcome with Finvu rather than trusting the redirect alone:
            // look up the consent we stored for this transaction, then query its status.
            if (!string.IsNullOrWhiteSpace(trnxid))
            {
                try
                {
                    var consent = await _aaService.GetAAConsentData(trnxid);
                    consentHandle = consent?.consentHandle;

                    if (consent != null && !string.IsNullOrWhiteSpace(consent.custId))
                    {
                        var details = await _aaService.CheckConsentStatus(trnxid, consent.custId);
                        consentStatus = details?.Body?.Status;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "AA consent status check failed for {TrnxId}", trnxid);
                }
            }

            // Prefer the verified status; fall back to the presence of ecres if the check couldn't run.
            var isSuccess = consentStatus != null
                ? accepted.Contains(consentStatus.Trim().ToUpperInvariant())
                : !string.IsNullOrEmpty(ecres);

            _logger.LogInformation(
                "AA consent callback: trnxid={TrnxId} status={Status} ecresPresent={HasEcres} resdate={ResDate} fi={Fi} result={Result}",
                trnxid, consentStatus, !string.IsNullOrEmpty(ecres), resdate, fi, isSuccess ? "success" : "failure");

            return View(new AAConsentCallbackViewModel
            {
                IsSuccess = isSuccess,
                ApplicationId = trnxid,
                ConsentHandle = consentHandle,
                RawStatus = consentStatus
            });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
