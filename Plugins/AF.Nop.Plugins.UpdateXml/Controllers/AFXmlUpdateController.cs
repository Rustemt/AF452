using System;
using System.Net;
using System.Linq;
using System.Web.Mvc;
using AF.Nop.Plugins.XmlUpdate.Models;
using Nop.Web.Framework.Controllers;
using AF.Nop.Plugins.XmlUpdate.Services;
using Nop.Services.Localization;
using AF.Nop.Plugins.XmlUpdate.Extensions;
using System.Collections.Generic;
using System.IO;
using System.Web;

namespace AF.Nop.Plugins.XmlUpdate.Controllers
{
    public enum NotifyType
    {
        Success,
        Error
    }

    public class AFXmlUpdateController : Controller
    {
        public const string LOG_PATH = "~/App_Data/Logs/XmlUpdate";
        private readonly IXmlProviderService _xmlService;
        private readonly ILocalizationService _localizationService;

        public AFXmlUpdateController(IXmlProviderService xmlService, ILocalizationService localizationService)
        {
            _xmlService = xmlService;
            _localizationService = localizationService;
        }

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            ConfigurationModel model = new ConfigurationModel();
            var profiders = _xmlService.GetAllProviders();
            model.Providers.Total = profiders.Count;
            model.Providers.Data = profiders.Select(x => x.ToViewModel());


            DirectoryInfo info = new DirectoryInfo(Server.MapPath(LOG_PATH));
            Directory.CreateDirectory(info.FullName);
            FileInfo[] files = info.GetFiles("*.xlsx").OrderByDescending(p => p.CreationTime).ToArray();
            model.ReportFiles = files;

            return View("~/Plugins/AF.Nop.Plugins.XmlUpdate/Views/Configure.cshtml", model);
        }
        
