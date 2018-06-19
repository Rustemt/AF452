using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace AF.Nop.Plugins.XmlUpdate
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("AF.Nop.Plugins.XmlUpdate.Controllers.Privider.Add",
                 "Admin/AF/XmlUpdate/AddProvider",
                 new { controller = "AFXmlProvider", action = "AddProvider" },
                 new[] { "AF.Nop.Plugins.XmlUpdate.Controllers" }
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
