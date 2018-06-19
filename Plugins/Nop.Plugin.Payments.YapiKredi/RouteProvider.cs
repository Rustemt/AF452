using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Payments.YapiKredi
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Payments.YapiKredi.Configure",
                 "Plugins/PaymentYapiKredi/Configure",
                 new { controller = "PaymentYapiKredi", action = "Configure" },
                 new[] { "Nop.Plugin.Payments.YapiKredi.Controllers" }
            );

            routes.MapRoute("Plugin.Payments.YapiKredi.PaymentInfo",
                 "Plugins/PaymentYapiKredi/PaymentInfo",
                 new { controller = "PaymentYapiKredi", action = "PaymentInfo" },
                 new[] { "Nop.Plugin.Payments.YapiKredi.Controllers" }
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
