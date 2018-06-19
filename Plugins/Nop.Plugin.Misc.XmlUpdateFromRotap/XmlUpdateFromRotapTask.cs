using Nop.Core.Infrastructure;
using Nop.Core.Plugins;
using Nop.Plugin.Misc.XmlUpdateFromRotap.Services;
using Nop.Services.Tasks;
using System;
using Nop.Services.Logging;
using Nop.Services.Configuration;

namespace Nop.Plugin.Misc.XmlUpdateFromRotap
{
    public class XmlUpdateFromRotapTask : ITask
    {
        //private readonly IXmlUpdateFromRotapApiService _xmlUpdateFromRotapApiService = EngineContext.Current.Resolve<IXmlUpdateFromRotapApiService>();
        private readonly IPluginFinder _pluginFinder = EngineContext.Current.Resolve<IPluginFinder>();
        private readonly IXmlUpdateFromRotapConsumingService _scheduledConsumingService = EngineContext.Current.Resolve<IXmlUpdateFromRotapConsumingService>();
        private readonly ISettingService _settingService = EngineContext.Current.Resolve<ISettingService>();
        private readonly XmlUpdateFromRotapSettings _xmlUpdateFromRotapSettings = EngineContext.Current.Resolve<XmlUpdateFromRotapSettings>();

        /// <summary>
        /// Execute task
        /// </summary>
        public void Execute()
        {
            //is plugin installed?
            var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName("Misc.XmlUpdateFromRotap");
            if (pluginDescriptor == null || !pluginDescriptor.Installed)
                return;

            //is plugin configured?
            var plugin = pluginDescriptor.Instance() as XmlUpdateFromRotapPlugin;
            if (plugin == null || !plugin.IsConfigured())
                return;

            //if (DateTime.Now.Hour != int.Parse(_xmlUpdateFromRotapSettings.ScheduleTime))
            //    return;

            _xmlUpdateFromRotapSettings.LastStartDate = DateTime.Now.ToString();
            _settingService.SaveSetting<XmlUpdateFromRotapSettings>(_xmlUpdateFromRotapSettings);

            try
            {
                _scheduledConsumingService.XmlUpdate();
            }
            catch (Exception ex)
            {
                LogException("Nop.Plugin.Misc.XmlUpdateFromRotap eklentisinde XmlUpdate() metodu hata donderdi!", ex);
            }                       
        }

        private void LogException(string msg, Exception exc)
        {
            var logger = EngineContext.Current.Resolve<ILogger>();

            logger.Error(msg, exc);
        }
    }
}