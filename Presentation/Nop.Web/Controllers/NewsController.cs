using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web.Caching;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.News;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.News;
using Nop.Web.Extensions;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;
using Nop.Web.Models.News;
using Nop.Services.Seo;
using Nop.Web.Models.Common;
using AlternativeDataAccess;
using Nop.Core.Caching;
using Nop.Core.Infrastructure;
using Nop.Web.Infrastructure.Cache;

namespace Nop.Web.Controllers
{
    public class NewsController : BaseNopController
    {
        #region Fields

        private readonly INewsService _newsService;
        private readonly IWorkContext _workContext;
        private readonly IPictureService _pictureService;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerContentService _customerContentService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IWebHelper _webHelper;
        private readonly MediaSettings _mediaSettings;
        private readonly NewsSettings _newsSettings;
        private readonly LocalizationSettings _localizationSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly StoreInformationSettings _storeInformationSettings;
        private readonly MediaSettings _mediaSetting;
        private readonly IExtraContentService _extraContentService;
        private readonly ICacheManager _cacheManager;
       
        #endregion

        #region Constructors

        public NewsController(INewsService newsService,
            IWorkContext workContext, IPictureService pictureService, ILocalizationService localizationService,
            ICustomerContentService customerContentService, IDateTimeHelper dateTimeHelper,
            IWorkflowMessageService workflowMessageService, IWebHelper webHelper,
            MediaSettings mediaSettings, NewsSettings newsSettings,
            LocalizationSettings localizationSettings, CustomerSettings customerSettings,
            StoreInformationSettings storeInformationSettings, MediaSettings mediaSetting, IExtraContentService extraContentService, ICacheManager cacheManager)
        {
            this._newsService = newsService;
            this._workContext = workContext;
            this._pictureService = pictureService;
            this._localizationService = localizationService;
            this._customerContentService = customerContentService;
            this._dateTimeHelper = dateTimeHelper;
            this._workflowMessageService = workflowMessageService;
            this._webHelper = webHelper;
            this._mediaSettings = mediaSettings;
            this._newsSettings = newsSettings;
            this._localizationSettings = localizationSettings;
            this._customerSettings = customerSettings;
            this._storeInformationSettings = storeInformationSettings;
            this._mediaSetting = mediaSetting;
            this._extraContentService = extraContentService;
            this._cacheManager = EngineContext.Current.ContainerManager.Resolve<ICacheManager>("nop_cache_static");
        }

        #endregion

        #region Utilities

        [NonAction]
        private void PrepareNewsItemListModel(NewsItemModel model, NewsItem newsItem, bool prepareComments, NewsItemPictureType newsItemPictureType, bool getCommentCount = true)
        {
            if (newsItem == null)
                throw new ArgumentNullException("newsItem");

            if (model == null)
                throw new ArgumentNullException("model");

            model.Id = newsItem.Id;
            model.SeName = newsItem.GetSeName();
            model.Title = newsItem.Title;
            model.Short = newsItem.Short;
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(newsItem.CreatedOnUtc, DateTimeKind.Utc);
			if (getCommentCount)
				model.NumberOfComments = newsItem.NewsComments.Count;
            model.SystemType = newsItem.SystemTypeId;
            model.Full = newsItem.Full;
            //pictures
            var pictures = _newsService.GetNewsItemPicturesByNewsItemId(newsItem.Id, newsItemPictureType);
            if (pictures.Count > 0)
            {
                var picture = pictures.FirstOrDefault();
                int pictureSize = 0;
                if (newsItemPictureType == NewsItemPictureType.Main)
                    pictureSize = _mediaSettings.NewsItemPictureSize;
                else if (newsItemPictureType == NewsItemPictureType.Thumb)
                    pictureSize = _mediaSettings.NewsItemThumbPictureSize;
                else
                    pictureSize = _mediaSettings.NewsItemDetailPictureSize;


                model.DefaultPictureModel = new PictureModel()
                    {
                        //ImageUrl = _pictureService.GetPictureUrl(picture.PictureId, pictureSize),
						ImageUrl = _pictureService.GetPictureUrl(_newsService.GetMediaPictureByNewsItemPictureId(picture.PictureId), pictureSize),
						FullSizeImageUrl = _pictureService.GetPictureUrl(_newsService.GetMediaPictureByNewsItemPictureId(picture.PictureId)),
						//FullSizeImageUrl = _pictureService.GetPictureUrl(picture.PictureId),
                        Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat"), model.Title),
                        AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat"), model.Title),
                    };
            }
            else
            {
                //no images. set the default one
                model.DefaultPictureModel = new PictureModel()
                {
                    ImageUrl = _pictureService.GetDefaultPictureUrl(_mediaSettings.NewsItemThumbPictureSize),
                    FullSizeImageUrl = _pictureService.GetDefaultPictureUrl(),
                    Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat"), model.Title),
                    AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat"), model.Title),
                };
            }
        }

