using FinRiskLensAI.Core.Common;
using FinRiskLensAI.Core.Interfaces.IServices.OnBoarding;
using FinRiskLensAI.Core.Models.Onboarding;
using Microsoft.AspNetCore.Mvc;

namespace FinRiskLensAI.Controllers
{
    public class OnboardingController : Controller
    {
        private readonly IOnboardingService _onboardingService;

        public OnboardingController(IOnboardingService onboardingService)
        {
            _onboardingService = onboardingService;
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
        public async Task<IActionResult> RegisterUser(UserRegistrationModel model)
        {
            if (model == null)
            {
                return Json(new {status = false,message = "Invalid request." });
            }

            var result = await _onboardingService.AddUpdateUserRegst(model);

            if (result == null)
            {
                return Json(new{ status = false, message = "Something went wrong." });
            }

            return Json(new
            {
                status = true,
                message = "User registered successfully."
            });
        }

    }
}
