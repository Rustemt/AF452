using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Xml;
using Nop.Core;
using Nop.Core.Plugins;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Core.Infrastructure;
using Nop.Services.ExportImport;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Media;
using Nop.Core.Domain.Directory;
using Nop.Services.Orders;
using System.Collections;
using Nop.Core.Domain.Orders;
using System.Linq;
using Nop.Services.Logging;
using Nop.Services.Common;
using System.Web.Routing;
using System.Data;
using Nop.Plugin.Misc.NebimIntegration.API;

namespace Nop.Plugin.Misc.NebimIntegration
{
    public class NebimIntegrationProvider : BasePlugin, INebimIntegrationProvider
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
        private readonly IOrderService _orderService;
        private readonly ILogger _logger;
        private NebimIntegrationSettings _NebimIntegrationSettings;


        public NebimIntegrationProvider(IProductService productService, ILanguageService languageService,
            ILocalizationService localizationService, ICategoryService categoryService,
            IManufacturerService manufacturerService, IPictureService pictureService,
            ILocalizedEntityService localizedEntityService,
            ICurrencyService currencyService, CurrencySettings currencySettings,
            IOrderService orderService, ILogger logger, NebimIntegrationSettings NebimIntegrationSettings)
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
            this._orderService = orderService;
            this._logger = logger;
            this._NebimIntegrationSettings = NebimIntegrationSettings;
        }

        #region utilities



        #endregion utilities

        public void ResetConnection()
        {
            ConnectionManager.ResetConnection();
        }

        public void Connect(NebimIntegrationSettings settings)
        {
            ConnectionManager.ConnectDB(settings);
        }

        public void LoadSettings(NebimIntegrationSettings settings)
        {
            this._NebimIntegrationSettings = settings;
        }
        #region Import


        #endregion Import

        #region export
        public void AddUpdateProductToNebim(
                    string Barcode,
                    string ProductCode,/*sku*/
                    string ProductNameTR,
                    string ProductNameEN,
                    int ItemDimTypeCode,
                    IList<Tuple<string, string, string, string, string, string, string>> combinations,//renk kodu,renk adı, renk adı, boyut kodu,boyut adı,boyut adı
                    string UnitOfMeasureCode1,
                    string CountryCode,
                    string PurcVatRate,//tax rate
                    string SellVatRate,//tax rate
                    string ItemAccountGrCode,//tax rate
                    int[] ProductHierarchyLevelCodes,
                    IList<Tuple<byte, string, string, string>> attributes,//attr type code (1-20),attr code, attr name,attr name
                    decimal price1,
                    decimal price2,
             decimal price3,
                    string currencyCode)
        {
            _logger.Warning("ProductManager pm = new ProductManager(_NebimIntegrationSettings);");
            ProductManager pm = new ProductManager(_NebimIntegrationSettings);
            pm.CreateUpdateProduct(
                Barcode,
                ProductCode,
                     ProductNameTR,
                     ProductNameEN,
                     ItemDimTypeCode,
                     combinations,//renk kodu,renk adı, renk adı, boyut kodu,boyut adı,boyut adı
                     UnitOfMeasureCode1,
                     CountryCode,
                     PurcVatRate,//tax rate
                     SellVatRate,//tax rate
                     ItemAccountGrCode,//tax rate
                     ProductHierarchyLevelCodes,
                     attributes,//attr type code (1-20),attr code, attr name,attr name
                     price1,
                     price2,
                     price3,
                     currencyCode);
        }

        public void AddUpdateProductToNebim(
                   string Barcode,
                   string ProductCode,/*sku*/
                   string ProductNameTR,
                   string ProductNameEN,
                   int ItemDimTypeCode,
                   IList<Tuple<string, string, string, string, string, string, string>> combinations,//renk kodu,renk adı, renk adı, boyut kodu,boyut adı,boyut adı
                   string UnitOfMeasureCode1,
                   string CountryCode,
                   string PurcVatRate,//tax rate
                   string SellVatRate,//tax rate
                   string ItemAccountGrCode,//tax rate
                   Dictionary<int, string> ProductHierarchyLevelNames,// level*ProductHierarchyLevelName
                   IList<Tuple<byte, string, string, string>> attributes,//attr type code (1-20),attr code, attr name,attr name
                   decimal price1,
                   decimal price2,
                   decimal price3,
                   string currencyCode)
        {
            _logger.Warning("ProductManager pm = new ProductManager(_NebimIntegrationSettings);");
            ProductManager pm = new ProductManager(_NebimIntegrationSettings);
            Dictionary<int, IList<int>> ProductHierarchyLevelCodes = new Dictionary<int, IList<int>>();//level*ProductHierarchyLevelCodes(more than one for the same named levels)
            foreach (var kvp in ProductHierarchyLevelNames)
            {
                ProductHierarchyLevelCodes.Add(kvp.Key, pm.FindProductHierarchyLevelCode(kvp.Value, kvp.Key));
            }

            pm.CreateUpdateProduct(Barcode, ProductCode,/*sku*/
                     ProductNameTR,
                     ProductNameEN,
                     ItemDimTypeCode,
                     combinations,//renk kodu,renk adı, renk adı, boyut kodu,boyut adı,boyut adı
                     UnitOfMeasureCode1,
                     CountryCode,
                     PurcVatRate,//tax rate
                     SellVatRate,//tax rate
                     ItemAccountGrCode,//tax rate
                     ProductHierarchyLevelCodes,
                     attributes,//attr type code (1-20),attr code, attr name,attr name
                     price1,
                     price2,
                     price3,
                     currencyCode);
        }


        public string AddUpdateCustomerToNebim(string description, string languageCode, string firstName, string lastName,
            List<string> emails, List<string> phones, string identityNum, string gender, string customerSegment, DateTime? registerDate = null)
        {
            _logger.Warning("CustomerManager cm = new CustomerManager(_NebimIntegrationSettings);");
            CustomerManager cm = new CustomerManager(_NebimIntegrationSettings);
            byte genderCode;
            if (gender == "M")
                genderCode = 1;
            else if (gender == "F")
                genderCode = 2;
            else
                genderCode = 3;
            return cm.CreateUpdateCustomer(description, languageCode, firstName, lastName, emails, phones, identityNum, genderCode, customerSegment, registerDate);
        }

        public Guid AddUpdateCustomerAddressToNebimByCustomerDescription(string customerDescription, string type,
            string firstName, string lastName,
            string addressLine, string district, string city,
            string zipCode, string countryCode,
            string taxNumber, string taxOffice)
        {
            CustomerManager cm = new CustomerManager(_NebimIntegrationSettings);
            return cm.CreateCustomerAddressByCustomerDescription(customerDescription, type,
             firstName, lastName,
             addressLine, district, city,
             zipCode, countryCode,
             taxNumber, taxOffice);
        }

        public Guid AddUpdateCustomerAddressToNebimByCustomerCode(string customerCode, string type,
          string firstName, string lastName,
          string addressLine, string district, string city,
          string zipCode, string countryCode,
          string taxNumber, string taxOffice)
        {
            CustomerManager cm = new CustomerManager(_NebimIntegrationSettings);
            return cm.CreateCustomerAddressByCustomerCode(customerCode, type,
             firstName, lastName,
             addressLine, district, city,
             zipCode, countryCode,
             taxNumber, taxOffice);
        }


        public string AddUpdateOrderToNebim(string description, DateTime? orderDate, string customerCode,
            Guid? shippingPostalAddressID, Guid? BillingPostalAddressID,
            IList<Tuple<string, string, string, string, int, decimal?, decimal?, Tuple<string>>> items,
            decimal? discountAmount, double? discountRate, string currencyCode, decimal shippingCost, decimal giftBoxCost, string discountNames)
        {
            OrderManager om = new OrderManager(_NebimIntegrationSettings, _logger);
            return om.CreateOrder(description, orderDate, customerCode, shippingPostalAddressID, BillingPostalAddressID, items,
             discountAmount, discountRate, currencyCode, shippingCost, giftBoxCost, discountNames);
        }


        public void AddUpdateOrderPaymentToNebim(string orderNumber, int paymentType, string creditCardTypeCode, byte installment, string maskedCCNo, string provisionNo)
        {
            _logger.Warning("OrderPaymentManager opm = new OrderPaymentManager(_NebimIntegrationSettings);");
            OrderPaymentManager opm = new OrderPaymentManager(_NebimIntegrationSettings);
            opm.SavePayment(orderNumber, (myPaymentTypes)paymentType, creditCardTypeCode, installment, maskedCCNo, provisionNo);
        }

        public void CreateShipment(string orderDescription)
        {
            OrderManager om = new OrderManager(_NebimIntegrationSettings, _logger);
            om.CreateShipment(orderDescription);
            // testestset
        }

        #endregion export


        public DataTable GetAllProducts()
        {
            //ConnectionManager.ConnectDB(this._NebimIntegrationSettings);
            return new ProductManager(_NebimIntegrationSettings).GetAllProducts(_NebimIntegrationSettings.ProdImpLanguageCode, _NebimIntegrationSettings.ProdImpPriceGroup1Code, _NebimIntegrationSettings.ProdImpPriceGroup2Code, _NebimIntegrationSettings.ProdImpWarehouse1Code, _NebimIntegrationSettings.ProdImpWarehouse2Code, _NebimIntegrationSettings.ProdImpBarcodeTypeCode1, _NebimIntegrationSettings.ProdImpLastDay);

        }

        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "NebimIntegration";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Misc.NebimIntegration.Controllers" }, { "area", null } };
        }

        //public void SyncProducts()
        //{
        //    //get all product from V3
        //    var productsTable = GetAllProducts();

        //    //foreach (var row in productsTable.Rows)
        //    //{
        //    //    SyncProducts(
        //    //}

        //}

    }
}
