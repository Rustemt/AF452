using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using Nop.Services.Seo;
using Nop.Web.Extensions;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Security;
using Nop.Web.Infrastructure;
using Nop.Web.Models.Common;
using Nop.Web.Models.Customer;
using Nop.Web.Models.Media;
using Nop.Web.Models.ShoppingCart;
using Nop.Web.Models.Topics;
using Nop.Services.Topics;
using Nop.Core.Domain.Custom;
using Nop.Core.Infrastructure;

namespace Nop.Web.Controllers
{
    public class ShoppingCartController : BaseNopController
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IPictureService _pictureService;
        private readonly ILocalizationService _localizationService;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly ITaxService _taxService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly ICheckoutAttributeFormatter _checkoutAttributeFormatter;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IDiscountService _discountService;
        private readonly ICustomerService _customerService;
        private readonly IGiftCardService _giftCardService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IShippingService _shippingService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ICheckoutAttributeService _checkoutAttributeService;
        private readonly IPaymentService _paymentService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IPermissionService _permissionService;

        private readonly MediaSettings _mediaSetting;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly OrderSettings _orderSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly TaxSettings _taxSettings;
        private readonly ITokenizer _tokenizer = EngineContext.Current.Resolve<ITokenizer>();
        private readonly ITopicService _topicService = EngineContext.Current.Resolve<ITopicService>();
        private readonly IMessageTokenProvider _messageTokenProvider = EngineContext.Current.Resolve<IMessageTokenProvider>();

        #endregion

        #region Constructors

        public ShoppingCartController(IProductService productService, IWorkContext workContext,
            IShoppingCartService shoppingCartService, IPictureService pictureService,
            ILocalizationService localizationService, IProductAttributeFormatter productAttributeFormatter,
            ITaxService taxService, ICurrencyService currencyService,
            IPriceCalculationService priceCalculationService, IPriceFormatter priceFormatter,
            ICheckoutAttributeParser checkoutAttributeParser, ICheckoutAttributeFormatter checkoutAttributeFormatter,
            IOrderProcessingService orderProcessingService,
            IDiscountService discountService, ICustomerService customerService,
            IGiftCardService giftCardService, ICountryService countryService,
            IStateProvinceService stateProvinceService, IShippingService shippingService,
            IOrderTotalCalculationService orderTotalCalculationService,
            ICheckoutAttributeService checkoutAttributeService, IPaymentService paymentService,
            IWorkflowMessageService workflowMessageService,
            IProductAttributeService productAttributeService,
            IProductAttributeParser productAttributeParser,
            IPermissionService permissionService,
            MediaSettings mediaSetting, ShoppingCartSettings shoppingCartSettings,
            CatalogSettings catalogSettings, OrderSettings orderSettings,
            ShippingSettings shippingSettings, TaxSettings taxSettings)
        {
            this._productService = productService;
            this._workContext = workContext;
            this._shoppingCartService = shoppingCartService;
            this._pictureService = pictureService;
            this._localizationService = localizationService;
            this._productAttributeFormatter = productAttributeFormatter;
            this._taxService = taxService;
            this._currencyService = currencyService;
            this._priceCalculationService = priceCalculationService;
            this._priceFormatter = priceFormatter;
            this._checkoutAttributeParser = checkoutAttributeParser;
            this._checkoutAttributeFormatter = checkoutAttributeFormatter;
            this._orderProcessingService = orderProcessingService;
            this._discountService = discountService;
            this._customerService = customerService;
            this._giftCardService = giftCardService;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._shippingService = shippingService;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._checkoutAttributeService = checkoutAttributeService;
            this._paymentService = paymentService;
            this._workflowMessageService = workflowMessageService;
            this._productAttributeService = productAttributeService;
            this._productAttributeParser = productAttributeParser;
            this._permissionService = permissionService;

            this._mediaSetting = mediaSetting;
            this._shoppingCartSettings = shoppingCartSettings;
            this._catalogSettings = catalogSettings;
            this._orderSettings = orderSettings;
            this._shippingSettings = shippingSettings;
            this._taxSettings = taxSettings;
        }

        #endregion

        #region Utilities

