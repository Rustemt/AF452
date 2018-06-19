using System.Linq;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain.News;
using Nop.Services.AFServices;
using Nop.Services.Customers;
using Nop.Services.Media;
using Nop.Web.Extensions;
using Nop.Web.Models.Common;
using Nop.Services.News;
using System.Collections.Generic;
using Nop.Services.Localization;
using Nop.Services.Directory;
using Nop.Services.Messages;
using System;
using Nop.Web.Models.Newsletter;
using Nop.Core.Domain.Messages;
using Nop.Services.Catalog;
using Nop.Services.ExportImport;
using Nop.Core.Domain.Catalog;
using Nop.Web.Framework.Mvc;
using AlternativeDataAccess;
using Nop.Core.Caching;
using Nop.Web.Infrastructure.Cache;
using Nop.Core.Infrastructure;



namespace Nop.Web.Controllers
{
    public class HomeController : BaseNopController
    {
        #region Constants
        //caching by category, language, currency
        //private const string NEWS_MODEL_BY_TYPE_NEWS = "Nop.NewsItem.lgg-{0}.NewsType-{1}.Top-{2}.NewsItemPictureType-{3}";
        //private const string NEWS_MODEL_BY_TYPE_HOMEMAINCONTENT = "Nop.HomeMainContent.lgg-{0}.NewsType-{1}.Top-{2}";
        //private const string NEWS_MODEL_BY_TYPE_HOMEBOTTOMCONTENT = "Nop.HomeMainContent.lgg-{0}.NewsType-{1}.Top-{2}";
        #endregion

        private readonly IPictureService _pictureService;
        private readonly IContentService _contentService;
        private readonly INewsService _newsService;
        private readonly IWorkContext _workContext;
        private readonly NewsSettings _newsSettings;
        private readonly ILocalizationService _localizationService;
        private readonly ICountryService _countryService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IProductService _productService;
        private readonly IExportManager _exportManager;
        private readonly ICacheManager _cacheManager;

        public HomeController(
            IPictureService pictureService,
            IContentService contentService,
            INewsService newsService,
            IWorkContext workContext,
            NewsSettings newsSettings,
            ILocalizationService localizationService,
            ICountryService countryService,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            IWorkflowMessageService workflowMessageService,
            IProductService productService,
            IExportManager exportManager,
            ICacheManager cacheManager
            )
        {
            this._pictureService = pictureService;
            this._contentService = contentService;
            this._newsService = newsService;
            this._workContext = workContext;
            this._newsSettings = newsSettings;
            this._localizationService = localizationService;
            this._countryService = countryService;
            this._newsLetterSubscriptionService = newsLetterSubscriptionService;
            this._workflowMessageService = workflowMessageService;
            this._productService = productService;
            this._exportManager = exportManager;
            this._cacheManager = EngineContext.Current.ContainerManager.Resolve<ICacheManager>("nop_cache_static");
        }

        #region Remote Validation

