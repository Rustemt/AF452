using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Tax;
using Nop.Core.Infrastructure;
using Nop.Services.AFServices;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Forums;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Tax;
using Nop.Services.Topics;
using Nop.Web.Extensions;
using Nop.Web.Framework.Localization;
using Nop.Web.Framework.Themes;
using Nop.Web.Framework.Security.Captcha;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Common;
using Nop.Web.Framework;
using Nop.Core.Caching;
using AlternativeDataAccess;
using Nop.Services.News;
using Nop.Web.Infrastructure.Cache;

namespace Nop.Web.Controllers
{
    public class CommonController : BaseNopController
    {
        private readonly INewsService _newsService;
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly IManufacturerService _manufacturerService;
        private readonly ITopicService _topicService;
        private readonly ILanguageService _languageService;
        private readonly ICurrencyService _currencyService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly ISitemapGenerator _sitemapGenerator;
        private readonly IThemeContext _themeContext;
        private readonly IThemeProvider _themeProvider;
        private readonly IForumService _forumservice;
        private readonly ICustomerService _customerService;
        private readonly IWebHelper _webHelper;
        private readonly IPermissionService _permissionService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly ITaxService _taxService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;

        private readonly CustomerSettings _customerSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly TaxSettings _taxSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly StoreInformationSettings _storeInformationSettings;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly CommonSettings _commonSettings;
        private readonly BlogSettings _blogSettings;
        private readonly ForumSettings _forumSettings;
        private readonly LocalizationSettings _localizationSettings;
        private readonly IContentService _contentService;
        private readonly IPictureService _pictureService;
        private readonly CaptchaSettings _captchaSettings;
        private readonly ICacheManager _cacheManager;

        public CommonController(INewsService newsService, ICacheManager cacheManager, ICategoryService categoryService, IProductService productService,
            IManufacturerService manufacturerService, ITopicService topicService,
            ILanguageService languageService,
            ICurrencyService currencyService, ILocalizationService localizationService,
            IWorkContext workContext,
            IQueuedEmailService queuedEmailService, IEmailAccountService emailAccountService,
            ISitemapGenerator sitemapGenerator, IThemeContext themeContext,
            IThemeProvider themeProvider, IForumService forumService,
            ICustomerService customerService, IWebHelper webHelper,
            IPermissionService permissionService,
            CustomerSettings customerSettings, ShoppingCartSettings shoppingCartSettings,
            TaxSettings taxSettings, CatalogSettings catalogSettings,
            StoreInformationSettings storeInformationSettings, EmailAccountSettings emailAccountSettings,
            CommonSettings commonSettings, BlogSettings blogSettings, ForumSettings forumSettings,
            LocalizationSettings localizationSettings,
            IContentService contentService,
            IPictureService pictureService, IWorkflowMessageService messageService, ITaxService taxService, IPriceCalculationService priceCalculationService, IPriceFormatter priceFormatter,CaptchaSettings captchaSettings)
        {
            this._categoryService = categoryService;
            this._productService = productService;
            this._manufacturerService = manufacturerService;
            this._topicService = topicService;
            this._languageService = languageService;
            this._currencyService = currencyService;
            this._localizationService = localizationService;
            this._workContext = workContext;
            this._queuedEmailService = queuedEmailService;
            this._emailAccountService = emailAccountService;
            this._sitemapGenerator = sitemapGenerator;
            this._themeContext = themeContext;
            this._themeProvider = themeProvider;
            this._forumservice = forumService;
            this._customerService = customerService;
            this._webHelper = webHelper;
            this._permissionService = permissionService;
            this._workflowMessageService = messageService;
            this._priceFormatter = priceFormatter;
            this._customerSettings = customerSettings;
            this._shoppingCartSettings = shoppingCartSettings;
            this._taxSettings = taxSettings;
            this._catalogSettings = catalogSettings;
            this._storeInformationSettings = storeInformationSettings;
            this._emailAccountSettings = emailAccountSettings;
            this._commonSettings = commonSettings;
            this._blogSettings = blogSettings;
            this._forumSettings = forumSettings;
            this._localizationSettings = localizationSettings;
            this._contentService = contentService;
            this._pictureService = pictureService;
            this._taxService = taxService;
            this._priceCalculationService = priceCalculationService;
            this._captchaSettings = captchaSettings;
            this._cacheManager = EngineContext.Current.ContainerManager.Resolve<ICacheManager>("nop_cache_static");
            this._newsService = newsService;
        }


        public ActionResult CacheViewer()
        {
            ViewBag.keys = EngineContext.Current.ContainerManager.Resolve<ICacheManager>("nop_cache_static").GetCacheList();
            ViewBag.keysPerRequest = _cacheManager.GetCacheList();

            return View();
        }

        //AF
        //TODO: mustafa cache here
        [OutputCache(Duration = int.MaxValue, VaryByCustom = "lgg")]
        [ChildActionOnly]
        public ActionResult FooterCategoryMenu()
        {
            List<HeaderItem> endHeaderListItems = new List<HeaderItem>();
            var categories = _categoryService.GetAllCategoriesDisplayedOnHomePage();
            var categoryModels = categories.Select(x => x.ToModel());
            var model = new MenuModel();
            foreach (var categoryModel in categoryModels)
            {
                var headerItem = new HeaderItem() { Name = categoryModel.Name, Title = categoryModel.Name, Url = Url.RouteUrl("CategoryMain", new { categoryId = categoryModel.Id, SeName = categoryModel.SeName }) };
                if (categoryModel.DisplayOrder > 10)
                {
                    endHeaderListItems.Add(headerItem);
                    continue;
                }
                model.Add(headerItem);
            }
             //news
            if(_workContext.WorkingLanguage.LanguageCulture.ToLower() == "tr-tr")
                model.Add(new HeaderItem() { Name = _localizationService.GetResource("News.MenuHeader"), Url = Url.RouteUrl("NewsArchive"), HasMenu = false });

            model.Add(new HeaderItem() { Name = _localizationService.GetResource("Manufacturer.Manufacturers"), Url = Url.RouteUrl("ManufacturerList"), HasMenu = false });

            if (_catalogSettings.RecentlyAddedProductsEnabled)
            {
                model.Add(new HeaderItem() { Name = _localizationService.GetResource("Products.NewProducts"), Url = Url.RouteUrl("RecentlyAddedProducts"), HasMenu = false });
            }

            foreach (var headerItem in endHeaderListItems)
            {
                model.Add(headerItem);
            }

            return PartialView(model);
        }

