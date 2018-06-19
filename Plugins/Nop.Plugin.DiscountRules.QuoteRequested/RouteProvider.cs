using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.DiscountRules.QuoteRequested
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.DiscountRules.QuoteRequested.Configure",
                 "Plugins/DiscountRulesQuoteRequested/Configure",
                 new { controller = "DiscountRulesQuoteRequested", action = "Configure" },
                 new[] { "Nop.Plugin.DiscountRules.QuoteRequested.Controllers" }
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
