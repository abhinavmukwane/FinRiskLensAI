using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FinRiskLensAI.Utility
{
    public class CustDashboardAuthorize : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var session = context.HttpContext.Session;

            var msmeEnquiryId = session.GetInt32("MsmeEnquiryID");
            var email = session.GetString("Email");

            if (msmeEnquiryId == null || string.IsNullOrEmpty(email))
            {
                context.Result = new RedirectToActionResult(
                    "CustOnboarding",
                    "Onboarding",
                    null);
            }
        }

    }
}
