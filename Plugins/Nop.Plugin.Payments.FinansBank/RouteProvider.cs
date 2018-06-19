using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Payments.FinansBank
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Payments.FinansBank.Configure",
                 "Plugins/PaymentFinansBank/Configure",
                 new { controller = "PaymentFinansBank", action = "Configure" },
                 new[] { "Nop.Plugin.Payments.FinansBank.Controllers" }
            );

            routes.MapRoute("Plugin.Payments.FinansBank.PaymentInfo",
                 "Plugins/PaymentFinansBank/PaymentInfo",
                 new { controller = "PaymentFinansBank", action = "PaymentInfo" },
                 new[] { "Nop.Plugin.Payments.FinansBank.Controllers" }
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
