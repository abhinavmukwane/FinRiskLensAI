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
        public async Task<IActionResult> GenCustomerOtp(UserOtpModel model)
        {
            try
            {
                var check = await _onboardingService.ValidateCustomerEmail(model.Email);

                if (check.Result != tflResultType.tflSuccess)
                {
                    return Json(new
                    {
                        status = false,
                        message = check.Message
                    });
                }
                var otp = Random.Shared.Next(100000, 999999).ToString();

                bool emailSent = await _emailService.SendLoginOtpAsync(model.Email,otp,check.Message, expiryMinutes: 5,ct: HttpContext.RequestAborted);

                if (!emailSent)
                {
                    return Json(new
                    {
                        status = false,
                        message = "Failed to send OTP."
                    });
                }

                // Save/Update OTP
                var otpModel = new UserOtpModel
                {
                    UserOtpID=check.Data.UserOtpID,
                    UserRegistrationID = check.Data.UserRegistrationID,
                    MsmeEnquiryID = check.Data.MsmeEnquiryID,
                    MobileNumber = check.Data.MobileNumber,
                    Email = model.Email,
                    OTP = _encryption.EncryptString(otp),
                    UpdatedAt = check.Data.UpdatedAt,
                    UpdatedBy = "1",
                };

                var otpResult = await _onboardingService.AddUpdateUserOtp(otpModel);

                if (otpResult.Result == tflResultType.tflSuccess)
                {
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
                HttpContext.Session.SetInt32("MsmeEnquiryID", result.Data.MsmeEnquiryID ?? 0);
                HttpContext.Session.SetString("Email", result.Data.Email);

                return Json(new
                {
                    status = true,
                    message = "Login successful."
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
