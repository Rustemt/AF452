using System;
using System.Web.Routing;
using Nop.Core.Domain.Payments;
using Nop.Core.Plugins;
using Nop.Plugin.Payments.FinansBank.Controllers;
using Nop.Services.Configuration;
using Nop.Services.Payments;
using ePayment;
using Nop.Core.Domain.Directory;
using Nop.Services.Directory;
using Nop.Core;
using Nop.Core.Domain;
using System.Globalization;
using Nop.Services.Localization;
using Nop.Core.Domain.Orders;
using Nop.Services.Customers;
using Nop.Services.Logging;
using Nop.Core.Infrastructure;

namespace Nop.Plugin.Payments.FinansBank
{
    /// <summary>
    /// FinansBank payment processor
    /// </summary>
    public class FinansBankPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly FinansBankPaymentSettings _FinansBankPaymentSettings;
        private readonly ISettingService _settingService;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly IWebHelper _webHelper;
        private readonly StoreInformationSettings _storeInformationSettings;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerService _customerService;

        #endregion

        #region Ctor

        public FinansBankPaymentProcessor(FinansBankPaymentSettings FinansBankPaymentSettings,
         ISettingService settingService, ICurrencyService currencyService,
            CurrencySettings currencySettings, IWebHelper webHelper,
            StoreInformationSettings storeInformationSettings,
            ILocalizationService localizationService,
            ICustomerService customerService)
        {
            this._FinansBankPaymentSettings = FinansBankPaymentSettings;
            this._settingService = settingService;
            this._currencyService = currencyService;
            this._currencySettings = currencySettings;
            this._webHelper = webHelper;
            this._storeInformationSettings = storeInformationSettings;
            this._localizationService = localizationService;
            this._customerService = customerService;
        }

        #endregion

        #region Methods
        public static string GetCulturePrice(string currency, decimal price)
        {
            switch (currency)
            {
                case "840":
                    return Math.Round(price, 2).ToString("N", new CultureInfo("en-us"));
                case "978":
                    return Math.Round(price, 2).ToString("N", new CultureInfo("en-us"));
                case "949":
                    return Math.Round(price, 2).ToString("N", new CultureInfo("tr-tr")).Replace(".", "");
                default:
                    return Math.Round(price, 2).ToString("N", new CultureInfo("tr-tr")).Replace(".", "");
            }
        

            
        }

        public static string GetCurrency(Currency currency)
        {
            switch (currency.CurrencyCode)
            {
                case "USD":
                    return "840";
                case "EUR":
                    return "978";
                case "TL":
                    return "949";
                default:
                    return "949";
            }
        }

        public static string GetMonth(string value)
        {
            if (value.Length == 2) return value;
            return value.PadLeft(2, '0');
        }

