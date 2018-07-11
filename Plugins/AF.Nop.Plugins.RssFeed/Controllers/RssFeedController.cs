using System;
using System.Linq;
using System.Web.Mvc;
using AF.Nop.Plugins.RssFeed.Models;
using Nop.Web.Framework.Controllers;
using Nop.Services.Localization;
using System.IO;
using System.Web;
using Nop.Admin.Controllers;
using Nop.Services.Configuration;
using AF.Nop.Plugins.RssFeed.Services;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using System.Text;
using Nop.Services.Logging;
using NopTask = Nop.Services.Tasks;
using System.Threading.Tasks;
using System.Xml;
using System.Threading;
using Nop.Core;

namespace AF.Nop.Plugins.RssFeed.Controllers
{
    public enum NotifyType
    {
        Success,
        Error
    }

    public class RssFeedController : BaseNopController
    {
        public const string LOG_PATH = "~/App_Data/Logs/XmlUpdate";

        private readonly ISettingService _settingService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly RssFeedSetting _rssFeedSettings;
        private readonly IRssFeedService _rssFeedService;
        private readonly IRepository<ProductVariant> _productVariantRepository;
        private readonly NopTask.IScheduleTaskService _scheduleTaskService;
        private readonly IWebHelper _webHelper;
        private readonly ILogger _logger;

        public RssFeedController(RssFeedSetting rssFeedSettings
            , ISettingService settingService
            , ILanguageService languageService
            , ILocalizationService localizationService
            , IRssFeedService rssFeedService
            , IRepository<ProductVariant> productVariantRepository
            , NopTask.IScheduleTaskService scheduleTaskService
            , IWebHelper webHelper
            , ILogger logger
            )
        {
            _settingService = settingService;
            _rssFeedSettings = rssFeedSettings;
            _languageService = languageService;
            _localizationService = localizationService;
            _rssFeedService = rssFeedService;
            _productVariantRepository = productVariantRepository;
            _scheduleTaskService = scheduleTaskService;
            _logger = logger;
            _webHelper = webHelper;
        }

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            ConfigurationModel model = new ConfigurationModel();
            var defaultLanguage = _languageService.GetAllLanguages().FirstOrDefault();

            model.TaskId = _rssFeedSettings.TaskId;
            model.TaskName = _rssFeedSettings.TaskName;
            model.TaskRunTime = _rssFeedSettings.TaskRunTime;
            model.TaskCheckTime = _rssFeedSettings.TaskCheckTime;
            model.LastRunTime = _rssFeedSettings.LastRunTime;
            model.EnabledSchedule = _rssFeedSettings.EnabledSchedule;

            DirectoryInfo info = new DirectoryInfo(RssFeedHelper.GetFilePath(""));
            Directory.CreateDirectory(info.FullName);
            model.Files = info.GetFiles("*.xml").OrderByDescending(p => p.CreationTime).ToArray();

            _rssFeedSettings.RebuildList();
            AddLocales(_languageService, model.Locales);
            foreach (var m in model.Locales)
            {
                foreach (var s in _rssFeedSettings.Locales)
                {
                    if (m.LanguageId == s.LanguageId)
                    {
                        m.Link = s.Link;
                        m.Title = s.Title;
                        m.Description = s.Description;
                        m.FileName = s.FileName;
                        break;
                    }
                }
            }

            return View("~/Plugins/AF.Nop.Plugins.RssFeed/Views/Configure.cshtml", model);
        }

        [HttpPost, ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            DirectoryInfo info = new DirectoryInfo(RssFeedHelper.GetFilePath(""));
            Directory.CreateDirectory(info.FullName);
            model.Files = info.GetFiles("*.xml").OrderByDescending(p => p.CreationTime).ToArray();

            if (!ModelState.IsValid)
                return View("~/Plugins/AF.Nop.Plugins.RssFeed/Views/Configure.cshtml", model);

            try
            {
                _rssFeedSettings.EnabledSchedule = model.EnabledSchedule;
                _rssFeedSettings.Locales.Clear();
                foreach (var m in model.Locales)
                {
                    _rssFeedSettings.Locales.Add(new RssFeedSettingLocale()
                    {
                        Title = m.Title,
                        Link = m.Link,
                        FileName = m.FileName,
                        Description = m.Description,
                        LanguageId = m.LanguageId
                    });
                }
                _rssFeedSettings.RebuildJson();

                _rssFeedSettings.EnabledSchedule = model.EnabledSchedule;
                _rssFeedSettings.TaskRunTime = model.TaskRunTime;
                _rssFeedSettings.TaskCheckTime = model.TaskCheckTime;

                var task = _scheduleTaskService.GetTaskById(_rssFeedSettings.TaskId);
                if (task == null)
                {
                    ErrorNotification("The schedualTaskId is corrupted. So, the plugin cannot be run in a schedule till this value is retrieved correctly.");
                }
                else
                {
                    task.Enabled = model.EnabledSchedule;
                    task.Seconds = model.TaskCheckTime;
                    _scheduleTaskService.UpdateTask(task);
                    _rssFeedSettings.TaskName = task.Name;
                }

                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
                _settingService.SaveSetting(_rssFeedSettings);
                _settingService.ClearCache();

                return Configure();
            }
            catch (Exception ex)
            {
                ErrorNotification(ex);
                return View("~/Plugins/AF.Nop.Plugins.RssFeed/Views/Configure.cshtml", model);
            }


        }

