using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;
using Nop.Plugin.Shipping.ByWeight.Domain;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Media;
using System.Xml.Linq;
using System.Text;
using Nop.Services.Directory;
using Nop.Core.Domain.Directory;
using System.Collections.Generic;
using OfficeOpenXml;
using Nop.Plugin.Shipping.ByWeight;
using Nop.Plugin.Shipping.ByWeight.Services;
using OfficeOpenXml.Style;
using System.Web;
using System.Drawing;




namespace Nop.Services.ExportImport
{
    /// <summary>
    /// Import manager
    /// </summary>
    public partial class ImportManager : IImportManager
    {
        private readonly IProductService _productService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IPictureService _pictureService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly ICountryService _countryService;
        private readonly IShippingByWeightService _shippingByWeightService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly HttpContextBase _httpContext;

        public ImportManager(IProductService productService, ILanguageService languageService,
            ILocalizationService localizationService, ICategoryService categoryService,
            IManufacturerService manufacturerService, IPictureService pictureService,
            ILocalizedEntityService localizedEntityService,
            ICurrencyService currencyService, CurrencySettings currencySettings,ICountryService countryService,
            IShippingByWeightService shippingByWeightService, IProductAttributeService productAttributeService,HttpContextBase httpContext)
        {
            this._productService = productService;
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._pictureService = pictureService;
            this._localizedEntityService = localizedEntityService;
            this._currencyService = currencyService;
            this._currencySettings = currencySettings;
            this._countryService = countryService;
            this._shippingByWeightService = shippingByWeightService;
            this._productAttributeService = productAttributeService;
            this._httpContext = httpContext;
        }

