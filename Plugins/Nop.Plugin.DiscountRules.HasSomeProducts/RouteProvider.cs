using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.DiscountRules.HasSomeProducts
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.DiscountRules.HasSomeProducts.Configure",
                 "Plugins/DiscountRulesHasSomeProducts/Configure",
                 new { controller = "DiscountRulesHasSomeProducts", action = "Configure" },
                 new[] { "Nop.Plugin.DiscountRules.HasSomeProducts.Controllers" }
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
