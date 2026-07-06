using FinRiskLensAI.Core.Interfaces.IServices.Common;
using FinRiskLensAI.Services.Implementation.Common;
using Microsoft.AspNetCore.Mvc;

namespace FinRiskLensAI.Controllers
{
    public class ReportController : Controller
    {

        public IActionResult FinancialReport()
        {
            return View();
        }

                                                            
    }
}
