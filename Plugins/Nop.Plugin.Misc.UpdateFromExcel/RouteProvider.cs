using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Misc.UpdateFromExcel
{
    public class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Misc.UpdateFromExcel.Configure",
                 "Plugins/UpdateFromExcel/Configure",
                 new { controller = "UpdateFromExcel", action = "Configure" },
                 new[] { "Nop.Plugin.Misc.UpdateFromExcel.Controllers" }
            );

            routes.MapRoute("Plugin.Misc.UpdateFromExcel.UpdateFromExcelPopup",
                 "Plugins/UpdateFromExcel/UpdateFromExcelPopup",
                 new { controller = "UpdateFromExcel", action = "UpdateFromExcelPopup" },
                 new[] { "Nop.Plugin.Misc.UpdateFromExcel.Controllers" }
            );
        }

        public int Priority
        {
            get { return 0; }
        }

    }
}