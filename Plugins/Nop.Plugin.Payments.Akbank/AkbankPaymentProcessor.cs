using System;
using System.Web.Routing;
using Nop.Core.Domain.Payments;
using Nop.Core.Plugins;
using Nop.Plugin.Payments.Akbank.Controllers;
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
using Nop.Web.Framework;

namespace Nop.Plugin.Payments.Akbank
{
    /// <summary>
    /// Akbank payment processor
    /// </summary>
    public class AkbankPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly AkbankPaymentSettings _AkbankPaymentSettings;
        private readonly ISettingService _settingService;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly IWebHelper _webHelper;
        private readonly StoreInformationSettings _storeInformationSettings;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public AkbankPaymentProcessor(AkbankPaymentSettings AkbankPaymentSettings,
         ISettingService settingService, ICurrencyService currencyService,
            CurrencySettings currencySettings, IWebHelper webHelper,
            StoreInformationSettings storeInformationSettings,
            ILocalizationService localizationService,
            ICustomerService customerService,
            IWorkContext workContext)
        {
            this._AkbankPaymentSettings = AkbankPaymentSettings;
            this._settingService = settingService;
            this._currencyService = currencyService;
            this._currencySettings = currencySettings;
            this._webHelper = webHelper;
            this._storeInformationSettings = storeInformationSettings;
            this._localizationService = localizationService;
            this._customerService = customerService;
            this._workContext = workContext;
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

            String taksit = _workContext.CurrentCustomer.GetAttribute<string>("taksit");
            String clientId = _AkbankPaymentSettings.ClientId;
            var currency = GetCurrency(_currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId));
            String amount = processPaymentRequest.OrderTotal.ToString("######.00"); //Decimal.Parse(GetCulturePrice(currency, processPaymentRequest.OrderTotal)).ToString("######.00"); //postProcessPaymentRequest.Order.OrderTotal.ToString("######.00");
            amount = amount.Replace(",", ".");
            Random rndOid = new Random();
            int oidRnd = rndOid.Next(111111111, 999999999);
            //result.OrderId = oidRnd.ToString("00000000000000000000");
            String oid = oidRnd.ToString("00000000000000000000");
            String okUrl = _AkbankPaymentSettings._3dokUrl;
            String failUrl = _AkbankPaymentSettings._3dfailUrl;
            String rnd = DateTime.Now.ToString();
            String islemtipi = "Auth";
            String storekey = _AkbankPaymentSettings._3dStoreKey;
            //String storetype = "3d_pay";
            String hashstr = clientId + oid + amount + okUrl + failUrl + islemtipi + taksit + rnd + storekey;
            System.Security.Cryptography.SHA1 sha = new System.Security.Cryptography.SHA1CryptoServiceProvider();
            byte[] hashbytes = System.Text.Encoding.GetEncoding("ISO-8859-9").GetBytes(hashstr);
            byte[] inputbytes = sha.ComputeHash(hashbytes);
            String hash = Convert.ToBase64String(inputbytes);

            string pan = processPaymentRequest.CreditCardNumber; //_encryptionService.DecryptText(postProcessPaymentRequest.Order.CardNumber);
            string cv2 = processPaymentRequest.CreditCardCvv2; //_encryptionService.DecryptText(postProcessPaymentRequest.Order.CardCvv2);
            string Ecom_Payment_Card_ExpDate_Year = (processPaymentRequest.CreditCardExpireYear % 100).ToString(); //_encryptionService.DecryptText(postProcessPaymentRequest.Order.CardExpirationYear);
            string Ecom_Payment_Card_ExpDate_Month = GetMonth(processPaymentRequest.CreditCardExpireMonth.ToString()); //_encryptionService.DecryptText(postProcessPaymentRequest.Order.CardExpirationMonth);            
            string cardType = processPaymentRequest.CreditCardType.ToLower().Trim(); //_encryptionService.DecryptText(postProcessPaymentRequest.Order.CardType);


            if (processPaymentRequest.CreditCardType.ToLower().Trim() == "visa")
                result.CardType = "1";
            if (processPaymentRequest.CreditCardType.ToLower().Trim() == "mastercard")
                result.CardType = "2";
            //CC shall be either visa or master card!
            if (result.CardType != "1" && result.CardType != "2")
                return result;
            result.PaymentRequest = processPaymentRequest;
            result.FailUrl = failUrl;
            result.OkUrl = okUrl;
            result.RandomToken = rnd;
            result.RedirectURL = _AkbankPaymentSettings._3durl;
            result.StoreType = _AkbankPaymentSettings._3dstoretype;
            result.StoreKey = _AkbankPaymentSettings._3dStoreKey;
            result.ChargeType = islemtipi;
            result.Lang = "tr";
            result.Currency = "949";
            result.ClientId = _AkbankPaymentSettings._3dclientid;
            result.OrderId = oid; // processPaymentRequest.OrderGuid.ToString();
            result.Installment = taksit;
            result.CreditCardNumber = processPaymentRequest.CreditCardNumber;
            result.CreditCardExpireYear = (processPaymentRequest.CreditCardExpireYear % 100).ToString();
            result.CreditCardExpireMonth = GetMonth(processPaymentRequest.CreditCardExpireMonth.ToString()); ;
            result.CreditCardCvv2 = processPaymentRequest.CreditCardCvv2;
            result.CreditCardNumber = processPaymentRequest.CreditCardNumber;
            result.CustomerId = processPaymentRequest.CustomerId.ToString();
            result.Amount = amount; // GetCulturePrice(currency, processPaymentRequest.OrderTotal);


