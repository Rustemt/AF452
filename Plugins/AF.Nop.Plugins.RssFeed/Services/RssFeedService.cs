using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using AF.Nop.Plugins.RssFeed.Models.Xml;
using Nop.Core.Domain.Localization;
using Nop.Services.Localization;
using System.Web.Mvc;
using System.Web;
using System.Xml.Serialization;
using System.IO;
using Nop.Services.Logging;
using Nop.Core.Data;
using Nop.Services.Configuration;
using Nop.Services.Catalog;
using Nop.Core.Domain.Directory;
using Nop.Services.Directory;
using Nop.Core;
using System.Globalization;
using Nop.Services.Tax;
using Nop.Services.Media;
using Nop.Services.Seo;
using System.Xml;
using System.Text.RegularExpressions;
using Nop.Services.Customers;
using Nop.Core.Domain.Customers;

namespace AF.Nop.Plugins.RssFeed.Services
{

    public class RssFeedService : IRssFeedService
    {
        #region Fields & Ctor()
        protected Customer _defaultCustomer;
        protected Currency _storeCurrency;
        protected HttpContext _httpContext;
        protected UrlHelper _urlHelper;

        protected readonly IWebHelper _webHelper;
        protected readonly IWorkContext _workContext;
        protected readonly ITaxService _taxService;
        protected readonly CurrencySettings _currencySettings;
        protected readonly RssFeedSetting _rssFeedSettings;
        protected readonly ISettingService _settingService;
        protected readonly ILanguageService _languageService;
        protected readonly ICurrencyService _currencyService;
        protected readonly ICustomerService _customerService;
        protected readonly IPriceCalculationService _priceCalculationService;
        protected readonly ICategoryService _categoryService;
        protected readonly IPictureService _pictureService;
        protected readonly ILogger _logger;

        private readonly IRepository<ProductVariant> _productVariantRepository;

        public RssFeedService(IWebHelper webHelper
            , IWorkContext workContext
            , RssFeedSetting rssFeedSettings
            , CurrencySettings currencySettings
            , ISettingService settingService
            , ICurrencyService currencyService
            , ILanguageService languageService
            , ICustomerService customerService
            , IPriceCalculationService priceCalculationService
            , IRepository<ProductVariant> productVariantRepository
            , ITaxService taxService
            , ICategoryService categoryService
            , IPictureService pictureService
            , ILogger logger)
        {
            _webHelper = webHelper;
            _workContext = workContext;
            _settingService = settingService;
            _rssFeedSettings = rssFeedSettings;
            _currencyService = currencyService;
            _priceCalculationService = priceCalculationService;
            _languageService = languageService;
            _productVariantRepository = productVariantRepository;
            _categoryService = categoryService;
            _taxService = taxService;
            _pictureService = pictureService;
            _currencySettings = currencySettings;
            _customerService = customerService;
            _logger = logger;
            
        } 
        #endregion

        public HttpContext GetHttpContext(string defaultUrl)
        {
            if (_httpContext == null)
                    _httpContext = HttpContext.Current;
                if (_httpContext == null)
                {
                HttpRequest request = new HttpRequest("/", defaultUrl, "");
                HttpResponse response = new HttpResponse(new StringWriter());
                _httpContext = new HttpContext(request, response);
            }
                return _httpContext;
        }

        public UrlHelper GetUrlHelper(string defaultUrl)
        {
            if (_urlHelper == null)
                _urlHelper = new UrlHelper(GetHttpContext(defaultUrl).Request.RequestContext);

            return _urlHelper;
        }

        public Customer DefaultCustomer
        {
            get
            {
                if (_defaultCustomer == null)
                    _defaultCustomer = _workContext.CurrentCustomer;

                if (_defaultCustomer==null)
                {
                    var customerId = 1535;
                    _defaultCustomer = _customerService.GetCustomerById(customerId);
                    if (_defaultCustomer == null)
                        throw new Exception($"Fail to load the default customer #{customerId}");
                }
                return _defaultCustomer;
            }
        }

        public Currency StoreCurrency
        {
            get
            {
                if (_storeCurrency == null)
                {
                    _storeCurrency = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId);
                }
                return _storeCurrency;
            }
        }

        public IQueryable<ProductVariant> ProductRepository { get { return _productVariantRepository.Table; } }

        public void GeneratRssFeedFiles()
        {
            var products = _productVariantRepository
                .Table.Where(x => x.Published && x.Product.Published && !x.Deleted && !x.Product.Deleted)
                .OrderByDescending(x => x.UpdatedOnUtc)
                .ToList();
            GeneratRssFeedFiles(products);
        }

