using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.DiscountRules.HasManufacturer
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.DiscountRules.HasManufacturer.Configure",
                 "Plugins/DiscountRulesHasManufacturer/Configure",
                 new { controller = "DiscountRulesHasManufacturer", action = "Configure" },
                 new[] { "Nop.Plugin.DiscountRules.HasManufacturer.Controllers" }
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
