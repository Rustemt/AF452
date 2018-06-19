using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Nop.Admin.Models.Messages;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Messages;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework.Controllers;
using Telerik.Web.Mvc;
using System.Net;
using Nop.Core.Infrastructure;
using Nop.Services.Logging;
using System.Collections.Generic;
using System.Web;
using Nop.Services.Customers;
using Nop.Services.Configuration;
using Nop.Core.Domain.Customers;

namespace Nop.Admin.Controllers
{
	[AdminAuthorize]
	public class NewsLetterSubscriptionController : BaseNopController
	{
		private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
		private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly AdminAreaSettings _adminAreaSettings;
        private readonly ILanguageService _languageService;
        private readonly ICustomerService _customerService;
        private readonly ISettingService _settingService;
        private readonly ILogger _logger;


        public NewsLetterSubscriptionController(INewsLetterSubscriptionService newsLetterSubscriptionService, 
            ILanguageService languageService,IDateTimeHelper dateTimeHelper,ILocalizationService localizationService,
            IPermissionService permissionService, AdminAreaSettings adminAreaSettings, ICustomerService customerService, ISettingService settingService, ILogger logger)
		{
			this._newsLetterSubscriptionService = newsLetterSubscriptionService;
			this._dateTimeHelper = dateTimeHelper;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._adminAreaSettings = adminAreaSettings;
            this._languageService = languageService;
            this._customerService = customerService;
            this._settingService = settingService;
            this._logger = logger;

		}

		public ActionResult Index()
		{
			return RedirectToAction("List");
		}