        public void GeneratRssFeedFiles(IList<ProductVariant> products)
        {
            try
            {
                var rssFeeds = CreateRssFeeds(products);

                var path = RssFeedHelper.GetFilePath("");
                Directory.CreateDirectory(path);

                var xmlSettings = new XmlWriterSettings();
                xmlSettings.Indent = true;
                xmlSettings.Encoding = Encoding.UTF8;

                foreach (var local in _rssFeedSettings.Locales)
                {
                    var fileName = Path.Combine(path, local.FileName);
                    string fileNameAll = Path.Combine(path, Path.GetFileNameWithoutExtension(local.FileName) + "-all." + Path.GetExtension(local.FileName));

                    var tt1 = DateTime.Now.Ticks;
                    var feedsAll = rssFeeds[local.LanguageId];
                    var feedsWithoutCallForPrice = new Rss20()
                    {
                        Version = feedsAll.Version,
                        Channel = new Rss20Channel()
                        {
                            Title = feedsAll.Channel.Title,
                            Link = feedsAll.Channel.Link,
                            Description = feedsAll.Channel.Description,
                            Items = feedsAll.Channel.Items.Where(x => x.CallForPrice == false).ToList(),
                        },
                    };

                    XmlSerializer serializer = new XmlSerializer(typeof(Rss20));
                    var ns = new XmlSerializerNamespaces();
                    ns.Add("g", RssFeedHelper.GoogleNamespce);

                    using (var stream = XmlWriter.Create(fileNameAll, xmlSettings))
                    {
                        serializer.Serialize(stream, feedsAll, ns);
                        stream.Close();
                        _logger.Information($"RssFeed: the file {fileNameAll} has been saved");
                    }
                    using (var stream = XmlWriter.Create(fileName, xmlSettings))
                    {
                        serializer.Serialize(stream, feedsWithoutCallForPrice, ns);
                        stream.Close();
                        _logger.Information($"RssFeed: the file {fileName} has been saved");
                    }

                }
            }
            catch (Exception ex)
            {
                _logger.Error("RssFeed: error occured in generating XML files", ex);
                throw;
            }
        }

