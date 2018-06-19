using System.Web.Routing;
using Nop.Core.Plugins;
using Nop.Services.Common;
using Nop.Plugin.Misc.ScheduledXmlEporter.Services;

namespace Nop.Plugin.Misc.ScheduledXmlEporter
{
    public class ScheduledXmlEporterPlugin : BasePlugin, IMiscPlugin
    {
        private readonly ScheduledXmlEporterSettings _scheduledXmlEporterSettings;
        private readonly ScheduledXmlEporterInstallationService _scheduledXmlEporterInstallationService;

        public ScheduledXmlEporterPlugin(ScheduledXmlEporterSettings scheduledXmlEporterSettings, ScheduledXmlEporterInstallationService scheduledXmlEporterInstallationService)
        {
            this._scheduledXmlEporterSettings = scheduledXmlEporterSettings;
            this._scheduledXmlEporterInstallationService = scheduledXmlEporterInstallationService;
        }

        /// <summary>
        /// Is plugin configured?
        /// </summary>
        /// <returns></returns>
        public virtual bool IsConfigured()
        {
            //return !string.IsNullOrEmpty(_scheduledXmlEporterSettings.ApiKey);
            return true;
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            _scheduledXmlEporterInstallationService.Install(this);
            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            _scheduledXmlEporterInstallationService.Uninstall(this);
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
            actionName = "Index";
            controllerName = "Settings";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Misc.ScheduledXmlEporter.Controllers" }, { "area", null } };
        }
    }
}