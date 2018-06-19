using System.Web.Routing;
using Nop.Core.Plugins;
using Nop.Services.Common;

namespace Nop.Plugin.Misc.UpdateFromExcel
{
    public class UpdateFromExcelPlugin : BasePlugin, IMiscPlugin
    {

        public UpdateFromExcelPlugin()
        {
        }

        /// <summary>
        /// Is plugin configured?
        /// </summary>
        /// <returns></returns>
        public virtual bool IsConfigured()
        {
            //return !string.IsNullOrEmpty(_updateFromExcelSettings.ApiKey);
            return true;
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            base.Uninstall();
        }

        /// <summary>
        /// Gets a route for plugin configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "UpdateFromExcel";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Misc.UpdateFromExcel.Controllers" }, { "area", null } };
        }
    }
}