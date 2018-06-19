using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.DiscountRules.PaymentCC
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.DiscountRules.PaymentCC.Configure",
                 "Plugins/DiscountRulesPaymentCC/Configure",
                 new { controller = "DiscountRulesPaymentCC", action = "Configure" },
                 new[] { "Nop.Plugin.DiscountRules.PaymentCC.Controllers" }
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