        [NonAction]
        private ShoppingCartModel PrepareMiniShoppingCartModel(ShoppingCartModel model,
            IList<Nop.Core.Domain.Orders.ShoppingCartItem> cart, bool isEditable, bool setEstimateShippingDefaultAddress = true)
        {
            if (cart == null)
                throw new ArgumentNullException("cart");

            if (model == null)
                throw new ArgumentNullException("model");

            if (cart.Count == 0)
                return model;

            #region Simple properties

            model.IsEditable = isEditable;
            model.ShowProductImages = _shoppingCartSettings.ShowProductImagesOnShoppingCart;
            model.ShowSku = _catalogSettings.ShowProductSku;
            model.CheckoutAttributeInfo = _checkoutAttributeFormatter.FormatAttributes(_workContext.CurrentCustomer.CheckoutAttributes, _workContext.CurrentCustomer);
            bool minOrderSubtotalAmountOk = _orderProcessingService.ValidateMinOrderSubtotalAmount(cart);
            if (!minOrderSubtotalAmountOk)
            {
                decimal minOrderSubtotalAmount = _currencyService.ConvertFromPrimaryStoreCurrency(_orderSettings.MinOrderSubtotalAmount, _workContext.WorkingCurrency);
                model.MinOrderSubtotalWarning = string.Format(_localizationService.GetResource("Checkout.MinOrderSubtotalAmount"), _priceFormatter.FormatPrice(minOrderSubtotalAmount, true, false));
            }
            model.TermsOfServiceEnabled = _orderSettings.TermsOfServiceEnabled;
            model.ShowDiscountBox = _shoppingCartSettings.ShowDiscountBox;
            model.ShowGiftCardBox = _shoppingCartSettings.ShowGiftCardBox;

            //cart warnings
            var cartWarnings = _shoppingCartService.GetShoppingCartWarnings(cart, "", false);
            foreach (var warning in cartWarnings)
                model.Warnings.Add(warning);

            #endregion
/*
            #region Checkout attributes

            var checkoutAttributes = _checkoutAttributeService.GetAllCheckoutAttributes(!cart.RequiresShipping());
            foreach (var attribute in checkoutAttributes)
            {
                var caModel = new ShoppingCartModel.CheckoutAttributeModel()
                {
                    Id = attribute.Id,
                    Name = attribute.GetLocalized(x => x.Name),
                    TextPrompt = attribute.TextPrompt,
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType
                };

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var caValues = _checkoutAttributeService.GetCheckoutAttributeValues(attribute.Id);
                    foreach (var caValue in caValues)
                    {
                        var pvaValueModel = new ShoppingCartModel.CheckoutAttributeValueModel()
                        {
                            Id = caValue.Id,
                            Name = caValue.GetLocalized(x => x.Name),
                            IsPreSelected = caValue.IsPreSelected
                        };
                        caModel.Values.Add(pvaValueModel);

                        //display price if allowed
                        if (_permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
                        {
                            decimal priceAdjustmentBase = _taxService.GetCheckoutAttributePrice(caValue);
                            decimal priceAdjustment = _currencyService.ConvertFromPrimaryStoreCurrency(priceAdjustmentBase, _workContext.WorkingCurrency);
                            if (priceAdjustmentBase > decimal.Zero)
                                pvaValueModel.PriceAdjustment = "+" + _priceFormatter.FormatPrice(priceAdjustment);
                            else if (priceAdjustmentBase < decimal.Zero)
                                pvaValueModel.PriceAdjustment = "-" + _priceFormatter.FormatPrice(-priceAdjustment);
                        }
                    }
                }



                //set already selected attributes
                string selectedCheckoutAttributes = _workContext.CurrentCustomer.CheckoutAttributes;
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.Checkboxes:
                        {
                            if (!String.IsNullOrEmpty(selectedCheckoutAttributes))
                            {
                                //clear default selection
                                foreach (var item in caModel.Values)
                                    item.IsPreSelected = false;

                                //select new values
                                var selectedCaValues = _checkoutAttributeParser.ParseCheckoutAttributeValues(selectedCheckoutAttributes);
                                foreach (var caValue in selectedCaValues)
                                    foreach (var item in caModel.Values)
                                        if (caValue.Id == item.Id)
                                            item.IsPreSelected = true;
                            }
                        }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            if (!String.IsNullOrEmpty(selectedCheckoutAttributes))
                            {
                                var enteredText = _checkoutAttributeParser.ParseValues(selectedCheckoutAttributes, attribute.Id);
                                if (enteredText.Count > 0)
                                    caModel.DefaultValue = enteredText[0];
                            }
                        }
                        break;
                    default:
                        break;
                }

                model.CheckoutAttributes.Add(caModel);
            }

            #endregion

            #region Estimate shipping

            model.EstimateShipping.Enabled = cart.Count > 0 && cart.RequiresShipping() && _shippingSettings.EstimateShippingEnabled;
            if (model.EstimateShipping.Enabled)
            {
                //countries
                int? defaultEstimateCountryId = (setEstimateShippingDefaultAddress && _workContext.CurrentCustomer.ShippingAddress != null) ? _workContext.CurrentCustomer.ShippingAddress.CountryId : model.EstimateShipping.CountryId;
                model.EstimateShipping.AvailableCountries.Add(new SelectListItem() { Text = _localizationService.GetResource("Address.SelectCountry"), Value = "0" });
                foreach (var c in _countryService.GetAllCountries())
                    model.EstimateShipping.AvailableCountries.Add(new SelectListItem()
                    {
                        Text = c.Name,
                        Value = c.Id.ToString(),
                        Selected = c.Id == defaultEstimateCountryId
                    });
                //states
                int? defaultEstimateStateId = (setEstimateShippingDefaultAddress && _workContext.CurrentCustomer.ShippingAddress != null) ? _workContext.CurrentCustomer.ShippingAddress.StateProvinceId : model.EstimateShipping.StateProvinceId;
                var states = defaultEstimateCountryId.HasValue ? _stateProvinceService.GetStateProvincesByCountryId(defaultEstimateCountryId.Value).ToList() : new List<StateProvince>();
                if (states.Count > 0)
                    foreach (var s in states)
                        model.EstimateShipping.AvailableStates.Add(new SelectListItem()
                        {
                            Text = s.Name,
                            Value = s.Id.ToString(),
                            Selected = s.Id == defaultEstimateStateId
                        });
                else
                    model.EstimateShipping.AvailableStates.Add(new SelectListItem() { Text = _localizationService.GetResource("Address.OtherNonUS"), Value = "0" });

                if (setEstimateShippingDefaultAddress && _workContext.CurrentCustomer.ShippingAddress != null)
                    model.EstimateShipping.ZipPostalCode = _workContext.CurrentCustomer.ShippingAddress.ZipPostalCode;
            }

            #endregion
            */
            #region Cart items

            foreach (var sci in cart)
            {
                var cartItemModel = new ShoppingCartModel.ShoppingCartItemModel()
                {
                    Id = sci.Id,
                    Sku = sci.ProductVariant.Sku,
                    ProductId = sci.ProductVariant.ProductId,
                    ProductSeName = sci.ProductVariant.Product.GetSeName(),
                    Quantity = sci.Quantity,
                    MaxQuantity = Math.Min(_productService.GetVariantStockQuantity(sci.ProductVariant, sci.AttributesXml), sci.ProductVariant.OrderMaximumQuantity),
                    AttributeInfo = _productAttributeFormatter.FormatAttributes(sci.ProductVariant, sci.AttributesXml, "<span>{0}</span> :{1}", _workContext.CurrentCustomer, htmlEncode: false),
                    VariantId = sci.ProductVariant.Id

                };
                //Stock quantity


                //recurring info
                if (sci.ProductVariant.IsRecurring)
                    cartItemModel.RecurringInfo = string.Format(_localizationService.GetResource("ShoppingCart.RecurringPeriod"), sci.ProductVariant.RecurringCycleLength, sci.ProductVariant.RecurringCyclePeriod.GetLocalizedEnum(_localizationService, _workContext));

                //unit prices
                if (sci.ProductVariant.CallForPrice && sci.ProductVariant.Price == 0)
                {

                    cartItemModel.UnitPrice = _localizationService.GetResource("Products.Price.CallForPrice");
                }
                else
                {
                    decimal taxRate = decimal.Zero;
                    decimal shoppingCartUnitPriceWithDiscountBase = _taxService.GetProductPrice(sci.ProductVariant, _priceCalculationService.GetUnitPrice(sci, true), out taxRate);
                    decimal shoppingCartUnitPriceWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartUnitPriceWithDiscountBase, _workContext.WorkingCurrency);
                    cartItemModel.UnitPrice = _priceFormatter.FormatPrice(shoppingCartUnitPriceWithDiscount);
                }
                //subtotal, discount
                if (sci.ProductVariant.CallForPrice && sci.ProductVariant.Price == 0)
                {
                    cartItemModel.SubTotal = _localizationService.GetResource("Products.Price.CallForPrice");
                }
                else
                {
                    //sub total
                    decimal taxRate = decimal.Zero;
                    decimal shoppingCartItemSubTotalWithDiscountBase = _taxService.GetProductPrice(sci.ProductVariant, _priceCalculationService.GetSubTotal(sci, true), out taxRate);
                    decimal shoppingCartItemSubTotalWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartItemSubTotalWithDiscountBase, _workContext.WorkingCurrency);
                    cartItemModel.SubTotal = _priceFormatter.FormatPrice(shoppingCartItemSubTotalWithDiscount);

                    //display an applied discount amount
                    decimal shoppingCartItemSubTotalWithoutDiscountBase = _taxService.GetProductPrice(sci.ProductVariant, _priceCalculationService.GetSubTotal(sci, false), out taxRate);
                    decimal shoppingCartItemDiscountBase = shoppingCartItemSubTotalWithoutDiscountBase - shoppingCartItemSubTotalWithDiscountBase;
                    if (shoppingCartItemDiscountBase > decimal.Zero)
                    {
                        decimal shoppingCartItemDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartItemDiscountBase, _workContext.WorkingCurrency);
                        cartItemModel.Discount = _priceFormatter.FormatPrice(shoppingCartItemDiscount);
                    }
                }

                //product name
                if (!String.IsNullOrEmpty(sci.ProductVariant.GetLocalized(x => x.Name)))
                    cartItemModel.ProductName =  sci.ProductVariant.GetLocalized(x => x.Name);
                else
                    cartItemModel.ProductName = sci.ProductVariant.Product.GetLocalized(x => x.Name);

                //Manufacturer name
                cartItemModel.Manufacturer = sci.ProductVariant.Product.ManufacturerName();

                //picture
                if (_shoppingCartSettings.ShowProductImagesOnShoppingCart)
                {
                    var picture = sci.ProductVariant.GetDefaultProductVariantPicture(_pictureService);
                    cartItemModel.Picture = new PictureModel()
                    {
                        ImageUrl = _pictureService.GetPictureUrl(picture, _mediaSetting.MiniCartThumbPictureSize, true),
                        Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat"), cartItemModel.ProductName),
                        AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat"), cartItemModel.ProductName),

                    };
                }

                //item warnings
                var itemWarnings = _shoppingCartService.GetShoppingCartItemWarnings(_workContext.CurrentCustomer,
                            sci.ShoppingCartType,
                            sci.ProductVariant,
                            sci.AttributesXml,
                            sci.CustomerEnteredPrice,
                            sci.Quantity, false);
                foreach (var warning in itemWarnings)
                    cartItemModel.Warnings.Add(warning);
                model.Items.Add(cartItemModel);
            }

            decimal subtotalBase = decimal.Zero;
            decimal orderSubTotalDiscountAmountBase = decimal.Zero;
            Discount orderSubTotalAppliedDiscount = null;
            decimal subTotalWithoutDiscountBase = decimal.Zero;
            decimal subTotalWithDiscountBase = decimal.Zero;
            _orderTotalCalculationService.GetShoppingCartSubTotal(cart, true ,
                out orderSubTotalDiscountAmountBase, out orderSubTotalAppliedDiscount,
                out subTotalWithoutDiscountBase, out subTotalWithDiscountBase);
            subtotalBase = subTotalWithoutDiscountBase;
            decimal subtotal = _currencyService.ConvertFromPrimaryStoreCurrency(subtotalBase, _workContext.WorkingCurrency);
            model.SubTotal = _priceFormatter.FormatPrice(subtotal);

            #endregion
           /* 
            #region Button payment methods

            var boundPaymentMethods = _paymentService
                .LoadActivePaymentMethods()
                .Where(pm => pm.PaymentMethodType == PaymentMethodType.Button)
                .ToList();
            foreach (var pm in boundPaymentMethods)
            {
                if (cart.IsRecurring() && pm.RecurringPaymentType == RecurringPaymentType.NotSupported)
                    continue;

                string actionName;
                string controllerName;
                RouteValueDictionary routeValues;
                pm.GetPaymentInfoRoute(out actionName, out controllerName, out routeValues);

                model.ButtonPaymentMethodActionNames.Add(actionName);
                model.ButtonPaymentMethodControllerNames.Add(controllerName);
                model.ButtonPaymentMethodRouteValues.Add(routeValues);
            }

            #endregion
*/
            return model;
        }


        [NonAction]
        private ShoppingCartModel PrepareShoppingCartModel(ShoppingCartModel model,
            IList<Nop.Core.Domain.Orders.ShoppingCartItem> cart, bool isEditable, bool setEstimateShippingDefaultAddress = true, bool loadShippingNotification = false)
        {
            if (cart == null)
                throw new ArgumentNullException("cart");

            if (model == null)
                throw new ArgumentNullException("model");

            if (cart.Count == 0)
                return model;

            #region Simple properties
            model.IsEditable = isEditable;
            model.ShowProductImages = _shoppingCartSettings.ShowProductImagesOnShoppingCart;
            model.ShowSku = _catalogSettings.ShowProductSku;
            model.CheckoutAttributeInfo = _checkoutAttributeFormatter.FormatAttributes(_workContext.CurrentCustomer.CheckoutAttributes, _workContext.CurrentCustomer);
            bool minOrderSubtotalAmountOk = _orderProcessingService.ValidateMinOrderSubtotalAmount(cart);
            if (!minOrderSubtotalAmountOk)
            {
                decimal minOrderSubtotalAmount = _currencyService.ConvertFromPrimaryStoreCurrency(_orderSettings.MinOrderSubtotalAmount, _workContext.WorkingCurrency);
                model.MinOrderSubtotalWarning = string.Format(_localizationService.GetResource("Checkout.MinOrderSubtotalAmount"), _priceFormatter.FormatPrice(minOrderSubtotalAmount, true, false));
            }
            model.TermsOfServiceEnabled = _orderSettings.TermsOfServiceEnabled;
            model.ShowDiscountBox = _shoppingCartSettings.ShowDiscountBox;
            model.DiscountCouponCode = _workContext.CurrentCustomer.DiscountCouponCode;
            model.ShowGiftCardBox = _shoppingCartSettings.ShowGiftCardBox;

            //cart warnings
            var cartWarnings = _shoppingCartService.GetShoppingCartWarnings(cart, "", false);
            foreach (var warning in cartWarnings)
                model.Warnings.Add(warning);

            #endregion

            #region Checkout attributes

            var checkoutAttributes = _checkoutAttributeService.GetAllCheckoutAttributes(!cart.RequiresShipping());
            foreach (var attribute in checkoutAttributes)
            {
                var caModel = new ShoppingCartModel.CheckoutAttributeModel()
                {
                    Id = attribute.Id,
                    Name = attribute.GetLocalized(x => x.Name),
                    TextPrompt = attribute.TextPrompt,
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType
                };

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var caValues = _checkoutAttributeService.GetCheckoutAttributeValues(attribute.Id);
                    foreach (var caValue in caValues)
                    {
                        var pvaValueModel = new ShoppingCartModel.CheckoutAttributeValueModel()
                        {
                            Id = caValue.Id,
                            Name = caValue.GetLocalized(x => x.Name),
                            IsPreSelected = caValue.IsPreSelected
                        };
                        caModel.Values.Add(pvaValueModel);

                        //display price if allowed
                        if (_permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
                        {
                            decimal priceAdjustmentBase = _taxService.GetCheckoutAttributePrice(caValue);
                            decimal priceAdjustment = _currencyService.ConvertFromPrimaryStoreCurrency(priceAdjustmentBase, _workContext.WorkingCurrency);
                            if (priceAdjustmentBase > decimal.Zero)
                                pvaValueModel.PriceAdjustment = "+" + _priceFormatter.FormatPrice(priceAdjustment);
                            else if (priceAdjustmentBase < decimal.Zero)
                                pvaValueModel.PriceAdjustment = "-" + _priceFormatter.FormatPrice(-priceAdjustment);
                        }
                    }
                }



                //set already selected attributes
                string selectedCheckoutAttributes = _workContext.CurrentCustomer.CheckoutAttributes;
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.Checkboxes:
                        {
                            if (!String.IsNullOrEmpty(selectedCheckoutAttributes))
                            {
                                //clear default selection
                                foreach (var item in caModel.Values)
                                    item.IsPreSelected = false;

                                //select new values
                                var selectedCaValues = _checkoutAttributeParser.ParseCheckoutAttributeValues(selectedCheckoutAttributes);
                                foreach (var caValue in selectedCaValues)
                                    foreach (var item in caModel.Values)
                                        if (caValue.Id == item.Id)
                                            item.IsPreSelected = true;
                            }
                        }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            if (!String.IsNullOrEmpty(selectedCheckoutAttributes))
                            {
                                var enteredText = _checkoutAttributeParser.ParseValues(selectedCheckoutAttributes, attribute.Id);
                                if (enteredText.Count > 0)
                                    caModel.DefaultValue = enteredText[0];
                            }
                        }
                        break;
                    default:
                        break;
                }

                model.CheckoutAttributes.Add(caModel);
            }

            #endregion

            #region Estimate shipping

            model.EstimateShipping.Enabled = cart.Count > 0 && cart.RequiresShipping() && _shippingSettings.EstimateShippingEnabled;
            if (model.EstimateShipping.Enabled)
            {
                //countries
                int? defaultEstimateCountryId = (setEstimateShippingDefaultAddress && _workContext.CurrentCustomer.ShippingAddress != null) ? _workContext.CurrentCustomer.ShippingAddress.CountryId : model.EstimateShipping.CountryId;
                model.EstimateShipping.AvailableCountries.Add(new SelectListItem() { Text = _localizationService.GetResource("Address.SelectCountry"), Value = "0" });
                foreach (var c in _countryService.GetAllCountries())
                    model.EstimateShipping.AvailableCountries.Add(new SelectListItem()
                    {
                        Text = c.Name,
                        Value = c.Id.ToString(),
                        Selected = c.Id == defaultEstimateCountryId
                    });
                //states
                int? defaultEstimateStateId = (setEstimateShippingDefaultAddress && _workContext.CurrentCustomer.ShippingAddress != null) ? _workContext.CurrentCustomer.ShippingAddress.StateProvinceId : model.EstimateShipping.StateProvinceId;
                var states = defaultEstimateCountryId.HasValue ? _stateProvinceService.GetStateProvincesByCountryId(defaultEstimateCountryId.Value).ToList() : new List<StateProvince>();
                if (states.Count > 0)
                    foreach (var s in states)
                        model.EstimateShipping.AvailableStates.Add(new SelectListItem()
                        {
                            Text = s.Name,
                            Value = s.Id.ToString(),
                            Selected = s.Id == defaultEstimateStateId
                        });
                else
                    model.EstimateShipping.AvailableStates.Add(new SelectListItem() { Text = _localizationService.GetResource("Address.OtherNonUS"), Value = "0" });

                if (setEstimateShippingDefaultAddress && _workContext.CurrentCustomer.ShippingAddress != null)
                    model.EstimateShipping.ZipPostalCode = _workContext.CurrentCustomer.ShippingAddress.ZipPostalCode;
            }

            #endregion

            #region Cart items

            foreach (var sci in cart)
            {
                var cartItemModel = new ShoppingCartModel.ShoppingCartItemModel()
                {
                    Id = sci.Id,
                    Sku = sci.ProductVariant.Sku,
                    ProductId = sci.ProductVariant.ProductId,
                    ProductSeName = sci.ProductVariant.Product.GetSeName(),
                    Quantity = sci.Quantity,
                    MaxQuantity = Math.Min(_productService.GetVariantStockQuantity(sci.ProductVariant, sci.AttributesXml), sci.ProductVariant.OrderMaximumQuantity),
                    AttributeInfo = _productAttributeFormatter.FormatAttributes(sci.ProductVariant, sci.AttributesXml, "<span>{0}</span> :{1}", _workContext.CurrentCustomer, htmlEncode: false),
                    VariantId = sci.ProductVariant.Id
                };
                //Stock quantity


                //recurring info
                if (sci.ProductVariant.IsRecurring)
                    cartItemModel.RecurringInfo = string.Format(_localizationService.GetResource("ShoppingCart.RecurringPeriod"), sci.ProductVariant.RecurringCycleLength, sci.ProductVariant.RecurringCyclePeriod.GetLocalizedEnum(_localizationService, _workContext));

                //unit prices
                if (sci.ProductVariant.CallForPrice && sci.ProductVariant.Price == 0)
                {
                    cartItemModel.UnitPrice = _localizationService.GetResource("Products.Price.CallForPrice");
                }
                else
                {
                    decimal taxRate = decimal.Zero;
                    decimal shoppingCartUnitPriceWithoutDiscountBase = _taxService.GetProductPrice(sci.ProductVariant, _priceCalculationService.GetUnitPrice(sci, true), out taxRate);
                    decimal shoppingCartUnitPriceWithoutDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartUnitPriceWithoutDiscountBase, _workContext.WorkingCurrency);
                    cartItemModel.UnitPrice = _priceFormatter.FormatPrice(shoppingCartUnitPriceWithoutDiscount);
                    cartItemModel.UnitPriceValue = shoppingCartUnitPriceWithoutDiscount;
                }
                //subtotal, discount
                if (sci.ProductVariant.CallForPrice && sci.ProductVariant.Price == 0)
                {
                    cartItemModel.SubTotal = _localizationService.GetResource("Products.Price.CallForPrice");
                }
                else
                {
                    //sub total
                    decimal taxRate = decimal.Zero;
                    decimal shoppingCartItemSubTotalWithDiscountBase = _taxService.GetProductPrice(sci.ProductVariant, _priceCalculationService.GetSubTotal(sci, true), out taxRate);
                    decimal shoppingCartItemSubTotalWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartItemSubTotalWithDiscountBase, _workContext.WorkingCurrency);
                    cartItemModel.SubTotal = _priceFormatter.FormatPrice(shoppingCartItemSubTotalWithDiscount);

                    //display an applied discount amount
                    decimal shoppingCartItemSubTotalWithoutDiscountBase = _taxService.GetProductPrice(sci.ProductVariant, _priceCalculationService.GetSubTotal(sci, false), out taxRate);
                    decimal shoppingCartItemSubTotalWithoutDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartItemSubTotalWithoutDiscountBase, _workContext.WorkingCurrency);
                    //cartItemModel.SubTotal = _priceFormatter.FormatPrice(shoppingCartItemSubTotalWithoutDiscount);
                    decimal shoppingCartItemDiscountBase = shoppingCartItemSubTotalWithoutDiscountBase - shoppingCartItemSubTotalWithDiscountBase;
                    if (shoppingCartItemDiscountBase > decimal.Zero)
                    {
                        cartItemModel.DiscountAmount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartItemDiscountBase, _workContext.WorkingCurrency);
                        cartItemModel.Discount = _priceFormatter.FormatPrice(cartItemModel.DiscountAmount);
                    }
                }

                //product name
                if (!String.IsNullOrEmpty(sci.ProductVariant.GetLocalized(x => x.Name)))
                    cartItemModel.ProductName = sci.ProductVariant.GetLocalized(x => x.Name);
                else
                    cartItemModel.ProductName = sci.ProductVariant.Product.GetLocalized(x => x.Name);

                //Manufacturer name
                cartItemModel.Manufacturer = sci.ProductVariant.Product.ManufacturerName();

                //picture
                if (_shoppingCartSettings.ShowProductImagesOnShoppingCart)
                {
                    var picture = sci.ProductVariant.GetDefaultProductVariantPicture(_pictureService);
                    cartItemModel.Picture = new PictureModel()
                    {
                        ImageUrl = _pictureService.GetPictureUrl(picture, _mediaSetting.CartThumbPictureSize, true),
                        Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat"), cartItemModel.ProductName),
                        AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat"), cartItemModel.ProductName),

                    };
                }

                if (loadShippingNotification)
                {
                    var specification = sci.ProductVariant.Product.ProductSpecificationAttributes.FirstOrDefault(x => x.SpecificationAttributeOption.SpecificationAttribute.Name == "Kargo");
                    if (specification != null)
                        cartItemModel.ShippingNotification = specification.SpecificationAttributeOption.GetLocalized(x => x.Name);
                }

                //item warnings
                var itemWarnings = _shoppingCartService.GetShoppingCartItemWarnings(_workContext.CurrentCustomer,
                            sci.ShoppingCartType,
                            sci.ProductVariant,
                            sci.AttributesXml,
                            sci.CustomerEnteredPrice,
                            sci.Quantity, false);
                foreach (var warning in itemWarnings)
                    cartItemModel.Warnings.Add(warning);
                model.Items.Add(cartItemModel);
            }

            decimal subtotalBase = decimal.Zero;
            decimal orderSubTotalDiscountAmountBase = decimal.Zero;
            Discount orderSubTotalAppliedDiscount = null;
            decimal subTotalWithoutDiscountBase = decimal.Zero;
            decimal subTotalWithDiscountBase = decimal.Zero;
            _orderTotalCalculationService.GetShoppingCartSubTotal(cart,true,
                out orderSubTotalDiscountAmountBase, out orderSubTotalAppliedDiscount,
                out subTotalWithoutDiscountBase, out subTotalWithDiscountBase);
            subtotalBase = subTotalWithoutDiscountBase;
            decimal subtotal = _currencyService.ConvertFromPrimaryStoreCurrency(subtotalBase, _workContext.WorkingCurrency);
            model.SubTotal = _priceFormatter.FormatPrice(subtotal);

            #endregion

            #region Button payment methods

            var boundPaymentMethods = _paymentService
                .LoadActivePaymentMethods()
                .Where(pm => pm.PaymentMethodType == PaymentMethodType.Button)
                .ToList();
            foreach (var pm in boundPaymentMethods)
            {
                if (cart.IsRecurring() && pm.RecurringPaymentType == RecurringPaymentType.NotSupported)
                    continue;

                string actionName;
                string controllerName;
                RouteValueDictionary routeValues;
                pm.GetPaymentInfoRoute(out actionName, out controllerName, out routeValues);

                model.ButtonPaymentMethodActionNames.Add(actionName);
                model.ButtonPaymentMethodControllerNames.Add(controllerName);
                model.ButtonPaymentMethodRouteValues.Add(routeValues);
            }

            #endregion

            return model;
        }


        [NonAction]
        private WishlistModel PrepareWishlistModel(WishlistModel model, IList<Nop.Core.Domain.Orders.ShoppingCartItem> cart, bool isEditable)
        {
            if (cart == null)
                throw new ArgumentNullException("cart");

            if (model == null)
                throw new ArgumentNullException("model");

            model.EmailWishlistEnabled = _shoppingCartSettings.EmailWishlistEnabled;
            model.IsEditable = isEditable;

            if (cart.Count == 0)
                return model;

            #region Simple properties

            var customer = cart.FirstOrDefault().Customer;
            model.CustomerGuid = customer.CustomerGuid;
            model.CustomerFullname = customer.GetFullName();
            model.ShowProductImages = _shoppingCartSettings.ShowProductImagesOnShoppingCart;
            model.ShowSku = _catalogSettings.ShowProductSku;



            //cart warnings
            var cartWarnings = _shoppingCartService.GetShoppingCartWarnings(cart, "", false);
            foreach (var warning in cartWarnings)
                model.Warnings.Add(warning);

            #endregion

            #region Cart items

            foreach (var sci in cart)
            {
                var cartItemModel = new WishlistModel.ShoppingCartItemModel()
                {
                    Id = sci.Id,
                    Sku = sci.ProductVariant.Sku,
                    ProductId = sci.ProductVariant.ProductId,
                    ProductSeName = sci.ProductVariant.Product.GetSeName(),
                    Quantity = sci.Quantity,
                    AttributeInfo = _productAttributeFormatter.FormatAttributes(sci.ProductVariant, sci.AttributesXml),
                };

                //recurring info
                if (sci.ProductVariant.IsRecurring)
                    cartItemModel.RecurringInfo = string.Format(_localizationService.GetResource("ShoppingCart.RecurringPeriod"), sci.ProductVariant.RecurringCycleLength, sci.ProductVariant.RecurringCyclePeriod.GetLocalizedEnum(_localizationService, _workContext));

                //VariantId
                cartItemModel.VariantId = sci.ProductVariantId;
                cartItemModel.HasStock = sci.ProductVariant.StockQuantity > 0;
                //unit prices)
                cartItemModel.HasStockText = sci.ProductVariant.FormatStockMessage(_localizationService);
                if (sci.ProductVariant.CallForPrice)
                    cartItemModel.CallForPrice = !sci.ProductVariant.CallforPriceRequested(_workContext.CurrentCustomer);
                if (cartItemModel.CallForPrice)
                {
                    cartItemModel.UnitPrice = _localizationService.GetResource("Products.CallForPrice");
                }
                else
                {
                    decimal taxRate = decimal.Zero;
                    var a = sci.ProductVariant.Price;
                    //decimal shoppingCartUnitPriceWithDiscountBase = _taxService.GetProductPrice(sci.ProductVariant, _priceCalculationService.GetUnitPrice(sci, true), out taxRate);
                    //decimal shoppingCartUnitPriceWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartUnitPriceWithDiscountBase, _workContext.WorkingCurrency);
                    // decimal taxRate = decimal.Zero;
                    decimal oldPriceBase = _taxService.GetProductPrice(sci.ProductVariant, sci.ProductVariant.OldPrice, out taxRate);
                    decimal finalPriceWithoutDiscountBase = _taxService.GetProductPrice(sci.ProductVariant, _priceCalculationService.GetFinalPrice(sci.ProductVariant, false), out taxRate);
                    decimal finalPriceWithDiscountBase = _taxService.GetProductPrice(sci.ProductVariant, _priceCalculationService.GetFinalPrice(sci.ProductVariant, true), out taxRate);

                    //decimal oldPrice = _currencyService.ConvertFromPrimaryStoreCurrency(oldPriceBase, _workContext.WorkingCurrency);
                    decimal finalPriceWithoutDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithoutDiscountBase, _workContext.WorkingCurrency);
                    decimal finalPriceWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithDiscountBase, _workContext.WorkingCurrency);

                    //if (finalPriceWithoutDiscountBase != oldPriceBase && oldPriceBase > decimal.Zero)
                    //    model.ProductVariantPrice.OldPrice = _priceFormatter.FormatPrice(oldPrice, false, false);

                    //ccc
                    //cartItemModel.UnitPrice = _priceFormatter.FormatPrice(shoppingCartUnitPriceWithDiscount);
                    //cartItemModel.UnitPriceDecimal = shoppingCartUnitPriceWithDiscount;
                    cartItemModel.UnitPrice = _priceFormatter.FormatPrice(finalPriceWithDiscount);
                    cartItemModel.UnitPriceDecimal = finalPriceWithDiscount;

                    //if (shoppingCartUnitPriceWithDiscount < sci.ProductVariant.Price)
                    //{
                    if (finalPriceWithDiscount < finalPriceWithoutDiscount)
                    {
                        cartItemModel.OldPriceDecimal = sci.ProductVariant.Price;
                        cartItemModel.OldPrice = _priceFormatter.FormatPrice(finalPriceWithoutDiscount);
                    }
                    else
                    {
                        cartItemModel.OldPriceDecimal = 0;
                        cartItemModel.OldPrice = _priceFormatter.FormatPrice(0);
                    }


                }
                if (sci.ProductVariant.CallForPrice)
                    cartItemModel.CallForPrice = !sci.ProductVariant.CallforPriceRequested(_workContext.CurrentCustomer);
                else
                    cartItemModel.CallForPrice = sci.ProductVariant.CallForPrice;

                //subtotal, discount
                if (cartItemModel.CallForPrice)
                {
                    cartItemModel.SubTotal = _localizationService.GetResource("Products.CallForPrice");
                }
                else
                {
                    //sub total
                    decimal taxRate = decimal.Zero;
                    decimal shoppingCartItemSubTotalWithDiscountBase = _taxService.GetProductPrice(sci.ProductVariant, _priceCalculationService.GetSubTotal(sci, true), out taxRate);
                    decimal shoppingCartItemSubTotalWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartItemSubTotalWithDiscountBase, _workContext.WorkingCurrency);
                    cartItemModel.SubTotal = _priceFormatter.FormatPrice(shoppingCartItemSubTotalWithDiscount);

                    //display an applied discount amount
                    decimal shoppingCartItemSubTotalWithoutDiscountBase = _taxService.GetProductPrice(sci.ProductVariant, _priceCalculationService.GetSubTotal(sci, false), out taxRate);
                    decimal shoppingCartItemDiscountBase = shoppingCartItemSubTotalWithoutDiscountBase - shoppingCartItemSubTotalWithDiscountBase;
                    if (shoppingCartItemDiscountBase > decimal.Zero)
                    {
                        decimal shoppingCartItemDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartItemDiscountBase, _workContext.WorkingCurrency);
                        cartItemModel.Discount = _priceFormatter.FormatPrice(shoppingCartItemDiscount);
                    }
                }

                //Created Date
                cartItemModel.Created = sci.CreatedOnUtc.ToShortDateString();


                //Manufacturer name
                cartItemModel.Manufacturer = sci.ProductVariant.Product.ManufacturerName();

                //Customer Comment
                cartItemModel.CustomerComment = sci.CustomerComment;


                //product name
                if (!String.IsNullOrEmpty(sci.ProductVariant.GetLocalized(x => x.Name)))
                    cartItemModel.ProductName = sci.ProductVariant.GetLocalized(x => x.Name); //string.Format("{0} ({1})", sci.ProductVariant.Product.GetLocalized(x => x.Name), sci.ProductVariant.GetLocalized(x => x.Name));
                else
                    cartItemModel.ProductName = sci.ProductVariant.Product.GetLocalized(x => x.Name);

                //picture
                if (_shoppingCartSettings.ShowProductImagesOnShoppingCart)
                {
                    var picture = sci.ProductVariant.GetDefaultProductVariantPicture(_pictureService);
                    cartItemModel.Picture = new PictureModel()
                    {
                        LargeSizeImageUrl = _pictureService.GetPictureUrl(picture, _mediaSetting.CategoryThumbPictureSize, true),
                        ImageUrl = _pictureService.GetPictureUrl(picture, _mediaSetting.ProductVariantPictureSize, true),
                        Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat"), cartItemModel.ProductName),
                        AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat"), cartItemModel.ProductName),

                    };
                }

                //item warnings
                var itemWarnings = _shoppingCartService.GetShoppingCartItemWarnings(_workContext.CurrentCustomer,
                            sci.ShoppingCartType,
                            sci.ProductVariant,
                            sci.AttributesXml,
                            sci.CustomerEnteredPrice,
                            sci.Quantity, false);
                foreach (var warning in itemWarnings)
                    cartItemModel.Warnings.Add(warning);

                model.Items.Add(cartItemModel);
            }

            #endregion

            return model;
        }

        #endregion

        #region Shopping cart

        //public ActionResult AddProductToCart(int productId)
        //{
        //    var product = _productService.GetProductById(productId);
        //    if (product == null)
        //        return RedirectToAction("Index", "Home");

        //    int productVariantId = 0;
        //    if (_shoppingCartService.DirectAddToCartAllowed(productId, out productVariantId))
        //    {
        //        var productVariant = _productService.GetProductVariantById(productVariantId);
        //        var addToCartWarnings = _shoppingCartService.AddToCart(_workContext.CurrentCustomer,
        //            productVariant, ShoppingCartType.ShoppingCart,
        //            string.Empty, decimal.Zero, 1, true);
        //        if (addToCartWarnings.Count == 0)
        //            return RedirectToRoute("ShoppingCart");
        //        else
        //            return RedirectToRoute("Product", new { productId = product.Id, SeName = product.GetSeName() });
        //    }
        //    else
        //        return RedirectToRoute("Product", new { productId = product.Id, SeName = product.GetSeName() });
        //}

        public ActionResult GetProduct(string id)
        {

            Product product = _productService.GetProductById(Convert.ToInt32(id));
            return View("WishListProduct", product);
        }

        [NopHttpsRequirement(SslRequirement.Yes)]
        public ActionResult Cart()
        {
            var cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();
            var model = PrepareShoppingCartModel(new ShoppingCartModel(), cart, true);
            if (model.Items.Count == 0)
            {
                var messageModel = new MessageModel();
                messageModel.MessageList.Add(_localizationService.GetResource("ShoppingCart.CartIsEmpty"));
                messageModel.ActionText = _localizationService.GetResource("ShoppingCart.CartIsEmpty.Continue");
                messageModel.ActionUrl = ControllerContext.HttpContext.Request.ApplicationPath;
                ViewBag.MessageModel = messageModel;
            }
            return View(model);
        }

        [NonAction]
        private bool IsCurrentUserRegistered()
        {
            return _workContext.CurrentCustomer.IsRegistered();
        }

        [HttpPost]
        public JsonResult AddProductToWishlist(AddToCartModel model)
        {
            var url = Url.Action("Wishlist", "ShoppingCart");
            int productVariantId = model.VariantId;
            var hasVariantWishList = _workContext.CurrentCustomer.ShoppingCartItems.Where(x => x.ShoppingCartType == ShoppingCartType.Wishlist && x.ProductVariantId == productVariantId);
            if (hasVariantWishList.Count() != 0)
                return Json(new { Message = _localizationService.GetResource("WishList.Product.AllReadyExist"), click = _localizationService.GetResource("Wishlist.Goto.Wishlist"), href = url, success = false }, JsonRequestBehavior.AllowGet);

            var productVariant = _productService.GetProductVariantById(productVariantId);
            if (productVariant == null)
                return Json(new { Message = _localizationService.GetResource("WishList.Product.NotFound"), click = _localizationService.GetResource("Wishlist.Goto.Wishlist"), href = url, success = false }, JsonRequestBehavior.AllowGet);


            #region Product attributes
            string selectedAttributes = string.Empty;




            var productVariantAttributes = _productAttributeService.GetProductVariantAttributesByProductVariantId(productVariant.Id);
            foreach (var attribute in productVariantAttributes)
            {
                var attr = model.Attributes.FirstOrDefault(x => x.ProductAttributeId == attribute.ProductAttributeId);
                if (attr == null) continue;
                selectedAttributes = _productAttributeParser.AddProductAttribute(selectedAttributes,
                                        attribute, attr.ProductVariantAttributeValueId);
            }
            #endregion
            


            var addToCartWarnings = _shoppingCartService.AddToCart(_workContext.CurrentCustomer,
                productVariant, ShoppingCartType.Wishlist,
                selectedAttributes, decimal.Zero, 1, false);
            if (_workContext.CurrentCustomer.IsGuest())
            {
                return Json(new { href = Url.RouteUrl("Login") + "?ReturnUrl=" + url, success = false, isGuest = true }, JsonRequestBehavior.AllowGet);
            }

            //int productId = Convert.ToInt32(id);
            //var product = _productService.GetProductById(productId);
            //if (product == null)
            //    return RedirectToAction("Index", "Home");

            if (addToCartWarnings.Count > 0)
            {
                return Json(new { Message = addToCartWarnings[0], click = _localizationService.GetResource("Wishlist.Goto.Wishlist"), href = url, success = false }, JsonRequestBehavior.AllowGet);
            }

            //if (addToCartWarnings.Count == 0)
            //    return RedirectToRoute("ShoppingCart");
            //else
            //    return RedirectToRoute("Product", new { productId = product.Id, SeName = product.GetSeName() });
            return Json(new { Message = _localizationService.GetResource("Wishlist.Product.Added"), click = _localizationService.GetResource("Wishlist.Goto.Wishlist"), href = url, success = true }, JsonRequestBehavior.AllowGet);
            //else
            //    return RedirectToRoute("Product", new { productId = product.Id, SeName = product.GetSeName() });
        }

        //[NopHttpsRequirement(SslRequirement.Yes)]
        //public ActionResult WishList()
        //{
        //    var cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist).ToList();
        //    var model = PrepareWishlistModel(new WishlistModel(), cart, true);
        //    if (model.Items.Count == 0)
        //    {
        //        var messageModel = new MessageModel();
        //        messageModel.MessageList.Add(_localizationService.GetResource("ShoppingCart.CartIsEmpty"));
        //        messageModel.ActionText = _localizationService.GetResource("ShoppingCart.CartIsEmpty.Continue");
        //        messageModel.ActionUrl = ControllerContext.HttpContext.Request.ApplicationPath;
        //        ViewBag.MessageModel = messageModel;
        //    }
        //    return View(model);
        //}

        //AF for checkout order summary part
        public ActionResult CheckoutOrderSummary()
        {
            var cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();
            var model = PrepareShoppingCartModel(new ShoppingCartModel(), cart, true, true, true);
            return View(model);
        }

        //AF for checkout order summary part
        public ActionResult CheckoutConfirmOrderSummary()
        {
            var cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();
            var model = PrepareShoppingCartModel(new ShoppingCartModel(), cart, true, true, true);

            return View(model);
        }

        [ChildActionOnly]
        public ActionResult OrderSummary(bool isEditable)
        {
            var cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();
            var model = PrepareShoppingCartModel(new ShoppingCartModel(), cart, isEditable);
            return PartialView(model);
        }

        [ValidateInput(false)]
        [HttpPost, ActionName("Cart")]
        [FormValueRequired("updatecart")]
        public ActionResult UpdateCart(FormCollection form)
        {
            var cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();

            var allIdsToRemove = form["removefromcart"] != null ? form["removefromcart"].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToList() : new List<int>();
            foreach (var sci in cart)
            {
                bool remove = allIdsToRemove.Contains(sci.Id);
                if (remove)
                    _shoppingCartService.DeleteShoppingCartItem(sci, true);
                else
                {
                    int newQuantity = sci.Quantity;
                    foreach (string formKey in form.AllKeys)
                        if (formKey.Equals(string.Format("itemquantity{0}", sci.Id), StringComparison.InvariantCultureIgnoreCase))
                        {
                            int.TryParse(form[formKey], out newQuantity);
                            break;
                        }
                    _shoppingCartService.UpdateShoppingCartItem(_workContext.CurrentCustomer,
                        sci.Id, newQuantity, true);
                }
            }

            //updated cart
            cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();
            var model = PrepareShoppingCartModel(new ShoppingCartModel(), cart, true);
            return View(model);
        }



        //AF
        public ActionResult DeleteCartItem(int id)
        {

            var cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();
            var item = cart.FirstOrDefault(x => x.Id == id);
            if (item != null)
                _shoppingCartService.DeleteShoppingCartItem(item, true);

            return RedirectToAction("Cart");

        }



        //AF
        public JsonResult DeleteWishListCartItem(string id)
        {
            int _id = Convert.ToInt32(id);
            var cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist).ToList();
            var item = cart.FirstOrDefault(x => x.Id == _id);
            if (item != null)
                _shoppingCartService.DeleteShoppingCartItem(item, true);

            Customer customer = _workContext.CurrentCustomer;



            var cartItems = customer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist).ToList();
            var model = PrepareWishlistModel(new WishlistModel(), cartItems, false).Items;


            string HTMLOutput = Utilities.RenderViewToString(this, @"~/Views\ShoppingCart\WishListProducts.cshtml", model);

            //string htmlOutput = PartialView("WishListProducts", model).View.ToString();

            return Json(new { output = HTMLOutput, count = cartItems.Count() }, JsonRequestBehavior.AllowGet);
            

        }

        public JsonResult AddToCartSelected(string id)
        {
            var cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist).ToList();
            var items = cart.Where(x => id.Contains("-" + x.Id.ToString() + "-"));
            var warnings = new List<string>();
            if (items.Count() == 0)
            {
                warnings.Add(_localizationService.GetResource("Wishlist.NoSelectedItem", _workContext.WorkingLanguage.Id));
                return Json(new { success = false, message = warnings.Distinct() },
                        JsonRequestBehavior.AllowGet);
            }


            //if(callForPriceCount.Count>0 || hasStock.Count>0 )
            //{
            //    if(callForPriceCount.Count>0)
            //        warnings.Add("Teklif Al Ürünler Bulunmaktadır.");
            //    if(hasStock.Count>0)
            //        warnings.Add("Stok Tükenmiş Ürünler Bulunmaktadır.");

            //    return Json(new { success = false, message = warnings[0]+"<br/>"+warnings[1], },
            //            JsonRequestBehavior.AllowGet);
            //}

            //foreach (var item in items)
            //{
            //    warnings.AddRange(AddtoCartFromWishlist(item));
            //    if (warnings.Count == 0)
            //        addeddItemCount++;
            //}
            warnings = AddtoCartFromWishlist(items).ToList();
            //if (addeddItemCount == 0)
            //{
            //    return Json(new { success = false, message = warnings.Distinct(XmlHelpe) }, JsonRequestBehavior.AllowGet);
            //}
            if (warnings.Count <= 0)
                return Json(new { html = MiniShoppingCartHtml(), success = true, href = Url.Action("Cart", "ShoppingCart") }, JsonRequestBehavior.AllowGet);

            return Json(new { success = false, message = warnings.Distinct() }, JsonRequestBehavior.AllowGet);

        }



        //AF
        public JsonResult DeleteWishListCartItemCollection(string id)
        {
            var cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist).ToList();
            var items = cart.Where(x => id.Contains("-" + x.Id.ToString() + "-"));
            foreach (var item in items)
                _shoppingCartService.DeleteShoppingCartItem(item, true);

            Customer customer = _workContext.CurrentCustomer;
            var cartItems = customer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist).ToList();
            var model = PrepareWishlistModel(new WishlistModel(), cartItems, false).Items;


            string HTMLOutput = Utilities.RenderViewToString(this, @"~/Views\ShoppingCart\WishListProducts.cshtml", model);

            //string htmlOutput = PartialView("WishListProducts", model).View.ToString();

            return Json(HTMLOutput, JsonRequestBehavior.AllowGet);

        }


        public string SortWishListby(string id)
        {
            Customer customer = _workContext.CurrentCustomer;

            var cartItems = customer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist).ToList();
            var model = PrepareWishlistModel(new WishlistModel(), cartItems, false).Items;

            switch (id)
            {
                case "15":
                    model = model.OrderByDescending(x => Convert.ToDateTime(x.Created)).ToList();
                    break;
                case "10":
                    model = model.OrderBy(x => x.UnitPrice).ToList();
                    break;
                case "20":
                    model = model.OrderByDescending(x => x.UnitPrice).ToList();
                    break;
                default:
                    break;
            }

            string HTMLOutput = Utilities.RenderViewToString(this, @"~/Views\ShoppingCart\WishListProducts.cshtml", model);

            //string htmlOutput = PartialView("WishListProducts", model).View.ToString();

            //return Json(HTMLOutput, JsonRequestBehavior.AllowGet);

            return HTMLOutput;

        }

        //AF
        public ActionResult UpdateCartItem(int id, int qty)
        {
            var cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();
            var item = cart.FirstOrDefault(x => x.Id == id);
            if (item == null) return RedirectToAction("Cart");
            item.Quantity = qty;
            //TODO: mustafa display warnings.
            var warnings = _shoppingCartService.UpdateShoppingCartItem(_workContext.CurrentCustomer, id, qty, true, item.CustomerComment);
            return RedirectToAction("Cart");
        }


        [ValidateInput(false)]
        [HttpPost, ActionName("Cart")]
        [FormValueRequired("continueshopping")]
        public ActionResult ContinueShopping()
        {
            string returnUrl = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.LastContinueShoppingPage);
            if (!String.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [ValidateInput(false)]
        [HttpPost, ActionName("Cart")]
        [FormValueRequired("startcheckout")]
        public ActionResult StartCheckout(FormCollection form)
        {
            var cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();

            //apply attributes
            string selectedAttributes = "";

            var checkoutAttributes = _checkoutAttributeService.GetAllCheckoutAttributes(!cart.RequiresShipping());
            foreach (var attribute in checkoutAttributes)
            {
                string controlId = string.Format("checkout_attribute_{0}", attribute.Id);
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                        {
                            var ddlAttributes = form[controlId];
                            if (!String.IsNullOrEmpty(ddlAttributes))
                            {
                                int selectedAttributeId = int.Parse(ddlAttributes);
                                if (selectedAttributeId > 0)
                                    selectedAttributes = _checkoutAttributeParser.AddCheckoutAttribute(selectedAttributes,
                                        attribute, selectedAttributeId.ToString());
                            }
                        }
                        break;
                    case AttributeControlType.RadioList:
                        {
                            var rblAttributes = form[controlId];
                            if (!String.IsNullOrEmpty(rblAttributes))
                            {
                                int selectedAttributeId = int.Parse(rblAttributes);
                                if (selectedAttributeId > 0)
                                    selectedAttributes = _checkoutAttributeParser.AddCheckoutAttribute(selectedAttributes,
                                        attribute, selectedAttributeId.ToString());
                            }
                        }
                        break;
                    case AttributeControlType.Checkboxes:
                        {
                            var cblAttributes = form[controlId];
                            if (!String.IsNullOrEmpty(cblAttributes))
                            {
                                foreach (var item in cblAttributes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    int selectedAttributeId = int.Parse(item);
                                    if (selectedAttributeId > 0)
                                        selectedAttributes = _checkoutAttributeParser.AddCheckoutAttribute(selectedAttributes,
                                            attribute, selectedAttributeId.ToString());
                                }
                            }
                        }
                        break;
                    case AttributeControlType.TextBox:
                        {
                            var txtAttribute = form[controlId];
                            if (!String.IsNullOrEmpty(txtAttribute))
                            {
                                string enteredText = txtAttribute.Trim();
                                selectedAttributes = _checkoutAttributeParser.AddCheckoutAttribute(selectedAttributes,
                                    attribute, enteredText);
                            }
                        }
                        break;
                    case AttributeControlType.MultilineTextbox:
                        {
                            var txtAttribute = form[controlId];
                            if (!String.IsNullOrEmpty(txtAttribute))
                            {
                                string enteredText = txtAttribute.Trim();
                                selectedAttributes = _checkoutAttributeParser.AddCheckoutAttribute(selectedAttributes,
                                    attribute, enteredText);
                            }
                        }
                        break;
                    case AttributeControlType.Datepicker:
                        {
                            var date = form[controlId + "_day"];
                            var month = form[controlId + "_month"];
                            var year = form[controlId + "_year"];
                            DateTime? selectedDate = null;
                            try
                            {
                                selectedDate = new DateTime(Int32.Parse(year), Int32.Parse(month), Int32.Parse(date));
                            }
                            catch { }
                            if (selectedDate.HasValue)
                            {
                                selectedAttributes = _checkoutAttributeParser.AddCheckoutAttribute(selectedAttributes,
                                    attribute, selectedDate.Value.ToString("D"));
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            _workContext.CurrentCustomer.CheckoutAttributes = selectedAttributes;
            _customerService.UpdateCustomer(_workContext.CurrentCustomer);

            if (_workContext.CurrentCustomer.IsGuest())
            {
                if (_orderSettings.AnonymousCheckoutAllowed)
                {
                    return RedirectToRoute("LoginCheckoutAsGuest");
                }
                else
                {
                    return RedirectToRoute("Login");
                }
            }
            else
            {
                return RedirectToRoute("Checkout");
            }
        }

        [ValidateInput(false)]
        [HttpPost, ActionName("Cart")]
        [FormValueRequired("applydiscountcouponcode")]
        public ActionResult ApplyDiscountCoupon(string discountcouponcode)
        {
            var cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();
            var model = new ShoppingCartModel();

            if (!String.IsNullOrWhiteSpace(discountcouponcode))
            {
                var discounts = _discountService.GetAllDiscounts(null);
                var discount = discounts.Where(d => !String.IsNullOrEmpty(d.CouponCode) && _discountService.IsDiscountCouponCodeValid(d, discountcouponcode)).FirstOrDefault();
                string message = "";
                bool isDiscountValid = discount != null &&
                    discount.RequiresCouponCode &&
                    _discountService.IsDiscountValid(discount, _workContext.CurrentCustomer, discountcouponcode, out message);
                if (discount != null)
                {
                    if (isDiscountValid)
                    {
                        _workContext.CurrentCustomer.DiscountCouponCode = discountcouponcode;
                        _customerService.UpdateCustomer(_workContext.CurrentCustomer);
                        //model.DiscountWarning = _localizationService.GetResource("ShoppingCart.DiscountCouponCode.Applied");
                    }
                    else
                        model.DiscountWarning = string.IsNullOrWhiteSpace(message)
                                                    ? _localizationService.GetResource(
                                                        "ShoppingCart.DiscountCouponCode.RequirementsFailed")
                                                    : message;
                }
                else
                {
                    model.DiscountWarning = _localizationService.GetResource("ShoppingCart.DiscountCouponCode.InvalidCode");
                }
            }
            else
            {
                _workContext.CurrentCustomer.DiscountCouponCode = "";
                _customerService.UpdateCustomer(_workContext.CurrentCustomer);
            }

            model = PrepareShoppingCartModel(model, cart, true);
            return View(model);
        }

        [ValidateInput(false)]
        [HttpPost, ActionName("Cart")]
        [FormValueRequired("applygiftcardcouponcode")]
        public ActionResult ApplyGiftCard(string giftcardcouponcode)
        {
            var cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();
            var model = new ShoppingCartModel();

            if (!cart.IsRecurring())
            {
                if (!String.IsNullOrWhiteSpace(giftcardcouponcode))
                {
                    var giftCard = _giftCardService.GetAllGiftCards(null, null, null, null, giftcardcouponcode).FirstOrDefault();
                    bool isGiftCardValid = giftCard != null && giftCard.IsGiftCardValid();
                    if (isGiftCardValid)
                    {
                        _workContext.CurrentCustomer.ApplyGiftCardCouponCode(giftcardcouponcode);
                        _customerService.UpdateCustomer(_workContext.CurrentCustomer);
                    }
                    else
                        model.GiftCardWarning = _localizationService.GetResource("ShoppingCart.GiftCardCouponCode.WrongGiftCard");
                }
            }
            else
                model.GiftCardWarning = _localizationService.GetResource("ShoppingCart.GiftCardCouponCode.DontWorkWithAutoshipProducts");

            model = PrepareShoppingCartModel(model, cart, true);
            return View(model);
        }

        [ValidateInput(false)]
        [HttpPost, ActionName("Cart")]
        [FormValueRequired("estimateshipping")]
        public ActionResult GetEstimateShipping(EstimateShippingModel shippingModel)
        {
            var cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();
            var model = new ShoppingCartModel();
            model.EstimateShipping.CountryId = shippingModel.CountryId;
            model.EstimateShipping.StateProvinceId = shippingModel.StateProvinceId;
            model.EstimateShipping.ZipPostalCode = shippingModel.ZipPostalCode;
            model = PrepareShoppingCartModel(model, cart, true, false);

            if (cart.RequiresShipping())
            {
                var address = new Address()
                {
                    CountryId = shippingModel.CountryId,
                    Country = shippingModel.CountryId.HasValue ? _countryService.GetCountryById(shippingModel.CountryId.Value) : null,
                    StateProvinceId = shippingModel.StateProvinceId,
                    StateProvince = shippingModel.StateProvinceId.HasValue ? _stateProvinceService.GetStateProvinceById(shippingModel.StateProvinceId.Value) : null,
                    ZipPostalCode = shippingModel.ZipPostalCode,
                };
                GetShippingOptionResponse getShippingOptionResponse = _shippingService.GetShippingOptions(cart, address);
                if (!getShippingOptionResponse.Success)
                {
                    foreach (var error in getShippingOptionResponse.Errors)
                        model.EstimateShipping.Warnings.Add(error);
                }
                else
                {
                    if (getShippingOptionResponse.ShippingOptions.Count > 0)
                    {
                        foreach (var shippingOption in getShippingOptionResponse.ShippingOptions)
                        {
                            var soModel = new EstimateShippingModel.ShippingOptionModel()
                            {
                                Name = shippingOption.Name,
                                Description = shippingOption.Description,

                            };
                            //calculate discounted and taxed rate
                            Discount appliedDiscount = null;
                            decimal shippingTotalWithoutDiscount = shippingOption.Rate;
                            decimal discountAmount = _orderTotalCalculationService.GetShippingDiscount(_workContext.CurrentCustomer,
                                shippingTotalWithoutDiscount, out appliedDiscount);
                            decimal shippingTotalWithDiscount = shippingTotalWithoutDiscount - discountAmount;
                            if (shippingTotalWithDiscount < decimal.Zero)
                                shippingTotalWithDiscount = decimal.Zero;
                            shippingTotalWithDiscount = Math.Round(shippingTotalWithDiscount, 2);

                            decimal rateBase = _taxService.GetShippingPrice(shippingTotalWithDiscount, _workContext.CurrentCustomer);
                            decimal rate = _currencyService.ConvertFromPrimaryStoreCurrency(rateBase, _workContext.WorkingCurrency);
                            soModel.Price = _priceFormatter.FormatShippingPrice(rate, true);
                            model.EstimateShipping.ShippingOptions.Add(soModel);
                        }
                    }
                    else
                    {
                        model.EstimateShipping.Warnings.Add(_localizationService.GetResource("Checkout.ShippingIsNotAllowed"));
                    }
                }
            }

            return View(model);
        }

        [ChildActionOnly]
        public ActionResult OrderTotals(bool isEditable)
        {
            var cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();
            var model = new OrderTotalsModel();
            model.IsEditable = isEditable;

            if (cart.Count > 0)
            {
                //payment method (if already selected)
                string paymentMethodSystemName = _workContext.CurrentCustomer != null ? _workContext.CurrentCustomer.SelectedPaymentMethodSystemName : null;


                //subtotal
                decimal subtotalBase = decimal.Zero;
                decimal orderSubTotalDiscountAmountBase = decimal.Zero;
                Discount orderSubTotalAppliedDiscount = null;
                decimal subTotalWithoutDiscountBase = decimal.Zero;
                decimal subTotalWithDiscountBase = decimal.Zero;
                _orderTotalCalculationService.GetShoppingCartSubTotal(cart, true,
                    out orderSubTotalDiscountAmountBase, out orderSubTotalAppliedDiscount,
                    out subTotalWithoutDiscountBase, out subTotalWithDiscountBase);
                subtotalBase = subTotalWithoutDiscountBase;
                decimal subtotal = _currencyService.ConvertFromPrimaryStoreCurrency(subtotalBase, _workContext.WorkingCurrency);
                model.SubTotal = _priceFormatter.FormatPrice(subtotal);
                if (orderSubTotalDiscountAmountBase > decimal.Zero)
                {
                    decimal orderSubTotalDiscountAmount = _currencyService.ConvertFromPrimaryStoreCurrency(orderSubTotalDiscountAmountBase, _workContext.WorkingCurrency);
                    model.SubTotalDiscount = _priceFormatter.FormatPrice(-orderSubTotalDiscountAmount);
                    model.AllowRemovingSubTotalDiscount = orderSubTotalAppliedDiscount != null &&
                        orderSubTotalAppliedDiscount.RequiresCouponCode &&
                        !String.IsNullOrEmpty(orderSubTotalAppliedDiscount.CouponCode) &&
                        model.IsEditable;
                }


                //shipping info
                model.RequiresShipping = cart.RequiresShipping();
                if (model.RequiresShipping)
                {
                    decimal? shoppingCartShippingBase = _orderTotalCalculationService.GetShoppingCartShippingTotal(cart);
                    if (shoppingCartShippingBase.HasValue)
                    {
                        decimal shoppingCartShipping = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartShippingBase.Value, _workContext.WorkingCurrency);
                        model.Shipping = _priceFormatter.FormatShippingPrice(shoppingCartShipping, false);
                    }
                }

                //payment method fee
                decimal paymentMethodAdditionalFee = _paymentService.GetAdditionalHandlingFee(paymentMethodSystemName);
                decimal paymentMethodAdditionalFeeWithTaxBase = _taxService.GetPaymentMethodAdditionalFee(paymentMethodAdditionalFee, _workContext.CurrentCustomer);
                if (paymentMethodAdditionalFeeWithTaxBase > decimal.Zero)
                {
                    decimal paymentMethodAdditionalFeeWithTax = _currencyService.ConvertFromPrimaryStoreCurrency(paymentMethodAdditionalFeeWithTaxBase, _workContext.WorkingCurrency);
                    model.PaymentMethodAdditionalFee = _priceFormatter.FormatPaymentMethodAdditionalFee(paymentMethodAdditionalFeeWithTax, true);
                }

                //tax
                bool displayTax = true;
                bool displayTaxRates = true;
                if (_taxSettings.HideTaxInOrderSummary && _workContext.TaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    displayTax = false;
                    displayTaxRates = false;
                }
                else
                {
                    SortedDictionary<decimal, decimal> taxRates = null;
                    decimal shoppingCartTaxBase = _orderTotalCalculationService.GetTaxTotal(cart, out taxRates);
                    decimal shoppingCartTax = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartTaxBase, _workContext.WorkingCurrency);

                    if (shoppingCartTaxBase == 0 && _taxSettings.HideZeroTax)
                    {
                        displayTax = false;
                        displayTaxRates = false;
                    }
                    else
                    {
                        displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Count > 0;
                        displayTax = !displayTaxRates;

                        model.Tax = _priceFormatter.FormatPrice(shoppingCartTax, false, false);
                        foreach (var tr in taxRates)
                        {
                            model.TaxRates.Add(new OrderTotalsModel.TaxRate()
                                {
                                    Rate = _priceFormatter.FormatTaxRate(tr.Key),
                                    Value = _priceFormatter.FormatPrice(_currencyService.ConvertFromPrimaryStoreCurrency(tr.Value, _workContext.WorkingCurrency), false, false),
                                });
                        }
                    }
                }
                model.DisplayTaxRates = displayTaxRates;
                model.DisplayTax = displayTax;

                //total
                decimal orderTotalDiscountAmountBase = decimal.Zero;
                Discount orderTotalAppliedDiscount = null;
                List<AppliedGiftCard> appliedGiftCards = null;
                int redeemedRewardPoints = 0;
                decimal redeemedRewardPointsAmount = decimal.Zero;
                decimal? shoppingCartTotalBase = _orderTotalCalculationService.GetShoppingCartTotal(cart,
                    out orderTotalDiscountAmountBase, out orderTotalAppliedDiscount,
                    out appliedGiftCards, out redeemedRewardPoints, out redeemedRewardPointsAmount);
                if (shoppingCartTotalBase.HasValue)
                {
                    decimal shoppingCartTotal = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartTotalBase.Value, _workContext.WorkingCurrency);
                    model.OrderTotal = _priceFormatter.FormatPrice(shoppingCartTotal, false, false);
                }

                //discount
                if (orderTotalDiscountAmountBase > decimal.Zero)
                {
                    decimal orderTotalDiscountAmount = _currencyService.ConvertFromPrimaryStoreCurrency(orderTotalDiscountAmountBase, _workContext.WorkingCurrency);
                    model.OrderTotalDiscount = _priceFormatter.FormatPrice(-orderTotalDiscountAmount, false, false);
                    model.AllowRemovingOrderTotalDiscount = orderTotalAppliedDiscount != null &&
                        orderTotalAppliedDiscount.RequiresCouponCode &&
                        !String.IsNullOrEmpty(orderTotalAppliedDiscount.CouponCode) &&
                        model.IsEditable;
                }

                //gift cards
                if (appliedGiftCards != null && appliedGiftCards.Count > 0)
                {
                    foreach (var appliedGiftCard in appliedGiftCards)
                    {
                        var gcModel = new OrderTotalsModel.GiftCard()
                            {
                                Id = appliedGiftCard.GiftCard.Id,
                                CouponCode = appliedGiftCard.GiftCard.GiftCardCouponCode,
                            };
                        decimal amountCanBeUsed = _currencyService.ConvertFromPrimaryStoreCurrency(appliedGiftCard.AmountCanBeUsed, _workContext.WorkingCurrency);
                        gcModel.Amount = _priceFormatter.FormatPrice(-amountCanBeUsed, false, false);

                        decimal remainingAmountBase = appliedGiftCard.GiftCard.GetGiftCardRemainingAmount() - appliedGiftCard.AmountCanBeUsed;
                        decimal remainingAmount = _currencyService.ConvertFromPrimaryStoreCurrency(remainingAmountBase, _workContext.WorkingCurrency);
                        gcModel.Remaining = _priceFormatter.FormatPrice(remainingAmount, false, false);

                        model.GiftCards.Add(gcModel);
                    }
                }

                //reward points
                if (redeemedRewardPointsAmount > decimal.Zero)
                {
                    decimal redeemedRewardPointsAmountInCustomerCurrency = _currencyService.ConvertFromPrimaryStoreCurrency(redeemedRewardPointsAmount, _workContext.WorkingCurrency);
                    model.RedeemedRewardPoints = redeemedRewardPoints;
                    model.RedeemedRewardPointsAmount = _priceFormatter.FormatPrice(-redeemedRewardPointsAmountInCustomerCurrency, false, false);
                }
            }


            return PartialView(model);
        }

        [ValidateInput(false)]
        [HttpPost, ActionName("Cart")]
        [FormValueRequired("removesubtotaldiscount", "removeordertotaldiscount")]
        public ActionResult RemoveDiscountCoupon()
        {
            var cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();
            var model = new ShoppingCartModel();

            _workContext.CurrentCustomer.DiscountCouponCode = "";
            _customerService.UpdateCustomer(_workContext.CurrentCustomer);

            model = PrepareShoppingCartModel(model, cart, true);
            return View(model);
        }

        [ValidateInput(false)]
        [HttpPost, ActionName("Cart")]
        [FormValueRequired("removegiftcard")]
        public ActionResult RemoveGiftardCode(int giftCardId)
        {
            var cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();
            var model = new ShoppingCartModel();

            var gc = _giftCardService.GetGiftCardById(giftCardId);
            if (gc != null)
            {
                _workContext.CurrentCustomer.RemoveGiftCardCouponCode(gc.GiftCardCouponCode);
                _customerService.UpdateCustomer(_workContext.CurrentCustomer);
            }

            model = PrepareShoppingCartModel(model, cart, true);
            return View(model);
        }

        //AF
        [ChildActionOnly]
        public ActionResult MiniShoppingCart()
        {
            if (!_shoppingCartSettings.MiniShoppingCartEnabled)
                return Content("");

            var cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();
            var model = PrepareMiniShoppingCartModel(new ShoppingCartModel(), cart, true);
            return PartialView(model);
        }

        [ChildActionOnly]
        public ActionResult MiniShoppingCartNew()
        {
            if (!_shoppingCartSettings.MiniShoppingCartEnabled)
                return Content("");

            var cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();
            var model = PrepareMiniShoppingCartModel(new ShoppingCartModel(), cart, true);
            return PartialView(model);
        }

        //AF
        [HttpPost]
        [ValidateInput(false)]
        public JsonResult AddToCartProduct(AddToCartModel model)
        {
            List<string> messages = new List<string>();
            var product = _productService.GetProductById(model.ProductId);
            if (product == null || product.Deleted || !product.Published)
                messages.Add(_localizationService.GetResource("Cart.Product.NotFound"));
            var productVariant = _productService.GetProductVariantById(model.VariantId);
            if (productVariant == null)
                messages.Add(_localizationService.GetResource("Cart.Product.NotFound"));


            #region Product attributes
            string selectedAttributes = "<Attributes><GiftCardInfo><RecipientName>"+ model.RecipientName +"</RecipientName><RecipientEmail>"+model.RecipientEmail+"</RecipientEmail><SenderName>"+model.YourName+"</SenderName><SenderEmail>"+model.YourEmail+"</SenderEmail><Message>"+model.Message+"</Message></GiftCardInfo></Attributes>";
            var productVariantAttributes = _productAttributeService.GetProductVariantAttributesByProductVariantId(productVariant.Id);
            foreach (var attribute in productVariantAttributes)
            {
                var attr = model.Attributes.FirstOrDefault(x => x.ProductAttributeId == attribute.ProductAttributeId);
                if (attr == null) continue;
                selectedAttributes = _productAttributeParser.AddProductAttribute(selectedAttributes,
                                        attribute, attr.ProductVariantAttributeValueId);
            }
            #endregion

            //save item
            messages.AddRange(_shoppingCartService.AddToCart(_workContext.CurrentCustomer,
                productVariant, (ShoppingCartType)model.ShoppingCartType, selectedAttributes, 0, model.Quantity, true));


            return Json(new { Messages = messages, Html = MiniShoppingCartHtml() });

        }




        //AF
        public string MiniShoppingCartHtml()
        {
            if (!_shoppingCartSettings.MiniShoppingCartEnabled)
                return string.Empty;

            var cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();
            var model = PrepareMiniShoppingCartModel(new ShoppingCartModel(), cart, true);
            return Utilities.RenderPartialViewToString(this, @"~/Views\ShoppingCart\MiniShoppingCartNew.cshtml", model);
        }



        #endregion

        #region Wishlist

        [NopHttpsRequirement(SslRequirement.Yes)]
        public ActionResult Wishlist(Guid? customerGuid)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableWishlist))
                return RedirectToAction("Index", "Home");

            Customer customer = customerGuid.HasValue ?
                _customerService.GetCustomerByGuid(customerGuid.Value)
                : _workContext.CurrentCustomer;

            if (customer == null)
                return RedirectToAction("Index", "Home");
            var cart = customer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist).ToList();
            var model = PrepareWishlistModel(new WishlistModel(), cart, !customerGuid.HasValue);
            if (model.Items.Count == 0)
            {
                var messageModel = new MessageModel();
                messageModel.MessageList.Add(_localizationService.GetResource("Wishlist.WishlistIsEmpty"));
                messageModel.ActionText = _localizationService.GetResource("ShoppingCart.CartIsEmpty.Continue");
                messageModel.ActionUrl = ControllerContext.HttpContext.Request.ApplicationPath;
                ViewBag.MessageModel = messageModel;
            }
            model.Navigationmodel = new CustomerNavigationModel();
            model.Navigationmodel.SelectedTab = CustomerNavigationEnum.WishList;
            return View(model);
        }


        [NopHttpsRequirement(SslRequirement.Yes)]
        public ActionResult WishlistSummary(Guid? customerGuid)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableWishlist))
                return RedirectToAction("Index", "Home");

            Customer customer = customerGuid.HasValue ?
                _customerService.GetCustomerByGuid(customerGuid.Value)
                : _workContext.CurrentCustomer;

            if (customer == null)
                return RedirectToAction("Index", "Home");
            var cart = customer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist).ToList();
            var model = PrepareWishlistModel(new WishlistModel(), cart, !customerGuid.HasValue);
            model.Navigationmodel = new CustomerNavigationModel();
            model.Navigationmodel.SelectedTab = CustomerNavigationEnum.WishList;
            return View("WishListSummaryProducts", model.Items.Take(1).ToList());
        }


        [ValidateInput(false)]
        [HttpPost, ActionName("Wishlist")]
        [FormValueRequired("updatecart")]
        public ActionResult UpdateWishlist(FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableWishlist))
                return RedirectToAction("Index", "Home");

            var cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist).ToList();

            var allIdsToRemove = form["removefromcart"] != null ? form["removefromcart"].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToList() : new List<int>();
            foreach (var sci in cart)
            {
                bool remove = allIdsToRemove.Contains(sci.Id);
                if (remove)
                    _shoppingCartService.DeleteShoppingCartItem(sci, true);
                else
                {
                    int newQuantity = sci.Quantity;
                    foreach (string formKey in form.AllKeys)
                        if (formKey.Equals(string.Format("itemquantity{0}", sci.Id), StringComparison.InvariantCultureIgnoreCase))
                        {
                            int.TryParse(form[formKey], out newQuantity);
                            break;
                        }
                    _shoppingCartService.UpdateShoppingCartItem(_workContext.CurrentCustomer,
                        sci.Id, newQuantity, true, sci.CustomerComment);
                }
            }

            //updated cart
            cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist).ToList();
            var model = PrepareWishlistModel(new WishlistModel(), cart, true);
            return View(model);
        }

        public JsonResult UpdateWishlist(string id, string comment)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableWishlist))
                return Json(new { header = _localizationService.GetResource("Wishlist.UpdateNotificationUnsuccess"), message = "", success = false }, JsonRequestBehavior.AllowGet);

            var item = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist && sci.Id.ToString() == id).FirstOrDefault();
            item.CustomerComment = comment;

            var productVariant = _productService.GetProductVariantById(item.ProductVariantId);
            if (productVariant == null)
                return Json(new { header = _localizationService.GetResource("Wishlist.UpdateNotificationUnsuccess"), message = "", success = false }, JsonRequestBehavior.AllowGet);
            _shoppingCartService.UpdateShoppingCartItem(_workContext.CurrentCustomer, item.Id, 1, true, item.CustomerComment);

            return Json(new { header = _localizationService.GetResource("Wishlist.UpdateNotificationSuccess"), message = "" + id, success = true }, JsonRequestBehavior.AllowGet);
        }

        [ValidateInput(false)]
        [HttpPost, ActionName("Wishlist")]
        [FormValueRequired("addtocartbutton")]
        public ActionResult AddtoCartFromWishlist(FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableWishlist))
                return RedirectToAction("Index", "Home");

            var cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist).ToList();

            var allIdsToAdd = form["addtocart"] != null ? form["addtocart"].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToList() : new List<int>();
            foreach (var sci in cart)
            {
                if (allIdsToAdd.Contains(sci.Id))
                {
                    _shoppingCartService.AddToCart(_workContext.CurrentCustomer,
                        sci.ProductVariant, ShoppingCartType.ShoppingCart,
                        sci.AttributesXml, sci.CustomerEnteredPrice, sci.Quantity, true);
                }
            }

            //updated cart
            cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist).ToList();
            var model = PrepareWishlistModel(new WishlistModel(), cart, true);
            return View(model);
        }


        //AF
        public JsonResult AddtoCartFromWishlist(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableWishlist))
                return Json(new { success = false, message = _localizationService.GetResource("WishList.Settings.NotEnabled") }, JsonRequestBehavior.AllowGet);

            var item = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist && sci.Id.ToString() == id).FirstOrDefault();

            List<string> messages = new List<string>();
            var product = _productService.GetProductById(item.ProductVariant.ProductId);
            if (product == null || product.Deleted || !product.Published)
                return Json(new { success = false, message = _localizationService.GetResource("Cart.Product.NotFound") }, JsonRequestBehavior.AllowGet);
            var productVariant = _productService.GetProductVariantById(item.ProductVariantId);
            if (productVariant == null)
                return Json(new { success = false, message = _localizationService.GetResource("Cart.Product.NotFound") }, JsonRequestBehavior.AllowGet);


            #region Product attributes
            string selectedAttributes = string.Empty;
            var productVariantAttributes = _productAttributeService.GetProductVariantAttributesByProductVariantId(productVariant.Id);
            //foreach (var attribute in productVariantAttributes)
            //{
            //    var attr = model.Attributes.FirstOrDefault(x => x.ProductAttributeId == attribute.ProductAttributeId);
            //    if (attr == null) continue;
            //    selectedAttributes = _productAttributeParser.AddProductAttribute(selectedAttributes,
            //                            attribute, attr.ProductVariantAttributeValueId);
            //}
            #endregion

            // wish list item.Quantity instead of 1 set value to be added
            var warnings = _shoppingCartService.AddToCartFromWishList(_workContext.CurrentCustomer, item.ProductVariant, ShoppingCartType.ShoppingCart,
                                          item.AttributesXml, item.CustomerEnteredPrice, item.Quantity, true, item.CustomerComment);

            if (warnings.Count > 0)
                return Json(new { success = false, message = warnings[0] }, JsonRequestBehavior.AllowGet);

            return Json(new { html = MiniShoppingCartHtml(), success = true }, JsonRequestBehavior.AllowGet);
        }

        //item.Quantity static 1
        private IList<string> AddtoCartFromWishlist(IEnumerable<Nop.Core.Domain.Orders.ShoppingCartItem> items)
        {
            //IList<string> warnings =
            //    _shoppingCartService.AddToCart(_workContext.CurrentCustomer, item.ProductVariant, ShoppingCartType.ShoppingCart,
            //                              item.AttributesXml, item.CustomerEnteredPrice, 1, true, item.CustomerComment);

            IList<string> warnings = _shoppingCartService.AddToCartByList(items, _workContext.CurrentCustomer, ShoppingCartType.ShoppingCart, 1, true);
            return warnings;
        }


        public ActionResult EmailWishlist()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableWishlist) || !_shoppingCartSettings.EmailWishlistEnabled)
                return RedirectToAction("Index", "Home");

            var cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist).ToList();

            if (cart.Count == 0)
                return RedirectToAction("Index", "Home");

            var model = new WishlistEmailAFriendModel()
            {
                YourEmailAddress = _workContext.CurrentCustomer.Email
            };
            return View(model);
        }


        public ActionResult WishListEmailAFriend()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableWishlist) || !_shoppingCartSettings.EmailWishlistEnabled)
                return RedirectToAction("Index", "Home");
            WishlistEmailAFriendModel model = new WishlistEmailAFriendModel();
            model.YourEmailAddress = _workContext.CurrentCustomer.Email;
            return View(model);
        }


        [HttpPost, ActionName("EmailWishlistSend")]
        public ActionResult EmailWishlistSend(WishlistEmailAFriendModel model)
        {

            var cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist).ToList();

            if (cart.Count == 0)
                return RedirectToAction("Index", "Home");

            if (ModelState.IsValid)
            {
                if (_workContext.CurrentCustomer.IsGuest())
                {
                    ModelState.AddModelError("", _localizationService.GetResource("Wishlist.EmailAFriend.OnlyRegisteredUsers"));
                }
                else
                {
                    //email
                    _workflowMessageService.SendWishlistEmailAFriendMessage(_workContext.CurrentCustomer,
                            _workContext.WorkingLanguage.Id, model.YourEmailAddress,
                            model.FriendEmail, Core.Html.HtmlHelper.FormatText(model.PersonalMessage, false, true, false, false, false, false), true);

                    model.SuccessfullySent = true;
                    model.Result = _localizationService.GetResource("Wishlist.EmailAFriend.SuccessfullySent");

                    return Json(new { header = _localizationService.GetResource("Wishlist.EmailAFriend.Success"), message = _localizationService.GetResource("Wishlist.EmailAFriend.Success.Detail"), success = true }, JsonRequestBehavior.AllowGet);
                }
            }

            //If we got this far, something failed, redisplay form
            return Json(new { header = _localizationService.GetResource("Wishlist.EmailAFriend.Success"), message = _localizationService.GetResource("Wishlist.EmailAFriend.Fail.Detail"), success = false }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult WishListAction()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableWishlist) || !_shoppingCartSettings.EmailWishlistEnabled)
                return RedirectToAction("Index", "Home");
            WishlistEmailAFriendModel model = new WishlistEmailAFriendModel();
            model.YourEmailAddress = _workContext.CurrentCustomer.Email;
            model.PersonalMessage = "";
            model.FriendEmail = "";
            return View("WishListEmailAFriend", model);
        }



        #endregion

        #region  ********* SalesAgreements**********

        [NonAction]
        protected TopicModel PrepareTopicModel(string systemName)
        {
            //load by store
            var topic = _topicService.GetTopicBySystemName(systemName);//, _storeContext.CurrentStore.Id
            if (topic == null)
                return null;

            var model = new TopicModel()
            {
                Id = topic.Id,
                SystemName = topic.SystemName,
                IncludeInSitemap = topic.IncludeInSitemap,
                IsPasswordProtected = topic.IsPasswordProtected,
                Title = topic.IsPasswordProtected ? "" : topic.GetLocalized(x => x.Title),
                Body = topic.IsPasswordProtected ? "" : topic.GetLocalized(x => x.Body),
                MetaKeywords = topic.GetLocalized(x => x.MetaKeywords),
                MetaDescription = topic.GetLocalized(x => x.MetaDescription),
                MetaTitle = topic.GetLocalized(x => x.MetaTitle),
            };
            return model;
        }

        [NopHttpsRequirement(SslRequirement.Yes)]
        public ActionResult SalesAgreement(string systemName)
        {
            var c = PrepareTopicModel(systemName);
            if (c == null)
                return RedirectToRoute("HomePage");

            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                  .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();

            #region  model values

            var model = new ShoppingCartModel();
            PrepareShoppingCartModel(model, cart, true, true, true);

            #region order total

            var ordertotal = new OrderTotalsModel();
            ordertotal.IsEditable = model.IsEditable;

            if (cart.Count > 0)
            {
                //subtotal
                decimal subtotalBase = decimal.Zero;
                decimal orderSubTotalDiscountAmountBase = decimal.Zero;
                Discount orderSubTotalAppliedDiscount = null;
                decimal subTotalWithoutDiscountBase = decimal.Zero;
                decimal subTotalWithDiscountBase = decimal.Zero;
                _orderTotalCalculationService.GetShoppingCartSubTotal(cart,
                    out orderSubTotalDiscountAmountBase, out orderSubTotalAppliedDiscount,
                    out subTotalWithoutDiscountBase, out subTotalWithDiscountBase);
                subtotalBase = subTotalWithoutDiscountBase;
                decimal subtotal = _currencyService.ConvertFromPrimaryStoreCurrency(subtotalBase, _workContext.WorkingCurrency);
                ordertotal.SubTotal = _priceFormatter.FormatPrice(subtotal);
                if (orderSubTotalDiscountAmountBase > decimal.Zero)
                {
                    decimal orderSubTotalDiscountAmount = _currencyService.ConvertFromPrimaryStoreCurrency(orderSubTotalDiscountAmountBase, _workContext.WorkingCurrency);
                    ordertotal.SubTotalDiscount = _priceFormatter.FormatPrice(-orderSubTotalDiscountAmount);
                    ordertotal.AllowRemovingSubTotalDiscount = orderSubTotalAppliedDiscount != null &&
                        orderSubTotalAppliedDiscount.RequiresCouponCode &&
                        !String.IsNullOrEmpty(orderSubTotalAppliedDiscount.CouponCode) &&
                        model.IsEditable;
                }

                #region  Gift Wrapping Values

                Customer customer = cart.GetCustomer();
                decimal cavalues = new decimal();
                var checkoutAttributesXml = customer.GetAttribute<string>(SystemCustomerAttributeNames.CheckoutAttributes); /****_genericAttributeService parametresi silindi************/
                var caValues = _checkoutAttributeParser.ParseCheckoutAttributeValues(checkoutAttributesXml);
                if (caValues != null)
                {
                    foreach (var caValue in caValues)
                    {
                        cavalues += _taxService.GetCheckoutAttributePrice(caValue);
                    }
                    if (cavalues > 0)
                    {
                        decimal ca = _currencyService.ConvertFromPrimaryStoreCurrency(cavalues, _workContext.WorkingCurrency);
                        decimal subtotalwithoutca = subtotal - ca;
                        ordertotal.SubTotal = _priceFormatter.FormatPrice(subtotalwithoutca);
                        ordertotal.cavalue = _priceFormatter.FormatPrice(ca);
                    }
                    else
                    {
                        ordertotal.cavalue = null;
                    }
                }

                #endregion

                //shipping info
                ordertotal.RequiresShipping = cart.RequiresShipping();
                if (ordertotal.RequiresShipping)
                {
                    decimal? shoppingCartShippingBase = _orderTotalCalculationService.GetShoppingCartShippingTotal(cart);
                    if (shoppingCartShippingBase.HasValue)
                    {
                        decimal shoppingCartShipping = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartShippingBase.Value, _workContext.WorkingCurrency);
                        ordertotal.Shipping = _priceFormatter.FormatShippingPrice(shoppingCartShipping, false);

                        //selected shipping method
                        var shippingOption = _workContext.CurrentCustomer.GetAttribute<ShippingOption>(SystemCustomerAttributeNames.SelectedShippingOption);//, _storeContext.CurrentStore.Id
                        if (shippingOption != null)
                            ordertotal.SelectedShippingMethod = shippingOption.Name;
                    }
                }

                //payment method fee
                string paymentMethodSystemName = _workContext.CurrentCustomer.GetAttribute<string>(
                    SystemCustomerAttributeNames.SelectedPaymentMethod); // _storeContext.CurrentStore.Id parametresi silindi
                //decimal paymentMethodAdditionalFee = _paymentService.GetAdditionalHandlingFee(cart, paymentMethodSystemName);
                decimal paymentMethodAdditionalFee = _paymentService.GetAdditionalHandlingFee(paymentMethodSystemName);
                decimal paymentMethodAdditionalFeeWithTaxBase = _taxService.GetPaymentMethodAdditionalFee(paymentMethodAdditionalFee, _workContext.CurrentCustomer);
                if (paymentMethodAdditionalFeeWithTaxBase > decimal.Zero)
                {
                    decimal paymentMethodAdditionalFeeWithTax = _currencyService.ConvertFromPrimaryStoreCurrency(paymentMethodAdditionalFeeWithTaxBase, _workContext.WorkingCurrency);
                    ordertotal.PaymentMethodAdditionalFee = _priceFormatter.FormatPaymentMethodAdditionalFee(paymentMethodAdditionalFeeWithTax, false);
                }

                //tax
                bool displayTax = true;
                bool displayTaxRates = true;
                if (_taxSettings.HideTaxInOrderSummary && _workContext.TaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    displayTax = false;
                    displayTaxRates = false;
                }
                else
                {
                    SortedDictionary<decimal, decimal> taxRates = null;
                    decimal shoppingCartTaxBase = _orderTotalCalculationService.GetTaxTotal(cart, out taxRates);
                    decimal shoppingCartTax = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartTaxBase, _workContext.WorkingCurrency);

                    if (shoppingCartTaxBase == 0 && _taxSettings.HideZeroTax)
                    {
                        displayTax = false;
                        displayTaxRates = false;
                    }
                    else
                    {
                        displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Count > 0;
                        displayTax = !displayTaxRates;

                        ordertotal.Tax = _priceFormatter.FormatPrice(shoppingCartTax, false, false);
                        foreach (var tr in taxRates)
                        {
                            ordertotal.TaxRates.Add(new OrderTotalsModel.TaxRate()
                            {
                                Rate = _priceFormatter.FormatTaxRate(tr.Key),
                                Value = _priceFormatter.FormatPrice(_currencyService.ConvertFromPrimaryStoreCurrency(tr.Value, _workContext.WorkingCurrency), false, false),
                            });
                        }
                    }
                }
                ordertotal.DisplayTaxRates = displayTaxRates;
                ordertotal.DisplayTax = displayTax;

                //total
                decimal orderTotalDiscountAmountBase = decimal.Zero;
                Discount orderTotalAppliedDiscount = null;
                List<AppliedGiftCard> appliedGiftCards = null;
                int redeemedRewardPoints = 0;
                decimal redeemedRewardPointsAmount = decimal.Zero;
                decimal? shoppingCartTotalBase = _orderTotalCalculationService.GetShoppingCartTotal(cart,
                    out orderTotalDiscountAmountBase, out orderTotalAppliedDiscount,
                    out appliedGiftCards, out redeemedRewardPoints, out redeemedRewardPointsAmount);
                if (shoppingCartTotalBase.HasValue)
                {
                    decimal shoppingCartTotal = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartTotalBase.Value, _workContext.WorkingCurrency);
                    ordertotal.OrderTotal = _priceFormatter.FormatPrice(shoppingCartTotal, false, false);
                }

                //discount
                if (orderTotalDiscountAmountBase > decimal.Zero)
                {
                    decimal orderTotalDiscountAmount = _currencyService.ConvertFromPrimaryStoreCurrency(orderTotalDiscountAmountBase, _workContext.WorkingCurrency);
                    ordertotal.OrderTotalDiscount = _priceFormatter.FormatPrice(-orderTotalDiscountAmount, false, false);
                    ordertotal.AllowRemovingOrderTotalDiscount = orderTotalAppliedDiscount != null &&
                        orderTotalAppliedDiscount.RequiresCouponCode &&
                        !String.IsNullOrEmpty(orderTotalAppliedDiscount.CouponCode) &&
                        model.IsEditable;
                }

                //gift cards
                if (appliedGiftCards != null && appliedGiftCards.Count > 0)
                {
                    foreach (var appliedGiftCard in appliedGiftCards)
                    {
                        var gcModel = new OrderTotalsModel.GiftCard()
                        {
                            Id = appliedGiftCard.GiftCard.Id,
                            CouponCode = appliedGiftCard.GiftCard.GiftCardCouponCode,
                        };
                        decimal amountCanBeUsed = _currencyService.ConvertFromPrimaryStoreCurrency(appliedGiftCard.AmountCanBeUsed, _workContext.WorkingCurrency);
                        gcModel.Amount = _priceFormatter.FormatPrice(-amountCanBeUsed, false, false);

                        decimal remainingAmountBase = appliedGiftCard.GiftCard.GetGiftCardRemainingAmount() - appliedGiftCard.AmountCanBeUsed;
                        decimal remainingAmount = _currencyService.ConvertFromPrimaryStoreCurrency(remainingAmountBase, _workContext.WorkingCurrency);
                        gcModel.Remaining = _priceFormatter.FormatPrice(remainingAmount, false, false);

                        ordertotal.GiftCards.Add(gcModel);

                    }
                }

                //reward points
                if (redeemedRewardPointsAmount > decimal.Zero)
                {
                    decimal redeemedRewardPointsAmountInCustomerCurrency = _currencyService.ConvertFromPrimaryStoreCurrency(redeemedRewardPointsAmount, _workContext.WorkingCurrency);
                    ordertotal.RedeemedRewardPoints = redeemedRewardPoints;
                    ordertotal.RedeemedRewardPointsAmount = _priceFormatter.FormatPrice(-redeemedRewardPointsAmountInCustomerCurrency, false, false);
                }
            }


            #endregion

            #endregion

            #region domain model

            var salesAgrementDetails = new SalesAgreement();
            foreach (var m in model.Items)
            {
                var salesItem = new Nop.Core.Domain.Custom.ShoppingCartItem();
                salesItem.AllowedQuantities = m.AllowedQuantities;
                salesItem.AttributeInfo = m.AttributeInfo;
                salesItem.Discount = m.Discount;
                salesItem.Id = m.Id;
                salesItem.ProductId = m.ProductId;
                salesItem.ProductName = m.ProductName;
                salesItem.ProductSeName = m.ProductSeName;
                salesItem.Quantity = m.Quantity;
                salesItem.RecurringInfo = m.RecurringInfo;
                salesItem.Sku = m.Sku;
                salesItem.SubTotal = m.SubTotal;
                salesItem.UnitPrice = m.UnitPrice;
                salesItem.Warnings = m.Warnings;
                salesAgrementDetails.Items.Add(salesItem);
            }
            foreach (var m in model.CheckoutAttributes)
            {
                var checkOutAttributeInfo = new Nop.Core.Domain.Custom.CheckoutAttribute();
                checkOutAttributeInfo.Id = m.Id;
                checkOutAttributeInfo.AttributeControlType = m.AttributeControlType;
                checkOutAttributeInfo.DefaultValue = m.DefaultValue;
                checkOutAttributeInfo.IsRequired = m.IsRequired;
                checkOutAttributeInfo.Name = m.Name;
                checkOutAttributeInfo.SelectedDay = m.SelectedDay;
                checkOutAttributeInfo.SelectedMonth = m.SelectedMonth;
                checkOutAttributeInfo.SelectedYear = m.SelectedYear;
                checkOutAttributeInfo.TextPrompt = m.TextPrompt;
                foreach (var i in m.Values)
                {
                    var value = new Nop.Core.Domain.Custom.CheckoutAttributeValue();
                    value.Id = i.Id;
                    value.IsPreSelected = i.IsPreSelected;
                    value.Name = i.Name;
                    value.PriceAdjustment = i.PriceAdjustment;
                    value.ColorSquaresRgb = i.ColorSquaresRgb;
                    checkOutAttributeInfo.Values.Add(value);
                }
                salesAgrementDetails.CheckoutAttributes.Add(checkOutAttributeInfo);
            }
            var a = new Nop.Core.Domain.Common.Address();
            salesAgrementDetails.CheckoutAttributeInfo = model.CheckoutAttributeInfo;
            salesAgrementDetails.DiscountBox.CurrentCode = model.DiscountBox.CurrentCode;
            salesAgrementDetails.DiscountBox.Message = model.DiscountBox.Message;
            salesAgrementDetails.DiscountBox.Display = model.DiscountBox.Display;
            salesAgrementDetails.GiftCardBox.Message = model.GiftCardBox.Message;
            salesAgrementDetails.GiftCardBox.Display = model.GiftCardBox.Display;
            salesAgrementDetails.IsEditable = model.IsEditable;

            salesAgrementDetails.MinOrderSubtotalWarning = model.MinOrderSubtotalWarning;
            salesAgrementDetails.OrderReviewData.BillingAddress.Address1 = _workContext.CurrentCustomer.BillingAddress.Address1;
            salesAgrementDetails.OrderReviewData.BillingAddress.Address2 = _workContext.CurrentCustomer.BillingAddress.Address2;
            salesAgrementDetails.OrderReviewData.BillingAddress.City = _workContext.CurrentCustomer.BillingAddress.City;
            salesAgrementDetails.OrderReviewData.BillingAddress.Company = _workContext.CurrentCustomer.BillingAddress.Company;
            salesAgrementDetails.OrderReviewData.Billing_Country = _workContext.CurrentCustomer.BillingAddress.Country.Name;
            salesAgrementDetails.OrderReviewData.BillingAddress.CountryId = _workContext.CurrentCustomer.BillingAddress.CountryId;
            salesAgrementDetails.OrderReviewData.BillingAddress.Email = _workContext.CurrentCustomer.Email;
            salesAgrementDetails.OrderReviewData.BillingAddress.FaxNumber = _workContext.CurrentCustomer.BillingAddress.FaxNumber;
            salesAgrementDetails.OrderReviewData.BillingAddress.FirstName = _workContext.CurrentCustomer.BillingAddress.FirstName;
            salesAgrementDetails.OrderReviewData.BillingAddress.Id = _workContext.CurrentCustomer.BillingAddress.Id;
            salesAgrementDetails.OrderReviewData.BillingAddress.LastName = _workContext.CurrentCustomer.BillingAddress.LastName;
            salesAgrementDetails.OrderReviewData.BillingAddress.PhoneNumber = _workContext.CurrentCustomer.BillingAddress.PhoneNumber;
            if (_workContext.CurrentCustomer.BillingAddress.StateProvince != null)
            {
                salesAgrementDetails.OrderReviewData.Billing_SatateProvision = _workContext.CurrentCustomer.BillingAddress.StateProvince.Name;
            }
            salesAgrementDetails.OrderReviewData.BillingAddress.StateProvinceId = _workContext.CurrentCustomer.BillingAddress.StateProvinceId;
            salesAgrementDetails.OrderReviewData.BillingAddress.ZipPostalCode = _workContext.CurrentCustomer.BillingAddress.ZipPostalCode;


            salesAgrementDetails.OrderReviewData.ShippingAddress.Address1 = _workContext.CurrentCustomer.ShippingAddress.Address1;
            salesAgrementDetails.OrderReviewData.ShippingAddress.Address2 = _workContext.CurrentCustomer.ShippingAddress.Address2;
            salesAgrementDetails.OrderReviewData.ShippingAddress.City = _workContext.CurrentCustomer.ShippingAddress.City;
            salesAgrementDetails.OrderReviewData.ShippingAddress.Company = _workContext.CurrentCustomer.ShippingAddress.Company;
            salesAgrementDetails.OrderReviewData.Shipping_Country = _workContext.CurrentCustomer.ShippingAddress.Country.Name;
            salesAgrementDetails.OrderReviewData.ShippingAddress.CountryId = _workContext.CurrentCustomer.ShippingAddress.CountryId;
            salesAgrementDetails.OrderReviewData.ShippingAddress.Email = _workContext.CurrentCustomer.ShippingAddress.Email;
            salesAgrementDetails.OrderReviewData.ShippingAddress.FaxNumber = _workContext.CurrentCustomer.ShippingAddress.FaxNumber;
            salesAgrementDetails.OrderReviewData.ShippingAddress.FirstName = _workContext.CurrentCustomer.ShippingAddress.FirstName;
            salesAgrementDetails.OrderReviewData.ShippingAddress.Id = _workContext.CurrentCustomer.ShippingAddress.Id;
            salesAgrementDetails.OrderReviewData.ShippingAddress.LastName = _workContext.CurrentCustomer.ShippingAddress.LastName;
            salesAgrementDetails.OrderReviewData.ShippingAddress.PhoneNumber = _workContext.CurrentCustomer.ShippingAddress.PhoneNumber;
            if (_workContext.CurrentCustomer.ShippingAddress.StateProvince != null)
            {
                salesAgrementDetails.OrderReviewData.Shipping_SatateProvision = _workContext.CurrentCustomer.ShippingAddress.StateProvince.Name;
            }
            salesAgrementDetails.OrderReviewData.ShippingAddress.StateProvinceId = _workContext.CurrentCustomer.ShippingAddress.StateProvinceId;
            salesAgrementDetails.OrderReviewData.ShippingAddress.ZipPostalCode = _workContext.CurrentCustomer.ShippingAddress.ZipPostalCode;
            salesAgrementDetails.OrderReviewData.Display = model.OrderReviewData.Display;
            salesAgrementDetails.OrderReviewData.IsShippable = model.OrderReviewData.IsShippable;

            if (cart.GetCustomer().SelectedPaymentMethodSystemName != null)
            {
                if (cart.GetCustomer().SelectedPaymentMethodSystemName == "Payments.PurchaseOrder")
                    salesAgrementDetails.OrderReviewData.PaymentMethod = _localizationService.GetResource("Checkout.Confirm.Payment.PurchaseOrderByWireTransfer").ToString();
                else
                    salesAgrementDetails.OrderReviewData.PaymentMethod = _localizationService.GetResource("Payment.PaymentMethod.CC").ToString();
            }

            else if (_workContext.CurrentCustomer.SelectedPaymentMethodSystemName != null)
            {
                if (_workContext.CurrentCustomer.SelectedPaymentMethodSystemName == "Payments.PurchaseOrder")
                    salesAgrementDetails.OrderReviewData.PaymentMethod = _localizationService.GetResource("Checkout.Confirm.Payment.PurchaseOrderByWireTransfer");
                else
                    salesAgrementDetails.OrderReviewData.PaymentMethod = _localizationService.GetResource("Payment.PaymentMethod.CC");
            }

            else
            {
                salesAgrementDetails.OrderReviewData.PaymentMethod = "";
            }

            salesAgrementDetails.OrderReviewData.ShippingMethod = _shippingService.GetShippingOptions(cart, _workContext.CurrentCustomer.ShippingAddress).ShippingOptions.FirstOrDefault().Name;
            salesAgrementDetails.OrderTotal.AllowRemovingOrderTotalDiscount = ordertotal.AllowRemovingOrderTotalDiscount;
            salesAgrementDetails.OrderTotal.AllowRemovingSubTotalDiscount = ordertotal.AllowRemovingSubTotalDiscount;
            salesAgrementDetails.OrderTotal.cavalue = ordertotal.cavalue;
            salesAgrementDetails.OrderTotal.DisplayTax = ordertotal.DisplayTax;
            salesAgrementDetails.OrderTotal.DisplayTaxRates = ordertotal.DisplayTaxRates;

            foreach (var m in ordertotal.GiftCards)
            {
                var giftCard = new Nop.Core.Domain.Custom.OrderTotals.GiftCard();
                giftCard.Amount = m.Amount;
                giftCard.CouponCode = m.CouponCode;
                giftCard.Id = m.Id;
                giftCard.Remaining = m.Remaining;
                salesAgrementDetails.OrderTotal.GiftCards.Add(giftCard);
            }
            salesAgrementDetails.OrderTotal.IsEditable = ordertotal.IsEditable;
            salesAgrementDetails.OrderTotal.OrderTotal = ordertotal.OrderTotal;
            salesAgrementDetails.OrderTotal.OrderTotalDiscount = ordertotal.OrderTotalDiscount;
            salesAgrementDetails.OrderTotal.PaymentMethodAdditionalFee = ordertotal.PaymentMethodAdditionalFee;
            salesAgrementDetails.OrderTotal.RedeemedRewardPoints = ordertotal.RedeemedRewardPoints;
            salesAgrementDetails.OrderTotal.RedeemedRewardPointsAmount = ordertotal.RedeemedRewardPointsAmount;
            salesAgrementDetails.OrderTotal.RequiresShipping = ordertotal.RequiresShipping;
            salesAgrementDetails.OrderTotal.SelectedShippingMethod = ordertotal.SelectedShippingMethod;
            salesAgrementDetails.OrderTotal.Shipping = ordertotal.Shipping;
            salesAgrementDetails.OrderTotal.SubTotal = ordertotal.SubTotal;
            salesAgrementDetails.OrderTotal.SubTotalDiscount = ordertotal.SubTotalDiscount;
            salesAgrementDetails.OrderTotal.Tax = ordertotal.Tax;
            foreach (var m in ordertotal.TaxRates)
            {
                var taxRate = new Nop.Core.Domain.Custom.OrderTotals.TaxRate();
                taxRate.Rate = m.Rate;
                taxRate.Value = m.Value;
                salesAgrementDetails.OrderTotal.TaxRates.Add(taxRate);
            }
            salesAgrementDetails.ShowProductImages = model.ShowProductImages;
            salesAgrementDetails.ShowSku = model.ShowSku;
            salesAgrementDetails.TermsOfServiceEnabled = model.TermsOfServiceEnabled;
            salesAgrementDetails.Warnings = model.Warnings;

            #endregion

            int languageId = _workContext.WorkingLanguage.Id;
            //var store = _storeContext.CurrentStore;
            var tokens = new List<Token>();
            //_messageTokenProvider.AddStoreTokens(tokens, store);
            _messageTokenProvider.AddSalesAgreementTokens(tokens, salesAgrementDetails);
            //  _messageTokenProvider.AddCustomerTokens(tokens, );

            string bodyReplaced = _tokenizer.Replace(c.Body, tokens, true);
            c.Body = bodyReplaced;
            c.MetaDescription = "";
            c.MetaKeywords = "";
            c.MetaTitle = "";
            c.Title = "";
            ViewBag.IsPopup = true;
            return View("~/Views/Checkout/TopicDetailsSalesAgreements.cshtml", c);
        }

        #endregion
    }
}
