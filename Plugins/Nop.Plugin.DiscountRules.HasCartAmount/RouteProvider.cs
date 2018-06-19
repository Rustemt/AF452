using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.DiscountRules.HasCartAmount
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.DiscountRules.HasCartAmount.Configure",
                 "Plugins/DiscountRulesHasCartAmount/Configure",
                 new { controller = "DiscountRulesHasCartAmount", action = "Configure" },
                 new[] { "Nop.Plugin.DiscountRules.HasCartAmount.Controllers" }
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
