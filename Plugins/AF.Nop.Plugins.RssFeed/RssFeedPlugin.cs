using System;
using System.Linq;
using System.Web.Routing;
using Nop.Core.Plugins;
using Nop.Services.Localization;
using Nop.Services.Common;
using Nop.Core.Domain.Tasks;
using Nop.Core.Data;
using Nop.Services.Configuration;

namespace AF.Nop.Plugins.RssFeed
{
    public class RssFeedPlugin : BasePlugin, IMiscPlugin
    {
        private readonly ISettingService _settingService;
        private readonly RssFeedSetting _rssFeedSettings;
        private readonly IRepository<ScheduleTask> _scheduleTaskRepository;

        public RssFeedPlugin(ISettingService settingService
            , RssFeedSetting rssFeedSettings
            , IRepository<ScheduleTask> scheduleTaskRepository)
        {
            _settingService = settingService;
            _rssFeedSettings = rssFeedSettings;
            _scheduleTaskRepository = scheduleTaskRepository;
        }

        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "RssFeed";
            routeValues = new RouteValueDictionary() { { "Namespaces", "AF.Nop.Plugins.RssFeed.Controllers" }, { "area", null } };
        }

        public override void Install()
        {
            DeleteScheduleTask();
            var task = new ScheduleTask()
            {
                Name = RssFeedHelper.ScheduleTaskName,
                Seconds = 3600,
                Type = "AF.Nop.Plugins.RssFeed.RssFeedTask, AF.Nop.Plugins.RssFeed",
                Enabled = false,
                StopOnError = false,
            };
             _scheduleTaskRepository.Insert(task);
            _rssFeedSettings.TaskId = task.Id;
            _rssFeedSettings.TaskName = task.Name;
            _rssFeedSettings.TaskCheckTime = task.Seconds;
            _settingService.SaveSetting(_rssFeedSettings);

            RssFeedLocales.Install(this);
            base.Install();
        }

        public override void Uninstall()
        {
            DeleteScheduleTask();
            RssFeedLocales.Uninstall(this);
            base.Uninstall();
        }

        protected void DeleteScheduleTask()
        {
            var task = _scheduleTaskRepository.Table.Where(x => x.Name == RssFeedHelper.ScheduleTaskName).FirstOrDefault();
            if (task != null)
                _scheduleTaskRepository.Delete(task);
        }
    }
}
