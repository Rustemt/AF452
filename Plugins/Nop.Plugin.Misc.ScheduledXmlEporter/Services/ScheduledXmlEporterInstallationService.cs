using System;
using System.Linq;
using Nop.Core.Domain.Tasks;
using Nop.Core.Plugins;
//using Nop.Plugin.Misc.ScheduledXmlEporter.Data;
using Nop.Services.Localization;
using Nop.Services.Tasks;

namespace Nop.Plugin.Misc.ScheduledXmlEporter.Services
{
    public class ScheduledXmlEporterInstallationService
    {
        private readonly IScheduleTaskService _scheduleTaskService;

        public ScheduledXmlEporterInstallationService(IScheduleTaskService scheduleTaskService)
        {
            _scheduleTaskService = scheduleTaskService;
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
                    Name = "ScheduledXmlEporter sync",
                    //for each 60 minutes: 3600
                    Seconds = 86400,
                    Type = "Nop.Plugin.Misc.ScheduledXmlEporter.ScheduledXmlEporterTask, Nop.Plugin.Misc.ScheduledXmlEporter",
                    Enabled = false,
                    StopOnError = false,
                };
                _scheduleTaskService.InsertTask(task);
            }
        }

        private ScheduleTask FindScheduledTask()
        {
            return _scheduleTaskService.GetAllTasks().Where(x => x.Type.Equals("Nop.Plugin.Misc.ScheduledXmlEporter.ScheduledXmlEporterSynchronizationTask, Nop.Plugin.Misc.ScheduledXmlEporter", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        }

        /// <summary>
        /// Installs this instance.
        /// </summary>
        /// <param name="plugin">The plugin.</param>
        public virtual void Install(BasePlugin plugin)
        {
            ////locales
            //plugin.AddOrUpdatePluginLocaleResource("Plugin.Misc.ScheduledXmlEporter.ApiKey", "ScheduledXmlEporter API Key");
            //plugin.AddOrUpdatePluginLocaleResource("Plugin.Misc.ScheduledXmlEporter.DefaultListId", "Default ScheduledXmlEporter List");
            plugin.AddOrUpdatePluginLocaleResource("Plugin.Misc.ScheduledXmlEporter.AutoSync", "Use AutoSync task");
            plugin.AddOrUpdatePluginLocaleResource("Plugin.Misc.ScheduledXmlEporter.AutoSyncEachMinutes", "AutoSync task period (minutes)");
            plugin.AddOrUpdatePluginLocaleResource("Plugin.Misc.ScheduledXmlEporter.AutoSyncRestart", "If sync task period has been changed, please restart the application");
            //plugin.AddOrUpdatePluginLocaleResource("Plugin.Misc.ScheduledXmlEporter.WebHookKey", "WebHooks Key");
            //plugin.AddOrUpdatePluginLocaleResource("Plugin.Misc.ScheduledXmlEporter.QueueAll", "Initial Queue");
            //plugin.AddOrUpdatePluginLocaleResource("Plugin.Misc.ScheduledXmlEporter.QueueAll.Hint", "Queue existing newsletter subscribers (run only once)");
            //plugin.AddOrUpdatePluginLocaleResource("Plugin.Misc.ScheduledXmlEporter.ManualSync", "Manual Sync");
            //plugin.AddOrUpdatePluginLocaleResource("Plugin.Misc.ScheduledXmlEporter.ManualSync.Hint", "Manually synchronize nopCommerce newsletter subscribers with ScheduledXmlEporter database");

            //Install sync task
            InstallSyncTask();

            ////Install the database tables
            //_scheduledXmlEporterObjectContext.Install();
        }

        /// <summary>
        /// Uninstalls this instance.
        /// </summary>
        /// <param name="plugin">The plugin.</param>
        public virtual void Uninstall(BasePlugin plugin)
        {
            ////locales
            //plugin.DeletePluginLocaleResource("Plugin.Misc.ScheduledXmlEporter.ApiKey");
            //plugin.DeletePluginLocaleResource("Plugin.Misc.ScheduledXmlEporter.DefaultListId");
            plugin.DeletePluginLocaleResource("Plugin.Misc.ScheduledXmlEporter.AutoSync");
            plugin.DeletePluginLocaleResource("Plugin.Misc.ScheduledXmlEporter.AutoSyncEachMinutes");
            plugin.DeletePluginLocaleResource("Plugin.Misc.ScheduledXmlEporter.AutoSyncRestart");
            //plugin.DeletePluginLocaleResource("Plugin.Misc.ScheduledXmlEporter.WebHookKey");
            //plugin.DeletePluginLocaleResource("Plugin.Misc.ScheduledXmlEporter.QueueAll");
            //plugin.DeletePluginLocaleResource("Plugin.Misc.ScheduledXmlEporter.QueueAll.Hint");
            //plugin.DeletePluginLocaleResource("Plugin.Misc.ScheduledXmlEporter.ManualSync");
            //plugin.DeletePluginLocaleResource("Plugin.Misc.ScheduledXmlEporter.ManualSync.Hint");

            //Remove scheduled task
            var task = FindScheduledTask();
            if (task != null)
                _scheduleTaskService.DeleteTask(task);

            //Uninstall the database tables
            //_scheduledXmlEporterObjectContext.Uninstall();
        }
    }
}