        [NonAction]
        private void PrepareNewsItemDetailModel(NewsItemModel model, NewsItem newsItem, bool prepareComments, bool prepareProductModel = true)
        {
            if (newsItem == null)
                throw new ArgumentNullException("newsItem");

            if (model == null)
                throw new ArgumentNullException("model");

            model.Id = newsItem.Id;
            model.SeName = newsItem.GetSeName();
            model.Title = newsItem.Title;
            model.Short = newsItem.Short;
            model.Full = newsItem.Full;
            model.MetaDescription = newsItem.MetaDescription;
            model.MetaTitle = newsItem.MetaTitle;
            model.MetaKeywords = newsItem.MetaKeywords;
            model.AllowComments = newsItem.AllowComments;
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(newsItem.CreatedOnUtc, DateTimeKind.Utc);
			if (prepareComments)
				model.NumberOfComments = newsItem.NewsComments.Count;
            model.IsGuest = _workContext.CurrentCustomer.IsGuest();

            #region pictures

            var pictures = _newsService.GetNewsItemPicturesByNewsItemId(newsItem.Id, NewsItemPictureType.Standard);
            if (pictures.Count > 0)
            {
                foreach (var newsItemPicture in pictures)
                {
                    model.PictureModels.Add(new PictureModel()
                                                {
                                                    ImageUrl =
														_pictureService.GetPictureUrl(_newsService.GetMediaPictureByNewsItemPictureId(newsItemPicture.PictureId),
                                                                                      _mediaSettings.NewsItemDetailPictureSize),
                                                    FullSizeImageUrl = _pictureService.GetPictureUrl(_newsService.GetMediaPictureByNewsItemPictureId(newsItemPicture.PictureId)),
                                                    Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat"), model.Title),
                                                    AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat"), model.Title),

                                                });
                }
                model.DefaultPictureModel = model.PictureModels.FirstOrDefault();
            }
            else
            {
                //no images. set the default one
                model.DefaultPictureModel = new PictureModel()
                                                {
                                                    ImageUrl =
                                                        _pictureService.GetDefaultPictureUrl(
                                                            _mediaSettings.NewsItemDetailPictureSize),
                                                    FullSizeImageUrl = _pictureService.GetDefaultPictureUrl(),
                                                    Title =
                                                        string.Format(
                                                            _localizationService.GetResource(
                                                                "Media.Product.ImageLinkTitleFormat"), model.Title),
                                                    AlternateText =
                                                        string.Format(
                                                            _localizationService.GetResource(
                                                                "Media.Product.ImageAlternateTextFormat"), model.Title),
                                                };
            }

            #endregion pictures

            #region extra contents

            var extraContents = newsItem.ExtraContents.OrderBy(x => x.DisplayOrder).ToList();
            if (extraContents.Count > 0)
            {
                foreach (var newItemExtraContent in extraContents)
                {
                    model.ExtraContentModels.Add(new ExtraContentModel()
                    {
                        FullDescription = newItemExtraContent.FullDescription,
                        Id = newItemExtraContent.Id,
                        DisplayOrder = newItemExtraContent.DisplayOrder,
                        Title = newItemExtraContent.Title
                    });
                }
            }


       
            #endregion extra contents

            #region comments

            if (prepareComments)
            {
                var newsComments = newsItem.NewsComments.Where(pr => pr.IsApproved).OrderBy(pr => pr.CreatedOnUtc);
                foreach (var nc in newsComments)
                {
                    var commentModel = new NewsCommentModel()
                                           {
                                               Id = nc.Id,
                                               CustomerId = nc.CustomerId,
                                               CustomerName = nc.Customer.FormatUserName(),
                                               CommentTitle = nc.CommentTitle,
                                               CommentText = nc.CommentText,
                                               CreatedOn =
                                                   _dateTimeHelper.ConvertToUserTime(nc.CreatedOnUtc, DateTimeKind.Utc),
                                               AllowViewingProfiles =
                                                   _customerSettings.AllowViewingProfiles && nc.Customer != null &&
                                                   !nc.Customer.IsGuest(),
                                           };
                    if (_customerSettings.AllowCustomersToUploadAvatars)
                    {
                        var customer = nc.Customer;
                        string avatarUrl =
                            _pictureService.GetPictureUrl(
                                customer.GetAttribute<int>(SystemCustomerAttributeNames.AvatarPictureId),
                                _mediaSettings.AvatarPictureSize, false);
                        if (String.IsNullOrEmpty(avatarUrl) && _customerSettings.DefaultAvatarEnabled)
                            avatarUrl = _pictureService.GetDefaultPictureUrl(_mediaSettings.AvatarPictureSize,
                                                                             PictureType.Avatar);
                        commentModel.CustomerAvatarUrl = avatarUrl;
                    }
                    model.Comments.Add(commentModel);
                }
            }

            #endregion comments

            #region products

            if (prepareProductModel)
            {
				//ViewBag.NewsItemProductsSummary = _newsService.GetNewsItemProductsSummaryByNewsItemId(newsItem.Id);
				//CatalogRepository cr = new CatalogRepository();
				//var products = cr.ProductsByNewsItem(newsItem.Id);
				////var products = _newsService.GetNewsItemProductsByNewsItemId(newsItem.Id);
				//model.ProductModels = products.Select(x => PrepareProductOverviewModel(x)).ToList();
				CatalogRepository cr = new CatalogRepository();
				var products = cr.ProductsSummaryByNewsItem(newsItem.Id);
				foreach (var p in products)
					p.PictureUrl = _pictureService.GetPictureUrl(p.PictureId, 222);
				ViewBag.Products = products;
				
            }
            #endregion products


        }