        //AF
        //TODO: mustafa cache here
        [OutputCache(Duration = int.MaxValue, VaryByCustom = "lgg")]
        [ChildActionOnly]
        public ActionResult Menu()
        {
            var allCategories = new CatalogRepository().Categories();
            //var categories = _categoryService.GetAllCategoriesDisplayedOnHomePage();
            var categories = allCategories.Where(x => x.ShowOnHomePage == true).OrderBy(x => x.DisplayOrder);
            var categoryModels = categories.Select(x => x.ToModel());
            var model = new MenuModel();

            IList<Category> subCategories;
            IList<Manufacturer> manufacturers;
            IEnumerable<CategoryModel> subCategoryModels;
            IEnumerable<ManufacturerModel> manufacturerModels;


            foreach (var categoryModel in categoryModels)
            {
                var headerItem = new HeaderItem() { Name = categoryModel.Name, Title = categoryModel.Name, Url = Url.RouteUrl("CategoryMain", new { categoryId = categoryModel.Id, SeName = categoryModel.SeName }) };

                //subCategories = _categoryService.GetAllCategoriesByParentCategoryId(categoryModel.Id);
                subCategories = allCategories.Where(x => x.ParentCategoryId == categoryModel.Id).OrderBy(x => x.DisplayOrder).ToList();
                subCategoryModels = subCategories.Select(x => x.ToModel());
                foreach (var subCategoryModel in subCategoryModels)
                {
                    var subHeaderItem = new SubHeaderItem() { Name = subCategoryModel.Name, Title = subCategoryModel.Name, Url = Url.RouteUrl("Category", new { categoryId = subCategoryModel.Id, SeName = subCategoryModel.SeName }) };
                    //subHeaderItem.Items = _categoryService.GetAllCategoriesByParentCategoryId(subCategoryModel.Id).Select(x => x.ToModel()).Select(x => new MenuItem() { Name = x.Name, Title = x.Name, Url = Url.RouteUrl("Category", new { categoryId = x.Id, SeName = x.SeName }) }).ToList<MenuItem>();
                    subHeaderItem.Items = allCategories.Where(x => x.ParentCategoryId == subCategoryModel.Id).OrderBy(x => x.DisplayOrder).Select(x => x.ToModel()).Select(x => new MenuItem() { Name = x.Name, Title = x.Name, Url = Url.RouteUrl("Category", new { categoryId = x.Id, SeName = x.SeName }) }).ToList<MenuItem>();
                    headerItem.SubHeaders.Add(subHeaderItem);
                }
                //manufacturers = GetProductsManufacturers(GetNestedCategoryProducts(categoryModel.Id));
                manufacturers = _manufacturerService.GetManufacturerByCategoryIds(GetChildCategoryIds(categoryModel.Id)).OrderBy(a=>a.DisplayOrder).ToList();
                manufacturerModels = manufacturers.Select(x => x.ToModel());
                headerItem.Brands = manufacturerModels.Select(x => new MenuItem() { Name = x.Name, Title = x.Name, Url = Url.RouteUrl("ManufacturerCategorySe", new { SeName = x.SeName, categoryId = categoryModel.Id, cSeName = categoryModel.SeName }) }).ToList<MenuItem>();
                headerItem.ContentItem = GetMenuCategoryContent(categoryModel.Id).FirstOrDefault();
                headerItem.HasMenu = true;
                headerItem.AllBrands = new MenuItem()
                {
                    Name = _localizationService.GetResource("Menu.ViewAllBrands"),
                    Title = _localizationService.GetResource("Menu.ViewAllBrands"),
                    Url = Url.RouteUrl("CategoryManufacturerList", new { categoryId = categoryModel.Id, SeName = categoryModel.SeName })
                };
                model.Add(headerItem);
            }

            //TODO: temporary solution!!
            //var giftCategoryModel = model.Last();
    
            //news
            if(_workContext.WorkingLanguage.LanguageCulture.ToLower() == "tr-tr")
                model.Add(new HeaderItem() { Name = _localizationService.GetResource("News.MenuHeader"), Url = Url.RouteUrl("NewsArchive"), HasMenu = false });

            //manufacturers
            model.Add(new HeaderItem() { Name = _localizationService.GetResource("Manufacturer.Manufacturers"), Url = Url.RouteUrl("ManufacturersAll"), HasMenu = false });

            //new products
            if (_catalogSettings.RecentlyAddedProductsEnabled)
            {
                model.Add(new HeaderItem() { Name = _localizationService.GetResource("Products.NewProducts"), Url = Url.RouteUrl("RecentlyAddedProducts"), HasMenu = false });
            }
            //TODO: temporary solution for "gift" menu item
            //model.Remove(giftCategoryModel);
            //model.Add(giftCategoryModel);

        
            return PartialView(model);
        }
        //AF
        [NonAction]
        private IEnumerable<ContentItemModel> GetMenuCategoryContent(int categoryId)
        {
            var contentProductCategories = _contentService.GetContent(ContentType.CategoryMenuContent.ToContentString(), categoryId, int.MaxValue);
            var contentProducts = contentProductCategories.Select(x => x.Product);
            var contentItems = contentProducts.ToContentItems(_pictureService);
            return contentItems;
        }
        //AF
        [NonAction]
        private List<Product> GetNestedCategoryProducts(int categoryId)
        {
            List<Product> products = new List<Product>();
            products.AddRange(_productService.SearchProducts(categoryId,
                                0, null, null, null, 0, string.Empty, false, 0, null,
                                ProductSortingEnum.Position, 0, int.MaxValue));
            foreach (var subCategory in _categoryService.GetAllCategoriesByParentCategoryId(categoryId))
            {
                products.AddRange(GetNestedCategoryProducts(subCategory.Id));
            }
            return products;
        }
        //AF
        [NonAction]
        private List<Manufacturer> GetProductsManufacturers(IList<Product> products)
        {
            List<Manufacturer> manufacturers = new List<Manufacturer>();
            foreach (var product in products)
            {
                foreach (var productManufacturer in product.ProductManufacturers)
                {
                    if (productManufacturer.Manufacturer.Published)
                        if (!manufacturers.Contains(productManufacturer.Manufacturer))
                        {
                            manufacturers.Add(productManufacturer.Manufacturer);
                        }
                }
            }
            return manufacturers.OrderBy(x=>x.DisplayOrder).ToList();
        }

