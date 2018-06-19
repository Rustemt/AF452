using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;

namespace Nop.Web.Infrastructure
{
     public class Utilities
    {

         public static string RenderViewToString<T>(string viewPath, T model, System.Web.Mvc.ControllerContext controllerContext)
         {
             using (var writer = new StringWriter())
             {
                 var view = new WebFormView(controllerContext, viewPath);
                 var vdd = new ViewDataDictionary<T>(model);
                 var viewCxt = new ViewContext(controllerContext, view, vdd, new TempDataDictionary(), writer);
                 viewCxt.View.Render(viewCxt, writer);
                 return writer.ToString();
             }
         }

         public static string RenderPartialViewToString(Controller controller, string viewName, object model)
         {
             controller.ViewData.Model = model;
             try
             {
                 using (StringWriter sw = new StringWriter())
                 {
                     ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, viewName);
                     ViewContext viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);
                     viewResult.View.Render(viewContext, sw);

                     return sw.ToString();
                 }
             }
             catch (Exception ex)
             {
                 return ex.ToString();
             }
         }
         public static string RenderViewToString(Controller controller, string viewName, object model)
         {
             controller.ViewData.Model = model;
             try
             {
                 using (StringWriter sw = new StringWriter())
                 {
                     ViewEngineResult viewResult = ViewEngines.Engines.FindView(controller.ControllerContext, viewName, null);
                     ViewContext viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);
                     viewResult.View.Render(viewContext, sw);

                     return sw.ToString();
                 }
             }
             catch (Exception ex)
             {
                 return ex.ToString();
             }
         }

         public static void ClearCacheCategoryProducts(string key)
         {
             HttpContext.Current.Cache.Remove(key);
         }
    }
}