        [NonAction]
        private ProductModel PrepareProductOverviewModel(Product product, bool preparePictureModel = true)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            //var model = product.ToModel();
			var model = new ProductModel { Id = product.Id, SeName = product.SeName, DefaultPictureModel = new PictureModel() };

            //picture
            if (preparePictureModel)
            {
				product.ProductVariants.FirstOrDefault().Product = product;
                var picture = product.GetProductPicture(_pictureService);
                if (picture != null)
                    model.DefaultPictureModel.ImageUrl = _pictureService.GetPictureUrl(picture, _mediaSetting.NewsItemProductPictureSize, true);
                else
                    model.DefaultPictureModel.ImageUrl = _pictureService.GetDefaultPictureUrl(_mediaSetting.NewsItemProductPictureSize);
                model.DefaultPictureModel.Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat"), model.Name);
                model.DefaultPictureModel.AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat"), model.Name);
            }
            return model;
        }

        #endregion


        #region Methods

        public ActionResult HomePageNews()
        {
            if (!_newsSettings.Enabled || !_newsSettings.ShowNewsOnMainPage)
                return Content("");

            var model = new NewsItemListModel();
            model.WorkingLanguageId = _workContext.WorkingLanguage.Id;

            var newsItems = _newsService.GetAllNews(_workContext.WorkingLanguage.Id,
                    null, null, 0, _newsSettings.MainPageNewsCount);

            model.MainNewsItems = newsItems
                .Select(x =>
                {
                    var newsModel = new NewsItemModel();
                    PrepareNewsItemListModel(newsModel, x, false, NewsItemPictureType.Main);
                    return newsModel;
                })
                .ToList();

            return PartialView(model);
        }

