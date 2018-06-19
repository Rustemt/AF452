using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.DiscountRules.ThreeProdsSameCat
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.DiscountRules.ThreeProdsSameCat.Configure",
                 "Plugins/DiscountRulesThreeProdsSameCat/Configure",
                 new { controller = "DiscountRulesThreeProdsSameCat", action = "Configure" },
                 new[] { "Nop.Plugin.DiscountRules.ThreeProdsSameCat.Controllers" }
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
