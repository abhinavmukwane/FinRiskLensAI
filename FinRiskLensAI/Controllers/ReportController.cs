using FinRiskLensAI.Common;
using FinRiskLensAI.Core.Interfaces;
using FinRiskLensAI.Core.Interfaces.IServices.Common;
using FinRiskLensAI.Core.Models.Storage;
using FinRiskLensAI.Services.Implementation.Common;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Utilities.Collections;

namespace FinRiskLensAI.Controllers
{
    public class ReportController : Controller
    {
        private readonly IEmailService _emailService;
        private readonly IBlobAnalysisService _analysis;
        private readonly IMsmeDataStore _store;
        public ReportController(IEmailService emailService, IBlobAnalysisService analysis, IMsmeDataStore store)
        {
            _emailService = emailService; _analysis = analysis;
            _store = store;
        }

        public IActionResult FinancialReport()
        {
            return View();
        }

    
    }
}