        //[OutputCache(Duration = int.MaxValue, VaryByCustom = "lgg")]
        public ActionResult List(NewsPagingFilteringModel command)
        {
            if (!_newsSettings.Enabled)
                return RedirectToAction("Index", "Home");

            var model = new NewsItemListModel();
            model.WorkingLanguageId = _workContext.WorkingLanguage.Id;

            if (command.PageSize <= 0) command.PageSize = _newsSettings.NewsArchivePageSize;
            if (command.PageNumber <= 0) command.PageNumber = 1;
            var DateStartMonth = DateTime.Now.AddMonths(-_newsSettings.NewsArchiveMonthSpan + 1);
            var date = new DateTime(DateStartMonth.Year, DateStartMonth.Month, 1);

            string cacheKey = string.Format(ModelCacheEventConsumer.AF_NEWS_PAGE, _workContext.WorkingLanguage.Id,
                    date, command.PageNumber - 1, command.PageSize);
            
            if (Request.Params["cc"] != null && Request.Params["cc"].ToString().ToLowerInvariant().Equals("true"))
            {
                _cacheManager.Remove(cacheKey);
            }

            var newsItems = _cacheManager.Get(cacheKey, 3600, () =>
            {
                return _newsService.GetAllNews(_workContext.WorkingLanguage.Id,
                    date, null, NewsType.News | NewsType.Interview, command.PageNumber - 1, command.PageSize);
            });

            model.PagingFilteringContext.LoadPagedList(newsItems);
            model.IsGuest = _workContext.CurrentCustomer.IsGuest();

            if(newsItems.Count==0)
                return RedirectToAction("Index", "Home");



            model.MainNewsItems = newsItems.Where(x=>x.IsFeatured).Take(_newsSettings.MainNewsCount).Select(x =>
            {
                var newsModel = new NewsItemModel();
                PrepareNewsItemListModel(newsModel, x, false, NewsItemPictureType.Main, false);
                return newsModel;
            })
                .OrderBy(x=>x.DisplayOrder).ToList();

            model.MonthlyNewsItems = newsItems.Select(x =>
            {
                var newsModel = new NewsItemModel();
                PrepareNewsItemListModel(newsModel, x, false, NewsItemPictureType.Thumb, false);
                return newsModel;
            }).ToList().GroupBy(x => x.CreatedOn.ToString("MMMM").ToUpper() + " " + x.CreatedOn.Year).ToList();




            return View(model);
        }

        public ActionResult ListRss(int languageId)
        {
            var feed = new SyndicationFeed(
                                    string.Format("{0}: News", _storeInformationSettings.StoreName),
                                    "News",
                                    new Uri(_webHelper.GetStoreLocation(false)),
                                    "NewsRSS",
                                    DateTime.UtcNow);

            if (!_newsSettings.Enabled)
                return new RssActionResult() { Feed = feed };

            var items = new List<SyndicationItem>();
            var newsItems = _newsService.GetAllNews(languageId,
                null, null, 0, int.MaxValue);
            foreach (var n in newsItems)
            {
                string newsUrl = Url.RouteUrl("NewsItem", new { newsItemId = n.Id, SeName = n.GetSeName() }, "http");
                items.Add(new SyndicationItem(n.Title, n.Short, new Uri(newsUrl), String.Format("Blog:{0}", n.Id), n.CreatedOnUtc));
            }
            feed.Items = items;
            return new RssActionResult() { Feed = feed };
        }

