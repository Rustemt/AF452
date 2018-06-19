using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Misc.ScheduledXmlEporter
{
    public class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Misc.ScheduledXmlEporter.Configure", "Plugins/MiscScheduledXmlEporter/Configure",
                            new { controller = "Settings", action = "Index" },
                            new[] { "Nop.Plugin.Misc.ScheduledXmlEporter.Controllers" }
                );
        }

        public int Priority
        {
            get { return 0; }
        }

    }
}