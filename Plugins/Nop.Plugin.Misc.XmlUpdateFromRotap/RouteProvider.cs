using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Misc.XmlUpdateFromRotap
{
    public class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Misc.XmlUpdateFromRotap.Configure", "Plugins/MiscXmlUpdateFromRotap/Configure",
                            new { controller = "MiscXmlUpdateFromRotapController", action = "Configure" },
                            new[] { "Nop.Plugin.Misc.XmlUpdateFromRotap.Controllers" }
                );
        }

        public int Priority
        {
            get { return 0; }
        }

    }
}