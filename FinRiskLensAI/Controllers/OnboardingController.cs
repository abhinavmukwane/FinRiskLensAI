using FinRiskLensAI.Core.Common;
using FinRiskLensAI.Core.Interfaces.ICommon;
using FinRiskLensAI.Core.Interfaces.IServices.Common;
using FinRiskLensAI.Core.Interfaces.IServices.OnBoarding;
using FinRiskLensAI.Core.Models;
using FinRiskLensAI.Core.Models.Onboarding;
using FinRiskLensAI.Core.Models.User_Activity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinRiskLensAI.Controllers
{
    public class OnboardingController : Controller
    {
        private readonly IOnboardingService _onboardingService;
        private readonly IEmailService _emailService;
        private readonly IEncryption _encryption;

        public OnboardingController(IOnboardingService onboardingService, IEmailService emailService, IEncryption encryption)
        {
            _onboardingService = onboardingService;
            _emailService = emailService;
            _encryption = encryption;
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
                if (await _onboardingService.IsUdyamRegistered(uan))
                {
                    return Json(new
                    {
                        status = false,
                        isRegistered = true,
                        message = "User is already registered. Kindly login."
                    });
                }
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

        [HttpPost]
        public async Task<IActionResult> RegisterUser(UserRegistrationModel model,string nameOfEnterprise)
        {
            if (model == null)
            {
                return Json(new { status = false, message = "Invalid request." });
            }

            // 1. Register / update the user first
            var result = await _onboardingService.AddUpdateUserRegst(model);
            if (result.Result == tflResultType.tflError)
            {
                return Json(new { status = false, message = "Something went wrong." });
            }

            if (string.IsNullOrWhiteSpace(model.Email))
            {
                return Json(new { status = false, message = "Email is required to send OTP." });
            }

            // 2. Generate OTP
            var otp = Random.Shared.Next(100000, 999999).ToString();

            // 3. Try sending the OTP email FIRST
            var emailSent = await _emailService.SendLoginOtpAsync(model.Email, otp, nameOfEnterprise, 
                            expiryMinutes: 5, ct: HttpContext.RequestAborted);
           
            // 4. Only persist the OTP if the email actually went out
            var otpModel = new UserOtpModel
            {

                UserRegistrationID = result.Data.UserRegistrationID,
                MsmeEnquiryID = result.Data.MsmeEnquiryID,
                Email = model.Email,
                MobileNumber = model.MobileNumber,
                OTP = _encryption.EncryptString(otp),
            };

            var otpResult = await _onboardingService.AddUpdateUserOtp(otpModel);
            if (otpResult == null)
            {
                return Json(new { status = false, message = "OTP sent but could not be saved. Please contact support." });
            }

            return Json(new
            {
                status = true,
                message = "OTP sent successfully to your email.",
                otp = otp 
            });
        }

        [HttpPost]
        public async Task<IActionResult> FetchUserOTPDet(UserOtpModel model)
        {
            var result = await _onboardingService.FetchUserOTPDet(model);

            if (result.Result == tflResultType.tflSuccess)
            {
                HttpContext.Session.SetInt32("MsmeEnquiryID", (int)result.Data.MsmeEnquiryID);
                HttpContext.Session.SetString("Email", result.Data.Email);

                return Json(new
                {
                    status = true,
                    message = "OTP validated successfully."
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