        [NonAction]
        protected List<int> GetChildCategoryIds(int parentCategoryId, bool showHidden = false)
        {
            //TODO: cache 
            var categoriesIds = new List<int>();
            var categories = _categoryService.GetAllCategoriesByParentCategoryId(parentCategoryId, showHidden);
            foreach (var category in categories)
            {
                categoriesIds.Add(category.Id);
                categoriesIds.AddRange(GetChildCategoryIds(category.Id, showHidden));
            }
            return categoriesIds;
        }

        //page not found
        public ActionResult PageNotFound()
        {
            this.Response.StatusCode = 404;
            this.Response.TrySkipIisCustomErrors = true;

            return View();
        }

        //language
        [ChildActionOnly]
        public ActionResult LanguageSelector()
        {
            var model = new LanguageSelectorModel()
            {
                CurrentLanguage = _workContext.WorkingLanguage.ToModel(),
                AvailableLanguages = _languageService.GetAllLanguages().Select(x => x.ToModel()).ToList(),
                UseImages = _localizationSettings.UseImagesForLanguageSelection
            };
            return PartialView(model);
        }

        [ChildActionOnly]
        public ActionResult LanguageSelectorNew()
        {
            var model = new LanguageSelectorModel()
            {
                CurrentLanguage = _workContext.WorkingLanguage.ToModel(),
                AvailableLanguages = _languageService.GetAllLanguages().Select(x => x.ToModel()).ToList(),
                UseImages = _localizationSettings.UseImagesForLanguageSelection
            };
            return PartialView(model);
        }