        //[OutputCache(Duration=int.MaxValue,VaryByParam="newsItemId")]
        public ActionResult NewsItem(int newsItemId)
        {
            if (!_newsSettings.Enabled)
                return RedirectToAction("Index", "Home");

            var newsItem = _newsService.GetNewsById(newsItemId);
            
            if (newsItem == null || (!newsItem.Published && !_workContext.CurrentCustomer.IsAdmin()))
                return RedirectToAction("Index", "Home");

            if (newsItem.LanguageId != _workContext.WorkingLanguage.Id)
                return RedirectToAction("Index", "Home");

            var newsItems = _newsService.GetAllNews(newsItem.LanguageId, null, null, NewsType.News, 0, int.MaxValue);
            int index = newsItems.IndexOf(newsItem);

            NewsItem previousNewsItem = null, nextNewsItem = null;
            if (index > 0)
                previousNewsItem = newsItems.ElementAt(index - 1);
            if (index < newsItems.Count - 1)
                nextNewsItem = newsItems.ElementAt(index + 1);


            var model = new NewsItemModel();
            PrepareNewsItemDetailModel(model, newsItem, false);

            if (previousNewsItem != null)
            {
                model.PreviousNewsItemId = previousNewsItem.Id;
                model.PreviousNewsItemSeName = previousNewsItem.GetSeName();
                //image url
                var newsItempicture = _newsService.GetNewsItemPicturesByNewsItemId(previousNewsItem.Id, NewsItemPictureType.Thumb).FirstOrDefault();
                if (newsItempicture != null)
                    model.PreviousNewsItemPictureUrl = _pictureService.GetPictureUrl(newsItempicture.PictureId, _mediaSettings.NewsItemThumbPictureSize);


            }

            if (nextNewsItem != null)
            {
                model.NextNewsItemId = nextNewsItem.Id;
                model.NextNewsItemSeName = nextNewsItem.GetSeName();
                //image url
                var newsItempicture = _newsService.GetNewsItemPicturesByNewsItemId(nextNewsItem.Id, NewsItemPictureType.Thumb).FirstOrDefault();
                if (newsItempicture != null)
                    model.NextNewsItemPictureUrl = _pictureService.GetPictureUrl(newsItempicture.PictureId, _mediaSettings.NewsItemThumbPictureSize);
            }

            return View(model);
        }

        [OutputCache(Duration = int.MaxValue, VaryByParam = "newsItemId")]
        public ActionResult InterviewItem(int newsItemId)
        {
            if (!_newsSettings.Enabled)
                return RedirectToAction("Index", "Home");

            var newsItem = _newsService.GetNewsById(newsItemId);
            if (newsItem == null || !newsItem.Published)
                return RedirectToAction("Index", "Home");
            var newsItems = _newsService.GetAllNews(newsItem.LanguageId, null, null, NewsType.Interview, 0, int.MaxValue);
            int index = newsItems.IndexOf(newsItem);

            NewsItem previousNewsItem = null, nextNewsItem = null;
            if (index > 0)
                previousNewsItem = newsItems.ElementAt(index - 1);
            if (index < newsItems.Count - 1)
                nextNewsItem = newsItems.ElementAt(index + 1);


            var model = new NewsItemModel();
            PrepareNewsItemDetailModel(model, newsItem, false, false);

            if (previousNewsItem != null)
            {
                model.PreviousNewsItemId = previousNewsItem.Id;
                model.PreviousNewsItemSeName = previousNewsItem.GetSeName();
                //image url
                var newsItempicture = _newsService.GetNewsItemPicturesByNewsItemId(previousNewsItem.Id, NewsItemPictureType.Thumb).FirstOrDefault();
                if (newsItempicture != null)
                    model.PreviousNewsItemPictureUrl = _pictureService.GetPictureUrl(newsItempicture.PictureId, _mediaSettings.NewsItemThumbPictureSize);


            }

            if (nextNewsItem != null)
            {
                model.NextNewsItemId = nextNewsItem.Id;
                model.NextNewsItemSeName = nextNewsItem.GetSeName();
                //image url
                var newsItempicture = _newsService.GetNewsItemPicturesByNewsItemId(nextNewsItem.Id, NewsItemPictureType.Thumb).FirstOrDefault();
                if (newsItempicture != null)
                    model.NextNewsItemPictureUrl = _pictureService.GetPictureUrl(newsItempicture.PictureId, _mediaSettings.NewsItemThumbPictureSize);
            }

            return View(model);
        }