        public ActionResult CampaignEmailCheck(string CampaignMail)
        {
            var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmail(CampaignMail);
            if (subscription != null && subscription.FirstName != null && subscription.RegistrationType == "Campaign")
            {
                return Json(_localizationService.GetResource("Campaign.Exist.MailAddress"), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }

        }
        
        public ActionResult  EmailCheck1(string Email1)
        {
            var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmail(Email1);
            if (subscription != null && subscription.FirstName != null && subscription.RegistrationType == "Campaign")
            {
                return Json(_localizationService.GetResource("Campaign.Exist.MailAddress"), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult EmailCheck2(string Email2, string Email1)
        {
            var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmail(Email2);
            if (subscription != null && subscription.FirstName != null && subscription.RegistrationType == "Campaign")
            {
                return Json(_localizationService.GetResource("Campaign.Exist.MailAddress"), JsonRequestBehavior.AllowGet);
            }
            else if (Email1 == Email2)
            {

                return Json(_localizationService.GetResource("Common.NotCompare.Email"), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult EmailCheck3(string Email3,string Email2, string Email1)
        {
            var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmail(Email3);
            if (subscription != null && subscription.FirstName != null && subscription.RegistrationType == "Campaign")
            {
                return Json(_localizationService.GetResource("Campaign.Exist.MailAddress"), JsonRequestBehavior.AllowGet);
            }
            else if (Email3 == Email2 || Email3 == Email1)
            {
                return Json(_localizationService.GetResource("Common.NotCompare.Email"), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult EmailCheck4(string Email4,string Email3, string Email2, string Email1)
        {
            var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmail(Email4);
            if (subscription != null && subscription.FirstName != null && subscription.RegistrationType == "Campaign")
            {
                return Json(_localizationService.GetResource("Campaign.Exist.MailAddress"), JsonRequestBehavior.AllowGet);
            }
            else if (Email4 == Email3 || Email4 == Email2 || Email4 == Email1)
            {
                return Json(_localizationService.GetResource("Common.NotCompare.Email"), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult EmailCheck5(string Email5, string Email4, string Email3, string Email2, string Email1)
        {
            var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmail(Email5);
            if (subscription != null && subscription.FirstName != null && subscription.RegistrationType == "Campaign")
            {
                return Json(_localizationService.GetResource("Campaign.Exist.MailAddress"), JsonRequestBehavior.AllowGet);
            }
            else if(Email5 == Email4 || Email5 ==Email3 || Email5 == Email2 || Email5 == Email1)
            {
                return Json(_localizationService.GetResource("Common.NotCompare.Email"), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Index

        //[OutputCache(Duration = int.MaxValue, VaryByCustom = "lgg")]
        public ActionResult Index()
        {
            return RedirectToRoute("HomePage");

            //var maincontentProductCategories = _contentService.GetContent(ContentType.MainContent.ToContentString());
            //var mainContentProducts = maincontentProductCategories.Select(x => x.Product);
            //var mainContentItems = mainContentProducts.ToContentItems(_pictureService);
            
            IEnumerable<ContentItemModel> mainContentItemsNews;
            var mainNews = _newsService.GetAllNews(_workContext.WorkingLanguage.Id, null, null, NewsType.HomeMainContent, 0,(_newsSettings.MainPageContentsCount!= 0) ? _newsSettings.MainPageContentsCount : 1);
            mainContentItemsNews = mainNews.ToContentItems(_pictureService);
            
            IEnumerable<ContentItemModel> bottomContentItemsNews;

            //var bottomContentProductCategoies = _contentService.GetContent(ContentType.PromoContent.ToContentString(), count: 3);
            //var bottomContentProducts = bottomContentProductCategoies.Select(x => x.Product);
            //bottomContentItemsNews = bottomContentProducts.ToContentItems(_pictureService);

            var bottomNews = _newsService.GetAllNews(_workContext.WorkingLanguage.Id, null, null, NewsType.HomeBottomContent, 0, 3);
            bottomContentItemsNews = bottomNews.ToContentItems(_pictureService);


            var model = new HomeModel() { MainContents = mainContentItemsNews.ToList(), BottomContents = bottomContentItemsNews.ToList(), IsGuest = _workContext.CurrentCustomer.IsGuest(), };

            return View(model);
        }


        //[OutputCache(Duration = int.MaxValue, VaryByCustom = "lgg")]
		public ActionResult Index0310()
		{
            if (Request.Params["cc"] != null && Request.Params["cc"].ToString().ToLowerInvariant().Equals("true"))
            {
                _cacheManager.RemoveByPattern(ModelCacheEventConsumer.AF_HOME_PATTERN_KEY);

                _cacheManager.Remove(ModelCacheEventConsumer.CATEGORIES_KEY);
                _cacheManager.RemoveByPattern(ModelCacheEventConsumer.CATEGORIES_KEY);
                _cacheManager.Remove(ModelCacheEventConsumer.CATEGORY_PICTURES_KEY);
                _cacheManager.RemoveByPattern(ModelCacheEventConsumer.CATEGORY_PICTURES_KEY);
                _cacheManager.RemoveByPattern(ModelCacheEventConsumer.CATEGORY_MANUFACTURES_PATTERN_KEY);
                _cacheManager.Remove(ModelCacheEventConsumer.HOMEPAGE_MANUFACTURES_BANNER_KEY);
                _cacheManager.RemoveByPattern(ModelCacheEventConsumer.HOMEPAGE_MANUFACTURES_BANNER_KEY);
                _cacheManager.Remove(ModelCacheEventConsumer.HOME_PAGE_PRODUCTS);
                _cacheManager.RemoveByPattern(ModelCacheEventConsumer.HOME_PAGE_PRODUCTS_PATTERN_KEY);

                //var cacheManager = new MemoryCacheManager();
                //cacheManager.Remove
            }               

			NewsRepository nr = new NewsRepository();
            //ViewBag.NewItemsHomeMainContent = nr.NewItemsBySystemType(_workContext.WorkingLanguage.Id, NewsType.HomeMainContent, 4);
            //ViewBag.NewItems = nr.NewItemsBySystemType(_workContext.WorkingLanguage.Id, NewsType.News, 3, NewsItemPictureType.Thumb);
            ViewBag.NewItemsHomeMainContent = _cacheManager.Get(string.Format(ModelCacheEventConsumer.NEWS_MODEL_BY_TYPE_NEWS, _workContext.WorkingLanguage.Id, (int)NewsType.HomeMainContent, 4),
                3600, () => nr.NewItemsBySystemType(_workContext.WorkingLanguage.Id, NewsType.HomeMainContent, 4));
            ViewBag.NewItems = _cacheManager.Get(string.Format(ModelCacheEventConsumer.NEWS_MODEL_BY_TYPE_NEWS, _workContext.WorkingLanguage.Id, (int)NewsType.News, 3),
                3600, () => nr.NewItemsBySystemType(_workContext.WorkingLanguage.Id, NewsType.News, 3, NewsItemPictureType.Thumb));
			

            //var bottomNews = _newsService.GetAllNews(_workContext.WorkingLanguage.Id, null, null, NewsType.HomeBottomContent, 0, 5);
            //ViewBag.BottomContentItemsNews = bottomNews.ToContentItems(_pictureService);
            var bottomNews = _cacheManager.Get(string.Format(ModelCacheEventConsumer.NEWS_MODEL_BY_TYPE_HOMEBOTTOMCONTENT, _workContext.WorkingLanguage.Id),
                3600, () => _newsService.GetAllNews(_workContext.WorkingLanguage.Id, null, null, NewsType.HomeBottomContent, 0, 5));
            ViewBag.BottomContentItemsNews = bottomNews.ToContentItems(_pictureService);


            //var bottomSlider = _newsService.GetAllNews(_workContext.WorkingLanguage.Id, null, null, NewsType.HomeSlider, 0, 10);
            //ViewBag.BottomSliderContentItems = bottomSlider.ToContentItems(_pictureService);
            var bottomSlider = _cacheManager.Get(string.Format(ModelCacheEventConsumer.NEWS_MODEL_BY_TYPE_HOMESLIDER, _workContext.WorkingLanguage.Id),
                3600, () => _newsService.GetAllNews(_workContext.WorkingLanguage.Id, null, null, NewsType.HomeSlider, 0, 10)); 
            ViewBag.BottomSliderContentItems = bottomSlider.ToContentItems(_pictureService);


            //var homeManufacturers = _newsService.GetAllNews(2/*TR*/, null, null, NewsType.HomeManufacturers, 0, 10);
            //ViewBag.HomeManufacturers = homeManufacturers.ToContentItems(_pictureService);
            var homeManufacturers = _cacheManager.Get(string.Format(ModelCacheEventConsumer.NEWS_MODEL_BY_TYPE_HOMEMANUFACTURER, _workContext.WorkingLanguage.Id),
                3600, () => _newsService.GetAllNews(2/*TR*/, null, null, NewsType.HomeManufacturers, 0, 10)); 
            ViewBag.HomeManufacturers = homeManufacturers.ToContentItems(_pictureService);


            //var homeContentBanner = _newsService.GetAllNews(_workContext.WorkingLanguage.Id, null, null, NewsType.HomeContentBanner, 0, 1);
            //ViewBag.HomeContentBanner = homeContentBanner.ToContentItems(_pictureService);
            var homeContentBanner = _cacheManager.Get(string.Format(ModelCacheEventConsumer.NEWS_MODEL_BY_TYPE_HOMECONTENTBANNER, _workContext.WorkingLanguage.Id),
                3600, () => _newsService.GetAllNews(_workContext.WorkingLanguage.Id, null, null, NewsType.HomeContentBanner, 0, 1)); 
            ViewBag.HomeContentBanner = homeContentBanner.ToContentItems(_pictureService);

            
			return View();
		}
        #endregion

        #region Campaign

        public ActionResult CampaignRegister()
        {
            var model = new Nop.Web.Models.Newsletter.NewsletterBoxModel();
            model.Genders = new List<SelectListItem>();
            model.Genders.Add(new SelectListItem() { Text = _localizationService.GetResource("Account.Fields.Gender.Female"), Value = "F" });
            model.Genders.Add(new SelectListItem() { Text = _localizationService.GetResource("Account.Fields.Gender.Male"), Value = "M" });
            model.AvailableCountries = new List<SelectListItem>();

            foreach (var c in _countryService.GetAllCountries())
            {
                    model.AvailableCountries.Add(new SelectListItem() { Text = c.Name, Value = c.Id.ToString(), Selected = c.DisplayOrder == 1 });
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult CampaignRegister(NewsletterBoxModel model)
        {
            var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmail(model.CampaignMail);

            if (model.CampaignMail != null)
                {
                    if (subscription == null)
                    {
                        NewsLetterSubscription subscriptionmodel = new NewsLetterSubscription();            // İlk üyelik için. tüm alanlar doldurulur.
                        subscriptionmodel.Active = false;
                        subscriptionmodel.CountryId = model.CountryId;
                        subscriptionmodel.CreatedOnUtc = DateTime.Now;
                        subscriptionmodel.Email = model.CampaignMail;
                        subscriptionmodel.FirstName = model.FirstName;
                        subscriptionmodel.LastName = model.LastName;
                        subscriptionmodel.Gender = model.Gender;
                        subscriptionmodel.LanguageId = _workContext.WorkingLanguage.Id;
                        subscriptionmodel.RegistrationType = "Campaign";
                        subscriptionmodel.NewsLetterSubscriptionGuid = Guid.NewGuid();
                        _newsLetterSubscriptionService.InsertNewsLetterSubscription(subscriptionmodel);
                        _workflowMessageService.SendCampaignRegisterActivationMessage(subscriptionmodel, _workContext.WorkingLanguage.Id);
                        return RedirectToAction("CampaignFriendsConvoke", new { id = subscriptionmodel.Id });
                    }

                    else if (subscription != null && subscription.RegistrationType != "Campaign")   // Önceden newsletterdaki kayıtlar için
                    {
                        subscription.Active = true;
                        subscription.Gender = model.Gender;
                        subscription.CountryId = model.CountryId;
                        subscription.FirstName = model.FirstName;
                        subscription.LastName = model.LastName;
                        subscription.RegistrationType = "Campaign";
                        subscription.LanguageId = _workContext.WorkingLanguage.Id;
                        _newsLetterSubscriptionService.UpdateNewsLetterSubscription(subscription);
                        //_workflowMessageService.SendCampaignRegisterActivationMessage(subscription, _workContext.WorkingLanguage.Id);
                        return RedirectToAction("CampaignFriendsConvoke", new { id = subscription.Id });
                    }

                    else    // Davetiye maili ile gelenler için
                    {
                        subscription.Email = model.CampaignMail;
                        subscription.Active = false;
                        subscription.CountryId = model.CountryId;
                        subscription.LanguageId = _workContext.WorkingLanguage.Id;
                        subscription.Gender = model.Gender;
                        subscription.FirstName = model.FirstName;
                        subscription.LastName = model.LastName;
                        subscription.RegistrationType = "Campaign";
                        _newsLetterSubscriptionService.UpdateNewsLetterSubscription(subscription);
                        _workflowMessageService.SendCampaignRegisterActivationMessage(subscription, _workContext.WorkingLanguage.Id);
                        return RedirectToAction("CampaignFriendsConvoke", new { id = subscription.Id });
                    }

                }
                  if(subscription != null)
                return RedirectToRoute("CampaingRegisterConvoke", new { id = subscription.Id }); 
                  else
                      return RedirectToRoute("HomePage");
        }  // İlk kayıt ekranı

        public ActionResult CampaignFriendsConvoke()                ///Davet Formu
        {
            return View();
        }        
        
        [HttpPost]
        public ActionResult CampaignFriendsConvoke(NewsletterBoxModel model)  // Davet edilen maillerin kaydı
        {
            string  Id = RouteData.Values["id"].ToString();
            if (Id == null)
            { 
                RedirectToAction("Index");
            }

            var referermodel = _newsLetterSubscriptionService.GetNewsLetterSubscriptionById(int.Parse(Id));

            if (model.Email1 != null)
            {
                var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmail(model.Email1);
                if (subscription == null)
                {
                    NewsLetterSubscription newsletterModel = new NewsLetterSubscription();
                    newsletterModel.Email = model.Email1;
                    newsletterModel.CreatedOnUtc = DateTime.Now;
                    newsletterModel.RegistrationType = "Campaign";
                    newsletterModel.NewsLetterSubscriptionGuid = Guid.NewGuid();
                    if (referermodel != null)
                    {
                        newsletterModel.RefererEmail = referermodel.Email;
                    }
                    _newsLetterSubscriptionService.InsertNewsLetterSubscription(newsletterModel);
                    _workflowMessageService.SendCampaignConvoke(newsletterModel, _workContext.WorkingLanguage.Id);

                }
            }

            if (model.Email2 != null)
            {
                var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmail(model.Email2);
                if (subscription == null)
                {
                    NewsLetterSubscription newsletterModel = new NewsLetterSubscription();
                    newsletterModel.Email = model.Email2;
                    newsletterModel.CreatedOnUtc = DateTime.Now;
                    newsletterModel.RegistrationType = "Campaign";
                    newsletterModel.NewsLetterSubscriptionGuid = Guid.NewGuid();
                    if (referermodel != null)
                    {
                        newsletterModel.RefererEmail = referermodel.Email;
                    }
                    _newsLetterSubscriptionService.InsertNewsLetterSubscription(newsletterModel);
                    _workflowMessageService.SendCampaignConvoke(newsletterModel, _workContext.WorkingLanguage.Id);
                }
            }

            if (model.Email3 != null)
            {
                var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmail(model.Email3);
                if (subscription == null)
                {
                    NewsLetterSubscription newsletterModel = new NewsLetterSubscription();
                    newsletterModel.Email = model.Email3;
                    newsletterModel.CreatedOnUtc = DateTime.Now;
                    newsletterModel.RegistrationType = "Campaign";
                    newsletterModel.NewsLetterSubscriptionGuid = Guid.NewGuid();
                    if (referermodel != null)
                    {
                        newsletterModel.RefererEmail = referermodel.Email;
                    }
                    _newsLetterSubscriptionService.InsertNewsLetterSubscription(newsletterModel);
                    _workflowMessageService.SendCampaignConvoke(newsletterModel, _workContext.WorkingLanguage.Id);
                }
            }

            if (model.Email4 != null)
            {
                var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmail(model.Email4);
                if (subscription == null)
                {
                    NewsLetterSubscription newsletterModel = new NewsLetterSubscription();
                    newsletterModel.Email = model.Email4;
                    newsletterModel.CreatedOnUtc = DateTime.Now;
                    newsletterModel.RegistrationType = "Campaign";
                    newsletterModel.NewsLetterSubscriptionGuid = Guid.NewGuid();
                    if (referermodel != null)
                    {
                        newsletterModel.RefererEmail = referermodel.Email;
                    }
                    _newsLetterSubscriptionService.InsertNewsLetterSubscription(newsletterModel);
                    _workflowMessageService.SendCampaignConvoke(newsletterModel, _workContext.WorkingLanguage.Id);
                }
            }

            if (model.Email5 != null)
            {
                var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmail(model.Email5);
                if (subscription == null)
                {
                    NewsLetterSubscription  newsletterModel = new NewsLetterSubscription();
                    newsletterModel.Email = model.Email5;
                    newsletterModel.CreatedOnUtc = DateTime.Now;
                    newsletterModel.RegistrationType = "Campaign";
                    newsletterModel.NewsLetterSubscriptionGuid = Guid.NewGuid();
                    if (referermodel != null)
                    {
                        newsletterModel.RefererEmail = referermodel.Email;
                    }
                    _newsLetterSubscriptionService.InsertNewsLetterSubscription(newsletterModel);
                    _workflowMessageService.SendCampaignConvoke(newsletterModel, _workContext.WorkingLanguage.Id);
                }
            }

            return RedirectToRoute("CampaignRecordSuccessFul");      //("CampaignRecordSuccessFul","Home");
        }

        public ActionResult CampaignRecordSuccessFul()
        {
            return View();
        }  //aktivasyon gönderildi Mesajı

        [HttpGet]
        public ActionResult CampaignRegisterActive(int id)   //Aktiflik metodu
        {
            if (id == null)
                return RedirectToAction("Index");

            var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionById(id);

            if (subscription != null)     //Üyeligi aktif etmek için
            {
                subscription.Active = true;
                _newsLetterSubscriptionService.UpdateNewsLetterSubscription(subscription);
            }
            else
            {
                return RedirectToAction("Index");
            }

            return RedirectToRoute("CampaignRecordCompleted");
        }      

        public ActionResult RegistedCompleted()
        {
            return View();
        }

        #endregion

        #region  Product Feed

        public ActionResult ExportRetargetingProductFeed()
        {
            if (_workContext.CurrentCustomer.IsGuest())
                return new HttpUnauthorizedResult();

            //try
            //{
                var products = _productService.SearchProducts(0, 0, null, null, null, 0, string.Empty, false,
                    _workContext.WorkingLanguage.Id, new List<int>(),
                    ProductSortingEnum.Position, 0, int.MaxValue, true).Where(x => x.Published == true).Where(x=>x.Deleted==false).ToList();

                var fileName = string.Format("RetargetingProducts_{0}.xml", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));
                var xml = _exportManager.RetargetingExportProductsToXml(products);
                return new XmlDownloadResult(xml, fileName);
            //}
            //catch (Exception exc)
            //{
            //    ErrorNotification(exc);
            //    return RedirectToRoute("HomePage");
            //}
        }

        #endregion

    }
}