        protected virtual int GetColumnIndex(string[] properties, string columnName)
        {
            if (properties == null)
                throw new ArgumentNullException("properties");

            if (columnName == null)
                throw new ArgumentNullException("columnName");

            for (int i = 0; i < properties.Length; i++)
                if (properties[i].Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                    return i + 1; //excel indexes start from 1
            return 0;
        }


        public virtual void ImportShippingByWeightXlsx (string filePath)
        {
            var newFile = new FileInfo(filePath);

            using (var xlPackage = new ExcelPackage(newFile))
            {
                var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                {
                    throw new NopException("worksheet not found");
                }

                int iRow = 0;
                int iColumn = 2;
                int countryId = 0;
                decimal To = 0;
                decimal From = 0;

                for (int i = iColumn; i < worksheet.Dimension.End.Column; i++)
                {
                    string columnName = worksheet.Cells[1, i].Value.ToString();

                    if (_countryService.GetAllCountries().Select(x => x.TwoLetterIsoCode).Contains(columnName) == false)
                        //throw new NopException("Ülke Kodu Kayıtlı değil. - " + columnName );
                        continue;

                    iRow = 2;
                    while (true)
                    {
                        countryId = _countryService.GetCountryByTwoLetterIsoCode(columnName.ToString()).Id;
                        To = Convert.ToDecimal(worksheet.Cells[iRow, 1].Value);
                        if (To == 500)
                        {
                            From = 0;
                        }
                        else if (To <= 10000 && 500 < To)
                        {
                            From = To - 500 + 1;
                        }
                        else
                        {
                            From = To - 1000 + 1;
                        }
                        
                        decimal ShippingChargeAmount = Convert.ToDecimal(worksheet.Cells[iRow, i].Value);

                        var model = new ShippingByWeightRecord()
                                    {
                                        CountryId = countryId,
                                        ShippingMethodId = 4,
                                        From = From,
                                        To = To,
                                        ShippingChargeAmount = ShippingChargeAmount,
                                        CurrencyId = 6
                                    };

                        _shippingByWeightService.InsertShippingByWeightRecord(model);

                        iRow ++;

                        if (worksheet.Cells[iRow,iColumn].Value == string.Empty || worksheet.Cells[iRow,iColumn].Value == null)
                        {
                            break;
                        }

                    }

                }
            }


        }

        public virtual string ImportProductsForStockUpdateFromXlsx(string filePath)
        {
            var file = new FileInfo(filePath);
            var languages = _languageService.GetAllLanguages(true);
            bool checkActivity = true;
            Dictionary<string, string>  MessageList = new Dictionary<string, string>();

            using (var xlPackage = new ExcelPackage(file))
            {
                var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                    throw new NopException("No worksheet found");

                string gtin = "";
                int rowCounter = 2;
                int columnCounter = 1;
                bool state =false;
                string sku = "";

                while (true)
                {
                    if (worksheet.Cells[1,columnCounter].Value.ToString() == "Miktar")
                        break;
                    columnCounter++;
                }

                while (true)
                {
                    state = false;
                    if(worksheet.Cells[rowCounter,1].Value == null || worksheet.Cells[rowCounter,columnCounter].Value == null )
                    {
                        state = true;
                    }

                    if (worksheet.Cells[rowCounter, 1].Value == null && worksheet.Cells[rowCounter, 2].Value == null && worksheet.Cells[rowCounter, columnCounter].Value == null)
                        break;
                    
                    if (state)
                    {
                        rowCounter++;
                        continue;
                    }

                    sku = worksheet.Cells[rowCounter, 1].Value.ToString().Trim();
                    if (worksheet.Cells[rowCounter, 2].Value != null)
                    gtin= worksheet.Cells[rowCounter,2].Value.ToString().Trim();

                    var variant = _productService.GetProductVariantBySku(sku);

                    if (variant == null)
                    {
                        if(!MessageList.ContainsKey(sku))
                        MessageList.Add(sku, "Bu sku ya ait variant bulunamadı");
                        
                        rowCounter++;
                        continue;
                    }
                    else
                    {
                        int quantity  = Convert.ToInt32(worksheet.Cells[rowCounter, columnCounter].Value);

                        if (variant.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
                        {   
                             if ((variant.Published == false || variant.Product.Published == false) && variant.StockQuantity == 0 && quantity > 0)
                                 MessageList.Add(sku, "Bu sku ya ait  Ürün veya variantı Unpublish, stok miktarı ise 0 iken artık 0'dan farklı. Lütfen kontrol ediniz.");

                             variant.StockQuantity = quantity;
                             _productService.UpdateProductVariant(variant);

                        }

                        else if (variant.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes)
                        {
                            var attrComb = _productAttributeService.GetProductVariantAttributeCombinationByBarcode(gtin);

                            if (attrComb != null)
                            {
                                if (attrComb.StockQuantity == 0 && quantity > 0 && (variant.Published == false || variant.Product.Published == false))
                                    MessageList.Add(gtin, "Bu gtin numarasına ait  kombinasyonun bulunduğu ürün veya variant unpublish, stok miktarı 0 iken artık 0'dan farklı. Sku :  "+sku+"");
                                
                                attrComb.StockQuantity = int.Parse(worksheet.Cells[rowCounter, columnCounter].Value.ToString());
                                _productService.UpdateProductVariant(variant);
                            }
                            else
                            {
                                MessageList.Add(gtin, "Bu gtin numarasına   ait kombinasyon bulunamadı  Sku : "+sku+"");
                                rowCounter++;
                                continue;
                            }
                        }
                    }

                    rowCounter++;
                }
            }

            string returnedFileName = string.Format("Güncelleme_Sonuclari{0}_{1}.xlsx", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), CommonHelper.GenerateRandomDigitCode(4));
            string returnedFilePath = string.Format("{0}content\\files\\ExportImport\\{1}", _httpContext.Request.PhysicalApplicationPath, returnedFileName);

            var newFile = new FileInfo(returnedFilePath);

             using (var xlPackage = new ExcelPackage(newFile))
            {
                 var worksheet = xlPackage.Workbook.Worksheets.Add("SKU");

                 worksheet.Cells[1,1].Value="SKU-GTİN";
                 worksheet.Cells[1, 2].Value = "Hata Sebebi";
                 worksheet.Cells[1,1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                 worksheet.Cells[1, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                 worksheet.Cells[1, 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(184, 204, 228));
                 worksheet.Cells[1, 2].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(184, 204, 228));
                 worksheet.Cells[1, 1].Style.Font.Bold = true;
                 worksheet.Cells[1, 2].Style.Font.Bold = true;
                 int  rowCount=2;

                 foreach (KeyValuePair<string, string> item in MessageList)
                 {
                     worksheet.Cells[rowCount, 1].Value = item.Key.ToString();
                     worksheet.Cells[rowCount, 2].Value = item.Value.ToString();
                     rowCount++;
                 }

                 xlPackage.Save();

                 return returnedFileName;
            }
        }

        /// <summary>
        /// Import products from XLSX file
        /// </summary>
        /// <param name="filePath">Excel file path</param>
        public virtual void ImportProductsFromXlsx(string filePath)
        {
            
            var newFile = new FileInfo(filePath);
            var languages = _languageService.GetAllLanguages(true);
            bool checkActivityState = true;
            // ok, we can run the real code of the sample now
            using (var xlPackage = new ExcelPackage(newFile))
            {
                // get the first worksheet in the workbook
                var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                    throw new NopException("No worksheet found");
                
                //the columns
                var properties = new string[]
                {
                    "Name",
                    "ShortDescription",
                    "FullDescription",
                    "ProductTemplateId",
                    "ShowOnHomePage",
                    "MetaKeywords",
                    "MetaDescription",
                    "MetaTitle",
                    "SeName",
                    "AllowCustomerReviews",
                    "Published",
                    "SKU",
                    "ManufacturerPartNumber",
                    "Gtin",
                    "IsGiftCard",
                    "GiftCardTypeId",
                    "RequireOtherProducts",
                    "RequiredProductVariantIds",
                    "AutomaticallyAddRequiredProductVariants",
                    "IsDownload",
                    "DownloadId",
                    "UnlimitedDownloads",
                    "MaxNumberOfDownloads",
                    "DownloadActivationTypeId",
                    "HasSampleDownload",
                    "SampleDownloadId",
                    "HasUserAgreement",
                    "UserAgreementText",
                    "IsRecurring",
                    "RecurringCycleLength",
                    "RecurringCyclePeriodId",
                    "RecurringTotalCycles",
                    "IsShipEnabled",
                    "IsFreeShipping",
                    "AdditionalShippingCharge",
                    "IsTaxExempt",
                    "TaxCategoryId",
                    "ManageInventoryMethodId",
                    "StockQuantity",
                    "DisplayStockAvailability",
                    "DisplayStockQuantity",
                    "MinStockQuantity",
                    "LowStockActivityId",
                    "NotifyAdminForQuantityBelow",
                    "BackorderModeId",
                    "AllowBackInStockSubscriptions",
                    "OrderMinimumQuantity",
                    "OrderMaximumQuantity",
                    "DisableBuyButton",
                    "DisableWishlistButton",
                    "CallForPrice",
                    "Price",
                    "OldPrice",
                    "ProductCost",
                    "SpecialPrice",
                    "SpecialPriceStartDateTimeUtc",
                    "SpecialPriceEndDateTimeUtc",
                    "CustomerEntersPrice",
                    "MinimumCustomerEnteredPrice",
                    "MaximumCustomerEnteredPrice",
                    "Weight",
                    "Length",
                    "Width",
                    "Height",
                    "CreatedOnUtc",
                    "CategoryIds",
                    "ManufacturerIds",
                    "Picture1",
                    "Picture2",
                    "Picture3",
                    "CurrencyId",
                    "CurrencyPrice",
                    "CurrencyOldPrice",
                    "CurrencyProductCost",
                    "DisplayPriceIfCallforPrice",
                    "HideDiscount"

                };


                int iRow = 1;
                bool[] columnActivityStates = new bool[properties.Length+1];

                for (int i=1;i<columnActivityStates.Length;i++)
                {
                    var header = worksheet.Cells[iRow, i].Value as string;
                    if (header == null) continue;
                    columnActivityStates[i] = header.StartsWith("#");
                }
                
                iRow = 2;
                while (true)
                {
                    bool allColumnsAreEmpty = true;
                    for (var i = 1; i <= properties.Length; i++)
                        if (worksheet.Cells[iRow, i].Value != null && !String.IsNullOrEmpty(worksheet.Cells[iRow, i].Value.ToString()))
                        {
                            allColumnsAreEmpty = false;
                            break;
                        }
                    if (allColumnsAreEmpty)
                        break;
                    
                    string name = worksheet.Cells[iRow, GetColumnIndex(properties, "Name")].Value as string;
                    string shortDescription = worksheet.Cells[iRow, GetColumnIndex(properties, "ShortDescription")].Value as string;
                    string fullDescription = worksheet.Cells[iRow, GetColumnIndex(properties, "FullDescription")].Value as string;
                    int productTemplateId = Convert.ToInt32(worksheet.Cells[iRow, GetColumnIndex(properties, "ProductTemplateId")].Value);
                    bool showOnHomePage = Convert.ToBoolean(worksheet.Cells[iRow, GetColumnIndex(properties, "ShowOnHomePage")].Value);
                    string metaKeywords = worksheet.Cells[iRow, GetColumnIndex(properties, "MetaKeywords")].Value as string;
                    string metaDescription = worksheet.Cells[iRow, GetColumnIndex(properties, "MetaDescription")].Value as string;
                    string metaTitle = worksheet.Cells[iRow, GetColumnIndex(properties, "MetaTitle")].Value as string;
                    string seName = worksheet.Cells[iRow, GetColumnIndex(properties, "SeName")].Value as string;
                    bool allowCustomerReviews = Convert.ToBoolean(worksheet.Cells[iRow, GetColumnIndex(properties, "AllowCustomerReviews")].Value);
                    bool published = Convert.ToBoolean(worksheet.Cells[iRow, GetColumnIndex(properties, "Published")].Value);
                    string sku = worksheet.Cells[iRow, GetColumnIndex(properties, "SKU")].Value.ToString();
                    string manufacturerPartNumber = worksheet.Cells[iRow, GetColumnIndex(properties, "ManufacturerPartNumber")].Value as string;
                    string gtin = worksheet.Cells[iRow, GetColumnIndex(properties, "Gtin")].Value as string;
                    bool isGiftCard = Convert.ToBoolean(worksheet.Cells[iRow, GetColumnIndex(properties, "IsGiftCard")].Value);
                    int giftCardTypeId = Convert.ToInt32(worksheet.Cells[iRow, GetColumnIndex(properties, "GiftCardTypeId")].Value);
                    bool requireOtherProducts = Convert.ToBoolean(worksheet.Cells[iRow, GetColumnIndex(properties, "RequireOtherProducts")].Value);
                    string requiredProductVariantIds = worksheet.Cells[iRow, GetColumnIndex(properties, "RequiredProductVariantIds")].Value as string;
                    bool automaticallyAddRequiredProductVariants = Convert.ToBoolean(worksheet.Cells[iRow, GetColumnIndex(properties, "AutomaticallyAddRequiredProductVariants")].Value);
                    bool isDownload = Convert.ToBoolean(worksheet.Cells[iRow, GetColumnIndex(properties, "IsDownload")].Value);
                    int downloadId = Convert.ToInt32(worksheet.Cells[iRow, GetColumnIndex(properties, "DownloadId")].Value);
                    bool unlimitedDownloads = Convert.ToBoolean(worksheet.Cells[iRow, GetColumnIndex(properties, "UnlimitedDownloads")].Value);
                    int maxNumberOfDownloads = Convert.ToInt32(worksheet.Cells[iRow, GetColumnIndex(properties, "MaxNumberOfDownloads")].Value);
                    int downloadActivationTypeId = Convert.ToInt32(worksheet.Cells[iRow, GetColumnIndex(properties, "DownloadActivationTypeId")].Value);
                    bool hasSampleDownload = Convert.ToBoolean(worksheet.Cells[iRow, GetColumnIndex(properties, "HasSampleDownload")].Value);
                    int sampleDownloadId = Convert.ToInt32(worksheet.Cells[iRow, GetColumnIndex(properties, "SampleDownloadId")].Value);
                    bool hasUserAgreement = Convert.ToBoolean(worksheet.Cells[iRow, GetColumnIndex(properties, "HasUserAgreement")].Value);
                    string userAgreementText = worksheet.Cells[iRow, GetColumnIndex(properties, "UserAgreementText")].Value as string;
                    bool isRecurring = Convert.ToBoolean(worksheet.Cells[iRow, GetColumnIndex(properties, "IsRecurring")].Value);
                    int recurringCycleLength = Convert.ToInt32(worksheet.Cells[iRow, GetColumnIndex(properties, "RecurringCycleLength")].Value);
                    int recurringCyclePeriodId = Convert.ToInt32(worksheet.Cells[iRow, GetColumnIndex(properties, "RecurringCyclePeriodId")].Value);
                    int recurringTotalCycles = Convert.ToInt32(worksheet.Cells[iRow, GetColumnIndex(properties, "RecurringTotalCycles")].Value);
                    bool isShipEnabled = Convert.ToBoolean(worksheet.Cells[iRow, GetColumnIndex(properties, "IsShipEnabled")].Value);
                    bool isFreeShipping = Convert.ToBoolean(worksheet.Cells[iRow, GetColumnIndex(properties, "IsFreeShipping")].Value);
                    decimal additionalShippingCharge = Convert.ToDecimal(worksheet.Cells[iRow, GetColumnIndex(properties, "AdditionalShippingCharge")].Value);
                    bool isTaxExempt = Convert.ToBoolean(worksheet.Cells[iRow, GetColumnIndex(properties, "IsTaxExempt")].Value);
                    int taxCategoryId = Convert.ToInt32(worksheet.Cells[iRow, GetColumnIndex(properties, "TaxCategoryId")].Value);
                    int manageInventoryMethodId = Convert.ToInt32(worksheet.Cells[iRow, GetColumnIndex(properties, "ManageInventoryMethodId")].Value);
                    int stockQuantity = Convert.ToInt32(worksheet.Cells[iRow, GetColumnIndex(properties, "StockQuantity")].Value);
                    bool displayStockAvailability = Convert.ToBoolean(worksheet.Cells[iRow, GetColumnIndex(properties, "DisplayStockAvailability")].Value);
                    bool displayStockQuantity = Convert.ToBoolean(worksheet.Cells[iRow, GetColumnIndex(properties, "DisplayStockQuantity")].Value);
                    int minStockQuantity = Convert.ToInt32(worksheet.Cells[iRow, GetColumnIndex(properties, "MinStockQuantity")].Value);
                    int lowStockActivityId = Convert.ToInt32(worksheet.Cells[iRow, GetColumnIndex(properties, "LowStockActivityId")].Value);
                    int notifyAdminForQuantityBelow = Convert.ToInt32(worksheet.Cells[iRow, GetColumnIndex(properties, "NotifyAdminForQuantityBelow")].Value);
                    int backorderModeId = Convert.ToInt32(worksheet.Cells[iRow, GetColumnIndex(properties, "BackorderModeId")].Value);
                    bool allowBackInStockSubscriptions = Convert.ToBoolean(worksheet.Cells[iRow, GetColumnIndex(properties, "AllowBackInStockSubscriptions")].Value);
                    int orderMinimumQuantity = Convert.ToInt32(worksheet.Cells[iRow, GetColumnIndex(properties, "OrderMinimumQuantity")].Value);
                    int orderMaximumQuantity = Convert.ToInt32(worksheet.Cells[iRow, GetColumnIndex(properties, "OrderMaximumQuantity")].Value);
                    bool disableBuyButton = Convert.ToBoolean(worksheet.Cells[iRow, GetColumnIndex(properties, "DisableBuyButton")].Value);
                    bool disableWishlistButton = Convert.ToBoolean(worksheet.Cells[iRow, GetColumnIndex(properties, "DisableWishlistButton")].Value);
                    bool callForPrice = Convert.ToBoolean(worksheet.Cells[iRow, GetColumnIndex(properties, "CallForPrice")].Value);
                    decimal price = Convert.ToDecimal(worksheet.Cells[iRow, GetColumnIndex(properties, "Price")].Value);
                    decimal oldPrice = Convert.ToDecimal(worksheet.Cells[iRow, GetColumnIndex(properties, "OldPrice")].Value);
                    decimal productCost = Convert.ToDecimal(worksheet.Cells[iRow, GetColumnIndex(properties, "ProductCost")].Value);
                    decimal? specialPrice = null;
                    var specialPriceExcel = worksheet.Cells[iRow, GetColumnIndex(properties, "SpecialPrice")].Value;
                    if (specialPriceExcel != null)
                        specialPrice = Convert.ToDecimal(specialPriceExcel);
                    DateTime? specialPriceStartDateTimeUtc = null;
                    var specialPriceStartDateTimeUtcExcel = worksheet.Cells[iRow, GetColumnIndex(properties, "SpecialPriceStartDateTimeUtc")].Value;
                    if (specialPriceStartDateTimeUtcExcel != null)
                        specialPriceStartDateTimeUtc = DateTime.FromOADate(Convert.ToDouble(specialPriceStartDateTimeUtcExcel));
                    DateTime? specialPriceEndDateTimeUtc = null;
                    var specialPriceEndDateTimeUtcExcel = worksheet.Cells[iRow, GetColumnIndex(properties, "SpecialPriceEndDateTimeUtc")].Value;
                    if (specialPriceEndDateTimeUtcExcel != null)
                        specialPriceEndDateTimeUtc = DateTime.FromOADate(Convert.ToDouble(specialPriceEndDateTimeUtcExcel));
                    
                    bool customerEntersPrice = Convert.ToBoolean(worksheet.Cells[iRow, GetColumnIndex(properties, "CustomerEntersPrice")].Value);
                    decimal minimumCustomerEnteredPrice = Convert.ToDecimal(worksheet.Cells[iRow, GetColumnIndex(properties, "MinimumCustomerEnteredPrice")].Value);
                    decimal maximumCustomerEnteredPrice = Convert.ToDecimal(worksheet.Cells[iRow, GetColumnIndex(properties, "MaximumCustomerEnteredPrice")].Value);
                    decimal weight = Convert.ToDecimal(worksheet.Cells[iRow, GetColumnIndex(properties, "Weight")].Value);
                    decimal length = Convert.ToDecimal(worksheet.Cells[iRow, GetColumnIndex(properties, "Length")].Value);
                    decimal width = Convert.ToDecimal(worksheet.Cells[iRow, GetColumnIndex(properties, "Width")].Value);
                    decimal height = Convert.ToDecimal(worksheet.Cells[iRow, GetColumnIndex(properties, "Height")].Value);
                    DateTime createdOnUtc = DateTime.FromOADate(Convert.ToDouble(worksheet.Cells[iRow, GetColumnIndex(properties, "CreatedOnUtc")].Value));
                    string categoryIds = worksheet.Cells[iRow, GetColumnIndex(properties, "CategoryIds")].Value as string;
                    string manufacturerIds = worksheet.Cells[iRow, GetColumnIndex(properties, "ManufacturerIds")].Value as string;
                    string picture1 = worksheet.Cells[iRow, GetColumnIndex(properties, "Picture1")].Value as string;
                    string picture2 = worksheet.Cells[iRow, GetColumnIndex(properties, "Picture2")].Value as string;
                    string picture3 = worksheet.Cells[iRow, GetColumnIndex(properties, "Picture3")].Value as string;


                    int? currencyId = null;
                     var currencyIdExcel = worksheet.Cells[iRow, GetColumnIndex(properties, "CurrencyId")].Value;
                      if (currencyIdExcel != null)
                        currencyId = Convert.ToInt32(currencyIdExcel);
                   
                    Decimal? currencyPrice = null;
                    var currencyPriceExcel = worksheet.Cells[iRow, GetColumnIndex(properties, "CurrencyPrice")].Value;
                      if (currencyPriceExcel != null)
                        currencyPrice = Convert.ToDecimal(currencyPriceExcel);

                    //calculate store currency price
                      if (currencyId.HasValue && currencyPrice.HasValue)
                      {
                          if (currencyId.Value != _currencySettings.PrimaryStoreCurrencyId)
                          {
                              var cur = _currencyService.GetCurrencyById(currencyId.Value);
                              price = _currencyService.ConvertToPrimaryStoreCurrency(currencyPrice.Value, cur);
                          }
                      }


                    Decimal? currencyOldPrice = null;
                    var currencyOldPriceExcel = worksheet.Cells[iRow, GetColumnIndex(properties, "CurrencyOldPrice")].Value;
                      if (currencyOldPriceExcel != null)
                        currencyOldPrice = Convert.ToDecimal(currencyOldPriceExcel);
                      //calculate store currency price
                      if (currencyId.HasValue && currencyOldPrice.HasValue)
                      {
                          if (currencyId.Value != _currencySettings.PrimaryStoreCurrencyId)
                          {
                              var cur = _currencyService.GetCurrencyById(currencyId.Value);
                              oldPrice = _currencyService.ConvertToPrimaryStoreCurrency(currencyOldPrice.Value, cur);
                          }
                      }



                    Decimal? currencyProductCost = null;
                    var currencyProductCostExcel = worksheet.Cells[iRow, GetColumnIndex(properties, "CurrencyProductCost")].Value;
                      if (currencyProductCostExcel != null)
                        currencyProductCost = Convert.ToDecimal(currencyProductCostExcel);
                      //calculate store currency price
                      if (currencyId.HasValue && currencyProductCost.HasValue)
                      {
                          if (currencyId.Value != _currencySettings.PrimaryStoreCurrencyId)
                          {
                              var cur = _currencyService.GetCurrencyById(currencyId.Value);
                              productCost = _currencyService.ConvertToPrimaryStoreCurrency(currencyProductCost.Value, cur);
                          }
                      }

                    bool displayPriceIfCallforPrice = Convert.ToBoolean(worksheet.Cells[iRow, GetColumnIndex(properties, "DisplayPriceIfCallforPrice")].Value);
                    bool hideDiscount = Convert.ToBoolean(worksheet.Cells[iRow, GetColumnIndex(properties, "HideDiscount")].Value);

                    var productVariant = _productService.GetProductVariantBySku(sku);
                    if (productVariant != null)
                    {
                        var product = productVariant.Product;
                        string[] localizedItems = null;
                        //product.Name = name;
                        if (columnActivityStates[GetColumnIndex(properties, "Name")])
                        {
                            localizedItems = name.Split(new string[] { "###" }, StringSplitOptions.None);
                            foreach (var item in localizedItems)
                            {
                                if (string.IsNullOrWhiteSpace(item)) continue;
                                var langSeo = item.Substring(0, 2);
                                var language = languages.FirstOrDefault(l => l.UniqueSeoCode == langSeo);
                                if (language != null)
                                {
                                    _localizedEntityService.SaveLocalizedValue(product,
                                                                   x => x.Name,
                                                                   item.Substring(2),
                                                                   language.Id);
                                }
                            }
                        }

                        //product.ShortDescription = shortDescription;
                        if (columnActivityStates[GetColumnIndex(properties, "ShortDescription")])
                        {
                            localizedItems = shortDescription.Split(new string[] { "###" }, StringSplitOptions.None);
                            string langSeo = null;
                            foreach (var item in localizedItems)
                            {
                               
                                if (item != localizedItems[0])
                                {
                                     langSeo = item.Substring(0, 2);
                                }
                                var language = languages.FirstOrDefault(l => l.UniqueSeoCode == langSeo);
                                if (language != null)
                                {
                                    _localizedEntityService.SaveLocalizedValue(product,
                                                                   x => x.ShortDescription,
                                                                   item.Substring(2),
                                                                   language.Id);
                                }
                            }
                        }
                        //product.FullDescription = fullDescription;
                        if (columnActivityStates[GetColumnIndex(properties, "FullDescription")])
                        {
                            localizedItems = fullDescription.Split(new string[] { "###" }, StringSplitOptions.None);
                            string langSeo = null;
                            foreach (var item in localizedItems)
                            {
                                if (item != localizedItems[0])
                                {
                                     langSeo = item.Substring(0, 2);
                                }
                                var language = languages.FirstOrDefault(l => l.UniqueSeoCode == langSeo);
                                if (language != null)
                                {
                                    _localizedEntityService.SaveLocalizedValue(product,
                                                                   x => x.FullDescription,
                                                                   item.Substring(2),
                                                                   language.Id);
                                }
                            }
                        } 
                        product.ProductTemplateId = productTemplateId;
                        product.ShowOnHomePage = showOnHomePage;
                        //product.MetaKeywords = metaKeywords;
                        if (columnActivityStates[GetColumnIndex(properties, "MetaKeywords")])
                        {
                            localizedItems = metaKeywords.Split(new string[] { "###" }, StringSplitOptions.None);
                            string langSeo = null;
                            foreach (var item in localizedItems)
                            {
                                if (item != localizedItems[0])
                                {
                                    langSeo = item.Substring(0, 2);
                                }
                                var language = languages.FirstOrDefault(l => l.UniqueSeoCode == langSeo);
                                if (language != null)
                                {
                                    _localizedEntityService.SaveLocalizedValue(product,
                                                                   x => x.MetaKeywords,
                                                                   item.Substring(2),
                                                                   language.Id);
                                }
                            }
                        }
                        //product.MetaDescription = metaDescription;
                        if (columnActivityStates[GetColumnIndex(properties, "MetaDescription")])
                        {
                            localizedItems = metaDescription.Split(new string[] { "###" }, StringSplitOptions.None);
                            string langSeo = null;
                            foreach (var item in localizedItems)
                            {
                                if (item != localizedItems[0])
                                {
                                    langSeo = item.Substring(0, 2);
                                }
                                var language = languages.FirstOrDefault(l => l.UniqueSeoCode == langSeo);
                                if (language != null)
                                {
                                    _localizedEntityService.SaveLocalizedValue(product,
                                                                   x => x.MetaDescription,
                                                                   item.Substring(2),
                                                                   language.Id);
                                }
                            }
                        }

                        //product.MetaTitle = metaTitle;
                        if (columnActivityStates[GetColumnIndex(properties, "MetaTitle")])
                        {
                            localizedItems = metaTitle.Split(new string[] { "###" }, StringSplitOptions.None);
                            string langSeo = null;
                            foreach (var item in localizedItems)
                            {
                                if (item != localizedItems[0])
                                {
                                    langSeo = item.Substring(0, 2);
                                }
                                var language = languages.FirstOrDefault(l => l.UniqueSeoCode == langSeo);
                                if (language != null)
                                {
                                    _localizedEntityService.SaveLocalizedValue(product,
                                                                   x => x.MetaTitle,
                                                                   item.Substring(2),
                                                                   language.Id);
                                }
                            }
                        }
                        //product.SeName = seName;
                        if (columnActivityStates[GetColumnIndex(properties, "SeName")])
                        {
                            localizedItems = seName.Split(new string[] { "###" }, StringSplitOptions.None);
                            string langSeo = null;
                            foreach (var item in localizedItems)
                            {
                                if (item != localizedItems[0])
                                {
                                    langSeo = item.Substring(0, 2);
                                }
                                var language = languages.FirstOrDefault(l => l.UniqueSeoCode == langSeo);
                                if (language != null)
                                {
                                    _localizedEntityService.SaveLocalizedValue(product,
                                                                   x => x.SeName,
                                                                   item.Substring(2),
                                                                   language.Id);
                                }
                            }
                        }
                        if (columnActivityStates[GetColumnIndex(properties, "AllowCustomerReviews")])
                        product.AllowCustomerReviews = allowCustomerReviews;
                        if (columnActivityStates[GetColumnIndex(properties, "Published")])
                        product.Published = published;
                        if (columnActivityStates[GetColumnIndex(properties, "CreatedOnUtc")])
                        product.CreatedOnUtc = createdOnUtc;

                        product.UpdatedOnUtc = DateTime.UtcNow;

                        _productService.UpdateProduct(product);

                        productVariant.Sku = sku;
                        if (columnActivityStates[GetColumnIndex(properties, "ManufacturerPartNumber")])
                        productVariant.ManufacturerPartNumber = manufacturerPartNumber;
                        if (columnActivityStates[GetColumnIndex(properties, "gtin")])
                        productVariant.Gtin = gtin;
                        if (columnActivityStates[GetColumnIndex(properties, "isGiftCard")])
                        productVariant.IsGiftCard = isGiftCard;
                        if (columnActivityStates[GetColumnIndex(properties, "giftCardTypeId")])
                        productVariant.GiftCardTypeId = giftCardTypeId;
                        if (columnActivityStates[GetColumnIndex(properties, "requireOtherProducts")])
                        productVariant.RequireOtherProducts = requireOtherProducts;
                        if (columnActivityStates[GetColumnIndex(properties, "RequiredProductVariantIds")])
                        productVariant.RequiredProductVariantIds = requiredProductVariantIds;
                        if (columnActivityStates[GetColumnIndex(properties, "AutomaticallyAddRequiredProductVariants")])
                        productVariant.AutomaticallyAddRequiredProductVariants = automaticallyAddRequiredProductVariants;
                        if (columnActivityStates[GetColumnIndex(properties, "IsDownload")])
                        productVariant.IsDownload = isDownload;
                        if (columnActivityStates[GetColumnIndex(properties, "DownloadId")])
                        productVariant.DownloadId = downloadId;
                        if (columnActivityStates[GetColumnIndex(properties, "UnlimitedDownloads")])
                        productVariant.UnlimitedDownloads = unlimitedDownloads;
                        if (columnActivityStates[GetColumnIndex(properties, "MaxNumberOfDownloads")])
                        productVariant.MaxNumberOfDownloads = maxNumberOfDownloads;
                        if (columnActivityStates[GetColumnIndex(properties, "DownloadActivationTypeId")])
                        productVariant.DownloadActivationTypeId = downloadActivationTypeId;
                        if (columnActivityStates[GetColumnIndex(properties, "HasSampleDownload")])
                        productVariant.HasSampleDownload = hasSampleDownload;
                        if (columnActivityStates[GetColumnIndex(properties, "SampleDownloadId")])
                        productVariant.SampleDownloadId = sampleDownloadId;
                        if (columnActivityStates[GetColumnIndex(properties, "HasUserAgreement")])
                        productVariant.HasUserAgreement = hasUserAgreement;
                        if (columnActivityStates[GetColumnIndex(properties, "UserAgreementText")])
                        productVariant.UserAgreementText = userAgreementText;
                        if (columnActivityStates[GetColumnIndex(properties, "IsRecurring")])
                        productVariant.IsRecurring = isRecurring;
                        if (columnActivityStates[GetColumnIndex(properties, "RecurringCycleLength")])
                        productVariant.RecurringCycleLength = recurringCycleLength;
                        if (columnActivityStates[GetColumnIndex(properties, "RecurringCyclePeriodId")])
                        productVariant.RecurringCyclePeriodId = recurringCyclePeriodId;
                        if (columnActivityStates[GetColumnIndex(properties, "RecurringTotalCycles")])
                        productVariant.RecurringTotalCycles = recurringTotalCycles;
                        if (columnActivityStates[GetColumnIndex(properties, "IsShipEnabled")])
                        productVariant.IsShipEnabled = isShipEnabled;
                        if (columnActivityStates[GetColumnIndex(properties, "IsFreeShipping")])
                        productVariant.IsFreeShipping = isFreeShipping;
                        if (columnActivityStates[GetColumnIndex(properties, "AdditionalShippingCharge")])
                        productVariant.AdditionalShippingCharge = additionalShippingCharge;
                        if (columnActivityStates[GetColumnIndex(properties, "IsTaxExempt")])
                        productVariant.IsTaxExempt = isTaxExempt;
                        if (columnActivityStates[GetColumnIndex(properties, "TaxCategoryId")])
                        productVariant.TaxCategoryId = taxCategoryId;
                        if (columnActivityStates[GetColumnIndex(properties, "ManageInventoryMethodId")])
                        productVariant.ManageInventoryMethodId = manageInventoryMethodId;
                        if (columnActivityStates[GetColumnIndex(properties, "StockQuantity")])
                        productVariant.StockQuantity = stockQuantity;
                        if (columnActivityStates[GetColumnIndex(properties, "DisplayStockAvailability")])
                        productVariant.DisplayStockAvailability = displayStockAvailability;
                        if (columnActivityStates[GetColumnIndex(properties, "DisplayStockQuantity")])
                        productVariant.DisplayStockQuantity = displayStockQuantity;
                        if (columnActivityStates[GetColumnIndex(properties, "MinStockQuantity")])
                        productVariant.MinStockQuantity = minStockQuantity;
                        if (columnActivityStates[GetColumnIndex(properties, "LowStockActivityId")])
                        productVariant.LowStockActivityId = lowStockActivityId;
                        if (columnActivityStates[GetColumnIndex(properties, "NotifyAdminForQuantityBelow")])
                        productVariant.NotifyAdminForQuantityBelow = notifyAdminForQuantityBelow;
                        if (columnActivityStates[GetColumnIndex(properties, "BackorderModeId")])
                        productVariant.BackorderModeId = backorderModeId;
                        if (columnActivityStates[GetColumnIndex(properties, "AllowBackInStockSubscriptions")])
                        productVariant.AllowBackInStockSubscriptions = allowBackInStockSubscriptions;
                        if (columnActivityStates[GetColumnIndex(properties, "OrderMinimumQuantity")])
                        productVariant.OrderMinimumQuantity = orderMinimumQuantity;
                        if (columnActivityStates[GetColumnIndex(properties, "OrderMaximumQuantity")])
                        productVariant.OrderMaximumQuantity = orderMaximumQuantity;
                        if (columnActivityStates[GetColumnIndex(properties, "DisableBuyButton")])
                        productVariant.DisableBuyButton = disableBuyButton;
                        if (columnActivityStates[GetColumnIndex(properties, "DisableWishlistButton")])
                        productVariant.DisableWishlistButton = disableWishlistButton;
                        if (columnActivityStates[GetColumnIndex(properties, "CallForPrice")])
                        productVariant.CallForPrice = callForPrice;
                        if (columnActivityStates[GetColumnIndex(properties, "Price")])
                        productVariant.Price = price;
                        if (columnActivityStates[GetColumnIndex(properties, "OldPrice")])
                        productVariant.OldPrice = oldPrice;
                        if (columnActivityStates[GetColumnIndex(properties, "ProductCost")])
                        productVariant.ProductCost = productCost;
                        if (columnActivityStates[GetColumnIndex(properties, "SpecialPrice")])
                        productVariant.SpecialPrice = specialPrice;
                        if (columnActivityStates[GetColumnIndex(properties, "SpecialPriceStartDateTimeUtc")])
                        productVariant.SpecialPriceStartDateTimeUtc = specialPriceStartDateTimeUtc;
                        if (columnActivityStates[GetColumnIndex(properties, "SpecialPriceEndDateTimeUtc")])
                        productVariant.SpecialPriceEndDateTimeUtc = specialPriceEndDateTimeUtc;
                        if (columnActivityStates[GetColumnIndex(properties, "CustomerEntersPrice")])
                        productVariant.CustomerEntersPrice = customerEntersPrice;
                        if (columnActivityStates[GetColumnIndex(properties, "MinimumCustomerEnteredPrice")])
                        productVariant.MinimumCustomerEnteredPrice = minimumCustomerEnteredPrice;
                        if (columnActivityStates[GetColumnIndex(properties, "MaximumCustomerEnteredPrice")])
                        productVariant.MaximumCustomerEnteredPrice = maximumCustomerEnteredPrice;
                        if (columnActivityStates[GetColumnIndex(properties, "Weight")])
                        productVariant.Weight = weight;
                        if (columnActivityStates[GetColumnIndex(properties, "Length")])
                        productVariant.Length = length;
                        if (columnActivityStates[GetColumnIndex(properties, "Width")])
                        productVariant.Width = width;
                        if (columnActivityStates[GetColumnIndex(properties, "Height")])
                        productVariant.Height = height;
                        if (columnActivityStates[GetColumnIndex(properties, "Published")])
                        productVariant.Published = published;
                        if (columnActivityStates[GetColumnIndex(properties, "CreatedOnUtc")])
                        productVariant.CreatedOnUtc = createdOnUtc;
                        productVariant.UpdatedOnUtc = DateTime.UtcNow;

                        if (columnActivityStates[GetColumnIndex(properties, "CurrencyId")])
                        productVariant.CurrencyId=currencyId;
                        if (columnActivityStates[GetColumnIndex(properties, "CurrencyPrice")])
                        productVariant.CurrencyPrice=currencyPrice;
                        if (columnActivityStates[GetColumnIndex(properties, "CurrencyOldPrice")])
                        productVariant.CurrencyOldPrice=currencyOldPrice;
                        if (columnActivityStates[GetColumnIndex(properties, "CurrencyProductCost")])
                        productVariant.CurrencyProductCost=currencyProductCost;
                        if (columnActivityStates[GetColumnIndex(properties, "HideDiscount")])
                        productVariant.HideDiscount=hideDiscount;
                        if (columnActivityStates[GetColumnIndex(properties, "DisplayPriceIfCallforPrice")])
                        productVariant.DisplayPriceIfCallforPrice=displayPriceIfCallforPrice;

                        _productService.UpdateProductVariant(productVariant);
                    }
                        /*
                    else
                    {
                        var product = new Product()
                        {
                            Name = name,
                            ShortDescription = shortDescription,
                            FullDescription = fullDescription,
                            ShowOnHomePage = showOnHomePage,
                            MetaKeywords = metaKeywords,
                            MetaDescription = metaDescription,
                            MetaTitle = metaTitle,
                            SeName = seName,
                            AllowCustomerReviews = allowCustomerReviews,
                            Published = published,
                            CreatedOnUtc = createdOnUtc,
                            UpdatedOnUtc = DateTime.UtcNow
                        };
                        _productService.InsertProduct(product);

                        productVariant = new ProductVariant()
                        {
                            ProductId = product.Id,
                            Sku = sku,
                            ManufacturerPartNumber = manufacturerPartNumber,
                            Gtin = gtin,
                            IsGiftCard = isGiftCard,
                            GiftCardTypeId = giftCardTypeId,
                            RequireOtherProducts = requireOtherProducts,
                            RequiredProductVariantIds = requiredProductVariantIds,
                            AutomaticallyAddRequiredProductVariants = automaticallyAddRequiredProductVariants,
                            IsDownload = isDownload,
                            DownloadId = downloadId,
                            UnlimitedDownloads = unlimitedDownloads,
                            MaxNumberOfDownloads = maxNumberOfDownloads,
                            DownloadActivationTypeId = downloadActivationTypeId,
                            HasSampleDownload = hasSampleDownload,
                            SampleDownloadId = sampleDownloadId,
                            HasUserAgreement = hasUserAgreement,
                            UserAgreementText = userAgreementText,
                            IsRecurring = isRecurring,
                            RecurringCycleLength = recurringCycleLength,
                            RecurringCyclePeriodId = recurringCyclePeriodId,
                            RecurringTotalCycles = recurringTotalCycles,
                            IsShipEnabled = isShipEnabled,
                            IsFreeShipping = isFreeShipping,
                            AdditionalShippingCharge = additionalShippingCharge,
                            IsTaxExempt = isTaxExempt,
                            TaxCategoryId = taxCategoryId,
                            ManageInventoryMethodId = manageInventoryMethodId,
                            StockQuantity = stockQuantity,
                            DisplayStockAvailability = displayStockAvailability,
                            DisplayStockQuantity = displayStockQuantity,
                            MinStockQuantity = minStockQuantity,
                            LowStockActivityId = lowStockActivityId,
                            NotifyAdminForQuantityBelow = notifyAdminForQuantityBelow,
                            BackorderModeId = backorderModeId,
                            AllowBackInStockSubscriptions = allowBackInStockSubscriptions,
                            OrderMinimumQuantity = orderMinimumQuantity,
                            OrderMaximumQuantity = orderMaximumQuantity,
                            DisableBuyButton = disableBuyButton,
                            CallForPrice = callForPrice,
                            Price = price,
                            OldPrice = oldPrice,
                            ProductCost = productCost,
                            SpecialPrice = specialPrice,
                            SpecialPriceStartDateTimeUtc = specialPriceStartDateTimeUtc,
                            SpecialPriceEndDateTimeUtc = specialPriceEndDateTimeUtc,
                            CustomerEntersPrice = customerEntersPrice,
                            MinimumCustomerEnteredPrice = minimumCustomerEnteredPrice,
                            MaximumCustomerEnteredPrice = maximumCustomerEnteredPrice,
                            Weight = weight,
                            Length = length,
                            Width = width,
                            Height = height,
                            Published = published,
                            CreatedOnUtc = createdOnUtc,
                            UpdatedOnUtc = DateTime.UtcNow,
                            CurrencyPrice = productVariant.CurrencyPrice,
                            CurrencyOldPrice = productVariant.CurrencyOldPrice,
                            CurrencyProductCost = productVariant.CurrencyProductCost,
                  
                        };

                        _productService.InsertProductVariant(productVariant);
                    }*/

                    //category mappings
                    if (columnActivityStates[GetColumnIndex(properties, "CategoryIds")])
                    {
                        if (!String.IsNullOrEmpty(categoryIds))
                        {
                            var  productCategories  = _categoryService.GetProductCategoriesByProductId(productVariant.Product.Id);
                            List<int> list = new List<int>();
                            
                            foreach (var item in productCategories)
                            {
                                list.Add(item.CategoryId);
                            }

                            foreach (var id in categoryIds.Split(new char[] {';'}, StringSplitOptions.RemoveEmptyEntries).Select(x => Convert.ToInt32(x.Trim())))
                            {
                                if (!list.Contains(id)) //productVariant.Product.ProductCategories.Where(a=>a.CategoryId==id).FirstOrDefault()==null
                                {
                                    //ensure that category exists
                                    var category = _categoryService.GetCategoryById(id);
                                    if (category != null)
                                    {
                                        var productCategory = new ProductCategory()
                                            {
                                                ProductId = productVariant.Product.Id,
                                                CategoryId = category.Id,
                                                IsFeaturedProduct = false,
                                                DisplayOrder = 1
                                            };
                                        _categoryService.InsertProductCategory(productCategory);
                                    }
                                }
                            }
                        }
                    }

                    //manufacturer mappings
                    if (columnActivityStates[GetColumnIndex(properties, "ManufacturerIds")])
                    {
                        var deletedmanufacturers = _manufacturerService.GetProductManufacturersByProductId(productVariant.Product.Id);
                        List<int> list = new List<int>();

                        foreach (var item in deletedmanufacturers)
                        {
                            list.Add(item.ManufacturerId);
                        }

                        if (!String.IsNullOrEmpty(manufacturerIds))
                        {
                            foreach ( var id in manufacturerIds.Split(new char[] {';'}, StringSplitOptions.RemoveEmptyEntries).Select(x => Convert.ToInt32(x.Trim())))
                            {
                                if (!list.Contains(id)) //productVariant.Product.ProductManufacturers.Where(x => x.ManufacturerId == id).FirstOrDefault() == null
                                {
                                    //ensure that manufacturer exists
                                    var manufacturer = _manufacturerService.GetManufacturerById(id);
                                    if (manufacturer != null)
                                    {
                                        var productManufacturer = new ProductManufacturer()
                                            {
                                                ProductId = productVariant.Product.Id,
                                                ManufacturerId = manufacturer.Id,
                                                IsFeaturedProduct = false,
                                                DisplayOrder = 1
                                            };
                                        _manufacturerService.InsertProductManufacturer(productManufacturer);
                                    }
                                }
                            }
                        }
                    }

                    //pictures
                    //foreach (var picture in new string[] { picture1, picture2, picture3 })
                    //{
                    //    if (String.IsNullOrEmpty(picture))
                    //        continue;

                    //    productVariant.Product.ProductPictures.Add(new ProductPicture()
                    //    {
                    //        Picture = _pictureService.InsertPicture(File.ReadAllBytes(picture), "image/jpeg", _pictureService.GetPictureSeName(name), true),
                    //        DisplayOrder = 1,
                    //    });
                    //    _productService.UpdateProduct(productVariant.Product);
                    //}

                    //next product
                    iRow++;
                }
            }
        }


        /// <summary>
        /// Import products from XML file
        /// </summary>
        /// <param name="language">Language</param>
        /// <param name="xml">XML</param>
        public virtual void UpdateProductVariantsFromPirlantXml(string xml)
        {
            if (xml == null)
                throw new ArgumentNullException("productvariant");

            if (String.IsNullOrEmpty(xml))
                return;
            XDocument xmlDoc = XDocument.Load(xml);

            var productVariants = from productVariant in
                                      xmlDoc.Descendants("SorguSonucu")
                           select new
                           {
                               Sku = productVariant.Element("Referans").Value,
                               StockAmount = productVariant.Element("Miktar").Value,
                               CurrencyPrice = productVariant.Element("EtiketFiyat").Value,
                               Currency = productVariant.Element("EtiketDoviz").Value
                           };

            foreach (var node in productVariants) 
            {
                string sku = node.Sku;
                string currentPrice;
                var productVariant = _productService.GetProductVariantBySku(sku);
                if (productVariant != null)
                {
                    var cur = _currencyService.GetCurrencyByCode(node.Currency);
                    if(cur.Id != 0)
                    productVariant.CurrencyId = cur.Id;
                    productVariant.Sku = sku;
                    productVariant.StockQuantity = Convert.ToInt32(node.StockAmount);
                  
                    productVariant.CurrencyPrice = Decimal.Parse(node.CurrencyPrice);
                    productVariant.Price = _currencyService.ConvertToPrimaryStoreCurrency((decimal)productVariant.CurrencyPrice, cur);
                    
                    productVariant.UpdatedOnUtc = DateTime.UtcNow;

                    _productService.UpdateProductVariant(productVariant);
                }
            }

            //var xmlDoc = new XmlDocument();
            //xmlDoc.Load(xml);
            //var nodes = xmlDoc.SelectNodes(@"//ArrayOfSorguSonucu/SorguSonucu");
            //foreach (XmlNode node in nodes)
            //{
            //    string sku = node.SelectSingleNode("//Referans").InnerText.Trim();
            //    var productVariant = _productService.GetProductVariantBySku(sku);
               
                   
               
            //    if (productVariant != null)
            //    {
            //        int stockQuantity = Convert.ToInt32(node.SelectSingleNode("//Miktar").InnerText.Trim());
            //        decimal price = Convert.ToDecimal(node.SelectSingleNode("//EtiketFiyat").InnerText.Trim());
            //        productVariant.Sku = sku;
            //        productVariant.StockQuantity = stockQuantity;
            //        productVariant.Price = price;
            //        productVariant.UpdatedOnUtc = DateTime.UtcNow;
                    
            //        _productService.UpdateProductVariant(productVariant);
            //    }

            //}
          
        }
        /// <summary>
        /// Import language resources from XML file
        /// </summary>
        /// <param name="language">Language</param>
        /// <param name="xml">XML</param>
        public virtual void ImportLanguageFromXml(Language language, string xml)
        {
            if (language == null)
                throw new ArgumentNullException("language");

            if (String.IsNullOrEmpty(xml))
                return;

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            var nodes = xmlDoc.SelectNodes(@"//Language/LocaleResource");
            foreach (XmlNode node in nodes)
            {
                string name = node.Attributes["Name"].InnerText.Trim();
                string value = "";
                var valueNode = node.SelectSingleNode("Value");
                if (valueNode != null)
                    value = valueNode.InnerText;
                
                if (String.IsNullOrEmpty(name))
                    continue;
                
                //do not use localizationservice because it'll clear cache and after adding each resoruce
                //let's bulk insert
                var resource = language.LocaleStringResources.Where(x => x.ResourceName.Equals(name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                if (resource != null)
                    resource.ResourceValue = value;
                else
                {
                    language.LocaleStringResources.Add(
                        new LocaleStringResource()
                        {
                            ResourceName = name,
                            ResourceValue = value
                        });
                }
            }
            _languageService.UpdateLanguage(language);

            //clear cache
            _localizationService.ClearCache();
        }

    }
}