        [HttpPost, ActionName("NewsItem")]
        [FormValueRequired("add-comment")]
        public ActionResult NewsCommentAdd(int newsItemId, NewsItemModel model)
        {
            if (!_newsSettings.Enabled)
                return RedirectToAction("Index", "Home");

            var newsItem = _newsService.GetNewsById(newsItemId);
            if (newsItem == null || !newsItem.Published || !newsItem.AllowComments)
                return RedirectToAction("Index", "Home");

            if (ModelState.IsValid)
            {
                if (_workContext.CurrentCustomer.IsGuest() && !_newsSettings.AllowNotRegisteredUsersToLeaveComments)
                {
                    ModelState.AddModelError("", _localizationService.GetResource("News.Comments.OnlyRegisteredUsersLeaveComments"));
                }
                else
                {
                    var comment = new NewsComment()
                    {
                        NewsItemId = newsItem.Id,
                        CustomerId = _workContext.CurrentCustomer.Id,
                        IpAddress = _webHelper.GetCurrentIpAddress(),
                        CommentTitle = model.AddNewComment.CommentTitle,
                        CommentText = model.AddNewComment.CommentText,
                        IsApproved = true,
                        CreatedOnUtc = DateTime.UtcNow,
                        UpdatedOnUtc = DateTime.UtcNow,
                    };
                    _customerContentService.InsertCustomerContent(comment);

                    //notify store owner
                    if (_newsSettings.NotifyAboutNewNewsComments)
                        _workflowMessageService.SendNewsCommentNotificationMessage(comment, _localizationSettings.DefaultAdminLanguageId);


                    PrepareNewsItemDetailModel(model, newsItem, true);
                    model.AddNewComment.CommentText = null;
                    model.AddNewComment.Result = _localizationService.GetResource("News.Comments.SuccessfullyAdded");

                    return View(model);
                }
            }

            //If we got this far, something failed, redisplay form
            PrepareNewsItemDetailModel(model, newsItem, true);
            return View(model);
        }

        [ChildActionOnly]
        public ActionResult RssHeaderLink()
        {
            if (!_newsSettings.Enabled || !_newsSettings.ShowHeaderRssUrl)
                return Content("");

            string link = string.Format("<link href=\"{0}\" rel=\"alternate\" type=\"application/rss+xml\" title=\"{1}: News\" />",
                Url.RouteUrl("NewsRSS", new { languageId = _workContext.WorkingLanguage.Id }, "http"), _storeInformationSettings.StoreName);

            return Content(link);
        }

        [ChildActionOnly]
        public ActionResult NewsAnnouncement(string view = "")
        {
            if (!_newsSettings.Enabled || !_newsSettings.ShowNewsOnMainPage || view != "NewsAnnouncementNew")
                return Content("");
            //var parentaction = controllercontext.parentactionviewcontext.routedata.values["action"].tostring();
            //var parentcontroller = controllercontext.parentactionviewcontext.routedata.values["controller"].tostring();
            //var allowedactions = new list<string>(){"index"};
            //if()

            int displayCount = 0;
            var displayCountCookie = HttpContext.Request.Cookies.Get("news.DisplayCount");
            if (displayCountCookie != null)
            {
                int.TryParse(displayCountCookie.Value, out displayCount);
            }

            int newsAnnouncementId = 0;
            var newsAnnouncementIdCookie = HttpContext.Request.Cookies.Get("news.NewsAnnouncementId");
            if (newsAnnouncementIdCookie != null)
            {
                int.TryParse(newsAnnouncementIdCookie.Value, out newsAnnouncementId);
            }

          

            var model = new NewsItemListModel();
            model.WorkingLanguageId = _workContext.WorkingLanguage.Id;
            var newsItems = _newsService.GetAllNews(_workContext.WorkingLanguage.Id,
                    null, null, NewsType.BannerNews, 0, _newsSettings.MainPageNewsCount);
            model.MainNewsItems = newsItems
                .Select(x =>
                {
                    var newsModel = new NewsItemModel();
                    PrepareNewsItemListModel(newsModel, x, false, NewsItemPictureType.Main);
                    return newsModel;
                })
                .ToList();
            var currentNews = newsItems.FirstOrDefault();
            int currentNewsId = currentNews == null ? 0 : currentNews.Id;

            if (newsAnnouncementId == currentNewsId && displayCount >= 3)
            {
                return Content("");
            }
            else if (newsAnnouncementId == currentNewsId)
            { 
                displayCount++;
            }
            else if (newsAnnouncementId != currentNewsId)
            {
                displayCount = 0;
                newsAnnouncementIdCookie = new System.Web.HttpCookie("news.NewsAnnouncementId", currentNewsId.ToString());
                HttpContext.Response.Cookies.Set(newsAnnouncementIdCookie);
            }

            displayCountCookie = new System.Web.HttpCookie("news.DisplayCount", displayCount.ToString());
            HttpContext.Response.Cookies.Set(displayCountCookie);

			if (view != "")
				return PartialView(view, model);
			else
				return PartialView(model);
        }

        #endregion
    }
}
