using Microsoft.AspNetCore.Mvc;

namespace FinRiskLensAI.Controllers
{
    public class OnboardingController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
