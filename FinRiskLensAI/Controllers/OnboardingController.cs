using FinRiskLensAI.Core.Common;
using FinRiskLensAI.Core.Interfaces.IServices.Common;
using FinRiskLensAI.Core.Interfaces.IServices.OnBoarding;
using Microsoft.AspNetCore.Mvc;

namespace FinRiskLensAI.Controllers
{
    public class OnboardingController : Controller
    {
        private readonly IOnboardingService _onboardingService;
        private readonly IEmailService _emailService;

        public OnboardingController(IOnboardingService onboardingService, IEmailService emailService)
        {
            _onboardingService = onboardingService;
            _emailService = emailService;
        }
        public IActionResult CustOnboarding()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> FetchUdyam(string uan)
        {
            try
            {
                var result = await _onboardingService.FetchUdyam(uan);

                if (result.Result == tflResultType.tflSuccess && result.Data != null)
                {
                    await _onboardingService.SaveMsmeData(result.Data.UdyamNumberResponce);

                    return Json(new{status = true, uan = result.Data.UdyamNumber, message = "Success"});
                }

                return Json(new{ status = false,message = result.Message});
            }
            catch (Exception ex)
            {
                return Json(new{status = false, message = ex.Message});
            }
        }
        /// <summary>
        /// Dev/test only — sends the login-OTP template email with a random code.
        /// GET /Onboarding/SendTestEmail?to=someone@example.com&name=Abhinav
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SendTestEmail(string to, string? name, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(to))
                return Json(new { status = false, message = "Pass ?to=recipient@email.com" });

            try
            {
                var otp = Random.Shared.Next(100000, 999999).ToString();
                var sent = await _emailService.SendLoginOtpAsync(to, otp, name, expiryMinutes: 10, ct);

                return Json(new
                {
                    status = sent,
                    message = sent ? $"Test OTP email sent to {to} (code {otp})" : "SMTP send failed — check logs/cre-*.log for details"
                });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUdyamDetails(string uan)
        {
            try
            {
                var result = await _onboardingService.GetUdyamDetails(uan);

                return Json(new
                {
                    status = result.Result == tflResultType.tflSuccess,
                    data = result.Data,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    status = false,
                    message = ex.Message
                });
            }
        }
    }
}
