using System;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Nop.Core.Domain.Tasks;
using Nop.Plugin.Misc.XmlUpdateFromRotap.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Tasks;
using Nop.Web.Framework.Controllers;
using Nop.Plugin.Misc.XmlUpdateFromRotap.Services;

namespace Nop.Plugin.Misc.XmlUpdateFromRotap.Controllers
{
    [AdminAuthorize]
    public class MiscXmlUpdateFromRotapController : Controller
    {
        private const string VIEW_PATH = "Nop.Plugin.Misc.XmlUpdateFromRotap.Views.MiscXmlUpdateFromRotap.Configure";
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly XmlUpdateFromRotapSettings _settings;
        private readonly IXmlUpdateFromRotapConsumingService _scheduledConsumingService;

        public MiscXmlUpdateFromRotapController(ISettingService settingService, IScheduleTaskService scheduleTaskService,              
            ILocalizationService localizationService, XmlUpdateFromRotapSettings settings
            , IXmlUpdateFromRotapConsumingService scheduledConsumingService)
        {
            this._settingService = settingService;
            this._scheduleTaskService = scheduleTaskService;
            this._localizationService = localizationService;
            this._settings = settings;
            this._scheduledConsumingService = scheduledConsumingService;
        }

        
        [NonAction]
        private XmlUpdateFromRotapSettingsModel PrepareModel()
        {
            var model = new XmlUpdateFromRotapSettingsModel();

            //Set the properties
            model.LastStartDate = _settings.LastStartDate.ToString();

            model.EmailForReporting = _settings.EmailForReporting;
            model.EmailForReportingCC = _settings.EmailForReportingCC;
            model.EnablePriceUpdate = _settings.EnablePriceUpdate;
            model.NameForReporting = _settings.NameForReporting;
            
            ScheduleTask task = FindScheduledTask();
            if (task != null)
            {
                model.AutoSyncEachMinutes = task.Seconds / 60;
                model.AutoSync = task.Enabled;
            }
            
            return model;
        }

        [NonAction]
        private ScheduleTask FindScheduledTask()
        {
            return _scheduleTaskService.GetAllTasks().Where(x => x.Type.Equals("Nop.Plugin.Misc.XmlUpdateFromRotap.XmlUpdateFromRotapTask, Nop.Plugin.Misc.XmlUpdateFromRotap", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        }

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            var model = PrepareModel();
            //Return the view
            return View(VIEW_PATH, model);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public ActionResult Configure(XmlUpdateFromRotapSettingsModel model)
        {
            string saveResult = "";
            if (ModelState.IsValid)
            {                
                _settings.EmailForReporting = model.EmailForReporting;
                _settings.EmailForReportingCC = model.EmailForReportingCC;
                _settings.EnablePriceUpdate = model.EnablePriceUpdate;
                _settings.NameForReporting = model.NameForReporting;
               
                _settingService.SaveSetting(_settings);
            }

            // Update the task
            var task = FindScheduledTask();
            if (task != null)
            {
                task.Enabled = model.AutoSync;
                task.Seconds = model.AutoSyncEachMinutes*60;
                _scheduleTaskService.UpdateTask(task);
                saveResult = _localizationService.GetResource("Plugin.Misc.XmlUpdateFromRotap.AutoSyncRestart");
            }

            model = PrepareModel();
            //set result text
            //model.SaveResult = saveResult;

            return View(VIEW_PATH, model);
        }

        [HttpPost, ActionName("Configure")]
        [FormValueRequired("run")]
        public ActionResult RunNow(XmlUpdateFromRotapSettingsModel model)
        {
            _scheduledConsumingService.XmlUpdate();

            model = PrepareModel();
            //set result text
            //model.SaveResult = saveResult;

            return View(VIEW_PATH, model);
        }
    }
}