using System;
using System.Web.Routing;
using Nop.Core.Domain.Payments;
using Nop.Core.Plugins;
using Nop.Plugin.Payments.Garanti.Controllers;
using Nop.Services.Configuration;
using Nop.Services.Payments;
using Nop.Core.Domain.Directory;
using Nop.Services.Directory;
using Nop.Core;
using Nop.Core.Domain;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Net;
using System.IO;
using System.Xml;
using Nop.Core.Domain.Orders;
using Nop.Services.Customers;
using System.Collections.Specialized;
using System.Collections.Generic;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Core.Infrastructure;

namespace Nop.Plugin.Payments.Garanti
{
    /// <summary>
    /// Garanti payment processor
    /// </summary>
    public class GarantiPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly GarantiPaymentSettings _GarantiPaymentSettings;
        private readonly ISettingService _settingService;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly IWebHelper _webHelper;
        private readonly StoreInformationSettings _storeInformationSettings;
        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;


        #endregion

        #region Ctor

        public GarantiPaymentProcessor(GarantiPaymentSettings garantiPaymentSettings,
         ISettingService settingService, ICurrencyService currencyService,
            CurrencySettings currencySettings, IWebHelper webHelper,
            StoreInformationSettings storeInformationSettings,
            ICustomerService customerService,
            ILocalizationService localizationService,
            IWorkContext workContext)
        {
            this._GarantiPaymentSettings = garantiPaymentSettings;
            this._settingService = settingService;
            this._currencyService = currencyService;
            this._currencySettings = currencySettings;
            this._webHelper = webHelper;
            this._storeInformationSettings = storeInformationSettings;
            this._customerService = customerService;
            this._localizationService = localizationService;
            this._workContext = workContext;
        }

        #endregion

        #region Methods

        internal string ConvertPaymentMessage(string code)
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
        internal static string GetCulturePrice(string currency, decimal price)
        {
            //switch (currency)
            //{
            //    case "840":
            //        return Math.Round(price, 2).ToString("N", new CultureInfo("en-us"));
            //    case "978":
            //        return Math.Round(price, 2).ToString("N", new CultureInfo("en-us"));
            //    case "949":
            //        return Math.Round(price, 2).ToString("N", new CultureInfo("tr-tr")).Replace(".","");
            //    default:
            //        return Math.Round(price, 2).ToString("N", new CultureInfo("tr-tr")).Replace(".","");
            //}

            return Math.Round(price, 2).ToString("N", new CultureInfo("en-us")).Replace(".", "").Replace(",", "");
            
        }

        internal static string GetCurrency(Currency currency)
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

        internal static string GetMonth(string value)
        {
            if (value.Length == 2) return value;
            return value.PadLeft(2, '0');
        }

        internal string GetSHA1(string SHA1Data)
        {
            SHA1 sha = new SHA1CryptoServiceProvider();
            string HashedPassword = SHA1Data;
            byte[] hashbytes = Encoding.GetEncoding("ISO-8859-9").GetBytes(HashedPassword);
            byte[] inputbytes = sha.ComputeHash(hashbytes);
            return GetHexaDecimal(inputbytes);
        }

        internal string GetHexaDecimal(byte[] bytes)
        {
            StringBuilder s = new StringBuilder();
            int length = bytes.Length;
            for (int n = 0; n <= length - 1; n++)
            {
                s.Append(String.Format("{0,2:x}", bytes[n]).Replace(" ", "0"));
            }
            return s.ToString();
        }

        internal static String ConstructQueryString(NameValueCollection parameters)
        {
            List<string> items = new List<string>();

            foreach (String name in parameters)
                items.Add(String.Concat(name, "=", System.Web.HttpUtility.UrlEncode(parameters[name])));

            return String.Join("&", items.ToArray());
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee()
        {
            return 0;
        }
        
        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            return ProcessPaymentAuth(processPaymentRequest);
        }


