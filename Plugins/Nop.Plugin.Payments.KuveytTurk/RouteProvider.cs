using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Payments.KuveytTurk
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Payments.KuveytTurk.Configure",
                 "Plugins/PaymentKuveytTurk/Configure",
                 new { controller = "PaymentKuveytTurk", action = "Configure" },
                 new[] { "Nop.Plugin.Payments.KuveytTurk.Controllers" }
            );

            routes.MapRoute("Plugin.Payments.KuveytTurk.PaymentInfo",
                 "Plugins/PaymentKuveytTurk/PaymentInfo",
                 new { controller = "PaymentKuveytTurk", action = "PaymentInfo" },
                 new[] { "Nop.Plugin.Payments.KuveytTurk.Controllers" }
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
