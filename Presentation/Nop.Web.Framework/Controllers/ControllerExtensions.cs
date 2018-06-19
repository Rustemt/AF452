using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.IO;

namespace Nop.Web.Framework.Controllers
{
    public static class ControllerExtensions
    {
        public static string RenderPartialViewToString(this Controller controller, string viewName)
        {
            return RenderPartialViewToString(controller, viewName, null);
        }

        public static string RenderPartialViewToString(this Controller controller, string viewName, object model)
        {
            //Original source code: http://craftycodeblog.com/2010/05/15/asp-net-mvc-render-partial-view-to-string/
            if (string.IsNullOrEmpty(viewName))
                viewName = controller.ControllerContext.RouteData.GetRequiredString("action");

            controller.ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                ViewEngineResult viewResult = System.Web.Mvc.ViewEngines.Engines.FindPartialView(controller.ControllerContext, viewName);
                var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }
    }
}