        private ProcessPaymentResult ProcessPaymentAuth(ProcessPaymentRequest processPaymentRequest)
        {
            var customer = _customerService.GetCustomerById(processPaymentRequest.CustomerId);
            var result = new ProcessPaymentResult();

            string strMode = _GarantiPaymentSettings.TestOrder ? _GarantiPaymentSettings.TestMode : _GarantiPaymentSettings.Mode;
            string strVersion = _GarantiPaymentSettings.TestOrder ? _GarantiPaymentSettings.TestVersion : _GarantiPaymentSettings.Version;
            string strTerminalID = _GarantiPaymentSettings.TestOrder ? _GarantiPaymentSettings.TestTerminalId : _GarantiPaymentSettings.TerminalId;
            string _strTerminalID = strTerminalID.PadLeft(9, '0');
            string strProvUserID = _GarantiPaymentSettings.TestOrder ? _GarantiPaymentSettings.TestProvisionUserId : _GarantiPaymentSettings.ProvisionUserId;
            string strProvisionPassword = _GarantiPaymentSettings.TestOrder ? _GarantiPaymentSettings.TestProvisionPassword : _GarantiPaymentSettings.ProvisionPassword;
            string strUserID = _GarantiPaymentSettings.TestOrder ? _GarantiPaymentSettings.TestUserId : _GarantiPaymentSettings.UserId;
            string strMerchantID = _GarantiPaymentSettings.TestOrder ? _GarantiPaymentSettings.TestMerchantId : _GarantiPaymentSettings.MerchantId;
            string strIPAddress = HttpContext.Current.Request.IsLocal ? "127.0.0.1" : HttpContext.Current.Request.UserHostAddress;
            string strEmailAddress = customer.BillingAddress.Email;
            string strOrderID = processPaymentRequest.OrderGuid.ToString().Replace('-','_');
            string strNumber = processPaymentRequest.CreditCardNumber;
            string strExpireDate = GetMonth(processPaymentRequest.CreditCardExpireMonth.ToString()) + processPaymentRequest.CreditCardExpireYear % 100;
            string strCVV2 = processPaymentRequest.CreditCardCvv2;
            var currency = GetCurrency(_currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId));
            string strAmount = GetCulturePrice(currency, processPaymentRequest.OrderTotal);
            string strType = _GarantiPaymentSettings.TestOrder ? _GarantiPaymentSettings.TestType : _GarantiPaymentSettings.Type;//"sales";
            string strCurrencyCode = GetCurrency(_currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId));
            string strCardholderPresentCode = "0";
            string strMotoInd = "N";
            string strInstallmentCount = processPaymentRequest.Installment > 1 ? processPaymentRequest.Installment.ToString() : "1";
            string strHostAddress = _GarantiPaymentSettings.TestOrder ? _GarantiPaymentSettings.TestHostAddress : _GarantiPaymentSettings.HostAddress;//"https://sanalposprov.garanti.com.tr/VPServlet";

            string SecurityData = GetSHA1(strProvisionPassword + _strTerminalID).ToUpper();
            string HashData = GetSHA1(strOrderID + strTerminalID + strNumber + strAmount + SecurityData).ToUpper();

            string strXML = null;
            strXML = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + "<GVPSRequest>" + "<Mode>" + strMode + "</Mode>" + "<Version>" + strVersion + "</Version>" + "<Terminal><ProvUserID>" + strProvUserID + "</ProvUserID><HashData>" + HashData + "</HashData><UserID>" + strUserID + "</UserID><ID>" + strTerminalID + "</ID><MerchantID>" + strMerchantID + "</MerchantID></Terminal>" + "<Customer><IPAddress>" + strIPAddress + "</IPAddress><EmailAddress>" + strEmailAddress + "</EmailAddress></Customer>" + "<Card><Number>" + strNumber + "</Number><ExpireDate>" + strExpireDate + "</ExpireDate><CVV2>" + strCVV2 + "</CVV2></Card>" + "<Order><OrderID>" + strOrderID + "</OrderID><GroupID></GroupID><AddressList><Address><Type>S</Type><Name></Name><LastName></LastName><Company></Company><Text></Text><District></District><City></City><PostalCode></PostalCode><Country></Country><PhoneNumber></PhoneNumber></Address></AddressList></Order>" + "<Transaction>" + "<Type>" + strType + "</Type><InstallmentCnt>" + strInstallmentCount + "</InstallmentCnt><Amount>" + strAmount + "</Amount><CurrencyCode>" + strCurrencyCode + "</CurrencyCode><CardholderPresentCode>" + strCardholderPresentCode + "</CardholderPresentCode><MotoInd>" + strMotoInd + "</MotoInd>" + "</Transaction>" + "</GVPSRequest>";

            try
            {
                string data = "data=" + strXML;

                WebRequest _WebRequest = WebRequest.Create(strHostAddress);
                _WebRequest.Method = "POST";

                byte[] byteArray = Encoding.UTF8.GetBytes(data);
                _WebRequest.ContentType = "application/x-www-form-urlencoded";
                _WebRequest.ContentLength = byteArray.Length;

                Stream dataStream = _WebRequest.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();

                WebResponse _WebResponse = _WebRequest.GetResponse();
                //Console.WriteLine(((HttpWebResponse)_WebResponse).StatusDescription);
                dataStream = _WebResponse.GetResponseStream();

                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();

                //Console.WriteLine(responseFromServer);

                //Müþteriye gösterilebilir ama Fraud riski açýsýndan bu deðerleri göstermeyelim.
                //responseFromServer

                //GVPSResponse XML'in deðerlerini okuyoruz. Ýstediðiniz geri dönüþ deðerlerini gösterebilirsiniz.
                string XML = responseFromServer;
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(XML);

                //Code
                XmlElement xElementCode = xDoc.SelectSingleNode("//GVPSResponse/Transaction/Response/Code") as XmlElement;

                //ReasonCode
                XmlElement xElement1 = xDoc.SelectSingleNode("//GVPSResponse/Transaction/Response/ReasonCode") as XmlElement;
                //lblResult2.Text = xElement1.InnerText;

                //Message
                //XmlElement xElement2 = xDoc.SelectSingleNode("//GVPSResponse/Transaction/Response/Message") as XmlElement;
                //lblResult2.Text = xElement2.InnerText;

                //ErrorMsg
                XmlElement xElement3 = xDoc.SelectSingleNode("//GVPSResponse/Transaction/Response/ErrorMsg") as XmlElement;
                //lblResult2.Text = xElement3.InnerText;

                //00 ReasonCode döndüðünde iþlem baþarýlýdýr. Müþteriye baþarýlý veya baþarýsýz þeklinde göstermeniz tavsiye edilir. (Fraud riski)
                if (xElement1.InnerText == "00")
                {
                    var xElementRefNum = xDoc.SelectSingleNode("//GVPSResponse/Transaction/Response/ErrorMsg") as XmlElement;
                    result.AuthorizationTransactionId = xElementRefNum.InnerText;
                }
                else
                {
                    result.AddError(ConvertPaymentMessage( xElement3.InnerText));
                    ILogger loger = EngineContext.Current.Resolve<ILogger>();
                    loger.Error("Payments.CC.Garanti: errormsg:" + xElement3.InnerText);

                    using (var stringWriter = new StringWriter())
                    using (var xmlTextWriter = XmlWriter.Create(stringWriter))
                    {
                        xDoc.WriteTo(xmlTextWriter);
                        xmlTextWriter.Flush();
                        loger.Error("Payments.CC.Garanti: " + stringWriter.GetStringBuilder().ToString()); 
                    }

                   
             
                }

            }
            catch (Exception ex)
            {
                result.AddError(ex.Message);
            }


            return result;
        }

        private ProcessPaymentResult ProcessPayment3D(ProcessPaymentRequest processPaymentRequest)
        {
            var customer = _customerService.GetCustomerById(processPaymentRequest.CustomerId);
            var result = new ProcessPaymentResult();

            string strMode = _GarantiPaymentSettings.TestOrder ? _GarantiPaymentSettings.TestMode : _GarantiPaymentSettings.Mode;
            string strVersion = _GarantiPaymentSettings.TestOrder ? _GarantiPaymentSettings.TestVersion : _GarantiPaymentSettings.Version;
            string strTerminalID = _GarantiPaymentSettings.TestOrder ? _GarantiPaymentSettings.TestTerminalId : _GarantiPaymentSettings.TerminalId;
            string _strTerminalID = strTerminalID.PadLeft(9, '0');
            string strProvUserID = _GarantiPaymentSettings.TestOrder ? _GarantiPaymentSettings.TestProvisionUserId : _GarantiPaymentSettings.ProvisionUserId;
            string strProvisionPassword = _GarantiPaymentSettings.TestOrder ? _GarantiPaymentSettings.TestProvisionPassword : _GarantiPaymentSettings.ProvisionPassword;
            string strUserID = _GarantiPaymentSettings.TestOrder ? _GarantiPaymentSettings.TestUserId : _GarantiPaymentSettings.UserId;
            string strMerchantID = _GarantiPaymentSettings.TestOrder ? _GarantiPaymentSettings.TestMerchantId : _GarantiPaymentSettings.MerchantId;
            string strIPAddress = HttpContext.Current.Request.UserHostAddress;
            string strEmailAddress = customer.BillingAddress.Email;
            string strOrderID = processPaymentRequest.OrderGuid.ToString();
            string strNumber = processPaymentRequest.CreditCardNumber;
            string strExpireDate = processPaymentRequest.CreditCardExpireYear + GetMonth(processPaymentRequest.CreditCardExpireMonth.ToString());
            string strCVV2 = processPaymentRequest.CreditCardCvv2;
            var currency = GetCurrency(_currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId));
            string strAmount = GetCulturePrice(currency, processPaymentRequest.OrderTotal);
            string strType = _GarantiPaymentSettings.TestOrder ? _GarantiPaymentSettings.TestType : _GarantiPaymentSettings.Type;//"sales";
            string strCurrencyCode = GetCurrency(_currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId));
            string strCardholderPresentCode = "0";
            string strMotoInd = "N";
            string strInstallmentCount = "";
            string strHostAddress = _GarantiPaymentSettings.TestOrder ? _GarantiPaymentSettings.TestHostAddress : _GarantiPaymentSettings.HostAddress;//"https://sanalposprov.garanti.com.tr/VPServlet";

            string SecurityData = GetSHA1(strProvisionPassword + _strTerminalID).ToUpper();
            string HashData = GetSHA1(strOrderID + strTerminalID + strNumber + strAmount + SecurityData).ToUpper();

            string strXML = null;
            strXML = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + "<GVPSRequest>" + "<Mode>" + strMode + "</Mode>" + "<Version>" + strVersion + "</Version>" + "<Terminal><ProvUserID>" + strProvUserID + "</ProvUserID><HashData>" + HashData + "</HashData><UserID>" + strUserID + "</UserID><ID>" + strTerminalID + "</ID><MerchantID>" + strMerchantID + "</MerchantID></Terminal>" + "<Customer><IPAddress>" + strIPAddress + "</IPAddress><EmailAddress>" + strEmailAddress + "</EmailAddress></Customer>" + "<Card><Number>" + strNumber + "</Number><ExpireDate>" + strExpireDate + "</ExpireDate><CVV2>" + strCVV2 + "</CVV2></Card>" + "<Order><OrderID>" + strOrderID + "</OrderID><GroupID></GroupID><AddressList><Address><Type>S</Type><Name></Name><LastName></LastName><Company></Company><Text></Text><District></District><City></City><PostalCode></PostalCode><Country></Country><PhoneNumber></PhoneNumber></Address></AddressList></Order>" + "<Transaction>" + "<Type>" + strType + "</Type><InstallmentCnt>" + strInstallmentCount + "</InstallmentCnt><Amount>" + strAmount + "</Amount><CurrencyCode>" + strCurrencyCode + "</CurrencyCode><CardholderPresentCode>" + strCardholderPresentCode + "</CardholderPresentCode><MotoInd>" + strMotoInd + "</MotoInd>" + "</Transaction>" + "</GVPSRequest>";

            try
            {
                string data = "data=" + strXML;

                WebRequest _WebRequest = WebRequest.Create(strHostAddress);
                _WebRequest.Method = "POST";

                byte[] byteArray = Encoding.UTF8.GetBytes(data);
                _WebRequest.ContentType = "application/x-www-form-urlencoded";
                _WebRequest.ContentLength = byteArray.Length;

                Stream dataStream = _WebRequest.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();

                WebResponse _WebResponse = _WebRequest.GetResponse();
                //Console.WriteLine(((HttpWebResponse)_WebResponse).StatusDescription);
                dataStream = _WebResponse.GetResponseStream();

                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();

                //Console.WriteLine(responseFromServer);

                //Müþteriye gösterilebilir ama Fraud riski açýsýndan bu deðerleri göstermeyelim.
                //responseFromServer

                //GVPSResponse XML'in deðerlerini okuyoruz. Ýstediðiniz geri dönüþ deðerlerini gösterebilirsiniz.
                string XML = responseFromServer;
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(XML);

                //Code
                XmlElement xElementCode = xDoc.SelectSingleNode("//GVPSResponse/Transaction/Response/Code") as XmlElement;

                //ReasonCode
                XmlElement xElement1 = xDoc.SelectSingleNode("//GVPSResponse/Transaction/Response/ReasonCode") as XmlElement;
                //lblResult2.Text = xElement1.InnerText;

                //Message
                //XmlElement xElement2 = xDoc.SelectSingleNode("//GVPSResponse/Transaction/Response/Message") as XmlElement;
                //lblResult2.Text = xElement2.InnerText;

                //ErrorMsg
                XmlElement xElement3 = xDoc.SelectSingleNode("//GVPSResponse/Transaction/Response/ErrorMsg") as XmlElement;
                //lblResult2.Text = xElement3.InnerText;

                //00 ReasonCode döndüðünde iþlem baþarýlýdýr. Müþteriye baþarýlý veya baþarýsýz þeklinde göstermeniz tavsiye edilir. (Fraud riski)
                if (xElement1.InnerText == "00")
                {
                    var xElementRefNum = xDoc.SelectSingleNode("//GVPSResponse/Transaction/Response/ErrorMsg") as XmlElement;
                    result.AuthorizationTransactionId = xElementRefNum.InnerText;
                }
                else
                {
                    result.AddError(xElement3.InnerText);
                }

            }
            catch (Exception ex)
            {
                result.AddError(ex.Message);
            }


            return result;
        }



        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            //nothing
        }

        public PreProcessPaymentResult PreProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new PreProcessPaymentResult();

            String taksit = _workContext.CurrentCustomer.GetAttribute<string>("taksit");
            String clientId = _GarantiPaymentSettings.MerchantId;
            var currency = GetCurrency(_currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId));
            //String amount = Decimal.Parse(GetCulturePrice(currency, processPaymentRequest.OrderTotal)).ToString("######.00"); //postProcessPaymentRequest.Order.OrderTotal.ToString("######.00");
            //amount = amount.Replace(",", ".");
            //String oid = processPaymentRequest.OrderGuid.ToString();
            
            String rnd = DateTime.Now.ToString();
            //String islemtipi = "Auth";
            ////String storetype = "3d_pay";
            //String hashstr = clientId + oid + amount + okUrl + failUrl + islemtipi + taksit + rnd + storekey;
            //System.Security.Cryptography.SHA1 sha = new System.Security.Cryptography.SHA1CryptoServiceProvider();
            //byte[] hashbytes = System.Text.Encoding.GetEncoding("ISO-8859-9").GetBytes(hashstr);
            //byte[] inputbytes = sha.ComputeHash(hashbytes);
            //String hash = Convert.ToBase64String(inputbytes);

            string pan = processPaymentRequest.CreditCardNumber; //_encryptionService.DecryptText(postProcessPaymentRequest.Order.CardNumber);
            string cv2 = processPaymentRequest.CreditCardCvv2; //_encryptionService.DecryptText(postProcessPaymentRequest.Order.CardCvv2);
            string Ecom_Payment_Card_ExpDate_Year = (processPaymentRequest.CreditCardExpireYear % 100).ToString(); //_encryptionService.DecryptText(postProcessPaymentRequest.Order.CardExpirationYear);
            string Ecom_Payment_Card_ExpDate_Month = GetMonth(processPaymentRequest.CreditCardExpireMonth.ToString()); //_encryptionService.DecryptText(postProcessPaymentRequest.Order.CardExpirationMonth);            
            string cardType = processPaymentRequest.CreditCardType.ToLower().Trim(); //_encryptionService.DecryptText(postProcessPaymentRequest.Order.CardType);

            string _strTerminalID = "0" + _GarantiPaymentSettings.TerminalId;

            if (processPaymentRequest.CreditCardType.ToLower().Trim() == "visa")
                result.CardType = "1";
            if (processPaymentRequest.CreditCardType.ToLower().Trim() == "mastercard")
                result.CardType = "2";
            //CC shall be either visa or master card!
            if (result.CardType != "1" && result.CardType != "2")
                return result;
            result.PaymentRequest = processPaymentRequest;
            result.FailUrl = _GarantiPaymentSettings.ErrorURL;
            result.OkUrl = _GarantiPaymentSettings.SuccessURL;
            result.RandomToken = rnd;
            result.RedirectURL = _GarantiPaymentSettings.HostAddress3D;
            result.StoreType = "";
            result.StoreKey = _GarantiPaymentSettings.StoreKey;
            result.ChargeType = "Auth";
            result.Lang = "tr";
            result.Currency = "949";
            result.ClientId = _GarantiPaymentSettings.MerchantId;
            result.ApiVersion = "v0.01";
            result.TerminalProvUserId = _GarantiPaymentSettings.ProvisionUserId;
            result.TerminalUserId = _GarantiPaymentSettings.UserId;
            result.TerminalId = _GarantiPaymentSettings.TerminalId;
            result.MerchantId = _GarantiPaymentSettings.MerchantId;
            result.CustomerEmailAddress = _workContext.CurrentCustomer.Email;
            result.CustomerIpAddress = _workContext.CurrentCustomer.LastIpAddress;



            Random rndOid = new Random();
            int oidRnd = rndOid.Next(111111111, 999999999);
            result.OrderId = oidRnd.ToString("00000000000000000000");
            if (taksit == "1" || taksit == "01") taksit = "0";
            result.Installment = taksit;
            result.CreditCardNumber = processPaymentRequest.CreditCardNumber;
            result.CreditCardExpireYear = (processPaymentRequest.CreditCardExpireYear % 100).ToString();
            result.CreditCardExpireMonth = GetMonth(processPaymentRequest.CreditCardExpireMonth.ToString());
            result.CreditCardCvv2 = processPaymentRequest.CreditCardCvv2;
            result.CreditCardNumber = processPaymentRequest.CreditCardNumber;
            result.CustomerId = processPaymentRequest.CustomerId.ToString();

            String amount;
            amount = processPaymentRequest.OrderTotal.ToString("###,###.00");
            amount = amount.Replace(",", "");
            amount = amount.Replace(".", "");
            result.Amount = amount; // GetCulturePrice(currency, processPaymentRequest.OrderTotal);




            string SecurityData = GetSHA1(_GarantiPaymentSettings.ProvisionPassword + _strTerminalID).ToUpper();
            //sbLog.AppendLine("GetSHA1(" + _GarantiPaymentSettings.ProvisionPassword.ToString() + " + " + _GarantiPaymentSettings.TerminalId.ToString() + ").ToUpper();");
            //sbLog.AppendLine("SecurityData: " + SecurityData);
            string tmp = _GarantiPaymentSettings.TerminalId + result.OrderId + amount + _GarantiPaymentSettings.SuccessURL + _GarantiPaymentSettings.ErrorURL + "sales" + taksit + _GarantiPaymentSettings.StoreKey + SecurityData;
            tmp = tmp.ToUpper();
            string HashData = GetSHA1(_GarantiPaymentSettings.TerminalId + result.OrderId + amount + _GarantiPaymentSettings.SuccessURL + _GarantiPaymentSettings.ErrorURL + "sales" + taksit + _GarantiPaymentSettings.StoreKey + SecurityData).ToUpper();
            //sbLog.AppendLine("GetSHA1(" + garanti3dPaymentSettings.TerminalID.ToString() + " + " + oid.ToString() + " + " + strAmount.ToString() + " + " + garanti3dPaymentSettings.SuccessURL.ToString() + " + " + garanti3dPaymentSettings.ErrorURL.ToString() + " + sales + " + taksit.ToString() + " + " + garanti3dPaymentSettings.StoreKey.ToString() + " + " + SecurityData.ToString() + ").ToUpper();");
            //sbLog.AppendLine("HashData: " + HashData);

            result.Hash = HashData;

            result.RequiresRedirection = true;
            

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

            result.AddError("Not supported transaction type");
            
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
            controllerName = "PaymentGaranti";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.Garanti.Controllers" }, { "area", null } };
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
            controllerName = "PaymentGaranti";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.Garanti.Controllers" }, { "area", null } };
        }

        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            //it's not a redirection payment method. So we always return false
            return false;
        }

        public Type GetControllerType()
        {
            return typeof(PaymentGarantiController);
        }

        public override void Install()
        {
            var settings = new GarantiPaymentSettings()
            {
                
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