        //[FormValueRequired("add-new-provider")]
        [AdminAuthorize]
        public ActionResult AddProvider()
        {
            var model = new ProviderModel();

            return View("~/Plugins/AF.Nop.Plugins.XmlUpdate/Views/AddProvider.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        public ActionResult AddProvider(ProviderModel model)
        {
            validatePropertyList(model.Properties);
            if (!ModelState.IsValid)
                return View("~/Plugins/AF.Nop.Plugins.XmlUpdate/Views/AddProvider.cshtml", model);
            try
            {
                var entity = _xmlService.AddNewProvider(model.Name, model.Url, model.XmlRootNode, model.XmlItemNode, model.Enabled, model.AuthType, model.Username, model.Password, model.AutoResetStock, model.AutoUnpublish, model.UnpublishZeroStock);
                var properties = model.Properties.Select(x => x.ToEntity()).ToList();
                _xmlService.SaveXmlProperties(entity.Id, properties);
                SuccessNotification(_localizationService.GetResource("AF.XmlUpdate.Save.Success"));
            }
            catch (Exception e)
            {
                ErrorNotification(e);
                return View("~/Plugins/AF.Nop.Plugins.XmlUpdate/Views/AddProvider.cshtml", model);
            }
            return RedirectToConfiguration();
        }

        [AdminAuthorize]
        public ActionResult EditProvider(int id)
        {
            var entity = _xmlService.GetProviderById(id);
            var model = entity.ToViewModel();
            model.SetProperties(_xmlService.GetProviderProperties(id));

            return View("~/Plugins/AF.Nop.Plugins.XmlUpdate/Views/EditProvider.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        public ActionResult EditProvider(ProviderModel model)
        {
            validatePropertyList(model.Properties);
            if (!ModelState.IsValid)
                return View("~/Plugins/AF.Nop.Plugins.XmlUpdate/Views/EditProvider.cshtml", model);
            try
            {
                var entity = _xmlService.GetProviderById(model.Id);
                entity.Name = model.Name;
                entity.Url = model.Url;
                entity.Username = model.Username;
                entity.Password = model.Password;
                entity.Enabled = model.Enabled;
                entity.AuthType = model.AuthType;
                entity.AutoUnpublish = model.AutoUnpublish;
                entity.AutoResetStock = model.AutoResetStock;
                entity.UnpublishZeroStock = model.UnpublishZeroStock;
                _xmlService.UpdateProvider(entity);

                var properties = model.Properties.Select(x => x.ToEntity()).ToList();
                _xmlService.SaveXmlProperties(entity.Id, properties);
                SuccessNotification(_localizationService.GetResource("AF.XmlUpdate.Save.Success"));
            }
            catch (Exception e)
            {
                ErrorNotification(e);
                return View("~/Plugins/AF.Nop.Plugins.XmlUpdate/Views/EditProvider.cshtml", model);
            }

            return RedirectToConfiguration();
        }

        [AdminAuthorize]
        public ActionResult ImportXml(int id)
        {
            string path = Server.MapPath(LOG_PATH);
            Directory.CreateDirectory(path);



            var provider = _xmlService.GetProviderById(id);
            if (provider == null)
                throw new Exception("No provider found with the id #" + id);

            string fileName = Path.Combine(path, String.Format("xml-update-{0}-{1}.xlsx", DateTime.Now.ToString("yyyyMMdd-HHmmss"), provider.Name));
            Stream stream = System.IO.File.Open(fileName, FileMode.Create);

            try
            {
                var result = _xmlService.UpdateProductsFromXML(provider, stream);
                stream.Close();

                

                ImportXmlModel model = new ImportXmlModel();
                model.Provider = provider.ToViewModel();
                model.Products.Total = result.XmlItemsCount;
                model.Products.Data = result.XmlRecords;//.Select(x => x.ToViewModel());

                return View("~/Plugins/AF.Nop.Plugins.XmlUpdate/Views/ImportXml.cshtml", model);
            }
            catch (Exception e)
            {
                stream.Close();
                ErrorNotification(e);
            }
            return RedirectToConfiguration();
        }

        [ValidateInput(false)]
        [AdminAuthorize]
        public ActionResult Download(string fileName)
        {
            try
            {
                string path = Server.MapPath(Path.Combine(LOG_PATH, fileName));
                FileInfo info = new FileInfo(path);
                if (!info.Exists)
                {
                    throw new HttpException(404, string.Format(@"The file ""{0}"" does not exists.", fileName));
                }

                return File(path, "application/vnd.ms-excel", fileName);
            }
            catch (Exception xp)
            {
                return Content(xp.Message);
            }
        }

        [HttpPost]
        [AdminAuthorize]
        public ActionResult DeleteReport()
        {
            try
            {
                string fileName = Request.Form["FileName"];
                if (string.IsNullOrEmpty(fileName))
                    throw new Exception("Invalid file name.");

                string path = Server.MapPath(Path.Combine(LOG_PATH, fileName));
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

        [HttpPost]
        [AdminAuthorize]
        public ActionResult Delete(int id)
        {
            var entity = _xmlService.GetProviderById(id);
            if (entity == null)
                throw new Exception("Page not found");

            _xmlService.DeleteProvider(entity);


            return RedirectToConfiguration();
        }

        public ActionResult Test()
        {
            using (WebClient client = new WebClient())
            {
                client.Credentials = new NetworkCredential("XmlUrunReader", "B2b12345678");
                Response.Headers["Content-Type"] += ";charset=utf-8";
                string htmlCode = client.DownloadString("http://www.tac.com.tr/xml/valeron.xml");
                return Content(htmlCode);
            }
        }

        protected ActionResult RedirectToConfiguration()
        {
            return RedirectToAction("ConfigureMiscPlugin", "Plugin", new { systemName = "AF.Nop.Plugins.XmlUpdate", Area = "Admin" });
        }

        protected void validatePropertyList(IList<PropertyModel> list)
        {
            for(int i=0; i< list.Count;i++)
            {
                var prop = list[i];
                if (prop.Enabled && String.IsNullOrEmpty(prop.Name))
                    ModelState.AddModelError("Properties["+i+"].Name", String.Format("The property is enabled but it does not have a value", prop.ProductProperty));
            }
        }


        protected virtual void SuccessNotification(string message, bool persistForTheNextRequest = true)
        {
            AddNotification(NotifyType.Success, message, persistForTheNextRequest);
        }

        protected virtual void ErrorNotification(Exception exception, bool persistForTheNextRequest = true)
        {
            AddNotification(NotifyType.Error, exception.Message, persistForTheNextRequest);
        }

        protected virtual void AddNotification(NotifyType type, string message, bool persistForTheNextRequest)
        {
            string dataKey = string.Format("nop.notifications.{0}", type);
            if (persistForTheNextRequest)
            {
                if (TempData[dataKey] == null)
                    TempData[dataKey] = new List<string>();
                ((List<string>)TempData[dataKey]).Add(message);
            }
            else
            {
                if (ViewData[dataKey] == null)
                    ViewData[dataKey] = new List<string>();
                ((List<string>)ViewData[dataKey]).Add(message);
            }
        }
    }
}
