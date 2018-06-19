using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using Nop.Web.Extensions;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Security;
using Nop.Web.Infrastructure;
using Nop.Web.Models.Checkout;
using Nop.Web.Models.Common;
using Nop.Web.Models.ShoppingCart;
using Nop.Services.Topics;
using System.Collections;
using System.Text;
using _PosnetDotNetTDSOOSModule;
using Nop.Services.Configuration;
using Nop.Services.Messages;
using Nop.Web.Models.Topics;


//
namespace Nop.Web.Controllers
{
    [NopHttpsRequirement(SslRequirement.Yes)]
    public class CheckoutController : BaseNopController
    {
		#region Fields

        private readonly IWorkContext _workContext;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ILocalizationService _localizationService;
        private readonly ITaxService _taxService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ICustomerService _customerService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IShippingService _shippingService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ILogger _logger;
        private readonly IOrderService _orderService;
        private readonly ITopicService _topicService;

        private readonly OrderSettings _orderSettings;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly PaymentSettings _paymentSettings;
        private readonly IAddressService _addressService;
        private readonly TaxSettings _taxSettings;

        private readonly ISettingService _settingService;

        private readonly IMessageTokenProvider _messageTokenProvider; // = EngineContext.Current.Resolve<IMessageTokenProvider>();
        private readonly ITokenizer _tokenizer; // = EngineContext.Current.Resolve<ITokenizer>();
        private readonly IPdfService _pdfService;
        #endregion

		#region Constructors

        public CheckoutController(IWorkContext workContext,
            IShoppingCartService shoppingCartService, ILocalizationService localizationService, 
            ITaxService taxService, ICurrencyService currencyService, 
            IPriceFormatter priceFormatter, IOrderProcessingService orderProcessingService,
            ICustomerService customerService,  ICountryService countryService,
            IStateProvinceService stateProvinceService, IShippingService shippingService, 
            IPaymentService paymentService, IOrderTotalCalculationService orderTotalCalculationService,
            ILogger logger, IOrderService orderService, 
            OrderSettings orderSettings, RewardPointsSettings rewardPointsSettings,
            PaymentSettings paymentSettings,
            IAddressService addressService,
            TaxSettings taxSettings,
            ITopicService topicService,
            ISettingService settingService
            , IMessageTokenProvider messageTokenProvider
            , ITokenizer tokenizer
            , IPdfService pdfService)
        {
            this._workContext = workContext;
            this._shoppingCartService = shoppingCartService;
            this._localizationService = localizationService;
            this._taxService = taxService;
            this._currencyService = currencyService;
            this._priceFormatter = priceFormatter;
            this._orderProcessingService = orderProcessingService;
            this._customerService = customerService;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._shippingService = shippingService;
            this._paymentService = paymentService;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._logger = logger;
            this._orderService = orderService;
            this._topicService = topicService;
            this._orderSettings = orderSettings;
            this._rewardPointsSettings = rewardPointsSettings;
            this._paymentSettings = paymentSettings;
            this._addressService = addressService;
            this._taxSettings = taxSettings;
            this._settingService = settingService;
            this._messageTokenProvider = messageTokenProvider;
            this._tokenizer = tokenizer;
            this._pdfService = pdfService;
        }

        #endregion

        #region Utilities

        [NonAction]
        private bool IsPaymentWorkflowRequired(IList<ShoppingCartItem> cart)
        {
            bool result = true;

            //check whether order total equals zero
            //decimal? shoppingCartTotalBase = _orderTotalCalculationService.GetShoppingCartTotal(cart);
            //if (shoppingCartTotalBase.HasValue && shoppingCartTotalBase.Value == decimal.Zero)
            //    result = false;
            return result;
        }

        #endregion

        #region Methods

        public ActionResult Index()
        { 
            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return new HttpUnauthorizedResult();
            
            var cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();
            if (cart.Count == 0)
                return RedirectToRoute("ShoppingCart");

            //reset checkout data
            _customerService.ResetCheckoutData(_workContext.CurrentCustomer, false);

            //OrderTotal
            var OrderTotalmodel = new OrderTotalsModel();
            decimal shoppingCartTotal = 0;
            if (cart.Count > 0)
            {
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
                    shoppingCartTotal = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartTotalBase.Value, _workContext.WorkingCurrency);
                    OrderTotalmodel.OrderTotal = _priceFormatter.FormatPrice(shoppingCartTotal, false, false);
                }
            }
            var model = new CheckoutProgressModel();
            model.OrderTotal = shoppingCartTotal.ToString();

            //validation
            var scWarnings = _shoppingCartService.GetShoppingCartWarnings(cart, _workContext.CurrentCustomer.CheckoutAttributes, true);
            if (scWarnings.Count > 0)
                return RedirectToRoute("ShoppingCart");
            else
            {
                foreach (ShoppingCartItem sci in cart)
                {
                    var sciWarnings = _shoppingCartService.GetShoppingCartItemWarnings(_workContext.CurrentCustomer,
                        sci.ShoppingCartType,
                            sci.ProductVariant,
                            sci.AttributesXml,
                            sci.CustomerEnteredPrice,
                            sci.Quantity,false);
                    if (sciWarnings.Count > 0)
                        return RedirectToRoute("ShoppingCart");
                }
            }


         //set default addresses
                var defaultShippingAddress = _workContext.CurrentCustomer.Addresses.FirstOrDefault(a => a.DefaultShippingAddress);
                var defaultBillingAddress = _workContext.CurrentCustomer.Addresses.FirstOrDefault(a => a.DefaultBillingAddress);
                if (defaultShippingAddress != null)
                    _workContext.CurrentCustomer.ShippingAddress = defaultShippingAddress;
                if (defaultBillingAddress != null)
                    _workContext.CurrentCustomer.BillingAddress = defaultBillingAddress;
                if (defaultShippingAddress != null || defaultBillingAddress != null) 
                    _customerService.UpdateCustomer(_workContext.CurrentCustomer);
           

            /*....*/

            //CheckoutProgressModel model = Session["CheckoutProgressModel"] as CheckoutProgressModel;
            //if (model == null)
            //    return View();
            //else


