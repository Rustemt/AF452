using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Services.Authentication;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Forums;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Tax;
using Nop.Services.Seo;
using Nop.Web.Extensions;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Security;
using Nop.Web.Framework.Security.Captcha;
using Nop.Web.Models.Common;
using Nop.Web.Models.Customer;
using Nop.Web.Models.Media;
using Nop.Web.Models.Newsletter;
using Nop.Web.Models.Order;
using Nop.Core.Domain.Discounts;
using System.Text.RegularExpressions;
using Nop.Services.Logging;
using Nop.Core.Infrastructure;
using System.Text;
using System.Net;
using System.Web.Hosting;


namespace Nop.Web.Controllers
{
    //AF
    public class CustomerController : BaseNopController
    {
        #region Fields
        private readonly IAuthenticationService _authenticationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly DateTimeSettings _dateTimeSettings;
        private readonly TaxSettings _taxSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly ITaxService _taxService;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly ForumSettings _forumSettings;
        private readonly OrderSettings _orderSettings;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IPictureService _pictureService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IForumService _forumService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ICustomerRegistrationService _customerRegistrationService;

        private readonly MediaSettings _mediaSettings;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly CaptchaSettings _captchaSettings;
        private readonly PdfSettings _pdfSettings;
        private readonly MeasureSettings _measureSettings;
        private readonly IMeasureService _measureService;
        private readonly IPaymentService _paymentService;
        private readonly IProductService _productService;
        private readonly IWebHelper _webHelper;

        #endregion

        #region Ctor

        public CustomerController(IAuthenticationService authenticationService,
            IDateTimeHelper dateTimeHelper,
            DateTimeSettings dateTimeSettings, TaxSettings taxSettings,
            ILocalizationService localizationService,
            IWorkContext workContext, ICustomerService customerService,
            ICustomerRegistrationService customerRegistrationService,
            ITaxService taxService, RewardPointsSettings rewardPointsSettings,
            CustomerSettings customerSettings, ForumSettings forumSettings,
            OrderSettings orderSettings, IAddressService addressService,
            ICountryService countryService, IStateProvinceService stateProvinceService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IOrderProcessingService orderProcessingService, IOrderService orderService,
            ICurrencyService currencyService, IPriceFormatter priceFormatter,
            IPictureService pictureService, INewsLetterSubscriptionService newsLetterSubscriptionService,
            IForumService forumService, IShoppingCartService shoppingCartService,
            MediaSettings mediaSettings,
            IWorkflowMessageService workflowMessageService, LocalizationSettings localizationSettings,
            CaptchaSettings captchaSettings,
            MeasureSettings measureSettings,
            PdfSettings pdfSettings,
            IMeasureService measureService,
            IPaymentService paymentService, IProductService productService, IWebHelper webHelper)
        {
            this._authenticationService = authenticationService;
            this._dateTimeHelper = dateTimeHelper;
            this._dateTimeSettings = dateTimeSettings;
            this._taxSettings = taxSettings;
            this._localizationService = localizationService;
            this._workContext = workContext;
            this._customerService = customerService;
            this._taxService = taxService;
            this._rewardPointsSettings = rewardPointsSettings;
            this._customerSettings = customerSettings;
            this._forumSettings = forumSettings;
            this._orderSettings = orderSettings;
            this._addressService = addressService;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._orderProcessingService = orderProcessingService;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._orderService = orderService;
            this._currencyService = currencyService;
            this._priceFormatter = priceFormatter;
            this._pictureService = pictureService;
            this._newsLetterSubscriptionService = newsLetterSubscriptionService;
            this._forumService = forumService;
            this._shoppingCartService = shoppingCartService;
            this._customerRegistrationService = customerRegistrationService;
            this._mediaSettings = mediaSettings;
            this._workflowMessageService = workflowMessageService;
            this._localizationSettings = localizationSettings;
            this._captchaSettings = captchaSettings;
            this._pdfSettings = pdfSettings;
            this._measureSettings = measureSettings;
            this._measureService = measureService;
            this._paymentService = paymentService;
            this._productService = productService;
            this._webHelper = webHelper;
        }

        #endregion

        #region Utilities

        [NonAction]
        private bool IsCurrentUserRegistered()
        {
            return _workContext.CurrentCustomer.IsRegistered();
        }

        [NonAction]
        private CustomerNavigationModel GetCustomerNavigationModel(Customer customer)
        {
            var model = new CustomerNavigationModel();
            model.HideAvatar = !_customerSettings.AllowCustomersToUploadAvatars;
            model.HideRewardPoints = !_rewardPointsSettings.Enabled;
            model.HideForumSubscriptions = !_forumSettings.ForumsEnabled || !_forumSettings.AllowCustomersToManageSubscriptions;
            model.HideReturnRequests = !_orderSettings.ReturnRequestsEnabled || _orderService.SearchReturnRequests(customer.Id, 0, null).Count == 0;
            model.HideDownloadableProducts = _customerSettings.HideDownloadableProductsTab;
            return model;
        }


        [NonAction]
        private OrderDetailsModel PrepareOrderDetailsModel(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");
            var model = new OrderDetailsModel();

            model.Id = order.Id;
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc);
            model.OrderStatus = order.OrderStatus.GetLocalizedEnum(_localizationService, _workContext);
            model.Status = order.OrderStatus;
            model.IsReOrderAllowed = _orderSettings.IsReOrderAllowed;
            model.IsReturnRequestAllowed = _orderProcessingService.IsReturnRequestAllowed(order);
            model.DisplayPdfInvoice = _pdfSettings.Enabled;

            //shipping info
            model.ShippingStatus = order.ShippingStatus.GetLocalizedEnum(_localizationService, _workContext);
            if (order.ShippingStatus != ShippingStatus.ShippingNotRequired)
            {
                model.IsShippable = true;
                model.ShippingAddress = order.ShippingAddress.ToModel();
                model.ShippingMethod = order.ShippingMethod;
                model.OrderWeight = order.OrderWeight;
                var baseWeight = _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId);
                if (baseWeight != null)
                    model.BaseWeightIn = baseWeight.Name;

                if (order.ShippedDateUtc.HasValue)
                    model.ShippedDate = _dateTimeHelper.ConvertToUserTime(order.ShippedDateUtc.Value, DateTimeKind.Utc).ToString("D");

                if (order.DeliveryDateUtc.HasValue)
                    model.DeliveryDate = _dateTimeHelper.ConvertToUserTime(order.DeliveryDateUtc.Value, DateTimeKind.Utc).ToString("D");

                model.TrackingNumber = order.TrackingNumber;
                model.TrackingUrl = order.GetTrackingUrl();
            }


            //billing info
            model.BillingAddress = order.BillingAddress.ToModel();

            //VAT number
            model.VatNumber = order.VatNumber;

            //payment method
            var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(order.PaymentMethodSystemName);
            model.PaymentMethod = paymentMethod != null ? paymentMethod.PluginDescriptor.FriendlyName : order.PaymentMethodSystemName;


