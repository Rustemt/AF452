using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Payments.IsBank
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Payments.IsBank.Configure",
                 "Plugins/PaymentIsBank/Configure",
                 new { controller = "PaymentIsBank", action = "Configure" },
                 new[] { "Nop.Plugin.Payments.IsBank.Controllers" }
            );

            routes.MapRoute("Plugin.Payments.IsBank.PaymentInfo",
                 "Plugins/PaymentIsBank/PaymentInfo",
                 new { controller = "PaymentIsBank", action = "PaymentInfo" },
                 new[] { "Nop.Plugin.Payments.IsBank.Controllers" }
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
