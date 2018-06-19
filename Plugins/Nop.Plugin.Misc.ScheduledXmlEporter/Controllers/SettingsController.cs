using System;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Nop.Core.Domain.Tasks;
using Nop.Plugin.Misc.ScheduledXmlEporter.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Tasks;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Misc.ScheduledXmlEporter.Controllers
{
    [AdminAuthorize]
    public class SettingsController : Controller
    {
        private const string VIEW_PATH = "Nop.Plugin.Misc.ScheduledXmlEporter.Views.Settings.Index";
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly ScheduledXmlEporterSettings _settings;

        public SettingsController(ISettingService settingService, IScheduleTaskService scheduleTaskService,              
            ILocalizationService localizationService, ScheduledXmlEporterSettings settings)
        {
            this._settingService = settingService;
            this._scheduleTaskService = scheduleTaskService;
            this._localizationService = localizationService;
            this._settings = settings;
        }

        
        [NonAction]
        private ScheduledXmlEporterSettingsModel PrepareModel()
        {
            var model = new ScheduledXmlEporterSettingsModel();

            //Set the properties
            model.LastStartDate = _settings.LastStartDate.ToString();
            model.ScheduleTime = _settings.ScheduleTime.ToString();

            //model.ApiKey = _settings.ApiKey;
            //model.DefaultListId = _settings.DefaultListId;
            //model.WebHookKey = _settings.WebHookKey;
            ScheduleTask task = FindScheduledTask();
            if (task != null)
            {
                //model.AutoSyncEachMinutes = task.Seconds / 60;
                model.AutoSync = task.Enabled;
            }

            ////Maps the list options
            //MapListOptions(model);

            return model;
        }

        [NonAction]
        private ScheduleTask FindScheduledTask()
        {
            return _scheduleTaskService.GetAllTasks().Where(x => x.Type.Equals("Nop.Plugin.Misc.ScheduledXmlEporter.ScheduledXmlEporterSynchronizationTask, Nop.Plugin.Misc.ScheduledXmlEporter", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        }

        public ActionResult Index()
        {
            var model = PrepareModel();
            //Return the view
            return View(VIEW_PATH, model);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public ActionResult Index(ScheduledXmlEporterSettingsModel model)
        {
            string saveResult = "";
            if (ModelState.IsValid)
            {                
                _settings.ScheduleTime = model.ScheduleTime;

                //_settings.DefaultListId = model.DefaultListId;
                //_settings.ApiKey = model.ApiKey;
                //_settings.WebHookKey = model.WebHookKey;

                _settingService.SaveSetting(_settings);
            }

            // Update the task
            var task = FindScheduledTask();
            if (task != null)
            {
                task.Enabled = model.AutoSync;
                task.Seconds = 20; //model.AutoSyncEachMinutes*60;
                _scheduleTaskService.UpdateTask(task);
                saveResult = _localizationService.GetResource("Plugin.Misc.ScheduledXmlEporter.AutoSyncRestart");
            }

            model = PrepareModel();
            //set result text
            //model.SaveResult = saveResult;

            return View(VIEW_PATH, model);
        }
    }
}