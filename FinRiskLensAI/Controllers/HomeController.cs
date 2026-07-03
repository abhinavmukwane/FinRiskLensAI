using System.Diagnostics;
using FinRiskLensAI.Models;
using Microsoft.AspNetCore.Mvc;

namespace FinRiskLensAI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
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
        /// Finvu AA end-user redirect landing page — register this URL as the
        /// consent-journey callback/redirect URL. One view renders both outcomes,
        /// switched on the model: explicit status params win; otherwise the
        /// presence of Finvu's encrypted consent response (ecres) means success.
        /// </summary>
        [HttpGet]
        public IActionResult AAConsentCallback(
            string? status, string? consentStatus, string? txnid, string? sessionid,
            string? consentHandle, string? ecres, string? applicationId)
        {
            var raw = consentStatus ?? status;
            var accepted = new[] { "ACCEPTED", "APPROVED", "SUCCESS", "READY", "TRUE", "S", "Y" };
            var isSuccess = raw != null
                ? accepted.Contains(raw.Trim().ToUpperInvariant())
                : !string.IsNullOrEmpty(ecres);

            _logger.LogInformation(
                "AA consent callback: status={Status} txnid={TxnId} handle={Handle} ecresPresent={HasEcres} result={Result}",
                raw, txnid, consentHandle, !string.IsNullOrEmpty(ecres), isSuccess ? "success" : "failure");

            return View(new AAConsentCallbackViewModel
            {
                IsSuccess = isSuccess,
                ApplicationId = applicationId ?? txnid ?? sessionid,
                ConsentHandle = consentHandle,
                RawStatus = raw
            });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