            var warnings = TempData["warnings"] as List<string>;
            if (warnings != null)
            {
                foreach(var warning in warnings)
                {
                    ModelState.AddModelError("", warning);
                }
            }
            return View(model);


        }
        //AF
        public ActionResult Addresses()
        {
            //model for shipping
            var shippingAddressModel = new CheckoutShippingAddressModel();
            //new address model
            shippingAddressModel.NewAddress = new AddressModel();
            //shippingAddressModel.NewAddress.AvailableCountries.Add(new SelectListItem() { Text = _localizationService.GetResource("Address.SelectCountry"), Value = "0" });
            foreach (var c in _countryService.GetAllCountriesForShipping())
                shippingAddressModel.NewAddress.AvailableCountries.Add(new SelectListItem() { Text = c.Name, Value = c.Id.ToString() });
            foreach (var p in _stateProvinceService.GetStateProvincesByCountryId(int.Parse(shippingAddressModel.NewAddress.AvailableCountries.First().Value)))
                shippingAddressModel.NewAddress.AvailableStates.Add(new SelectListItem() { Text = p.Name, Value = p.Id.ToString() });
            shippingAddressModel.NewAddress.CountryId = int.Parse(shippingAddressModel.NewAddress.AvailableCountries.FirstOrDefault().Value);
            shippingAddressModel.NewAddress.CountryName = shippingAddressModel.NewAddress.AvailableCountries.FirstOrDefault().Text;
            shippingAddressModel.NewAddress.StateProvinceId = int.Parse(shippingAddressModel.NewAddress.AvailableStates.FirstOrDefault().Value);
            shippingAddressModel.NewAddress.StateProvinceName = shippingAddressModel.NewAddress.AvailableStates.FirstOrDefault().Text;
            shippingAddressModel.NewAddress.Genders.Add(new SelectListItem() { Text = _localizationService.GetResource("Account.Fields.Gender.Female"), Value = "F" });
            shippingAddressModel.NewAddress.Genders.Add(new SelectListItem() { Text = _localizationService.GetResource("Account.Fields.Gender.Male"), Value = "M" });
            //existing addresses
            var addresses = _workContext.CurrentCustomer.Addresses.ToList();//.Where(a => a.Country.AllowsShipping)
            if (addresses.Count > 0)
            {
                if (_workContext.CurrentCustomer.BillingAddress == null)
                {
                    var address = addresses.FirstOrDefault(a => a.DefaultBillingAddress);
                    if (address == null)
                        address = addresses.First();
                    _workContext.CurrentCustomer.SetBillingAddress(address);
                }
                if (_workContext.CurrentCustomer.ShippingAddress == null)
                {
                    var address = addresses.FirstOrDefault(a => a.DefaultShippingAddress);
                    if (address == null)
                        address = addresses.First();
                    _workContext.CurrentCustomer.SetShippingAddress(address);
                }
            }
             foreach (var address in addresses)
            {
                var addressModel = address.ToModel();
                addressModel.AvailableCountries = shippingAddressModel.NewAddress.AvailableCountries;
                if (address.CountryId.Value != int.Parse(shippingAddressModel.NewAddress.AvailableCountries.First().Value))
                {
                    foreach (var p in _stateProvinceService.GetStateProvincesByCountryId(address.CountryId.Value))
                        addressModel.AvailableStates.Add(new SelectListItem() { Text = p.Name, Value = p.Id.ToString() });
                    if(addressModel.AvailableStates.Count==0)
                    {
                         addressModel.AvailableStates.Add(new SelectListItem() { Text = _localizationService.GetResource("Address.OtherNonUS"), Value = "0" });
                    }
                }
                else
                {
                    addressModel.AvailableStates = shippingAddressModel.NewAddress.AvailableStates;
                }
                addressModel.Genders = shippingAddressModel.NewAddress.Genders;
                shippingAddressModel.ExistingAddresses.Add(addressModel);
            }
            shippingAddressModel.ExistingAddresses.Add(shippingAddressModel.NewAddress);
            //model for billing
            var billingAddressModel = new CheckoutBillingAddressModel();
            //new address model
            billingAddressModel.NewAddress = new AddressModel();
            foreach (var c in _countryService.GetAllCountriesForBilling())
                billingAddressModel.NewAddress.AvailableCountries.Add(new SelectListItem() { Text = c.Name, Value = c.Id.ToString() });
            foreach (var p in _stateProvinceService.GetStateProvincesByCountryId(int.Parse(billingAddressModel.NewAddress.AvailableCountries.First().Value)))
                billingAddressModel.NewAddress.AvailableStates.Add(new SelectListItem() { Text = p.Name, Value = p.Id.ToString() });
            billingAddressModel.NewAddress.Genders.Add(new SelectListItem() { Text = _localizationService.GetResource("Account.Fields.Gender.Female"), Value = "F" });
            billingAddressModel.NewAddress.Genders.Add(new SelectListItem() { Text = _localizationService.GetResource("Account.Fields.Gender.Male"), Value = "M" });
            //existing addresses
            addresses = _workContext.CurrentCustomer.Addresses.ToList();//.Where(a => a.Country.AllowsBilling)
            foreach (var address in addresses)
            {
                var addressModel = address.ToModel();
                addressModel.AvailableCountries = billingAddressModel.NewAddress.AvailableCountries;
                if (address.CountryId.Value != int.Parse(billingAddressModel.NewAddress.AvailableCountries.First().Value))
                {
                    foreach (var p in _stateProvinceService.GetStateProvincesByCountryId(address.CountryId.Value))
                        addressModel.AvailableStates.Add(new SelectListItem() { Text = p.Name, Value = p.Id.ToString() });
                    if (addressModel.AvailableStates.Count == 0)
                    {
                        addressModel.AvailableStates.Add(new SelectListItem() { Text = _localizationService.GetResource("Address.OtherNonUS"), Value = "0" });
                    }
                }
                else
                {
                    addressModel.AvailableStates = billingAddressModel.NewAddress.AvailableStates;
                }
                addressModel.Genders = billingAddressModel.NewAddress.Genders;
                billingAddressModel.ExistingAddresses.Add(addressModel);
            }
            billingAddressModel.ExistingAddresses.Add(billingAddressModel.NewAddress);
            billingAddressModel.HasDefault =_workContext.CurrentCustomer.Addresses.FirstOrDefault(x=>x.DefaultBillingAddress) != null ;
            List<SelectListItem> billingAddressActions = new List<SelectListItem>();

            billingAddressModel.BillingAddressActions.Add(new SelectListItem() { Text = _localizationService.GetResource("Checkout.BillingAddressTheSameAsShippingAddress"),Value="S" });
            if (addresses.Count!=0)
            billingAddressModel.BillingAddressActions.Add(new SelectListItem() { Text = _localizationService.GetResource("Checkout.BillingToSelection"), Value = "O", Selected = billingAddressModel.HasDefault });
            billingAddressModel.BillingAddressActions.Add(new SelectListItem() { Text = _localizationService.GetResource("Checkout.BillingToThisAddress"), Value = "N" });

            CheckoutAddressesModel model = new CheckoutAddressesModel() { 
            CheckoutBillingAddressModel =  billingAddressModel,
            CheckoutShippingAddressModel =  shippingAddressModel,
            BillingAddressId = _workContext.CurrentCustomer.BillingAddress != null ? _workContext.CurrentCustomer.BillingAddress.Id : (int?)null,
            ShippingAddressId = _workContext.CurrentCustomer.ShippingAddress != null ? _workContext.CurrentCustomer.ShippingAddress.Id : (int?)null,

            };

            return View(model);

        }

        //AF
        [ChildActionOnly]
        public ActionResult Payment()
        {
            //Payment Methods
            //validation
            var cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();
            if (cart.Count == 0)
                return RedirectToRoute("ShoppingCart");

            if (_orderSettings.OnePageCheckoutEnabled)
                return RedirectToRoute("CheckoutOnePage");


            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return new HttpUnauthorizedResult();

            //Check whether payment workflow is required
            bool isPaymentWorkflowRequired = IsPaymentWorkflowRequired(cart);
            if (!isPaymentWorkflowRequired)
            {
                _workContext.CurrentCustomer.SelectedPaymentMethodSystemName = "";
                _customerService.UpdateCustomer(_workContext.CurrentCustomer);
                return RedirectToRoute("Checkout");
            }


            //model
            var checkoutPaymentMethodModel = new CheckoutPaymentMethodModel();

            //OrderTotal
            var OrderTotalmodel = new OrderTotalsModel();
            if (cart.Count > 0)
            {
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
                    OrderTotalmodel.OrderTotal = _priceFormatter.FormatPrice(shoppingCartTotal, false, false);
                }
            }

            //reward points
            if (_rewardPointsSettings.Enabled && !cart.IsRecurring())
            {
                int rewardPointsBalance = _workContext.CurrentCustomer.GetRewardPointsBalance();
                decimal rewardPointsAmountBase = _orderTotalCalculationService.ConvertRewardPointsToAmount(rewardPointsBalance);
                decimal rewardPointsAmount = _currencyService.ConvertFromPrimaryStoreCurrency(rewardPointsAmountBase, _workContext.WorkingCurrency);
                if (rewardPointsAmount > decimal.Zero)
                {
                    checkoutPaymentMethodModel.DisplayRewardPoints = true;
                    checkoutPaymentMethodModel.RewardPointsAmount = _priceFormatter.FormatPrice(rewardPointsAmount, true, false);
                    checkoutPaymentMethodModel.RewardPointsBalance = rewardPointsBalance;
                }
            }

            var boundPaymentMethods = _paymentService
                .LoadActivePaymentMethods()
                .Where(pm => pm.PaymentMethodType == PaymentMethodType.Standard)
                .ToList();
            foreach (var pm in boundPaymentMethods)
            {
                if (cart.IsRecurring() && pm.RecurringPaymentType == RecurringPaymentType.NotSupported)
                    continue;

                var pmModel = new CheckoutPaymentMethodModel.PaymentMethodModel()
                {
                    Name = pm.PluginDescriptor.FriendlyName,
                    PaymentMethodSystemName = pm.PluginDescriptor.SystemName,
                };
                //payment method additional fee
                decimal paymentMethodAdditionalFee = _paymentService.GetAdditionalHandlingFee(pm.PluginDescriptor.SystemName);
                decimal rateBase = _taxService.GetPaymentMethodAdditionalFee(paymentMethodAdditionalFee, _workContext.CurrentCustomer);
                decimal rate = _currencyService.ConvertFromPrimaryStoreCurrency(rateBase, _workContext.WorkingCurrency);
                if (rate > decimal.Zero)
                    pmModel.Fee = _priceFormatter.FormatPaymentMethodAdditionalFee(rate, true);

                var checkoutPaymentInfoModel = new CheckoutPaymentInfoModel();
                string actionName;
                string controllerName;
                RouteValueDictionary routeValues;
                pm.GetPaymentInfoRoute(out actionName, out controllerName, out routeValues);
                checkoutPaymentInfoModel.PaymentInfoActionName = actionName;
                checkoutPaymentInfoModel.PaymentInfoControllerName = controllerName;
                checkoutPaymentInfoModel.PaymentInfoRouteValues = routeValues;
                pmModel.CheckoutPaymentInfoModel = checkoutPaymentInfoModel;

                checkoutPaymentMethodModel.PaymentMethods.Add(pmModel);
            }

            checkoutPaymentMethodModel.OrderTotal = OrderTotalmodel.OrderTotal;

            return View(checkoutPaymentMethodModel);
        }

        public ActionResult ShippingAddress()
        {
            //validation
            var cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();
            if (cart.Count == 0)
                return RedirectToRoute("ShoppingCart");

            if (_orderSettings.OnePageCheckoutEnabled)
                return RedirectToRoute("CheckoutOnePage");

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return new HttpUnauthorizedResult();

            if (!cart.RequiresShipping())
            {
                _workContext.CurrentCustomer.SetShippingAddress(null);
                _customerService.UpdateCustomer(_workContext.CurrentCustomer);
                return RedirectToRoute("CheckoutBillingAddress");
            }

            //model
            var model = new CheckoutShippingAddressModel();
            //existing addresses
            var addresses = _workContext.CurrentCustomer.Addresses.Where(a => a.Country.AllowsShipping).ToList();
            foreach (var address in addresses)
                model.ExistingAddresses.Add(address.ToModel());

            //new address model
            model.NewAddress = new AddressModel();
            model.NewAddress.AvailableCountries.Add(new SelectListItem() { Text = _localizationService.GetResource("Address.SelectCountry"), Value = "0" });
            foreach (var c in _countryService.GetAllCountriesForShipping())
                model.NewAddress.AvailableCountries.Add(new SelectListItem() { Text = c.Name, Value = c.Id.ToString() });
            model.NewAddress.AvailableStates.Add(new SelectListItem() { Text = _localizationService.GetResource("Address.OtherNonUS"), Value = "0" });
            
            return View(model);
        }

        //AF
        //TODO:mustafa re-engineer !
        public JsonResult SaveShippingAddress(AddressModel model)
        {
            //validation
            

            var cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();
            if (cart.Count == 0)
                return Json(new { Status = 0, Message = "Empty shopping cart!" });

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return Json(new { Status = 0, Message = "Unauthorized operation!" });

            //if (ModelState.IsValid)
            {
                Address address;
                if (model.Id == 0)
                {
                    address = model.ToEntity();
                    address.CreatedOnUtc = DateTime.UtcNow;
                }
                else
                {
                    address = _workContext.CurrentCustomer.Addresses.Where(a => a.Id == model.Id).FirstOrDefault();
                    if (address == null)
                    {
                        return Json(new { Status = 0, Message = "Address info does not belong to customer!" });
                    }
                    address = model.ToEntity(address);
                }
                address.Email = _workContext.CurrentCustomer.Email;

                //some validation
                if (address.CountryId == 0)
                    address.CountryId = null;
                if (address.StateProvinceId == 0)
                    address.StateProvinceId = null;
                if (address.Id == 0)
                {
                    var addressName = _workContext.CurrentCustomer.Addresses.Where(x => x.Name.ToLower().Trim() == model.Name.ToLower().Trim()).ToList();
                    if (addressName.Count != 0)
                        return Json(new { Status = 0, Message = _localizationService.GetResource("Address.Name.AlreadyExists", _workContext.WorkingLanguage.Id) });
                    _workContext.CurrentCustomer.AddAddress(address);
                    _customerService.UpdateCustomer(_workContext.CurrentCustomer);
                }
                else
                {
                    _addressService.UpdateAddress(address);
                }
                //_workContext.CurrentCustomer.SetShippingAddress(address); 
                
                var shippingMethodModel = this.PrepareShippingMethodModel(cart);
                var orderTotalsModel = PrepareOrderTotalsModel(cart);
                if (model.Id == 0)
                {
                    model = address.ToModel();
                    foreach (var c in _countryService.GetAllCountriesForShipping())
                        model.AvailableCountries.Add(new SelectListItem() { Text = c.Name, Value = c.Id.ToString(), Selected = c.Id == address.CountryId });
                    foreach (var p in _stateProvinceService.GetStateProvincesByCountryId(int.Parse(model.AvailableCountries.First(c => c.Value == address.CountryId.Value.ToString()).Value)))
                        model.AvailableStates.Add(new SelectListItem() { Text = p.Name, Value = p.Id.ToString() });

                    model.Genders.Add(new SelectListItem() { Text = _localizationService.GetResource("Account.Fields.Gender.Female"), Value = "F" });
                    model.Genders.Add(new SelectListItem() { Text = _localizationService.GetResource("Account.Fields.Gender.Male"), Value = "M" });

                    ViewBag.IsBilling = false;
                    ViewBag.IsShipping = true;

                    return Json(new
                    {
                        Status = 1,
                        Message = _localizationService.GetResource("Address.SavedMessage"),
                        AddressId = model.Id,
                        TitleHtml = string.Format(_localizationService.GetResource("Checkout.SelectAddress"), address.Name),
                        Html = Utilities.RenderPartialViewToString(this, @"~\Views\Checkout\_address.cshtml", model),
                        ShippingMethod = Utilities.RenderPartialViewToString(this, @"~\Views\Checkout\ShippingMethod.cshtml", shippingMethodModel),
                        OrderTotal = Utilities.RenderPartialViewToString(this, @"~\Views\ShoppingCart\OrderTotals.cshtml", orderTotalsModel)
                    });
                }
                else
                {
                    return Json(new
                    {
                        Status = 2,
                        Message = _localizationService.GetResource("Address.UpdatedMessage"),
                        TitleHtml = string.Format(_localizationService.GetResource("Checkout.SelectAddress"), address.Name),
                        ShippingMethod = Utilities.RenderPartialViewToString(this, @"~\Views\Checkout\ShippingMethod.cshtml", shippingMethodModel),
                        OrderTotal = Utilities.RenderPartialViewToString(this, @"~\Views\ShoppingCart\OrderTotals.cshtml", orderTotalsModel)
                    });
                }
            }

        }

        //AF
        public JsonResult SaveBillingAddress(AddressModel model)
        {
            //validation
            
            var cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();
            if (cart.Count == 0)
                return Json(new { Status = 0, Message = "Empty shopping cart!" });

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return Json(new { Status = 0, Message = "Unauthorized operation!" });

            //if (ModelState.IsValid)
            {
                Address address;
                if (model.Id == 0)
                {
                    address = model.ToEntity();
                    address.CreatedOnUtc = DateTime.UtcNow;
                }
                else
                {
                    address = _workContext.CurrentCustomer.Addresses.Where(a => a.Id == model.Id).FirstOrDefault();
                    if (address == null)
                    {
                        return Json(new { Status = 0, Message = "Address info does not belong to customer!" });
                    }
                    address = model.ToEntity(address);
                }
                address.Email = _workContext.CurrentCustomer.Email;
                //some validation
                if (address.CountryId == 0)
                    address.CountryId = null;
                if (address.StateProvinceId == 0)
                    address.StateProvinceId = null;
                    
                if (address.Id == 0)
                {
                    var addresses = _workContext.CurrentCustomer.Addresses.ToList();
                    if (addresses.FirstOrDefault(x => x.Name.ToLower().Trim() == model.Name.ToLower().Trim()) != null)
                        return Json(new { Status = 0, Message = _localizationService.GetResource("Address.Name.AlreadyExists", _workContext.WorkingLanguage.Id) });
                    _workContext.CurrentCustomer.AddAddress(address);
                    _customerService.UpdateCustomer(_workContext.CurrentCustomer);
                }
                else
                {
                    _addressService.UpdateAddress(address);
                }
                //_workContext.CurrentCustomer.SetBillingAddress(address);
                if (model.Id == 0)
                {
                    model = address.ToModel();
                    foreach (var c in _countryService.GetAllCountriesForShipping())
                        model.AvailableCountries.Add(new SelectListItem() { Text = c.Name, Value = c.Id.ToString(), Selected = c.Id == address.CountryId });
                    foreach (var p in _stateProvinceService.GetStateProvincesByCountryId(int.Parse(model.AvailableCountries.First(c => c.Value == address.CountryId.Value.ToString()).Value)))
                        model.AvailableStates.Add(new SelectListItem() { Text = p.Name, Value = p.Id.ToString() });
                    model.Genders.Add(new SelectListItem() { Text = _localizationService.GetResource("Account.Fields.Gender.Female"), Value = "F" });
                    model.Genders.Add(new SelectListItem() { Text = _localizationService.GetResource("Account.Fields.Gender.Male"), Value = "M" });
                    ViewBag.IsBilling = true;
                    ViewBag.IsShipping = false;

                    return Json(new
                    {
                        Status = 1,
                        Message = _localizationService.GetResource("Address.SavedMessage"),
                        AddressId = model.Id,
                        TitleHtml = string.Format(_localizationService.GetResource("Checkout.SelectAddress"), address.Name),
                        Html = Utilities.RenderPartialViewToString(this, @"~\Views\Checkout\_address.cshtml", model)
                    });
                }
                else
                {
                    return Json(new
                    {
                        Status = 2,
                        Message = _localizationService.GetResource("Address.UpdatedMessage"),
                        TitleHtml = string.Format(_localizationService.GetResource("Checkout.SelectAddress"), address.Name)
                    });
                }
            }


        }

        private void SetDefaultBillingAddress(Address address)
        {
            int n = _workContext.CurrentCustomer.Addresses.Count;
            int i = 0;
            IList<Address> addresses = _workContext.CurrentCustomer.Addresses.ToList();
            for (i = 0; i < n; i++)
            {
                if (addresses[i].DefaultBillingAddress)
                {
                    break;
                }
            }
            if (i == n)
            {
                address.DefaultBillingAddress = true;
                _addressService.UpdateAddress(address);  
            }
            
            
        }

        private void SetDefaultShippingAddress(Address address)
        {
            int n = _workContext.CurrentCustomer.Addresses.Count;
            int i = 0;
            IList<Address> addresses = _workContext.CurrentCustomer.Addresses.ToList();
           

            for (i = 0; i < n; i++)
            {
                if (addresses[i].DefaultShippingAddress)
                {
                    break;
                }
            }
            if (i == n)
            {
                address.DefaultShippingAddress = true;
                _addressService.UpdateAddress(address);
            }
            
        }
        public ActionResult BillingAddress()
        {
            //validation
            var cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();
            if (cart.Count == 0)
                return RedirectToRoute("ShoppingCart");

            if (_orderSettings.OnePageCheckoutEnabled)
                return RedirectToRoute("CheckoutOnePage");

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return new HttpUnauthorizedResult();
            
            //model
            var model = new CheckoutBillingAddressModel();
            //existing addresses
            var addresses = _workContext.CurrentCustomer.Addresses.Where(a => a.Country.AllowsBilling).ToList();
            foreach (var address in addresses)
                model.ExistingAddresses.Add(address.ToModel());

            //new address model
            model.NewAddress = new AddressModel();
            model.NewAddress.AvailableCountries.Add(new SelectListItem() { Text = _localizationService.GetResource("Address.SelectCountry"), Value = "0" });
            foreach (var c in _countryService.GetAllCountriesForBilling())
                model.NewAddress.AvailableCountries.Add(new SelectListItem() { Text = c.Name, Value = c.Id.ToString() });
            model.NewAddress.AvailableStates.Add(new SelectListItem() { Text = _localizationService.GetResource("Address.OtherNonUS"), Value = "0" });

            return View(model);
        }

        //AF
        //public JsonResult SelectShippingAddress(int addressId)
        //{
        //    var address = _workContext.CurrentCustomer.Addresses.Where(a => a.Id == addressId).FirstOrDefault();
        //    if (address == null)
        //        return Json(new { Status = 0, Message = "No address found!" });

        //    _workContext.CurrentCustomer.SetShippingAddress(address);
        //    _customerService.UpdateCustomer(_workContext.CurrentCustomer);

        //    return Json(new { Status = 1 });
        //}

        //AF
        public ActionResult SelectAddress(int addressId, string selectionType)
        {
            var address = _workContext.CurrentCustomer.Addresses.Where(a => a.Id == addressId).FirstOrDefault();
            if (address == null)

                return Json(new { Status = 0, Message = "No address found!" });
            if (selectionType == "Shipping")
                _workContext.CurrentCustomer.SetShippingAddress(address);
            else if (selectionType == "Billing")
                _workContext.CurrentCustomer.SetBillingAddress(address);
            else
            {
                _workContext.CurrentCustomer.SetShippingAddress(address);
                _workContext.CurrentCustomer.SetBillingAddress(address);
            }
            _customerService.UpdateCustomer(_workContext.CurrentCustomer);

            if (selectionType == "Shipping" || selectionType == "Both")
            {
                var cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();
                var shippingMethodModel = this.PrepareShippingMethodModel(cart);
                var orderTotalsModel = PrepareOrderTotalsModel(cart);


                return Json(new
                {
                    Status = 1,
                    RefreshCheckout = true,
                    ShippingMethod = Utilities.RenderPartialViewToString(this, @"~\Views\Checkout\ShippingMethod.cshtml", shippingMethodModel),
                    OrderTotal = Utilities.RenderPartialViewToString(this, @"~\Views\ShoppingCart\OrderTotals.cshtml", orderTotalsModel)

                });
            }
            else
            {
                return Json(new
                {
                    Status = 1,
                    RefreshCheckout = false,
                });
 
            }

        }


        private  OrderTotalsModel PrepareOrderTotalsModel(IList<ShoppingCartItem> cart)
        {
            var model = new OrderTotalsModel();
            model.IsEditable = false;

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
                _orderTotalCalculationService.GetShoppingCartSubTotal(cart,false,
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
            return model;
        }



        //AF
        public ActionResult NewBillingAddress(CheckoutBillingAddressModel model)
        {
            //validation
            var cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();
            if (cart.Count == 0)
                return Json(new { Status = 0, Message = "Empty shopping cart!" });

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return Json(new {Status=0, Message="Unauthorized operation!"});

            if (ModelState.IsValid)
            {
                var address = model.NewAddress.ToEntity();
                address.Email = _workContext.CurrentCustomer.Email;
                address.CreatedOnUtc = DateTime.UtcNow;
                //some validation
                if (address.CountryId == 0)
                    address.CountryId = null;
                if (address.StateProvinceId == 0)
                    address.StateProvinceId = null;
                   
                _workContext.CurrentCustomer.AddAddress(address);
                _workContext.CurrentCustomer.SetBillingAddress(address);
                _customerService.UpdateCustomer(_workContext.CurrentCustomer);
            }
            return Json(new { Status = 1});
            }
       
        //AF
        public ActionResult ShippingMethod()
        {
            //validation
            var cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();
            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return new HttpUnauthorizedResult();
            //model
            var model = PrepareShippingMethodModel(cart);
            return View(model);
        }

        private CheckoutShippingMethodModel PrepareShippingMethodModel(IList<ShoppingCartItem> cart)
        {
            var model = new CheckoutShippingMethodModel();

            var getShippingOptionResponse = _shippingService.GetShippingOptions(cart, _workContext.CurrentCustomer.ShippingAddress);
            if (getShippingOptionResponse.Success)
            {
                foreach (var shippingOption in getShippingOptionResponse.ShippingOptions)
                {
                    var soModel = new CheckoutShippingMethodModel.ShippingMethodModel()
                    {
                        Name = shippingOption.Name,
                        Description = shippingOption.Description,
                        ShippingRateComputationMethodSystemName = shippingOption.ShippingRateComputationMethodSystemName,
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
                    soModel.Fee = _priceFormatter.FormatShippingPrice(rate, false);

                    model.ShippingMethods.Add(soModel);
                }
            }
            else
                foreach (var error in getShippingOptionResponse.Errors)
                    model.Warnings.Add(error);
            //Set default option
            _customerService.SaveCustomerAttribute<ShippingOption>(_workContext.CurrentCustomer, SystemCustomerAttributeNames.LastShippingOption, getShippingOptionResponse.ShippingOptions.First());
            return model;
        }

        [HttpPost, ActionName("ShippingMethod")]
        public JsonResult SelectShippingMethod(string shippingoption)
        {
            //validation
            var cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();
            if (cart.Count == 0)
                //return RedirectToRoute("ShoppingCart");
                return Json("");
            if (_orderSettings.OnePageCheckoutEnabled)
                //return RedirectToRoute("CheckoutOnePage");
                return Json("");
            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                //return new HttpUnauthorizedResult();
                return Json("");
            if (!cart.RequiresShipping())
            {
                _customerService.SaveCustomerAttribute<ShippingOption>(_workContext.CurrentCustomer, SystemCustomerAttributeNames.LastShippingOption, null);
                //return RedirectToRoute("CheckoutPaymentMethod");
                return Json("");
            }

            //parse selected method 
            if (String.IsNullOrEmpty(shippingoption))
                //return ShippingMethod();
                return Json("");
            var splittedOption = shippingoption.Split(new string[] { "___" }, StringSplitOptions.RemoveEmptyEntries);
            if (splittedOption.Length != 2)
                //return ShippingMethod();
                return Json("");
            string selectedName = splittedOption[0];
            string shippingRateComputationMethodSystemName = splittedOption[1];

            //find it
            var shippingOptions = _shippingService.GetShippingOptions(cart, _workContext.CurrentCustomer.ShippingAddress, shippingRateComputationMethodSystemName);
            var shippingOption = shippingOptions.ShippingOptions.ToList()
                .Find(so => !String.IsNullOrEmpty(so.Name) && so.Name.Equals(selectedName, StringComparison.InvariantCultureIgnoreCase));
            if (shippingOption == null)
                //return ShippingMethod();
                return Json("");
            //save
            _customerService.SaveCustomerAttribute<ShippingOption>(_workContext.CurrentCustomer, SystemCustomerAttributeNames.LastShippingOption, shippingOption);  
            //return RedirectToRoute("CheckoutPaymentMethod");


            string shippingTotal="";
                    decimal? shoppingCartShippingBase = _orderTotalCalculationService.GetShoppingCartShippingTotal(cart);
                    if (shoppingCartShippingBase.HasValue)
                    {
                        decimal shoppingCartShipping = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartShippingBase.Value, _workContext.WorkingCurrency);
                        shippingTotal = _priceFormatter.FormatShippingPrice(shoppingCartShipping, false);
                    }

            return Json(shippingTotal);
        }




        public ActionResult PaymentMethod()
        {
            //validation
            var cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();
            if (cart.Count == 0)
                return RedirectToRoute("ShoppingCart");

            if (_orderSettings.OnePageCheckoutEnabled)
                return RedirectToRoute("CheckoutOnePage");

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return new HttpUnauthorizedResult();

            //Check whether payment workflow is required
            bool isPaymentWorkflowRequired = IsPaymentWorkflowRequired(cart);
            if (!isPaymentWorkflowRequired)
            {
                _workContext.CurrentCustomer.SelectedPaymentMethodSystemName = "";
                _customerService.UpdateCustomer(_workContext.CurrentCustomer);
                return RedirectToRoute("Checkout");
            }

            //model
            var model = new CheckoutPaymentMethodModel();

            //reward points
            if (_rewardPointsSettings.Enabled && !cart.IsRecurring())
            {
                int rewardPointsBalance = _workContext.CurrentCustomer.GetRewardPointsBalance();
                decimal rewardPointsAmountBase = _orderTotalCalculationService.ConvertRewardPointsToAmount(rewardPointsBalance);
                decimal rewardPointsAmount = _currencyService.ConvertFromPrimaryStoreCurrency(rewardPointsAmountBase, _workContext.WorkingCurrency);
                if (rewardPointsAmount > decimal.Zero)
                {
                    model.DisplayRewardPoints = true;
                    model.RewardPointsAmount = _priceFormatter.FormatPrice(rewardPointsAmount, true, false);
                    model.RewardPointsBalance = rewardPointsBalance;
                }
            }

            var boundPaymentMethods = _paymentService
                .LoadActivePaymentMethods()
                .Where(pm => pm.PaymentMethodType == PaymentMethodType.Standard)
                .ToList();
            foreach (var pm in boundPaymentMethods)
            {
                if (cart.IsRecurring() && pm.RecurringPaymentType == RecurringPaymentType.NotSupported)
                    continue;

                var pmModel = new CheckoutPaymentMethodModel.PaymentMethodModel()
                {
                    Name = pm.PluginDescriptor.FriendlyName,
                    PaymentMethodSystemName = pm.PluginDescriptor.SystemName,
                };
                //payment method additional fee
                decimal paymentMethodAdditionalFee = _paymentService.GetAdditionalHandlingFee(pm.PluginDescriptor.SystemName);
                decimal rateBase = _taxService.GetPaymentMethodAdditionalFee(paymentMethodAdditionalFee, _workContext.CurrentCustomer);
                decimal rate = _currencyService.ConvertFromPrimaryStoreCurrency(rateBase, _workContext.WorkingCurrency);
                if (rate > decimal.Zero)
                    pmModel.Fee = _priceFormatter.FormatPaymentMethodAdditionalFee(rate, true);

                model.PaymentMethods.Add(pmModel);
            }

            return View("PaymentMethod",model);
        }

        [HttpPost, ActionName("PaymentMethod")]
        [FormValueRequired("nextstep")]
        [ValidateInput(false)]
        public ActionResult SelectPaymentMethod(string paymentmethod, CheckoutPaymentMethodModel model)
        {
            //validation
            var cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();
            if (cart.Count == 0)
                return RedirectToRoute("ShoppingCart");

            if (_orderSettings.OnePageCheckoutEnabled)
                return RedirectToRoute("CheckoutOnePage");

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return new HttpUnauthorizedResult();

            //Check whether payment workflow is required
            bool isPaymentWorkflowRequired = IsPaymentWorkflowRequired(cart);
            if (!isPaymentWorkflowRequired)
            {
                _workContext.CurrentCustomer.SelectedPaymentMethodSystemName = "";
                _customerService.UpdateCustomer(_workContext.CurrentCustomer);
                return RedirectToRoute("Checkout");
            }
            //payment method 
            if (String.IsNullOrEmpty(paymentmethod))
                return PaymentMethod();

            //reward points
            _workContext.CurrentCustomer.UseRewardPointsDuringCheckout = model.UseRewardPoints;
            _customerService.UpdateCustomer(_workContext.CurrentCustomer);

            var paymentMethodInst = _paymentService.LoadPaymentMethodBySystemName(paymentmethod);
            if (paymentMethodInst == null || !paymentMethodInst.IsPaymentMethodActive(_paymentSettings))
                return PaymentMethod();

            //save
            _workContext.CurrentCustomer.SelectedPaymentMethodSystemName = paymentmethod;
            _customerService.UpdateCustomer(_workContext.CurrentCustomer);

            return RedirectToRoute("Checkout");
        }


        public ActionResult PaymentInfo()
        {
            //validation
            var cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();
            if (cart.Count == 0)
                return RedirectToRoute("ShoppingCart");

            if (_orderSettings.OnePageCheckoutEnabled)
                return RedirectToRoute("CheckoutOnePage");

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return new HttpUnauthorizedResult();

            //Check whether payment workflow is required
            bool isPaymentWorkflowRequired = IsPaymentWorkflowRequired(cart);
            if (!isPaymentWorkflowRequired)
            {
                return RedirectToRoute("CheckoutConfirm");
            }
            
            var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(_workContext.CurrentCustomer.SelectedPaymentMethodSystemName);
            if (paymentMethod == null)
                return RedirectToRoute("CheckoutPaymentMethod");

            //model
            var model = new CheckoutPaymentInfoModel();
            string actionName;
            string controllerName;
            RouteValueDictionary routeValues;
            paymentMethod.GetPaymentInfoRoute(out actionName, out controllerName, out routeValues);
            model.PaymentInfoActionName = actionName;
            model.PaymentInfoControllerName = controllerName;
            model.PaymentInfoRouteValues = routeValues;

            return View(model);
        }

        [HttpPost, ActionName("PaymentInfo")]
        [ValidateInput(false)]
        public ActionResult EnterPaymentInfo(FormCollection form)
        {
            //validation
            var cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();
            if (cart.Count == 0)
                return Json(new { Status = 0, Message = "Empty cart!" });

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return Json(new { Status = 0, Message = "Unauthorized operation!" });

            //Billing Address is same as shipping address.
            //if (form["BillingAdressAction"] == "S")
            //{
            //    _workContext.CurrentCustomer.SetBillingAddress(_workContext.CurrentCustomer.ShippingAddress);
            //    _customerService.UpdateCustomer(_workContext.CurrentCustomer);
            //}

            //Set addresses if required
            int shippigAddressId = 0;
            int.TryParse(form["ShippingAddressId"], out shippigAddressId);
            int billingAddressId = 0;
            int.TryParse(form["BillingAddressId"], out billingAddressId);

            if (_workContext.CurrentCustomer.ShippingAddress==null || (_workContext.CurrentCustomer.ShippingAddress != null && _workContext.CurrentCustomer.ShippingAddress.Id != shippigAddressId))
            {
                var address = _workContext.CurrentCustomer.Addresses.FirstOrDefault(a => a.Id == shippigAddressId);
                if (address == null)
                    address = _workContext.CurrentCustomer.Addresses.FirstOrDefault(a => a.DefaultShippingAddress);
                _workContext.CurrentCustomer.SetShippingAddress(address);
            }

            if (_workContext.CurrentCustomer.BillingAddress == null || _workContext.CurrentCustomer.BillingAddress != null && _workContext.CurrentCustomer.BillingAddress.Id != billingAddressId)
            {
                var address = _workContext.CurrentCustomer.Addresses.FirstOrDefault(a => a.Id == billingAddressId);
                if (address == null)
                    address = _workContext.CurrentCustomer.Addresses.FirstOrDefault(a => a.DefaultBillingAddress);
                _workContext.CurrentCustomer.SetBillingAddress(address);
            }



            //Check whether payment workflow is required
            bool isPaymentWorkflowRequired = IsPaymentWorkflowRequired(cart);
            if (!isPaymentWorkflowRequired)
            {
                return Json(new { Status = 2, RedirectUrl = Url.RouteUrl("CheckoutConfirm") });
            }
            string paymentMethodSys = form["paymentMethod"];


            if (paymentMethodSys == "Payments.CC")
            {
                if ((form["Installment"] == "1" || string.IsNullOrEmpty(form["Installment"])) && form["CardCode"].Length<4)
                {
                    paymentMethodSys = "Payments.CC.IsBank"; //"Payments.CC.FinansBank";//Payments.CC.Garanti
                }
                else
                {
                    paymentMethodSys = GetPaymentMethodSystemName(form["CardNumber"]);
                }
            }

            var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(paymentMethodSys);
            if (paymentMethod == null || !paymentMethod.IsPaymentMethodActive(_paymentSettings))
                //TODO: can be moved to settings=> default CC payment pos
                //paymentMethod = _paymentService.LoadPaymentMethodBySystemName("Payments.CC.FinansBank");
                paymentMethod = _paymentService.LoadPaymentMethodBySystemName("Payments.CC.IsBank");
            if (paymentMethod == null || !paymentMethod.IsPaymentMethodActive(_paymentSettings))
                return Json(new { Status = 2, RedirectUrl = Url.Action("Index") });

            //save
            _workContext.CurrentCustomer.SelectedPaymentMethodSystemName = paymentMethodSys;

            if (paymentMethodSys == "Payments.CC.Garanti" || paymentMethodSys == "Payments.CC.KuveytTurk" || paymentMethodSys == "Payments.CC.Akbank" || paymentMethodSys == "Payments.CC.YapiKredi" || paymentMethodSys == "Payments.CC.FinansBank" || paymentMethodSys == "Payments.CC.IsBank")
            {
                if (!string.IsNullOrEmpty(form["Installment"]))
                {
                    CustomerAttribute ca = new CustomerAttribute() { Customer = _workContext.CurrentCustomer, Key = "taksit", Value = form["Installment"] };
                    _workContext.CurrentCustomer.CustomerAttributes.Add(ca);
                }
                else
                {
                    CustomerAttribute ca = new CustomerAttribute() { Customer = _workContext.CurrentCustomer, Key = "taksit", Value = "1" };
                    _workContext.CurrentCustomer.CustomerAttributes.Add(ca);
                }
            }

            _customerService.UpdateCustomer(_workContext.CurrentCustomer);

            var paymentControllerType = paymentMethod.GetControllerType();
            var paymentController = DependencyResolver.Current.GetService(paymentControllerType) as BaseNopPaymentController;
            var warnings = paymentController.ValidatePaymentForm(form);
            foreach (var warning in warnings)
                ModelState.AddModelError("", warning);
            if (ModelState.IsValid)
            {
                //get payment info
                var paymentInfo = paymentController.GetPaymentInfo(form);
                paymentInfo.PaymentMethodSystemName = paymentMethodSys;
                if (!string.IsNullOrEmpty(form["Installment"]))
                {
                    paymentInfo.Installment = Convert.ToInt32(form["Installment"]);
                }
                //session save
                this.Session["OrderPaymentInfo"] = paymentInfo;
                return Json(new { Status = 1, RedirectUrl = Url.RouteUrl("CheckoutConfirm") });
             
            }

            //If we got this far, something failed, redisplay form
            //model
            //var model = new CheckoutPaymentInfoModel();
            //string actionName;
            //string controllerName;
            //RouteValueDictionary routeValues;
            //paymentMethod.GetPaymentInfoRoute(out actionName, out controllerName, out routeValues);
            //model.PaymentInfoActionName = actionName;
            //model.PaymentInfoControllerName = controllerName;
            //model.PaymentInfoRouteValues = routeValues;
            //TempData["warnings"] = warnings;
            return Json(new { Status = 0, Messages = warnings});
        }

        private string GetPaymentMethodSystemName(string CCNo)
        {
          
            var bin = _paymentService.GetBinByCCNo(CCNo);
            if (bin == null)
                return "Payments.CC.IsBank"; //"Payments.CC.FinansBank";// "Payments.CC.KuveytTurk";
            //garanti
            if (bin.EffectiveCardType == "Bonus")
                return "Payments.CC.Garanti";
            if (bin.EffectiveCardType == "axess")
                return "Payments.CC.Akbank";
            if (bin.EffectiveCardType == "world")
                return "Payments.CC.YapiKredi";
            if (bin.EffectiveCardType == "CardFinans")
                return "Payments.CC.FinansBank";

            return "Payments.CC.IsBank"; //"Payments.CC.FinansBank";// "Payments.CC.KuveytTurk";
        }


        [AjaxOnly]
        public JsonResult GetCCEffectiveType(string CCNo)
        {
            var bin = _paymentService.GetBinByCCNo(CCNo);
            if (bin != null)
            {   
                var installmentList = new List<SelectListItem>();
                if (!string.IsNullOrWhiteSpace(bin.Installments))
                {
                    var installments = bin.Installments.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (installments.Length > 0)
                    {
                        string capital = _localizationService.GetResource("Payment.Installment.Single");
                        installmentList = new List<SelectListItem>();
                        installmentList.Add(new SelectListItem() { Text = capital, Value = "1", Selected = true });
                        foreach (var item in installments)
                        {
                            installmentList.Add(new SelectListItem() { Text = item, Value = item });
                        }
                    }
                }
                if (bin.EffectiveCardType == "world")
                {
                    var options = _paymentService.GetPaymentOptions(new ProcessPaymentRequest() { CreditCardNumber = CCNo, PaymentMethodSystemName = "Payments.CC.YapiKredi" });

                    var CCOptionsSelectItems = new List<SelectListItem>();
                    bool selected=true;
                    foreach (var KVP in options)
                    {
                        CCOptionsSelectItems.Add(new SelectListItem() { Value = KVP.Key, Text = KVP.Value, Selected=selected});
                        selected = false;
                    }

                    return Json(new { CCType = bin.EffectiveCardType, Installments = installmentList, CCOptions = CCOptionsSelectItems }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { CCType = bin.EffectiveCardType, Installments = installmentList }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { CCType = ""}, JsonRequestBehavior.AllowGet);
            }
        }


        [ChildActionOnly]
        public ActionResult CCPaymentInfo()
        {
            var model = new CCPaymentInfoModel();

            //CC types
            model.CreditCardTypes.Add(new SelectListItem()
            {
                Text = "Visa",
                Value = "Visa",
            });
            model.CreditCardTypes.Add(new SelectListItem()
            {
                Text = "Master card",
                Value = "MasterCard",
            });
            model.CreditCardTypes.Add(new SelectListItem()
            {
                Text = "Discover",
                Value = "Discover",
            });
            model.CreditCardTypes.Add(new SelectListItem()
            {
                Text = "Amex",
                Value = "Amex",
            });

            //years
            for (int i = 0; i < 15; i++)
            {
                string year = Convert.ToString(DateTime.Now.Year + i);
                model.ExpireYears.Add(new SelectListItem()
                {
                    Text = year,
                    Value = year,
                });
            }

            //months
            for (int i = 1; i <= 12; i++)
            {
                string text = (i < 10) ? "0" + i.ToString() : i.ToString();
                model.ExpireMonths.Add(new SelectListItem()
                {
                    Text = text,
                    Value = i.ToString(),
                });
            }

            //set postback values
            var form = this.Request.Form;
            model.CardholderName = form["CardholderName"];
            model.CardNumber = form["CardNumber"];
            model.CardCode = form["CardCode"];
            model.Installment = form["Installment"];
            var selectedCcType = model.CreditCardTypes.Where(x => x.Value.Equals(form["CreditCardType"], StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (selectedCcType != null)
                selectedCcType.Selected = true;
            var selectedMonth = model.ExpireMonths.Where(x => x.Value.Equals(form["ExpireMonth"], StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (selectedMonth != null)
                selectedMonth.Selected = true;
            var selectedYear = model.ExpireYears.Where(x => x.Value.Equals(form["ExpireYear"], StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (selectedYear != null)
                selectedYear.Selected = true;

            return View(model);

    }

        public ActionResult Confirm(CheckoutConfirmModel model)
        {
            //validation
            var cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();
            if (cart.Count == 0)
                return RedirectToRoute("ShoppingCart");
            ProcessPaymentRequest processPaymentRequest = this.Session["OrderPaymentInfo"] as ProcessPaymentRequest;
            if (processPaymentRequest == null)
            {
                //Check whether payment workflow is required
                if (IsPaymentWorkflowRequired(cart))
                    return RedirectToRoute("Checkout");
            }

            //Ensure billing address is set
            if (_workContext.CurrentCustomer.BillingAddress == null)
            {
                _workContext.CurrentCustomer.BillingAddress = _workContext.CurrentCustomer.ShippingAddress;
                _customerService.UpdateCustomer(_workContext.CurrentCustomer);
            }

            SetDefaultBillingAddress(_workContext.CurrentCustomer.BillingAddress);
            SetDefaultShippingAddress(_workContext.CurrentCustomer.ShippingAddress);



            if (_orderSettings.OnePageCheckoutEnabled)
                return RedirectToRoute("CheckoutOnePage");

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return new HttpUnauthorizedResult();

            //model
            //min order amount validation
            bool minOrderTotalAmountOk = _orderProcessingService.ValidateMinOrderTotalAmount(cart);
            if (!minOrderTotalAmountOk)
            {
                decimal minOrderTotalAmount = _currencyService.ConvertFromPrimaryStoreCurrency(_orderSettings.MinOrderTotalAmount, _workContext.WorkingCurrency);
                model.MinOrderTotalWarning =  string.Format(_localizationService.GetResource("Checkout.MinOrderTotalAmount"), _priceFormatter.FormatPrice(minOrderTotalAmount, true, false));
            }
            model.BillingAddress = _workContext.CurrentCustomer.BillingAddress.ToModel();
            model.ShippingAddress = _workContext.CurrentCustomer.ShippingAddress.ToModel();
            model.ShowWireTransferData = _workContext.CurrentCustomer.SelectedPaymentMethodSystemName == "Payments.PurchaseOrder";
            //model.IsPayment3DEnabled = processPaymentRequest.PaymentMethodSystemName == "Payments.CC.FinansBank";
            model.IsPayment3DEnabled = processPaymentRequest.PaymentMethodSystemName == "Payments.CC.IsBank";

            return View(model);
        }
        //public ActionResult Payment3DSecure(PreProcessPaymentResult result)
        //{


 
        //}

        //[HttpPost, ActionName("Confirm")]
        [ValidateInput(false)]
        public ActionResult ConfirmOrder()
        {
            //validation
            var cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();
            if (cart.Count == 0)
                return RedirectToRoute("ShoppingCart");

            if (_orderSettings.OnePageCheckoutEnabled)
                return RedirectToRoute("CheckoutOnePage");

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return new HttpUnauthorizedResult();

            var model = new CheckoutProgressModel();
            //model
            try
            {
                ProcessPaymentRequest processPaymentRequest = this.Session["OrderPaymentInfo"] as ProcessPaymentRequest;
                if (processPaymentRequest == null)
                {
                    //Check whether payment workflow is required
                    if (IsPaymentWorkflowRequired(cart))
                        return RedirectToRoute("Checkout");
                    else
                        processPaymentRequest = new ProcessPaymentRequest();
                }

                processPaymentRequest.CustomerId = _workContext.CurrentCustomer.Id;
                processPaymentRequest.PaymentMethodSystemName = _workContext.CurrentCustomer.SelectedPaymentMethodSystemName;
                processPaymentRequest.TrackingNumber = (_workContext.CurrentCustomer.ShippingAddress.Id + _workContext.CurrentCustomer.Orders.Count.ToString()).PadLeft(6, '0');

                ////////// Automatic 3D selection
                //var preProcessPaymentResult = _paymentService.PreProcessPayment(processPaymentRequest);
                //if (preProcessPaymentResult.RequiresRedirection)
                //{
                //    Session["PreProcessPaymentResult"] = preProcessPaymentResult;
                //    return RedirectToAction("Confirm3D", "Checkout");
                //}
                ////////// Automatic

                var placeOrderResult = _orderProcessingService.PlaceOrder(processPaymentRequest);
                this.Session["OrderPaymentInfo"] = null;
                
                if (placeOrderResult.Success)
                {
                    var postProcessPaymentRequest = new PostProcessPaymentRequest()
                    {
                        Order = placeOrderResult.PlacedOrder
                    };
                    _paymentService.PostProcessPayment(postProcessPaymentRequest);

                    string toName;
                    if (_workContext.CurrentCustomer.ShippingAddress != null)
                        toName = _workContext.CurrentCustomer.ShippingAddress.FirstName + _workContext.CurrentCustomer.ShippingAddress.LastName;
                    else if (_workContext.CurrentCustomer.BillingAddress != null)
                        toName = _workContext.CurrentCustomer.BillingAddress.FirstName + _workContext.CurrentCustomer.BillingAddress.LastName;
                    else
                        toName = "";

                    int languageId = _workContext.WorkingLanguage.Id;
                    var order = _orderService.GetOrderById(placeOrderResult.PlacedOrder.Id);
                    var tokens = new List<Token>();
                    order.CustomerTaxDisplayType = TaxDisplayType.IncludingTax;
                    _messageTokenProvider.AddOrderTokensForSalesAgreementsPdf(tokens, order, languageId);
                    _messageTokenProvider.AddCustomerTokensForSalesAgreementsPdf(tokens, order.Customer);

                    var c = PrepareTopicModel("SalesAgreementpdf");
                    string bodyReplaced = _tokenizer.Replace(c.Body, tokens, true);
                    c.Body = bodyReplaced;
                    c.MetaDescription = "";
                    c.MetaKeywords = "";
                    c.MetaTitle = "";
                    c.Title = "";
                    ViewBag.IsPopup = true;


                    string html = this.RenderPartialViewToString("TopicDetailsSalesAgreements", c);
                    //string bodyReplaced = _tokenizer.Replace(html, tokens, true);
                    int order_id = placeOrderResult.PlacedOrder.Id;
                    _pdfService.PrintOrdersSalesAggrimentToPdf(html, order_id);


                    return RedirectToRoute("CheckoutCompleted");
                }
                else
                {
                    model.Warnings = new List<string>();
                    foreach (var error in placeOrderResult.Errors)
                    {
                        model.Warnings.Add(error);
                        ModelState.AddModelError("", error);
                    }
                }
            }
            catch (Exception exc)
            {
                _logger.Warning(exc.Message, exc);
                model.Warnings.Add(exc.Message);
            }

            //If we got this far, something failed, redisplay form
            //model.BillingAddress = _workContext.CurrentCustomer.BillingAddress.ToModel();
            //model.ShippingAddress = _workContext.CurrentCustomer.ShippingAddress.ToModel();

            TempData["warnings"] = model.Warnings;
            return RedirectToRoute("Checkout");

        }
        
        public ActionResult Confirm3D(CheckoutConfirmModel model)
        {
            //validation
            var cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();
            if (cart.Count == 0)
                return RedirectToRoute("ShoppingCart");
            ProcessPaymentRequest processPaymentRequest = this.Session["OrderPaymentInfo"] as ProcessPaymentRequest;
            if (processPaymentRequest == null)
            {
                //Check whether payment workflow is required
                if (IsPaymentWorkflowRequired(cart))
                    return RedirectToRoute("Checkout");
            }
            processPaymentRequest.CustomerId = _workContext.CurrentCustomer.Id;
            processPaymentRequest.PaymentMethodSystemName = _workContext.CurrentCustomer.SelectedPaymentMethodSystemName;
            processPaymentRequest.OrderTotal = _orderProcessingService.GetPlaceOrderTotal(processPaymentRequest);
            ////3D
            var preProcessPaymentResult = _paymentService.PreProcessPayment(processPaymentRequest);
            Session["PreProcessPaymentResult"] = preProcessPaymentResult;
            ////
            if (preProcessPaymentResult == null || !preProcessPaymentResult.RequiresRedirection)
            {
                //Check whether payment workflow is required
                if (IsPaymentWorkflowRequired(cart))
                    return RedirectToRoute("Checkout");
            }

            //Ensure billing address is set
            if (_workContext.CurrentCustomer.BillingAddress == null)
            {
                _workContext.CurrentCustomer.BillingAddress = _workContext.CurrentCustomer.ShippingAddress;
                _customerService.UpdateCustomer(_workContext.CurrentCustomer);
            }
      
            if (_orderSettings.OnePageCheckoutEnabled)
                return RedirectToRoute("CheckoutOnePage");

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return new HttpUnauthorizedResult();

            //model
            //min order amount validation
            bool minOrderTotalAmountOk = _orderProcessingService.ValidateMinOrderTotalAmount(cart);
            if (!minOrderTotalAmountOk)
            {
                decimal minOrderTotalAmount = _currencyService.ConvertFromPrimaryStoreCurrency(_orderSettings.MinOrderTotalAmount, _workContext.WorkingCurrency);
                model.MinOrderTotalWarning = string.Format(_localizationService.GetResource("Checkout.MinOrderTotalAmount"), _priceFormatter.FormatPrice(minOrderTotalAmount, true, false));
            }
            model.BillingAddress = _workContext.CurrentCustomer.BillingAddress.ToModel();
            model.ShippingAddress = _workContext.CurrentCustomer.ShippingAddress.ToModel();
            model.ShowWireTransferData = _workContext.CurrentCustomer.SelectedPaymentMethodSystemName == "Payments.PurchaseOrder";
            if (processPaymentRequest.PaymentMethodSystemName == "Payments.CC.Garanti" || processPaymentRequest.PaymentMethodSystemName == "Payments.CC.KuveytTurk" || processPaymentRequest.PaymentMethodSystemName == "Payments.CC.Akbank" || processPaymentRequest.PaymentMethodSystemName == "Payments.CC.YapiKredi" || processPaymentRequest.PaymentMethodSystemName == "Payments.CC.FinansBank" || processPaymentRequest.PaymentMethodSystemName == "Payments.CC.IsBank")
            {
                model.IsPayment3DEnabled = processPaymentRequest.PaymentMethodSystemName == "Payments.CC.IsBank"; //processPaymentRequest.PaymentMethodSystemName == "Payments.CC.FinansBank";
            }
            else
            {
                model.IsPayment3DEnabled = false;
            }            
            
            //model.Title = _localizationService.GetResource("Payment.3D.Title");
            //model.Message = _localizationService.GetResource("Payment.3D.Message");
            //model.LoadingContent= _localizationService.GetResource("Payment.3D.LoadingContent");
            
            return View(model);

        }


        public ActionResult Payment3DPortal(FormCollection form)
        {
            return View();
 
        }

        public ActionResult Payment3DRequest()
        {
            //TODO:
            //Get 3d_pay request data from plugin

            //TODO:use model object!
            PreProcessPaymentResult preProcessPaymentResult = this.Session["PreProcessPaymentResult"] as PreProcessPaymentResult;

            if (preProcessPaymentResult == null)
            {
                    return RedirectToRoute("Checkout");
            }
            return View(preProcessPaymentResult);
        }

        [ValidateInput(false)]
        public ActionResult Payment3DResponse()
        {

            bool stateIsValid = true;
            string TransId = Request.Form.Get("TransId");
            String[] odemeparametreleri = new String[] { "AuthCode", "Response", "HostRefNum", "ProcReturnCode", "TransId", "ErrMsg" };
            IEnumerator e = Request.Form.GetEnumerator();
            while (e.MoveNext())
            {
                String xkey = (String)e.Current;
                String xval = Request.Form.Get(xkey);
                bool ok = true;
                for (int i = 0; i < odemeparametreleri.Length; i++)
                {
                    if (xkey.Equals(odemeparametreleri[i]))
                    {
                        ok = false;
                        break;
                    }
                }
                if (ok)
                    _logger.Information(string.Format("3D {0}:{1}=>{2}",TransId, xkey, xval));
            }

            
            _logger.Information(string.Format("3D {0}:{1}=>{2}",TransId, "TransId", TransId));
            _logger.Information(string.Format("3D {0}:{1}=>{2}",TransId, "Response", Request.Form.Get("Response")));
        String hashparams = Request.Form.Get("HASHPARAMS");
        String hashparamsval = Request.Form.Get("HASHPARAMSVAL");

            //TODO: get from payment settings
            //String storekey = "AF3005AF";
            String storekey = "S8jJ2g8N6PB"; //işbank
        String paramsval = "";
        int index1 = 0, index2 = 0;
        // hash hesaplamada kullanılacak değerler ayrıştırılıp değerleri birleştiriliyor.
        do
        {
            index2 = hashparams.IndexOf(":", index1);
            String val = Request.Form.Get(hashparams.Substring(index1, index2-index1)) == null ? "" : Request.Form.Get(hashparams.Substring(index1, index2-index1));
            paramsval += val;
            index1 = index2 + 1;
        }
        while (index1 < hashparams.Length);

        //out.println("hashparams="+hashparams+"<br/>");
        //out.println("hashparamsval="+hashparamsval+"<br/>");
        //out.println("paramsval="+paramsval+"<br/>");
        String hashval = paramsval + storekey;         //elde edilecek hash değeri için paramsval e store key ekleniyor. (işyeri anahtarı)
        String hashparam = Request.Form.Get("HASH");

        System.Security.Cryptography.SHA1 sha = new System.Security.Cryptography.SHA1CryptoServiceProvider();
        byte[] hashbytes = System.Text.Encoding.GetEncoding("ISO-8859-9").GetBytes(hashval);
        byte[] inputbytes = sha.ComputeHash(hashbytes);

        String hash = Convert.ToBase64String(inputbytes); //Güvenlik ve kontrol amaçlı oluşturulan hash

        if (!paramsval.Equals(hashparamsval) || !hash.Equals(hashparam)) //oluşturulan hash ile gelen hash ve hash parametreleri değerleri ile ayrıştırılıp edilen edilen aynı olmalı.
        {
            _logger.Information(string.Format("3D {0}:{1}", TransId, "=> Güvenlik Uyarısı. Sayısal İmza Geçerli Değil"));
            stateIsValid=false;
        }
        
        
        
        
      String mdStatus = Request.Form.Get("mdStatus"); // 3d işlemin sonucu
      _logger.Information(string.Format("3D {0}:{1}=>{2}",TransId,  "mdstatus", mdStatus));
        if(mdStatus.Equals("1") || mdStatus.Equals("2") || mdStatus.Equals("3") || mdStatus.Equals("4"))
        {
        
         for(int i=0;i<odemeparametreleri.Length;i++)
         {
               String paramname = odemeparametreleri[i];
               String paramval = Request.Form.Get(paramname);
               _logger.Information(string.Format("3D {0}:{1}=>{2}",TransId, paramname, paramval));
         }
    

            if("Approved".Equals(Request.Form.Get("Response")))
            {

                 _logger.Information(string.Format("3D {0}:{1}",TransId, "3D-secure ödeme işlemi başarılı"));
            }
            else
            {
                _logger.Information(string.Format("3D {0}:{1}",TransId,"3D-secure ödeme işlem başarısız"));
               stateIsValid = false;
            }
        }
        else
        {
            _logger.Information(string.Format("3D {0}:{1}", TransId, "3D-secure ödeme işlem başarısız"));
            stateIsValid = false;
        }

 
            var model = new CheckoutProgressModel();
            string state = "";
            try{

           
            ProcessPaymentRequest processPaymentRequest = this.Session["OrderPaymentInfo"] as ProcessPaymentRequest;
            if (stateIsValid)
            {

                var placeOrderResult = _orderProcessingService.PlaceOrder3D(processPaymentRequest);
                this.Session["OrderPaymentInfo"] = null;
                this.Session["PreProcessPaymentResult"] = null;

                if (placeOrderResult.Success)
                {
                    var postProcessPaymentRequest = new PostProcessPaymentRequest()
                    {
                        Order = placeOrderResult.PlacedOrder
                    };
                    _paymentService.PostProcessPayment(postProcessPaymentRequest);

                    //return RedirectToRoute("CheckoutCompleted");
                }
                else
                {
                    model.Warnings = new List<string>();
                    foreach (var error in placeOrderResult.Errors)
                    {
                        model.Warnings.Add(error);
                        ModelState.AddModelError("", error);
                    }
                }
            }
            else
            {
                model.Warnings.Add(_localizationService.GetResource("Checkout.3DpayError"));
                ModelState.AddModelError("", _localizationService.GetResource("Checkout.3DpayError"));
            }
            
            
            }
            catch (Exception exc)
            {
                _logger.Warning(exc.Message, exc);
                model.Warnings.Add(exc.Message);
            }

            TempData["warnings"] = model.Warnings;

            //return View(stateIsValid ? WebHelper.GetFullPath(Url.RouteUrl("CheckoutCompleted")) : WebHelper.GetFullPath(Url.RouteUrl("Checkout")));
            return View(new CheckoutPayment3DResponse() { UrlToRedirect = stateIsValid ? Url.RouteUrl("CheckoutCompleted") : Url.RouteUrl("Checkout") });
            //return RedirectToRoute("Checkout");
 

        }

        [ValidateInput(false)]
        public ActionResult akbank3DResSuccess()
        {

            bool stateIsValid = true;
            string TransId = Request.Form.Get("TransId");
            String[] odemeparametreleri = new String[] { "AuthCode", "Response", "HostRefNum", "ProcReturnCode", "TransId", "ErrMsg" };
            IEnumerator e = Request.Form.GetEnumerator();
            while (e.MoveNext())
            {
                String xkey = (String)e.Current;
                String xval = Request.Form.Get(xkey);
                bool ok = true;
                for (int i = 0; i < odemeparametreleri.Length; i++)
                {
                    if (xkey.Equals(odemeparametreleri[i]))
                    {
                        ok = false;
                        break;
                    }
                }
                if (ok)
                    _logger.Information(string.Format("3D {0}:{1}=>{2}", TransId, xkey, xval));
            }


            _logger.Information(string.Format("3D {0}:{1}=>{2}", TransId, "TransId", TransId));
            _logger.Information(string.Format("3D {0}:{1}=>{2}", TransId, "Response", Request.Form.Get("Response")));
            String hashparams = Request.Form.Get("HASHPARAMS");
            String hashparamsval = Request.Form.Get("HASHPARAMSVAL");

            //TODO: get from payment settings
            String storekey = "AF3005AF";
            String paramsval = "";
            int index1 = 0, index2 = 0;
            // hash hesaplamada kullanılacak değerler ayrıştırılıp değerleri birleştiriliyor.
            do
            {
                index2 = hashparams.IndexOf(":", index1);
                String val = Request.Form.Get(hashparams.Substring(index1, index2 - index1)) == null ? "" : Request.Form.Get(hashparams.Substring(index1, index2 - index1));
                paramsval += val;
                index1 = index2 + 1;
            }
            while (index1 < hashparams.Length);

            //out.println("hashparams="+hashparams+"<br/>");
            //out.println("hashparamsval="+hashparamsval+"<br/>");
            //out.println("paramsval="+paramsval+"<br/>");
            String hashval = paramsval + storekey;         //elde edilecek hash değeri için paramsval e store key ekleniyor. (işyeri anahtarı)
            String hashparam = Request.Form.Get("HASH");

            System.Security.Cryptography.SHA1 sha = new System.Security.Cryptography.SHA1CryptoServiceProvider();
            byte[] hashbytes = System.Text.Encoding.GetEncoding("ISO-8859-9").GetBytes(hashval);
            byte[] inputbytes = sha.ComputeHash(hashbytes);

            String hash = Convert.ToBase64String(inputbytes); //Güvenlik ve kontrol amaçlı oluşturulan hash

            if (!paramsval.Equals(hashparamsval) || !hash.Equals(hashparam)) //oluşturulan hash ile gelen hash ve hash parametreleri değerleri ile ayrıştırılıp edilen edilen aynı olmalı.
            {
                _logger.Information(string.Format("3D {0}:{1}", TransId, "=> Güvenlik Uyarısı. Sayısal İmza Geçerli Değil"));
                stateIsValid = false;
            }


            String mdStatus = Request.Form.Get("mdStatus"); // 3d işlemin sonucu
            _logger.Information(string.Format("3D {0}:{1}=>{2}", TransId, "mdstatus", mdStatus));
            if (mdStatus.Equals("1") || mdStatus.Equals("2") || mdStatus.Equals("3") || mdStatus.Equals("4"))
            {

                for (int i = 0; i < odemeparametreleri.Length; i++)
                {
                    String paramname = odemeparametreleri[i];
                    String paramval = Request.Form.Get(paramname);
                    _logger.Information(string.Format("3D {0}:{1}=>{2}", TransId, paramname, paramval));
                }


                if ("Approved".Equals(Request.Form.Get("Response")))
                {

                    _logger.Information(string.Format("3D {0}:{1}", TransId, "3D-secure ödeme işlemi başarılı"));
                }
                else
                {
                    _logger.Information(string.Format("3D {0}:{1}", TransId, "3D-secure ödeme işlem başarısız"));
                    stateIsValid = false;
                }
            }
            else
            {
                _logger.Information(string.Format("3D {0}:{1}", TransId, "3D-secure ödeme işlem başarısız"));
                stateIsValid = false;
            }


            var model = new CheckoutProgressModel();
            string state = "";
            try
            {
                ProcessPaymentRequest processPaymentRequest = this.Session["OrderPaymentInfo"] as ProcessPaymentRequest;
                if (stateIsValid)
                {

                    var placeOrderResult = _orderProcessingService.PlaceOrder3D(processPaymentRequest);
                    this.Session["OrderPaymentInfo"] = null;
                    this.Session["PreProcessPaymentResult"] = null;

                    if (placeOrderResult.Success)
                    {
                        var postProcessPaymentRequest = new PostProcessPaymentRequest()
                        {
                            Order = placeOrderResult.PlacedOrder
                        };
                        _paymentService.PostProcessPayment(postProcessPaymentRequest);

                        //return RedirectToRoute("CheckoutCompleted");
                    }
                    else
                    {
                        model.Warnings = new List<string>();
                        foreach (var error in placeOrderResult.Errors)
                        {
                            model.Warnings.Add(error);
                            ModelState.AddModelError("", error);
                        }
                    }
                }
                else
                {
                    model.Warnings.Add(_localizationService.GetResource("Checkout.3DpayError"));
                    ModelState.AddModelError("", _localizationService.GetResource("Checkout.3DpayError"));
                }


            }
            catch (Exception exc)
            {
                _logger.Warning(exc.Message, exc);
                model.Warnings.Add(exc.Message);
            }

            TempData["warnings"] = model.Warnings;

            //return View(stateIsValid ? WebHelper.GetFullPath(Url.RouteUrl("CheckoutCompleted")) : WebHelper.GetFullPath(Url.RouteUrl("Checkout")));
            return View(new CheckoutPayment3DResponse() { UrlToRedirect = stateIsValid ? Url.RouteUrl("CheckoutCompleted") : Url.RouteUrl("Checkout") });
            //return RedirectToRoute("Checkout");
 
        }

        [ValidateInput(false)]
        public ActionResult akbank3DResUnSuccess()
        {            
            bool stateIsValid = true;
            string TransId = Request.Form.Get("TransId");
            String[] odemeparametreleri = new String[] { "AuthCode", "Response", "HostRefNum", "ProcReturnCode", "TransId", "ErrMsg" };
            IEnumerator e = Request.Form.GetEnumerator();
            while (e.MoveNext())
            {
                String xkey = (String)e.Current;
                String xval = Request.Form.Get(xkey);
                bool ok = true;
                for (int i = 0; i < odemeparametreleri.Length; i++)
                {
                    if (xkey.Equals(odemeparametreleri[i]))
                    {
                        ok = false;
                        break;
                    }
                }
                if (ok)
                    _logger.Information(string.Format("3D {0}:{1}=>{2}", TransId, xkey, xval));
            }


            _logger.Information(string.Format("3D {0}:{1}=>{2}", TransId, "TransId", TransId));
            _logger.Information(string.Format("3D {0}:{1}=>{2}", TransId, "Response", Request.Form.Get("Response")));
            String hashparams = Request.Form.Get("HASHPARAMS");
            String hashparamsval = Request.Form.Get("HASHPARAMSVAL");

            //TODO: get from payment settings
            String storekey = "AF3005AF";
            String paramsval = "";
            int index1 = 0, index2 = 0;
            // hash hesaplamada kullanılacak değerler ayrıştırılıp değerleri birleştiriliyor.
            do
            {
                index2 = hashparams.IndexOf(":", index1);
                String val = Request.Form.Get(hashparams.Substring(index1, index2 - index1)) == null ? "" : Request.Form.Get(hashparams.Substring(index1, index2 - index1));
                paramsval += val;
                index1 = index2 + 1;
            }
            while (index1 < hashparams.Length);

            //out.println("hashparams="+hashparams+"<br/>");
            //out.println("hashparamsval="+hashparamsval+"<br/>");
            //out.println("paramsval="+paramsval+"<br/>");
            String hashval = paramsval + storekey;         //elde edilecek hash değeri için paramsval e store key ekleniyor. (işyeri anahtarı)
            String hashparam = Request.Form.Get("HASH");

            System.Security.Cryptography.SHA1 sha = new System.Security.Cryptography.SHA1CryptoServiceProvider();
            byte[] hashbytes = System.Text.Encoding.GetEncoding("ISO-8859-9").GetBytes(hashval);
            byte[] inputbytes = sha.ComputeHash(hashbytes);

            String hash = Convert.ToBase64String(inputbytes); //Güvenlik ve kontrol amaçlı oluşturulan hash

            if (!paramsval.Equals(hashparamsval) || !hash.Equals(hashparam)) //oluşturulan hash ile gelen hash ve hash parametreleri değerleri ile ayrıştırılıp edilen edilen aynı olmalı.
            {
                _logger.Information(string.Format("3D {0}:{1}", TransId, "=> Güvenlik Uyarısı. Sayısal İmza Geçerli Değil"));
                stateIsValid = false;
            }


            String mdStatus = Request.Form.Get("mdStatus"); // 3d işlemin sonucu
            _logger.Information(string.Format("3D {0}:{1}=>{2}", TransId, "mdstatus", mdStatus));
            if (mdStatus.Equals("1") || mdStatus.Equals("2") || mdStatus.Equals("3") || mdStatus.Equals("4"))
            {

                for (int i = 0; i < odemeparametreleri.Length; i++)
                {
                    String paramname = odemeparametreleri[i];
                    String paramval = Request.Form.Get(paramname);
                    _logger.Information(string.Format("3D {0}:{1}=>{2}", TransId, paramname, paramval));
                }


                if ("Approved".Equals(Request.Form.Get("Response")))
                {

                    _logger.Information(string.Format("3D {0}:{1}", TransId, "3D-secure ödeme işlemi başarılı"));
                }
                else
                {
                    _logger.Information(string.Format("3D {0}:{1}", TransId, "3D-secure ödeme işlem başarısız"));
                    stateIsValid = false;
                }
            }
            else
            {
                _logger.Information(string.Format("3D {0}:{1}", TransId, "3D-secure ödeme işlem başarısız"));
                stateIsValid = false;
            }


            var model = new CheckoutProgressModel();
            string state = "";
            try
            {


                ProcessPaymentRequest processPaymentRequest = this.Session["OrderPaymentInfo"] as ProcessPaymentRequest;
                if (stateIsValid)
                {

                    var placeOrderResult = _orderProcessingService.PlaceOrder3D(processPaymentRequest);
                    this.Session["OrderPaymentInfo"] = null;
                    this.Session["PreProcessPaymentResult"] = null;

                    if (placeOrderResult.Success)
                    {
                        var postProcessPaymentRequest = new PostProcessPaymentRequest()
                        {
                            Order = placeOrderResult.PlacedOrder
                        };
                        _paymentService.PostProcessPayment(postProcessPaymentRequest);

                        //return RedirectToRoute("CheckoutCompleted");
                    }
                    else
                    {
                        model.Warnings = new List<string>();
                        foreach (var error in placeOrderResult.Errors)
                        {
                            model.Warnings.Add(error);
                            ModelState.AddModelError("", error);
                        }
                    }
                }
                else
                {
                    model.Warnings.Add(_localizationService.GetResource("Checkout.3DpayError"));
                    ModelState.AddModelError("", _localizationService.GetResource("Checkout.3DpayError"));
                }


            }
            catch (Exception exc)
            {
                _logger.Warning(exc.Message, exc);
                model.Warnings.Add(exc.Message);
            }

            TempData["warnings"] = model.Warnings;

            //return View(stateIsValid ? WebHelper.GetFullPath(Url.RouteUrl("CheckoutCompleted")) : WebHelper.GetFullPath(Url.RouteUrl("Checkout")));
            return View(new CheckoutPayment3DResponse() { UrlToRedirect = stateIsValid ? Url.RouteUrl("CheckoutCompleted") : Url.RouteUrl("Checkout") });
            //return RedirectToRoute("Checkout");


        }

        [ValidateInput(false)]
        public ActionResult garanti3DResSuccess(FormCollection form)
        {

            bool stateIsValid = true;
            var SipDetay = new StringBuilder(); //Sanal pos detaylarını tutar

            //var processor = _paymentService.LoadPaymentMethodBySystemName("Payments.AllBanksTurkiye") as AllBanksTurkiyePaymentProcessor;
            //if (processor == null ||
            //    !processor.IsPaymentMethodActive(_paymentSettings) || !processor.PluginDescriptor.Installed)
            //    throw new NopException("Garanti module cannot be loaded");

            string xid = form.Get("xid");
            string mdStatus = form.Get("mdStatus");
            string mderrormessage = form.Get("mderrormessage");
            string txnstatus = form.Get("txnstatus");
            string txntype = form.Get("txntype");
            string authcode = form.Get("authcode");
            string response = form.Get("response");
            string txnamount = form.Get("txnamount");
            string MaskedPan = form.Get("MaskedPan");
            string errmsg = form.Get("errmsg");
            string hostmsg = form.Get("hostmsg");
            string oid = form.Get("oid");

            SipDetay.AppendLine("oid: " + oid);
            SipDetay.AppendLine("xid :" + xid);
            SipDetay.AppendLine("mdStatus :" + mdStatus);
            SipDetay.AppendLine("mderrormessage :" + mderrormessage);
            SipDetay.AppendLine("txnstatus :" + txnstatus);
            SipDetay.AppendLine("txntype :" + txntype);
            SipDetay.AppendLine("authcode (ödeme işlem onay kodu): " + authcode);
            SipDetay.AppendLine("response (Ödeme işlemi sonucu başarılı ödeme için Approved veya başarısız işlem için Declined değeri alır.): " + response);
            SipDetay.AppendLine("txnamount :" + txnamount);
            SipDetay.AppendLine("MaskedPan :" + MaskedPan);
            SipDetay.AppendLine("errmsg (Hata Mesajı): " + errmsg);
            SipDetay.AppendLine("hostmsg (Host Mesajı): " + hostmsg);

            int _oid = int.Parse(oid);
            if (oid == "-hata-" || oid == "")
            {
                // ORDER GUID YOK.
                _logger.Error("Garanti3dSuccess sayfasında bankadan dönen değer hatalı. 'oid' bulunamadı.");
                //ViewBag.Message = "Bankadan dönen değer hatalı. Order Id bulunamadı. Tekrar deneyin.";
                //return View("Nop.Plugin.Payments.AllBanksTurkiye.Views.PaymentAllBanksTurkiye.Garanti3dError");
                stateIsValid = false;
            }
            else
            {
                // oid değeri bankadan döndü.
            }

            var model = new CheckoutProgressModel();
            string state = "";
            try
            {
                ProcessPaymentRequest processPaymentRequest = this.Session["OrderPaymentInfo"] as ProcessPaymentRequest;
                if (stateIsValid)
                {

                    var placeOrderResult = _orderProcessingService.PlaceOrder3D(processPaymentRequest);
                    this.Session["OrderPaymentInfo"] = null;
                    this.Session["PreProcessPaymentResult"] = null;

                    if (placeOrderResult.Success)
                    {
                        var postProcessPaymentRequest = new PostProcessPaymentRequest()
                        {
                            Order = placeOrderResult.PlacedOrder
                        };
                        _paymentService.PostProcessPayment(postProcessPaymentRequest);
                    }
                    else
                    {
                        model.Warnings = new List<string>();
                        foreach (var error in placeOrderResult.Errors)
                        {
                            model.Warnings.Add(error);
                            ModelState.AddModelError("", error);
                        }
                    }
                }
                else
                {
                    model.Warnings.Add(_localizationService.GetResource("Checkout.3DpayError"));
                    ModelState.AddModelError("", _localizationService.GetResource("Checkout.3DpayError"));
                }


            }
            catch (Exception exc)
            {
                _logger.Warning(exc.Message, exc);
                model.Warnings.Add(exc.Message);
            }

            TempData["warnings"] = model.Warnings;

            //return View(stateIsValid ? WebHelper.GetFullPath(Url.RouteUrl("CheckoutCompleted")) : WebHelper.GetFullPath(Url.RouteUrl("Checkout")));
            return View(new CheckoutPayment3DResponse() { UrlToRedirect = stateIsValid ? Url.RouteUrl("CheckoutCompleted") : Url.RouteUrl("Checkout") });
            //return RedirectToRoute("Checkout");
 

            

        }

        [ValidateInput(false)]
        public ActionResult garanti3DResUnSuccess()
        {

            bool stateIsValid = true;
            string TransId = Request.Form.Get("TransId");
            String[] odemeparametreleri = new String[] { "AuthCode", "Response", "HostRefNum", "ProcReturnCode", "TransId", "ErrMsg" };
            IEnumerator e = Request.Form.GetEnumerator();
            while (e.MoveNext())
            {
                String xkey = (String)e.Current;
                String xval = Request.Form.Get(xkey);
                bool ok = true;
                for (int i = 0; i < odemeparametreleri.Length; i++)
                {
                    if (xkey.Equals(odemeparametreleri[i]))
                    {
                        ok = false;
                        break;
                    }
                }
                if (ok)
                    _logger.Information(string.Format("3D {0}:{1}=>{2}", TransId, xkey, xval));
            }


            _logger.Information(string.Format("3D {0}:{1}=>{2}", TransId, "TransId", TransId));
            _logger.Information(string.Format("3D {0}:{1}=>{2}", TransId, "Response", Request.Form.Get("Response")));
            String hashparams = Request.Form.Get("HASHPARAMS");
            String hashparamsval = Request.Form.Get("HASHPARAMSVAL");

            //TODO: get from payment settings
            String storekey = "AF3005AF";
            String paramsval = "";
            int index1 = 0, index2 = 0;
            // hash hesaplamada kullanılacak değerler ayrıştırılıp değerleri birleştiriliyor.
            do
            {
                index2 = hashparams.IndexOf(":", index1);
                String val = Request.Form.Get(hashparams.Substring(index1, index2 - index1)) == null ? "" : Request.Form.Get(hashparams.Substring(index1, index2 - index1));
                paramsval += val;
                index1 = index2 + 1;
            }
            while (index1 < hashparams.Length);

            //out.println("hashparams="+hashparams+"<br/>");
            //out.println("hashparamsval="+hashparamsval+"<br/>");
            //out.println("paramsval="+paramsval+"<br/>");
            String hashval = paramsval + storekey;         //elde edilecek hash değeri için paramsval e store key ekleniyor. (işyeri anahtarı)
            String hashparam = Request.Form.Get("HASH");

            System.Security.Cryptography.SHA1 sha = new System.Security.Cryptography.SHA1CryptoServiceProvider();
            byte[] hashbytes = System.Text.Encoding.GetEncoding("ISO-8859-9").GetBytes(hashval);
            byte[] inputbytes = sha.ComputeHash(hashbytes);

            String hash = Convert.ToBase64String(inputbytes); //Güvenlik ve kontrol amaçlı oluşturulan hash

            if (!paramsval.Equals(hashparamsval) || !hash.Equals(hashparam)) //oluşturulan hash ile gelen hash ve hash parametreleri değerleri ile ayrıştırılıp edilen edilen aynı olmalı.
            {
                _logger.Information(string.Format("3D {0}:{1}", TransId, "=> Güvenlik Uyarısı. Sayısal İmza Geçerli Değil"));
                stateIsValid = false;
            }


            String mdStatus = Request.Form.Get("mdStatus"); // 3d işlemin sonucu
            _logger.Information(string.Format("3D {0}:{1}=>{2}", TransId, "mdstatus", mdStatus));
            if (mdStatus.Equals("1") || mdStatus.Equals("2") || mdStatus.Equals("3") || mdStatus.Equals("4"))
            {

                for (int i = 0; i < odemeparametreleri.Length; i++)
                {
                    String paramname = odemeparametreleri[i];
                    String paramval = Request.Form.Get(paramname);
                    _logger.Information(string.Format("3D {0}:{1}=>{2}", TransId, paramname, paramval));
                }


                if ("Approved".Equals(Request.Form.Get("Response")))
                {

                    _logger.Information(string.Format("3D {0}:{1}", TransId, "3D-secure ödeme işlemi başarılı"));
                }
                else
                {
                    _logger.Information(string.Format("3D {0}:{1}", TransId, "3D-secure ödeme işlem başarısız"));
                    stateIsValid = false;
                }
            }
            else
            {
                _logger.Information(string.Format("3D {0}:{1}", TransId, "3D-secure ödeme işlem başarısız"));
                stateIsValid = false;
            }


            var model = new CheckoutProgressModel();
            string state = "";
            try
            {


                ProcessPaymentRequest processPaymentRequest = this.Session["OrderPaymentInfo"] as ProcessPaymentRequest;
                if (stateIsValid)
                {

                    var placeOrderResult = _orderProcessingService.PlaceOrder3D(processPaymentRequest);
                    this.Session["OrderPaymentInfo"] = null;
                    this.Session["PreProcessPaymentResult"] = null;

                    if (placeOrderResult.Success)
                    {
                        var postProcessPaymentRequest = new PostProcessPaymentRequest()
                        {
                            Order = placeOrderResult.PlacedOrder
                        };
                        _paymentService.PostProcessPayment(postProcessPaymentRequest);

                        //return RedirectToRoute("CheckoutCompleted");
                    }
                    else
                    {
                        model.Warnings = new List<string>();
                        foreach (var error in placeOrderResult.Errors)
                        {
                            model.Warnings.Add(error);
                            ModelState.AddModelError("", error);
                        }
                    }
                }
                else
                {
                    model.Warnings.Add(_localizationService.GetResource("Checkout.3DpayError"));
                    ModelState.AddModelError("", _localizationService.GetResource("Checkout.3DpayError"));
                }


            }
            catch (Exception exc)
            {
                _logger.Warning(exc.Message, exc);
                model.Warnings.Add(exc.Message);
            }

            TempData["warnings"] = model.Warnings;

            //return View(stateIsValid ? WebHelper.GetFullPath(Url.RouteUrl("CheckoutCompleted")) : WebHelper.GetFullPath(Url.RouteUrl("Checkout")));
            return View(new CheckoutPayment3DResponse() { UrlToRedirect = stateIsValid ? Url.RouteUrl("CheckoutCompleted") : Url.RouteUrl("Checkout") });
            //return RedirectToRoute("Checkout");


        }

        [ValidateInput(false)]
        public ActionResult ykb3DResponse(FormCollection form)
        {

            bool stateIsValid = true;

            C_PosnetOOSTDS posnetOOSTDSObj = new C_PosnetOOSTDS();

            string merchantPacket = null;
            string bankPacket = null;
            string sign = null;
            string CCPrefix = null;
            string tranType = null;

            int oid = 0;

            merchantPacket = form.Get("MerchantPacket");
            bankPacket = form.Get("BankPacket");
            sign = form.Get("Sign");
            CCPrefix = form.Get("CCPrefix");
            tranType = form.Get("TranType");

            //TODO: get from payment settings
            //posnetOOSTDSObj.SetMid(ykb3dPaymentSettings.MERCHANT_ID);
            //posnetOOSTDSObj.SetTid(ykb3dPaymentSettings.TERMINAL_ID);
            //posnetOOSTDSObj.SetPosnetID(ykb3dPaymentSettings.POSNET_ID);
            //posnetOOSTDSObj.SetKey(ykb3dPaymentSettings.ENCKEY);
            //posnetOOSTDSObj.SetURL(ykb3dPaymentSettings.XML_SERVICE_URL);

            posnetOOSTDSObj.SetMid(_settingService.GetSettingByKey<string>("yapikredipaymentsettings.merchantid"));
            posnetOOSTDSObj.SetTid(_settingService.GetSettingByKey<string>("yapikredipaymentsettings.terminalid"));
            posnetOOSTDSObj.SetPosnetID(_settingService.GetSettingByKey<string>("yapikredipaymentsettings.posnetid"));
            posnetOOSTDSObj.SetKey(_settingService.GetSettingByKey<string>("yapikredipaymentsettings.enckey"));
            posnetOOSTDSObj.SetURL(_settingService.GetSettingByKey<string>("yapikredipaymentsettings.hostaddress3d"));

            bool result;
            result = posnetOOSTDSObj.CheckAndResolveMerchantData(merchantPacket, bankPacket, sign);

            try
            {
                oid = int.Parse(posnetOOSTDSObj.GetXID());
            }
            catch
            {
                oid = 0;
            }
            finally
            {
            }

            var model = new CheckoutProgressModel();

            //Başarısız ise
            if (!result)
            {
                _logger.Error("YKB 3D dönüşü - Merchant datası cozulemedi. Müşteri Id :" + _workContext.CurrentCustomer.Id.ToString());
                
                _logger.Error("YKB 3D dönüşü - merchantPacket :" + merchantPacket);
                _logger.Error("YKB 3D dönüşü - BankPacket :" + bankPacket);
                _logger.Error("YKB 3D dönüşü - Sign :" + sign);
                _logger.Error("YKB 3D dönüşü - CCPrefix :" + CCPrefix);
                _logger.Error("YKB 3D dönüşü - TranType :" + tranType);
                
                stateIsValid = false;
                model.Warnings.Add(_localizationService.GetResource("Checkout.3DpayError"));
                ModelState.AddModelError("", _localizationService.GetResource("Checkout.3DpayError"));

                _logger.Error("Merchant datası cozulemedi. - Error Message : '" + posnetOOSTDSObj.GetResponseText() + "' - Error Code : '" + posnetOOSTDSObj.GetResponseCode() + "'");
                //return View("Nop.Plugin.Payments.AllBanksTurkiye.Views.PaymentAllBanksTurkiye.Garanti3dError");

                TempData["warnings"] = model.Warnings;

                //return View(stateIsValid ? WebHelper.GetFullPath(Url.RouteUrl("CheckoutCompleted")) : WebHelper.GetFullPath(Url.RouteUrl("Checkout")));
                return View(new CheckoutPayment3DResponse() { UrlToRedirect = stateIsValid ? Url.RouteUrl("CheckoutCompleted") : Url.RouteUrl("Checkout") });
                //return RedirectToRoute("Checkout");
            }//Başarılı ise            
            else
            {
                // true döndermesi durumunda başarılı
                result = false;

                result = posnetOOSTDSObj.ConnectAndDoTDSTransaction(merchantPacket, bankPacket, sign);

                if (!result)
                {
                    stateIsValid = false;
                    _logger.Error(string.Format("Finansallaştırma başarısız. ResponseCode: {0}, ResponseText: {1}", posnetOOSTDSObj.GetResponseCode(), posnetOOSTDSObj.GetResponseText()));
                }
                else
                {
                    switch (posnetOOSTDSObj.GetApprovedCode())
                    {
                        case "0":
                            {
                                // [Onaylanmadı]
                                stateIsValid = false;
                                _logger.Error(string.Format("Onaylanmadı. ApproveCode: 0, ResponseCode: {0}, ResponseText: {1}", posnetOOSTDSObj.GetResponseCode(), posnetOOSTDSObj.GetResponseText()));
                            }
                            break;
                        case "1":
                            {
                                // [Onaylandı]
                                stateIsValid = true;
                                _logger.Information(string.Format("Onaylandı. ApproveCode: 1, ResponseCode: {0}, ResponseText: {1}", posnetOOSTDSObj.GetResponseCode(), posnetOOSTDSObj.GetResponseText()));
                            }
                            break;
                        case "2":
                            {
                                // [Daha önce onaylanmış...]
                                stateIsValid = true;
                                _logger.Information(string.Format("Daha önce onaylanmış. ApproveCode: 2, ResponseCode: {0}, ResponseText: {1}", posnetOOSTDSObj.GetResponseCode(), posnetOOSTDSObj.GetResponseText()));
                            }
                            break;
                        default:
                            {
                                // [Onaylanmadı]
                                stateIsValid = false;
                                _logger.Error(string.Format("Onaylanmadı. ResponseCode: {0}, ResponseText: {1}", posnetOOSTDSObj.GetResponseCode(), posnetOOSTDSObj.GetResponseText()));
                            }
                            break;
                    }

                    _logger.Information(string.Format("Onay bilgisi : {0}", posnetOOSTDSObj.GetApprovedCode()));
                    _logger.Information(string.Format("Onay kodu : {0}", posnetOOSTDSObj.GetAuthcode()));
                    _logger.Information(string.Format("Referans Numarası : {0}", posnetOOSTDSObj.GetHostlogkey()));
                    _logger.Information(string.Format("Hata kodu : {0}", posnetOOSTDSObj.GetResponseCode()));
                    _logger.Information(string.Format("Hata Mesajı : {0}", posnetOOSTDSObj.GetResponseText()));

                    _logger.Information(string.Format("Taksit Sayısı : {0}", posnetOOSTDSObj.GetInstalmentNumber()));
                    _logger.Information(string.Format("Taksit Tutarı : {0}", posnetOOSTDSObj.GetInstalmentAmount()));

                    _logger.Information(string.Format("Vade Farkı Tutarı : {0}", posnetOOSTDSObj.GetVFTAmount()));
                    _logger.Information(string.Format("Vade Gün Sayısı : {0}", posnetOOSTDSObj.GetVFTDayCount()));  
                }
                
            }

            //var model = new CheckoutProgressModel();
            string state = "";
            try
            {
                ProcessPaymentRequest processPaymentRequest = this.Session["OrderPaymentInfo"] as ProcessPaymentRequest;
                if (stateIsValid)
                {

                    var placeOrderResult = _orderProcessingService.PlaceOrder3D(processPaymentRequest);
                    this.Session["OrderPaymentInfo"] = null;
                    this.Session["PreProcessPaymentResult"] = null;

                    if (placeOrderResult.Success)
                    {
                        var postProcessPaymentRequest = new PostProcessPaymentRequest()
                        {
                            Order = placeOrderResult.PlacedOrder
                        };
                        _paymentService.PostProcessPayment(postProcessPaymentRequest);
                    }
                    else
                    {
                        model.Warnings = new List<string>();
                        foreach (var error in placeOrderResult.Errors)
                        {
                            model.Warnings.Add(error);
                            ModelState.AddModelError("", error);
                        }
                    }
                }
                else
                {
                    model.Warnings.Add(_localizationService.GetResource("Checkout.3DpayError"));
                    ModelState.AddModelError("", _localizationService.GetResource("Checkout.3DpayError"));
                }


            }
            catch (Exception exc)
            {
                _logger.Warning(exc.Message, exc);
                model.Warnings.Add(exc.Message);
            }

                        
            TempData["warnings"] = model.Warnings;

            //return View(stateIsValid ? WebHelper.GetFullPath(Url.RouteUrl("CheckoutCompleted")) : WebHelper.GetFullPath(Url.RouteUrl("Checkout")));
            return View(new CheckoutPayment3DResponse() { UrlToRedirect = stateIsValid ? Url.RouteUrl("CheckoutCompleted") : Url.RouteUrl("Checkout") });
            //return RedirectToRoute("Checkout");
        }


        public ActionResult Completed()
        {
            //validation
            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return new HttpUnauthorizedResult();

            //model
            var model = new CheckoutCompletedModel();
            Order lastOrder;

            var orders = _orderService.GetOrdersByCustomerId(_workContext.CurrentCustomer.Id);
            if (orders.Count == 0)
                return RedirectToAction("Index", "Home");
            else
            {
                lastOrder = orders[0];
                model.OrderId = lastOrder.Id;
            }


            // GOOGLE ANALYTICS CONVERSION SCRIPTINI WIDGET PLUGIN CALISMADIGI ICIN BURAYA YERLESTIRIYORUM

            var sb = new StringBuilder();
            sb.AppendLine("<script type=\"text/javascript\">");
            sb.AppendLine("ga('require', 'ecommerce', 'ecommerce.js');");

            sb.AppendLine("ga('ecommerce:addTransaction', {");
            sb.AppendLine("'id': '" + model.OrderId + "',");
            sb.AppendLine("'affiliation': 'Alwaysfashion',");
            sb.AppendLine("'revenue': '" + lastOrder.OrderTotal.ToString("0.##") + "',");
            sb.AppendLine("'shipping': '" + lastOrder.OrderShippingInclTax.ToString("0.##") + "',");
            sb.AppendLine("'tax': '" + lastOrder.OrderTax.ToString("0.##") + "'");
            sb.AppendLine("});");

            foreach (OrderProductVariant x in lastOrder.OrderProductVariants)
            {
                string _cat = "";
                try
                {
                    _cat = x.ProductVariant.Product.GetDefaultProductCategory().Name;
                }
                catch 
                {
                    _cat = "Belirtilmedi";
                }

                sb.AppendLine("ga('ecommerce:addItem', {");
                sb.AppendLine("'id': '" + model.OrderId + "',");
                sb.AppendLine("'name': '" + x.ProductVariant.Product.Name + "',");
                sb.AppendLine("'sku': '" + x.ProductVariant.Sku + "',");
                sb.AppendLine("'category': '" + _cat + "',");
                sb.AppendLine("'price': '" + x.PriceInclTax.ToString("0.##") + "',");
                sb.AppendLine("'quantity': '" + x.Quantity.ToString() + "'");
                sb.AppendLine("});");
            }

            #region OrderDetails
            ////purchased products
            //var orderProductVariants = _orderService.GetAllOrderProductVariants(lastOrder.Id, null, null, null, null, null, null);
            //foreach (var opv in orderProductVariants)
            //{
            //    //var opvModel = new OrderDetailsModel.OrderProductVariantModel()
            //    //{
            //    //    Id = opv.Id,
            //    //    Sku = opv.ProductVariant.Sku,
            //    //    ProductId = opv.ProductVariant.ProductId,
            //    //    ProductSeName = opv.ProductVariant.Product.GetSeName(),
            //    //    Quantity = opv.Quantity,
            //    //    AttributeInfo = opv.AttributeDescription,
            //    //};

            //    //product name
            //    if (!String.IsNullOrEmpty(opv.ProductVariant.GetLocalized(x => x.Name)))
            //        opvModel.ProductName = string.Format("{0} ({1})", opv.ProductVariant.Product.GetLocalized(x => x.Name), opv.ProductVariant.GetLocalized(x => x.Name));
            //    else
            //        opvModel.ProductName = opv.ProductVariant.Product.GetLocalized(x => x.Name);
            //    model.Items.Add(opvModel);

            //    //unit price, subtotal
            //    switch (order.CustomerTaxDisplayType)
            //    {
            //        case TaxDisplayType.ExcludingTax:
            //            {
            //                var opvUnitPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(opv.UnitPriceExclTax, order.CurrencyRate);
            //                opvModel.UnitPrice = _priceFormatter.FormatPrice(opvUnitPriceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, false);

            //                var opvPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(opv.PriceExclTax, order.CurrencyRate);
            //                opvModel.SubTotal = _priceFormatter.FormatPrice(opvPriceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, false);
            //            }
            //            break;
            //        case TaxDisplayType.IncludingTax:
            //            {
            //                var opvUnitPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(opv.UnitPriceInclTax, order.CurrencyRate);
            //                opvModel.UnitPrice = _priceFormatter.FormatPrice(opvUnitPriceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, true);

            //                var opvPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(opv.PriceInclTax, order.CurrencyRate);
            //                opvModel.SubTotal = _priceFormatter.FormatPrice(opvPriceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, true);
            //            }
            //            break;
            //    }
            //    //picture
            //    var picture = opv.ProductVariant.GetDefaultProductVariantPicture(_pictureService);
            //    if (picture == null)
            //    {
            //        picture = _pictureService.GetPicturesByProductId(opv.ProductVariant.Product.Id, 1).FirstOrDefault();
            //    }

            //    var pictureModel = new PictureModel()
            //    {
            //        ImageUrl = _pictureService.GetPictureUrl(picture, _mediaSettings.CartThumbPictureSize, true),
            //        Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat"), opv.ProductVariant.Product.Name),
            //        AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat"), opv.ProductVariant.Name),

            //    };
            //    opvModel.Picture = pictureModel;
            //}
            #endregion

            sb.AppendLine("ga('ecommerce:send');");
            sb.AppendLine("</script>");
            model.GgConversionScript = sb.ToString();


            return View(model);
        }



        [ChildActionOnly]
        public ActionResult CCVInfo(string systemName)
        {

            var topic = _topicService.GetTopicBySystemName(systemName);
            if (topic == null)
                return Content("");

            var model = topic.ToModel();
            if (model.IsPasswordProtected)
            {
                model.Title = string.Empty;
                model.Body = string.Empty;
            }

            return PartialView(model);
        }


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

        #endregion
    }
}