        [HttpPost, AdminAuthorize]
        public ActionResult Generate()
        {
            try
            {
                //var ctx = System.Web.HttpContext.Current;
                //var t = Task.Factory.StartNew<bool>(() =>
                //
                    //System.Web.HttpContext.Current = ctx;
                    _rssFeedService.GeneratRssFeedFiles();
                    _rssFeedService.SaveUpdateTime(DateTime.Now);
                //    return true;
                //});
                //t.Wait();
                return Content("Files have been generated successfully.");

            }
            catch (Exception ex)
            {
                _logger.Error("RssFeed: error occured in generating XML files", ex);

                return Content(ex.Message);
            }
        }

        protected Task GeneratRssFeedTask(HttpContext ctx)
        {
            var T = new Task(() =>
            {
                System.Web.HttpContext.Current = ctx;
                _rssFeedService.GeneratRssFeedFiles();
                _rssFeedService.SaveUpdateTime(DateTime.Now);
            });
            T.Start();
            return T;
        }

        [HttpPost]
        public ActionResult ScheduleTask()
        {
            var cntx = System.Web.HttpContext.Current;
            var form = HttpContext.Request.Form;
            var username = form["username"];
            var password = form["password"];
            if (username == RssFeedHelper.TASK_USERNAME && password == RssFeedHelper.TASK_PASSWORD)
            {
                try
                {
                    _rssFeedService.GeneratRssFeedFiles();
                    _rssFeedService.SaveUpdateTime(DateTime.Now);
                    _logger.Information("RssFeed: Finish daily Schualed Task");
                }
                catch (Exception ex)
                {
                    _logger.Error("RssFeed: error occured in generating XML files", ex);

                }

                return Content("RssFeed: generating RSS task has been started");
            }


            return HttpNotFound();
        }

        [HttpPost, AdminAuthorize]
        public ActionResult ResetLastTime()
        {
            try
            {
                _rssFeedService.SaveUpdateTime(DateTime.MinValue);

                return Content("Configure");
            }
            catch (Exception ex)
            {
                _logger.Error("RssFeed: error occured in generating XML files", ex);

                throw ex;
            }
        }

        [HttpPost, AdminAuthorize]
        public ActionResult Test()
        {
            var lang = Request.Form["Language"];
            var maxCount = Request.Form["MaxCount"];
            try
            {
                int n = int.Parse(maxCount);
                var l = _languageService.GetAllLanguages().Where(x => x.UniqueSeoCode.ToLower() == lang.ToLower()).FirstOrDefault();
                if (l == null)
                    throw new Exception("Unrecognized language code '" + lang + "'");

                foreach (var locale in _rssFeedSettings.Locales)
                {
                    if (locale.LanguageId == l.Id)
                    {
                        //var date = DateTime.Parse("2016-02-04");
                        var products = _productVariantRepository.Table
                            .Where(x => x.Published && x.Product.Published && !x.Deleted && !x.Product.Deleted /* && x.UpdatedOnUtc < date*/)
                            .OrderByDescending(x => x.UpdatedOnUtc)
                            .Take(n)
                            .ToList();

                        var xmlSettings = new XmlWriterSettings();
                        xmlSettings.Indent = true;
                        xmlSettings.Encoding = Encoding.UTF8;

                        Response.ClearContent();
                        Response.Buffer = true;
                        Response.AddHeader("content-disposition", "attachment;filename = rss20-test-" + lang + ".xml");
                        Response.ContentType = "text/xml";

                        using (var stream = XmlWriter.Create(Response.Output, xmlSettings))
                        {
                            _rssFeedService.GeneratRssFeedFiles(stream, products, locale.LanguageId);
                            stream.Close();
                        }

                        return new EmptyResult();
                    }
                }

                throw new Exception("The language code '" + lang + "' is not saved in plugin settings");
            }
            catch (Exception xp)
            {
                ErrorNotification(xp);
                return RedirectToConfiguration();
            }
        }



        [ValidateInput(false)]
        [AdminAuthorize]
        public ActionResult DownloadXml(string fileName)
        {
            try
            {
                string path = RssFeedHelper.GetFilePath(fileName);
                FileInfo info = new FileInfo(path);
                if (!info.Exists)
                {
                    throw new HttpException(404, string.Format(@"The file ""{0}"" does not exists.", fileName));
                }

                return File(path, "text/xml", fileName);
            }
            catch (Exception xp)
            {
                ErrorNotification(xp);
                return RedirectToConfiguration();
            }
        }

        [HttpPost]
        [AdminAuthorize]
        public ActionResult DeleteXml()
        {
            try
            {
                string fileName = Request.Form["FileName"];
                if (string.IsNullOrEmpty(fileName))
                    throw new Exception("Invalid file name.");

                string path = RssFeedHelper.GetFilePath(fileName);
                FileInfo info = new FileInfo(path);
                if (!info.Exists)
                {
                    throw new HttpException(404, string.Format(@"The file ""{0}"" does not exists.", fileName));
                }
                info.Delete();
                SuccessNotification(string.Format(@"The file ""{0}"" has been deleted successfully.", fileName));
            }
            catch (Exception e)
            {
                ErrorNotification(e);
            }

            return RedirectToConfiguration();
        }

        protected ActionResult RedirectToConfiguration()
        {
            return RedirectToAction("ConfigureMiscPlugin", "Plugin", new { systemName = "AF.Nop.Plugins.RssFeed", Area = "Admin" });
        }
    }
}