        public ActionResult LanguageSelected(int customerlanguage)
        {
            var language = _languageService.GetLanguageById(customerlanguage);
            if (language != null)
            {
                _workContext.WorkingLanguage = language;
                SetLanguageCurrency(language);
            }

            if (_localizationSettings.SeoFriendlyUrlsForLanguagesEnabled)
            {
                string applicationPath = HttpContext.Request.ApplicationPath;
                if (HttpContext.Request.UrlReferrer != null)
                {
                    string redirectUrl = HttpContext.Request.UrlReferrer.PathAndQuery;
                    if (redirectUrl.IsLocalizedUrl(applicationPath, true))
                    {
                        //already localized URL
                        redirectUrl = redirectUrl.RemoveLocalizedPathFromRawUrl(applicationPath);
                    }
                    redirectUrl = redirectUrl.AddLocalizedPathToRawUrl(applicationPath, _workContext.WorkingLanguage);
                    return Redirect(redirectUrl);
                }
                else
                {
                    string redirectUrl = Url.RouteUrl("HomePage");
                    redirectUrl = redirectUrl.AddLocalizedPathToRawUrl(applicationPath, _workContext.WorkingLanguage);
                    return Redirect(redirectUrl);
                }
            }
            else
            {
                //TODO: URL referrer is null in IE 8. Fix it
                if (HttpContext.Request.UrlReferrer != null)
                {
                    return Redirect(HttpContext.Request.UrlReferrer.PathAndQuery);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
        }
        //TODO: re-implement relatin between lanuage and currency
        private void SetLanguageCurrency(Language language)
        {
            Currency currency = null;
            switch (language.Name.ToLower())
            {

                case "turkish":
                    currency = _currencyService.GetCurrencyById(12);
                    break;
                case "english":
                    currency = _currencyService.GetCurrencyById(1);
                    break;
                default:
                    break;
            }
            if (currency != null)
            {
                _workContext.WorkingCurrency = currency;
            }

        }

        //currency
        [ChildActionOnly]
        public ActionResult CurrencySelector()
        {
            var model = new CurrencySelectorModel();
            model.CurrentCurrency = _workContext.WorkingCurrency.ToModel();
            model.AvailableCurrencies = _currencyService.GetAllCurrencies().Select(x => x.ToModel()).ToList();
            return PartialView(model);
        }

        [ChildActionOnly]
        public ActionResult CurrencySelectorNew()
        {
            var model = new CurrencySelectorModel();
            model.CurrentCurrency = _workContext.WorkingCurrency.ToModel();
            model.AvailableCurrencies = _currencyService.GetAllCurrencies().Select(x => x.ToModel()).ToList();
            return PartialView(model);
        }

        public ActionResult CurrencySelected(int customerCurrency)
        {
            var currency = _currencyService.GetCurrencyById(customerCurrency);
            if (currency != null)
            {
                _workContext.WorkingCurrency = currency;
            }
            if (Request.UrlReferrer == null)
                return RedirectToAction("Index", "Home");
            return Redirect(Request.UrlReferrer.PathAndQuery);
        }

        //tax type
        [ChildActionOnly]
        public ActionResult TaxTypeSelector()
        {
            var model = new TaxTypeSelectorModel();
            model.Enabled = _taxSettings.AllowCustomersToSelectTaxDisplayType;
            model.CurrentTaxType = _workContext.TaxDisplayType;
            return PartialView(model);
        }

        public ActionResult TaxTypeSelected(int customerTaxType)
        {
            var taxDisplayType = (TaxDisplayType)Enum.ToObject(typeof(TaxDisplayType), customerTaxType);
            _workContext.TaxDisplayType = taxDisplayType;

            var model = new TaxTypeSelectorModel();
            model.Enabled = _taxSettings.AllowCustomersToSelectTaxDisplayType;
            model.CurrentTaxType = _workContext.TaxDisplayType;
            return PartialView("TaxTypeSelector", model);
        }

        //header links
        //AF
        [OutputCache(Duration = int.MaxValue, VaryByCustom = "lgg")]
        [ChildActionOnly]
        public ActionResult _Footer()
        {
            CatalogRepository cr = new CatalogRepository();
            List<Category> categories = cr.Categories();
            Dictionary<int, string> categoryPictures = new Dictionary<int, string>();

            categories.ForEach(x =>
            {
                x.Name = x.GetLocalized(c => c.Name);
                x.SeName = x.GetLocalized(c => c.SeName);
            });
            ViewBag.Categories = categories;

            ViewBag.LanguageCulture = _workContext.WorkingLanguage.LanguageCulture.ToLower();

            ViewBag.ManufacturersShowCase = cr.Manufacturers().OrderBy(x => x.DisplayOrder).Take(10).ToList();

            return PartialView();
        }

        private Dictionary<int, string> getCatPictures(List<Category> categories, PictureRepository _pr)
        {
            Dictionary<int, string> _categoryPictures = new Dictionary<int, string>();

            categories.ForEach(x =>
            {
                if (x.PictureId != 0)
                    _categoryPictures.Add(x.Id, _pictureService.GetPictureUrl(_pr.Find(x.PictureId), 0));
            });

            return _categoryPictures;
        }

        private List<Manufacturer> getManufacturers(int catId, CatalogRepository _cr)
        {
            return _cr.ManufacturerByParentCategory(catId).OrderBy(x => x.DisplayOrder).Take(10).ToList();
        }

        //[OutputCache(Duration = int.MaxValue, VaryByCustom = "lgg")]
        [ChildActionOnly]
        public ActionResult _Header()
        {
            List<Category> categories;
            Dictionary<int, string> categoryPictures = new Dictionary<int, string>();
            Dictionary<int, string> manufacturerPictures = new Dictionary<int, string>();
            Dictionary<int, string> manufacturerShowcasePictures = new Dictionary<int, string>();
            Dictionary<string, List<Manufacturer>> manufacturers = new Dictionary<string, List<Manufacturer>>();
            List<string> manufacturersFirstLetters = new List<string>();




            CatalogRepository cr = new CatalogRepository();
            PictureRepository pr = new PictureRepository();

            //categories = cr.Categories();
            categories = _cacheManager.Get(string.Format(ModelCacheEventConsumer.CATEGORIES_KEY, _workContext.WorkingLanguage.Id), 3600, () =>
            {
                var _categories =  cr.Categories();
                _categories.ForEach(x =>
                {
                    x.Name = x.GetLocalized(c => c.Name);
                    x.SeName = x.GetLocalized(c => c.SeName);
                });
                return _categories;
            });

            categoryPictures = _cacheManager.Get(string.Format(ModelCacheEventConsumer.CATEGORY_PICTURES_KEY, _webHelper.IsCurrentConnectionSecured().ToString()), 3600, () =>
            {
                return getCatPictures(categories, pr);
            });

            //categoryPictures = getCatPictures(categories, pr);

            foreach (var category in categories.Where(x => x.ShowOnHomePage == true).ToList())
            {
                var _manufacturers = _cacheManager.Get(string.Format(ModelCacheEventConsumer.CATEGORY_MANUFACTURES_KEY, category.Id), 3600, () =>
                {
                    return getManufacturers(category.Id, cr);
                }); 

                manufacturers.Add(category.Id.ToString(), _manufacturers);
                manufacturers[category.Id.ToString()].ForEach(x =>
                {
                    if (x.MenuPictureId != 0 && !manufacturerPictures.ContainsKey(x.Id))
                        manufacturerPictures.Add(x.Id, _pictureService.GetPictureUrl(pr.Find(x.MenuPictureId), 0));

                    if (x.MenuShowcasePictureId != 0 && !manufacturerShowcasePictures.ContainsKey(x.Id))
                        manufacturerShowcasePictures.Add(x.Id, _pictureService.GetPictureUrl(pr.Find(x.MenuShowcasePictureId), 0));
                });
            }



            ViewBag.Categories = categories;
            ViewBag.CategoryPictures = categoryPictures;
            ViewBag.Manufacturers = manufacturers;
            ViewBag.ManufacturerPictures = manufacturerPictures;
            ViewBag.ManufacturerShowcasePictures = manufacturerShowcasePictures;
            ViewBag.ManufacturersShowCase = cr.Manufacturers().OrderBy(x => x.DisplayOrder).Take(10).ToList();

            var Ms = cr.Manufacturers().ToList();
            foreach (var m in Ms)
                manufacturersFirstLetters.Add(char.IsLetter(m.Name, 0) ? (m.Name.Substring(0, 1) == "i" ? "İ" : m.Name.Substring(0, 1)) : "#");



            ViewBag.ManufacturersFirstLetters = manufacturersFirstLetters.Distinct().OrderBy(x => x).ToList();

            var customer = _workContext.CurrentCustomer;
            ViewBag.IsAuthenticated = customer.IsRegistered();

            ViewBag.LanguageCulture = _workContext.WorkingLanguage.LanguageCulture.ToLower();

            ViewBag.HomeContentManufacturerBanner = _cacheManager.Get(string.Format(ModelCacheEventConsumer.HOMEPAGE_MANUFACTURES_BANNER_KEY, _webHelper.IsCurrentConnectionSecured().ToString()), 3600, () =>
            {
                return _homeContentManufacturerBanner();
            }); 

            return PartialView();
        }

        private IEnumerable<ContentItemModel> _homeContentManufacturerBanner()
        {
            var homeContentManufacturerBanner = _newsService.GetAllNews(2 /*TR*/, null, null, NewsType.HomeContentManufacturerBanner, 0, 1);
            return homeContentManufacturerBanner.ToContentItems(_pictureService);
        }

        [ChildActionOnly]
        public ActionResult HeaderTop()
        {
            var customer = _workContext.CurrentCustomer;

            //var unreadMessageCount = GetUnreadPrivateMessages();
            //var unreadMessage = string.Empty;
            //var alertMessage = string.Empty;
            //if (unreadMessageCount > 0)
            //{
            //    unreadMessage = string.Format(_localizationService.GetResource("PrivateMessages.TotalUnread"), unreadMessageCount);

            //    //notifications here
            //    if (_forumSettings.ShowAlertForPM &&
            //        !customer.GetAttribute<bool>(SystemCustomerAttributeNames.NotifiedAboutNewPrivateMessages))
            //    {
            //        _customerService.SaveCustomerAttribute<bool>(customer, SystemCustomerAttributeNames.NotifiedAboutNewPrivateMessages, true);
            //        alertMessage = string.Format(_localizationService.GetResource("PrivateMessages.YouHaveUnreadPM"), unreadMessageCount);
            //    }
            //}

            var model = new HeaderLinksModel()
            {
                IsAuthenticated = customer.IsRegistered(),
                CustomerEmailUsername = customer.IsRegistered() ? (_customerSettings.UsernamesEnabled ? customer.Username : customer.Email) : "",
                IsCustomerImpersonated = _workContext.OriginalCustomerIfImpersonated != null,
                //DisplayAdminLink = _permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel),
                WishlistEnabled = true,
                AllowPrivateMessages = _forumSettings.AllowPrivateMessages,
                UnreadPrivateMessages = "",
                AlertMessage = "",
            };

            return PartialView(model);
        }

        [ChildActionOnly]
        public ActionResult HeaderTopNew()
        {
            var customer = _workContext.CurrentCustomer;

            var unreadMessageCount = GetUnreadPrivateMessages();
            var unreadMessage = string.Empty;
            var alertMessage = string.Empty;
            if (unreadMessageCount > 0)
            {
                unreadMessage = string.Format(_localizationService.GetResource("PrivateMessages.TotalUnread"), unreadMessageCount);

                //notifications here
                if (_forumSettings.ShowAlertForPM &&
                    !customer.GetAttribute<bool>(SystemCustomerAttributeNames.NotifiedAboutNewPrivateMessages))
                {
                    _customerService.SaveCustomerAttribute<bool>(customer, SystemCustomerAttributeNames.NotifiedAboutNewPrivateMessages, true);
                    alertMessage = string.Format(_localizationService.GetResource("PrivateMessages.YouHaveUnreadPM"), unreadMessageCount);
                }
            }

            var model = new HeaderLinksModel()
            {
                IsAuthenticated = customer.IsRegistered(),
                CustomerEmailUsername = customer.IsRegistered() ? (_customerSettings.UsernamesEnabled ? customer.Username : customer.Email) : "",
                IsCustomerImpersonated = _workContext.OriginalCustomerIfImpersonated != null,
                //DisplayAdminLink = _permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel),
                WishlistEnabled = true,
                AllowPrivateMessages = _forumSettings.AllowPrivateMessages,
                UnreadPrivateMessages = unreadMessage,
                AlertMessage = alertMessage,
            };

            return PartialView(model);
        }

        [NonAction]
        private int GetUnreadPrivateMessages()
        {
            var result = 0;
            var customer = _workContext.CurrentCustomer;
            if (_forumSettings.AllowPrivateMessages && !customer.IsGuest())
            {
                var privateMessages = _forumservice.GetAllPrivateMessages(0, customer.Id, false, null, false, string.Empty, 0, 1);

                if (privateMessages.TotalCount > 0)
                {
                    result = privateMessages.TotalCount;
                }
            }

            return result;
        }

        ////menu
        //[ChildActionOnly]
        //public ActionResult Menu()
        //{
        //    var model = new MenuModel()
        //    {
        //        RecentlyAddedProductsEnabled = _catalogSettings.RecentlyAddedProductsEnabled,
        //        BlogEnabled = _blogSettings.Enabled,
        //        ForumEnabled = _forumSettings.ForumsEnabled
        //    };

        //    return PartialView(model);
        //}

        //info block
        [ChildActionOnly]
        public ActionResult InfoBlock()
        {
            var model = new InfoBlockModel()
            {
                RecentlyAddedProductsEnabled = _catalogSettings.RecentlyAddedProductsEnabled,
                RecentlyViewedProductsEnabled = _catalogSettings.RecentlyViewedProductsEnabled,
                CompareProductsEnabled = _catalogSettings.CompareProductsEnabled,
                BlogEnabled = _blogSettings.Enabled,
                SitemapEnabled = _commonSettings.SitemapEnabled,
                ForumEnabled = _forumSettings.ForumsEnabled
            };

            return PartialView(model);
        }

        //contact us page
        [BotGetControl]
        public ActionResult ContactUs()
        {
            var model = new ContactUsModel()
            {
                Email = _workContext.CurrentCustomer.Email,
                FirstName = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName),
                LastName = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.LastName),
                DisplayCaptcha = _captchaSettings.ShowOnContactUsPage
            };
            return View(model);
        }

