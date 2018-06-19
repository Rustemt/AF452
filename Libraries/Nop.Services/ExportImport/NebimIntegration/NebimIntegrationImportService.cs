using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Media;
using OfficeOpenXml;
using System.Xml.Linq;
using System.Text;
using Nop.Services.Directory;
using Nop.Core.Domain.Directory;
using Nop.Services.ExportImport;
using Nop.Core.Plugins;
using Nop.Services.Orders;
using System.Data;
using System.Collections.Generic;
using Nop.Services.Tax;
using Nop.Core.Infrastructure;
using Nop.Services.Logging;
using Nop.Services.Customers;
using Nop.Core.Domain.Customers;
using Nop.Services.Helpers;
using Nop.Core.Domain.Discounts;
using System.Collections;
using Nop.Services.Security;

namespace Nop.Services.ExportImport
{
    /// <summary>
    /// Import manager
    /// </summary>
    public partial class NebimIntegrationImportService : INebimIntegrationImportService
    {
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IPictureService _pictureService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly IPluginFinder _pluginFinder;
        private readonly ILogger _logger;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly NebimIntegrationSettings _NebimIntegrationSettings;
        private readonly IEncryptionService _encryptionService;
        private readonly ITaxService _taxService;
        private readonly IWorkContext _workContext;


        public NebimIntegrationImportService(IProductService productService, IOrderService orderService,
            ILanguageService languageService,
            ILocalizationService localizationService, ICategoryService categoryService,
            IManufacturerService manufacturerService, IPictureService pictureService,
            ILocalizedEntityService localizedEntityService,
             ICurrencyService currencyService, CurrencySettings currencySettings,
              IPluginFinder pluginFinder, ILogger logger, IDateTimeHelper dateTimeHelper,
            IProductAttributeParser productAttributeParser, NebimIntegrationSettings nebimIntegrationSettings,
            ITaxService taxService, IWorkContext workContext, IEncryptionService encryptionService)
        {
            this._productService = productService;
            this._orderService = orderService;
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._pictureService = pictureService;
            this._localizedEntityService = localizedEntityService;
            this._currencyService = currencyService;
            this._currencySettings = currencySettings;
            this._pluginFinder = pluginFinder;
            this._logger = logger;
            this._dateTimeHelper = dateTimeHelper;
            this._productAttributeParser = productAttributeParser;
            this._NebimIntegrationSettings = nebimIntegrationSettings;
            this._taxService = taxService;
            this._workContext = workContext;
            this._encryptionService = encryptionService;
        }


        //system name = Misc.Nebim
        private INebimIntegrationProvider LoadNebimIntegrationServiceBySystemName(string systemName)
        {
            var descriptor = _pluginFinder.GetPluginDescriptorBySystemName<INebimIntegrationProvider>(systemName);
            if (descriptor != null)
                return descriptor.Instance<INebimIntegrationProvider>();

            return null;
        }

        public void GetAllProducts(string LangCode, int LastNDay)
        {
            var nebimIntegrationProvider = LoadNebimIntegrationServiceBySystemName("Misc.Nebim");
            var productsTable = nebimIntegrationProvider.GetAllProducts();
        }

