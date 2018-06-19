using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.DiscountRules.Customer
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.DiscountRules.Customer.Configure",
                 "Plugins/DiscountRulesCustomer/Configure",
                 new { controller = "DiscountRulesCustomer", action = "Configure" },
                 new[] { "Nop.Plugin.DiscountRules.Customer.Controllers" }
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
