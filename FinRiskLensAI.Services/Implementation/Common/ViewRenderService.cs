using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinRiskLensAI.Services.Implementation.Common
{
    public interface IViewRenderService
    {
        Task<string> RenderViewToStringAsync(Controller controller, string viewName, object model);
    }

    public class ViewRenderService : IViewRenderService
    {
        private readonly IRazorViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;

        public ViewRenderService(
            IRazorViewEngine viewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider)
        {
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
        }

        public async Task<string> RenderViewToStringAsync(Controller controller, string viewName, object model)
        {
            controller.ViewData.Model = model;

            using var sw = new StringWriter();

            var viewResult = _viewEngine.FindView(controller.ControllerContext, viewName, false);

            if (!viewResult.Success)
                throw new Exception($"View '{viewName}' not found.");

            var viewContext = new ViewContext(
                controller.ControllerContext,
                viewResult.View,
                controller.ViewData,
                controller.TempData,
                sw,
                new HtmlHelperOptions());

            await viewResult.View.RenderAsync(viewContext);

            return sw.ToString();
        }
    }
}
