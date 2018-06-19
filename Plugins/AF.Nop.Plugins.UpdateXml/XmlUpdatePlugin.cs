using System;
using System.Web.Routing;
using Nop.Core.Plugins;
using Nop.Services.Localization;
using Nop.Services.Common;
using AF.Nop.Plugins.XmlUpdate.Domain;

namespace AF.Nop.Plugins.XmlUpdate
{
    public class XmlUpdatePlugin : BasePlugin, IMiscPlugin
    {
        private readonly XmlUpdateObjectContext _context;

        public XmlUpdatePlugin(XmlUpdateObjectContext context)
        {
            _context = context;
        }

        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "AFXmlUpdate";
            routeValues = new RouteValueDictionary() { { "Namespaces", "AF.Nop.Plugins.XmlUpdate.Controllers" }, { "area", null } };
        }

        public override void Install()
        {
            _context.Install();
            XmlUpdateLocales.Install(this);
            base.Install();
        }

        public override void Uninstall()
        {
            _context.Uninstall();
            XmlUpdateLocales.Uninstall(this);
            base.Uninstall();
        }
        
    }
}
