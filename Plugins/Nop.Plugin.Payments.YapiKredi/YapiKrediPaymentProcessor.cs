using System;
using System.Web.Routing;
using Nop.Core.Domain.Payments;
using Nop.Core.Plugins;
using Nop.Plugin.Payments.YapiKredi.Controllers;
using Nop.Services.Configuration;
using Nop.Services.Payments;
using _PosnetDotNetModule;
using Nop.Core.Domain.Directory;
using Nop.Services.Directory;
using Nop.Core;
using Nop.Core.Domain;
using System.Globalization;
using Nop.Services.Localization;
using Nop.Core.Domain.Orders;
using Nop.Services.Customers;
using System.Collections;
using System.Web.Mvc;
using System.Collections.Generic;
using Nop.Services.Logging;
using Nop.Core.Infrastructure;
using _PosnetDotNetTDSOOSModule;
using Nop.Web.Framework;

namespace Nop.Plugin.Payments.YapiKredi
{
    /// <summary>
    /// YapiKredi payment processor
    /// </summary>
    public class YapiKrediPaymentProcessor : BasePlugin, IPaymentMethod, ICampaignedPaymentMethod
    {
        #region Fields

        private readonly YapiKrediPaymentSettings _YapiKrediPaymentSettings;
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

        public YapiKrediPaymentProcessor(YapiKrediPaymentSettings YapiKrediPaymentSettings,
         ISettingService settingService, ICurrencyService currencyService,
            CurrencySettings currencySettings, IWebHelper webHelper,
            StoreInformationSettings storeInformationSettings,
            ILocalizationService localizationService,
            ICustomerService customerService,
            IWorkContext workContext)
        {
            this._YapiKrediPaymentSettings = YapiKrediPaymentSettings;
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
     

        public static string GetCurrency(Currency currency)
        {
            switch (currency.CurrencyCode)
            {
                case "USD":
                    return "US";
                case "EUR":
                    return "EU";
                case "TL":
                    return "YT";
                default:
                    return "YT";
            }
        }

        public static string GetMonth(string value)
        {
            return value.PadLeft(2, '0');
        }

        //TODO: it is suggested to always show a generic message
        private string ConvertPaymentMessage(string code)
        {
            switch (code)
            {
                case "0376":
                   return _localizationService.GetResource("Payments.CC.YapiKredi.JVadaError");
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

            C_PosnetOOSTDS posnetOOSTDSObj = new C_PosnetOOSTDS();

            Random rndOid = new Random();
            int oidRnd = rndOid.Next(111111111, 999999999);
            result.OrderId = oidRnd.ToString("00000000000000000000");

            string custName = processPaymentRequest.CreditCardName;
            string xid = oidRnd.ToString("00000000000000000000");

            string ccno = processPaymentRequest.CreditCardNumber; //Request.Form.Get("ccno");

            string expYear = (processPaymentRequest.CreditCardExpireYear % 100).ToString();
            string expMonth = GetMonth(processPaymentRequest.CreditCardExpireMonth.ToString());

            string expdate = expYear.ToString().Replace("20", string.Empty) + int.Parse(expMonth).ToString("00");
            string cvv = processPaymentRequest.CreditCardCvv2; //Request.Form.Get("cvv");

            //string amount = processPaymentRequest.OrderTotal.ToString().Replace(",", "").Replace(".", "");
            String amount;
            amount = processPaymentRequest.OrderTotal.ToString("###,###.00");
            amount = amount.Replace(",", "");
            amount = amount.Replace(".", "");
            result.Amount = amount;

            string currencyCode = "YT";
            string instalment = int.Parse(taksit).ToString("00");
            string tranType = "Sale";
            string vftCode = _YapiKrediPaymentSettings.VftCode;



            posnetOOSTDSObj.SetMid(_YapiKrediPaymentSettings.MerchantId);
            //sbLog.AppendLine("posnetOOSTDSObj.SetMid(" + diger3dPaymentSettings.MERCHANT_ID.ToString() + ")");
            posnetOOSTDSObj.SetTid(_YapiKrediPaymentSettings.TerminalId);
            //sbLog.AppendLine("posnetOOSTDSObj.SetTid(" + diger3dPaymentSettings.TERMINAL_ID.ToString() + ")");
            posnetOOSTDSObj.SetPosnetID(_YapiKrediPaymentSettings.PosnetId);
            //sbLog.AppendLine("posnetOOSTDSObj.SetPosnetID(" + diger3dPaymentSettings.POSNET_ID.ToString() + ")");
            posnetOOSTDSObj.SetKey(_YapiKrediPaymentSettings.EncKey);
            //sbLog.AppendLine("posnetOOSTDSObj.SetKey(" + diger3dPaymentSettings.ENCKEY.ToString() + ")");



            if ((!posnetOOSTDSObj.CreateTranRequestDatas(custName, amount, currencyCode, instalment, xid, tranType, ccno, expdate, cvv)))
            {
                //TODO: HATA DURUMU ELE ALINMALI
                //Log(string.Format("CreateTranRequestDatas Hatasý - Posnet Data 'larý olusturulamadi: {0} - Error Code: {1}", posnetOOSTDSObj.GetResponseText(), posnetOOSTDSObj.GetResponseCode()));
                //sbLog.AppendLine("'posnetOOSTDSObj.CreateTranRequestDatas(custName, amount, currencyCode, instalment, xid, tranType, ccno, expdate, cvv)' metodu olumsuz sonuc donderdi.");
                //postProcessPaymentRequest.Order.OrderStatus = OrderStatus.Pending;

            }
            else
            {
                result.PaymentRequest = processPaymentRequest;
                result.OkUrl = _YapiKrediPaymentSettings.MerchantReturnURL;
                result.RedirectURL = _YapiKrediPaymentSettings.HostAddress3D;
                result.MerchantId = _YapiKrediPaymentSettings.MerchantId;
                result.PosnetID = _YapiKrediPaymentSettings.PosnetId;
                result.PosnetData = posnetOOSTDSObj.GetPosnetData();
                result.PosnetData2 = posnetOOSTDSObj.GetPosnetData2();
                result.Digest = posnetOOSTDSObj.GetSign();
                result.VftCode = _YapiKrediPaymentSettings.VftCode;
                result.Lang = "tr";
                result.RequiresRedirection = true;
                
                



                ////var remotePostHelper = new RemotePost();
                ////remotePostHelper.FormName = "YkbForm";
                ////remotePostHelper.Url = diger3dPaymentSettings.OOS_TDS_SERVICE_URL;
                ////remotePostHelper.Add("mid", diger3dPaymentSettings.MERCHANT_ID);
                ////remotePostHelper.Add("posnetID", diger3dPaymentSettings.POSNET_ID);
                ////remotePostHelper.Add("posnetData", posnetOOSTDSObj.GetPosnetData());
                ////remotePostHelper.Add("posnetData2", posnetOOSTDSObj.GetPosnetData2());
                ////remotePostHelper.Add("digest", posnetOOSTDSObj.GetSign());
                ////remotePostHelper.Add("vftCode", diger3dPaymentSettings.vtfCode);
                ////remotePostHelper.Add("merchantReturnURL", diger3dPaymentSettings.MERCHANT_RETURN_URL);
                ////remotePostHelper.Add("lang", "tr");
                ////remotePostHelper.Add("url", diger3dPaymentSettings.OOS_TDS_SERVICE_URL);
                //var remotePostHelper = new RemotePost();
                ////sbLog.AppendLine("var remotePostHelper = new RemotePost();");

                //remotePostHelper.FormName = "YkbForm";
                ////sbLog.AppendLine("remotePostHelper.FormName = " + remotePostHelper.FormName.ToString());

                //remotePostHelper.Url = diger3dPaymentSettings.OOS_TDS_SERVICE_URL;
                ////sbLog.AppendLine("remotePostHelper.Url = " + diger3dPaymentSettings.OOS_TDS_SERVICE_URL.ToString());

                //remotePostHelper.Add("mid", diger3dPaymentSettings.MERCHANT_ID);
                ////sbLog.AppendLine("remotePostHelper.Add('mid', " + diger3dPaymentSettings.MERCHANT_ID.ToString() + ");");

                //remotePostHelper.Add("posnetID", diger3dPaymentSettings.POSNET_ID);
                ////sbLog.AppendLine("remotePostHelper.Add('posnetID', " + diger3dPaymentSettings.POSNET_ID.ToString() + ");");

                //remotePostHelper.Add("posnetData", posnetOOSTDSObj.GetPosnetData());
                ////sbLog.AppendLine("remotePostHelper.Add('posnetData', " + posnetOOSTDSObj.GetPosnetData().ToString() + ");");

                //remotePostHelper.Add("posnetData2", posnetOOSTDSObj.GetPosnetData2());
                ////sbLog.AppendLine("remotePostHelper.Add('posnetData2', " + posnetOOSTDSObj.GetPosnetData2().ToString() + ");");

                //remotePostHelper.Add("digest", posnetOOSTDSObj.GetSign());
                ////sbLog.AppendLine("remotePostHelper.Add('digest', " + posnetOOSTDSObj.GetSign().ToString() + ");");

                //remotePostHelper.Add("vftCode", diger3dPaymentSettings.vtfCode);
                ////sbLog.AppendLine("remotePostHelper.Add('vftCode', " + diger3dPaymentSettings.vtfCode.ToString() + ");");

                //remotePostHelper.Add("merchantReturnURL", diger3dPaymentSettings.MERCHANT_RETURN_URL);
                ////sbLog.AppendLine("remotePostHelper.Add('merchantReturnURL', " + diger3dPaymentSettings.MERCHANT_RETURN_URL.ToString() + ");");

                //remotePostHelper.Add("lang", "tr");
                ////sbLog.AppendLine("remotePostHelper.Add('lang', 'tr');");

                //remotePostHelper.Add("url", diger3dPaymentSettings.OOS_TDS_SERVICE_URL);
                ////sbLog.AppendLine("remotePostHelper.Add('url', " + diger3dPaymentSettings.OOS_TDS_SERVICE_URL.ToString() + ");");

                ////remotePostHelper.Add("openANewWindow", "false");
                ////sbLog.AppendLine("remotePostHelper.Add('openANewWindow', 'false');");

                ////sbLog.AppendLine("remotePostHelper.Post();");
                //postProcessPaymentRequest.Order.OrderNotes.Add(new OrderNote()
                //{
                //    Note = sbLog.ToString(),
                //    DisplayToCustomer = false,
                //    CreatedOnUtc = DateTime.UtcNow
                //});
                //_orderService.UpdateOrder(postProcessPaymentRequest.Order);

                //remotePostHelper.Post();

                ////NameValueCollection collection = new NameValueCollection();
                ////collection.Add("mid", ykb3dPaymentSettings.MERCHANT_ID);
                ////collection.Add("posnetID", ykb3dPaymentSettings.POSNET_ID);
                ////collection.Add("posnetData", posnetOOSTDSObj.GetPosnetData());
                ////collection.Add("posnetData2", posnetOOSTDSObj.GetPosnetData2());
                ////collection.Add("digest", posnetOOSTDSObj.GetSign());
                ////collection.Add("vftCode", ykb3dPaymentSettings.vtfCode);
                ////collection.Add("merchantReturnURL", ykb3dPaymentSettings.MERCHANT_RETURN_URL);

                //////FormCollection myForm = new FormCollection(collection);

                ////var builder = new StringBuilder();

                //////Errore
                ////builder.Append(ykb3dPaymentSettings.OOS_TDS_SERVICE_URL);
                //////builder.Append(_webHelper.GetStoreLocation(false) + "Plugins/PaymentGestPay/Error");
                ////builder.AppendFormat("?posnetData={0}&posnetData2={1}", posnetOOSTDSObj.GetPosnetData(), posnetOOSTDSObj.GetPosnetData2());
                ////builder.AppendFormat("&digest={0}&mid={1}", posnetOOSTDSObj.GetSign(), ykb3dPaymentSettings.MERCHANT_ID);
                ////builder.AppendFormat("&posnetID={0}&vftCode={1}", ykb3dPaymentSettings.POSNET_ID, ykb3dPaymentSettings.vtfCode);
                ////builder.AppendFormat("&merchantReturnURL={0}&lang={1}", ykb3dPaymentSettings.MERCHANT_RETURN_URL, "tr");
                ////builder.AppendFormat("&url={0}&openANewWindow={1}", "", ykb3dPaymentSettings.OPEN_NEW_WINDOW);            

                ////_httpContext.Response.Redirect(builder.ToString());
            }


            return result;




            
            
            
            
            
            
            
            
            
            
            //var currency = GetCurrency(_currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId));
            ////String amount = Decimal.Parse(GetCulturePrice(currency, processPaymentRequest.OrderTotal)).ToString("######.00"); //postProcessPaymentRequest.Order.OrderTotal.ToString("######.00");
            //String oid = processPaymentRequest.OrderGuid.ToString();

            //String rnd = DateTime.Now.ToString();
            

            //string pan = processPaymentRequest.CreditCardNumber; //_encryptionService.DecryptText(postProcessPaymentRequest.Order.CardNumber);
            //string cv2 = processPaymentRequest.CreditCardCvv2; //_encryptionService.DecryptText(postProcessPaymentRequest.Order.CardCvv2);
            //string Ecom_Payment_Card_ExpDate_Year = (processPaymentRequest.CreditCardExpireYear % 100).ToString(); //_encryptionService.DecryptText(postProcessPaymentRequest.Order.CardExpirationYear);
            //string Ecom_Payment_Card_ExpDate_Month = GetMonth(processPaymentRequest.CreditCardExpireMonth.ToString()); //_encryptionService.DecryptText(postProcessPaymentRequest.Order.CardExpirationMonth);            
            //string cardType = processPaymentRequest.CreditCardType.ToLower().Trim(); //_encryptionService.DecryptText(postProcessPaymentRequest.Order.CardType);


            //if (processPaymentRequest.CreditCardType.ToLower().Trim() == "visa")
            //    result.CardType = "1";
            //if (processPaymentRequest.CreditCardType.ToLower().Trim() == "mastercard")
            //    result.CardType = "2";
            ////CC shall be either visa or master card!
            //if (result.CardType != "1" && result.CardType != "2")
            //    return result;
            //result.PaymentRequest = processPaymentRequest;
            //result.FailUrl = _GarantiPaymentSettings.ErrorURL;
            //result.OkUrl = _GarantiPaymentSettings.SuccessURL; ;
            //result.RandomToken = DateTime.Now.ToString();
            //result.RedirectURL = _GarantiPaymentSettings.HostAddress3D;
            //result.StoreType = "";
            //result.StoreKey = _GarantiPaymentSettings.StoreKey;
            //result.ChargeType = "Auth";
            //result.Lang = "tr";
            //result.Currency = "949";
            //result.ClientId = _GarantiPaymentSettings.MerchantId;
            //result.ApiVersion = "v0.01";
            //result.TerminalProvUserId = _GarantiPaymentSettings.ProvisionUserId;
            //result.TerminalUserId = _GarantiPaymentSettings.UserId;
            //result.TerminalId = _GarantiPaymentSettings.TerminalId;
            //result.MerchantId = _GarantiPaymentSettings.MerchantId;
            //result.CustomerEmailAddress = _workContext.CurrentCustomer.Email;
            //result.CustomerIpAddress = _workContext.CurrentCustomer.LastIpAddress;



            //Random rndOid = new Random();
            //int oidRnd = rndOid.Next(111111111, 999999999);
            //result.OrderId = oidRnd.ToString("00000000000000000000");
            //result.Installment = taksit;
            //result.CreditCardNumber = processPaymentRequest.CreditCardNumber;
            //result.CreditCardExpireYear = (processPaymentRequest.CreditCardExpireYear % 100).ToString();
            //result.CreditCardExpireMonth = GetMonth(processPaymentRequest.CreditCardExpireMonth.ToString()); ;
            //result.CreditCardCvv2 = processPaymentRequest.CreditCardCvv2;
            //result.CreditCardNumber = processPaymentRequest.CreditCardNumber;
            //result.CustomerId = processPaymentRequest.CustomerId.ToString();
            //result.Amount = amount; // GetCulturePrice(currency, processPaymentRequest.OrderTotal);



            //amount = Decimal.Parse(GetCulturePrice(currency, processPaymentRequest.OrderTotal)).ToString("###,###.00");
            //amount = amount.Replace(",", "");
            //amount = amount.Replace(".", "");
            //string SecurityData = GetSHA1(_GarantiPaymentSettings.ProvisionPassword + _GarantiPaymentSettings.TerminalId).ToUpper();
            ////sbLog.AppendLine("GetSHA1(" + _GarantiPaymentSettings.ProvisionPassword.ToString() + " + " + _GarantiPaymentSettings.TerminalId.ToString() + ").ToUpper();");
            ////sbLog.AppendLine("SecurityData: " + SecurityData);
            //string HashData = GetSHA1(_GarantiPaymentSettings.TerminalId + oid + amount + _GarantiPaymentSettings.SuccessURL + _GarantiPaymentSettings.ErrorURL + "sales" + taksit + _GarantiPaymentSettings.StoreKey + SecurityData).ToUpper();
            ////sbLog.AppendLine("GetSHA1(" + garanti3dPaymentSettings.TerminalID.ToString() + " + " + oid.ToString() + " + " + strAmount.ToString() + " + " + garanti3dPaymentSettings.SuccessURL.ToString() + " + " + garanti3dPaymentSettings.ErrorURL.ToString() + " + sales + " + taksit.ToString() + " + " + garanti3dPaymentSettings.StoreKey.ToString() + " + " + SecurityData.ToString() + ").ToUpper();");
            ////sbLog.AppendLine("HashData: " + HashData);

            //result.Hash = HashData;

            //result.RequiresRedirection = true;


            //return result;
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
            C_Posnet posnet = new C_Posnet();

            var hostAddress = _YapiKrediPaymentSettings.UseTestServer ? _YapiKrediPaymentSettings.TestServiceUrl : _YapiKrediPaymentSettings.ServiceUrl;

            posnet.SetURL(hostAddress);
            //"https://netpos.YapiKredi.com.tr/servlet/cc5ApiServer";
            //"https://testsanalpos.est.com.tr/servlet/cc5ApiServer";
            // test=> http://setmpos.ykb.com/PosnetWebService/XML
            // prod=> https://www.posnet.ykb.com/PosnetWebService/XML
            posnet.SetMid(_YapiKrediPaymentSettings.UseTestServer ? _YapiKrediPaymentSettings.TestMerchantId : _YapiKrediPaymentSettings.MerchantId);
            posnet.SetTid(_YapiKrediPaymentSettings.UseTestServer ? _YapiKrediPaymentSettings.TestTerminalId : _YapiKrediPaymentSettings.TerminalId);

            string ccNo = processPaymentRequest.CreditCardNumber;
            string expMonth = GetMonth(processPaymentRequest.CreditCardExpireMonth.ToString());
            string expYear = (processPaymentRequest.CreditCardExpireYear%100).ToString();
            string expDate = expYear + expMonth;
            string cv2 = processPaymentRequest.CreditCardCvv2;
            string orderGUID = processPaymentRequest.OrderGuid.ToString().Replace("-", "").Substring(0, 24);
            var currency = GetCurrency(_currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId));
            string amount = Math.Round(processPaymentRequest.OrderTotal, 2).ToString().Replace(".", "").Replace(",", "");
            string installment = processPaymentRequest.Installment ==1 ?"00" : processPaymentRequest.Installment.ToString().PadLeft(2, '0');

            int KOIICode = 0;
            if (int.TryParse(processPaymentRequest.CCOption, out KOIICode))
            {
                if (KOIICode > 0 && KOIICode < 7)
                    posnet.SetKOICode(KOIICode.ToString());

            }

            bool posnetResult = posnet.DoSaleTran(ccNo, expDate, cv2, orderGUID, amount, currency, installment);




            var result = new ProcessPaymentResult();
            if (!posnetResult)//can not communicate
            {
                result.Errors.Add(_localizationService.GetResource("Checkout.Payment.ConnectionNotEstablished"));

            }
            else
            {
                string code = posnet.GetApprovedCode();
                if (code == "1" || code == "2")//success
                {
                    result.AuthorizationTransactionId = posnet.GetHostlogkey();
                    result.PaymentCampaignNotes = KOIICode.ToString();
                }
                else if(code=="0")
                {
                    result.Errors.Add(this.ConvertPaymentMessage(posnet.GetResponseCode().Trim()));
                    ILogger loger = EngineContext.Current.Resolve<ILogger>();
                    loger.Error("Payments.CC.YapiKredi: errorcode:" + posnet.GetResponseCode() + "\n\r errortext:" + posnet.GetResponseText());
              
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
            controllerName = "PaymentYapiKredi";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.YapiKredi.Controllers" }, { "area", null } };
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
            controllerName = "PaymentYapiKredi";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.YapiKredi.Controllers" }, { "area", null } };
        }

        public Type GetControllerType()
        {
            return typeof(PaymentYapiKrediController);
        }

        public override void Install()
        {
            var settings = new YapiKrediPaymentSettings()
            {
                TransactMode = TransactMode.Pending
            };
            _settingService.SaveSetting(settings);

            base.Install();
        }


        public IList<KeyValuePair<string,string>> GetPaymentOptions(ProcessPaymentRequest processPaymentRequest)
        {

            var result = new List<KeyValuePair<string,string>>();

            //remove
            //result.Add(new KeyValuePair<string,string>("1", "1. seçenek"));
            //result.Add(new KeyValuePair<string, string>("2", "2. seçenek"));
            //result.Add(new KeyValuePair<string, string>("3", "3. seçenek"));
            //result.Add(new KeyValuePair<string, string>("4", "4. seçenek"));
            //return result;

            C_Posnet posnet = new C_Posnet();
            var hostAddress = _YapiKrediPaymentSettings.UseTestServer ? _YapiKrediPaymentSettings.TestServiceUrl : _YapiKrediPaymentSettings.ServiceUrl;

            posnet.SetURL(hostAddress);
            posnet.SetMid(_YapiKrediPaymentSettings.UseTestServer ? _YapiKrediPaymentSettings.TestMerchantId : _YapiKrediPaymentSettings.MerchantId);
            posnet.SetTid(_YapiKrediPaymentSettings.UseTestServer ? _YapiKrediPaymentSettings.TestTerminalId : _YapiKrediPaymentSettings.TerminalId);

            posnet.DoKOIInquiry(processPaymentRequest.CreditCardNumber);
            if (posnet.GetApprovedCode() == "1")
            {
                string code="";
                for (int i = 1; i <= posnet.GetCampMessageCount(); i++)
                {
                    code = posnet.GetCampCode(i);
                    if (string.IsNullOrWhiteSpace(code)) continue;
                    result.Add(new KeyValuePair<string,string>(code, posnet.GetCampMessage(i)));
                }
            }

            if (result.Count > 0)
            {
                result.Insert(0, new KeyValuePair<string, string>("", _localizationService.GetResource("Payments.CC.YapiKredi.SelectOption")));
            }
            else
            {
                ILogger loger = EngineContext.Current.Resolve<ILogger>();
                loger.Error("Payments.CC.YapiKredi: errorcode:" + posnet.GetResponseCode() + "\n\r errortext:" + posnet.GetResponseText());

            }

            return result;
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
