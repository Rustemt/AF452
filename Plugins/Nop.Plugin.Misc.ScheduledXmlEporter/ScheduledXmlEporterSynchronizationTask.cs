using Nop.Core.Infrastructure;
using Nop.Core.Plugins;
using Nop.Plugin.Misc.ScheduledXmlEporter.Services;
using Nop.Services.Tasks;
using System;
using Nop.Services.Logging;
using Nop.Services.Configuration;

namespace Nop.Plugin.Misc.ScheduledXmlEporter
{
    public class ScheduledXmlEporterSynchronizationTask : ITask
    {
        //private readonly IScheduledXmlEporterApiService _scheduledXmlEporterApiService = EngineContext.Current.Resolve<IScheduledXmlEporterApiService>();
        private readonly IPluginFinder _pluginFinder = EngineContext.Current.Resolve<IPluginFinder>();
        private readonly IScheduledXmlEporterConsumingService _scheduledConsumingService = EngineContext.Current.Resolve<IScheduledXmlEporterConsumingService>();
        private readonly ISettingService _settingService = EngineContext.Current.Resolve<ISettingService>();
        private readonly ScheduledXmlEporterSettings _scheduledXmlEporterSettings = EngineContext.Current.Resolve<ScheduledXmlEporterSettings>();

        /// <summary>
        /// Execute task
        /// </summary>
        public void Execute()
        {
            //is plugin installed?
            var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName("Misc.ScheduledXmlEporter");
            if (pluginDescriptor == null || !pluginDescriptor.Installed)
                return;

            //is plugin configured?
            var plugin = pluginDescriptor.Instance() as ScheduledXmlEporterPlugin;
            if (plugin == null || !plugin.IsConfigured())
                return;

            if (DateTime.Now.Hour != int.Parse(_scheduledXmlEporterSettings.ScheduleTime))
                return;

            _scheduledXmlEporterSettings.LastStartDate = DateTime.Now.Hour.ToString();
            _settingService.SaveSetting<ScheduledXmlEporterSettings>(_scheduledXmlEporterSettings);


            //_scheduledXmlEporterApiService.Synchronize();
            try
            {
                _scheduledConsumingService.XmlExportForN11(0);
            }
            catch (Exception ex)
            {
                LogException("XmlExportForN11(0) metodu hata donderdi!", ex);
            }

            try
            {
                _scheduledConsumingService.XmlExportForGG(0);
            }
            catch (Exception ex)
            {
                LogException("XmlExportForGG(0) metodu hata donderdi!", ex);
            }
            
        }

        private void LogException(string msg, Exception exc)
        {
            var logger = EngineContext.Current.Resolve<ILogger>();

            logger.Error(msg, exc);
        }
    }
}