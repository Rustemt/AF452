using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;
using Nop.Web.Framework.Localization;

namespace AF.Nop.Plugins.RssFeed
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("AF.Nop.Plugins.RssFeed.Generate",
                 "Rss/Feed/Generate",
                 new { controller = "RssFeed", action = "Generate" },
                 new[] { "AF.Nop.Plugins.RssFeed.Controllers" }
            );

            //routes.MapLocalizedRoute("AF.Nop.Plugins.RssFeed.Download",
            //    "Rss/Feed/alwaysfahion.xml",
            //    new { controller = "RssFeed", action = "Download" },
            //     new[] { "AF.Nop.Plugins.RssFeed.Controllers" }
            //);
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