            //totals
            switch (order.CustomerTaxDisplayType)
            {
                case TaxDisplayType.ExcludingTax:
                    {
                        //order subtotal
                        var orderSubtotalExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubtotalExclTax, order.CurrencyRate);
                        model.OrderSubtotal = _priceFormatter.FormatPrice(orderSubtotalExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, false);
                        //discount (applied to order subtotal)
                        var orderSubTotalDiscountExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubTotalDiscountExclTax, order.CurrencyRate);
                        if (orderSubTotalDiscountExclTaxInCustomerCurrency > decimal.Zero)
                            model.OrderSubTotalDiscount = _priceFormatter.FormatPrice(-orderSubTotalDiscountExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, false);
                        //order shipping
                        var orderShippingExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingExclTax, order.CurrencyRate);
                        model.OrderShipping = _priceFormatter.FormatShippingPrice(orderShippingExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, false);
                        //payment method additional fee
                        var paymentMethodAdditionalFeeExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeExclTax, order.CurrencyRate);
                        if (paymentMethodAdditionalFeeExclTaxInCustomerCurrency > decimal.Zero)
                            model.PaymentMethodAdditionalFee = _priceFormatter.FormatPaymentMethodAdditionalFee(paymentMethodAdditionalFeeExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, false);
                    }
                    break;
                case TaxDisplayType.IncludingTax:
                    {
                        //order subtotal
                        var orderSubtotalInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubtotalInclTax, order.CurrencyRate);
                        model.OrderSubtotal = _priceFormatter.FormatPrice(orderSubtotalInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, true);
                        //discount (applied to order subtotal)
                        var orderSubTotalDiscountInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubTotalDiscountInclTax, order.CurrencyRate);
                        if (orderSubTotalDiscountInclTaxInCustomerCurrency > decimal.Zero)
                            model.OrderSubTotalDiscount = _priceFormatter.FormatPrice(-orderSubTotalDiscountInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, true);
                        //order shipping
                        var orderShippingInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingInclTax, order.CurrencyRate);
                        model.OrderShipping = _priceFormatter.FormatShippingPrice(orderShippingInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, true);
                        //payment method additional fee
                        var paymentMethodAdditionalFeeInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeInclTax, order.CurrencyRate);
                        if (paymentMethodAdditionalFeeInclTaxInCustomerCurrency > decimal.Zero)
                            model.PaymentMethodAdditionalFee = _priceFormatter.FormatPaymentMethodAdditionalFee(paymentMethodAdditionalFeeInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, true);
                    }
                    break;
            }

            //tax
            bool displayTax = true;
            bool displayTaxRates = true;
            if (_taxSettings.HideTaxInOrderSummary && order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
            {
                displayTax = false;
                displayTaxRates = false;
            }
            else
            {
                if (order.OrderTax == 0 && _taxSettings.HideZeroTax)
                {
                    displayTax = false;
                    displayTaxRates = false;
                }
                else
                {
                    displayTaxRates = _taxSettings.DisplayTaxRates && order.TaxRatesDictionary.Count > 0;
                    displayTax = !displayTaxRates;

                    var orderTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTax, order.CurrencyRate);
                    model.Tax = _priceFormatter.FormatPrice(orderTaxInCustomerCurrency, true, order.CustomerCurrencyCode, false, _workContext.WorkingLanguage);

                    foreach (var tr in order.TaxRatesDictionary)
                    {
                        model.TaxRates.Add(new OrderDetailsModel.TaxRate()
                        {
                            Rate = _priceFormatter.FormatTaxRate(tr.Key),
                            Value = _priceFormatter.FormatPrice(_currencyService.ConvertCurrency(tr.Value, order.CurrencyRate), true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, false),
                        });
                    }
                }
            }
            model.DisplayTaxRates = displayTaxRates;
            model.DisplayTax = displayTax;


            //discount (applied to order total)
            var orderDiscountInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderDiscount, order.CurrencyRate);
            if (orderDiscountInCustomerCurrency > decimal.Zero)
                model.OrderTotalDiscount = _priceFormatter.FormatPrice(-orderDiscountInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, false);


            //gift cards
            foreach (var gcuh in order.GiftCardUsageHistory)
            {
                model.GiftCards.Add(new OrderDetailsModel.GiftCard()
                {
                    CouponCode = gcuh.GiftCard.GiftCardCouponCode,
                    Amount = _priceFormatter.FormatPrice(-(_currencyService.ConvertCurrency(gcuh.UsedValue, order.CurrencyRate)), true, order.CustomerCurrencyCode, false, _workContext.WorkingLanguage),
                });
            }

            //reward points           
            if (order.RedeemedRewardPointsEntry != null)
            {
                model.RedeemedRewardPoints = -order.RedeemedRewardPointsEntry.Points;
                model.RedeemedRewardPointsAmount = _priceFormatter.FormatPrice(-(_currencyService.ConvertCurrency(order.RedeemedRewardPointsEntry.UsedAmount, order.CurrencyRate)), true, order.CustomerCurrencyCode, false, _workContext.WorkingLanguage);
            }

            //total
            var orderTotalInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
            model.OrderTotal = _priceFormatter.FormatPrice(orderTotalInCustomerCurrency, true, order.CustomerCurrencyCode, false, _workContext.WorkingLanguage);

            //checkout attributes
            model.CheckoutAttributeInfo = order.CheckoutAttributeDescription;

            //order notes
            foreach (var orderNote in order.OrderNotes
                .Where(on => on.DisplayToCustomer)
                .OrderByDescending(on => on.CreatedOnUtc)
                .ToList())
            {
                model.OrderNotes.Add(new OrderDetailsModel.OrderNote()
                {
                    Note = Nop.Core.Html.HtmlHelper.FormatText(orderNote.Note, false, true, false, false, false, false),
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(orderNote.CreatedOnUtc, DateTimeKind.Utc)
                });
            }


            //purchased products
            model.ShowSku = true;
            var orderProductVariants = _orderService.GetAllOrderProductVariants(order.Id, null, null, null, null, null, null);
            foreach (var opv in orderProductVariants)
            {
                var opvModel = new OrderDetailsModel.OrderProductVariantModel()
                {
                    Id = opv.Id,
                    Sku = opv.ProductVariant.Sku,
                    ProductId = opv.ProductVariant.ProductId,
                    ProductSeName = opv.ProductVariant.Product.GetSeName(),
                    Quantity = opv.Quantity,
                    AttributeInfo = opv.AttributeDescription,
                    ManufacturerName = opv.ProductVariant.Product.ManufacturerName()
                };

                //picture
                var picture = opv.ProductVariant.GetDefaultProductVariantPicture(_pictureService);
                if (picture == null)
                {
                    picture = _pictureService.GetPicturesByProductId(opv.ProductVariant.Product.Id, 1).FirstOrDefault();
                }

                var pictureModel = new PictureModel()
                {
                    ImageUrl = _pictureService.GetPictureUrl(picture, _mediaSettings.CartThumbPictureSize, true),
                    Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat"), opv.ProductVariant.Product.Name),
                    AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat"), opv.ProductVariant.Name),

                };
                opvModel.Picture = pictureModel;

                //product name
                if (!String.IsNullOrEmpty(opv.ProductVariant.GetLocalized(x => x.Name)))
                    opvModel.ProductName = string.Format("{0} ({1})", opv.ProductVariant.Product.GetLocalized(x => x.Name), opv.ProductVariant.GetLocalized(x => x.Name));
                else
                    opvModel.ProductName = opv.ProductVariant.Product.GetLocalized(x => x.Name);
                model.Items.Add(opvModel);

                //unit price, subtotal
                switch (order.CustomerTaxDisplayType)
                {
                    case TaxDisplayType.ExcludingTax:
                        {
                            var opvUnitPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(opv.UnitPriceExclTax, order.CurrencyRate);
                            opvModel.UnitPrice = _priceFormatter.FormatPrice(opvUnitPriceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, false);

                            var opvPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(opv.PriceExclTax, order.CurrencyRate);
                            opvModel.SubTotal = _priceFormatter.FormatPrice(opvPriceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, false);
                        }
                        break;
                    case TaxDisplayType.IncludingTax:
                        {
                            var opvUnitPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(opv.UnitPriceInclTax, order.CurrencyRate);
                            opvModel.UnitPrice = _priceFormatter.FormatPrice(opvUnitPriceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, true);

                            var opvPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(opv.PriceInclTax, order.CurrencyRate);
                            opvModel.SubTotal = _priceFormatter.FormatPrice(opvPriceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, true);
                        }
                        break;
                }
            }

            return model;
        }



        #endregion

        #region Login / logout / register

        [NopHttpsRequirement(SslRequirement.Yes)]
        public ActionResult Login(bool? checkoutAsGuest)
        {
            var model = new LoginModel();
            model.UsernamesEnabled = _customerSettings.UsernamesEnabled;
            model.CheckoutAsGuest = checkoutAsGuest.HasValue ? checkoutAsGuest.Value : false;
            //TODO: refactor
            if (HttpContext.Request.QueryString["ReturnUrl"] != null)
                Session["ReturnUrl"] = HttpContext.Request.QueryString["ReturnUrl"];
            return View(model);
        }

        [HttpPost]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            bool isValidPassword = true;
            if (ModelState.Values.ToList()[0].Errors.Count < 1 && ModelState.Values.ToList()[1].Errors.Count < 1)
            {
                if (_customerRegistrationService.ValidateCustomer(_customerSettings.UsernamesEnabled ? model.UserName : model.Email, model.Password))
                {
                    var customer = _customerSettings.UsernamesEnabled ? _customerService.GetCustomerByUsername(model.UserName) : _customerService.GetCustomerByEmail(model.Email);
                    if(customer.IsAdmin())
                        {
                            Regex regex = new Regex(_customerSettings.AdminPasswordRegex);
                            isValidPassword = regex.IsMatch(model.Password);
                        }
                    if (!isValidPassword)
                    {
                        ModelState.AddModelError("", "The credentials provided is incorrect.");
                        MessageModel msg = new MessageModel();
                        msg.Successful = false;
                        msg.MessageList.Add(_localizationService.GetResource("Login.InvalidPassword"));
                        ViewBag.msgModel = msg;
                    }
                    else
                    {//migrate shopping cart
                        _shoppingCartService.MigrateShoppingCart(_workContext.CurrentCustomer, customer);
                        //_customerService.MigrateCustomerProductVariantQuotes(_workContext.CurrentCustomer, customer);
                        //sign in new customer
                        _authenticationService.SignIn(customer, model.RememberMe);


                        if (!String.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                            return Redirect(returnUrl);
                        else
                            //return RedirectToAction("Index", "Home");
                            return RedirectToRoute("HomePage");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "The credentials provided is incorrect.");
                    MessageModel msg = new MessageModel();
                    msg.Successful = false;
                    msg.MessageList.Add(_localizationService.GetResource("Login.FailureText"));
                    ViewBag.msgModel = msg;
                }
            }

            //If we got this far, something failed, redisplay form
            model.UsernamesEnabled = _customerSettings.UsernamesEnabled;
            return View(model);
        }


        private void Login(LoginModel model)
        {
            if (ModelState.Values.ToList()[0].Errors.Count < 1 && ModelState.Values.ToList()[1].Errors.Count < 1)
            {
                if (_customerRegistrationService.ValidateCustomer(_customerSettings.UsernamesEnabled ? model.UserName : model.Email, model.Password))
                {
                    var customer = _customerSettings.UsernamesEnabled ? _customerService.GetCustomerByUsername(model.UserName) : _customerService.GetCustomerByEmail(model.Email);

                    //migrate shopping cart
                    _shoppingCartService.MigrateShoppingCart(_workContext.CurrentCustomer, customer);

                    //sign in new customer
                    _authenticationService.SignIn(customer, model.RememberMe);
                }
                else
                {
                    ModelState.AddModelError("", "The credentials provided is incorrect.");
                    MessageModel msg = new MessageModel();
                    msg.Successful = false;
                    msg.MessageList.Add(_localizationService.GetResource("Login.FailureText"));
                    ViewBag.msgModel = msg;
                }
            }
        }

        //AF
        public ActionResult RegisterNewsletter()
        {
            var model = new RegisterModel();
            //model.Genders.Add(new SelectListItem() { Text = _localizationService.GetResource("Common.Dropdown.SelectValue"), Value = "" });
            model.Genders.Add(new SelectListItem() { Text = _localizationService.GetResource("Account.Fields.Gender.Female"), Value = "F" });
            model.Genders.Add(new SelectListItem() { Text = _localizationService.GetResource("Account.Fields.Gender.Male"), Value = "M" });

            return View(model);
        }

        [HttpPost]
        public JsonResult RegisterNewsletter(RegisterModel model)
        {
            //JavaScriptSerializer serializer = new JavaScriptSerializer();
            //RegisterModel model = serializer.Deserialize<RegisterModel>(model);


            return Json(new { Result = 1 });
        }

        //SUBSCRIBE
        public JsonResult RegisterCustomerNewsletter(CustomerInfoModel modelNewsletter)
        {
            //JavaScriptSerializer serializer = new JavaScriptSerializer();
            //RegisterModel model = serializer.Deserialize<RegisterModel>(model);


            var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmail(modelNewsletter.Email);
            var customer = _customerService.GetCustomerByEmail(_workContext.CurrentCustomer.Email);


            if (subscription == null)
            {
                subscription = new NewsLetterSubscription();
                subscription.Active = true;
                subscription.CreatedOnUtc = DateTime.Now;
                subscription.Email = modelNewsletter.Email;
                subscription.FirstName = modelNewsletter.FirstName;
                subscription.LastName = modelNewsletter.LastName;
                subscription.Gender = modelNewsletter.Gender;
                subscription.NewsLetterSubscriptionGuid = new Guid();
                subscription.LanguageId = _workContext.WorkingLanguage.Id;

                _newsLetterSubscriptionService.InsertNewsLetterSubscription(subscription);
                subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmail(modelNewsletter.Email);
                _workflowMessageService.SendNewsLetterSubscriptionActivationMessage(subscription, _workContext.WorkingLanguage.Id);
                return Json(new
                {
                    Success = true,
                    header = _localizationService.GetResource("NewsLetterSubscription.Newsletter.Inserted"),
                    message = ""
                }, JsonRequestBehavior.AllowGet);
            }
            else if (subscription.Active)
            {
                return Json(new
                {
                    Success = false,
                    header = _localizationService.GetResource("NewsLetterSubscription.Newsletter.AlreadyActive"),
                    message = ""
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                subscription.Active = true;
                _newsLetterSubscriptionService.UpdateNewsLetterSubscription(subscription);
                _workflowMessageService.SendNewsLetterSubscriptionActivationMessage(subscription, _workContext.WorkingLanguage.Id);
                return Json(new
                {
                    Success = true,
                    header = _localizationService.GetResource("NewsLetterSubscription.Newsletter.Updated"),
                    message = ""
                }, JsonRequestBehavior.AllowGet);
            }
            //NewsletterBoxModel model = new NewsletterBoxModel();
            //model.Email = modelNewsletter.Email;
            //model.FirstName = modelNewsletter.FirstName;
            //model.LastName = modelNewsletter.LastName;
            //model.Subscribe = subscription.Active;
            //model.Gender = modelNewsletter.Gender;

            //string result;
            //bool success = false;
            //var email = model.Email;
            //var subscribe = true;
            //if (!CommonHelper.IsValidEmail(email))
            //    result = _localizationService.GetResource("Newsletter.Email.Wrong");
            //else
            //{
            //    //subscribe/unsubscribe
            //    email = email.Trim();


            //    if (subscription != null)
            //    {
            //        if (subscribe)
            //        {
            //            if (subscription.Active)
            //            {
            //                _workflowMessageService.SendNewsLetterSubscriptionActivationMessage(subscription, _workContext.WorkingCurrency.Id);
            //                subscription.Active = true;
            //                _newsLetterSubscriptionService.UpdateNewsLetterSubscription(subscription);
            //            }
            //            result = _localizationService.GetResource("Newsletter.SubscribeEmailSent");
            //        }
            //        else
            //        {
            //            if (subscription.Active)
            //            {
            //                _workflowMessageService.SendNewsLetterSubscriptionDeactivationMessage(subscription, _workContext.WorkingLanguage.Id);
            //            }
            //            result = _localizationService.GetResource("Newsletter.UnsubscribeEmailSent");
            //        }
            //    }
            //    else if (subscribe)
            //    {
            //        subscription = new NewsLetterSubscription()
            //        {
            //            NewsLetterSubscriptionGuid = Guid.NewGuid(),
            //            Email = email,
            //            FirstName = model.FirstName,
            //            LastName = model.LastName,
            //            Gender = model.Gender,
            //            Active = true,
            //            CreatedOnUtc = DateTime.UtcNow
            //        };
            //        _newsLetterSubscriptionService.InsertNewsLetterSubscription(subscription);
            //        //_workflowMessageService.SendNewsLetterSubscriptionActivationMessage(subscription, _workContext.WorkingLanguage.Id);

            //        //result = _localizationService.GetResource("Newsletter.SubscribeEmailSent");
            //    }
            //    else
            //    {
            //        //result = _localizationService.GetResource("Newsletter.UnsubscribeEmailSent");
            //    }
            //    success = true;
            //}



        }

        //UNSUBSCRIBE
        [HttpGet]
        public JsonResult UnRegisterCustomerNewsletter(CustomerInfoModel modelNewsletter)
        {
            //JavaScriptSerializer serializer = new JavaScriptSerializer();
            //RegisterModel model = serializer.Deserialize<RegisterModel>(model);
            var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmail(modelNewsletter.Email);

            if (subscription == null)
            {
                subscription = new NewsLetterSubscription();
                subscription.Active = true;
                subscription.CreatedOnUtc = DateTime.Now;
                subscription.Email = modelNewsletter.Email;
                subscription.FirstName = modelNewsletter.FirstName;
                subscription.LastName = modelNewsletter.LastName;
                subscription.Gender = modelNewsletter.Gender;
                subscription.NewsLetterSubscriptionGuid = new Guid();
                subscription.LanguageId = _workContext.WorkingLanguage.Id;

                _newsLetterSubscriptionService.InsertNewsLetterSubscription(subscription);
                subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmail(modelNewsletter.Email);

            }
            NewsletterBoxModel model = new NewsletterBoxModel();
            model.Email = modelNewsletter.Email;
            model.FirstName = modelNewsletter.FirstName;
            model.LastName =modelNewsletter.LastName;
            model.Subscribe = subscription.Active;
            model.Gender =modelNewsletter.Gender;


            string result;
            bool success = false;
            var email = model.Email;
            //Unsubscribe
            var subscribe = false;
            if (!CommonHelper.IsValidEmail(email))
                result = _localizationService.GetResource("Newsletter.Email.Wrong");
            else
            {
                //subscribe/unsubscribe
                email = email.Trim();


                if (subscription != null)
                {
                    if (subscribe)
                    {
                        if (!subscription.Active)
                        {
                            _workflowMessageService.SendNewsLetterSubscriptionActivationMessage(subscription, _workContext.WorkingCurrency.Id);
                        }
                        result = _localizationService.GetResource("Newsletter.SubscribeEmailSent");
                    }
                    else
                    {
                        if (subscription.Active)
                        {

                            _workflowMessageService.SendNewsLetterSubscriptionDeactivationMessage(subscription, _workContext.WorkingLanguage.Id);
                            subscription.Active = false;
                            _newsLetterSubscriptionService.UpdateNewsLetterSubscription(subscription);

                        }
                        result = _localizationService.GetResource("NewsLetterSubscription.Newsletter.Updated");
                    }
                }
                else if (subscribe)
                {
                    subscription = new NewsLetterSubscription()
                    {
                        NewsLetterSubscriptionGuid = Guid.NewGuid(),
                        Email = email,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Gender = model.Gender,
                        Active = true,
                        CreatedOnUtc = DateTime.UtcNow,
                        LanguageId = _workContext.WorkingLanguage.Id
                    };
                    _newsLetterSubscriptionService.InsertNewsLetterSubscription(subscription);
                    //_workflowMessageService.SendNewsLetterSubscriptionActivationMessage(subscription, _workContext.WorkingLanguage.Id);

                    //result = _localizationService.GetResource("Newsletter.SubscribeEmailSent");
                }
                else
                {
                    //result = _localizationService.GetResource("Newsletter.UnsubscribeEmailSent");
                }
                success = true;
            }

            return Json(new
            {
                Success = false,
                header = _localizationService.GetResource("Newsletter.UnsubscribeEmailSent"),
                message = ""
            },JsonRequestBehavior.AllowGet);
          
        }

        [NopHttpsRequirement(SslRequirement.Yes)]
        public ActionResult Register(string returnUrl)
        {
            ViewBag.msgModel = new MessageModel();
            //check whether registration is allowed
            if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled)
                return RedirectToAction("RegisterResult", new { resultId = (int)UserRegistrationType.Disabled });

            var model = new RegisterModel();
            model.Genders = new List<SelectListItem>();
            model.Genders.Add(new SelectListItem() { Text = _localizationService.GetResource("Account.Fields.Gender.Female"), Value = "F" });
            model.Genders.Add(new SelectListItem() { Text = _localizationService.GetResource("Account.Fields.Gender.Male"), Value = "M" });

            model.AllowCustomersToSetTimeZone = _dateTimeSettings.AllowCustomersToSetTimeZone;
            foreach (var tzi in _dateTimeHelper.GetSystemTimeZones())
                model.AvailableTimeZones.Add(new SelectListItem() { Text = tzi.DisplayName, Value = tzi.Id, Selected = (tzi.Id == _dateTimeHelper.DefaultStoreTimeZone.Id) });
            model.DisplayVatNumber = _taxSettings.EuVatEnabled;
            //form fields
            model.GenderEnabled = _customerSettings.GenderEnabled;
            model.CompanyEnabled = _customerSettings.CompanyEnabled;
            model.NewsletterEnabled = _customerSettings.NewsletterEnabled;
            model.DateOfBirthEnabled = _customerSettings.DateOfBirthEnabled;
            model.UsernamesEnabled = _customerSettings.UsernamesEnabled;
            model.LocationEnabled = _customerSettings.ShowCustomersLocation;
            model.DisplayCaptcha = _captchaSettings.Enabled;
            if (_customerSettings.ShowCustomersLocation)
            {
                foreach (var c in _countryService.GetAllCountries())
                {
                    model.AvailableLocations.Add(new SelectListItem() { Text = c.Name, Value = c.Id.ToString() });
                }
            }
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // [CaptchaValidator]
        public ActionResult Register(RegisterModel model, bool? captchaValid)
        {
            //check whether registration is allowed
            CustomerRegistrationResult registrationResult = new CustomerRegistrationResult();
            model.Newsletter = true;
            if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled)
                return RedirectToAction("RegisterResult", new { resultId = (int)UserRegistrationType.Disabled });
            ViewBag.msgModel = new MessageModel();
            if (_workContext.CurrentCustomer.IsRegistered())
            {
                //Already registered customer. 
                //_authenticationService.SignOut();


                registrationResult.Errors.Add(_localizationService.GetResource("Registration.AlreadyRegistered"));
                //Save a new record
                //_workContext.CurrentCustomer = _customerService.InsertGuestCustomer();
            }
            var customer = _workContext.CurrentCustomer;


            if (captchaValid.HasValue && !captchaValid.Value)
                ModelState.AddModelError("", _localizationService.GetResource("Common.WrongCaptcha"));

            if (ModelState.IsValid)
            {
                bool isApproved = _customerSettings.UserRegistrationType == UserRegistrationType.Standard;
                var registrationRequest = new CustomerRegistrationRequest(customer, model.Email,
                    _customerSettings.UsernamesEnabled ? model.Username : model.Email, model.Password, PasswordFormat.Hashed, isApproved);
                var regResult = _customerRegistrationService.RegisterCustomer(registrationRequest);
                foreach (var error in regResult.Errors)
                {
                    registrationResult.Errors.Add(error);
                }
                if (registrationResult.Success)
                {
                    //properties
                    if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                        customer.TimeZoneId = model.TimeZoneId;
                    //VAT number
                    if (_taxSettings.EuVatEnabled)
                    {
                        customer.VatNumber = model.VatNumber;

                        string vatName = string.Empty;
                        string vatAddress = string.Empty;
                        customer.VatNumberStatus = _taxService.GetVatNumberStatus(customer.VatNumber, out vatName, out vatAddress);
                        //send VAT number admin notification
                        if (!String.IsNullOrEmpty(customer.VatNumber) && _taxSettings.EuVatEmailAdminWhenNewVatSubmitted)
                            _workflowMessageService.SendNewVatSubmittedStoreOwnerNotification(customer, customer.VatNumber, vatAddress, _localizationSettings.DefaultAdminLanguageId);

                    }
                    //save
                    _customerService.UpdateCustomer(customer);

                    //form fields
                    if (_customerSettings.GenderEnabled)
                        _customerService.SaveCustomerAttribute(customer, SystemCustomerAttributeNames.Gender, model.Gender);
                    _customerService.SaveCustomerAttribute(customer, SystemCustomerAttributeNames.FirstName, model.FirstName);
                    _customerService.SaveCustomerAttribute(customer, SystemCustomerAttributeNames.LastName, model.LastName);
                    if (_customerSettings.DateOfBirthEnabled)
                    {
                        DateTime? dateOfBirth = null;
                        try
                        {
                            dateOfBirth = new DateTime(model.DateOfBirthYear.Value, model.DateOfBirthMonth.Value, model.DateOfBirthDay.Value);
                        }
                        catch { }
                        _customerService.SaveCustomerAttribute(customer, SystemCustomerAttributeNames.DateOfBirth, dateOfBirth);
                    }
                    if (_customerSettings.CompanyEnabled)
                        _customerService.SaveCustomerAttribute(customer, SystemCustomerAttributeNames.Company, model.Company);
                    if (_customerSettings.NewsletterEnabled)
                    {
                        //save newsletter value
                        var newsletter = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmail(model.Email);
                        if (newsletter != null)
                        {
                            if (model.Newsletter)
                            {
                                newsletter.Active = true;
                                _newsLetterSubscriptionService.UpdateNewsLetterSubscription(newsletter);
                            }
                            else
                                _newsLetterSubscriptionService.DeleteNewsLetterSubscription(newsletter);
                        }
                        else
                        {
                            if (model.Newsletter)
                            {
                                _newsLetterSubscriptionService.InsertNewsLetterSubscription(new NewsLetterSubscription()
                                {
                                    NewsLetterSubscriptionGuid = Guid.NewGuid(),
                                    Email = model.Email,
                                    Active = true,
                                    CreatedOnUtc = DateTime.UtcNow,
                                    LanguageId = _workContext.WorkingLanguage.Id
                                });
                            }
                        }
                    }

                    //Migrate product quotes
                    try
                    {
                      //  _customerService.MigrateCustomerProductVariantQuotes(customer.Email, customer);

                        var quotes = _customerService.GetAllProductVariantQuotes(null, null, customer.Email, null, null, 0, int.MaxValue);
                        foreach (var customerProductVariantQuote in quotes)
                        {
                            customerProductVariantQuote.CustomerId = customer.Id;
                            _customerService.UpdateCustomerProductVariantQuote(customerProductVariantQuote);
                        }

                    }
                    catch (Exception ex)
                    {
                        var logger = EngineContext.Current.Resolve<ILogger>();
                        logger.Error("Error migrating product quotes after registration", ex);
                    }

                    //login customer now
                    if (isApproved)
                        _authenticationService.SignIn(customer, true);

                    //notifications
                    if (_customerSettings.NotifyNewCustomerRegistration)
                        _workflowMessageService.SendCustomerRegisteredNotificationMessage(customer, _localizationSettings.DefaultAdminLanguageId);

                    switch (_customerSettings.UserRegistrationType)
                    {
                        case UserRegistrationType.EmailValidation:
                            {
                                //email validation message
                                _customerService.SaveCustomerAttribute(customer, SystemCustomerAttributeNames.AccountActivationToken, Guid.NewGuid().ToString());
                                _workflowMessageService.SendCustomerEmailValidationMessage(customer, _workContext.WorkingLanguage.Id);

                                //result
                                return RedirectToAction("RegisterResult", new { resultId = (int)UserRegistrationType.EmailValidation });
                            }
                        case UserRegistrationType.AdminApproval:
                            {
                                //LoginModel loginModel = new LoginModel();
                                //loginModel.Email = model.Email;
                                //loginModel.Password = model.Password;
                                //loginModel.UserName = customer.Username;
                                //loginModel.RememberMe = false;
                                //_customerService.RegisterCustomer(new CustomerRegistrationRequest(customer, model.Email, customer.Username, model.Password, PasswordFormat.Hashed, true));
                                //_authenticationService.SignIn(customer, true);
                                //Login(loginModel);
                                //TODO:Active myaccount link
                                return RedirectToAction("RegisterResult", new { resultId = (int)UserRegistrationType.AdminApproval });
                            }
                        case UserRegistrationType.Standard:
                            {
                                //TODO: mustafa re-engineer - send customer welcome message
                                _workflowMessageService.SendCustomerWelcomeMessage(customer, _workContext.WorkingLanguage.Id);
                                var returnUrl = Session["ReturnUrl"] == null ? "" : Session["ReturnUrl"].ToString();
                                Session["ReturnUrl"] = null;
                                if (!String.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                                    return Redirect(returnUrl);
                                else
                                    return RedirectToAction("RegisterResult", new { resultId = (int)UserRegistrationType.Standard });
                            }
                        default:
                            {
                                var returnUrl = Session["ReturnUrl"] == null ? "" : Session["ReturnUrl"].ToString();
                                if (!String.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                                    return Redirect(returnUrl);
                                return RedirectToAction("Index", "Home");
                            }

                    }
                }
                else
                {

                    MessageModel msgModel = new MessageModel();
                    foreach (var error in registrationResult.Errors)
                    {
                        ModelState.AddModelError("", error);
                        msgModel.MessageList.Add(error);
                        ViewBag.msgModel = msgModel;
                    }
                }
            }

            //If we got this far, something failed, redisplay form
            model.AllowCustomersToSetTimeZone = _dateTimeSettings.AllowCustomersToSetTimeZone;
            foreach (var tzi in _dateTimeHelper.GetSystemTimeZones())
                model.AvailableTimeZones.Add(new SelectListItem() { Text = tzi.DisplayName, Value = tzi.Id, Selected = (tzi.Id == _dateTimeHelper.DefaultStoreTimeZone.Id) });
            model.DisplayVatNumber = _taxSettings.EuVatEnabled;
            //form fields
            model.GenderEnabled = _customerSettings.GenderEnabled;
            model.CompanyEnabled = _customerSettings.CompanyEnabled;
            model.NewsletterEnabled = _customerSettings.NewsletterEnabled;
            model.DateOfBirthEnabled = _customerSettings.DateOfBirthEnabled;
            model.UsernamesEnabled = _customerSettings.UsernamesEnabled;
            model.LocationEnabled = _customerSettings.ShowCustomersLocation;
            model.DisplayCaptcha = _captchaSettings.Enabled;
            if (_customerSettings.ShowCustomersLocation)
            {
                foreach (var c in _countryService.GetAllCountries())
                {
                    model.AvailableLocations.Add(new SelectListItem() { Text = c.Name, Value = c.Id.ToString(), Selected = (c.Id == model.LocationCountryId) });
                }
            }

            return View(model);
        }

        public ActionResult RegisterResult(int resultId)
        {
            var resultText = "";
            switch ((UserRegistrationType)resultId)
            {
                case UserRegistrationType.Disabled:
                    resultText = _localizationService.GetResource("Account.Register.Result.Disabled");
                    break;
                case UserRegistrationType.Standard:
                    resultText = _localizationService.GetResource("Account.Register.Result.Standard");
                    break;
                case UserRegistrationType.AdminApproval:
                    resultText = _localizationService.GetResource("Account.Register.Result.AdminApproval");
                    break;
                case UserRegistrationType.EmailValidation:
                    resultText = _localizationService.GetResource("Account.Register.Result.EmailValidation");
                    break;
                default:
                    break;
            }
            var model = new RegisterResultModel()
            {
                Result = resultText
            };
            return View(model);
        }

        public ActionResult Logout()
        {
            if (_workContext.OriginalCustomerIfImpersonated != null)
            {
                //logout impersonated customer
                _customerService.SaveCustomerAttribute<int?>(_workContext.OriginalCustomerIfImpersonated,
                    SystemCustomerAttributeNames.ImpersonatedCustomerId, null);
                //redirect back to customer details page (admin area)
                return this.RedirectToAction("Edit", "Customer", new { id = _workContext.CurrentCustomer.Id, area = "Admin" });

            }
            else
            {
                //standard logout 
                _authenticationService.SignOut();
                //return this.RedirectToAction("Index", "Home");
                return RedirectToRoute("HomePage");
            }

        }

        [NopHttpsRequirement(SslRequirement.Yes)]
        public ActionResult AccountActivation(Guid token, string email)
        {
            var customer = _customerService.GetCustomerByEmail(email);
            if (customer == null)
                return RedirectToAction("Index", "Home");

            var cToken = customer.GetAttribute<string>(SystemCustomerAttributeNames.AccountActivationToken);
            if (String.IsNullOrEmpty(cToken))
                return RedirectToAction("Index", "Home");

            if (!cToken.Equals(token.ToString(), StringComparison.InvariantCultureIgnoreCase))
                return RedirectToAction("Index", "Home");

            //activate user account
            customer.Active = true;
            _customerService.UpdateCustomer(customer);
            _customerService.SaveCustomerAttribute(customer, SystemCustomerAttributeNames.AccountActivationToken, "");
            //send welcome message
            _workflowMessageService.SendCustomerWelcomeMessage(customer, _workContext.WorkingLanguage.Id);

            var model = new AccountActivationModel();
            model.Result = _localizationService.GetResource("Account.AccountActivation.Activated");
            return View(model);
        }

        #endregion

        #region My account

        [NopHttpsRequirement(SslRequirement.Yes)]
        public ActionResult MyAccount()
        {
            return RedirectToAction("info");
        }

        #region Info

        [NopHttpsRequirement(SslRequirement.Yes)]
        public ActionResult Info()
        {

            if (!IsCurrentUserRegistered())
                return new HttpUnauthorizedResult();

            var customer = _workContext.CurrentCustomer;

            var model = new CustomerInfoModel();
            PrepareCustomerInfoModel(model, customer, false);
            try
            {
                ViewData["isSubscriptionActive"] = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmail(model.Email).Active;
            }
            catch
            {
                ViewData["isSubscriptionActive"] = false;
            }

            model.NavigationModel.SelectedTab = CustomerNavigationEnum.Info;

            return View(model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Info(CustomerInfoModel model)
        {
            if (!IsCurrentUserRegistered())
                return new HttpUnauthorizedResult();

            var customer = _workContext.CurrentCustomer;


            //email
            //if (model.Email)
            //    ModelState.AddModelError("", "Email is not provided.");
            bool isLoginNeeded = false;
            MessageModel msgModel = new MessageModel();
            try
            {

                if (ModelState.IsValid)
                {

                    //username 
                    if (_customerSettings.UsernamesEnabled &&
                        this._customerSettings.AllowUsersToChangeUsernames)
                    {
                        if (!customer.Username.Equals(model.Username.Trim(), StringComparison.InvariantCultureIgnoreCase))
                        {
                            //change username
                            _customerRegistrationService.SetUsername(customer, model.Username.Trim());
                            //re-authenticate
                            _authenticationService.SignIn(customer, true);
                        }
                    }
                    //email
                    if (!string.IsNullOrWhiteSpace(model.NewEmail) &&
                        !customer.Email.Equals(model.NewEmail.Trim(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        //change email
                        _customerRegistrationService.SetEmail(customer, model.NewEmail.Trim());
                        //re-authenticate (if usernames are disabled)
                        if (!_customerSettings.UsernamesEnabled)
                        {
                            _authenticationService.SignIn(customer, true);
                        }
                    }

                    //properties
                    if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                        customer.TimeZoneId = model.TimeZoneId;
                    //VAT number
                    if (_taxSettings.EuVatEnabled)
                    {
                        var prevVatNumber = customer.VatNumber;
                        customer.VatNumber = model.VatNumber;
                        if (prevVatNumber != model.VatNumber)
                        {
                            string vatName = string.Empty;
                            string vatAddress = string.Empty;
                            customer.VatNumberStatus = _taxService.GetVatNumberStatus(customer.VatNumber, out vatName, out vatAddress);
                            //send VAT number admin notification
                            if (!String.IsNullOrEmpty(customer.VatNumber) && _taxSettings.EuVatEmailAdminWhenNewVatSubmitted)
                                _workflowMessageService.SendNewVatSubmittedStoreOwnerNotification(customer, customer.VatNumber, vatAddress, _localizationSettings.DefaultAdminLanguageId);
                        }
                    }

                    //save
                    _customerService.UpdateCustomer(customer);


                    //form fields
                    if (_customerSettings.GenderEnabled)
                        _customerService.SaveCustomerAttribute(customer, SystemCustomerAttributeNames.Gender, model.Gender);
                    _customerService.SaveCustomerAttribute(customer, SystemCustomerAttributeNames.FirstName, model.FirstName);
                    _customerService.SaveCustomerAttribute(customer, SystemCustomerAttributeNames.LastName, model.LastName);
                    if (_customerSettings.DateOfBirthEnabled)
                    {
                        DateTime? dateOfBirth = null;
                        try
                        {
                            dateOfBirth = new DateTime(model.DateOfBirthYear.Value, model.DateOfBirthMonth.Value, model.DateOfBirthDay.Value);
                        }
                        catch { }
                        _customerService.SaveCustomerAttribute(customer, SystemCustomerAttributeNames.DateOfBirth, dateOfBirth);
                    }
                    if (_customerSettings.CompanyEnabled)
                        _customerService.SaveCustomerAttribute(customer, SystemCustomerAttributeNames.Company, model.Company);
                    //newsletter
                    if (_customerSettings.NewsletterEnabled)
                    {
                        //save newsletter value
                        var newsletter = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmail(customer.Email);
                        if (newsletter != null)
                        {
                            if (model.Newsletter)
                            {
                                newsletter.Active = true;
                                _newsLetterSubscriptionService.UpdateNewsLetterSubscription(newsletter);
                            }
                            else
                                _newsLetterSubscriptionService.DeleteNewsLetterSubscription(newsletter);
                        }
                        else
                        {
                            if (model.Newsletter)
                            {
                                _newsLetterSubscriptionService.InsertNewsLetterSubscription(new NewsLetterSubscription()
                                {
                                    NewsLetterSubscriptionGuid = Guid.NewGuid(),
                                    Email = customer.Email,
                                    Active = true,
                                    CreatedOnUtc = DateTime.UtcNow,
                                    LanguageId = _workContext.WorkingLanguage.Id
                                });
                            }
                        }
                    }

                    if (_forumSettings.SignaturesEnabled)
                    {
                        _customerService.SaveCustomerAttribute(customer, SystemCustomerAttributeNames.Signature, model.Signature);
                    }



                    if (!string.IsNullOrWhiteSpace(model.NewPassword))
                    {
                        var changePasswordRequest = new ChangePasswordRequest(customer.Email,
                   false, PasswordFormat.Hashed, model.NewPassword, model.Password);
                        var changePasswordResult = _customerRegistrationService.ChangePassword(changePasswordRequest);

                        if (!changePasswordResult.Success)
                        {
                            foreach (var error in changePasswordResult.Errors)
                            {
                                ModelState.AddModelError("", error);
                                msgModel.MessageList.Add(error);
                            }
                            msgModel.Successful = false;
                            TempData["MessageModel"] = msgModel;
                            return RedirectToAction("info");
                        }
                        else
                            isLoginNeeded = true;



                    }

                }

            }
            catch (Exception exc)
            {
                msgModel.MessageList.Add(exc.Message);
                TempData["MessageModel"] = msgModel;
                return RedirectToAction("info");
            }


            //If we got this far, something failed, redisplay form
            PrepareCustomerInfoModel(model, customer, false);


            msgModel.Successful = true;
            //if (isLoginNeeded)
            //{
            //    //msgModel.MessageList.Add(_localizationService.GetResource("MYACCOUNT.OVERVIEW.INFO.SAVE.LOGIN.NEEDED"));
            //}
            msgModel.MessageList.Add(_localizationService.GetResource("MYACCOUNT.OVERVIEW.INFO.SAVE.SUCCESSFUL"));
           
            TempData["MessageModel"] = msgModel;
            return RedirectToAction("Info");
        }

        [NonAction]
        private void PrepareCustomerInfoModel(CustomerInfoModel model, Customer customer, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (customer == null)
                throw new ArgumentNullException("customer");

            model.AllowCustomersToSetTimeZone = _dateTimeSettings.AllowCustomersToSetTimeZone;
            if (model.AllowCustomersToSetTimeZone)
                foreach (var tzi in _dateTimeHelper.GetSystemTimeZones())
                    model.AvailableTimeZones.Add(new SelectListItem() { Text = tzi.DisplayName, Value = tzi.Id, Selected = (excludeProperties ? tzi.Id == model.TimeZoneId : tzi.Id == _dateTimeHelper.CurrentTimeZone.Id) });

            if (!excludeProperties)
            {
                model.VatNumber = customer.VatNumber;
                model.FirstName = customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName);
                model.LastName = customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName);
                model.Gender = customer.GetAttribute<string>(SystemCustomerAttributeNames.Gender);

                model.Genders = new List<SelectListItem>();
                //model.Genders.Add(new SelectListItem() { Text = _localizationService.GetResource("Common.Dropdown.SelectValue"), Value = ""});
                model.Genders.Add(new SelectListItem() { Text = _localizationService.GetResource("Account.Fields.Gender.Female"), Value = "F" });
                model.Genders.Add(new SelectListItem() { Text = _localizationService.GetResource("Account.Fields.Gender.Male"), Value = "M" });

                Address address = customer.Addresses.FirstOrDefault();
                if (address != null)
                {
                    model.Nation = address.Country != null ? address.Country.Name : "";
                    model.PhoneNumber = address.PhoneNumber;
                }
                else
                {
                    model.Nation = "";
                    model.PhoneNumber = "";
                }
                try
                {
                    if (model.Gender != null)
                        model.Genders.FirstOrDefault(x => string.Compare(x.Value, model.Gender, true) == 0).Selected = true;
                }
                catch
                {
                    model.Genders.FirstOrDefault().Selected = true;
                }

                var dateOfBirth = customer.GetAttribute<DateTime?>(SystemCustomerAttributeNames.DateOfBirth);
                if (dateOfBirth.HasValue)
                {
                    model.DateOfBirthDay = dateOfBirth.Value.Day;
                    model.DateOfBirthMonth = dateOfBirth.Value.Month;
                    model.DateOfBirthYear = dateOfBirth.Value.Year;
                }
                model.Company = customer.GetAttribute<string>(SystemCustomerAttributeNames.Company);

                //newsletter
                var newsletter = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmail(customer.Email);
                model.Newsletter = newsletter != null && newsletter.Active;

                model.Signature = customer.GetAttribute<string>(SystemCustomerAttributeNames.Signature);
                model.Email = customer.Email;
                model.Username = customer.Username;
            }
            else
            {
                if (_customerSettings.UsernamesEnabled && !_customerSettings.AllowUsersToChangeUsernames)
                    model.Username = customer.Username;
            }
            model.DisplayVatNumber = _taxSettings.EuVatEnabled;
            model.VatNumberStatusNote = customer.VatNumberStatus.GetLocalizedEnum(_localizationService, _workContext);
            model.GenderEnabled = _customerSettings.GenderEnabled;
            model.DateOfBirthEnabled = _customerSettings.DateOfBirthEnabled;
            model.CompanyEnabled = _customerSettings.CompanyEnabled;
            model.NewsletterEnabled = _customerSettings.NewsletterEnabled;
            model.UsernamesEnabled = _customerSettings.UsernamesEnabled;
            model.AllowUsersToChangeUsernames = _customerSettings.AllowUsersToChangeUsernames;
            model.SignatureEnabled = _forumSettings.SignaturesEnabled;
            model.LocationEnabled = _customerSettings.ShowCustomersLocation;


            model.NavigationModel = GetCustomerNavigationModel(customer);


        }

        #endregion

        #region Addresses

        private void PrepareAddressModelDropDowns(AddressModel model)
        {
            //model.AvailableCountries.Add(new SelectListItem() { Text = _localizationService.GetResource("Address.SelectCountry"), Value = "0" });
            foreach (var c in _countryService.GetAllCountries())
                model.AvailableCountries.Add(new SelectListItem() { Text = c.Name, Value = c.Id.ToString(), Selected = (c.Id == model.CountryId) });
            //states
            var states = model.CountryId != null ? _stateProvinceService.GetStateProvincesByCountryId(model.CountryId.Value).ToList() : new List<StateProvince>();
            if (states.Count > 0)
            {
                foreach (var s in states)
                    model.AvailableStates.Add(new SelectListItem() { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == model.StateProvinceId) });
            }
            else
                model.AvailableStates.Add(new SelectListItem() { Text = _localizationService.GetResource("Address.OtherNonUS"), Value = "0" });

            //if(viewName!= null || viewName!="")
            //    return View("AddressesOverviewEdit",model);

            model.Genders.Add(new SelectListItem() { Text = _localizationService.GetResource("Address.Fields.Title.Male"), Value = "M" });
            model.Genders.Add(new SelectListItem() { Text = _localizationService.GetResource("Address.Fields.Title.Female"), Value = "F" });
            try
            {
                if (model.Title != null)
                    model.Genders.FirstOrDefault(x => string.Compare(x.Value, model.Title, true) == 0).Selected = true;
            }
            catch
            {
                model.Genders.FirstOrDefault().Selected = true;
            }
        }

        [NopHttpsRequirement(SslRequirement.Yes)]
        public ActionResult Addresses(int? id, int? id1)
        {
            if (!IsCurrentUserRegistered())
                return new HttpUnauthorizedResult();

            if (TempData["modelState"] != null)
                ModelState.Merge(TempData["modelState"] as ModelStateDictionary);

            var customer = _workContext.CurrentCustomer;

            var model = new CustomerAddressListModel();
            model.NavigationModel = GetCustomerNavigationModel(customer);
            model.NavigationModel.SelectedTab = CustomerNavigationEnum.Addresses;
            foreach (var address in customer.Addresses)
                model.Addresses.Add(address.ToModel());
            model.Addresses.Add(new AddressModel() { Id = 0, Name = _localizationService.GetResource("Address.New.Address") });
            ViewBag.Msg = null;
            MessageModel msg = new MessageModel();
            if (!id.HasValue)
            {
                model.SelectedAddressModel = model.Addresses.FirstOrDefault();
                //
                if (id1 == 2)
                {
                    msg.Successful = false;
                    msg.MessageList.Add(_localizationService.GetResource("Address.Name.AlreadyExists", _workContext.WorkingLanguage.Id));
                    ViewBag.Msg = msg;
                }
            }
            else
                model.SelectedAddressModel = model.Addresses.FirstOrDefault(x => x.Id == id.Value);

            //model.SelectedAddressModel.AvailableCountries.Add(new SelectListItem() { Text = _localizationService.GetResource("Address.SelectCountry"), Value = "0" });
            foreach (var c in _countryService.GetAllCountries())
                model.SelectedAddressModel.AvailableCountries.Add(new SelectListItem() { Text = c.Name, Value = c.Id.ToString(), Selected = (c.Id == model.SelectedAddressModel.CountryId) });
            //states
            var states = model.SelectedAddressModel.CountryId != null ? _stateProvinceService.GetStateProvincesByCountryId(model.SelectedAddressModel.CountryId.Value).ToList() : _stateProvinceService.GetStateProvincesByCountryId(Convert.ToInt32(model.SelectedAddressModel.AvailableCountries.First().Value));
            if (states.Count > 0)
            {
                foreach (var s in states)
                    model.SelectedAddressModel.AvailableStates.Add(new SelectListItem() { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == model.SelectedAddressModel.StateProvinceId) });
            }
            else
                model.SelectedAddressModel.AvailableStates.Add(new SelectListItem() { Text = _localizationService.GetResource("Address.OtherNonUS"), Value = "0" });

            //if(viewName!= null || viewName!="")
            //    return View("AddressesOverviewEdit",model);

            model.SelectedAddressModel.Genders.Add(new SelectListItem() { Text = _localizationService.GetResource("Address.Fields.Title.Male"), Value = "M" });
            model.SelectedAddressModel.Genders.Add(new SelectListItem() { Text = _localizationService.GetResource("Address.Fields.Title.Female"), Value = "F" });
            //model.SelectedAddressModel.EnterpriseOptions.Add(new SelectListItem() { Text = _localizationService.GetResource("Address.Fields.Enterprise"), Value = "true" });
            //model.SelectedAddressModel.EnterpriseOptions.Add(new SelectListItem() { Text = _localizationService.GetResource("Address.Fields.Personal"), Value = "false" });
            try
            {
                if (model.SelectedAddressModel.Title != null)
                    model.SelectedAddressModel.Genders.FirstOrDefault(x => string.Compare(x.Value, model.SelectedAddressModel.Title, true) == 0).Selected = true;
                if (model.SelectedAddressModel.IsEnterprise != null)
                    model.SelectedAddressModel.EnterpriseOptions.FirstOrDefault(x => bool.Equals(Convert.ToBoolean(x.Value), model.SelectedAddressModel.IsEnterprise)).Selected = true;

            }
            catch
            {
                model.SelectedAddressModel.Genders.FirstOrDefault().Selected = true;
            }

            if (id != null && id1 == 0)
            {
                msg.Successful = true;
                msg.MessageList.Add(_localizationService.GetResource("Address.Saved.Success", _workContext.WorkingLanguage.Id));
                ViewBag.Msg = msg;
            }
            else if (id != null && id1 == 1)
            {
                msg.Successful = true;
                msg.MessageList.Add(_localizationService.GetResource("Address.Update.Success", _workContext.WorkingLanguage.Id));
                ViewBag.Msg = msg;
            }

            return View(model);
        }


        [NopHttpsRequirement(SslRequirement.Yes)]
        public ActionResult AddressesPartial(int? id)
        {
            if (!IsCurrentUserRegistered())
                return new HttpUnauthorizedResult();

            if (TempData["modelState"] != null)
                ModelState.Merge(TempData["modelState"] as ModelStateDictionary);

            var customer = _workContext.CurrentCustomer;

            var model = new CustomerAddressListModel();
            model.NavigationModel = GetCustomerNavigationModel(customer);
            model.NavigationModel.SelectedTab = CustomerNavigationEnum.Addresses;
            foreach (var address in customer.Addresses)
                model.Addresses.Add(address.ToModel());
            model.Addresses.Add(new AddressModel() { Id = 0, Name = "YENİ ADRES ..." });

            if (!id.HasValue)
                model.SelectedAddressModel = model.Addresses.FirstOrDefault();
            else
                model.SelectedAddressModel = model.Addresses.FirstOrDefault(x => x.Id == id.Value);

            //model.SelectedAddressModel.AvailableCountries.Add(new SelectListItem() { Text = _localizationService.GetResource("Address.SelectCountry"), Value = "0" });
            foreach (var c in _countryService.GetAllCountries())
                model.SelectedAddressModel.AvailableCountries.Add(new SelectListItem() { Text = c.Name, Value = c.Id.ToString(), Selected = (c.Id == model.SelectedAddressModel.CountryId) });
            //states
            var states = model.SelectedAddressModel.CountryId != null ? _stateProvinceService.GetStateProvincesByCountryId(model.SelectedAddressModel.CountryId.Value).ToList() : new List<StateProvince>();
            if (states.Count > 0)
            {
                foreach (var s in states)
                    model.SelectedAddressModel.AvailableStates.Add(new SelectListItem() { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == model.SelectedAddressModel.StateProvinceId) });
            }
            else
                model.SelectedAddressModel.AvailableStates.Add(new SelectListItem() { Text = _localizationService.GetResource("Address.OtherNonUS"), Value = "0" });

            return View("AddressesOverviewEdit", model);

        }

        [NopHttpsRequirement(SslRequirement.Yes)]
        public ActionResult AddressDelete(int addressId)
        {
            if (!IsCurrentUserRegistered())
                return new HttpUnauthorizedResult();

            var customer = _workContext.CurrentCustomer;

            //find address (ensure that it belongs to the current customer)
            var address = customer.Addresses.Where(a => a.Id == addressId).FirstOrDefault();
            if (address != null)
            {
                customer.RemoveAddress(address);
                _customerService.UpdateCustomer(customer);
                _addressService.DeleteAddress(address);
            }

            return RedirectToAction("Addresses");
        }

        [NopHttpsRequirement(SslRequirement.Yes)]
        public ActionResult AddressAdd()
        {
            if (!IsCurrentUserRegistered())
                return new HttpUnauthorizedResult();

            var customer = _workContext.CurrentCustomer;

            var model = new CustomerAddressEditModel();
            model.NavigationModel = GetCustomerNavigationModel(customer);
            model.NavigationModel.SelectedTab = CustomerNavigationEnum.Addresses;

            model.Address = new AddressModel();
            //countries
            //model.Address.AvailableCountries.Add(new SelectListItem() { Text = _localizationService.GetResource("Address.SelectCountry"), Value = "0" });
            foreach (var c in _countryService.GetAllCountries())
                model.Address.AvailableCountries.Add(new SelectListItem() { Text = c.Name, Value = c.Id.ToString() });
            model.Address.AvailableStates.Add(new SelectListItem() { Text = _localizationService.GetResource("Address.OtherNonUS"), Value = "0" });

            return View(model);
        }

        [HttpPost]
        public ActionResult AddressAdd(CustomerAddressEditModel model)
        {
            if (!IsCurrentUserRegistered())
                return new HttpUnauthorizedResult();

            var customer = _workContext.CurrentCustomer;

            model.NavigationModel = GetCustomerNavigationModel(customer);
            model.NavigationModel.SelectedTab = CustomerNavigationEnum.Addresses;


            if (ModelState.IsValid)
            {
                var address = model.Address.ToEntity();
                address.Email = _workContext.CurrentCustomer.Email;
                address.CreatedOnUtc = DateTime.UtcNow;
                //some validation
                if (address.CountryId == 0)
                    address.CountryId = null;
                if (address.StateProvinceId == 0)
                    address.StateProvinceId = null;

                customer.AddAddress(address);
                _customerService.UpdateCustomer(customer);

                return RedirectToAction("Addresses");
            }


            //If we got this far, something failed, redisplay form
            //countries
            //model.Address.AvailableCountries.Add(new SelectListItem() { Text = _localizationService.GetResource("Address.SelectCountry"), Value = "0" });
            foreach (var c in _countryService.GetAllCountries())
                model.Address.AvailableCountries.Add(new SelectListItem() { Text = c.Name, Value = c.Id.ToString(), Selected = (c.Id == model.Address.CountryId) });
            //states
            var states = model.Address.CountryId.HasValue ? _stateProvinceService.GetStateProvincesByCountryId(model.Address.CountryId.Value).ToList() : _stateProvinceService.GetStateProvincesByCountryId(Convert.ToInt32(model.Address.AvailableCountries.First().Value));
            if (states.Count > 0)
            {
                foreach (var s in states)
                    model.Address.AvailableStates.Add(new SelectListItem() { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == model.Address.StateProvinceId) });
            }
            else
                model.Address.AvailableStates.Add(new SelectListItem() { Text = _localizationService.GetResource("Address.OtherNonUS"), Value = "0" });

            return View(model);
        }

        [NopHttpsRequirement(SslRequirement.Yes)]
        public ActionResult AddressEdit(int addressId)
        {
            if (!IsCurrentUserRegistered())
                return new HttpUnauthorizedResult();

            var customer = _workContext.CurrentCustomer;

            var model = new CustomerAddressEditModel();
            model.NavigationModel = GetCustomerNavigationModel(customer);
            model.NavigationModel.SelectedTab = CustomerNavigationEnum.Addresses;

            //find address (ensure that it belongs to the current customer)
            var address = customer.Addresses.Where(a => a.Id == addressId).FirstOrDefault();
            if (address == null)
                //address is not found
                return RedirectToAction("Addresses");

            model.Address = address.ToModel();
            //countries
            //model.Address.AvailableCountries.Add(new SelectListItem() { Text = _localizationService.GetResource("Address.SelectCountry"), Value = "0" });
            foreach (var c in _countryService.GetAllCountries())
                model.Address.AvailableCountries.Add(new SelectListItem() { Text = c.Name, Value = c.Id.ToString(), Selected = (c.Id == address.CountryId) });
            //states
            var states = address.Country != null ? _stateProvinceService.GetStateProvincesByCountryId(address.Country.Id).ToList() : _stateProvinceService.GetStateProvincesByCountryId(Convert.ToInt32(model.Address.AvailableCountries.First().Value));
            if (states.Count > 0)
            {
                foreach (var s in states)
                    model.Address.AvailableStates.Add(new SelectListItem() { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == address.StateProvinceId) });
            }
            else
                model.Address.AvailableStates.Add(new SelectListItem() { Text = _localizationService.GetResource("Address.OtherNonUS"), Value = "0" });

            return View(model);
        }

        [HttpGet]
        [NopHttpsRequirement(SslRequirement.Yes)]
        public string GetAddressBilling(string id)
        {
            Address _address;
            _address = _addressService.GetAddressById(Convert.ToInt32(id));
            AddressModel model = new AddressModel();
            model = _address.ToModel();
            PrepareAddressModelDropDowns(model);
            return Nop.Web.Infrastructure.Utilities.RenderPartialViewToString(this, @"~/Views\Customer\AddressEditBilling.cshtml", model);
        }
        [HttpGet]
        [NopHttpsRequirement(SslRequirement.Yes)]
        public string GetAddressShipping(string id)
        {
            Address _address;
            _address = _addressService.GetAddressById(Convert.ToInt32(id));
            AddressModel model = new AddressModel();
            model = _address.ToModel();
            PrepareAddressModelDropDowns(model);
            return Nop.Web.Infrastructure.Utilities.RenderPartialViewToString(this, @"~/Views\Customer\AddressEdit.cshtml", model);
        }

        [HttpPost]
        public ActionResult AddressEdit(AddressModel model)
        {
            int id = model.Id;
            if (!IsCurrentUserRegistered())
                return new HttpUnauthorizedResult();
            var customer = _workContext.CurrentCustomer;


            //Add new address


            if (id == 0)
            {
                var addressName = _workContext.CurrentCustomer.Addresses.Where(x => x.Name == model.Name).ToList();
                if (addressName.Count != 0)
                    return RedirectToAction("Addresses", new { id1 = 2 });
                if (model.DefaultBillingAddress)
                {
                    foreach (var adres in customer.Addresses)
                    {
                        adres.DefaultBillingAddress = false;
                        _addressService.UpdateAddress(adres);
                    }
                }
                if (model.DefaultShippingAddress)
                {
                    foreach (var adres in customer.Addresses)
                    {
                        adres.DefaultShippingAddress = false;
                        _addressService.UpdateAddress(adres);
                    }
                }
                var address = model.ToEntity();
                address.Email = _workContext.CurrentCustomer.Email;
                address.CreatedOnUtc = DateTime.UtcNow;
                //some validation
                if (address.CountryId == 0)
                    address.CountryId = null;
                if (address.StateProvinceId == 0)
                    address.StateProvinceId = null;

                customer.AddAddress(address);
                _customerService.UpdateCustomer(customer);
                return RedirectToAction("Addresses", new { id = address.Id, id1 = 0 });

            }
            //find address (ensure that it belongs to the current customer)
            else
            {
                var address = customer.Addresses.Where(a => a.Id == id).FirstOrDefault();
                if (model.DefaultBillingAddress == true)
                {
                    foreach (var adres in customer.Addresses)
                    {
                        adres.DefaultBillingAddress = false;
                        _addressService.UpdateAddress(adres);
                    }
                    address.DefaultBillingAddress = true;
                }
                if (model.DefaultShippingAddress == true)
                {
                    foreach (var adres in customer.Addresses)
                    {
                        adres.DefaultShippingAddress = false;
                        _addressService.UpdateAddress(adres);
                    }
                    address.DefaultShippingAddress = true;
                }
                if (address == null)
                    //address is not found
                    return RedirectToAction("Addresses");

                address = model.ToEntity(address);
                address.Email = _workContext.CurrentCustomer.Email;
                //some validation
                if (address.CountryId == 0)
                    address.CountryId = null;
                if (address.StateProvinceId == 0)
                    address.StateProvinceId = null;

                _addressService.UpdateAddress(address);
                return RedirectToAction("Addresses", new { id = address.Id, id1 = 1 });
            }
        }

        #endregion

        #region Orders

        [NopHttpsRequirement(SslRequirement.Yes)]
        public ActionResult Orders()
        {
            if (!IsCurrentUserRegistered())
                return new HttpUnauthorizedResult();

            var customer = _workContext.CurrentCustomer;
            var model = PrepareCustomerOrderListModel(customer);
            if (model.Orders.Count == 0)
            {
                var messageModel = new MessageModel();
                messageModel.MessageList.Add(_localizationService.GetResource("Myaccount.Overview.OrdersPage.EmptyMessage"));
                messageModel.ActionText = _localizationService.GetResource("ShoppingCart.CartIsEmpty.Continue");
                messageModel.ActionUrl = ControllerContext.HttpContext.Request.ApplicationPath;
                ViewBag.MessageModel = messageModel;
            }
            return View(model);
        }

        private CustomerOrderListModel PrepareCustomerOrderListModel(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var model = new CustomerOrderListModel();
            model.NavigationModel = GetCustomerNavigationModel(customer);
            model.NavigationModel.SelectedTab = CustomerNavigationEnum.Orders;
            var orders = _orderService.GetOrdersByCustomerId(customer.Id);
            var ordersInOrder = orders.OrderByDescending(x => x.CreatedOnUtc).ToList();
           
            foreach (var order in ordersInOrder)
            {
                var orderModel = new CustomerOrderListModel.OrderDetailsModel()
                {
                    Id = order.Id,
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc),
                    OrderStatus = order.OrderStatus.ToString(),
                    OrderStatusText = order.OrderStatus.GetLocalizedEnum(_localizationService, _workContext),
                    IsReturnRequestAllowed = _orderProcessingService.IsReturnRequestAllowed(order),
                    ShippingStatus=order.ShippingStatus.GetLocalizedEnum(_localizationService, _workContext)
                };
                if (order.ShippingStatus == ShippingStatus.Shipped)
                    orderModel.OrderStatusText = order.ShippingStatus.GetLocalizedEnum(_localizationService, _workContext);
                var orderTotalInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
                orderModel.OrderTotal = _priceFormatter.FormatPrice(orderTotalInCustomerCurrency, true, order.CustomerCurrencyCode, false, _workContext.WorkingLanguage);
                orderModel.Detail = PrepareOrderDetailsModel(order);

                model.Orders.Add(orderModel);
            }

            var recurringPayments = _orderService.SearchRecurringPayments(customer.Id, 0, null);
            foreach (var recurringPayment in recurringPayments)
            {
                var recurringPaymentModel = new CustomerOrderListModel.RecurringOrderModel()
                {
                    Id = recurringPayment.Id,
                    StartDate = _dateTimeHelper.ConvertToUserTime(recurringPayment.StartDateUtc, DateTimeKind.Utc).ToString(),
                    CycleInfo = string.Format("{0} {1}", recurringPayment.CycleLength, recurringPayment.CyclePeriod.GetLocalizedEnum(_localizationService, _workContext)),
                    NextPayment = recurringPayment.NextPaymentDate.HasValue ? _dateTimeHelper.ConvertToUserTime(recurringPayment.NextPaymentDate.Value, DateTimeKind.Utc).ToString() : "",
                    TotalCycles = recurringPayment.TotalCycles,
                    CyclesRemaining = recurringPayment.CyclesRemaining,
                    InitialOrderId = recurringPayment.InitialOrder.Id,
                    CanCancel = _orderProcessingService.CanCancelRecurringPayment(customer, recurringPayment),
                };

                model.RecurringOrders.Add(recurringPaymentModel);
            }

            return model;
        }
        
        public ActionResult GetDownloadSalesAgreement(int id)
        {

            var orderItem = _orderService.GetOrderById(id);

            if (orderItem == null)
                return Content("Sorry Order Not Find");

            string p = _webHelper.GetThisPageUrl(false);

            if (_workContext.CurrentCustomer.IsGuest())
                return RedirectToRoute("login");

            if (_workContext.CurrentCustomer.Id != orderItem.Customer.Id)
                return Content("This is Not your order");


            string filePath = HostingEnvironment.MapPath("~/Content/Pdf");
            var path = filePath + "\\SalesAgreement-" + orderItem.Id + ".pdf";
            Response.ContentType = "Application/pdf";
            Response.AppendHeader("Content-Disposition", "attachment; filename=SalesAgreement.pdf");
            Response.TransmitFile(path);
            return null;

        }

        #endregion

        #region Return request

        [NopHttpsRequirement(SslRequirement.Yes)]
        public ActionResult ReturnRequests()
        {
            if (!IsCurrentUserRegistered())
                return new HttpUnauthorizedResult();

            var customer = _workContext.CurrentCustomer;

            var model = new CustomeReturnRequestsModel();
            model.NavigationModel = GetCustomerNavigationModel(customer);
            model.NavigationModel.SelectedTab = CustomerNavigationEnum.ReturnRequests;
            var returnRequests = _orderService.SearchReturnRequests(customer.Id, 0, null);
            foreach (var returnRequest in returnRequests)
            {
                var opv = _orderService.GetOrderProductVariantById(returnRequest.OrderProductVariantId);
                var pv = opv.ProductVariant;

                var itemModel = new CustomeReturnRequestsModel.ReturnRequestModel()
                {
                    Id = returnRequest.Id,
                    ReturnRequestStatus = returnRequest.ReturnRequestStatus.GetLocalizedEnum(_localizationService, _workContext),
                    ProductId = pv.ProductId,
                    ProductSeName = pv.Product.GetSeName(),
                    Quantity = returnRequest.Quantity,
                    ReturnAction = returnRequest.RequestedAction,
                    ReturnReason = returnRequest.ReasonForReturn,
                    Comments = returnRequest.CustomerComments,
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(returnRequest.CreatedOnUtc, DateTimeKind.Utc),
                };
                model.Items.Add(itemModel);

                if (!String.IsNullOrEmpty(pv.GetLocalized(x => x.Name)))
                    itemModel.ProductName = string.Format("{0} ({1})", pv.Product.GetLocalized(x => x.Name), pv.GetLocalized(x => x.Name));
                else
                    itemModel.ProductName = pv.Product.GetLocalized(x => x.Name);
            }

            return View(model);
        }

        #endregion

        #region Reward points

        [NopHttpsRequirement(SslRequirement.Yes)]
        public ActionResult RewardPoints()
        {
            if (!IsCurrentUserRegistered())
                return new HttpUnauthorizedResult();

            if (!_rewardPointsSettings.Enabled)
                return RedirectToAction("MyAccount");

            var customer = _workContext.CurrentCustomer;

            var model = new CustomerRewardPointsModel();
            model.NavigationModel = GetCustomerNavigationModel(customer);
            model.NavigationModel.SelectedTab = CustomerNavigationEnum.RewardPoints;
            foreach (var rph in customer.RewardPointsHistory.OrderByDescending(rph => rph.CreatedOnUtc).ThenByDescending(rph => rph.Id))
            {
                model.RewardPoints.Add(new CustomerRewardPointsModel.RewardPointsHistoryModel()
                {
                    Points = rph.Points,
                    PointsBalance = rph.PointsBalance,
                    Message = rph.Message,
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(rph.CreatedOnUtc, DateTimeKind.Utc)
                });
            }
            int rewardPointsBalance = customer.GetRewardPointsBalance();
            decimal rewardPointsAmountBase = _orderTotalCalculationService.ConvertRewardPointsToAmount(rewardPointsBalance);
            decimal rewardPointsAmount = _currencyService.ConvertFromPrimaryStoreCurrency(rewardPointsAmountBase, _workContext.WorkingCurrency);
            model.RewardPointsBalance = string.Format(_localizationService.GetResource("RewardPoints.CurrentBalance"), rewardPointsBalance, _priceFormatter.FormatPrice(rewardPointsAmount, true, false));

            return View(model);
        }

        #endregion

        #region Change password

        [NopHttpsRequirement(SslRequirement.Yes)]
        public ActionResult ChangePassword()
        {
            if (!IsCurrentUserRegistered())
                return new HttpUnauthorizedResult();

            var customer = _workContext.CurrentCustomer;

            var model = new ChangePasswordModel();
            model.NavigationModel = GetCustomerNavigationModel(customer);
            model.NavigationModel.SelectedTab = CustomerNavigationEnum.ChangePassword;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            if (!IsCurrentUserRegistered())
                return new HttpUnauthorizedResult();

            var customer = _workContext.CurrentCustomer;

            model.NavigationModel = GetCustomerNavigationModel(customer);
            model.NavigationModel.SelectedTab = CustomerNavigationEnum.ChangePassword;

            if (ModelState.IsValid)
            {
                var changePasswordRequest = new ChangePasswordRequest(customer.Email,
                    false, PasswordFormat.Hashed, model.NewPassword, model.OldPassword);
                var changePasswordResult = _customerRegistrationService.ChangePassword(changePasswordRequest);
                if (changePasswordResult.Success)
                {
                    model.Result = _localizationService.GetResource("Account.ChangePassword.Success");
                    return View(model);
                }
                else
                {
                    foreach (var error in changePasswordResult.Errors)
                        ModelState.AddModelError("", error);
                }
            }


            //If we got this far, something failed, redisplay form
            return View(model);
        }

        #endregion



        [NopHttpsRequirement(SslRequirement.Yes)]
        public ActionResult Overview()
        {
            Customer customer = _workContext.CurrentCustomer;
            var model = new CustomerInfoModel();
            PrepareCustomerInfoModel(model, customer, false);
            model.NavigationModel.SelectedTab = CustomerNavigationEnum.Overview;
            ViewData["customer"] = customer;
            return View("CustomerOverview", model);
        }




        #endregion

        #region Password recovery

        [NopHttpsRequirement(SslRequirement.Yes)]
        public ActionResult PasswordRecovery()
        {
            var model = new PasswordRecoveryModel();
            return View(model);
        }

        [HttpPost, ActionName("PasswordRecovery")]
        [FormValueRequired("send-email")]
        public ActionResult PasswordRecoverySend(PasswordRecoveryModel model)
        {
            if (ModelState.IsValid)
            {
                var customer = _customerService.GetCustomerByEmail(model.Email);
                if (customer != null)
                {
                    var passwordRecoveryToken = Guid.NewGuid();
                    _customerService.SaveCustomerAttribute(customer, SystemCustomerAttributeNames.PasswordRecoveryToken, passwordRecoveryToken.ToString());
                    _workflowMessageService.SendCustomerPasswordRecoveryMessage(customer, _workContext.WorkingLanguage.Id);

                    model.Result = _localizationService.GetResource("Account.PasswordRecovery.EmailHasBeenSent");
                    return View("RecoveryEmailSent", model);
                }
                else
                    model.Result = _localizationService.GetResource("Account.PasswordRecovery.EmailNotFound");

                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        //TODO:Af- delete this method
        //public ActionResult BulkPasswordRecover(int id1,int id2)
        //{
        //    if (_workContext.CurrentCustomer.CustomerRoles.FirstOrDefault(x => x.SystemName == "Administrators") == null)
        //        return Content("");

        //    for (int i = id1; i <= id2; i++)
        //    {
        //        var customer = _customerService.GetCustomerById(i);
        //        if (customer != null)
        //        {
        //            var passwordRecoveryToken = Guid.NewGuid();
        //            _customerService.SaveCustomerAttribute(customer, SystemCustomerAttributeNames.PasswordRecoveryToken, passwordRecoveryToken.ToString());
        //            _workflowMessageService.SendCustomerMigrationPasswordRecoveryMessage(customer, _workContext.WorkingLanguage.Id);

        //        }
        //    }

        //    return Content("");
        //}

        



        [NopHttpsRequirement(SslRequirement.Yes)]
        public ActionResult PasswordRecoveryConfirm(string token, string email)
        {
            var customer = _customerService.GetCustomerByEmail(email);
            if (customer == null)
                return RedirectToAction("Index", "Home");

            var cPrt = customer.GetAttribute<string>(SystemCustomerAttributeNames.PasswordRecoveryToken);
            if (String.IsNullOrEmpty(cPrt))
                return RedirectToAction("Index", "Home");

            if (!cPrt.Equals(token, StringComparison.InvariantCultureIgnoreCase))
                return RedirectToAction("Index", "Home");

            var model = new PasswordRecoveryConfirmModel();
            return View(model);
        }

        [HttpPost, ActionName("PasswordRecoveryConfirm")]
        [FormValueRequired("set-password")]
        public ActionResult PasswordRecoveryConfirmPOST(string token, string email, PasswordRecoveryConfirmModel model)
        {
            var customer = _customerService.GetCustomerByEmail(email);
            if (customer == null)
                return RedirectToAction("Index", "Home");

            var cPrt = customer.GetAttribute<string>(SystemCustomerAttributeNames.PasswordRecoveryToken);
            if (String.IsNullOrEmpty(cPrt))
                return RedirectToAction("Index", "Home");

            if (!cPrt.Equals(token, StringComparison.InvariantCultureIgnoreCase))
                return RedirectToAction("Index", "Home");

            if (ModelState.IsValid)
            {
                var response = _customerRegistrationService.ChangePassword(new ChangePasswordRequest(email,
                    false, PasswordFormat.Hashed, model.NewPassword));
                if (response.Success)
                {
                    _customerService.SaveCustomerAttribute(customer, SystemCustomerAttributeNames.PasswordRecoveryToken, "");

                    model.SuccessfullyChanged = true;
                    model.Result = _localizationService.GetResource("Account.PasswordRecovery.PasswordHasBeenChanged");
                    return View("RecoveryConfirmMessage");
                }
                else
                {
                    model.Result = response.Errors.FirstOrDefault();
                }

                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        #endregion

        #region Customer Discount

        public ActionResult CustomerDiscount(int customerId, int productVariantId)
        {

            var quote = _customerService.GetCustomerProductVariantQuoteByVariantId(customerId, productVariantId);
            var productVariant = _productService.GetProductVariantById(productVariantId);
            if (productVariant == null)
                return RedirectToAction("Index","Home");
            if (quote != null)
            {
                if (_workContext.CurrentCustomer.Id != quote.CustomerId &&(_workContext.CurrentCustomer.Email==null ||  _workContext.CurrentCustomer.Email.Trim().ToLower() != quote.Email.Trim().ToLower()))
                    return RedirectToAction("Login", "Customer");

                quote.ActivateDate = DateTime.Now;
                _customerService.UpdateCustomerProductVariantQuote(quote);

                var customer = _customerService.GetCustomerByEmail(quote.Email);

                if (customer != null/* && customer.Id != customerId*/)
                {
                    var previousQuote = customer.CustomerProductVariantQuotes.FirstOrDefault(x => x.ProductVariantId == quote.ProductVariantId);
                    if (previousQuote != null)
                    {
                        previousQuote.ActivateDate = quote.ActivateDate;
                        _customerService.UpdateCustomerProductVariantQuote(previousQuote);
                    }
                    else
                    {
                        _customerService.InsertCustomerProductVariantQuote(new CustomerProductVariantQuote()
                        {
                            ActivateDate = quote.ActivateDate,
                            RequestDate = quote.RequestDate,
                            CustomerId = customer.Id,
                            ProductVariantId = quote.ProductVariantId,
                            Email = customer.Email,
                            Description = quote.Description,
                            DiscountPercentage = quote.DiscountPercentage,
                            Enquiry = quote.Enquiry,
                            PhoneNumber = quote.PhoneNumber,
                            PriceWithDiscount = quote.PriceWithDiscount,
                            PriceWithoutDiscount = quote.PriceWithoutDiscount
                        });
                    }
                }
               

                return RedirectToRoute("Product", new { productId = productVariant.Product.Id, variantId = productVariant.Id, SeName = productVariant.Product.GetSeName() });
            }
            else
            {
                return RedirectToRoute("Product", new { productId = productVariant.Product.Id, variantId = productVariant.Id, SeName = productVariant.Product.GetSeName() });
            }

        }

        #endregion

        //public ActionResult ExportMadMimi()
        //{
        //    //if (!_permissionService.Authorize(StandardPermissionProvider.ManageNewsletterSubscribers))
        //    //    return AccessDeniedView();

        //    string fileName = String.Format("newsletter_emails_{0}_{1}_{2}_{3}.csv", "madmimi", "export", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), CommonHelper.GenerateRandomDigitCode(4));
        //    string sImportType = "add_list";
        //    var sb = new StringBuilder();
        //    var newsLetterSubscriptions = _newsLetterSubscriptionService.GetAllNewsLetterSubscriptions(null, 0, int.MaxValue, true);
        //    if (newsLetterSubscriptions.Count == 0)
        //    {
        //        throw new NopException("No emails to export");
        //    }

        //    sb.Append(string.Format("email,first name,last name,{0}", sImportType));
        //    sb.Append("\\");
        //    for (int i = 0; i < newsLetterSubscriptions.Count; i++)
        //    {
        //        var subscription = newsLetterSubscriptions[i];
        //        sb.Append(subscription.Email + ",");
        //        sb.Append(subscription.FirstName + ",");
        //        sb.Append(subscription.LastName + ",");
        //        sb.Append("customer");
        //        sb.Append("\\");
        //    }
        //    string result = sb.ToString();
            
        //    using (WebClient client = new WebClient())
        //    {
        //        System.Collections.Specialized.NameValueCollection reqparm = new System.Collections.Specialized.NameValueCollection();
        //        reqparm.Add("username", "elberbozovali@gmail.com");
        //        reqparm.Add("api_key", "f48a937566b27230fe12ff2f2c8b37a1");
        //        reqparm.Add("csv_file", result);
        //        reqparm.Add("audience_list", "elber");
        //        byte[] responsebytes = client.UploadValues("http://api.madmimi.com/audience_members", "POST", reqparm);
        //        string responsebody = Encoding.UTF8.GetString(responsebytes);
        //    }

        //    return File(Encoding.UTF8.GetBytes(result), "text/csv", fileName);
        //}
    }
}
