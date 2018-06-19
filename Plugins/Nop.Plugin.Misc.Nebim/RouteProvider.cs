using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Misc.NebimIntegration
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Misc.Nebim.Configure",
                 "Plugins/MiscNebim/Configure",
                 new { controller = "NebimIntegration", action = "Configure" },
                 new[] { "Nop.Plugin.Misc.NebimIntegration.Controllers" }
            );

        }
        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}
