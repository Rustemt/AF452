using System;
using System.Linq;
using Nop.Core.Domain.Tasks;
using Nop.Core.Plugins;
using Nop.Services.Localization;
using Nop.Services.Tasks;

namespace Nop.Plugin.Misc.XmlUpdateFromRotap.Services
{
    public class XmlUpdateFromRotapInstallationService
    {
        private readonly IScheduleTaskService _scheduleTaskService;

        public XmlUpdateFromRotapInstallationService(IScheduleTaskService scheduleTaskService)
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
        /// Installs this instance.
        /// </summary>
        /// <param name="plugin">The plugin.</param>
        public virtual void Install(BasePlugin plugin)
        {
            ////locales
            plugin.AddOrUpdatePluginLocaleResource("Plugin.Misc.XmlUpdateFromRotap.AutoSync", "Use AutoSync task");
            plugin.AddOrUpdatePluginLocaleResource("Plugin.Misc.XmlUpdateFromRotap.AutoSyncEachMinutes", "AutoSync task period (minutes)");
            plugin.AddOrUpdatePluginLocaleResource("Plugin.Misc.XmlUpdateFromRotap.AutoSyncRestart", "If sync task period has been changed, please restart the application");

            plugin.AddOrUpdatePluginLocaleResource("Plugin.Misc.XmlUpdateFromRotap.ScheduleTime", "Zamanlama periyodu (dk)");
            plugin.AddOrUpdatePluginLocaleResource("Plugin.Misc.XmlUpdateFromRotap.LastStartDate", "En son çalışma zamanı");
            plugin.AddOrUpdatePluginLocaleResource("Plugin.Misc.XmlUpdateFromRotap.EmailForReporting", "Rapor email adresi");
            plugin.AddOrUpdatePluginLocaleResource("Plugin.Misc.XmlUpdateFromRotap.EmailForReportingCC", "Rapor email adresi (cc)");
            plugin.AddOrUpdatePluginLocaleResource("Plugin.Misc.XmlUpdateFromRotap.NameForReporting", "Rapor adı");
            plugin.AddOrUpdatePluginLocaleResource("Plugin.Misc.XmlUpdateFromRotap.EnablePriceUpdate", "Fiyat güncellensin");
            
            //Install sync task
            InstallSyncTask();

            ////Install the database tables
            //_xmlUpdateFromRotapObjectContext.Install();
        }

        /// <summary>
        /// Uninstalls this instance.
        /// </summary>
        /// <param name="plugin">The plugin.</param>
        public virtual void Uninstall(BasePlugin plugin)
        {
            ////locales
            plugin.DeletePluginLocaleResource("Plugin.Misc.XmlUpdateFromRotap.AutoSync");
            plugin.DeletePluginLocaleResource("Plugin.Misc.XmlUpdateFromRotap.AutoSyncEachMinutes");
            plugin.DeletePluginLocaleResource("Plugin.Misc.XmlUpdateFromRotap.AutoSyncRestart");


            plugin.DeletePluginLocaleResource("Plugin.Misc.XmlUpdateFromRotap.ScheduleTime");
            plugin.DeletePluginLocaleResource("Plugin.Misc.XmlUpdateFromRotap.LastStartDate");
            plugin.DeletePluginLocaleResource("Plugin.Misc.XmlUpdateFromRotap.EmailForReporting");
            plugin.DeletePluginLocaleResource("Plugin.Misc.XmlUpdateFromRotap.EmailForReportingCC");
            plugin.DeletePluginLocaleResource("Plugin.Misc.XmlUpdateFromRotap.NameForReporting");
            plugin.DeletePluginLocaleResource("Plugin.Misc.XmlUpdateFromRotap.EnablePriceUpdate");

            //Remove scheduled task
            var task = FindScheduledTask();
            if (task != null)
                _scheduleTaskService.DeleteTask(task);

            //Uninstall the database tables
            //_xmlUpdateFromRotapObjectContext.Uninstall();
        }
    }
}