        //TODO: it is suggested to always show a generic message
        private string ConvertPaymentMessage(string code)
        {
            switch (code)
            {
                case "0": 
                    return _localizationService.GetResource("Checkout.Payment.LimitFailed");
                case "13":
                    return _localizationService.GetResource("Checkout.Payment.ConnectionFailed");
                default:
                    return _localizationService.GetResource("Checkout.Payment.Error");

            }
        }


        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee()
        {
            return 0;
        }

 
        public PreProcessPaymentResult PreProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new PreProcessPaymentResult();
            if (processPaymentRequest.CreditCardType.ToLower().Trim() == "visa")
                result.CardType = "1";
            if (processPaymentRequest.CreditCardType.ToLower().Trim() == "mastercard")
                result.CardType = "2";
            //CC shall be either visa or master card!
            if (result.CardType != "1" && result.CardType != "2")
                return result;
            result.PaymentRequest = processPaymentRequest;
            result.FailUrl = _FinansBankPaymentSettings.ErrorURL;
            result.OkUrl = _FinansBankPaymentSettings.SuccessURL;
            result.RandomToken = DateTime.Now.ToString();
            result.RedirectURL = _FinansBankPaymentSettings.HostAddress3D;
            result.StoreType = _FinansBankPaymentSettings.StoreType;
            result.StoreKey = _FinansBankPaymentSettings.StoreKey;
            result.ChargeType = "Auth";
            result.Lang = "tr";
            result.Currency = "949";
            result.StoreName = _FinansBankPaymentSettings.StoreName;
            result.ClientId = _FinansBankPaymentSettings.ClientId;
            result.OrderId = processPaymentRequest.OrderGuid.ToString();
            string installment = processPaymentRequest.Installment <= 1 ? "" : processPaymentRequest.Installment.ToString();
            result.Installment = installment;
            result.CreditCardNumber = processPaymentRequest.CreditCardNumber;
            result.CreditCardExpireYear = (processPaymentRequest.CreditCardExpireYear % 100).ToString();
            result.CreditCardExpireMonth = GetMonth(processPaymentRequest.CreditCardExpireMonth.ToString()); ;
            result.CreditCardCvv2 = processPaymentRequest.CreditCardCvv2;
            result.CreditCardNumber = processPaymentRequest.CreditCardNumber;
            result.CustomerId = processPaymentRequest.CustomerId.ToString();
            var currency = GetCurrency(_currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId));
            result.Amount = GetCulturePrice(currency, processPaymentRequest.OrderTotal);
           
            
            String hashstr = result.ClientId +
                             result.OrderId +
                             result.Amount +
                             result.OkUrl +
                            result.FailUrl +
                            result.ChargeType +
                            result.Installment +//Installment- for now we do not use 
                            result.RandomToken +
                            result.StoreKey;
            System.Security.Cryptography.SHA1 sha = new System.Security.Cryptography.SHA1CryptoServiceProvider();
            byte[] hashbytes = System.Text.Encoding.GetEncoding("ISO-8859-9").GetBytes(hashstr);
            byte[] inputbytes = sha.ComputeHash(hashbytes);
            String hash = Convert.ToBase64String(inputbytes);
            result.Hash = hash;

            result.RequiresRedirection = true;

            //var customer = _customerService.GetCustomerById(processPaymentRequest.CustomerId);

         

            return result;
        }




        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
                return AuthorizeOrSale(processPaymentRequest);
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            //nothing
        }

        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            //it's not a redirection payment method. So we always return false
            return false;
        }


        protected ProcessPaymentResult AuthorizeOrSale(ProcessPaymentRequest processPaymentRequest)
        {
            var customer = _customerService.GetCustomerById(processPaymentRequest.CustomerId);
            cc5payment ccpayment = new cc5payment();
            var hostAddress = _FinansBankPaymentSettings.UseTestServer ? _FinansBankPaymentSettings.TestServiceUrl : _FinansBankPaymentSettings.ServiceUrl;
            ccpayment.host = hostAddress;// "https://netpos.finansbank.com.tr/servlet/cc5ApiServer";//"https://testsanalpos.est.com.tr/servlet/cc5ApiServer";//
            ccpayment.name = _FinansBankPaymentSettings.UseTestServer?_FinansBankPaymentSettings.TestName :_FinansBankPaymentSettings.Name;// "sanalpos"; //"KUVEYTAPI";//
            ccpayment.password = _FinansBankPaymentSettings.UseTestServer?_FinansBankPaymentSettings.TestPassword:_FinansBankPaymentSettings.Password;// "V7Z4M3SgmiJ";//"KUVEYT123";//
            ccpayment.clientid = _FinansBankPaymentSettings.UseTestServer ? _FinansBankPaymentSettings.TestClientId : _FinansBankPaymentSettings.ClientId;//"110000810";//"110000000";//
            ccpayment.chargetype = "Auth";
            ccpayment.orderresult =_FinansBankPaymentSettings.TestOrder ? 1 : 0;
            ccpayment.cardnumber = processPaymentRequest.CreditCardNumber;
            ccpayment.expmonth = GetMonth(processPaymentRequest.CreditCardExpireMonth.ToString());
            ccpayment.expyear = (processPaymentRequest.CreditCardExpireYear%100).ToString();
            ccpayment.cv2 = processPaymentRequest.CreditCardCvv2;
            var currency = GetCurrency(_currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId));
            ccpayment.currency = currency;
            ccpayment.subtotal = GetCulturePrice(currency, processPaymentRequest.OrderTotal);

            ccpayment.oid = processPaymentRequest.OrderGuid.ToString();
            ccpayment.userid = processPaymentRequest.CustomerId.ToString();
            ccpayment.email = customer.BillingAddress.Email;

            string installment = processPaymentRequest.Installment <= 1 ? "" : processPaymentRequest.Installment.ToString();
            ccpayment.taksit = installment;

            var ccpaymentResult = ccpayment.processorder();
            var result = new ProcessPaymentResult();

            if (ccpaymentResult == "0")//can not communicate
            {
                result.Errors.Add(this.ConvertPaymentMessage("0"));

            }
            else
            {
                if (ccpayment.appr.ToLower() == "approved")//success
                {
                    result.AuthorizationTransactionId = ccpayment.transid;
                }
                else
                {
                    result.Errors.Add( this.ConvertPaymentMessage(ccpayment.code));
                    ILogger loger = EngineContext.Current.Resolve<ILogger>();
                    loger.Error("Payments.CC.FinansBank: errorcode:" + ccpayment.code + "\n\r errortext:" + ccpayment.errmsg);
             
                }

            }
            return result;
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();
            result.AddError("Capture method not supported");
            return result;
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();
            result.AddError("Refund method not supported");
            return result;
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();
            result.AddError("Void method not supported");
            return result;
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.AddError("Recurring method not supported");
            return result;
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            //always success
            return new CancelRecurringPaymentResult();
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "PaymentFinansBank";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.FinansBank.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Gets a route for payment info
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PaymentInfo";
            controllerName = "PaymentFinansBank";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.FinansBank.Controllers" }, { "area", null } };
        }

        public Type GetControllerType()
        {
            return typeof(PaymentFinansBankController);
        }

        public override void Install()
        {
            var settings = new FinansBankPaymentSettings()
            {
                TransactMode = TransactMode.Pending
            };
            _settingService.SaveSetting(settings);

            base.Install();
        }

        #endregion

        #region Properies

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType
        {
            get
            {
                return RecurringPaymentType.Manual;
            }
        }

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType
        {
            get
            {
                return PaymentMethodType.Standard;
            }
        }

        #endregion
        

        
    }
}
