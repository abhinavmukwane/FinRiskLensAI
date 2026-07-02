using Microsoft.AspNetCore.Mvc;

namespace FinRiskLensAI.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
