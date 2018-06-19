using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Payments.Akbank
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Payments.Akbank.Configure",
                 "Plugins/PaymentAkbank/Configure",
                 new { controller = "PaymentAkbank", action = "Configure" },
                 new[] { "Nop.Plugin.Payments.Akbank.Controllers" }
            );

            routes.MapRoute("Plugin.Payments.Akbank.PaymentInfo",
                 "Plugins/PaymentAkbank/PaymentInfo",
                 new { controller = "PaymentAkbank", action = "PaymentInfo" },
                 new[] { "Nop.Plugin.Payments.Akbank.Controllers" }
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
