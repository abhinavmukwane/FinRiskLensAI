using FinRiskLensAI.Common;
using FinRiskLensAI.Core.Common;
using FinRiskLensAI.Core.Interfaces.ICommon;
using FinRiskLensAI.Core.Interfaces.IServices.Common;
using FinRiskLensAI.Core.Interfaces.IServices.OnBoarding;
using FinRiskLensAI.Core.Models.User_Activity;
using Microsoft.AspNetCore.Mvc;

namespace FinRiskLensAI.Controllers
{
    public class AuthController : Controller
    {
        private readonly IEmailService _emailService;
        private readonly IEncryption _encryption;
        private readonly IOnboardingService _onboardingService;

        public AuthController(IOnboardingService onboardingService, IEmailService emailService, IEncryption encryption)
        {
            _onboardingService = onboardingService;
            _emailService = emailService;
            _encryption = encryption;
        }
        public IActionResult CustLogin()
        {
            return View();
        }
        public ActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> GenCustomerOtp(UserOtpModel model, string? theme = null)
        {
            try
            {
                var check = await _onboardingService.ValidateCustomerEmail(model.Email);

                if (check.Result != tflResultType.tflSuccess)
                {
                    // Email isn't registered — send them to onboarding to register first.
                    return Json(new
                    {
                        status = false,
                        message = check.Message,
                        redirectUrl = Url.Action("CustOnboarding", "Onboarding")
                    });
                }
                var otp = Random.Shared.Next(100000, 999999).ToString();

                // Save/Update OTP
                var otpModel = new UserOtpModel
                {
                    UserOtpID=check.Data.UserOtpID,
                    UserRegistrationID = check.Data.UserRegistrationID,
                    MsmeEnquiryID = check.Data.MsmeEnquiryID,
                    MobileNumber = check.Data.MobileNumber,
                    Email = model.Email,
                    OTP = _encryption.EncryptString(otp),
                    UpdatedAt = DateTime.Now,
                    UpdatedBy = "system",
                };

                var otpResult = await _onboardingService.AddUpdateUserOtp(otpModel);

                if (otpResult.Result == tflResultType.tflSuccess)
                {
                    bool emailSent = await _emailService.SendLoginOtpAsync(model.Email, otp, check.Message, expiryMinutes: 5, theme: theme, ct: HttpContext.RequestAborted);

                    if (!emailSent)
                    {
                        return Json(new
                        {
                            status = false,
                            message = "Failed to send OTP! Please try again"
                        });
                    }

                    return Json(new
                    {
                        status = true,
                        message = "OTP sent successfully.",
                        otp = otp
                    });
                }

                return Json(new
                {
                    status = false,
                    message = otpResult.Message
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

        [HttpPost]
        public async Task<IActionResult> ValidateUserOtp(UserOtpModel model)
        {
            var result = await _onboardingService.FetchUserOTPDet(model);

            if (result.Result == tflResultType.tflSuccess)
            {
                // Capture the client IP once, then keep the logged-in user's key
                // identifiers (incl. ClientIP) in session so any page can read them.
                result.Data.ClientIP = await IP_Get_Service.GetClientIPAddressAsync(HttpContext);
                HttpContext.Session.SetCurrentUser(result.Data);

                return Json(new
                {
                    status = true,
                    message = "Login successful.",
                    redirectUrl = Url.Action("CustDashboard", "Dashboard")
                });
            }

            return Json(new
            {
                status = false,
                message = result.Message
            });
        }

    }
}
