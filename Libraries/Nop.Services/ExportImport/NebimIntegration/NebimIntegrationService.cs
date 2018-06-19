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
    public partial class NebimIntegrationService : INebimIntegrationService
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
        private readonly NebimIntegrationExtSettings _NebimIntegrationExtSettings;
        private readonly IEncryptionService _encryptionService;
        private readonly ITaxService _taxService;
        private readonly IWorkContext _workContext;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;


        public NebimIntegrationService(IProductService productService, IOrderService orderService,
            ILanguageService languageService,
            ILocalizationService localizationService, ICategoryService categoryService,
            IManufacturerService manufacturerService, IPictureService pictureService,
            ILocalizedEntityService localizedEntityService,
             ICurrencyService currencyService, CurrencySettings currencySettings,
              IPluginFinder pluginFinder, ILogger logger, IDateTimeHelper dateTimeHelper,
            IProductAttributeParser productAttributeParser, NebimIntegrationSettings nebimIntegrationSettings,
            ITaxService taxService, IWorkContext workContext, IEncryptionService encryptionService,
            ICheckoutAttributeParser checkoutAttributeParser)
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
            this._checkoutAttributeParser = checkoutAttributeParser;
        }


        //system name = Misc.Nebim
        private INebimIntegrationProvider LoadNebimIntegrationServiceBySystemName(string systemName)
        {
            var descriptor = _pluginFinder.GetPluginDescriptorBySystemName<INebimIntegrationProvider>(systemName);
            if (descriptor != null)
                return descriptor.Instance<INebimIntegrationProvider>();

            return null;
        }

        public string AddUpdateOrderCustomerToNebim(Core.Domain.Orders.Order order)
        {
            var customer = order.Customer;
            var address = order.BillingAddress;
            var nebimIntegrationProvider = LoadNebimIntegrationServiceBySystemName("Misc.Nebim");
            string description = customer.Id.ToString();
            string languageCode = customer.Language.UniqueSeoCode.ToUpper();
            string firstName = customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName);
            string lastName = customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName);
            if (string.IsNullOrWhiteSpace(firstName))
            {
                firstName = address.FirstName;
            }
            if (string.IsNullOrWhiteSpace(lastName))
            {
                lastName = address.LastName;
            }

            string email = customer.Email;//email will be null for unregistered customers
            string billingEmail = order.BillingAddress.Email;
            string shippingEmail = order.ShippingAddress.Email;
            List<string> emails = new List<string>();
            if (!string.IsNullOrWhiteSpace(email))
            {
                emails.Add(email.Trim().ToLower());
            }
            if (!string.IsNullOrWhiteSpace(billingEmail) && !emails.Contains(billingEmail.Trim().ToLower()))
            {
                emails.Add(billingEmail.Trim().ToLower());
            }
            if (!string.IsNullOrWhiteSpace(shippingEmail) && !emails.Contains(shippingEmail.Trim().ToLower()))
            {
                emails.Add(shippingEmail.Trim().ToLower());
            }

            string billingPhone = address.PhoneNumber;
            string shippingPhone = order.ShippingAddress.PhoneNumber;
            List<string> phones = new List<string>();
            if (!string.IsNullOrWhiteSpace(billingPhone))
            {
                phones.Add(billingPhone.Trim());
            }
            if (!string.IsNullOrWhiteSpace(shippingPhone) && !phones.Contains(shippingPhone.Trim()))
            {
                phones.Add(shippingPhone.Trim());
            }
            string identityNum = address.CivilNo;
            string gender = customer.GetAttribute<string>(SystemCustomerAttributeNames.Gender);
            if (gender != "F" && gender != "M")
            {
                gender = address.Title;
            }
            //segment-role
            string segment = null;
            var customerRole = customer.CustomerRoles.FirstOrDefault(x => !x.IsSystemRole);
            if (customerRole != null)
            {
                segment = customerRole.Name;
            }
            return nebimIntegrationProvider.AddUpdateCustomerToNebim(description, languageCode, firstName, lastName, emails, phones, identityNum, gender, segment, customer.CreatedOnUtc);
        }

        public string AddUpdateOrderToNebim(Core.Domain.Orders.Order order, string customerCode, Guid? shippingPostalAddressID, Guid? billingPostalAddressID)
        {
            var nebimIntegrationProvider = LoadNebimIntegrationServiceBySystemName("Misc.Nebim");
            string description = order.Id.ToString();
            DateTime? orderDate = _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, TimeZoneInfo.Utc);
            #region parameters
            //string itemCode,
            //string colorCode,
            //string itemDim1Code,
            //string itemDim2Code,
            //int quantity,
            //decimal priceExcludingTax,
            //decimal priceIncludingTax,
            //in the rest tupple
            //barcode
            #endregion parameters
            IList<Tuple<string, string, string, string, int, decimal?, decimal?, Tuple<string>>> items = new List<Tuple<string, string, string, string, int, decimal?, decimal?, Tuple<string>>>();

            foreach (var orderItem in order.OrderProductVariants)
            {
                //string itemCode = string.IsNullOrWhiteSpace(orderItem.ProductVariant.ManufacturerPartNumber) ? orderItem.ProductVariant.Sku : orderItem.ProductVariant.ManufacturerPartNumber;
                string itemCode = string.IsNullOrWhiteSpace(orderItem.ProductVariant.ManufacturerPartNumber) ? GetProductCode(orderItem.ProductVariant) : orderItem.ProductVariant.ManufacturerPartNumber;
                
                var barcode = orderItem.ProductVariant.Gtin;
                string colorCode = null;
                string itemDim1Code = null;
                var pvaValues = _productAttributeParser.ParseProductVariantAttributeValues(orderItem.AttributesXml);
                var pvavColor = pvaValues.FirstOrDefault(x => x.ProductVariantAttribute.ProductAttribute.Id == _NebimIntegrationSettings.ColorAttributeId);
                var pvavDims = pvaValues.Where(x => (x.ProductVariantAttribute.ProductAttribute.Id == _NebimIntegrationSettings.Dim1AttributeId) || (x.ProductVariantAttribute.ProductAttribute.Id == _NebimIntegrationSettings.Dim2AttributeId)).ToList();

                if (pvavColor != null)
                {
                    colorCode = pvavColor.ProductAttributeOptionId.ToString();

                    if (pvavDims.Count > 0)
                    {
                        var dim1 = pvavDims.First();
                        itemDim1Code = dim1.Name;//not localized!
                    }
                    else
                    {
                        itemDim1Code = _NebimIntegrationSettings.API_StandardSizeCode;
                    }
                }
                else
                {
                    colorCode = _NebimIntegrationSettings.API_StandardColorCode;
                    if (pvavDims.Count > 0)
                    {
                        var dim1 = pvavDims.First();
                        itemDim1Code = dim1.Name;//not localized!
                    }
                    else
                    {
                        itemDim1Code = _NebimIntegrationSettings.API_StandardSizeCode;
                    }
                }
                string itemDim2Code = null;
                int quantity = orderItem.Quantity;
                decimal? discountIncudingTax = orderItem.DiscountAmountInclTax;
                decimal? priceIncludingTax = orderItem.UnitPriceInclTax;
                string orderItemDescription = null;

                Tuple<string, string, string, string, int, decimal?, decimal?, Tuple<string>> item = Tuple.Create<string, string, string, string, int, decimal?, decimal?, string>(itemCode, colorCode, itemDim1Code, itemDim2Code, quantity, discountIncudingTax, priceIncludingTax, barcode);
                items.Add(item);
            }
            decimal? discountAmount = null;
            decimal? giftCardAmount = null;
            double? discountRate = null;
            //Discount discountToOrderTotal = null; 
            string discountNames = null;//"--" seperated discount names.

            if (order.GiftCardUsageHistory != null && order.GiftCardUsageHistory.Count > 0)
            {
                giftCardAmount = order.GiftCardUsageHistory.Sum(x => x.UsedValue);

            }
            if (order.DiscountUsageHistory != null && order.DiscountUsageHistory.Count > 0)
            {
                //order total discount
                //discountToOrderTotal = order.DiscountUsageHistory.Where(n => (n.Discount.DiscountTypeId == 1 || n.Discount.DiscountTypeId == 20)).Select(n => n.Discount).FirstOrDefault();
                //all discount names sperated by "--"
                discountNames = string.Join("--", order.DiscountUsageHistory.Select(n => n.Discount.Name));
            }
            if (order.DiscountUsageHistory.Any(n => (n.Discount.DiscountTypeId == 1 || n.Discount.DiscountTypeId == 20)))
            {
                //discountRate = (double)((order.OrderDiscount / (order.OrderTotal + order.OrderDiscount)) * 100);
                discountAmount = order.OrderDiscount;
            }
            if (discountAmount.HasValue || giftCardAmount.HasValue)
            {
                decimal dsc = discountAmount.HasValue ? discountAmount.Value : 0;
                decimal gft = giftCardAmount.HasValue ? giftCardAmount.Value : 0;
                discountRate = (double)(((dsc + gft) / (order.OrderTotal + dsc + gft)) * 100);
            }
            //shipment expense
            decimal shippingCost = order.OrderShippingInclTax;
            decimal giftBoxCost = 0;
            var caValues = _checkoutAttributeParser.ParseCheckoutAttributeValues(order.CheckoutAttributesXml);
            var giftBoxAttribute = caValues.FirstOrDefault(x => x.PriceAdjustment > 0);
            if (giftBoxAttribute != null)
            {
                giftBoxCost = giftBoxAttribute.PriceAdjustment;
            }
            string currencyCode = "TRY";
            // if barcode is null save order line(s) via color/dim values else save via barcode
            string orderNumber = nebimIntegrationProvider.AddUpdateOrderToNebim(description, orderDate, customerCode, shippingPostalAddressID, billingPostalAddressID, items, null, discountRate, currencyCode, shippingCost, giftBoxCost, discountNames);
            return orderNumber;
        }

        public void AddUpdateOrderPaymentToNebim(Core.Domain.Orders.Order order, string orderNumber)
        {
            var nebimIntegrationProvider = LoadNebimIntegrationServiceBySystemName("Misc.Nebim");
            int paymentType = 1;
            if (order.PaymentMethodSystemName == "Payments.PurchaseOrder")
                paymentType = 2;
            else
                paymentType = 1;
            int installment = order.Installment.HasValue ? order.Installment.Value : 1;
            installment = installment < 1 ? 1 : installment;
            string maskedCCno = _encryptionService.DecryptText(order.MaskedCreditCardNumber);
            string provisionNo = order.AuthorizationTransactionId;
            string creditCardTypeCode = null;
            if (paymentType == 1)// credit card 
            {
                Dictionary<string, string> paymentMethodSystemNameCreditCardType = new Dictionary<string, string>();
                try
                {
                    foreach (var match in _NebimIntegrationSettings.PaymentMethodSystemName_API_CreditCardTypeCode.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var codes = match.Split(new string[] { "--" }, StringSplitOptions.RemoveEmptyEntries);
                        paymentMethodSystemNameCreditCardType.Add(codes[0], codes[1]);
                    }
                    creditCardTypeCode = order.PaymentMethodSystemName == "" ? "indirim kuponu" : paymentMethodSystemNameCreditCardType[order.PaymentMethodSystemName];
                }
                catch (Exception ex)
                {
                    _logger.Error("Not able to match credit card type. Edit _NebimIntegrationSettings.PaymentMethodSystemName_API_CreditCardTypeCode", ex);
                }
            }
            nebimIntegrationProvider.AddUpdateOrderPaymentToNebim(orderNumber, paymentType, creditCardTypeCode, (byte)installment, maskedCCno, provisionNo);

        }

        public void CreateShipment(Core.Domain.Orders.Order order)
        {
            var nebimIntegrationProvider = LoadNebimIntegrationServiceBySystemName("Misc.Nebim");
            nebimIntegrationProvider.CreateShipment(order.Id.ToString());
        }
        
        //TODO: refactor, do it at one time!
        public void AddUpdateProductToNebim(Product product)
        {
            this.AddUpdateProductVariantsToNebim(_productService.GetProductVariantsByProductId(product.Id));
        }
        public void AddUpdateProductVariantToNebim(ProductVariant productVariant)
        {
            var nebimIntegrationProvider = LoadNebimIntegrationServiceBySystemName("Misc.Nebim");
            IProductAttributeParser productAttributeParser = EngineContext.Current.Resolve<IProductAttributeParser>();
            ITaxService taxService = EngineContext.Current.Resolve<ITaxService>();
            IWorkContext workContext = EngineContext.Current.Resolve<IWorkContext>();
            ISpecificationAttributeService specificationAttributeService = EngineContext.Current.Resolve<ISpecificationAttributeService>();
            var pv = productVariant;
            var product = pv.Product;
            if (pv == null)
            {
                return;
            }
            try
            {
                string barcode = "";
                string nameTr = "", nameEn = "";
                int dimTypeCode = 0;
                string colorCode = "";
                string colorNameTr = "", colorNameEn = "";
                string dim1Code = "", dim1TR = "", dim1En = "", dim2Code = "";
                string taxRate = string.Format("%{0}", taxService.GetTaxRate(pv.TaxCategoryId, workContext.CurrentCustomer));
                Manufacturer manufacturer = null;
                string manufacturerName = "", manufacturerCode = "";
                string attributeCode = "", attributeNameTR = "", attributeNameEN = "", attribute3Code = "", attribute3NameTR = "", attribute3NameEN = "";
                decimal? USD = null, TL = null, EURO = null, CHF = null;
                string currencyCode = "TRY";
                decimal price;
                decimal purchasePrice;
                IList<Tuple<string, string, string, string, string, string,string>> combinations = new List<Tuple<string, string, string, string, string, string, string>>();
                IList<Tuple<byte, string, string, string>> attributes = new List<Tuple<byte, string, string, string>>();

                nameTr = pv.GetLocalized(x => x.Name, 2);
                if (string.IsNullOrWhiteSpace(nameTr)) nameTr = product.GetLocalized(x => x.Name, 2);
                nameEn = pv.GetLocalized(x => x.Name, 1);
                if (string.IsNullOrWhiteSpace(nameEn)) nameEn = product.GetLocalized(x => x.Name, 1);

                #region hierarchy
                //var category = product.GetDefaultProductCategory();
                var category = product.GetPublishDefaultProductCategory();
                List<Category> categories = new List<Category>();
                List<int> categoryIds = new List<int>();

                while (category != null && //category is not null
                        !category.Deleted && //category is not deleted
                        category.Published) //category is published
                {
                    categories.Add(category);
                    category = _categoryService.GetCategoryById(category.ParentCategoryId);
                }
                categories.Reverse();
                categoryIds = categories.Select(x => x.Id).ToList();
                #region match hierarchy by categoryId and hierarchylevelCode matching input

                //List<int> hierarchyLevelCodes = new List<int>();
               
                //var categoryHierarchyLevelMatchDictionary = new Dictionary<int, int>();
                //try
                //{
                //    var categoryHierarchyLevelMatches = _NebimIntegrationSettings.Category_API_HierarchyLevelCodes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                //    foreach (var match in categoryHierarchyLevelMatches)
                //    {
                //        var ids = match.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                //        categoryHierarchyLevelMatchDictionary.Add(int.Parse(ids[0]), int.Parse(ids[1]));
                //    }

                //    foreach (var catId in categoryIds)
                //    {
                //        hierarchyLevelCodes.Add(categoryHierarchyLevelMatchDictionary[catId]);
                //    }
                //}
                //catch (Exception ex)
                //{
                //    throw new Exception("categoryHierarchyLevelMatchDictionary matching error", ex);
                //}
                #endregion match hierarchy by categoryId and hierarchylevelCode matching input

                Dictionary<int, string> hierarchyLevelCodeNames = new Dictionary<int, string>();

                for(int i = 0; i<categories.Count;i++)
                {
                    var cat = categories[i];
                    hierarchyLevelCodeNames.Add(i+1,cat.GetLocalized(x=> x.Name,2));
                }

                //there are always 3 levels hierarchy for af if not add standard "none" category
                if (_NebimIntegrationSettings.Database == "V3_DaimaModa" || _NebimIntegrationSettings.Database == "V3_Test")
                {
                    while (hierarchyLevelCodeNames.Count < 3)
                    {
                        hierarchyLevelCodeNames.Add(hierarchyLevelCodeNames.Count + 1, "NONE");
                    }
                }

                #endregion hierarchy
   
                #region attributes
                
                manufacturer = product.GetDefaultManufacturer();
                if (manufacturer != null)
                {
                    manufacturerName = manufacturer.Name;
                    manufacturerCode = manufacturer.Id.ToString();
                    if (_NebimIntegrationSettings.API_AttributeTypeCodeForManufacturer > 0 && _NebimIntegrationSettings.API_AttributeTypeCodeForManufacturer < 21)
                    {
                        attributes.Add(Tuple.Create<byte, string, string, string>(_NebimIntegrationSettings.API_AttributeTypeCodeForManufacturer, manufacturer.Id.ToString(), manufacturer.Name, manufacturer.Name));
                    }
                }

                var psas = specificationAttributeService.GetProductSpecificationAttributesByProductId(pv.ProductId);

                //matching convention: "specificationId-attributeTypeCode" by comma separated. ex: 3-1,5-2,34-3
                foreach (var match in _NebimIntegrationSettings.SpecificationAttribute_API_AttributeTypeCodes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    try
                    {
                        var ids = match.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                        var specificationAttributeId = int.Parse(ids[0]);
                        var attributeTypeCode = byte.Parse(ids[1]);
                        var specification = psas.FirstOrDefault(x => x.SpecificationAttributeOption.SpecificationAttributeId == specificationAttributeId);
                        if (specification != null)
                        {
                            attributeCode = specification.SpecificationAttributeOptionId.ToString();
                            attributeNameTR = specification.SpecificationAttributeOption.GetLocalized(x => x.Name, 2);
                            attributeNameEN = specification.SpecificationAttributeOption.GetLocalized(x => x.Name, 1);
                            attributes.Add(Tuple.Create<byte, string, string, string>(attributeTypeCode, attributeCode, attributeNameTR, attributeNameEN));
                        }

                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Specification attribute-Nebim attribute matching error", ex);
                    }
                }

                #endregion attributes
                if (pv.CurrencyId.HasValue)
                {
                    currencyCode = pv.Currency.CurrencyCode;
                    price = pv.CurrencyPrice.HasValue ? pv.CurrencyPrice.Value : pv.Price;
                    switch (pv.CurrencyId.Value)
                    {
                        case 12://tl    
                            TL = pv.Price;
                            break;
                        case 1://$    
                            USD = pv.CurrencyPrice;
                            break;
                        case 6://Euro    
                            EURO = pv.CurrencyPrice;
                            break;
                        case 13://CHF   
                            CHF = pv.CurrencyPrice;
                            break;
                        default:
                            TL = pv.Price;
                            break;
                    }
                }
                else
                {
                    TL = pv.Price;
                    price = pv.Price;
                }
                //TODO: calculate purchase price, ?should or not include vat??
            //      public virtual decimal GetProductPrice(ProductVariant productVariant, int taxCategoryId,
            //decimal price, bool includingTax, Customer customer,
            //bool priceIncludesTax, out decimal taxRate)
                decimal taxR = 0;
                purchasePrice = _taxService.GetProductPrice(pv, pv.TaxCategoryId, price, false, _workContext.CurrentCustomer, true, out taxR);

                var pvas = pv.ProductVariantAttributes;
                var combs = pv.ProductVariantAttributeCombinations;
                //   var pvaValues = productAttributeParser.ParseProductVariantAttributeValues(comb.AttributesXml);


                if (!pvas.Any())
                {
                    //dimTypeCode = 0;
                    //colorCode = "";
                    //colorNameTr = "";
                    //colorNameEn = "";
                    //barcode = productVariant.Gtin;
                    dimTypeCode = 2;
                    colorCode = _NebimIntegrationSettings.API_StandardColorCode;
                    colorNameTr = _NebimIntegrationSettings.API_StandardColorCode;
                    colorNameEn = _NebimIntegrationSettings.API_StandardColorCode;
                    dim1Code = _NebimIntegrationSettings.API_StandardSizeCode;
                    dim1TR = _NebimIntegrationSettings.API_StandardSizeCode;
                    dim1En = _NebimIntegrationSettings.API_StandardSizeCode;
                    barcode = productVariant.Gtin;
                    combinations.Add(Tuple.Create<string, string, string, string, string, string, string>(colorCode, colorNameTr, colorNameEn, dim1Code, dim1TR, dim1En, barcode));
                }

                else if (!combs.Any())
                {
                    if (pvas.Any(x => x.ProductAttribute.Id == _NebimIntegrationSettings.ColorAttributeId))//only color attribute
                    {
                        //dimTypeCode = 1;
                        //barcode = productVariant.Gtin;
                        //var value = pvas.FirstOrDefault(x => x.ProductAttribute.Id == _NebimIntegrationSettings.ColorAttributeId).ProductVariantAttributeValues.FirstOrDefault();

                        //if (value != null)
                        //{
                        //    colorNameTr = value.GetLocalized(pvav => pvav.Name, 2);
                        //    colorNameEn = value.GetLocalized(pvav => pvav.Name, 1);
                        //    colorCode = value.ProductAttributeOptionId.ToString();
                        //}
                        dimTypeCode = 2;
                        barcode = productVariant.Gtin;
                        var value = pvas.FirstOrDefault(x => x.ProductAttribute.Id == _NebimIntegrationSettings.ColorAttributeId).ProductVariantAttributeValues.FirstOrDefault();
                        if (value != null)
                        {
                            colorNameTr = value.GetLocalized(pvav => pvav.Name, 2);
                            colorNameEn = value.GetLocalized(pvav => pvav.Name, 1);
                            colorCode = value.ProductAttributeOptionId.ToString();
                        }
                        dim1Code = _NebimIntegrationSettings.API_StandardSizeCode;
                        dim1TR = _NebimIntegrationSettings.API_StandardSizeCode;
                        dim1En = _NebimIntegrationSettings.API_StandardSizeCode;
                        combinations.Add(Tuple.Create<string, string, string, string, string, string, string>(colorCode, colorNameTr, colorNameEn, dim1Code, dim1TR, dim1En, barcode));
                    }
                    else // add default color so have +1 attributes. if dimension attribute exists and there is no combination, save the poduct eith std color and dimension 
                    {
                        //colorCode = "STD";
                        //colorNameTr = "";
                        //colorNameEn = "";
                        //dimTypeCode = 2;
                        //var nonColorAtributes = pvas.Where(x => x.ProductAttribute.Id != _NebimIntegrationSettings.ColorAttributeId).ToList();
                        //var first = nonColorAtributes.FirstOrDefault();
                        //if (first != null)
                        //{
                        //    var value = first.ProductVariantAttributeValues.FirstOrDefault();
                        //    dim1Code = value == null ? "" : value.GetLocalized(x => x.Name, 2);
                        //    dim1TR = dim1Code;
                        //    dim1En = value == null ? "" : value.GetLocalized(x => x.Name, 1);
                        //}

                        dimTypeCode = 2;
                        colorCode = _NebimIntegrationSettings.API_StandardColorCode;
                        colorNameTr = _NebimIntegrationSettings.API_StandardColorCode;
                        colorNameEn = _NebimIntegrationSettings.API_StandardColorCode;
                        dim1Code = _NebimIntegrationSettings.API_StandardSizeCode;
                        dim1TR = _NebimIntegrationSettings.API_StandardSizeCode;
                        dim1En = _NebimIntegrationSettings.API_StandardSizeCode;
                        barcode = productVariant.Gtin;
                        combinations.Add(Tuple.Create<string, string, string, string, string, string, string>(colorCode, colorNameTr, colorNameEn, dim1Code, dim1TR, dim1En, barcode));
                    }
                }

                if (combs.Any())//has combinations
                {
                    //add rows for each combination
                    foreach (var comb in combs)
                    {

                        var pvaValues = productAttributeParser.ParseProductVariantAttributeValues(comb.AttributesXml);
                        var pvavColor = pvaValues.FirstOrDefault(x => x.ProductVariantAttribute.ProductAttribute.Id == _NebimIntegrationSettings.ColorAttributeId);
                        //for af there are 2 dim attribute, for bisse and hatemoglu there is 1.
                        var pvavDims = pvaValues.Where(x => (x.ProductVariantAttribute.ProductAttribute.Id == _NebimIntegrationSettings.Dim1AttributeId)||(x.ProductVariantAttribute.ProductAttribute.Id == _NebimIntegrationSettings.Dim2AttributeId)).ToList();
                        barcode = comb.ProductVariantBarcode;
                        if (pvavColor != null && pvavDims.Count == 0)
                        {
                            //dimTypeCode = 1;
                            //colorNameTr = pvavColor.GetLocalized(pvav => pvav.Name, 2);
                            //colorNameEn = pvavColor.GetLocalized(pvav => pvav.Name, 1);
                            //colorCode = pvavColor.ProductAttributeOptionId.ToString();

                            dimTypeCode = 2;
                            if (pvavColor != null)
                            {
                                colorNameTr = pvavColor.GetLocalized(pvav => pvav.Name, 2);
                                colorNameEn = pvavColor.GetLocalized(pvav => pvav.Name, 1);
                                colorCode = pvavColor.ProductAttributeOptionId.ToString();
                            }
                            dim1Code = _NebimIntegrationSettings.API_StandardSizeCode;
                            dim1TR = _NebimIntegrationSettings.API_StandardSizeCode;
                            dim1En = _NebimIntegrationSettings.API_StandardSizeCode;
                           
                        }
                        else if (pvavColor != null && pvavDims.Count > 0)
                        {
                            var dim1 = pvavDims.First();
                            dimTypeCode = 2;
                            colorNameTr = pvavColor.GetLocalized(pvav => pvav.Name, 2);
                            colorNameEn = pvavColor.GetLocalized(pvav => pvav.Name, 1);
                            colorCode = pvavColor.ProductAttributeOptionId.ToString();
                            dim1Code = dim1.GetLocalized(x => x.Name, 2);
                            dim1TR = dim1Code;
                            dim1En = dim1.GetLocalized(x => x.Name, 1);
                        }
                        else//no color attribute
                        {
                            dimTypeCode = 2;
                            colorNameTr = _NebimIntegrationSettings.API_StandardColorCode;
                            colorNameEn = _NebimIntegrationSettings.API_StandardColorCode;
                            colorCode = _NebimIntegrationSettings.API_StandardColorCode;
                            var dim1 = pvavDims.FirstOrDefault();
                            dim1Code = dim1 == null ? "" : dim1.GetLocalized(x => x.Name, 2);
                            dim1TR = dim1Code;
                            dim1En = dim1 == null ? "" : dim1.GetLocalized(x => x.Name, 1);

                        }
                        //TODO: store and get barcode from combination!
                        combinations.Add(Tuple.Create<string, string, string, string, string, string, string>(colorCode, colorNameTr, colorNameEn, dim1Code, dim1TR, dim1En, barcode));
                    }

                }

                //GetProductCode(pv)
               // nebimIntegrationProvider.AddUpdateProductToNebim(pv.Sku, nameTr, nameEn, dimTypeCode, combinations, "AD", "TR", taxRate, taxRate, taxRate, hierarchyLevelCodes.ToArray(), attributes, price, currencyCode);
                nebimIntegrationProvider.AddUpdateProductToNebim(barcode, GetProductCode(pv), nameTr, nameEn, dimTypeCode, combinations, "AD", "TR", taxRate, taxRate, taxRate, hierarchyLevelCodeNames, attributes, price, purchasePrice, 0, currencyCode);
            }
            catch (Exception ex)
            {
                throw new Exception("NebimExport: productid='" + product.Id + "'   variantId:'" + pv.Id + "'. Inner Message:"+ex.Message , ex);
            }



        }
        public void AddUpdateProductsToNebim(IList<Product> products)
        {
            var nebimIntegrationProvider = LoadNebimIntegrationServiceBySystemName("Misc.Nebim");
            IProductAttributeParser productAttributeParser = EngineContext.Current.Resolve<IProductAttributeParser>();
            ITaxService taxService = EngineContext.Current.Resolve<ITaxService>();
            IWorkContext workContext = EngineContext.Current.Resolve<IWorkContext>();
            ISpecificationAttributeService specificationAttributeService = EngineContext.Current.Resolve<ISpecificationAttributeService>();
            foreach (var pv in products)
            {
                AddUpdateProductToNebim(pv);
            }


            // we had better add some document properties to the spreadsheet 


        }
        public void AddUpdateProductVariantsToNebim(IList<ProductVariant> productVariants)
        {
            var nebimIntegrationProvider = LoadNebimIntegrationServiceBySystemName("Misc.Nebim");
            IProductAttributeParser productAttributeParser = EngineContext.Current.Resolve<IProductAttributeParser>();
            ITaxService taxService = EngineContext.Current.Resolve<ITaxService>();
            IWorkContext workContext = EngineContext.Current.Resolve<IWorkContext>();
            ISpecificationAttributeService specificationAttributeService = EngineContext.Current.Resolve<ISpecificationAttributeService>();
            foreach (var pv in productVariants)
            {
                AddUpdateProductVariantToNebim(pv);
            }
        }
        private void AddUpdateOrderAddressesToNebim(Core.Domain.Orders.Order order, string customerCode, out Guid? ba, out Guid? sa)
        {
            var nebimIntegrationProvider = LoadNebimIntegrationServiceBySystemName("Misc.Nebim");
            var shippingAddress = order.ShippingAddress;
            var billingAddress = order.BillingAddress;
            if (shippingAddress.Id == billingAddress.Id)
            {
                ba = sa = this.AddUpdateAddressToNebimByCustomerCode(customerCode, shippingAddress, "F");
            }
            else
            {
                sa = this.AddUpdateAddressToNebimByCustomerCode(customerCode, shippingAddress, "T");
                ba = this.AddUpdateAddressToNebimByCustomerCode(customerCode, billingAddress, "F");
            }
        }
        private Guid AddUpdateAddressToNebimByCustomerCode(string customerCode, Core.Domain.Common.Address address, string addressType=null)
        {
            var nebimIntegrationProvider = LoadNebimIntegrationServiceBySystemName("Misc.Nebim");
           
            string type = address.IsEnterprise ? "2" : "1"; 
            //fatura basýmý için dandik çözüm
            // fatura adresi için 7, teslimat adresi için 6 nolu adres tipleri atanýr.
            //string type = addressType == "F" ? "7" : "6";

            string firstName = address.FirstName;
            string lastName = address.LastName;
            if (address.IsEnterprise)//þirket adresi
            {
                if(!string.IsNullOrEmpty(address.Company))
                {
                    firstName = address.Company;
                    lastName = string.Empty;
                }
            }
            string addressLine = address.Address1 + " " + address.Address2;
            string district = address.City;
            string city = address.StateProvince == null ? "" : this.GetAPICityCode(address.StateProvince.Id.ToString());
            string zipCode = address.ZipPostalCode;
            string countryCode = address.Country == null ? "TR" : address.Country.TwoLetterIsoCode.ToUpper();
            string taxNumber = address.TaxNo;
            string taxOffice = address.TaxOffice;

            try
            {
                //shipping address
                return nebimIntegrationProvider.AddUpdateCustomerAddressToNebimByCustomerCode(customerCode,
                      type, firstName, lastName, addressLine,
                     district, city, zipCode,
                     countryCode, taxNumber, taxOffice);
            }
            catch (Exception ex)
            {
                throw new Exception("AddUpdateAddressToNebimByCustomerCode: customerId='" + customerCode+". Inner message: "+ex.Message , ex);
            }
        }

        private string GetAPICityCode(string B2CCityId)
        {
            string cityCode = null;
            Dictionary<string, string> cities = new Dictionary<string, string>();//(B2C city id) * (api city code)
                try
                {
                    foreach (var match in _NebimIntegrationSettings.StateProvinceId_API_CityCodes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var codes = match.Split(new string[] { "--" }, StringSplitOptions.RemoveEmptyEntries);
                        cities.Add(codes[0], codes[1]);
                    }
                    if (cities.ContainsKey(B2CCityId))
                        cityCode = cities[B2CCityId];
                }
                catch (Exception ex)
                {
                    _logger.Error("Not able to match credit card type. Edit _NebimIntegrationSettings.PaymentMethodSystemName_API_CreditCardTypeCode", ex);
                }
                return cityCode;

        }
        //Update order and all related data : products, customer, addresses
        public void SyncOrderToNebim(Core.Domain.Orders.Order order)
        {
            
            if (order == null)
                throw new ArgumentNullException("order");
            _logger.Information(string.Format("Nebim Integration:{0} id:{1}", "order", order.Id));

                #region productItems

                var orderProductVariants = _orderService.GetAllOrderProductVariants(order.Id, null, null, null, null, null, null); //(IEnumerable<OrderProductVariant>)order.OrderProductVariants;
                if (orderProductVariants.Count == 0)
                    throw new IndexOutOfRangeException("order items count can not be zero");

                var productVariants = orderProductVariants.Select(x => x.ProductVariant).ToList();

                AddUpdateProductVariantsToNebim(productVariants);

                #endregion productItems

                #region customer

                string customerCode = AddUpdateOrderCustomerToNebim(order);

                #endregion customer

                #region order
                Guid? ba, sa;
                AddUpdateOrderAddressesToNebim(order, customerCode, out ba, out sa);
                string orderNumber = AddUpdateOrderToNebim(order, customerCode, sa, ba);

                AddUpdateOrderPaymentToNebim(order, orderNumber);
                #endregion order
          
        }

        //This is an extra order syncronization to an external Nebim database
        // resets connection both start and end of the method.
        public void SyncOrderToNebimExt(Core.Domain.Orders.Order order)
        {

            if (order == null)
                throw new ArgumentNullException("order");
            _logger.Information(string.Format("Nebim Integration:{0} id:{1}", "order", order.Id));
            this.ResetNebimConnection();
            this.ConnectToNebim(_NebimIntegrationExtSettings);

            #region productItems

            var orderProductVariants = _orderService.GetAllOrderProductVariants(order.Id, null, null, null, null, null, null); //(IEnumerable<OrderProductVariant>)order.OrderProductVariants;
            if (orderProductVariants.Count == 0)
                throw new IndexOutOfRangeException("order items count can not be zero");

            var productVariants = orderProductVariants.Select(x => x.ProductVariant).ToList();

            AddUpdateProductVariantsToNebim(productVariants);

            #endregion productItems

            #region customer

            string customerCode = AddUpdateOrderCustomerToNebim(order);

            #endregion customer

            #region order
            Guid? ba, sa;
            AddUpdateOrderAddressesToNebim(order, customerCode, out ba, out sa);
            string orderNumber = AddUpdateOrderToNebim(order, customerCode, sa, ba);

            AddUpdateOrderPaymentToNebim(order, orderNumber);
            #endregion order

            this.ResetNebimConnection();

        }
        
        //Update all order and all related data : products, customer
        public void SyncOrdersToNebim()
        {
            var orders = _orderService.SearchOrders(DateTime.Now.AddDays(-10), null, null, null, null, null, null, 0, int.MaxValue);
            foreach (var order in orders)
            {
                this.SyncOrderToNebim(order);
            }
        }

        private void ResetNebimConnection()
        {
            var nebimIntegrationProvider = LoadNebimIntegrationServiceBySystemName("Misc.Nebim");
            nebimIntegrationProvider.ResetConnection();
        }

        private void ConnectToNebim(NebimIntegrationSettings settings)
        {
            var nebimIntegrationProvider = LoadNebimIntegrationServiceBySystemName("Misc.Nebim");
            nebimIntegrationProvider.Connect(settings);
        }

        private string GetProductCode(ProductVariant pv)
        {
              if (_NebimIntegrationSettings.Database == "V3_DaimaModa" || _NebimIntegrationSettings.Database == "V3_Test")//af
                return pv.Sku;
            var parts = pv.Sku.Split(new char[] { ' ' });// sku nun ilk kelimesi product code'dur.
            if (parts.Any())
                return parts[0];
            return null;
        }

    }
}
