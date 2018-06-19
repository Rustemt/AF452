using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Payments.Garanti
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Payments.Garanti.Configure",
                 "Plugins/PaymentGaranti/Configure",
                 new { controller = "PaymentGaranti", action = "Configure" },
                 new[] { "Nop.Plugin.Payments.Garanti.Controllers" }
            );

            routes.MapRoute("Plugin.Payments.Garanti.PaymentInfo",
                 "Plugins/PaymentGaranti/PaymentInfo",
                 new { controller = "PaymentGaranti", action = "PaymentInfo" },
                 new[] { "Nop.Plugin.Payments.Garanti.Controllers" }
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