            result.Hash = hash;

            result.RequiresRedirection = true;


            //postProcessPaymentRequest.Order.OrderNotes.Add(new OrderNote()
            //{
            //    Note = sbLog.ToString(),
            //    DisplayToCustomer = false,
            //    CreatedOnUtc = DateTime.UtcNow
            //});
            //_orderService.UpdateOrder(postProcessPaymentRequest.Order);

            //_logger.Information();  






            //rph.Post();






            //if (processPaymentRequest.CreditCardType.ToLower().Trim() == "visa")
            //    result.CardType = "1";
            //if (processPaymentRequest.CreditCardType.ToLower().Trim() == "mastercard")
            //    result.CardType = "2";
            ////CC shall be either visa or master card!
            //if (result.CardType != "1" && result.CardType != "2")
            //    return result;
            //result.PaymentRequest = processPaymentRequest;
            //result.FailUrl = _KuveytTurkPaymentSettings.ErrorURL;
            //result.OkUrl = _KuveytTurkPaymentSettings.SuccessURL;
            //result.RandomToken = DateTime.Now.ToString();
            //result.RedirectURL = _KuveytTurkPaymentSettings.HostAddress3D;
            //result.StoreType = _KuveytTurkPaymentSettings.StoreType;
            //result.StoreKey = _KuveytTurkPaymentSettings.StoreKey;
            //result.ChargeType = "Auth";
            //result.Lang = "tr";
            //result.Currency = "949";
            //result.StoreName = _KuveytTurkPaymentSettings.StoreName;
            //result.ClientId = _KuveytTurkPaymentSettings.ClientId;
            //result.OrderId = processPaymentRequest.OrderGuid.ToString();
            //result.Installment = "";
            //result.CreditCardNumber = processPaymentRequest.CreditCardNumber;
            //result.CreditCardExpireYear = (processPaymentRequest.CreditCardExpireYear % 100).ToString();
            //result.CreditCardExpireMonth = GetMonth(processPaymentRequest.CreditCardExpireMonth.ToString()); ;
            //result.CreditCardCvv2 = processPaymentRequest.CreditCardCvv2;
            //result.CreditCardNumber = processPaymentRequest.CreditCardNumber;
            //result.CustomerId = processPaymentRequest.CustomerId.ToString();
            //var currency = GetCurrency(_currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId));
            //result.Amount = GetCulturePrice(currency, processPaymentRequest.OrderTotal);


            //String hashstr = result.ClientId +
            //                 result.OrderId +
            //                 result.Amount +
            //                 result.OkUrl +
            //                result.FailUrl +
            //                result.ChargeType +
            //                result.Installment +//Installment- for now we do not use 
            //                result.RandomToken +
            //                result.StoreKey;
            //System.Security.Cryptography.SHA1 sha = new System.Security.Cryptography.SHA1CryptoServiceProvider();
            //byte[] hashbytes = System.Text.Encoding.GetEncoding("ISO-8859-9").GetBytes(hashstr);
            //byte[] inputbytes = sha.ComputeHash(hashbytes);
            //String hash = Convert.ToBase64String(inputbytes);
            //result.Hash = hash;

            //result.RequiresRedirection = true;

            ////var customer = _customerService.GetCustomerById(processPaymentRequest.CustomerId);



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
            var hostAddress = _AkbankPaymentSettings.UseTestServer ? _AkbankPaymentSettings.TestServiceUrl : _AkbankPaymentSettings.ServiceUrl;
            ccpayment.host = hostAddress;// "https://netpos.Akbank.com.tr/servlet/cc5ApiServer";//"https://testsanalpos.est.com.tr/servlet/cc5ApiServer";//
            ccpayment.name = _AkbankPaymentSettings.UseTestServer?_AkbankPaymentSettings.TestName :_AkbankPaymentSettings.Name;// "sanalpos"; //"KUVEYTAPI";//
            ccpayment.password = _AkbankPaymentSettings.UseTestServer?_AkbankPaymentSettings.TestPassword:_AkbankPaymentSettings.Password;// "V7Z4M3SgmiJ";//"KUVEYT123";//
            ccpayment.clientid = _AkbankPaymentSettings.UseTestServer ? _AkbankPaymentSettings.TestClientId : _AkbankPaymentSettings.ClientId;//"110000810";//"110000000";//
            ccpayment.chargetype = "Auth";
            ccpayment.orderresult =_AkbankPaymentSettings.TestOrder ? 1 : 0;
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

            string installment = processPaymentRequest.Installment <= 1 ? "1" : processPaymentRequest.Installment.ToString();
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
                    loger.Error("Payments.CC.Akbank: errorcode:" + ccpayment.code + "\n\r errortext:" + ccpayment.errmsg);
             
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
            controllerName = "PaymentAkbank";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.Akbank.Controllers" }, { "area", null } };
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
            controllerName = "PaymentAkbank";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.Akbank.Controllers" }, { "area", null } };
        }

        public Type GetControllerType()
        {
            return typeof(PaymentAkbankController);
        }

        public override void Install()
        {
            var settings = new AkbankPaymentSettings()
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