        public void ImportAllProducts()
        {
            var nebimIntegrationProvider = LoadNebimIntegrationServiceBySystemName("Misc.Nebim");
            DataTable productsTable = nebimIntegrationProvider.GetAllProducts();
            var groupedRows = from row in productsTable.AsEnumerable()
                              group row by row.Field<string>("ProductCode") into grp
                              select grp;
            DataRow dr = null;
            foreach (var grp in groupedRows)//for each nebim product
            {
                dr = grp.FirstOrDefault();

                string ProductDescription = dr["ProductDescription"].ToString();
                string ProductCode = dr["ProductCode"].ToString();
                string ItemDimTypeCode = dr["ItemDimTypeCode"].ToString();
                string ItemTaxGrCode = dr["ItemTaxGrCode"].ToString();
                string ItemTaxGrDescription = dr["ItemTaxGrDescription"].ToString();

                //category
                int ProductHierarchyLevelCode01 = 0; int.TryParse(dr["ProductHierarchyLevelCode01"].ToString(), out ProductHierarchyLevelCode01);
                int ProductHierarchyLevelCode02 = 0; int.TryParse(dr["ProductHierarchyLevelCode02"].ToString(), out ProductHierarchyLevelCode02);
                int ProductHierarchyLevelCode03 = 0; int.TryParse(dr["ProductHierarchyLevelCode03"].ToString(), out ProductHierarchyLevelCode03);
                int ProductHierarchyLevelCode04 = 0; int.TryParse(dr["ProductHierarchyLevelCode04"].ToString(), out ProductHierarchyLevelCode04);

                //attributes
                string ProductAtt01 = dr["ProductAtt01"].ToString();
                string ProductAtt01Desc = dr["ProductAtt01Desc"].ToString();
                string ProductAtt02 = dr["ProductAtt02"].ToString();
                string ProductAtt02Desc = dr["ProductAtt02Desc"].ToString();
                string ProductAtt03 = dr["ProductAtt03"].ToString();
                string ProductAtt03Desc = dr["ProductAtt03Desc"].ToString();
                string ColorCode = dr["ColorCode"].ToString();

                string ItemDim1Code = dr["ItemDim1Code"].ToString();

                decimal Price1 = 0; decimal.TryParse(dr["Price1"].ToString(), out Price1);

                decimal Price2 = 0; decimal.TryParse(dr["Price2"].ToString(), out Price2);

                string Warehouse1InventoryQty = dr["Warehouse1InventoryQty"].ToString();

                string Warehouse2InventoryQty = dr["Warehouse2InventoryQty"].ToString();



                //To nop notation
                var sku = ProductCode;

                string name = ProductDescription;
                decimal price = Price1;
                decimal oldPrice = Price2;


                var productVariant = _productService.GetProductVariantBySku(sku);
                // if already existing product than only update prices, (?stock quantites?)
                if (productVariant != null)
                {
                    productVariant.Price = price;
                    productVariant.OldPrice = oldPrice;
                }
                // if new product  than insert product in unpublished state.
                else
                {
                    var product = new Product()
                    {
                        Name = name,
                        ShortDescription = null,
                        FullDescription = null,
                        Published = false,
                        CreatedOnUtc = DateTime.UtcNow
                    };
                    _productService.InsertProduct(product);

                    productVariant = new ProductVariant()
                    {
                        //    ProductId = product.Id,
                        //    Sku = sku,
                        //    ManufacturerPartNumber = manufacturerPartNumber,
                        //    Gtin = gtin,
                        //    IsGiftCard = isGiftCard,
                        //    GiftCardTypeId = giftCardTypeId,
                        //    RequireOtherProducts = requireOtherProducts,
                        //    RequiredProductVariantIds = requiredProductVariantIds,
                        //    AutomaticallyAddRequiredProductVariants = automaticallyAddRequiredProductVariants,
                        //    IsDownload = isDownload,
                        //    DownloadId = downloadId,
                        //    UnlimitedDownloads = unlimitedDownloads,
                        //    MaxNumberOfDownloads = maxNumberOfDownloads,
                        //    DownloadActivationTypeId = downloadActivationTypeId,
                        //    HasSampleDownload = hasSampleDownload,
                        //    SampleDownloadId = sampleDownloadId,
                        //    HasUserAgreement = hasUserAgreement,
                        //    UserAgreementText = userAgreementText,
                        //    IsRecurring = isRecurring,
                        //    RecurringCycleLength = recurringCycleLength,
                        //    RecurringCyclePeriodId = recurringCyclePeriodId,
                        //    RecurringTotalCycles = recurringTotalCycles,
                        //    IsShipEnabled = isShipEnabled,
                        //    IsFreeShipping = isFreeShipping,
                        //    AdditionalShippingCharge = additionalShippingCharge,
                        //    IsTaxExempt = isTaxExempt,
                        //    TaxCategoryId = taxCategoryId,
                        //    ManageInventoryMethodId = manageInventoryMethodId,
                        //    StockQuantity = stockQuantity,
                        //    DisplayStockAvailability = displayStockAvailability,
                        //    DisplayStockQuantity = displayStockQuantity,
                        //    MinStockQuantity = minStockQuantity,
                        //    LowStockActivityId = lowStockActivityId,
                        //    NotifyAdminForQuantityBelow = notifyAdminForQuantityBelow,
                        //    BackorderModeId = backorderModeId,
                        //    AllowBackInStockSubscriptions = allowBackInStockSubscriptions,
                        //    OrderMinimumQuantity = orderMinimumQuantity,
                        //    OrderMaximumQuantity = orderMaximumQuantity,
                        //    AllowedQuantities = allowedQuantities,
                        //    DisableBuyButton = disableBuyButton,
                        //    CallForPrice = callForPrice,
                        //    Price = price,
                        //    OldPrice = oldPrice,
                        //    ProductCost = productCost,
                        //    SpecialPrice = specialPrice,
                        //    SpecialPriceStartDateTimeUtc = specialPriceStartDateTimeUtc,
                        //    SpecialPriceEndDateTimeUtc = specialPriceEndDateTimeUtc,
                        //    CustomerEntersPrice = customerEntersPrice,
                        //    MinimumCustomerEnteredPrice = minimumCustomerEnteredPrice,
                        //    MaximumCustomerEnteredPrice = maximumCustomerEnteredPrice,
                        //    Weight = weight,
                        //    Length = length,
                        //    Width = width,
                        //    Height = height,
                        //    Published = published,
                        //    CreatedOnUtc = createdOnUtc,
                        //    UpdatedOnUtc = DateTime.UtcNow
                    };

                    _productService.InsertProductVariant(productVariant);


                }


                //insert/update product

                //insert/update productVariants

            }



        }
    }
}