        public void GeneratRssFeedFiles(XmlWriter stream, IList<ProductVariant> products, int languageId)
        {
            var rss = CreateRssFeeds(products);
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Rss20));
                var ns = new XmlSerializerNamespaces();
                ns.Add("g", RssFeedHelper.GoogleNamespce);
                serializer.Serialize(stream, rss[languageId], ns);

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public void SaveUpdateTime(DateTime time)
        {
            _rssFeedSettings.LastRunTime = time;
            _settingService.SaveSetting(_rssFeedSettings);
        }

        protected Dictionary<int, Rss20> CreateRssFeeds(IList<ProductVariant> products)
        {
            var dic = new Dictionary<int, Rss20>();

            if (_rssFeedSettings.Locales.Count == 0)
                throw new Exception("The RssFeed plugin is not configured");

            _logger.Information($"RssFeed: Start generating feeds for {products.Count} products");

            var t1 = DateTime.Now.Ticks;
            _urlHelper = null; // reset

            foreach (var L in _rssFeedSettings.Locales)
            {
                CleanInvalidXmlChars(L.Title);
                CleanInvalidXmlChars(L.Link);
                CleanInvalidXmlChars(L.Description);
                var rss = new Rss20(L.Title, L.Link, L.Description);
                dic[L.LanguageId] = rss;
            }

            int cntr = 0;

            foreach (var p in products)
            {
                cntr++;
                var item = CreateProductRssItems(p, _rssFeedSettings.Locales);
                if (cntr % 1000 == 0)
                    _logger.Information($"RssFeed: {cntr} products has been processed so far.");

                foreach (var k in item.Keys)
                    dic[k].Channel.Items.Add(item[k]);
            }

            var t2 = DateTime.Now.Ticks;
            var time = TimeSpan.FromTicks(t2 - t1);
            _logger.Information("RssFeed: Rss20 object is created within " + time);

            return dic;
        }

        protected Dictionary<int, Rss20Item> CreateProductRssItems(ProductVariant productVariant, IList<RssFeedSettingLocale> locales)
        {
            decimal price, salePrice;
            string imageUrl = null;
            var product = productVariant.Product;
            var dic = new Dictionary<int, Rss20Item>();


            var categories = _categoryService.GetProductCategoriesByProductId(product.Id).Select(x => x.Category).ToList();
            CalculateProductPrices(productVariant, StoreCurrency, out price, out salePrice);
            var brand = product.ProductManufacturers.OrderBy(x => x.DisplayOrder).FirstOrDefault();
            var picture = product.GetDefaultProductPicture(_pictureService);
            if (picture != null)
            {
                imageUrl = _pictureService.GetPictureUrl(picture, _rssFeedSettings.ImageSize).Replace("https://", "http://");
            }

            foreach (var L in locales)
            {
                var lang = _languageService.GetLanguageById(L.LanguageId); ;
                var item = new Rss20Item()
                {
                    ProductId = product.Id,
                    Title = product.GetLocalized(x => x.Name, lang.Id),
                    Description = product.GetLocalized(x => x.FullDescription, lang.Id),
                    Price = FormatPrice(price, StoreCurrency),
                    SalePrice = FormatPrice(salePrice, StoreCurrency),
                    Link = GetProductUrl(productVariant, lang, L.Link),
                    MPN = productVariant.Sku,
                    Gtin = productVariant.Sku,
                    Condition = "new",
                    Image = imageUrl,
                    Availability = productVariant.StockQuantity > 0 ? "in stock" : "out of stock",
                    ExpirationDate = DateTime.Now.AddDays(29).ToString("yyyy-MM-dd"),
                    CallForPrice = productVariant.CallForPrice,
                };

                item.ProductType.AddRange(GetCategorieBreadCrumbs(categories, lang));

                if (!string.IsNullOrEmpty(item.Description))
                    item.Description = RssFeedHelper.HtmlToPlainText(item.Description);

                if (brand != null)
                    item.Brand = brand.Manufacturer.GetLocalized(x => x.Name, lang.Id);
                
                dic[L.LanguageId]=  item;
            }

            //CleanInvalidXmlChars(item.Title);
            //CleanInvalidXmlChars(item.MPN);
            //CleanInvalidXmlChars(item.Gtin);
            //CleanInvalidXmlChars(item.Brand);
            //CleanInvalidXmlChars(item.Description);

            return dic;
        }

        protected void CalculateProductPrices(ProductVariant product, Currency currency, out decimal price, out decimal dicountedPrice)
        {
            decimal tax = 0, tax2=0;

            price = _priceCalculationService.GetFinalPrice(product, DefaultCustomer, false, false);
            dicountedPrice = _priceCalculationService.GetFinalPrice(product, DefaultCustomer, true, false);

            price = _taxService.GetProductPrice(product, price, DefaultCustomer, out tax);
            dicountedPrice = _taxService.GetProductPrice(product, dicountedPrice, DefaultCustomer, out tax2);

            //price = _currencyService.ConvertFromPrimaryStoreCurrency(price, currency);
            //dicountedPrice = _currencyService.ConvertFromPrimaryStoreCurrency(dicountedPrice, currency);
        }

        protected string GetProductUrl(ProductVariant productVariant, Language lang, string baseUrl)
        {
            try
            {
                var product = productVariant.Product;
                var seName = product.GetLocalized(x => x.SeName, lang.Id);
                if (string.IsNullOrEmpty(seName))
                    seName = product.SeName;
                if (!string.IsNullOrEmpty(seName))
                    seName = SeoExtensions.GetSeName(seName, true);

                //var helper = GetUrlHelper(baseUrl);
                //helper.RequestContext.HttpContext;
                var link = $"{baseUrl}/p/{product.Id}/{seName}/{productVariant.Id}";// helper.RouteUrl("Product", new { productId = product.Id, SeName = seName, variantId = productVariant.Id });
                return link;
                //return baseUrl + "/"+lang.UniqueSeoCode + link;

                //var urlBuilder = new UriBuilder()
                //{
                //    Path = lang.UniqueSeoCode + link,
                //    Query = null,
                //    Scheme = "http",
                //};


                //return urlBuilder.ToString().Replace("https://", "http://");
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        protected string FormatPrice(decimal price, Currency currency)
        {
            return Math.Round(price, 2).ToString(CultureInfo.InvariantCulture) + " " + currency.CurrencyCode;
        }

        protected List<string> GetCategorieBreadCrumbs(IList<Category> categories, Language lang)
        {
            string crumb = null;
            var types = new List<string>();
            foreach (var c in categories)
            {
                crumb = GetCategoryBreadCrumb(c, lang);
                crumb = crumb.Replace(">>", ">");
                types.Add(crumb);
            }

            return types;
        }

        protected string GetCategoryBreadCrumb(Category category, Language lang)
        {
            string result = string.Empty;
            string name = "";

            while (category != null && !category.Deleted)
            {
                name = category.GetLocalized(x => x.Name, lang.Id);
                if (String.IsNullOrEmpty(result))
                    result = name;
                else
                    result = name + " >> " + result;

                category = _categoryService.GetCategoryById(category.ParentCategoryId);

            }

            CleanInvalidXmlChars(result);
            return result;
        }

        private static Regex _invalidXMLChars = new Regex(@"(?<![\uD800-\uDBFF])[\uDC00-\uDFFF]|[\uD800-\uDBFF](?![\uDC00-\uDFFF])|[\x00-\x08\x0B\x0C\x0E-\x1F\x7F-\x9F\uFEFF\uFFFE\uFFFF]", RegexOptions.Compiled);

        public static void CleanInvalidXmlChars(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            string re = @"[^\x09\x0A\x0D\x20-\xD7FF\xE000-\xFFFD\x10000-x10FFFF]";
            text = _invalidXMLChars.Replace(text, "");
        }
    }
}