		public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNewsletterSubscribers))
                return AccessDeniedView();

            var model = new NewsLetterSubscriptionListModel();


            //languages
            model.AvailableLanguageNames.Add(new SelectListItem() { Text  = "---", Value = "0" });
            foreach (var lgs in _languageService.GetAllLanguages(true))
                model.AvailableLanguageNames.Add(new SelectListItem() { Text = lgs.Name,Value= lgs.Id.ToString() });

            var newsletterSubscriptions = _newsLetterSubscriptionService.GetAllNewsLetterSubscriptions(String.Empty, 0, _adminAreaSettings.GridPageSize, true);
			model.NewsLetterSubscriptions = new GridModel<NewsLetterSubscriptionModel>
			{
				Data = newsletterSubscriptions.Select(x => 
				{
					var m = x.ToModel();
				    m.LanguageName =_languageService.GetLanguageById( x.LanguageId).Name;
					m.CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc);
					return m;
				}),
				Total = newsletterSubscriptions.TotalCount
			};
			return View(model);
		}

		[HttpPost, GridAction(EnableCustomBinding = true)]
		public ActionResult SubscriptionList(GridCommand command, NewsLetterSubscriptionListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNewsletterSubscribers))
                return AccessDeniedView();

            var newsletterSubscriptions = _newsLetterSubscriptionService.GetAllNewsLetterSubscriptions(model.SearchEmail, model.SearchLanguageId,
                command.Page - 1, command.PageSize, true);

            var gridModel = new GridModel<NewsLetterSubscriptionModel>
            {
                Data = newsletterSubscriptions.Select(x =>
				{
					var m = x.ToModel();
					m.CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc);
					return m;
				}),
                Total = newsletterSubscriptions.TotalCount
            };
            return new JsonResult
            {
                Data = gridModel
            };
		}

        [GridAction(EnableCustomBinding = true)]
        public ActionResult SubscriptionUpdate(NewsLetterSubscriptionModel model, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNewsletterSubscribers))
                return AccessDeniedView();
            
            if (!ModelState.IsValid)
            {
                //display the first model error
                var modelStateErrors = this.ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
                return Content(modelStateErrors.FirstOrDefault());
            }

            var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionById(model.Id);
            subscription.Email = model.Email;
            subscription.Active = model.Active;
            _newsLetterSubscriptionService.UpdateNewsLetterSubscription(subscription);

            return SubscriptionList(command, new NewsLetterSubscriptionListModel());
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult SubscriptionDelete(int id, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNewsletterSubscribers))
                return AccessDeniedView();

            var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionById(id);
            if (subscription == null)
                throw new ArgumentException("No subscription found with the specified id");
            _newsLetterSubscriptionService.DeleteNewsLetterSubscription(subscription);

            return SubscriptionList(command, new NewsLetterSubscriptionListModel());
        }

		public ActionResult ExportCsv(NewsLetterSubscriptionListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNewsletterSubscribers))
                return AccessDeniedView();

			string fileName = String.Format("newsletter_emails_{0}_{1}.txt", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), CommonHelper.GenerateRandomDigitCode(4));

			var sb = new StringBuilder();
			var newsLetterSubscriptions = _newsLetterSubscriptionService.GetAllNewsLetterSubscriptions(model.SearchEmail, 0, int.MaxValue, true);
			if (newsLetterSubscriptions.Count == 0)
			{
				throw new NopException("No emails to export");
			}
			for (int i = 0; i < newsLetterSubscriptions.Count; i++)
			{
				var subscription = newsLetterSubscriptions[i];
				sb.Append(subscription.Email);
                sb.Append("\t");
                sb.Append(subscription.Active);
                sb.Append(Environment.NewLine);  //new line
			}
			string result = sb.ToString();

			return File(Encoding.UTF8.GetBytes(result), "text/csv", fileName);
		}

		[HttpPost]
		public ActionResult ImportCsv(FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNewsletterSubscribers))
                return AccessDeniedView();

			try
			{
				var file = Request.Files["importcsvfile"];
				if (file != null && file.ContentLength > 0)
				{
					int count = 0;

					using (var reader = new StreamReader(file.InputStream))
					{
						while (!reader.EndOfStream)
						{
							string line = reader.ReadLine();
							string[] tmp = line.Split('\t');

							if (tmp.Length == 2)
							{
								string email = tmp[0].Trim();
								bool isActive = Boolean.Parse(tmp[1]);

								var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmail(email);
								if (subscription != null)
								{
									subscription.Email = email;
									subscription.Active = isActive;
									_newsLetterSubscriptionService.UpdateNewsLetterSubscription(subscription);
								}
								else
								{
									subscription = new NewsLetterSubscription()
													   {
														   Active = isActive,
														   CreatedOnUtc = DateTime.UtcNow,
														   Email = email,
														   NewsLetterSubscriptionGuid = Guid.NewGuid()
													   };
									_newsLetterSubscriptionService.InsertNewsLetterSubscription(subscription);
								}
								count++;
							}
						}
						SuccessNotification(
							String.Format(
								_localizationService.GetResource(
                                    "Admin.Promotions.NewsLetterSubscriptions.ImportEmailsSuccess"), count))
									;
						return RedirectToAction("List");
					}
				}
				ErrorNotification("Please upload a file");
				return RedirectToAction("List");
			}
			catch (Exception exc)
			{
				ErrorNotification(exc);
				return RedirectToAction("List");
			}
		}


        public ActionResult Synchronize()
        {
            var customers = _customerService.GetAllCustomers(null, null, null, null, null, null, null, 0, 0, false, null,
             0, int.MaxValue).Where(x => !string.IsNullOrEmpty(x.Email));

            var newsletterSubscriptions = _newsLetterSubscriptionService.
            GetAllNewsLetterSubscriptions(String.Empty, 0, int.MaxValue, true);

            if (customers.ToList().Count == 0 || newsletterSubscriptions.Count == 0)
            {
                return RedirectToAction("List");
            }


            var PassiveNewsletter = newsletterSubscriptions.Where(x => x.Active == false).ToList();

            List<MadMimiItem> AddList = new List<MadMimiItem>();
            List<MadMimiItem> DeleteList = new List<MadMimiItem>();
            List<MadMimiItem> AddListCustomer = new List<MadMimiItem>();
            List<MadMimiItem> AddListNewsLetter = new List<MadMimiItem>();


            MadMimiItem madmimiitem = null;

            foreach (var item in customers)
            {
                madmimiitem = new MadMimiItem();
                madmimiitem.FirstName = item.GetAttribute<string>(SystemCustomerAttributeNames.FirstName);
                madmimiitem.LastName = item.GetAttribute<string>(SystemCustomerAttributeNames.LastName);
                madmimiitem.Gender = item.GetAttribute<string>(SystemCustomerAttributeNames.Gender);
                madmimiitem.Email = item.Email;

                if (item.LanguageId == 1)
                {
                    madmimiitem.Language = "English";
                }

                else if (item.LanguageId == 2)
                {
                    madmimiitem.Language = "Turkish";
                }

                else
                {
                    madmimiitem.Language = "";
                }

                AddListCustomer.Add(madmimiitem);

            }


            foreach (var item in newsletterSubscriptions.Where(x => x.Active == true))
            {
                madmimiitem = new MadMimiItem();
                madmimiitem.FirstName = item.FirstName;
                madmimiitem.LastName = item.LastName;
                madmimiitem.Gender = item.Gender;
                madmimiitem.Email = item.Email;

                if (item.LanguageId == 1)
                {
                    madmimiitem.Language = "English";
                }

                else if (item.LanguageId == 2)
                {
                    madmimiitem.Language = "Turkish";
                }

                else
                {
                    madmimiitem.Language = "";
                }

                AddListNewsLetter.Add(madmimiitem);


            }

            foreach (var item in PassiveNewsletter)
            {
                madmimiitem = new MadMimiItem();
                madmimiitem.FirstName = item.FirstName;
                madmimiitem.LastName = item.LastName;
                madmimiitem.Gender = item.Gender;
                madmimiitem.Email = item.Email;

                if (item.LanguageId == 1)
                {
                    madmimiitem.Language = "English";
                }

                else if (item.LanguageId == 2)
                {
                    madmimiitem.Language = "Turkish";
                }

                else
                {
                    madmimiitem.Language = "";
                }

                DeleteList.Add(madmimiitem);


            }


            AddList = AddListCustomer.Union(AddListNewsLetter, new MadMimiItemComparer()).ToList();                          
            AddList = AddList.Except(DeleteList, new MadMimiItemComparer()).ToList();                                        

            AddMadmimi(AddList, _settingService.GetSettingByKey<string>("madmimi.list.af.kayit", ""));
            DeleteMadmimi(DeleteList, _settingService.GetSettingByKey<string>("madmimi.list.af.kayit", ""));


            return RedirectToAction("List");

        }


        public void AddMadmimi(List<MadMimiItem> list, string listname)
        {

            var sb = new StringBuilder();

            string[] columns = new string[] { "add_list", "email", "firstname", "lastname", "gender", "language" };

            sb.AppendLine(String.Join(",", columns));

            for (int i = 0; i < list.Count; i++)
            {
                var subscription = list[i];
                sb.Append(listname);
                sb.Append(",");
                sb.Append(subscription.Email);
                sb.Append(",");
                sb.Append(subscription.FirstName);
                sb.Append(",");
                sb.Append(subscription.LastName);
                sb.Append(",");
                sb.Append(subscription.Gender);
                sb.Append(",");
                sb.Append(subscription.Language);
                sb.Append(Environment.NewLine);
            }


            string result = sb.ToString();

            string data = String.Format("csv_file={0}", HttpUtility.UrlEncode(result.ToString()));

            HttpWebResponse resp = PostToMadMimi("/audience_members", data);

        }


        public void DeleteMadmimi(List<MadMimiItem> list, string listname)
        {
            var sb = new StringBuilder();

            string[] columns = new string[] { "remove_list", "email", "firstname", "lastname", "gender", "language" };

            sb.AppendLine(String.Join(",", columns));

            sb.Append(Environment.NewLine);

            for (int i = 0; i < list.Count; i++)
            {
                var subscription = list[i];
                sb.Append(listname);
                sb.Append(",");
                sb.Append(subscription.Email);
                sb.Append(",");
                sb.Append(subscription.FirstName);
                sb.Append(",");
                sb.Append(subscription.LastName);
                sb.Append(",");
                sb.Append(subscription.Gender);
                sb.Append(",");
                sb.Append(subscription.Language);
                sb.Append(Environment.NewLine);
            }


            string result = sb.ToString();

            string data = String.Format("csv_file={0}", HttpUtility.UrlEncode(result.ToString()));

            HttpWebResponse resp = PostToMadMimi("/audience_members", data);
        }


        public class MadMimiItem
        {
            public string Email { get; set; }

            public string FirstName { get; set; }

            public string LastName { get; set; }

            public string Gender { get; set; }

            public string Language { get; set; }

        }


        public class MadMimiItemComparer : EqualityComparer<MadMimiItem>
        {
            public override int GetHashCode(MadMimiItem obj)
            {
                return obj.Email.GetHashCode();
            }

            public override bool Equals(MadMimiItem a1, MadMimiItem a2)
            {
                return a1.Email.ToLower().Trim() == a2.Email.ToLower().Trim();
            }
        }


        private string GetMadMimiMailerURL(string purl)
        {
            return String.Format("{0}{1}", "https://api.madmimi.com/mailer", purl);
        }


        private string GetMadMimiURL(string purl)
        {
            return String.Format("{0}{1}", "http://api.madmimi.com", purl);
        }


        private HttpWebResponse PostToMadMimi(string url, string data, bool useMailer)
        {
            string username = _settingService.GetSettingByKey<string>("madmimi.username", "");
            string apikey = _settingService.GetSettingByKey<string>("madmimi.apikey", "");

            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(
                    (useMailer) ? GetMadMimiMailerURL(url) : GetMadMimiURL(url));
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";

                string reqStr = String.Format("username={0}&api_key={1}&{2}", username, apikey, data);
                req.ContentLength = reqStr.Length;

                using (StreamWriter reqStream = new StreamWriter(req.GetRequestStream(), System.Text.Encoding.ASCII))
                {
                    reqStream.Write(reqStr);
                    reqStream.Close();
                }

                return (HttpWebResponse)(req.GetResponse());
            }
            catch (Exception exc)
            {
                var logger = EngineContext.Current.Resolve<ILogger>();
                logger.Error(exc.Message, exc, null);
                return null;
            }
        }


        private HttpWebResponse PostToMadMimi(string url, string data)
        {
            return PostToMadMimi(url, data, false);
        }


	}
}