        //AF
        [ChildActionOnly]
        public ActionResult ContactBox()
        {
            var model = new CustomerSupportModel()
            {
                Email = _workContext.CurrentCustomer.Email,
                FirstName = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName),
                LastName = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.LastName)
            };
            PrepareCustomerSupport(model);
            return View(model);
        }


        //AF
        [ChildActionOnly]
        [BotGetControl]
        public ActionResult OfferPrice()
        {
            var model = new PriceOfferModel()
            {
                Email = _workContext.CurrentCustomer.Email,
                FirstName = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName),
                LastName = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.LastName)
            };
            return View("PriceOffer", model);
        }

        //AF
        [HttpPost]
        [BotPostControl(RedirectUrl = "/", RedirectAjaxUrl = "/Catalog/ProductEmailAFriendSuccess", TrapFormElementName = "Surname", MinimumRequestPeriod = 2)]
        public ActionResult PriceOfferSend(PriceOfferModel model)
        {
            //ajax form
            if (!ModelState.IsValid)
                return Json(new { Success = false, Message = _localizationService.GetResource("ProductEmailFriend.UnSucsess"), SubMessage = _localizationService.GetResource("PriceOffer.NotificationFailMessage") });

            var productVariant = _productService.GetProductVariantById(model.ProductId);
            if (productVariant == null || productVariant.Deleted || !productVariant.Published)
                return Json(new { Success = false, Message = _localizationService.GetResource("ProductEmailFriend.UnSucsess"), SubMessage = _localizationService.GetResource("PriceOffer.NotificationFailMessage") });

            var productVariantModel = new ProductModel.ProductVariantModel();

            string fullName = string.Format("{0} {1}", model.FirstName, model.LastName);
            string email = model.Email.Trim().ToLower();
            string url = Url.Action("CustomerDiscount", "Customer", new { customerId = _workContext.CurrentCustomer.Id, productVariantId = model.ProductId });

            var priceOfferCustomer = _customerService.GetCustomerProductVariantQuoteByVariantId(email, model.ProductId);
            if (priceOfferCustomer == null)
            {
                var customerProductVariantQuote = new CustomerProductVariantQuote();
              
                    customerProductVariantQuote.CustomerId = _workContext.CurrentCustomer.Id;
                    customerProductVariantQuote.ProductVariantId = model.ProductId;
                    customerProductVariantQuote.Email = email;
                    customerProductVariantQuote.PriceWithDiscount = productVariantModel.ProductVariantPrice.PriceWithDiscount;
                    customerProductVariantQuote.PriceWithoutDiscount = productVariantModel.ProductVariantPrice.Price;
                    customerProductVariantQuote.DiscountPercentage = productVariantModel.ProductVariantPrice.DiscountPercentage;
                    
                if ( _workContext.CurrentCustomer.IsGuest())
                    {
                        customerProductVariantQuote.PhoneNumber = model.Phone;
                        customerProductVariantQuote.Name = model.FirstName;
                        customerProductVariantQuote.Enquiry = model.Enquiry;
                    }
                // Quote Insert
                _customerService.InsertCustomerProductVariantQuote(customerProductVariantQuote);
                //_workContext.CurrentCustomer.CustomerProductVariantQuotes.Add(customerProductVariantQuote);
                //_customerService.UpdateCustomer(_workContext.CurrentCustomer);
                //Get Discount Price 
                try
                {
                    StatefulStorage.PerSession.Add<bool?>("SkipQuoteDiscountActivationCheck", () => (bool?)true);
                    PrepareProductVariantModel(productVariantModel, productVariant);
                }
                finally
                {
                    StatefulStorage.PerSession.Remove<bool?>("SkipQuoteDiscountActivationCheck");
                }

                customerProductVariantQuote.PriceWithDiscount = productVariantModel.ProductVariantPrice.PriceWithDiscount;
                customerProductVariantQuote.PriceWithoutDiscount = productVariantModel.ProductVariantPrice.Price;
                customerProductVariantQuote.DiscountPercentage = productVariantModel.ProductVariantPrice.DiscountPercentage;
                _customerService.UpdateCustomerProductVariantQuote(customerProductVariantQuote);

               
                //customerProductVariantQuote.ActivateDate = null;
                //_customerService.UpdateCustomerProductVariantQuote(customerProductVariantQuote);
                // End Get Discount Price 
                
                //Customer Notification
                int priceOfferStatus = _workflowMessageService.SendPriceOfferCustomerNotification(fullName, email, model.Phone, model.ProductId, model.ProductName, model.Enquiry, _workContext.WorkingLanguage.Id, url, productVariant, productVariantModel.ProductVariantPrice.PriceWithDiscount);
                if (priceOfferStatus != -1)
                {
                    // Store Notification
                    _workflowMessageService.SendPriceOfferStoreOwnerNotification(fullName, email, model.Subject, model.Phone, model.ProductId, model.ProductName, model.Enquiry, _workContext.WorkingLanguage.Id, customerProductVariantQuote.Id.ToString(), productVariantModel.ProductVariantPrice.DiscountPercentage, productVariantModel.Sku, productVariant.ProductId.ToString(),productVariantModel.ProductVariantPrice.PriceWithDiscount);
                     return Json(new { Success = true, Message = _localizationService.GetResource("CustomerSupport.YourEnquiryHasBeenSent"), SubMessage = _localizationService.GetResource("PriceOffer.NotificationMessage") }); 
                }
                else
                {
                    return Json(new { Success = false, Message = _localizationService.GetResource("CustomerSupport.YourEnquiryHasNotBeenSent"), SubMessage = _localizationService.GetResource("PriceOffer.BlackListMessage") }); 
                    
                }
            }
            //TODO: Query the necessity
            else if (priceOfferCustomer.ActivateDate == null)
            {
                //var customerProductVariantQuote =_customerService.GetCustomerProductVariantQuoteByVariantId(email, model.ProductId);
                //customerProductVariantQuote.ActivateDate = DateTime.Now;
                //_customerService.UpdateCustomerProductVariantQuote(customerProductVariantQuote);
                try
                {
                    StatefulStorage.PerSession.Add<bool?>("SkipQuoteDiscountActivationCheck", () => (bool?)true);
                    PrepareProductVariantModel(productVariantModel, productVariant);
                }
                finally
                {
                    StatefulStorage.PerSession.Remove<bool?>("SkipQuoteDiscountActivationCheck");
                }
                _workflowMessageService.SendPriceOfferCustomerNotification(fullName, email, model.Phone, model.ProductId, model.ProductName, model.Enquiry, _workContext.WorkingLanguage.Id, url, productVariant, productVariantModel.ProductVariantPrice.PriceWithDiscount);


               // customerProductVariantQuote.ActivateDate = null;
               // _customerService.UpdateCustomerProductVariantQuote(customerProductVariantQuote);
                return Json(new { Success = true, Message = _localizationService.GetResource("PriceOffer.AllReadyExistToSendMail"), SubMessage = "" });
            }
            else
            {
                return Json(new { Success = false, Message = _localizationService.GetResource("PriceOffer.AllReadyExist"), SubMessage = "" });
            }
            // return Json(new { Success = true, Message = _localizationService.GetResource("CustomerSupport.YourEnquiryHasBeenSent"), SubMessage = _localizationService.GetResource("PriceOffer.NotificationMessage") });

//

        }

        //AF
        [HttpPost, ActionName("ContactUs")]
        [BotPostControl(RedirectUrl = "/contactus", RedirectAjaxUrl = "/Catalog/ProductEmailAFriendSuccess", TrapFormElementName = "Surname", MinimumRequestPeriod = 5)]
        [CaptchaValidator]
        public ActionResult ContactUsSend(ContactUsModel model, bool captchaValid)
        {
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnContactUsPage && !captchaValid)
            {

                ModelState.AddModelError("", _localizationService.GetResource("Common.WrongCaptcha"));
                return Json(new { header = _localizationService.GetResource("ContactUs.YourEnquiryHasNotBeenSent"), message = _localizationService.GetResource("Common.WrongCaptcha"), success = false });

            }
            //ajax form
            if (ModelState.IsValid)
            {

                string email = model.Email.Trim();
                string fullName = string.Format("{0} {1}", model.FirstName, model.LastName);
                string phone = model.Phone;
                string subject = model.Subject;
                string body = Core.Html.HtmlHelper.FormatText(model.Enquiry, false, true, false, false, false, false);

                int result = _workflowMessageService.SendCustomerCareStoreOwnerNotification(_workContext.WorkingLanguage.Id, model.Email.Trim(), model.Enquiry, fullName, subject,null, phone);
                if (result != 0)
                {
                    //informing the customer
                    _workflowMessageService.SendCustomerCareCustomerNotification(_workContext.WorkingLanguage.Id, model.Email.Trim(), fullName);


                    //notificationMailSend
                    //_workflowMessageService.SendMailReceivedNotification(_workContext.CurrentCustomer, emailAccount, _workContext.WorkingLanguage.Id, fullName, email);


                     return Json(new { header = _localizationService.GetResource("ContactUs.YourEnquiryHasBeenSent"), message=" ", success = true });

                }
            }
            // return Content(ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage);
                     return Json(new { header ="",message=_localizationService.GetResource("CustomerCare.MessageNotSent") , success = false });
        }

        //AF
        [HttpPost]
        public JsonResult CustomerSupportSend(CustomerSupportModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { header = _localizationService.GetResource("CustomerCare.MessageNotSent"), success = false });

            string Subject = _localizationService.GetResource("Common.Dropdown.SelectValue");

            switch (model.Subject)
            {
                case "R":
                    Subject = _localizationService.GetResource("Help.Fields.Order");
                    break;
                case "I":
                    Subject = _localizationService.GetResource("Help.Fields.Inquires");
                    break;
                case "G":
                    Subject = _localizationService.GetResource("Help.Fields.GiftCards");
                    break;
                case "O":
                    Subject = _localizationService.GetResource("Help.Fields.Others");
                    break;
            }
            string fullName = string.Format("{0} {1}", model.FirstName, model.LastName);
            int result = _workflowMessageService.SendCustomerCareStoreOwnerNotification(_workContext.WorkingLanguage.Id, model.Email.Trim(), model.Explanation, fullName, Subject);
            if (result != 0)
            {
                //informing the customer
                _workflowMessageService.SendCustomerCareCustomerNotification(_workContext.WorkingLanguage.Id, model.Email.Trim(), fullName);
                return Json(new { header = _localizationService.GetResource("CustomerCare.MessageSent"), message = _localizationService.GetResource("CustomerCare.MessageSent.Detail"), success = true });
            }
            else
                return Json(new { header = _localizationService.GetResource("CustomerCare.MessageNotSent"), success = false });
            #region

            //ajax form
            /*  if (ModelState.IsValid)
              {
                  string email = model.Email.Trim();
                  string fullName = string.Format("{0} {1}", model.FirstName, model.LastName);
                  string subject = string.Format("{0}. {1} - {2}", _storeInformationSettings.StoreName, "Customer Care :" + model.Subject , "Help");
                  var emailAccount = _emailAccountService.GetEmailAccountById(_emailAccountSettings.DefaultEmailAccountId);
                  string from = null;
                  string fromName = null;
                  string body = Core.Html.HtmlHelper.FormatText(model.Explanation, false, true, false, false, false, false);
                  //required for some SMTP servers
                  if (_commonSettings.UseSystemEmailForContactUsForm)
                  {
                      from = emailAccount.Email;
                      fromName = emailAccount.DisplayName;
                      body = string.Format("<b>From</b>: {0} - {1}<br /><br />{2}", Server.HtmlEncode(fullName), Server.HtmlEncode(email), body);
                  }
                  else
                  {
                      from = email;
                      fromName = fullName;
                  }

                 
                  _queuedEmailService.InsertQueuedEmail(new QueuedEmail()
                  {
                      From = from,
                      FromName = fromName,
                      To = emailAccount.Email,
                      ToName = emailAccount.DisplayName,
                      Priority = 5,
                      Subject = subject,
                      Body = body,
                      CreatedOnUtc = DateTime.UtcNow,
                      EmailAccountId = emailAccount.Id,
                      Bcc = "assistance@alwaysfashion.com"
                  });

                  return Json(new { header = _localizationService.GetResource("CustomerCare.MessageSent"), message = _localizationService.GetResource("CustomerCare.MessageSent.Detail"),success = true });
              }
              return Json(new { header = _localizationService.GetResource("CustomerCare.MessageNotSent"), success = false });
        */
            #endregion
        }

        //
        [NonAction]
        private void PrepareCustomerSupport(CustomerSupportModel model)
        {
            model.Subjects = new List<SelectListItem>();
            model.Subjects.Add(new SelectListItem() { Text = _localizationService.GetResource("Common.Dropdown.SelectValue"), Value = "" });
            model.Subjects.Add(new SelectListItem() { Text = _localizationService.GetResource("Help.Fields.Order"), Value = "R" });
            model.Subjects.Add(new SelectListItem() { Text = _localizationService.GetResource("Help.Fields.Inquires"), Value = "I" });
            model.Subjects.Add(new SelectListItem() { Text = _localizationService.GetResource("Help.Fields.GiftCards"), Value = "G" });
            model.Subjects.Add(new SelectListItem() { Text = _localizationService.GetResource("Help.Fields.Others"), Value = "O" });
        }


        private ProductModel.ProductVariantModel PrepareProductVariantModel(ProductModel.ProductVariantModel model, ProductVariant productVariant)
        {
            if (productVariant == null)
                throw new ArgumentNullException("productVariant");

            if (model == null)
                throw new ArgumentNullException("model");
            model.Sku = productVariant.Sku;
            #region Product variant price
            model.ProductVariantPrice.ProductVariantId = productVariant.Id;
            model.ProductVariantPrice.DynamicPriceUpdate = _catalogSettings.EnableDynamicPriceUpdate;
            if (_permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
            {
                model.ProductVariantPrice.HidePrices = false;
                if (productVariant.CustomerEntersPrice)
                {
                    model.ProductVariantPrice.CustomerEntersPrice = true;
                }
                else
                {
                    if (productVariant.CallForPrice)
                        model.ProductVariantPrice.CallForPrice = !productVariant.CallforPriceRequested(_workContext.CurrentCustomer);

                    decimal taxRate = decimal.Zero;
                    decimal oldPriceBase = _taxService.GetProductPrice(productVariant, productVariant.OldPrice, out taxRate);
                    decimal finalPriceWithoutDiscountBase = _taxService.GetProductPrice(productVariant, _priceCalculationService.GetFinalPrice(productVariant, false), out taxRate);
                    decimal finalPriceWithDiscountBase = _taxService.GetProductPrice(productVariant, _priceCalculationService.GetFinalPrice(productVariant, true, false), out taxRate);

                    decimal oldPrice = _currencyService.ConvertFromPrimaryStoreCurrency(oldPriceBase, _workContext.WorkingCurrency);
                    decimal finalPriceWithoutDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithoutDiscountBase, _workContext.WorkingCurrency);
                    decimal finalPriceWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithDiscountBase, _workContext.WorkingCurrency);

                    if (finalPriceWithoutDiscountBase != oldPriceBase && oldPriceBase > decimal.Zero)
                        model.ProductVariantPrice.OldPrice = _priceFormatter.FormatPrice(oldPrice, false, false);

                    model.ProductVariantPrice.Price = _priceFormatter.FormatPrice(finalPriceWithoutDiscount, false, false);

                    if (finalPriceWithoutDiscountBase != finalPriceWithDiscountBase)
                    {
                        IList<Discount> discounts = null;
                        model.ProductVariantPrice.PriceWithDiscount = _priceFormatter.FormatPrice(finalPriceWithDiscount, false, false);
                        decimal discountValueBase = _priceCalculationService.GetDiscountAmount(productVariant, _workContext.CurrentCustomer, 0, out discounts, false);
                        model.ProductVariantPrice.DiscountValue = _currencyService.ConvertFromPrimaryStoreCurrency(discountValueBase, _workContext.WorkingCurrency);
                        model.ProductVariantPrice.DiscountPrice = _priceFormatter.FormatPrice(model.ProductVariantPrice.DiscountValue, false, false);

                        if (_workContext.WorkingLanguage.DisplayOrder == 2)
                            model.ProductVariantPrice.DiscountPercentage = String.Format("({0}%)", ((int)discounts.First().DiscountPercentage).ToString());
                        else
                            model.ProductVariantPrice.DiscountPercentage = String.Format("(%{0})", ((int)discounts.First().DiscountPercentage).ToString());

                    }
                    model.ProductVariantPrice.PriceValue = finalPriceWithoutDiscount;
                    model.ProductVariantPrice.PriceWithDiscountValue = finalPriceWithDiscount;
                    model.ProductVariantPrice.Currency = CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol;
                    model.CallForPriceMessageTemplateId = productVariant.MessageTemplateId;
                    
                }
            }
            else
            {
                model.ProductVariantPrice.HidePrices = true;
                model.ProductVariantPrice.OldPrice = null;
                model.ProductVariantPrice.Price = null;
            }
            #endregion

            return model;
        }

        //sitemap page
        public ActionResult Sitemap()
        {
            if (!_commonSettings.SitemapEnabled)
                return RedirectToAction("Index", "Home");

            var model = new SitemapModel();
            if (_commonSettings.SitemapIncludeCategories)
            {
                var categories = _categoryService.GetAllCategories();
                model.Categories = categories.Select(x => x.ToModel()).ToList();
            }
            if (_commonSettings.SitemapIncludeManufacturers)
            {
                var manufacturers = _manufacturerService.GetAllManufacturers();
                model.Manufacturers = manufacturers.Select(x => x.ToModel()).ToList();
            }
            if (_commonSettings.SitemapIncludeProducts)
            {
                //limit product to 200 until paging is supported on this page
                var products = _productService.SearchProducts(0, 0, null, null, null, 0, null, false, 0, null,
                     ProductSortingEnum.Position, 0, 200);
                model.Products = products.Select(x => x.ToModel()).ToList();
            }
            if (_commonSettings.SitemapIncludeTopics)
            {
                var topics = _topicService.GetAllTopics().ToList().FindAll(t => t.IncludeInSitemap);
                model.Topics = topics.Select(x => x.ToModel()).ToList();
            }
            return View(model);
        }

        //SEO sitemap page
        public ActionResult SitemapSeo()
        {
            if (!_commonSettings.SitemapEnabled)
                return RedirectToAction("Index", "Home");

            var siteMap = _cacheManager.Get("sitemapSeo", 1400, () =>
            {
                return _sitemapGenerator.Generate(false);                
            });

            return Content(siteMap, "text/xml");
        }

        public ActionResult SitemapImage()
        {
            if (!_commonSettings.SitemapEnabled)
                return RedirectToAction("Index", "Home");

            string siteMap = _sitemapGenerator.GenerateImageSitemap(true);

            return Content(siteMap, "text/xml");
        }

        //favicon
        [ChildActionOnly]
        public ActionResult Favicon()
        {
            var model = new FaviconModel()
            {
                Uploaded = System.IO.File.Exists(Request.PhysicalApplicationPath + "favicon.ico"),
                FaviconUrl = _webHelper.GetStoreLocation() + "favicon.ico"
            };

            return PartialView(model);
        }

        [ChildActionOnly]
        public ActionResult GoogleAnalytics(string placement)
        {
            //if (!_googleAnalyticsSettings.Enabled)
            //    return Content("");

            ////compare placement
            //if (String.IsNullOrEmpty(placement))
            //    throw new ArgumentNullException("placement");

            //if (!placement.Equals(_googleAnalyticsSettings.Placement, StringComparison.InvariantCultureIgnoreCase))
            //    return Content("");

            //return PartialView("GoogleAnalytics", _googleAnalyticsSettings.JavaScript);
            return new EmptyResult();
        }

    }
}
