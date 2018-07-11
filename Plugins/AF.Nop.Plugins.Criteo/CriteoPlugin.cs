using System;
using System.Web.Routing;
using Nop.Core.Plugins;
using Nop.Services.Localization;
using Nop.Services.Cms;
using Nop.Core.Domain.Cms;
using System.Collections.Generic;

namespace AF.Nop.Plugins.Criteo
{
    public class CriteoPlugin : BasePlugin, IWidgetPlugin
    {

        public CriteoPlugin()
        {
        }

        public void GetConfigurationRoute(int widgetId, out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "Criteo";
            routeValues = new RouteValueDictionary() { { "Namespaces", "AF.Nop.Plugins.Criteo.Controllers" }, { "area", null } };
        }

        public void GetDisplayWidgetRoute(int widgetId, out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "ProcessWidget";
            controllerName = "Criteo";
            routeValues = new RouteValueDictionary
            {
                {"Namespaces", "AF.Nop.Plugins.Criteo.Controllers"},
                {"area", null},
                {"widgetId", widgetId}
            };
        }

        public IList<WidgetZone> SupportedWidgetZones()
        {
            return new List<WidgetZone>()
            {
                WidgetZone.HeadHtmlTag
            };
        }

        public override void Install()
        {
            this.AddOrUpdatePluginLocaleResource("AF.Criteo.DataSaved", "The data has been saved successfully");


            this.AddOrUpdatePluginLocaleResource("AF.Criteo.AccountId", "Account Id");
            this.AddOrUpdatePluginLocaleResource("AF.Criteo.AccountId.Hint", "The account id on Criteo.");
            this.AddOrUpdatePluginLocaleResource("AF.Criteo.HashEmail", "Hash Email");
            this.AddOrUpdatePluginLocaleResource("AF.Criteo.HashEmail.Hint", "If true, the customer email will be hashed with MD5.");

            base.Install();
        }

        public override void Uninstall()
        {
            this.DeletePluginLocaleResource("AF.Criteo.DataSaved");

            this.DeletePluginLocaleResource("AF.Criteo.AccountId");
            this.DeletePluginLocaleResource("AF.Criteo.AccountId.Hint");
            this.DeletePluginLocaleResource("AF.Criteo.HashEmail");
            this.DeletePluginLocaleResource("AF.Criteo.HashEmail.Hint");
            
            base.Uninstall();
        }
        
    }
}
