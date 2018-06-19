using System.Web.Routing;
using Nop.Core.Plugins;
using Nop.Services.Common;
using Nop.Plugin.Misc.XmlUpdateFromRotap.Services;
using System;
using System.Linq;
using Nop.Core.Domain.Tasks;
using Nop.Services.Localization;
using Nop.Services.Tasks;

namespace Nop.Plugin.Misc.XmlUpdateFromRotap
{
    public class XmlUpdateFromRotapPlugin : BasePlugin, IMiscPlugin
    {
        private readonly XmlUpdateFromRotapSettings _xmlUpdateFromRotapSettings;
        private readonly IScheduleTaskService _scheduleTaskService;

        public XmlUpdateFromRotapPlugin(XmlUpdateFromRotapSettings xmlUpdateFromRotapSettings
            , IScheduleTaskService scheduleTaskService)
        {
            this._xmlUpdateFromRotapSettings = xmlUpdateFromRotapSettings;
            this._scheduleTaskService = scheduleTaskService;
        }

        /// <summary>
        /// Installs the sync task.
        /// </summary>
        private void InstallSyncTask()
        {
            //Check the database for the task
            var task = FindScheduledTask();

            if (task == null)
            {
                task = new ScheduleTask
                {
                    Name = "XmlUpdateFromRotap sync",
                    //for each 60 minutes: 3600
                    Seconds = 86400,
                    Type = "Nop.Plugin.Misc.XmlUpdateFromRotap.XmlUpdateFromRotapTask, Nop.Plugin.Misc.XmlUpdateFromRotap",
                    Enabled = false,
                    StopOnError = false,
                };
                _scheduleTaskService.InsertTask(task);
            }
        }

        private ScheduleTask FindScheduledTask()
        {
            return _scheduleTaskService.GetAllTasks().Where(x => x.Type.Equals("Nop.Plugin.Misc.XmlUpdateFromRotap.XmlUpdateFromRotapTask, Nop.Plugin.Misc.XmlUpdateFromRotap", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        }

        /// <summary>
        /// Is plugin configured?
        /// </summary>
        /// <returns></returns>
        public virtual bool IsConfigured()
        {
            //return !string.IsNullOrEmpty(_xmlUpdateFromRotapSettings.ApiKey);
            return true;
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            ////locales
            this.AddOrUpdatePluginLocaleResource("Plugin.Misc.XmlUpdateFromRotap.AutoSync", "Use AutoSync task");
            this.AddOrUpdatePluginLocaleResource("Plugin.Misc.XmlUpdateFromRotap.AutoSyncEachMinutes", "AutoSync task period (minutes)");
            this.AddOrUpdatePluginLocaleResource("Plugin.Misc.XmlUpdateFromRotap.AutoSyncRestart", "If sync task period has been changed, please restart the application");

            this.AddOrUpdatePluginLocaleResource("Plugin.Misc.XmlUpdateFromRotap.ScheduleTime", "Zamanlama periyodu (dk)");
            this.AddOrUpdatePluginLocaleResource("Plugin.Misc.XmlUpdateFromRotap.LastStartDate", "En son çalışma zamanı");
            this.AddOrUpdatePluginLocaleResource("Plugin.Misc.XmlUpdateFromRotap.EmailForReporting", "Rapor email adresi");
            this.AddOrUpdatePluginLocaleResource("Plugin.Misc.XmlUpdateFromRotap.EmailForReportingCC", "Rapor email adresi (cc)");
            this.AddOrUpdatePluginLocaleResource("Plugin.Misc.XmlUpdateFromRotap.NameForReporting", "Rapor adı");
            this.AddOrUpdatePluginLocaleResource("Plugin.Misc.XmlUpdateFromRotap.EnablePriceUpdate", "Fiyat güncellensin");

            //Install sync task
            InstallSyncTask();

            ////Install the database tables
            //_xmlUpdateFromRotapObjectContext.Install();
            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            ////locales
            this.DeletePluginLocaleResource("Plugin.Misc.XmlUpdateFromRotap.AutoSync");
            this.DeletePluginLocaleResource("Plugin.Misc.XmlUpdateFromRotap.AutoSyncEachMinutes");
            this.DeletePluginLocaleResource("Plugin.Misc.XmlUpdateFromRotap.AutoSyncRestart");


            this.DeletePluginLocaleResource("Plugin.Misc.XmlUpdateFromRotap.ScheduleTime");
            this.DeletePluginLocaleResource("Plugin.Misc.XmlUpdateFromRotap.LastStartDate");
            this.DeletePluginLocaleResource("Plugin.Misc.XmlUpdateFromRotap.EmailForReporting");
            this.DeletePluginLocaleResource("Plugin.Misc.XmlUpdateFromRotap.EmailForReportingCC");
            this.DeletePluginLocaleResource("Plugin.Misc.XmlUpdateFromRotap.NameForReporting");
            this.DeletePluginLocaleResource("Plugin.Misc.XmlUpdateFromRotap.EnablePriceUpdate");

            //Remove scheduled task
            var task = FindScheduledTask();
            if (task != null)
                _scheduleTaskService.DeleteTask(task);

            //Uninstall the database tables
            //_xmlUpdateFromRotapObjectContext.Uninstall();
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
            controllerName = "MiscXmlUpdateFromRotap";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Misc.XmlUpdateFromRotap.Controllers" }, { "area", null } };
        }
    }
}