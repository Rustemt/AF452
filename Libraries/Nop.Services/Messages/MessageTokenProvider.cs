using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Html;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Forums;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Seo;
using Nop.Core.Domain.Custom;
using Nop.Core.Events;
using Nop.Services.Payments;

namespace Nop.Services.Messages
{
    public partial class MessageTokenProvider : IMessageTokenProvider
    {
        #region Fields

        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ICurrencyService _currencyService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly IPaymentService _paymentService;
        private readonly StoreInformationSettings _storeSettings;
        private readonly MessageTemplatesSettings _templatesSettings;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly TaxSettings _taxSettings;
        private readonly IEventPublisher _eventPublisher = EngineContext.Current.Resolve<IEventPublisher>();

        #endregion

        #region Ctor

        public MessageTokenProvider(ILanguageService languageService,
            ILocalizationService localizationService, IDateTimeHelper dateTimeHelper,
            IEmailAccountService emailAccountService,
            IPriceFormatter priceFormatter, ICurrencyService currencyService,IWebHelper webHelper,
            IWorkContext workContext,
            StoreInformationSettings storeSettings, MessageTemplatesSettings templatesSettings,
            EmailAccountSettings emailAccountSettings, CatalogSettings catalogSettings,
            TaxSettings taxSettings, IPaymentService paymentService)
        {
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._dateTimeHelper = dateTimeHelper;
            this._emailAccountService = emailAccountService;
            this._priceFormatter = priceFormatter;
            this._currencyService = currencyService;
            this._webHelper = webHelper;
            this._workContext = workContext;
            this._paymentService = paymentService;
            this._storeSettings = storeSettings;
            this._templatesSettings = templatesSettings;
            this._emailAccountSettings = emailAccountSettings;
            this._catalogSettings = catalogSettings;
            this._taxSettings = taxSettings;
  
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Convert a collection to a HTML table
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>HTML table of products</returns>
        protected virtual string ProductListToHtmlTable(Order order, int languageId)
        {
            var result = "";

            var language = _languageService.GetLanguageById(languageId);

            var sb = new StringBuilder();
            sb.AppendLine("<table border=\"0\" style=\"width:100%;\">");

            #region Products
            sb.AppendLine(string.Format("<tr style=\"background-color:{0};text-align:center;\">", _templatesSettings.Color1));
            sb.AppendLine(string.Format("<th>{0}</th>", _localizationService.GetResource("Messages.Order.Product(s).Name", languageId)));
            sb.AppendLine(string.Format("<th>{0}</th>", _localizationService.GetResource("Messages.Order.Product(s).Price", languageId)));
            sb.AppendLine(string.Format("<th>{0}</th>", _localizationService.GetResource("Messages.Order.Product(s).Quantity", languageId)));
            sb.AppendLine(string.Format("<th>{0}</th>", _localizationService.GetResource("Messages.Order.Product(s).Total", languageId)));
            sb.AppendLine("</tr>");

            var table = order.OrderProductVariants.ToList();
            for (int i = 0; i <= table.Count - 1; i++)
            {
                var opv = table[i];
                var productVariant = opv.ProductVariant;
                if (productVariant == null)
                    continue;

                sb.AppendLine(string.Format("<tr style=\"background-color: {0};text-align: center;\">", _templatesSettings.Color2));
                //product name
                string productName = "";
                //product name
                if (!String.IsNullOrEmpty(opv.ProductVariant.GetLocalized(x => x.Name, languageId)))
                    productName = string.Format("{0}", opv.ProductVariant.Product.GetLocalized(x => x.Name, languageId));
                else
                    productName = opv.ProductVariant.Product.GetLocalized(x => x.Name, languageId);

                sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + HttpUtility.HtmlEncode(productName));
                //add download link
                var orderProcessingService = EngineContext.Current.Resolve<IOrderProcessingService>();
                if (orderProcessingService.IsDownloadAllowed(opv))
                {
                    //TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
                    string downloadUrl = string.Format("{0}download/getdownload?opvId={1}", _webHelper.GetStoreLocation(false), opv.OrderProductVariantGuid);
                    string downloadLink = string.Format("<a class=\"link\" href=\"{0}\">{1}</a>", downloadUrl, _localizationService.GetResource("Messages.Order.Products(s).Download", languageId));
                    sb.AppendLine("&nbsp;&nbsp;(");
                    sb.AppendLine(downloadLink);
                    sb.AppendLine(")");
                }
                //attributes
                if (!String.IsNullOrEmpty(opv.AttributeDescription))
                {
                    sb.AppendLine("<br />");
                    sb.AppendLine(opv.AttributeDescription);
                }
                //sku
                if (_catalogSettings.ShowProductSku)
                {
                    if (!String.IsNullOrEmpty(opv.ProductVariant.Sku))
                    {
                        sb.AppendLine("<br />");
                        string sku = string.Format(_localizationService.GetResource("Messages.Order.Product(s).SKU", languageId), HttpUtility.HtmlEncode(opv.ProductVariant.Sku));
                        sb.AppendLine(sku);
                    }
                }
                sb.AppendLine("</td>");

                string unitPriceStr = string.Empty;
                switch (order.CustomerTaxDisplayType)
                {
                    case TaxDisplayType.ExcludingTax:
                        {
                            var opvUnitPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(opv.UnitPriceExclTax, order.CurrencyRate);
                            unitPriceStr = _priceFormatter.FormatPrice(opvUnitPriceExclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, language, false);
                        }
                        break;
                    case TaxDisplayType.IncludingTax:
                        {
                            var opvUnitPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(opv.UnitPriceInclTax, order.CurrencyRate);
                            unitPriceStr = _priceFormatter.FormatPrice(opvUnitPriceInclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, language, true);
                        }
                        break;
                }
                sb.AppendLine(string.Format("<td style=\"padding: 0.6em 0.4em;text-align: center;\">{0}</td>", unitPriceStr));

                sb.AppendLine(string.Format("<td style=\"padding: 0.6em 0.4em;text-align: center;\">{0}</td>", opv.Quantity));

                string priceStr = string.Empty;
                switch (order.CustomerTaxDisplayType)
                {
                    case TaxDisplayType.ExcludingTax:
                        {
                            var opvPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(opv.PriceExclTax, order.CurrencyRate);
                            priceStr = _priceFormatter.FormatPrice(opvPriceExclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, language, false);
                        }
                        break;
                    case TaxDisplayType.IncludingTax:
                        {
                            var opvPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(opv.PriceInclTax, order.CurrencyRate);
                            priceStr = _priceFormatter.FormatPrice(opvPriceInclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, language, true);
                        }
                        break;
                }
                sb.AppendLine(string.Format("<td style=\"padding: 0.6em 0.4em;text-align: center;\">{0}</td>", priceStr));

                sb.AppendLine("</tr>");
            }
            #endregion

            #region Checkout Attributes

            if (!String.IsNullOrEmpty(order.CheckoutAttributeDescription))
            {
                sb.AppendLine("<tr><td style=\"text-align:right;\" colspan=\"1\">&nbsp;</td><td colspan=\"3\" style=\"text-align:right\">");
                sb.AppendLine(order.CheckoutAttributeDescription);
                sb.AppendLine("</td></tr>");
            }

            #endregion

            #region Totals

            string cusSubTotal = string.Empty;
            bool dislaySubTotalDiscount = false;
            string cusSubTotalDiscount = string.Empty;
            string cusShipTotal = string.Empty;
            string cusPaymentMethodAdditionalFee = string.Empty;
            var taxRates = new SortedDictionary<decimal, decimal>();
            string cusTaxTotal = string.Empty;
            string cusDiscount = string.Empty;
            string cusTotal = string.Empty;
            //subtotal, shipping, payment method fee
            switch (order.CustomerTaxDisplayType)
            {
                case TaxDisplayType.ExcludingTax:
                    {
                        //subtotal
                        var orderSubtotalExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubtotalExclTax, order.CurrencyRate);
                        cusSubTotal = _priceFormatter.FormatPrice(orderSubtotalExclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, language, false);
                        cusSubTotal = _priceFormatter.FormatPrice(orderSubtotalExclTaxInCustomerCurrency, false, false);
                        //discount (applied to order subtotal)
                        var orderSubTotalDiscountExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubTotalDiscountExclTax, order.CurrencyRate);
                        if (orderSubTotalDiscountExclTaxInCustomerCurrency > decimal.Zero)
                        {
                            cusSubTotalDiscount = _priceFormatter.FormatPrice(-orderSubTotalDiscountExclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, language, false);
                            dislaySubTotalDiscount = true;
                        }
                        //shipping
                        var orderShippingExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingExclTax, order.CurrencyRate);
                        cusShipTotal = _priceFormatter.FormatShippingPrice(orderShippingExclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, language, false);
                        //payment method additional fee
                        var paymentMethodAdditionalFeeExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeExclTax, order.CurrencyRate);
                        cusPaymentMethodAdditionalFee = _priceFormatter.FormatPaymentMethodAdditionalFee(paymentMethodAdditionalFeeExclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, language, false);
                    }
                    break;
                case TaxDisplayType.IncludingTax:
                    {
                        //subtotal
                        var orderSubtotalInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubtotalInclTax, order.CurrencyRate);
                        cusSubTotal = _priceFormatter.FormatPrice(orderSubtotalInclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, language, true);
                        cusSubTotal = _priceFormatter.FormatPrice(orderSubtotalInclTaxInCustomerCurrency, false, false);
                        //discount (applied to order subtotal)
                        var orderSubTotalDiscountInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubTotalDiscountInclTax, order.CurrencyRate);
                        if (orderSubTotalDiscountInclTaxInCustomerCurrency > decimal.Zero)
                        {
                            cusSubTotalDiscount = _priceFormatter.FormatPrice(-orderSubTotalDiscountInclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, language, true);
                            dislaySubTotalDiscount = true;
                        }
                        //shipping
                        var orderShippingInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingInclTax, order.CurrencyRate);
                        cusShipTotal = _priceFormatter.FormatShippingPrice(orderShippingInclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, language, true);
                        //payment method additional fee
                        var paymentMethodAdditionalFeeInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeInclTax, order.CurrencyRate);
                        cusPaymentMethodAdditionalFee = _priceFormatter.FormatPaymentMethodAdditionalFee(paymentMethodAdditionalFeeInclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, language, true);
                    }
                    break;
            }

            //shipping
            bool dislayShipping = order.ShippingStatus != ShippingStatus.ShippingNotRequired;

            //payment method fee
            bool displayPaymentMethodFee = true;
            if (order.PaymentMethodAdditionalFeeExclTax == decimal.Zero)
            {
                displayPaymentMethodFee = false;
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
                    taxRates = new SortedDictionary<decimal, decimal>();
                    foreach (var tr in order.TaxRatesDictionary)
                        taxRates.Add(tr.Key, _currencyService.ConvertCurrency(tr.Value, order.CurrencyRate));

                    displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Count > 0;
                    displayTax = !displayTaxRates;

                    var orderTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTax, order.CurrencyRate);
                    string taxStr = _priceFormatter.FormatPrice(orderTaxInCustomerCurrency, false, order.CustomerCurrencyCode, false, language);
                    cusTaxTotal = taxStr;
                }
            }

            //discount
            bool dislayDiscount = false;
            if (order.OrderDiscount > decimal.Zero)
            {
                var orderDiscountInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderDiscount, order.CurrencyRate);
                cusDiscount = _priceFormatter.FormatPrice(-orderDiscountInCustomerCurrency, false, order.CustomerCurrencyCode, false, language);
                dislayDiscount = true;
            }

            //total
            var orderTotalInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
            cusTotal = _priceFormatter.FormatPrice(orderTotalInCustomerCurrency, false, order.CustomerCurrencyCode, false, language);
            




            //subtotal
            sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, _localizationService.GetResource("Messages.Order.SubTotal", languageId), cusSubTotal));

            //discount (applied to order subtotal)
            if (dislaySubTotalDiscount)
            {
                sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, _localizationService.GetResource("Messages.Order.SubTotalDiscount", languageId), cusSubTotalDiscount));
            }


            //shipping
            if (dislayShipping)
            {
                sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, _localizationService.GetResource("Messages.Order.Shipping", languageId), cusShipTotal));
            }

            //payment method fee
            if (displayPaymentMethodFee)
            {
                string paymentMethodFeeTitle = _localizationService.GetResource("Messages.Order.PaymentMethodAdditionalFee", languageId);
                sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, paymentMethodFeeTitle, cusPaymentMethodAdditionalFee));
            }

            //tax
            if (displayTax)
            {
                sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, _localizationService.GetResource("Messages.Order.Tax", languageId), cusTaxTotal));
            }
            if (displayTaxRates)
            {
                foreach (var item in taxRates)
                {
                    string taxRate = String.Format(_localizationService.GetResource("Messages.Order.TaxRateLine"), _priceFormatter.FormatTaxRate(item.Key));
                    string taxValue = _priceFormatter.FormatPrice(item.Value, false, order.CustomerCurrencyCode, false, language);
                    sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, taxRate, taxValue));
                }
            }

            //discount
            if (dislayDiscount)
            {
                sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, _localizationService.GetResource("Messages.Order.TotalDiscount", languageId), cusDiscount));
            }

            //gift cards
            var gcuhC = order.GiftCardUsageHistory;
            foreach (var gcuh in gcuhC)
            {
                string giftCardText = String.Format(_localizationService.GetResource("Messages.Order.GiftCardInfo", languageId), HttpUtility.HtmlEncode(gcuh.GiftCard.GiftCardCouponCode));
                string giftCardAmount = _priceFormatter.FormatPrice(-(_currencyService.ConvertCurrency(gcuh.UsedValue, order.CurrencyRate)), false, order.CustomerCurrencyCode, false, language);
                sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, giftCardText, giftCardAmount));
            }

            //reward points
            if (order.RedeemedRewardPointsEntry != null)
            {
                string rpTitle = string.Format(_localizationService.GetResource("Messages.Order.RewardPoints", languageId), -order.RedeemedRewardPointsEntry.Points);
                string rpAmount = _priceFormatter.FormatPrice(-(_currencyService.ConvertCurrency(order.RedeemedRewardPointsEntry.UsedAmount, order.CurrencyRate)), false, order.CustomerCurrencyCode, false, language);
                sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, rpTitle, rpAmount));
            }

            //total
            sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, _localizationService.GetResource("Messages.Order.OrderTotal", languageId), cusTotal));
            #endregion

            sb.AppendLine("</table>");
            result = sb.ToString();
            return result;
        }

        #endregion

        #region Methods

        //Has AF version
        //public virtual void AddStoreTokens(IList<Token> tokens)
        //{
        //    tokens.Add(new Token("Store.Name", _storeSettings.StoreName));
        //    tokens.Add(new Token("Store.URL", _storeSettings.StoreUrl, true));
        //    var defaultEmailAccount = _emailAccountService.GetEmailAccountById(_emailAccountSettings.DefaultEmailAccountId);
        //    if (defaultEmailAccount == null)
        //        defaultEmailAccount = _emailAccountService.GetAllEmailAccounts().FirstOrDefault();
        //    tokens.Add(new Token("Store.Email", defaultEmailAccount.Email));
        //}

        //Has AF Version
        //public virtual void AddOrderTokens(IList<Token> tokens, Order order, int languageId)
        //{
        //    tokens.Add(new Token("Order.OrderNumber", order.Id.ToString()));

        //    tokens.Add(new Token("Order.CustomerFullName", string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName)));
        //    tokens.Add(new Token("Order.CustomerEmail", order.BillingAddress.Email));


        //    tokens.Add(new Token("Order.BillingFirstName", order.BillingAddress.FirstName));
        //    tokens.Add(new Token("Order.BillingLastName", order.BillingAddress.LastName));
        //    tokens.Add(new Token("Order.BillingPhoneNumber", order.BillingAddress.PhoneNumber));
        //    tokens.Add(new Token("Order.BillingEmail", order.BillingAddress.Email));
        //    tokens.Add(new Token("Order.BillingFaxNumber", order.BillingAddress.FaxNumber));
        //    tokens.Add(new Token("Order.BillingCompany", order.BillingAddress.Company));
        //    tokens.Add(new Token("Order.BillingAddress1", order.BillingAddress.Address1));
        //    tokens.Add(new Token("Order.BillingAddress2", order.BillingAddress.Address2));
        //    tokens.Add(new Token("Order.BillingCity", order.BillingAddress.City));
        //    tokens.Add(new Token("Order.BillingStateProvince", order.BillingAddress.StateProvince != null ? order.BillingAddress.StateProvince.GetLocalized(x => x.Name) : ""));
        //    tokens.Add(new Token("Order.BillingZipPostalCode", order.BillingAddress.ZipPostalCode));
        //    tokens.Add(new Token("Order.BillingCountry", order.BillingAddress.Country != null ? order.BillingAddress.Country.GetLocalized(x => x.Name) : ""));

        //    tokens.Add(new Token("Order.ShippingMethod", order.ShippingMethod));
        //    tokens.Add(new Token("Order.ShippingFirstName", order.ShippingAddress != null ? order.ShippingAddress.FirstName : ""));
        //    tokens.Add(new Token("Order.ShippingLastName", order.ShippingAddress != null ? order.ShippingAddress.LastName : ""));
        //    tokens.Add(new Token("Order.ShippingPhoneNumber", order.ShippingAddress != null ? order.ShippingAddress.PhoneNumber : ""));
        //    tokens.Add(new Token("Order.ShippingEmail", order.ShippingAddress != null ? order.ShippingAddress.Email : ""));
        //    tokens.Add(new Token("Order.ShippingFaxNumber", order.ShippingAddress != null ? order.ShippingAddress.FaxNumber : ""));
        //    tokens.Add(new Token("Order.ShippingCompany", order.ShippingAddress != null ? order.ShippingAddress.Company : ""));
        //    tokens.Add(new Token("Order.ShippingAddress1", order.ShippingAddress != null ? order.ShippingAddress.Address1 : ""));
        //    tokens.Add(new Token("Order.ShippingAddress2", order.ShippingAddress != null ? order.ShippingAddress.Address2 : ""));
        //    tokens.Add(new Token("Order.ShippingCity", order.ShippingAddress != null ? order.ShippingAddress.City : ""));
        //    tokens.Add(new Token("Order.ShippingStateProvince", order.ShippingAddress != null && order.ShippingAddress.StateProvince != null ? order.ShippingAddress.StateProvince.GetLocalized(x => x.Name) : ""));
        //    tokens.Add(new Token("Order.ShippingZipPostalCode", order.ShippingAddress != null ? order.ShippingAddress.ZipPostalCode : ""));
        //    tokens.Add(new Token("Order.ShippingCountry", order.ShippingAddress != null && order.ShippingAddress.Country != null ? order.ShippingAddress.Country.GetLocalized(x => x.Name) : ""));


        //    tokens.Add(new Token("Order.TrackingNumber", order.TrackingNumber));
        //    tokens.Add(new Token("Order.VatNumber", order.VatNumber));

        //    tokens.Add(new Token("Order.Product(s)", ProductListToHtmlTable(order, languageId), true));

        //    var language = _languageService.GetLanguageById(languageId);
        //    if (language != null && !String.IsNullOrEmpty(language.LanguageCulture))
        //    {
        //        DateTime createdOn = _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, TimeZoneInfo.Utc, _dateTimeHelper.GetCustomerTimeZone(order.Customer));
        //        tokens.Add(new Token("Order.CreatedOn", createdOn.ToString("D", new CultureInfo(language.LanguageCulture))));
        //    }
        //    else
        //    {
        //        tokens.Add(new Token("Order.CreatedOn", order.CreatedOnUtc.ToString("D")));
        //    }

        //    //TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
        //    tokens.Add(new Token("Order.OrderURLForCustomer", string.Format("{0}orderdetails/{1}", _webHelper.GetStoreLocation(false), order.Id), true));
        //}

        public virtual void AddReturnRequestTokens(IList<Token> tokens, ReturnRequest returnRequest, OrderProductVariant opv)
        {
            tokens.Add(new Token("ReturnRequest.ID", returnRequest.Id.ToString()));
            tokens.Add(new Token("ReturnRequest.Product.Quantity", returnRequest.Quantity.ToString()));
            tokens.Add(new Token("ReturnRequest.Product.Name", opv.ProductVariant.FullProductName));
            tokens.Add(new Token("ReturnRequest.Reason", returnRequest.ReasonForReturn));
            tokens.Add(new Token("ReturnRequest.RequestedAction", returnRequest.RequestedAction));
            tokens.Add(new Token("ReturnRequest.CustomerComment", HtmlHelper.FormatText(returnRequest.CustomerComments, false, true, false, false, false, false), true));
            tokens.Add(new Token("ReturnRequest.StaffNotes", HtmlHelper.FormatText(returnRequest.StaffNotes, false, true, false, false, false, false), true));
            tokens.Add(new Token("ReturnRequest.Status", returnRequest.ReturnRequestStatus.GetLocalizedEnum(_localizationService, _workContext)));
        }

        public virtual void AddGiftCardTokens(IList<Token> tokens, GiftCard giftCard)
        {   
            tokens.Add(new Token("GiftCard.SenderName", giftCard.SenderName));
            tokens.Add(new Token("GiftCard.SenderEmail",giftCard.SenderEmail));
            tokens.Add(new Token("GiftCard.RecipientName", giftCard.RecipientName));
            tokens.Add(new Token("GiftCard.RecipientEmail", giftCard.RecipientEmail));
            tokens.Add(new Token("GiftCard.Amount", _priceFormatter.FormatPrice(giftCard.Amount, false, false)));
            tokens.Add(new Token("GiftCard.CouponCode", giftCard.GiftCardCouponCode));

            var giftCardMesage = !String.IsNullOrWhiteSpace(giftCard.Message) ? 
                HtmlHelper.FormatText(giftCard.Message, false, true, false, false, false, false) : "";

            tokens.Add(new Token("GiftCard.Message", giftCardMesage, true));
        }

        public virtual void AddCustomerTokens(IList<Token> tokens, Customer customer)
        {
            tokens.Add(new Token("Customer.Email", customer.Email));
            tokens.Add(new Token("Customer.Username", customer.Username));
            tokens.Add(new Token("Customer.FullName", customer.GetFullName()));
            tokens.Add(new Token("Customer.VatNumber", customer.VatNumber));
            tokens.Add(new Token("Customer.VatNumberStatus", customer.VatNumberStatus.ToString()));


            //TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
            string passwordRecoveryUrl = string.Format("{0}passwordrecovery/confirm?token={1}&email={2}", _webHelper.GetStoreLocation(false), customer.GetAttribute<string>(SystemCustomerAttributeNames.PasswordRecoveryToken), customer.Email);
            string accountActivationUrl = string.Format("{0}customer/activation?token={1}&email={2}", _webHelper.GetStoreLocation(false), customer.GetAttribute<string>(SystemCustomerAttributeNames.AccountActivationToken), customer.Email);
            var wishlistUrl = string.Format("{0}wishlist/{1}", _webHelper.GetStoreLocation(false), customer.CustomerGuid);
            tokens.Add(new Token("Customer.PasswordRecoveryURL", passwordRecoveryUrl, true));
            tokens.Add(new Token("Customer.AccountActivationURL", accountActivationUrl, true));
            tokens.Add(new Token("Wishlist.URLForCustomer", wishlistUrl, true));
        }

        public virtual void AddNewsLetterSubscriptionTokens(IList<Token> tokens, NewsLetterSubscription subscription)
        {
            tokens.Add(new Token("NewsLetterSubscription.Email", subscription.Email));


            const string urlFormat = "{0}newsletter/subscriptionactivation/{1}/{2}";

            var activationUrl = String.Format(urlFormat, _webHelper.GetStoreLocation(false), subscription.NewsLetterSubscriptionGuid, "true");
            tokens.Add(new Token("NewsLetterSubscription.ActivationUrl", activationUrl, true));

            var deActivationUrl = String.Format(urlFormat, _webHelper.GetStoreLocation(false), subscription.NewsLetterSubscriptionGuid, "false");
            tokens.Add(new Token("NewsLetterSubscription.DeactivationUrl", deActivationUrl, true));
            tokens.Add(new Token("Customer.FullName", subscription.FirstName , true));
        }

        public virtual void AddCampaignNewsLetterSubscriptionTokens(IList<Token> tokens, NewsLetterSubscription subscription)
        {
            tokens.Add(new Token("NewsLetterSubscription.Email", subscription.Email));


            const string urlFormat = "{0}CampaignRegisterActive/{1}";

            var activationUrl = String.Format(urlFormat, _webHelper.GetStoreLocation(false), subscription.Id);
            tokens.Add(new Token("NewsLetterSubscription.ActivationUrl", activationUrl, true));

            var deActivationUrl = String.Format(urlFormat, _webHelper.GetStoreLocation(false), subscription.NewsLetterSubscriptionGuid, "false");
            tokens.Add(new Token("NewsLetterSubscription.DeactivationUrl", deActivationUrl, true));
            tokens.Add(new Token("Customer.FullName", subscription.FirstName, true));
        }

        public virtual void AddCampaignConvokeNewsLetterSubscriptionTokens(IList<Token> tokens, NewsLetterSubscription subscription)
        {
            tokens.Add(new Token("NewsLetterSubscription.Email", subscription.Email));
            tokens.Add(new Token("NewsLetterSubscription.RefererEmail", subscription.RefererEmail));

            const string urlFormat = "{0}CampaignRegister";

            var activationUrl = String.Format(urlFormat, _webHelper.GetStoreLocation(false));
            tokens.Add(new Token("NewsLetterSubscription.ActivationUrl", activationUrl, true));

            var deActivationUrl = String.Format(urlFormat, _webHelper.GetStoreLocation(false), subscription.NewsLetterSubscriptionGuid, "false");
            tokens.Add(new Token("NewsLetterSubscription.DeactivationUrl", deActivationUrl, true));
            tokens.Add(new Token("Customer.FullName", subscription.FirstName, true));
        }

        public virtual void AddProductReviewTokens(IList<Token> tokens, ProductReview productReview)
        {
            tokens.Add(new Token("ProductReview.ProductName", productReview.Product.Name));
        }

        public virtual void AddBlogCommentTokens(IList<Token> tokens, BlogComment blogComment)
        {
            tokens.Add(new Token("BlogComment.BlogPostTitle", blogComment.BlogPost.Title));
        }

        public virtual void AddNewsCommentTokens(IList<Token> tokens, NewsComment newsComment)
        {
            tokens.Add(new Token("NewsComment.NewsTitle", newsComment.NewsItem.Title));
        }

        //Has AF version
        //public virtual void AddProductTokens(IList<Token> tokens, Product product)
        //{
        //    tokens.Add(new Token("Product.Name", product.Name));
        //    tokens.Add(new Token("Product.ShortDescription", product.ShortDescription, true));

        //    //TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
        //    var productUrl = string.Format("{0}p/{1}/{2}", _webHelper.GetStoreLocation(false), product.Id, product.GetSeName());
        //    tokens.Add(new Token("Product.ProductURLForCustomer", productUrl, true));
        //}

        public virtual void AddProductVariantTokens(IList<Token> tokens, ProductVariant productVariant)
        {
            tokens.Add(new Token("ProductVariant.ID", productVariant.Id.ToString()));
            tokens.Add(new Token("ProductVariant.FullProductName", productVariant.FullProductName));
            tokens.Add(new Token("ProductVariant.StockQuantity", productVariant.StockQuantity.ToString()));
        }

        public virtual void AddForumTopicTokens(IList<Token> tokens, ForumTopic forumTopic, 
            int? friendlyForumTopicPageIndex = null, int? appendedPostIdentifierAnchor = null)
        {
            //TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
            string topicUrl = null;
            if (friendlyForumTopicPageIndex.HasValue && friendlyForumTopicPageIndex.Value > 1)
                topicUrl = string.Format("{0}boards/topic/{1}/{2}/page/{3}", _webHelper.GetStoreLocation(false), forumTopic.Id, forumTopic.GetSeName(), friendlyForumTopicPageIndex.Value);
            else
                topicUrl = string.Format("{0}boards/topic/{1}/{2}", _webHelper.GetStoreLocation(false), forumTopic.Id, forumTopic.GetSeName());
            if (appendedPostIdentifierAnchor.HasValue && appendedPostIdentifierAnchor.Value > 0)
                topicUrl = string.Format("{0}#{1}", topicUrl, appendedPostIdentifierAnchor.Value);
            tokens.Add(new Token("Forums.TopicURL", topicUrl, true));
            tokens.Add(new Token("Forums.TopicName", forumTopic.Subject));
        }

        public virtual void AddForumPostTokens(IList<Token> tokens, ForumPost forumPost)
        {
            tokens.Add(new Token("Forums.PostAuthor", forumPost.Customer.FormatUserName()));
            tokens.Add(new Token("Forums.PostBody", forumPost.FormatPostText(), true));
        }

        public virtual void AddForumTokens(IList<Token> tokens, Forum forum)
        {
            //TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
            var forumUrl = string.Format("{0}boards/forum/{1}/{2}", _webHelper.GetStoreLocation(false), forum.Id, forum.GetSeName());
            tokens.Add(new Token("Forums.ForumURL", forumUrl, true));
            tokens.Add(new Token("Forums.ForumName", forum.Name));
        }

        public virtual void AddPrivateMessageTokens(IList<Token> tokens, PrivateMessage privateMessage)
        {
            tokens.Add(new Token("PrivateMessage.Subject", privateMessage.Subject));
            tokens.Add(new Token("PrivateMessage.Text",  privateMessage.FormatPrivateMessageText(), true));
        }

        public virtual void AddBackInStockTokens(IList<Token> tokens, BackInStockSubscription subscription)
        {
            tokens.Add(new Token("BackInStockSubscription.ProductName", subscription.ProductVariant.FullProductName));
        }

        /// <summary>
        /// Gets list of allowed (supported) message tokens for campaigns
        /// </summary>
        /// <returns>List of allowed (supported) message tokens for campaigns</returns>
        public virtual string[] GetListOfCampaignAllowedTokens()
        {
            var allowedTokens = new List<string>()
            {
                "%Store.Name%",
                "%Store.URL%",
                "%Store.Email%",
                "%NewsLetterSubscription.Email%",
                "%NewsLetterSubscription.ActivationUrl%",
                "%NewsLetterSubscription.DeactivationUrl%"
            };
            return allowedTokens.ToArray();
        }

        public virtual string[] GetListOfAllowedTokens()
        {
            var allowedTokens = new List<string>()
            {
                "%Store.Name%",
                "%Store.URL%",
                "%Store.Email%",
                "%Order.OrderNumber%",
                "%Order.CustomerFullName%",
                "%Order.CustomerEmail%",
                "%Order.BillingFirstName%",
                "%Order.BillingLastName%",
                "%Order.BillingPhoneNumber%",
                "%Order.BillingEmail%",
                "%Order.BillingFaxNumber%",
                "%Order.BillingCompany%",
                "%Order.BillingAddress2%",
                "%Order.BillingCity%",
                "%Order.BillingStateProvince%",
                "%Order.BillingZipPostalCode%",
                "%Order.BillingCountry%",
                "%Order.ShippingMethod%",
                "%Order.ShippingFirstName%",
                "%Order.ShippingLastName%",
                "%Order.ShippingPhoneNumber%",
                "%Order.ShippingEmail%",
                "%Order.ShippingFaxNumber%",
                "%Order.ShippingCompany%",
                "%Order.ShippingAddress1%",
                "%Order.ShippingAddress2%",
                "%Order.ShippingCity%",
                "%Order.ShippingStateProvince%",
                "%Order.ShippingZipPostalCode%", 
                "%Order.ShippingCountry%",
                "%Order.TrackingNumber%",
                "%Order.VatNumber%", 
                "%Order.Product(s)%",
                "%Order.CreatedOn%",
                "%Order.OrderURLForCustomer%",
                "%ReturnRequest.ID%", 
                "%ReturnRequest.Product.Quantity%",
                "%ReturnRequest.Product.Name%", 
                "%ReturnRequest.Reason%", 
                "%ReturnRequest.RequestedAction%", 
                "%ReturnRequest.CustomerComment%", 
                "%ReturnRequest.StaffNotes%",
                "%ReturnRequest.Status%",
                "%GiftCard.SenderName%", 
                "%GiftCard.SenderEmail%",
                "%GiftCard.RecipientName%", 
                "%GiftCard.RecipientEmail%", 
                "%GiftCard.Amount%", 
                "%GiftCard.CouponCode%",
                "%GiftCard.Message%",
                "%Customer.Email%", 
                "%Customer.Username%", 
                "%Customer.FullName%", 
                "%Customer.VatNumber%",
                "%Customer.VatNumberStatus%", 
                "%Customer.PasswordRecoveryURL%", 
                "%Customer.AccountActivationURL%", 
                "%Wishlist.URLForCustomer%", 
                "NewsLetterSubscription.Email%", 
                "%NewsLetterSubscription.ActivationUrl%",
                "%NewsLetterSubscription.DeactivationUrl%", 
                "%ProductReview.ProductName%", 
                "%BlogComment.BlogPostTitle%", 
                "%NewsComment.NewsTitle%",
                "%Product.Name%",
                "%Product.ShortDescription%", 
                "%Product.ProductURLForCustomer%",
                "%ProductVariant.ID%", 
                "%ProductVariant.FullProductName%", 
                "%ProductVariant.StockQuantity%", 
                "%Forums.TopicURL%",
                "%Forums.TopicName%", 
                "%Forums.PostAuthor%",
                "%Forums.PostBody%",
                "%Forums.ForumURL%", 
                "%Forums.ForumName%", 
                "%PrivateMessage.Subject%", 
                "%PrivateMessage.Text%",
                "%BackInStockSubscription.ProductName%",
            };
            return allowedTokens.ToArray();
        }

        public virtual void AddSalesAgreementTokens(IList<Token> tokens, SalesAgreement SalesAgreementDetails)
        {

            tokens.Add(new Token("Order.CustomerFullName", string.Format("{0} {1}", SalesAgreementDetails.OrderReviewData.BillingAddress.FirstName, SalesAgreementDetails.OrderReviewData.BillingAddress.LastName)));
            tokens.Add(new Token("Order.CustomerEmail", SalesAgreementDetails.OrderReviewData.BillingAddress.Email));


            tokens.Add(new Token("Order.BillingFirstName", SalesAgreementDetails.OrderReviewData.BillingAddress.FirstName));
            tokens.Add(new Token("Order.BillingLastName", SalesAgreementDetails.OrderReviewData.BillingAddress.LastName));
            tokens.Add(new Token("Order.BillingPhoneNumber", SalesAgreementDetails.OrderReviewData.BillingAddress.PhoneNumber));
            tokens.Add(new Token("Order.BillingEmail", SalesAgreementDetails.OrderReviewData.BillingAddress.Email));
            tokens.Add(new Token("Order.BillingFaxNumber", SalesAgreementDetails.OrderReviewData.BillingAddress.FaxNumber));
            tokens.Add(new Token("Order.BillingCompany", SalesAgreementDetails.OrderReviewData.BillingAddress.Company));
            tokens.Add(new Token("Order.BillingAddress1", SalesAgreementDetails.OrderReviewData.BillingAddress.Address1));
            tokens.Add(new Token("Order.BillingAddress2", SalesAgreementDetails.OrderReviewData.BillingAddress.Address2));
            tokens.Add(new Token("Order.BillingCity", SalesAgreementDetails.OrderReviewData.BillingAddress.City));

            tokens.Add(new Token("Order.BillingStateProvince", SalesAgreementDetails.OrderReviewData.Billing_SatateProvision));
            tokens.Add(new Token("Order.BillingZipPostalCode", SalesAgreementDetails.OrderReviewData.BillingAddress.ZipPostalCode));
            tokens.Add(new Token("Order.BillingCountry", SalesAgreementDetails.OrderReviewData.Billing_Country));

            tokens.Add(new Token("Order.ShippingMethod", SalesAgreementDetails.OrderReviewData.ShippingMethod));
            tokens.Add(new Token("Order.ShippingFirstName", SalesAgreementDetails.OrderReviewData.ShippingAddress != null ? SalesAgreementDetails.OrderReviewData.ShippingAddress.FirstName : ""));
            tokens.Add(new Token("Order.ShippingLastName", SalesAgreementDetails.OrderReviewData.ShippingAddress != null ? SalesAgreementDetails.OrderReviewData.ShippingAddress.LastName : ""));
            tokens.Add(new Token("Order.ShippingPhoneNumber", SalesAgreementDetails.OrderReviewData.ShippingAddress != null ? SalesAgreementDetails.OrderReviewData.ShippingAddress.PhoneNumber : ""));
            tokens.Add(new Token("Order.ShippingEmail", SalesAgreementDetails.OrderReviewData.ShippingAddress != null ? SalesAgreementDetails.OrderReviewData.ShippingAddress.Email : ""));
            tokens.Add(new Token("Order.ShippingFaxNumber", SalesAgreementDetails.OrderReviewData.ShippingAddress != null ? SalesAgreementDetails.OrderReviewData.ShippingAddress.FaxNumber : ""));
            tokens.Add(new Token("Order.ShippingCompany", SalesAgreementDetails.OrderReviewData.ShippingAddress != null ? SalesAgreementDetails.OrderReviewData.ShippingAddress.Company : ""));
            tokens.Add(new Token("Order.ShippingAddress1", SalesAgreementDetails.OrderReviewData.ShippingAddress != null ? SalesAgreementDetails.OrderReviewData.ShippingAddress.Address1 : ""));
            tokens.Add(new Token("Order.ShippingAddress2", SalesAgreementDetails.OrderReviewData.ShippingAddress != null ? SalesAgreementDetails.OrderReviewData.ShippingAddress.Address2 : ""));
            tokens.Add(new Token("Order.ShippingCity", SalesAgreementDetails.OrderReviewData.ShippingAddress != null ? SalesAgreementDetails.OrderReviewData.ShippingAddress.City : ""));
            tokens.Add(new Token("Order.ShippingStateProvince", SalesAgreementDetails.OrderReviewData.Shipping_SatateProvision));
            tokens.Add(new Token("Order.ShippingZipPostalCode", SalesAgreementDetails.OrderReviewData.ShippingAddress != null ? SalesAgreementDetails.OrderReviewData.ShippingAddress.ZipPostalCode : ""));
            tokens.Add(new Token("Order.ShippingCountry", SalesAgreementDetails.OrderReviewData.Shipping_Country));

            tokens.Add(new Token("Order.PaymentMethod", SalesAgreementDetails.OrderReviewData.PaymentMethod));
            var languageId = _workContext.WorkingLanguage.Id;
            tokens.Add(new Token("Order.Product(s)", ProductListToHtmlTable(SalesAgreementDetails, languageId), true));

            var language = _languageService.GetLanguageById(languageId);
            if (language != null && !String.IsNullOrEmpty(language.LanguageCulture))
            {
                DateTime createdOn = _dateTimeHelper.ConvertToUserTime(System.DateTime.UtcNow, TimeZoneInfo.Utc);
                tokens.Add(new Token("Order.CreatedOn", createdOn.ToString("D", new CultureInfo(language.LanguageCulture))));
            }
            else
            {
                DateTime createdOn = System.DateTime.UtcNow;
                tokens.Add(new Token("Order.CreatedOn", createdOn.ToString("D")));
            }


            // Custome toekn
            #region

            if (SalesAgreementDetails.OrderTotal.cavalue != null)
            {

                tokens.Add(new Token("Order.Cavalue", SalesAgreementDetails.OrderTotal.cavalue));
            }
            else
            {
                tokens.Add(new Token("Order.OrderSubTotalFinal", SalesAgreementDetails.OrderTotal.SubTotal));
            }
            tokens.Add(new Token("Messages.Order.Shipping", SalesAgreementDetails.OrderTotal.Shipping));


            #endregion
            //event notification
            _eventPublisher.EntityTokensAdded(SalesAgreementDetails, tokens);
        }

        protected virtual string ProductListToHtmlTable(SalesAgreement SalesAgreementDetails, int languageId)
        {
            var result = "";

            var language = _languageService.GetLanguageById(languageId);

            var sb = new StringBuilder();
            sb.AppendLine("<table border=\"0\" style=\"width:100%;\">");

            #region Products
            sb.AppendLine(string.Format("<tr style=\"background-color:{0};text-align:center;\">", _templatesSettings.Color1));
            sb.AppendLine(string.Format("<th>{0}</th>", _localizationService.GetResource("Messages.Order.Product(s).Name", languageId)));
            sb.AppendLine(string.Format("<th>{0}</th>", _localizationService.GetResource("Messages.Order.Product(s).Price", languageId)));
            sb.AppendLine(string.Format("<th>{0}</th>", _localizationService.GetResource("Messages.Order.Product(s).Quantity", languageId)));
            sb.AppendLine(string.Format("<th>{0}</th>", _localizationService.GetResource("Messages.Order.Product(s).Total", languageId)));
            sb.AppendLine("</tr>");

            foreach (var item in SalesAgreementDetails.Items)
            {
                var product = item;

                if (product == null)
                    continue;

                sb.AppendLine(string.Format("<tr style=\"background-color: {0};text-align: center;\">", _templatesSettings.Color2));
                //product name
                string productName = product.ProductName;

                sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + HttpUtility.HtmlEncode(productName));

                //attributes
                if (!String.IsNullOrEmpty(item.AttributeInfo))
                {
                    sb.AppendLine("<br />");
                    sb.AppendLine(item.AttributeInfo.Replace("</dd>","<br/>").Replace("<dd>","").Replace("<dt>","").Replace("</dt>",""));
                }
                //sku
                if (_catalogSettings.ShowProductSku)
                {
                    var sku = item.Sku;
                    if (!String.IsNullOrEmpty(sku))
                    {
                        sb.AppendLine("<br />");
                        sb.AppendLine(string.Format(_localizationService.GetResource("Messages.Order.Product(s).SKU", languageId), HttpUtility.HtmlEncode(sku)));
                    }
                }
                sb.AppendLine("</td>");

                sb.AppendLine(string.Format("<td style=\"padding: 0.6em 0.4em;text-align: center;\">{0}</td>", item.UnitPrice));

                sb.AppendLine(string.Format("<td style=\"padding: 0.6em 0.4em;text-align: center;\">{0}</td>", item.Quantity));

                sb.AppendLine(string.Format("<td style=\"padding: 0.6em 0.4em;text-align: center;\">{0}</td>", item.SubTotal));

                sb.AppendLine("</tr>");
            }

            #endregion

            #region Checkout Attributes

            if (!String.IsNullOrEmpty(SalesAgreementDetails.CheckoutAttributeInfo))
            {
                sb.AppendLine( string.Format("<tr><td style=\"background-color: {0}; text-align:right;\" colspan=\"3\">&nbsp;",_templatesSettings.Color2));
                sb.AppendLine(SalesAgreementDetails.CheckoutAttributeInfo);
                sb.AppendLine(string.Format("</td> <td colspan=\"1\" style=\"background-color: {0};text-align:right\">",_templatesSettings.Color2));
                sb.AppendLine("</td></tr>");
            }

            #endregion

            #region Totals

            //subtotal
            sb.AppendLine(string.Format("<tr style=\"text-align:right;background-color: {0};\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color2, _localizationService.GetResource("Messages.Order.SubTotal", languageId), SalesAgreementDetails.OrderTotal.SubTotal));

            //discount (applied to order subtotal)
            if (!String.IsNullOrEmpty(SalesAgreementDetails.OrderTotal.SubTotalDiscount))
            {
                sb.AppendLine(string.Format("<tr style=\"text-align:right;background-color: {0};\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color2, _localizationService.GetResource("Messages.Order.SubTotalDiscount", languageId), SalesAgreementDetails.OrderTotal.SubTotalDiscount));
            }

            //Gift Wrapping or Checkout attribute values
            //if (!String.IsNullOrEmpty(SalesAgreementDetails.OrderTotal.cavalue))
            //{
            //    sb.AppendLine(string.Format("<tr style=\"text-align:right;background-color: {0};\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, _localizationService.GetResource("Order.Cavalue", languageId), SalesAgreementDetails.OrderTotal.cavalue));
            //}

            //shipping
            if (!String.IsNullOrWhiteSpace(SalesAgreementDetails.OrderTotal.Shipping))
            {
                sb.AppendLine(string.Format("<tr style=\"text-align:right;background-color: {0};\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color2, _localizationService.GetResource("Messages.Order.Shipping", languageId), SalesAgreementDetails.OrderTotal.Shipping));
            }

            //payment method fee
            if (!String.IsNullOrEmpty(SalesAgreementDetails.OrderTotal.PaymentMethodAdditionalFee))
            {
                string paymentMethodFeeTitle = _localizationService.GetResource("Messages.Order.PaymentMethodAdditionalFee", languageId);
                sb.AppendLine(string.Format("<tr style=\"text-align:right;background-color: {0};\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color2, paymentMethodFeeTitle, SalesAgreementDetails.OrderTotal.PaymentMethodAdditionalFee));
            }

            //tax
            if (!String.IsNullOrEmpty(SalesAgreementDetails.OrderTotal.Tax))
            {
                sb.AppendLine(string.Format("<tr style=\"text-align:right;background-color: {0};\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color2, _localizationService.GetResource("Messages.Order.Tax", languageId), SalesAgreementDetails.OrderTotal.Tax));
            }

            //discount
            if (!String.IsNullOrEmpty(SalesAgreementDetails.OrderTotal.OrderTotalDiscount))
            {
                sb.AppendLine(string.Format("<tr style=\"text-align:right;background-color: {0};\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color2, _localizationService.GetResource("Messages.Order.TotalDiscount", languageId), SalesAgreementDetails.OrderTotal.OrderTotalDiscount));
            }

            //gift cards
            var gcuhC = SalesAgreementDetails.OrderTotal.GiftCards;
            foreach (var gcuh in gcuhC)
            {
                string giftCardText = String.Format(_localizationService.GetResource("Messages.Order.GiftCardInfo", languageId), HttpUtility.HtmlEncode(gcuh.CouponCode));
                string giftCardAmount = gcuh.Amount;
                sb.AppendLine(string.Format("<tr style=\"text-align:right;background-color: {0};\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color2, giftCardText, giftCardAmount));
            }

            //reward points
            if (SalesAgreementDetails.OrderTotal.RedeemedRewardPointsAmount != null)
            {
                string rpTitle = string.Format(_localizationService.GetResource("Messages.Order.RewardPoints", languageId), -SalesAgreementDetails.OrderTotal.RedeemedRewardPoints);
                string rpAmount = SalesAgreementDetails.OrderTotal.RedeemedRewardPointsAmount;
                sb.AppendLine(string.Format("<tr style=\"text-align:right;background-color: {0};\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color2, rpTitle, rpAmount));
            }

            //total
            sb.AppendLine(string.Format("<tr style=\"text-align:right;background-color: {0};\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color2, _localizationService.GetResource("Messages.Order.OrderTotal", languageId), SalesAgreementDetails.OrderTotal.OrderTotal));
            #endregion

            sb.AppendLine("</table>");
            result = sb.ToString();
            return result;
        }

        public virtual void AddOrderTokensForSalesAgreementsPdf(IList<Token> tokens, Order order, int languageId)
        {
            tokens.Add(new Token("Order.OrderNumber", order.Id.ToString()));

            tokens.Add(new Token("Order.CustomerFullName", string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName)));
            tokens.Add(new Token("Order.CustomerEmail", order.BillingAddress.Email));


            tokens.Add(new Token("Order.BillingFirstName", order.BillingAddress.FirstName));
            tokens.Add(new Token("Order.BillingLastName", order.BillingAddress.LastName));
            tokens.Add(new Token("Order.BillingPhoneNumber", order.BillingAddress.PhoneNumber));
            tokens.Add(new Token("Order.BillingEmail", order.BillingAddress.Email));
            tokens.Add(new Token("Order.BillingFaxNumber", order.BillingAddress.FaxNumber));
            tokens.Add(new Token("Order.BillingCompany", order.BillingAddress.Company));
            tokens.Add(new Token("Order.BillingAddress1", order.BillingAddress.Address1));
            tokens.Add(new Token("Order.BillingAddress2", order.BillingAddress.Address2));
            tokens.Add(new Token("Order.BillingCity", order.BillingAddress.City));
            tokens.Add(new Token("Order.BillingStateProvince", order.BillingAddress.StateProvince != null ? order.BillingAddress.StateProvince.GetLocalized(x => x.Name) : ""));
            tokens.Add(new Token("Order.BillingZipPostalCode", order.BillingAddress.ZipPostalCode));
            tokens.Add(new Token("Order.BillingCountry", order.BillingAddress.Country != null ? order.BillingAddress.Country.GetLocalized(x => x.Name) : ""));

            tokens.Add(new Token("Order.ShippingMethod", order.ShippingMethod));
            tokens.Add(new Token("Order.ShippingFirstName", order.ShippingAddress != null ? order.ShippingAddress.FirstName : ""));
            tokens.Add(new Token("Order.ShippingLastName", order.ShippingAddress != null ? order.ShippingAddress.LastName : ""));
            tokens.Add(new Token("Order.ShippingPhoneNumber", order.ShippingAddress != null ? order.ShippingAddress.PhoneNumber : ""));
            tokens.Add(new Token("Order.ShippingEmail", order.ShippingAddress != null ? order.ShippingAddress.Email : ""));
            tokens.Add(new Token("Order.ShippingFaxNumber", order.ShippingAddress != null ? order.ShippingAddress.FaxNumber : ""));
            tokens.Add(new Token("Order.ShippingCompany", order.ShippingAddress != null ? order.ShippingAddress.Company : ""));
            tokens.Add(new Token("Order.ShippingAddress1", order.ShippingAddress != null ? order.ShippingAddress.Address1 : ""));
            tokens.Add(new Token("Order.ShippingAddress2", order.ShippingAddress != null ? order.ShippingAddress.Address2 : ""));
            tokens.Add(new Token("Order.ShippingCity", order.ShippingAddress != null ? order.ShippingAddress.City : ""));
            tokens.Add(new Token("Order.ShippingStateProvince", order.ShippingAddress != null && order.ShippingAddress.StateProvince != null ? order.ShippingAddress.StateProvince.GetLocalized(x => x.Name) : ""));
            tokens.Add(new Token("Order.ShippingZipPostalCode", order.ShippingAddress != null ? order.ShippingAddress.ZipPostalCode : ""));
            tokens.Add(new Token("Order.ShippingCountry", order.ShippingAddress != null && order.ShippingAddress.Country != null ? order.ShippingAddress.Country.GetLocalized(x => x.Name) : ""));

            //var paymentMethod =   _paymentService.LoadPaymentMethodBySystemName(order.PaymentMethodSystemName);
            //var paymentMethodName = paymentMethod != null ? paymentMethod.GetLocalizedFriendlyName(_localizationService, _workContext.WorkingLanguage.Id) : order.PaymentMethodSystemName;
            string paymentMethodName = null;
            if (order.PaymentMethodSystemName == "Payments.PurchaseOrder")
                paymentMethodName = _localizationService.GetResource("Checkout.Confirm.Payment.PurchaseOrderByWireTransfer").ToString();
            else
                paymentMethodName = _localizationService.GetResource("Payment.PaymentMethod.CC").ToString();

            tokens.Add(new Token("Order.PaymentMethod", paymentMethodName));
            tokens.Add(new Token("Order.VatNumber", order.VatNumber));

            tokens.Add(new Token("Order.Product(s)", ProductListToHtmlTable(order, languageId), true));

            var language = _languageService.GetLanguageById(languageId);
            if (language != null && !String.IsNullOrEmpty(language.LanguageCulture))
            {
                DateTime createdOn = _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, TimeZoneInfo.Utc, _dateTimeHelper.GetCustomerTimeZone(order.Customer));
                tokens.Add(new Token("Order.CreatedOn", createdOn.ToString("D", new CultureInfo(language.LanguageCulture))));
            }
            else
            {
                tokens.Add(new Token("Order.CreatedOn", order.CreatedOnUtc.ToString("D")));
            }

            //TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
            tokens.Add(new Token("Order.OrderURLForCustomer", string.Format("{0}orderdetails/{1}", _webHelper.GetStoreLocation(false), order.Id), true));

            // Custome toekn
            #region 

            var caValues = _checkoutAttributeParser.ParseCheckoutAttributeValues(order.CheckoutAttributesXml);
            decimal cavalues = new decimal();
            if (caValues != null)
            {
                foreach (var caValue in caValues)
                {
                    cavalues += _taxService.GetCheckoutAttributePrice(caValue);
                }
                if (cavalues > 0)
                {
                    decimal ca = _currencyService.ConvertFromPrimaryStoreCurrency(cavalues, _workContext.WorkingCurrency);
                    string cavalue = _priceFormatter.FormatPrice((order.OrderSubtotalExclTax - ca));
                    tokens.Add(new Token("Order.Cavalue", cavalue));
                }
                else
                {
                    string OrderSubtotal = _priceFormatter.FormatPrice(order.OrderSubtotalExclTax);
                    tokens.Add(new Token("Order.OrderSubTotalFinal", OrderSubtotal));
                }
            }
            
            string cusShipTotal = string.Empty;
            switch (order.CustomerTaxDisplayType)
            {
                    
                case TaxDisplayType.ExcludingTax:
                    {
                        
                        //shipping
                        var orderShippingExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingExclTax, order.CurrencyRate);
                        cusShipTotal = _priceFormatter.FormatShippingPrice(orderShippingExclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, language, false);
                    }
                    break;
                case TaxDisplayType.IncludingTax:
                    {
                        
                        //shipping
                        var orderShippingInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingInclTax, order.CurrencyRate);
                        cusShipTotal = _priceFormatter.FormatShippingPrice(orderShippingInclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, language, true);
                    }
                    break;
            }
            tokens.Add(new Token("Messages.Order.Shipping", cusShipTotal));
            tokens.Add(new Token("Order.Guid",order.OrderGuid.ToString()));
            
            #endregion
            //event notification
            _eventPublisher.EntityTokensAdded(order, tokens);
        }

        public virtual void AddCustomerTokensForSalesAgreementsPdf(IList<Token> tokens, Customer customer)
        {
            tokens.Add(new Token("Customer.Email", customer.Email));
            tokens.Add(new Token("Customer.Username", customer.Username));
            tokens.Add(new Token("Customer.FullName", customer.GetFullName()));
            tokens.Add(new Token("Customer.VatNumber", customer.VatNumber));
            tokens.Add(new Token("Customer.VatNumberStatus", customer.VatNumberStatusId.ToString()));



            //note: we do not use SEO friendly URLS because we can get errors caused by having .(dot) in the URL (from the email address)
            //TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
            string passwordRecoveryUrl = string.Format("{0}passwordrecovery/confirm?token={1}&email={2}", _webHelper.GetStoreLocation(false), customer.GetAttribute<string>(SystemCustomerAttributeNames.PasswordRecoveryToken), HttpUtility.UrlEncode(customer.Email));
            string accountActivationUrl = string.Format("{0}customer/activation?token={1}&email={2}", _webHelper.GetStoreLocation(false), customer.GetAttribute<string>(SystemCustomerAttributeNames.AccountActivationToken), HttpUtility.UrlEncode(customer.Email));
            var wishlistUrl = string.Format("{0}wishlist/{1}", _webHelper.GetStoreLocation(false), customer.CustomerGuid);
            tokens.Add(new Token("Customer.PasswordRecoveryURL", passwordRecoveryUrl, true));
            tokens.Add(new Token("Customer.AccountActivationURL", accountActivationUrl, true));
            tokens.Add(new Token("Wishlist.URLForCustomer", wishlistUrl, true));

            //event notification
            _eventPublisher.EntityTokensAdded(customer, tokens);
        }

        #endregion

    }
}
