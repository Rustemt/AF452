using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
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
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Forums;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Seo;
using Nop.Services.Media;
using Nop.Core.Domain.Media;
using System.Web.Routing;


using System.Web.UI;
using System.Web.Caching;
using Nop.Services.Configuration;
using Nop.Services.Orders;
using Nop.Services.Tax;
using System.Text.RegularExpressions;
namespace Nop.Services.Messages
{
    public partial class MessageTokenProvider : IMessageTokenProvider
    {
        #region Fields
        private readonly IPictureService _pictureService;
        private readonly IProductAttributeFormatter _productAttributeFormatterService;
        private readonly MediaSettings _mediaSettings;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly ICheckoutAttributeFormatter _checkoutAttributeFormatter;
        private readonly ICheckoutAttributeService _checkoutAttributeService;
        private readonly ITaxService _taxService;
        #endregion

        #region Ctor

        public MessageTokenProvider(ILanguageService languageService,
           ILocalizationService localizationService, IDateTimeHelper dateTimeHelper,
           IEmailAccountService emailAccountService,
           IPriceFormatter priceFormatter, ICurrencyService currencyService, IWebHelper webHelper,
           IWorkContext workContext, ITaxService taxService,
           StoreInformationSettings storeSettings, MessageTemplatesSettings templatesSettings,
           EmailAccountSettings emailAccountSettings, CatalogSettings catalogSettings,
           TaxSettings taxSettings, IProductAttributeFormatter productAttributeFormatter, IPictureService pictureService, MediaSettings mediaSettings, ICheckoutAttributeParser checkoutAttributeParser, ICheckoutAttributeFormatter checkoutAttributeFormatter, ICheckoutAttributeService checkoutAttributeService)
        {
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._dateTimeHelper = dateTimeHelper;
            this._emailAccountService = emailAccountService;
            this._priceFormatter = priceFormatter;
            this._currencyService = currencyService;
            this._webHelper = webHelper;
            this._workContext = workContext;
            this._checkoutAttributeService = checkoutAttributeService;
            this._storeSettings = storeSettings;
            this._templatesSettings = templatesSettings;
            this._emailAccountSettings = emailAccountSettings;
            this._catalogSettings = catalogSettings;
            this._taxSettings = taxSettings;
            this._checkoutAttributeParser = checkoutAttributeParser;
            this._checkoutAttributeFormatter = checkoutAttributeFormatter;
            this._pictureService = pictureService;
            this._mediaSettings = mediaSettings;
            this._productAttributeFormatterService = productAttributeFormatter;
            this._taxService = taxService;

        }

        #endregion

        #region Utilities

        /// <returns>HTML table of products</returns>
        protected virtual string WishListToHtmlTableTemplate(IList<ShoppingCartItem> wishListItems)
        {


            StringBuilder wishListTemplate = new StringBuilder("");
            wishListTemplate.AppendLine(" <table cellpadding=\"0\" cellspacing=\"0\"><tr><td style=\"border: 1px solid Silver;\">");
            wishListTemplate.AppendLine("<div style=\"background: #f3f3f3; padding: 30px 22px 20px 22px; margin: 0;font-size:0px; width: 571px;\">");
            int i = 0;
            foreach (ShoppingCartItem item in wishListItems)
            {

                var wpv = item.ProductVariant;
                string productName = string.IsNullOrWhiteSpace(wpv.GetLocalized(x => x.Name)) ? wpv.Product.GetLocalized(x => x.Name) : wpv.GetLocalized(x => x.Name);
                string pictureUrl = _pictureService.GetPictureUrl(wpv.GetDefaultProductVariantPicture(_pictureService).Id, _mediaSettings.ProductThumbPictureSize, true);
                wishListTemplate.AppendLine("<div style=\"display: inline-block; padding: 0px; margin: 0px; border: 0;\">");
                wishListTemplate.AppendLine("<table cellpadding=\"0\" cellspacing=\"0\" style=\"padding: 0px; margin: 0px; border: 0;border: 0;\"><tr style=\"padding: 0px; margin: 0px; border: 0;\">");
                wishListTemplate.AppendLine("<td style=\"padding: 0px; margin: 0px; border: 0;\">");
                wishListTemplate.AppendLine("<a style=\"color: #333333;\" href=\"" + _webHelper.GetAbsolutePath("/p/" + wpv.ProductId.ToString() + "/" + wpv.Product.SeName + "") + "\"><img border=\"0\"  width=\"187\" alt=\"Always Fashion\" style=\"display: block\" src=\"" + pictureUrl + "\"></a>");
                wishListTemplate.AppendLine("</td></tr><tr style=\"padding: 0px; margin: 0px; border: 0;\"><td align=\"center\" width=\"188\" height=\"62\" bgcolor=\"#f3f3f3\" style=\"font-size: 10px;color: #161616; font-family: 'Lucida Sans Unicode', 'Lucida Grande', sans-serif;padding: 0px; margin: 0px; border: 0;\">");
                if (wpv.Product.GetDefaultManufacturer() != null)
                wishListTemplate.AppendLine(wpv.Product.GetDefaultManufacturer().GetLocalized(x => x.Name));
                wishListTemplate.AppendLine("<br><span style=\"color: #666666\">" + productName + "</span><br>");
                wishListTemplate.AppendLine("<a style=\"color: #333333;\" href=\"" + _webHelper.GetAbsolutePath("/p/" + wpv.ProductId.ToString() + "/" + wpv.Product.SeName + "") + "\">" + _localizationService.GetResource("Messages.WishList.SeeMore") + "</a>");
                wishListTemplate.AppendLine("</td></tr></table></div>");
                i++;
                //after every three items new line
                if (i == 3)
                {
                    wishListTemplate.AppendLine("<br/><br/><br/>");
                    i = 0;
                }
            }
            wishListTemplate.AppendLine("</div></td></tr></table>");

            return wishListTemplate.ToString();
        }

        private string FormatCheckoutAttributes(string format)
        {
            Customer customer = _workContext.CurrentCustomer;
            string attributes = customer.CheckoutAttributes;
            var result = "";

            var caCollection = _checkoutAttributeParser.ParseCheckoutAttributes(attributes);
            for (int i = 0; i < caCollection.Count; i++)
            {
                var ca = caCollection[i];
                if (ca.CheckoutAttributeValues.FirstOrDefault(x => x.PriceAdjustment > 0) == null) continue;
                var valuesStr = _checkoutAttributeParser.ParseValues(attributes, ca.Id);
                for (int j = 0; j < valuesStr.Count; j++)
                {
                    string valueStr = valuesStr[j];
                    string caAttribute = string.Empty;
                    int caId = 0;
                    if (int.TryParse(valueStr, out caId))
                    {
                        var caValue = _checkoutAttributeService.GetCheckoutAttributeValueById(caId);
                        if (caValue != null && caValue.PriceAdjustment > 0)
                        {
                            decimal priceAdjustmentBase = _taxService.GetCheckoutAttributePrice(caValue, customer);
                            decimal priceAdjustment = _currencyService.ConvertFromPrimaryStoreCurrency(priceAdjustmentBase, _workContext.WorkingCurrency);
                            string priceAdjustmentStr = "";
                            if (priceAdjustmentBase > 0)
                            {
                                priceAdjustmentStr = _priceFormatter.FormatPrice(priceAdjustment);
                            }

                            caAttribute = string.Format(format,
                                ca.GetLocalized(a => a.TextPrompt, _workContext.WorkingLanguage.Id).ToUpper()+":", priceAdjustmentStr);

                            result += caAttribute;

                        }
                    }
                }
            }

            return result;
        }
      


        #endregion

        #region Methods

        public virtual void AddWishListTokens(IList<Token> tokens, IList<ShoppingCartItem> wishListItems)
        {
            tokens.Add(new Token("WishList.Product(s)", WishListToHtmlTableTemplate(wishListItems), true));
        }

        public virtual void AddVariantTokens(IList<Token> tokens, ProductVariant productVariant)
        {

            tokens.Add(new Token("Product.Name",string.IsNullOrWhiteSpace(productVariant.Name) ? productVariant.Product.Name : productVariant.Name));
            tokens.Add(new Token("Product.ShortDescription", productVariant.Product.ShortDescription, true));
            string pictureUrl = _pictureService.GetPictureUrl(productVariant.GetDefaultProductVariantPicture(_pictureService).Id, _mediaSettings.ProductThumbPictureSize, true);
            tokens.Add(new Token("Product.PictureUrl", pictureUrl, true));
            //TODO add a method for getting URL
            //var productUrl = string.Format("{0}p/{1}/{2}", _webHelper.GetStoreLocation(false), productVariant.Id, productVariant.Product.GetSeName());
            var url = new UrlHelper(HttpContext.Current.Request.RequestContext);
            var productUrl = String.Format("{0}{1}", _webHelper.GetStoreLocation(false).Substring(0, _webHelper.GetStoreLocation(false).LastIndexOf("/")), url.RouteUrl("Product", new { productId = productVariant.Product.Id, SeName = productVariant.Product.SeName, variantId = productVariant.Id }));
            tokens.Add(new Token("Product.ProductURLForCustomer", productUrl, true));
        }

        protected virtual string ProductListToHtmlTableTemplate(Order order, int languageId)
        {
            string result = string.Empty;

            var language = _languageService.GetLanguageById(languageId);

            var sb = new StringBuilder();
             
            sb.AppendLine("<tr><td><img src=\"" + _webHelper.GetAbsolutePath("/_img/mailing_39.jpg") + "\" style=\"display: block;\" alt=\"Always Fashion\" border=\"0\"></td></tr><tr><td  width=\"620\"><table style=\"border-left: 1px solid rgb(204, 204, 204); border-collapse: collapse; border-right: 1px solid rgb(204, 204, 204);\" bgcolor=\"#f3f3f3\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" height=\"auto\" width=\"620\"><tbody><tr><td height=\"36\" width=\"18\"></td>");
            // height=\"0\"
            #region Products

            sb.AppendLine(string.Format("<td style=\"font-size: 11px; color: rgb(102, 102, 102); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif; border-bottom: 1px solid rgb(219, 219, 219);\" height=\"36\" width=\"112\">{0}</td>", string.Format(_localizationService.GetResource("Messages.Order.Product(s).SKU", languageId), "")));
            sb.AppendLine(string.Format("<td style=\"font-size: 11px; color: rgb(102, 102, 102); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif; border-bottom: 1px solid rgb(219, 219, 219);\" align=\"center\" height=\"36\" width=\"120\">{0}</td>", _localizationService.GetResource("Messages.Order.Product(s).Description", languageId)));
            sb.AppendLine(string.Format("<td style=\"font-size: 11px; color: rgb(102, 102, 102); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif; border-bottom: 1px solid rgb(219, 219, 219);\" height=\"36\" width=\"138\">{0}</td>", ""));
            sb.AppendLine(string.Format("<td style=\"font-size: 11px; color: rgb(102, 102, 102); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif; border-bottom: 1px solid rgb(219, 219, 219);\" height=\"36\" width=\"54\">{0}</td>", _localizationService.GetResource("Messages.Order.Product(s).Quantity", languageId)));
            sb.AppendLine(string.Format("<td style=\"font-size: 11px; color: rgb(102, 102, 102); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif; border-bottom: 1px solid rgb(219, 219, 219);\" height=\"36\" width=\"85\">{0}</td>", _localizationService.GetResource("Messages.Order.Product(s).Price", languageId)));
            sb.AppendLine(string.Format("<td style=\"font-size: 11px; color: rgb(102, 102, 102); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif; border-bottom: 1px solid rgb(219, 219, 219);\" height=\"36\" width=\"75\">{0}</td>", _localizationService.GetResource("Messages.Order.Product(s).Status", languageId)));
            sb.AppendLine("<td height=\"36\" width=\"18\"></td>");
            sb.AppendLine("</tr>");

            var table = order.OrderProductVariants.ToList();
            for (int i = 0; i <= table.Count - 1; i++)
            {
                var opv = table[i];
                var productVariant = opv.ProductVariant;
                if (productVariant == null)
                    continue;
                //sku
                if (_catalogSettings.ShowProductSku)
                {
                    if (!String.IsNullOrEmpty(opv.ProductVariant.Sku))
                    {

                        string sku = string.Format(_localizationService.GetResource("Messages.Order.Product(s).SKU", languageId), HttpUtility.HtmlEncode(opv.ProductVariant.Sku));
                        sb.AppendLine(" <tr><td height=\"164\" width=\"18\"></td>");
                        sb.AppendLine(string.Format("<td style=\"font-size: 11px; border-bottom: 1px solid rgb(219, 219, 219); color: rgb(51, 51, 51); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif;\" height=\"164\"  width=\"112\"> {0} </td>", sku));

                    }
                    else
                    {   //if sku is not given, build html 

                        sb.AppendLine(" <tr><td height=\"164\" width=\"18\"></td>");
                        sb.AppendLine(string.Format("<td style=\"font-size: 11px; border-bottom: 1px solid rgb(219, 219, 219); color: rgb(51, 51, 51); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif;\" height=\"164\"  width=\"112\"> {0} </td>", string.Format(_localizationService.GetResource("Messages.Order.Product(s).SKU", languageId), "")));

                    }
                }
                else
                {   //if sku is not given, build html

                    sb.AppendLine(" <tr><td height=\"164\" width=\"18\"></td>");
                    sb.AppendLine(string.Format("<td style=\"font-size: 11px; border-bottom: 1px solid rgb(219, 219, 219); color: rgb(51, 51, 51); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif;\" height=\"164\"  width=\"112\"> {0} </td>", string.Format(_localizationService.GetResource("Messages.Order.Product(s).SKU", languageId), "")));
                }

                string status = "";
                ProductSpecificationAttribute attr = productVariant.Product.ProductSpecificationAttributes.Where(x => x.SpecificationAttributeOption.SpecificationAttribute.Name == "Kargo").FirstOrDefault();

                if (attr != null)
                    status = attr.SpecificationAttributeOption.GetLocalized(x => x.Name);
                else
                    status = "";

                StringBuilder sbProductNameAndMark = new StringBuilder();
                string pictureUrl = _pictureService.GetPictureUrl(opv.ProductVariant.GetDefaultProductVariantPicture(_pictureService), _mediaSettings.ProductVariantPictureSize, true);
                string productName = string.IsNullOrWhiteSpace(opv.ProductVariant.GetLocalized(x => x.Name)) ? opv.ProductVariant.Product.GetLocalized(x => x.Name) : opv.ProductVariant.GetLocalized(x => x.Name);
                string manufacturerName = opv.ProductVariant.Product.ProductManufacturers.Count > 0 ? opv.ProductVariant.Product.ProductManufacturers.FirstOrDefault().Manufacturer.GetLocalized(x => x.Name) : "";
               // var formattedProductPrice = _priceFormatter.FormatPrice(order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax ? opv.PriceInclTax : opv.PriceExclTax, false, order.CustomerCurrencyCode, language, false);
                string unitPriceStr = string.Empty;
                switch (order.CustomerTaxDisplayType)
                {
                    //case TaxDisplayType.ExcludingTax:
                    //    {
                    //        var opvUnitPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(opv.UnitPriceExclTax, order.CurrencyRate);
                    //        unitPriceStr = _priceFormatter.FormatPrice(opvUnitPriceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, false);
                    //    }
                    //    break;
                    case TaxDisplayType.IncludingTax:
                    case TaxDisplayType.ExcludingTax:
                        {
                            var opvUnitPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(opv.UnitPriceInclTax, order.CurrencyRate);
                            unitPriceStr = _priceFormatter.FormatPrice(opvUnitPriceInclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, language, true);
                        }
                        break;
                }

                sbProductNameAndMark.AppendLine(string.Format("<h1 style=\"font-size: 11px; color: rgb(51, 51, 51); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif; margin: 1px 0pt; padding: 0pt;\">{0}</h1>", HttpUtility.HtmlEncode(manufacturerName)));
                sbProductNameAndMark.AppendLine(string.Format("<p style=\"font-size: 11px; color: rgb(51, 51, 51); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif; margin: 1px 0pt; padding: 0pt;\">{0}</p>", HttpUtility.HtmlEncode(productName)));
                sbProductNameAndMark.AppendLine(string.Format("<p style=\"font-size: 11px; color: rgb(51, 51, 51); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif; margin: 1px 0pt; padding: 0pt;\">{0}</p>", _productAttributeFormatterService.FormatAttributes(opv.ProductVariant, opv.AttributesXml, "<span>{0}</span> :{1}", order.Customer, htmlEncode: false)));
                //sb.AppendLine(string.Format("<td style=\"font-size: 11px; border-bottom: 1px solid rgb(219, 219, 219); color: rgb(51, 51, 51);font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif;\" height=\"164\" width=\"112\"><img src=" + "{0}" + "/></td>", pictureUrl));
                sb.AppendLine(string.Format("<td style=\"font-size: 11px; border-bottom: 1px solid rgb(219, 219, 219); color: rgb(51, 51, 51);font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif;\" height=\"164\" width=\"112\"><img src=\"{0}\"/></td>", HttpUtility.HtmlEncode(pictureUrl)));
                sb.AppendLine(string.Format("<td style=\"font-size: 11px; border-bottom: 1px solid rgb(219, 219, 219); color: rgb(51, 51, 51);font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif;\" height=\"164\" width=\"112\">{0}</td>", sbProductNameAndMark.ToString()));
                sb.AppendLine(string.Format("<td style=\"font-size: 11px; border-bottom: 1px solid rgb(219, 219, 219); color: rgb(51, 51, 51);font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif;\" height=\"164\" width=\"112\">{0}</td>", opv.Quantity.ToString()));
                sb.AppendLine(string.Format("<td style=\"font-size: 11px; border-bottom: 1px solid rgb(219, 219, 219); color: rgb(51, 51, 51);font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif;\" height=\"164\" width=\"112\">{0}</td>", unitPriceStr));
                sb.AppendLine(string.Format("<td style=\"font-size: 11px; border-bottom: 1px solid rgb(219, 219, 219); color: rgb(51, 51, 51);font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif;\" height=\"164\" width=\"112\">{0}</td>", status));
                sb.AppendLine("<td height=\"164\" width=\"18\"></td></tr>");
            }
            sb.AppendLine("</tbody></table></td></tr>");


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
                case TaxDisplayType.IncludingTax:
                    {
                        //subtotal
                        var orderSubtotalInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubtotalInclTax, order.CurrencyRate);
                        cusSubTotal = _priceFormatter.FormatPrice(orderSubtotalInclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, language, false);
                        //discount (applied to order subtotal)
                        var orderSubTotalDiscountInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubTotalDiscountInclTax, order.CurrencyRate);
                        if (orderSubTotalDiscountInclTaxInCustomerCurrency > decimal.Zero)
                        {
                            cusSubTotalDiscount = _priceFormatter.FormatPrice(-orderSubTotalDiscountInclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, language, false);
                            dislaySubTotalDiscount = true;
                        }
                        //shipping
                        var orderShippingInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingInclTax, order.CurrencyRate);
                        cusShipTotal = _priceFormatter.FormatShippingPrice(orderShippingInclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, language, false);
                        //payment method additional fee
                        var paymentMethodAdditionalFeeInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeInclTax, order.CurrencyRate);
                        cusPaymentMethodAdditionalFee = _priceFormatter.FormatPaymentMethodAdditionalFee(paymentMethodAdditionalFeeInclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, language, false);
                    }
                    break;
                //case TaxDisplayType.IncludingTax:
                //    {
                //        //subtotal
                //        var orderSubtotalInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubtotalInclTax, order.CurrencyRate);
                //        cusSubTotal = _priceFormatter.FormatPrice(orderSubtotalInclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, language, true);
                //        //discount (applied to order subtotal)
                //        var orderSubTotalDiscountInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubTotalDiscountInclTax, order.CurrencyRate);
                //        if (orderSubTotalDiscountInclTaxInCustomerCurrency > decimal.Zero)
                //        {
                //            cusSubTotalDiscount = _priceFormatter.FormatPrice(-orderSubTotalDiscountInclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, language, true);
                //            dislaySubTotalDiscount = true;
                //        }
                //        //shipping
                //        var orderShippingInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingInclTax, order.CurrencyRate);
                //        cusShipTotal = _priceFormatter.FormatShippingPrice(orderShippingInclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, language, true);
                //        //payment method additional fee
                //        var paymentMethodAdditionalFeeInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeInclTax, order.CurrencyRate);
                //        cusPaymentMethodAdditionalFee = _priceFormatter.FormatPaymentMethodAdditionalFee(paymentMethodAdditionalFeeInclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, language, true);
                //    }
                //    break;
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
                    string taxStr = _priceFormatter.FormatPrice(orderTaxInCustomerCurrency, false, order.CustomerCurrencyCode, language, false);
                    cusTaxTotal = taxStr;
                }
            }

            //discount
            bool dislayDiscount = false;
            if (order.OrderDiscount > decimal.Zero)
            {
                var orderDiscountInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderDiscount, order.CurrencyRate);
                cusDiscount = _priceFormatter.FormatPrice(-orderDiscountInCustomerCurrency, false, order.CustomerCurrencyCode, language, false);
                cusSubTotalDiscount = cusDiscount;
                dislayDiscount = true;
            }

           

            //total
            var orderTotalInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
            cusTotal = _priceFormatter.FormatPrice(orderTotalInCustomerCurrency, false, order.CustomerCurrencyCode, language, false);
            sb.AppendLine("<tr><td> <img src=\"" + _webHelper.GetAbsolutePath("/_img/mailing_41.jpg") + "\" style=\"display: block;\" alt=\"Always Fashion\" border=\"0\"></td></tr>");
            sb.AppendLine("<tr><td  height=\"151\" width=\"620\"><table style=\"border-left: 1px solid rgb(204, 204, 204); border-collapse: collapse; border-right: 1px solid rgb(204, 204, 204);                    border-bottom: 1px solid rgb(204, 204, 204);\" bgcolor=\"#f3f3f3\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" height=\"151\" width=\"620\"><tbody> <tr><td height=\"16\" width=\"502\"> </td> <td height=\"16\" width=\"100\"> </td> <td rowspan=\"6\" height=\"151\" width=\"18\"> </td> </tr> <tr>");//1.tr
            sb.AppendLine(string.Format("<td style=\"font-size: 10px; color: rgb(51, 51, 51); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif;\" align=\"right\" height=\"28\" width=\"450\">{0}</td>", _localizationService.GetResource("Messages.Order.SubTotal", languageId)));
            sb.AppendLine(string.Format(" <td style=\"font-size: 12px; color: rgb(0, 0, 0); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif;\"align=\"right\" height=\"28\" width=\"152\">{0}</td></tr>", cusSubTotal));//2.tr
            sb.AppendLine(string.Format("<tr><td style=\"font-size: 10px; color: rgb(51, 51, 51); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif;\" align=\"right\" height=\"28\" width=\"450\">{0}</td>", _localizationService.GetResource("Messages.Order.Shipping", languageId)));
            sb.AppendLine(string.Format(" <td style=\"font-size: 12px; color: rgb(0, 0, 0); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif;\"align=\"right\" height=\"28\" width=\"152\">{0}</td></tr>", cusShipTotal));//3.tr
            if (!string.IsNullOrWhiteSpace(cusTaxTotal) && !string.IsNullOrEmpty(cusTaxTotal))
            {
                sb.AppendLine(string.Format("<tr><td style=\"font-size: 10px; color: rgb(51, 51, 51); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif;\" align=\"right\" height=\"28\" width=\"450\">{0}</td>", _localizationService.GetResource("Messages.Order.Tax", languageId)));
                sb.AppendLine(string.Format(" <td style=\"font-size: 12px; color: rgb(0, 0, 0); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif;\"align=\"right\" height=\"28\" width=\"152\">{0}</td></tr>", cusTaxTotal));//4.tr
            }
            //gift cards
            var gcuhC = order.GiftCardUsageHistory;
            foreach (var gcuh in gcuhC)
            {
                string giftCardText = String.Format(_localizationService.GetResource("Messages.Order.GiftCardInfo", languageId), HttpUtility.HtmlEncode(gcuh.GiftCard.GiftCardCouponCode));
                string giftCardAmount = _priceFormatter.FormatPrice(-(_currencyService.ConvertCurrency(gcuh.UsedValue, order.CurrencyRate)), false, false);
                //sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, giftCardText, giftCardAmount));

                sb.AppendLine(string.Format("<tr><td style=\"font-size: 10px; color: rgb(51, 51, 51); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif;\" align=\"right\" height=\"28\" width=\"450\">{0}</td>",giftCardText));
                sb.AppendLine(string.Format( " <td style=\"font-size: 12px; color: rgb(0, 0, 0); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif;\"align=\"right\" height=\"28\" width=\"152\">{0}</td></tr>",giftCardAmount)); //4.tr
            }

            if (order.OrderDiscount > 0)
            {
                sb.AppendLine(string.Format("<tr><td style=\"font-size: 10px; color: rgb(51, 51, 51); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif;\" align=\"right\" height=\"28\" width=\"450\">{0}</td>", _localizationService.GetResource("Messages.Order.SubTotalDiscount", languageId)));
                sb.AppendLine(string.Format(" <td style=\"font-size: 12px; color: rgb(0, 0, 0); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif;\"align=\"right\" height=\"28\" width=\"152\">{0}</td></tr>", cusSubTotalDiscount));//tr
            }
            sb.AppendLine(string.Format("<tr><td style=\"font-size: 10px; color: rgb(51, 51, 51); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif;\" align=\"right\" height=\"23\" width=\"450\">{0}</td>", _localizationService.GetResource("Messages.Order.Total", languageId)));
            sb.AppendLine(string.Format("<td style=\"font-size: 12px; color: rgb(0, 0, 0); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif;\" align=\"right\" height=\"23\" width=\"152\">{0} </td></tr>", cusTotal));//6.tr
            //sb.AppendLine(string.Format("<tr><td style=\"font-size: 10px; color: rgb(51, 51, 51); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif;\" align=\"right\" height=\"25\" width=\"450\"> {0}: </td>",_localizationService.GetResource( "Messages.Order.Currency",languageId)));
            //sb.AppendLine(string.Format("<td style=\"font-size: 12px; color: rgb(0, 0, 0); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif;\" align=\"right\" height=\"25\" width=\"152\"> {0} </td> </tr>",order.CustomerCurrencyCode));
            //sb.AppendLine(string.Format("<tr><td style=\"font-size: 10px; color: rgb(51, 51, 51); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif;\"  align=\"right\" height=\"25\" width=\"450\"> {0}: </td>",_localizationService.GetResource( "Messages.Order.PaymentMethod",languageId)));
            //sb.AppendLine(string.Format("<td style=\"font-size: 12px; color: rgb(0, 0, 0); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif;\" align=\"right\" height=\"25\" width=\"152\"> {0} </td></tr>", order.PaymentMethodSystemName));
            sb.AppendLine(string.Format(" <tr><td style=\"font-size: 10px; color: rgb(51, 51, 51); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif;\"  align=\"right\" height=\"34\" width=\"450\"> {0} </td>", _localizationService.GetResource("Messages.Order.ShippingMethod", languageId)));
            sb.AppendLine(string.Format("<td style=\"font-size: 12px; color: rgb(0, 0, 0); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif;\" align=\"right\" height=\"34\" width=\"152\">{0}</td></tr>", order.ShippingMethod));
            sb.AppendLine("</tbody></table></td></tr>");
            //subtotal
            //sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, _localizationService.GetResource("Messages.Order.SubTotal", languageId), cusSubTotal));

            //discount (applied to order subtotal)
            if (dislaySubTotalDiscount)
            {
                //sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, _localizationService.GetResource("Messages.Order.SubTotalDiscount", languageId), cusSubTotalDiscount));
            }


            //shipping
            if (dislayShipping)
            {
                //sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, _localizationService.GetResource("Messages.Order.Shipping", languageId), cusShipTotal));
            }

            //payment method fee
            if (displayPaymentMethodFee)
            {
                string paymentMethodFeeTitle = _localizationService.GetResource("Messages.Order.PaymentMethodAdditionalFee", languageId);
                //sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, paymentMethodFeeTitle, cusPaymentMethodAdditionalFee));
            }

            //tax
            if (displayTax)
            {
                //sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, _localizationService.GetResource("Messages.Order.Tax", languageId), cusTaxTotal));
            }
            if (displayTaxRates)
            {
                foreach (var item in taxRates)
                {
                    string taxRate = String.Format(_localizationService.GetResource("Messages.Order.TaxRateLine"), _priceFormatter.FormatTaxRate(item.Key));
                    string taxValue = _priceFormatter.FormatPrice(item.Value, true, order.CustomerCurrencyCode, language, false);
                    //sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, taxRate, taxValue));
                }
            }

            //discount
            if (dislayDiscount)
            {
                //sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, _localizationService.GetResource("Messages.Order.TotalDiscount", languageId), cusDiscount));
            }

            

            //reward points
            if (order.RedeemedRewardPointsEntry != null)
            {
                string rpTitle = string.Format(_localizationService.GetResource("Messages.Order.RewardPoints", languageId), -order.RedeemedRewardPointsEntry.Points);
                string rpAmount = _priceFormatter.FormatPrice(-(_currencyService.ConvertCurrency(order.RedeemedRewardPointsEntry.UsedAmount, order.CurrencyRate)), true, order.CustomerCurrencyCode, false, language);
                //sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, rpTitle, rpAmount));
            }

            //total
            //sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, _localizationService.GetResource("Messages.Order.OrderTotal", languageId), cusTotal));
            #endregion

            //sb.AppendLine("</table>");
            result = sb.ToString();
            return result;
        }
        protected virtual string ProductListToHtmlTableTemplateSO(Order order, int languageId)
        {
            string result = string.Empty;

            var language = _languageService.GetLanguageById(languageId);

            var sb = new StringBuilder();


            #region Products
            sb.AppendLine("<table width=\"620\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\" bgcolor=\"#fffef7\" align=\"center\" style=\"border-left:1px solid #cccccc; border-right:1px solid #cccccc;\"><tbody>");
            sb.AppendLine(string.Format("<tr><td width=\"18\" height=\"36\"></td><td width=\"112\" height=\"36\" style=\"font-size: 11px; color: #666666; font-family: 'Arial', 'Arial', sans-serif; border-bottom: 1px solid #dbdbdb;\">{0}</td>", string.Format(_localizationService.GetResource("Messages.Order.Product(s).SKU", languageId), "")));
            sb.AppendLine(string.Format("<td width=\"120\" height=\"36\" align=\"center\" style=\"font-size: 11px; color: #666666; font-family: 'Arial', 'Arial', sans-serif; border-bottom: 1px solid #dbdbdb;\">{0}</td>", string.Format(_localizationService.GetResource("Messages.Order.Product(s).Description", languageId), "")));
            sb.AppendLine("<td width=\"138\" height=\"36\" style=\"font-size: 11px; color: #666666; font-family: 'Arial', 'Arial', sans-serif; border-bottom: 1px solid #dbdbdb;\"></td>");
            sb.AppendLine(string.Format("<td width=\"54\" height=\"36\" style=\"font-size: 11px; color: #666666; font-family: 'Arial', 'Arial', sans-serif; border-bottom: 1px solid #dbdbdb;\">{0}</td>", string.Format(_localizationService.GetResource("OrderDetails.Header.Quantity", languageId), "")));
            sb.AppendLine(string.Format("<td width=\"85\" height=\"36\" style=\"font-size: 11px; color: #666666; font-family: 'Arial', 'Arial', sans-serif; border-bottom: 1px solid #dbdbdb;\">{0}</td>", string.Format(_localizationService.GetResource("OrderDetails.Header.Price", languageId), "")));
            sb.AppendLine(string.Format("<td width=\"75\" height=\"36\" align=\"right\" style=\"font-size: 11px; color: #666666; font-family: 'Arial', 'Arial', sans-serif; border-bottom: 1px solid #dbdbdb;\">{0}</td>", string.Format(_localizationService.GetResource("OrderDetails.Header.Status", languageId), "")));
            sb.AppendLine("<td width=\"18\" height=\"36\"></td></tr>");

            var table = order.OrderProductVariants.ToList();
            for (int i = 0; i <= table.Count - 1; i++)
            {
                var opv = table[i];
                var productVariant = opv.ProductVariant;
                if (productVariant == null)
                    continue;
                sb.AppendLine(" <tr><td width=\"18\" height=\"164\"></td>");


                //sku
                if (_catalogSettings.ShowProductSku)
                {
                    if (!String.IsNullOrEmpty(opv.ProductVariant.Sku))
                    {
                        string sku = string.Format(_localizationService.GetResource("Messages.Order.Product(s).SKU", languageId), HttpUtility.HtmlEncode(opv.ProductVariant.Sku));
                        sb.AppendLine(string.Format("<td width=\"112\" height=\"164\" style=\"font-size: 11px; border-bottom: 1px solid #dbdbdb; color: #333333; font-family: 'Arial', 'Arial', sans-serif;\">{0}</td>", sku));
                    }
                    else
                    {   //if sku is not given, build html 

                        sb.AppendLine(string.Format("<td width=\"112\" height=\"164\" style=\"font-size: 11px; border-bottom: 1px solid #dbdbdb; color: #333333; font-family: 'Arial', 'Arial', sans-serif;\">{0}</td>", string.Format(_localizationService.GetResource("Messages.Order.Product(s).SKU", languageId), "")));

                    }
                }
                else
                {   //if sku is not given, build html

                    sb.AppendLine(string.Format("<td width=\"112\" height=\"164\" style=\"font-size: 11px; border-bottom: 1px solid #dbdbdb; color: #333333; font-family: 'Arial', 'Arial', sans-serif;\">{0}</td>", string.Format(_localizationService.GetResource("Messages.Order.Product(s).SKU", languageId), "")));
                }

                string status = "";
                ProductSpecificationAttribute attr = productVariant.Product.ProductSpecificationAttributes.Where(x => x.SpecificationAttributeOption.SpecificationAttribute.Name == "Kargo").FirstOrDefault();

                if (attr != null)
                    status = attr.SpecificationAttributeOption.GetLocalized(x => x.Name);
                else
                    status = "";


                StringBuilder sbProductNameAndMark = new StringBuilder();
                string pictureUrl = _pictureService.GetPictureUrl(opv.ProductVariant.GetDefaultProductVariantPicture(_pictureService), 123, true);
                string productName = string.IsNullOrWhiteSpace(opv.ProductVariant.GetLocalized(x => x.Name)) ? opv.ProductVariant.Product.GetLocalized(x => x.Name) : opv.ProductVariant.GetLocalized(x => x.Name);
                string manufacturerName = opv.ProductVariant.Product.ProductManufacturers.Count > 0 ? opv.ProductVariant.Product.ProductManufacturers.FirstOrDefault().Manufacturer.GetLocalized(x => x.Name) : "";
                // var formattedProductPrice = _priceFormatter.FormatPrice(order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax ? opv.PriceInclTax : opv.PriceExclTax, false, order.CustomerCurrencyCode, language, false);
                string unitPriceStr = string.Empty;
                switch (order.CustomerTaxDisplayType)
                {

                    case TaxDisplayType.IncludingTax:
                    case TaxDisplayType.ExcludingTax:
                        {
                            var opvUnitPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(opv.UnitPriceInclTax, order.CurrencyRate);
                            unitPriceStr = _priceFormatter.FormatPrice(opvUnitPriceInclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, language, true);
                        }
                        break;
                }

                sb.AppendLine(string.Format("<td width=\"120\" height=\"164\" align=\"center\" valign=\"middle\" style=\"border-bottom: 1px solid #dbdbdb;\"><img src=\"{0}\" border=\"0\" /></td>", HttpUtility.HtmlEncode(pictureUrl)));
                sb.AppendLine("<td width=\"138\" height=\"164\" style=\"font-size: 11px; color: #333333; font-family: 'Arial', 'Arial', sans-serif; border-bottom: 1px solid #dbdbdb;\">");
                sb.AppendLine(string.Format("<h1 style=\"font-size: 11px; color: #333333; font-family: 'Arial', 'Arial', sans-serif; margin: 1px 0; padding: 0;\">{0}</h1>", HttpUtility.HtmlEncode(productName)));
                sb.AppendLine(string.Format("<p style=\"font-size: 11px; color: rgb(51, 51, 51); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif; margin: 1px 0pt; padding: 0pt;\">{0}</p>", _productAttributeFormatterService.FormatAttributes(opv.ProductVariant, opv.AttributesXml, "<p style=\"font-size: 11px; color: #333333; font-family: 'Arial', 'Arial', sans-serif; margin: 1px 0; padding: 0;\">{1}</p> :", order.Customer, htmlEncode: false)));
                sb.AppendLine("</td>");
                sb.AppendLine(string.Format("<td width=\"54\" height=\"164\" style=\"font-size: 11px; color: #333333; font-family: 'Arial', 'Arial', sans-serif; border-bottom: 1px solid #dbdbdb;\">{0}</td>", opv.Quantity.ToString()));
                sb.AppendLine(string.Format("<td width=\"85\" height=\"164\" style=\"font-size: 11px; color: #333333; font-family: 'Arial', 'Arial', sans-serif; border-bottom: 1px solid #dbdbdb;\">{0}</td>", unitPriceStr));
                sb.AppendLine(string.Format("<td width=\"75\" height=\"164\" style=\"font-size: 11px; color: #333333; font-family: 'Arial', 'Arial', sans-serif; border-bottom: 1px solid #dbdbdb;\" align=\"right\"> {0}</td>", status));
                sb.AppendLine(" <td width=\"18\" height=\"164\"></td>");

                sb.AppendLine("</tr>");
            }
            sb.AppendLine("</tbody></table></td></tr>");
            sb.AppendLine("<tr><td><img src=\"" + _webHelper.GetAbsolutePath("/_img/mailing_41.jpg") + "\" border=\"0\" style=\"display:block\" alt=\"Sabri Özel\" /></td></tr>");

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
                case TaxDisplayType.IncludingTax:
                    {
                        //subtotal
                        var orderSubtotalExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubtotalExclTax, order.CurrencyRate);
                        cusSubTotal = _priceFormatter.FormatPrice(orderSubtotalExclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, language, false);
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
                    string taxStr = _priceFormatter.FormatPrice(orderTaxInCustomerCurrency, false, order.CustomerCurrencyCode, language, false);
                    cusTaxTotal = taxStr;
                }
            }
            //checkout attribute


            //discount
            bool dislayDiscount = false;
            if (order.OrderDiscount > decimal.Zero)
            {
                var orderDiscountInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderDiscount, order.CurrencyRate);
                cusDiscount = _priceFormatter.FormatPrice(-orderDiscountInCustomerCurrency, false, order.CustomerCurrencyCode, language, false);
                cusSubTotalDiscount = cusDiscount;
                dislayDiscount = true;
            }

            //total
            var orderTotalInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
            cusTotal = _priceFormatter.FormatPrice(orderTotalInCustomerCurrency, false, order.CustomerCurrencyCode, language, false);
            sb.AppendLine("<tr><td width=\"620\"><table width=\"620\" height=\"151\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" bgcolor=\"#fffef7\" style=\"border-left: 1px solid #cccccc; border-right: 1px solid #cccccc; border-bottom: 1px solid #cccccc;\"><tr><td width=\"502\" height=\"16\"></td><td width=\"100\" height=\"16\"></td><td rowspan=\"6\" width=\"18\" height=\"151\"></td></tr>");
            sb.AppendLine(string.Format("<tr><td width=\"502\" height=\"28\" align=\"right\" style=\"font-size: 10px; color: #333333; font-family: 'Arial', 'Arial', sans-serif;\">{0}</td><td width=\"100\" height=\"28\" align=\"right\" style=\"font-size: 12px; color: #000000; font-family: 'Arial', 'Arial', sans-serif;\">{1}</td></tr>", _localizationService.GetResource("Messages.Order.SubTotal", languageId), cusSubTotal));
            sb.AppendLine(string.Format("<tr><td width=\"502\" height=\"28\" align=\"right\" style=\"font-size: 10px; color: #333333; font-family: 'Arial', 'Arial', sans-serif;\">{0}</td><td width=\"100\" height=\"28\" align=\"right\" style=\"font-size: 12px; color: #000000; font-family: 'Arial', 'Arial', sans-serif;\">{1}</td></tr>", _localizationService.GetResource("Messages.Order.Shipping", languageId), cusShipTotal));
            if (!string.IsNullOrEmpty(cusSubTotalDiscount))
                sb.AppendLine(string.Format("<tr><td width=\"502\" height=\"28\" align=\"right\" style=\"font-size: 10px; color: #333333; font-family: 'Arial', 'Arial', sans-serif;\">{0}</td><td width=\"100\" height=\"28\" align=\"right\" style=\"font-size: 12px; color: #000000; font-family: 'Arial', 'Arial', sans-serif;\">{1}</td></tr>", _localizationService.GetResource("Messages.Order.SubTotalDiscount", languageId), cusSubTotalDiscount));
            sb.AppendLine(FormatCheckoutAttributes("<tr><td width=\"502\" height=\"28\" align=\"right\" style=\"font-size: 10px; color: #333333; font-family: 'Arial', 'Arial', sans-serif;\">{0}</td><td width=\"100\" height=\"28\" align=\"right\" style=\"font-size: 12px; color: #000000; font-family: 'Arial', 'Arial', sans-serif;\">{1}</td></tr>"));

            sb.AppendLine(string.Format("<tr><td width=\"502\" height=\"28\" align=\"right\" style=\"font-size: 10px; color: #333333; font-family: 'Arial', 'Arial', sans-serif;\">{0}</td><td width=\"100\" height=\"28\" align=\"right\" style=\"font-size: 12px; color: #000000; font-family: 'Arial', 'Arial', sans-serif;\">{1}</td></tr>", _localizationService.GetResource("Messages.Order.Tax", languageId), cusTaxTotal));
            sb.AppendLine(string.Format("<tr><td width=\"502\" height=\"28\" align=\"right\" style=\"font-size: 10px; color: #333333; font-family: 'Arial', 'Arial', sans-serif;\">{0}</td><td width=\"100\" height=\"28\" align=\"right\" style=\"font-size: 12px; color: #000000; font-family: 'Arial', 'Arial', sans-serif;\">{1}</td></tr>", _localizationService.GetResource("Messages.Order.Total", languageId), cusTotal));
            sb.AppendLine("</table>");

            //sb.AppendLine("<td colspan=\"5\" height=\"3\" style=\"font-size: 0;\"><img src=\"" + _webHelper.GetAbsolutePath("_img/content-separator.png") + "\" width=\"580\" height=\"3\" alt=\"\" border=\"0\" /></td></tr>");

            //sb.AppendLine(string.Format("<tr><td colspan=\"5\" style=\"padding: 20px 0;\"><table width=\"366\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\" align=\"right\"><tr><td width=\"197\" align=\"right\" style=\"font-size: 11px;\">{0}</td><td width=\"169\" align=\"right\" style=\"color: #000; font-size: 14px; padding: 4px 0;\">{1}</td></tr><tr>", _localizationService.GetResource("Messages.Order.SubTotal", languageId), cusSubTotal));
            //sb.AppendLine(string.Format("<td width=\"197\" align=\"right\" style=\"font-size: 11px;\">{0}</td><td width=\"169\" align=\"right\" style=\"color: #000; font-size: 14px; padding: 4px 0;\">{1}</td></tr><tr>", _localizationService.GetResource("Messages.Order.Shipping", languageId), cusShipTotal));
            ////ToDo: Check other discount types
            //if (!string.IsNullOrEmpty(cusSubTotalDiscount)) sb.AppendLine(string.Format("<td width=\"197\" align=\"right\" style=\"font-size: 11px;\">{0}</td><td width=\"169\" align=\"right\" style=\"color: #000; font-size: 14px; padding: 4px 0;\">{1}</td></tr><tr>", _localizationService.GetResource("Messages.Order.SubTotalDiscount", languageId), cusSubTotalDiscount));
            //sb.AppendLine(FormatCheckoutAttributes("<td width=\"197\" align=\"right\" style=\"font-size: 11px;\">{0}</td><td width=\"169\" align=\"right\" style=\"color: #000; font-size: 14px; padding: 4px 0;\">{1}</td></tr><tr>"));
            //sb.AppendLine(string.Format("<td width=\"197\" align=\"right\" style=\"font-size: 11px;\">{0}</td><td width=\"169\" align=\"right\" style=\"color: #000; font-size: 14px; padding: 4px 0;\">{1}</td></tr><tr>", _localizationService.GetResource("Messages.Order.Tax", languageId), cusTaxTotal));
            //sb.AppendLine(string.Format("<td width=\"197\" align=\"right\" style=\"font-size: 11px;\">{0}</td><td width=\"169\" align=\"right\" style=\"color: #000; font-size: 14px; padding: 4px 0;\">{1}</td></tr></table></td></tr></table></td></tr>", _localizationService.GetResource("Messages.Order.Total", languageId), cusTotal));

            #endregion

            //sb.AppendLine("</table>");
            result = sb.ToString();
            return result;
        }
        protected virtual string ProductListToHtmlTableTemplateHtml(Order order, int languageId, bool ShowCheckOutAttribute)
        {
            string result = string.Empty;

            var language = _languageService.GetLanguageById(languageId);

            var sb = new StringBuilder();

           
            #region Products
            
            var table = order.OrderProductVariants.ToList();
            for (int i = 0; i <= table.Count - 1; i++)
            {
                var opv = table[i];
                var productVariant = opv.ProductVariant;
                if (productVariant == null)
                    continue;
               
                ////sku
                //if (_catalogSettings.ShowProductSku)
                //{
                //    if (!String.IsNullOrEmpty(opv.ProductVariant.Sku))
                //    {
                //        string sku = string.Format(_localizationService.GetResource("Messages.Order.Product(s).SKU", languageId), HttpUtility.HtmlEncode(opv.ProductVariant.Sku));
                //        sb.AppendLine(string.Format("<td width=\"112\" height=\"164\" style=\"font-size: 11px; border-bottom: 1px solid #dbdbdb; color: #333333; font-family: 'Arial', 'Arial', sans-serif;\">{0}</td>", sku));
                //    }
                //    else
                //    {   //if sku is not given, build html 

                //        sb.AppendLine(string.Format("<td width=\"112\" height=\"164\" style=\"font-size: 11px; border-bottom: 1px solid #dbdbdb; color: #333333; font-family: 'Arial', 'Arial', sans-serif;\">{0}</td>", string.Format(_localizationService.GetResource("Messages.Order.Product(s).SKU", languageId), "")));

                //    }
                //}
                //else
                //{   //if sku is not given, build html

                //    sb.AppendLine(string.Format("<td width=\"112\" height=\"164\" style=\"font-size: 11px; border-bottom: 1px solid #dbdbdb; color: #333333; font-family: 'Arial', 'Arial', sans-serif;\">{0}</td>", string.Format(_localizationService.GetResource("Messages.Order.Product(s).SKU", languageId), "")));
                //}

                string status = "";
                ProductSpecificationAttribute attr = productVariant.Product.ProductSpecificationAttributes.Where(x => x.SpecificationAttributeOption.SpecificationAttribute.Name == "KARGO").FirstOrDefault();

                if (attr != null)
                    status = attr.SpecificationAttributeOption.GetLocalized(x => x.Name);
                else
                    status = "";

                string manufacturer = "";
                ProductSpecificationAttribute specManufacturer = productVariant.Product.ProductSpecificationAttributes.Where(x => x.SpecificationAttributeOption.SpecificationAttributeId == 11).FirstOrDefault();

                if (specManufacturer != null)
                    manufacturer = specManufacturer.SpecificationAttributeOption.GetLocalized(x => x.Name);

                StringBuilder sbProductNameAndMark = new StringBuilder();
                string pictureUrl = _pictureService.GetPictureUrl(opv.ProductVariant.GetDefaultProductVariantPicture(_pictureService), 108, true);
                string productName = string.IsNullOrWhiteSpace(opv.ProductVariant.GetLocalized(x => x.Name)) ? opv.ProductVariant.Product.GetLocalized(x => x.Name) : opv.ProductVariant.GetLocalized(x => x.Name);
                string manufacturerName = opv.ProductVariant.Product.ProductManufacturers.Count > 0 ? opv.ProductVariant.Product.ProductManufacturers.FirstOrDefault().Manufacturer.GetLocalized(x => x.Name) : "";
                // var formattedProductPrice = _priceFormatter.FormatPrice(order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax ? opv.PriceInclTax : opv.PriceExclTax, false, order.CustomerCurrencyCode, language, false);
                string unitPriceStr = string.Empty;
                switch (order.CustomerTaxDisplayType)
                {
                  
                    case TaxDisplayType.IncludingTax:
                    case TaxDisplayType.ExcludingTax:
                        {
                            var opvUnitPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(opv.UnitPriceInclTax, order.CurrencyRate);
                            unitPriceStr = _priceFormatter.FormatPrice(opvUnitPriceInclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, language, true);
                        }
                        break;
                }

                sb.AppendLine("<tr><td style=\"border-bottom:1px solid #eeeeee;\"><table width=\"600\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\"><tr><td width=\"299\" style=\"border-right:1px solid #eee;\"><table width=\"299\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\"><tr><td height=\"160\" width=\"129\" valign=\"middle\" align=\"center\" style=\"text-align:center;\">");
                sb.AppendLine(string.Format("<img src=\"{0}\" width=\"86\" height=\"108\" alt=\" \" /></td>", HttpUtility.HtmlEncode(pictureUrl)));
                sb.AppendLine("<td height=\"160\" width=\"170\" valign=\"middle\" align=\"left\" style=\"text-align:left;\" ><table width=\"170\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\">");
                if (!string.IsNullOrWhiteSpace(manufacturer))
                {
                    sb.AppendLine(string.Format("<tr><td style=\"text-transform:uppercase; color:#000; font-size:10px; line-height:12px;\">{0}</td></tr>", HttpUtility.HtmlEncode(manufacturer)));
                }
                sb.AppendLine(string.Format("<tr><td style=\"text-transform:uppercase; color:#000; font-size:10px; line-height:12px;\">{0}</td></tr>", HttpUtility.HtmlEncode(productName)));
                string sku = string.Format(_localizationService.GetResource("Messages.Order.Product(s).SKU", languageId), HttpUtility.HtmlEncode(opv.ProductVariant.Sku));
                //        sb.AppendLine(string.Format("<td width=\"112\" height=\"164\" style=\"font-size: 11px; border-bottom: 1px solid #dbdbdb; color: #333333; font-family: 'Arial', 'Arial', sans-serif;\">{0}</td>", sku));
                sb.AppendLine(string.Format("<tr><td style=\"text-transform:uppercase; color:#000; font-size:10px; line-height:12px;\">{0}</td></tr>", sku));

                sb.AppendLine("<tr><td height=\"14\"><img src=\"\" width=\"1\" height=\"1\" alt=\" \" /></td></tr><tr><td><table width=\"170\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\">");
                //if (!String.IsNullOrEmpty(opv.AttributeDescription))
                //{

                //    string dd = opv.AttributeDescription;
                //    string dd1;
                //    string[] lines = Regex.Split(dd, "<br />");
                //    dd = lines[0];
                //    dd1 = lines[1];
                //    string[] color = Regex.Split(dd, ": ");
                //    string[] size = Regex.Split(dd1, ": ");
                //    sb.AppendLine(string.Format("<tr><td width=\"75\" style=\"color:#000; font-size:10px; line-height:12px;\">{0}</td><td width=\"95\" style=\"color:#663300; font-size:10px; line-height:12px;\">{1}</td>", color[0], color[1]));
                //    sb.AppendLine(string.Format("</tr><tr><td width=\"75\" style=\"color:#000; font-size:10px; line-height:12px;\">{0}</td><td width=\"95\" style=\"color:#663300; font-size:10px; line-height:12px;\">{1}</td></tr>", size[0], size[1]));
                //    //sb.AppendLine(string.Format("<tr><td width=\"75\" style=\"color:#000; font-size:10px; line-height:12px;\">{0}</td><td width=\"95\" style=\"color:#663300; font-size:10px; line-height:12px;\"></td>", opv.AttributeDescription));
                //}
                 if (!String.IsNullOrEmpty(opv.AttributeDescription))
                {
                    string dd = opv.AttributeDescription;
                    string dd1;
                    string[] lines = Regex.Split(dd, "<br />");
                    string[] color = null;
                    string[] size = null;
                    if (lines.Length == 0)
                    {
                        dd = null;
                        dd1 = null;
                    }
                    else if (lines.Length == 1)
                    {
                        dd = lines[0];
                        dd1 = null;
                       color  = Regex.Split(dd, ": ");
                       //size  = Regex.Split(dd1, ": ");
                    }
                    else
                    {
                        dd = lines[0];
                        dd1 = lines[1];
                        color = Regex.Split(dd, ": ");
                        size = Regex.Split(dd1, ": ");
                    }
                    if (dd!=null && color.Length>=2)
                        sb.AppendLine(string.Format("<tr><td width=\"75\" style=\"color:#000; font-size:10px; line-height:12px;\">{0}</td><td width=\"95\" style=\"color:#663300; font-size:10px; line-height:12px;\">{1}</td></tr>", color[0], color[1]));
                    if (dd1 != null && size.Length >= 2)
                        sb.AppendLine(string.Format("<tr><td width=\"75\" style=\"color:#000; font-size:10px; line-height:12px;\">{0}</td><td width=\"95\" style=\"color:#663300; font-size:10px; line-height:12px;\">{1}</td></tr>", size[0], size[1]));
                }


                //sb.AppendLine(string.Format("<tr><td width=\"75\" style=\"color:#000; font-size:10px; line-height:12px;\">{0}</td><td width=\"95\" style=\"color:#663300; font-size:10px; line-height:12px;\">{1}</td>",_localizationService.GetResource("Messages.Order.Product(s).Color", languageId) ,"product.color"));
                //sb.AppendLine(string.Format("</tr><tr><td width=\"75\" style=\"color:#000; font-size:10px; line-height:12px;\">{0}</td><td width=\"95\" style=\"color:#663300; font-size:10px; line-height:12px;\">{1}</td></tr>", _localizationService.GetResource("Messages.Order.Product(s).Size", languageId), "product.size"));
                sb.AppendLine(string.Format("<tr><td width=\"75\" style=\"color:#000; font-size:10px; line-height:12px;\">{0}:	</td><td width=\"95\" style=\"color:#663300; font-size:10px; line-height:12px;\">{1}</td>", _localizationService.GetResource("Messages.Order.Product(s).Quantity", languageId), opv.Quantity.ToString()));
                sb.AppendLine("</tr></table></td></tr></table></td></tr></table></td>");
                sb.AppendLine("<td width=\"300\"><table width=\"300\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\"><tr><td width=\"20\"><img src=\"\" width=\"1\" height=\"1\" alt=\" \" /></td>");
                sb.AppendLine(string.Format("<td width=\"75\" valign=\"middle\" style=\"line-height:16px; font-size: 12px; text-transform: uppercase; color: #dfdfd0;\">{0}</td>", status));
                sb.AppendLine("<td width=\"24\" height=\"160\" style=\"height:160px;border-right:1px solid #eee;\"><img src=\"\" width=\"1\" height=\"1\" alt=\" \" /></td><td width=\"25\"><img src=\"\" width=\"1\" height=\"1\" alt=\" \" /></td><td width=\"155\" style=\"text-align:left\" align=\"left\"><table width=\"100%\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\">");
                sb.AppendLine(string.Format("<tr><td style=\"font-size:30px; color:#000;\">{0}</td></tr>", unitPriceStr));
                sb.AppendLine("<tr><td height=\"12\"><img src=\"\" width=\"1\" height=\"1\" alt=\" \" /></td></tr>");
                //sb.AppendLine("<tr><td align=\"left\" style=\"font-size:12px; text-transform:uppercase; color:#dfdfd0;\">IN STOCK</td></tr>");
                sb.AppendLine("</table></td></tr></table></td></tr></table></td></tr>");

              
            }
           
            #endregion

            #region CheckoutAttribute


            if (!String.IsNullOrEmpty(order.CheckoutAttributeDescription) && ShowCheckOutAttribute)
            {
                sb.AppendLine("<tr><td style=\"text-align:right;font-family:Arial;font-size:9px;color:#6C5649; \" colspan=\"1\">");
                sb.AppendLine(order.CheckoutAttributeDescription);
                sb.AppendLine("</td><td colspan=\"3\" style=\"text-align:right; font-family:Arial;font-size:10px;color:#6C5649;\">&nbsp; </td></tr>");
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
                case TaxDisplayType.IncludingTax:
                    {
                        //subtotal

                        //var orderSubtotalExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubtotalExclTax, order.CurrencyRate);
                        //cusSubTotal = _priceFormatter.FormatPrice(orderSubtotalExclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, language, false);

                        var orderSubtotalInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubtotalInclTax, order.CurrencyRate);
                        cusSubTotal = _priceFormatter.FormatPrice(orderSubtotalInclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, language, false);

                        //discount (applied to order subtotal)
                        var orderSubTotalDiscountExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubTotalDiscountExclTax, order.CurrencyRate);
                        if (orderSubTotalDiscountExclTaxInCustomerCurrency > decimal.Zero)
                        {
                            cusSubTotalDiscount = _priceFormatter.FormatPrice(-orderSubTotalDiscountExclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, language, false);
                            dislaySubTotalDiscount = true;
                        }
                        //shipping

                        //var orderShippingExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingExclTax, order.CurrencyRate);  ***Kargoya kdv dahil değil.
                        //cusShipTotal = _priceFormatter.FormatShippingPrice(orderShippingExclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, language, false);

                        var orderShipingInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingInclTax, order.CurrencyRate);
                        cusShipTotal = _priceFormatter.FormatShippingPrice(orderShipingInclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, language, false);
                        
                        //payment method additional fee
                        var paymentMethodAdditionalFeeExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeExclTax, order.CurrencyRate);
                        cusPaymentMethodAdditionalFee = _priceFormatter.FormatPaymentMethodAdditionalFee(paymentMethodAdditionalFeeExclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, language, false);
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
                    string taxStr = _priceFormatter.FormatPrice(orderTaxInCustomerCurrency, false, order.CustomerCurrencyCode, language, false);
                    cusTaxTotal = taxStr;
                }
            }
            //checkout attribute

          
            //discount
            bool dislayDiscount = false;
            if (order.OrderDiscount > decimal.Zero)
            {
                var orderDiscountInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderDiscount, order.CurrencyRate);
                cusDiscount = _priceFormatter.FormatPrice(-orderDiscountInCustomerCurrency, false, order.CustomerCurrencyCode, language, false);
                cusSubTotalDiscount = cusDiscount;
                dislayDiscount = true;
            }

            //total
            var orderTotalInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
            cusTotal = _priceFormatter.FormatPrice(orderTotalInCustomerCurrency, false, order.CustomerCurrencyCode, language, false);

            sb.AppendLine("<tr><td><table width=\"600\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\">");

                sb.AppendLine("<tr><td height=\"15\" width=\"395\"><img src=\"\" width=\"1\" height=\"1\" alt=\" \" /></td>");
                sb.AppendLine("<td width=\"24\" style=\"border-right:1px solid #eee;\"><img src=\"\" width=\"1\" height=\"1\" alt=\" \" /></td>");
                sb.AppendLine("<td width=\"156\"><img src=\"\" width=\"1\" height=\"1\" alt=\" \" /></td>");
                sb.AppendLine("<td width=\"24\"><img src=\"\" width=\"1\" height=\"1\" alt=\" \" /></td></tr>");

                sb.AppendLine(string.Format("<tr><td width=\"395\" align=\"right\" style=\"color:#dfdfd0; font-size:20px; text-align:right;\">{0}</td>", _localizationService.GetResource("Messages.Order.SubTotal")));
                sb.AppendLine("<td width=\"24\" style=\"border-right:1px solid #eee;\" ><img src=\"\" width=\"1\" height=\"1\" alt=\" \" /></td>");
                sb.AppendLine(string.Format("<td width=\"156\" align=\"right\" style=\"color:#000; font-size:20px; text-align:right;\">{0}</td>", cusSubTotal));
                sb.AppendLine("<td width=\"24\"><img src=\"\" width=\"1\" height=\"1\" alt=\" \" /></td></tr>");

                sb.AppendLine("<tr><td height=\"2\" width=\"395\"><img src=\"\" width=\"1\" height=\"1\" alt=\" \" /></td>");
                sb.AppendLine("<td width=\"24\" style=\"border-right:1px solid #eee;\"><img src=\"\" width=\"1\" height=\"1\" alt=\" \" /></td>");
                sb.AppendLine("<td width=\"156\"><img src=\"\" width=\"1\" height=\"1\" alt=\" \" /></td>");
                sb.AppendLine("<td width=\"24\"><img src=\"\" width=\"1\" height=\"1\" alt=\" \" /></td></tr>");
            
            /*checkout attributes start*/
                string checkoutAttributesHtml = "<tr><td width=\"395\" align=\"right\" style=\"color:#dfdfd0; font-size:20px; text-align:right;\">{0}</td>"
                  + "<td width=\"24\" style=\"border-right:1px solid #eee;\" ><img src=\"\" width=\"1\" height=\"1\" alt=\" \" /></td>"
                  + "<td width=\"156\" align=\"right\" style=\"color:#000; font-size:20px; text-align:right;\">{1}</td>"
                  + "<td width=\"24\"><img src=\"\" width=\"1\" height=\"1\" alt=\" \" /></td></tr>"

                  + "<tr><td height=\"2\" width=\"395\"><img src=\"\" width=\"1\" height=\"1\" alt=\" \" /></td>"
                  + "<td width=\"24\" style=\"border-right:1px solid #eee;\"><img src=\"\" width=\"1\" height=\"1\" alt=\" \" /></td>"
                  + "<td width=\"156\"><img src=\"\" width=\"1\" height=\"1\" alt=\" \" /></td>"
                  + "<td width=\"24\"><img src=\"\" width=\"1\" height=\"1\" alt=\" \" /></td></tr>";

                sb.AppendLine(FormatCheckoutAttributes(checkoutAttributesHtml));


                /*checkout attributes end*/

                sb.AppendLine(string.Format("<tr><td width=\"395\" align=\"right\" style=\"color:#dfdfd0; font-size:20px; text-align:right;\">{0}</td>", _localizationService.GetResource("Messages.Order.Shipping")));
                sb.AppendLine("<td width=\"24\" style=\"border-right:1px solid #eee;\" ><img src=\"\" width=\"1\" height=\"1\" alt=\" \" /></td>");
                sb.AppendLine(string.Format("<td width=\"156\" align=\"right\" style=\"color:#000; font-size:20px; text-align:right;\">{0}</td>", cusShipTotal));
                sb.AppendLine("<td width=\"24\"><img src=\"\" width=\"1\" height=\"1\" alt=\" \" /></td></tr>");

                sb.AppendLine("<tr><td height=\"2\" width=\"395\"><img src=\"\" width=\"1\" height=\"1\" alt=\" \" /></td>");
                sb.AppendLine("<td width=\"24\" style=\"border-right:1px solid #eee;\"><img src=\"\" width=\"1\" height=\"1\" alt=\" \" /></td>");
                sb.AppendLine("<td width=\"156\"><img src=\"\" width=\"1\" height=\"1\" alt=\" \" /></td>");
                sb.AppendLine("<td width=\"24\"><img src=\"\" width=\"1\" height=\"1\" alt=\" \" /></td></tr>");
                if (!string.IsNullOrEmpty(cusSubTotalDiscount))
                {
                    sb.AppendLine(string.Format("<tr><td width=\"395\" align=\"right\" style=\"color:#dfdfd0; font-size:20px; text-align:right;\">{0}</td>", _localizationService.GetResource("Messages.Order.SubTotalDiscount", languageId)));
                    sb.AppendLine("<td width=\"24\" style=\"border-right:1px solid #eee;\" ><img src=\"\" width=\"1\" height=\"1\" alt=\" \" /></td>");
                    sb.AppendLine(string.Format("<td width=\"156\" align=\"right\" style=\"color:#000; font-size:20px; text-align:right;\">{0}</td>", cusSubTotalDiscount));
                    sb.AppendLine("<td width=\"24\"><img src=\"\" width=\"1\" height=\"1\" alt=\" \" /></td></tr>");

                    sb.AppendLine("<tr><td height=\"2\" width=\"395\"><img src=\"\" width=\"1\" height=\"1\" alt=\" \" /></td>");
                    sb.AppendLine("<td width=\"24\" style=\"border-right:1px solid #eee;\"><img src=\"\" width=\"1\" height=\"1\" alt=\" \" /></td>");
                    sb.AppendLine("<td width=\"156\"><img src=\"\" width=\"1\" height=\"1\" alt=\" \" /></td>");
                    sb.AppendLine("<td width=\"24\"><img src=\"\" width=\"1\" height=\"1\" alt=\" \" /></td></tr>");
                }


                //sb.AppendLine(FormatCheckoutAttributes("<tr><td width=\"395\" align=\"right\" style=\"color:#dfdfd0; font-size:20px; text-align:right;\">{0}</td><td width=\"24\" style=\"border-right:1px solid #eee;\" ><img src=\"\" width=\"1\" height=\"1\" alt=\" \" /></td><td width=\"156\" align=\"right\" style=\"color:#000; font-size:20px; text-align:right;\">{1}</td><tr><td height=\"2\" width=\"395\"><img src=\"\" width=\"1\" height=\"1\" alt=\" \" /></td><td width=\"24\" style=\"border-right:1px solid #eee;\"><img src=\"\" width=\"1\" height=\"1\" alt=\" \" /></td><td width=\"156\"><img src=\"\" width=\"1\" height=\"1\" alt=\" \" /></td><td width=\"24\"><img src=\"\" width=\"1\" height=\"1\" alt=\" \" /></td></tr>"));

                //sb.AppendLine(string.Format("<tr><td width=\"395\" align=\"right\" style=\"color:#dfdfd0; font-size:20px; text-align:right;\">{0}</td>", _localizationService.GetResource("Messages.Order.Tax", languageId)));
                //sb.AppendLine("<td width=\"24\" style=\"border-right:1px solid #eee;\" ><img src=\"\" width=\"1\" height=\"1\" alt=\" \" /></td>");
                //sb.AppendLine(string.Format("<td width=\"156\" align=\"right\" style=\"color:#000; font-size:20px; text-align:right;\">{0}</td>", cusTaxTotal));
                //sb.AppendLine("<td width=\"24\"><img src=\"\" width=\"1\" height=\"1\" alt=\" \" /></td></tr>");

                sb.AppendLine("<tr><td height=\"2\" width=\"395\"><img src=\"\" width=\"1\" height=\"1\" alt=\" \" /></td>");
                sb.AppendLine("<td width=\"24\" style=\"border-right:1px solid #eee;\"><img src=\"\" width=\"1\" height=\"1\" alt=\" \" /></td>");
                sb.AppendLine("<td width=\"156\"><img src=\"\" width=\"1\" height=\"1\" alt=\" \" /></td>");
                sb.AppendLine("<td width=\"24\"><img src=\"\" width=\"1\" height=\"1\" alt=\" \" /></td></tr>");

                sb.AppendLine(string.Format("<tr valign=\"middle\" style=\"height: 60px;\"><td width=\"395\" align=\"right\" style=\"color:#000; font-size:20px; text-align:right;\">{0}</td>", _localizationService.GetResource("Messages.Order.Total", languageId)));
                sb.AppendLine("<td width=\"24\" style=\"border-right:1px solid #eee;\" ><img src=\"\" width=\"1\" height=\"1\" alt=\" \" /></td>");
                sb.AppendLine(string.Format("<td width=\"156\" align=\"right\" style=\"color:#000; font-size:30px; text-align:right;\">{0}</td>", cusTotal));
                sb.AppendLine("<td width=\"24\"><img src=\"\" width=\"1\" height=\"1\" alt=\" \" /></td></tr>");

            //sb.AppendLine("</tr>");
            //    sb.AppendLine("<tr><td><table width=\"600\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\"><tbody><tr><td height=\"20\" width=\"395\"><img src=\"http://html.thesparxitsolutions.com/hatemoglu/newsletter/img/blank.gif\" width=\"1\" height=\"1\" alt=\" \"></td><td width=\"24\" style=\"border-right:1px solid #eee;\"><img src=\"http://html.thesparxitsolutions.com/hatemoglu/newsletter/img/blank.gif\" width=\"1\" height=\"1\" alt=\" \"></td><td width=\"156\"><img src=\"http://html.thesparxitsolutions.com/hatemoglu/newsletter/img/blank.gif\" width=\"1\" height=\"1\" alt=\" \"></td><td width=\"24\"><img src=\"http://html.thesparxitsolutions.com/hatemoglu/newsletter/img/blank.gif\" width=\"1\" height=\"1\" alt=\" \"></td></tr></tbody></table></td></tr>");

            //    sb.AppendLine("</td></table>");

                sb.AppendLine("</tr></td></table>");


            //sb.AppendLine("<tr><td><table width=\"600\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\"><tr>");
            //sb.AppendLine(string.Format("<td width=\"395\" align=\"right\" style=\"color:#000; font-size:20px; text-align:right;\">{0}</td>", _localizationService.GetResource("Messages.Order.Total", languageId)));
            //                       sb.AppendLine(" <td width=\"24\" style=\"border-right:1px solid #eee;\" ><img src=\"\" width=\"1\" height=\"1\" alt=\" \" /></td>");
            //                       sb.AppendLine(string.Format("<td width=\"165\" align=\"right\" style=\"color:#000; font-size:30px; text-align:right;\">{0}</td>", cusTotal));
            //                        sb.AppendLine(" <td width=\"15\"><img src=\"\" width=\"1\" height=\"1\" alt=\" \" /></td></tr></table></td></tr>");




            //sb.AppendLine("<tr><td width=\"620\"><table width=\"620\" height=\"151\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" bgcolor=\"#fffef7\" style=\"border-left: 1px solid #cccccc; border-right: 1px solid #cccccc; border-bottom: 1px solid #cccccc;\"><tr><td width=\"502\" height=\"16\"></td><td width=\"100\" height=\"16\"></td><td rowspan=\"6\" width=\"18\" height=\"151\"></td></tr>");
            //sb.AppendLine(string.Format("<tr><td width=\"502\" height=\"28\" align=\"right\" style=\"font-size: 10px; color: #333333; font-family: 'Arial', 'Arial', sans-serif;\">{0}</td><td width=\"100\" height=\"28\" align=\"right\" style=\"font-size: 12px; color: #000000; font-family: 'Arial', 'Arial', sans-serif;\">{1}</td></tr>", _localizationService.GetResource("Messages.Order.SubTotal", languageId), cusSubTotal));
            //sb.AppendLine(string.Format("<tr><td width=\"502\" height=\"28\" align=\"right\" style=\"font-size: 10px; color: #333333; font-family: 'Arial', 'Arial', sans-serif;\">{0}</td><td width=\"100\" height=\"28\" align=\"right\" style=\"font-size: 12px; color: #000000; font-family: 'Arial', 'Arial', sans-serif;\">{1}</td></tr>", _localizationService.GetResource("Messages.Order.Shipping", languageId), cusShipTotal));
            //if (!string.IsNullOrEmpty(cusSubTotalDiscount)) 
            //sb.AppendLine(string.Format("<tr><td width=\"502\" height=\"28\" align=\"right\" style=\"font-size: 10px; color: #333333; font-family: 'Arial', 'Arial', sans-serif;\">{0}</td><td width=\"100\" height=\"28\" align=\"right\" style=\"font-size: 12px; color: #000000; font-family: 'Arial', 'Arial', sans-serif;\">{1}</td></tr>", _localizationService.GetResource("Messages.Order.SubTotalDiscount", languageId), cusSubTotalDiscount));
            //sb.AppendLine(FormatCheckoutAttributes("<tr><td width=\"502\" height=\"28\" align=\"right\" style=\"font-size: 10px; color: #333333; font-family: 'Arial', 'Arial', sans-serif;\">{0}</td><td width=\"100\" height=\"28\" align=\"right\" style=\"font-size: 12px; color: #000000; font-family: 'Arial', 'Arial', sans-serif;\">{1}</td></tr>"));
            
            //sb.AppendLine(string.Format("<tr><td width=\"502\" height=\"28\" align=\"right\" style=\"font-size: 10px; color: #333333; font-family: 'Arial', 'Arial', sans-serif;\">{0}</td><td width=\"100\" height=\"28\" align=\"right\" style=\"font-size: 12px; color: #000000; font-family: 'Arial', 'Arial', sans-serif;\">{1}</td></tr>", _localizationService.GetResource("Messages.Order.Tax", languageId), cusTaxTotal));
            //sb.AppendLine(string.Format("<tr><td width=\"502\" height=\"28\" align=\"right\" style=\"font-size: 10px; color: #333333; font-family: 'Arial', 'Arial', sans-serif;\">{0}</td><td width=\"100\" height=\"28\" align=\"right\" style=\"font-size: 12px; color: #000000; font-family: 'Arial', 'Arial', sans-serif;\">{1}</td></tr>", _localizationService.GetResource("Messages.Order.Total", languageId), cusTotal));
            //sb.AppendLine("</table>");

           
            #endregion

           
            result = sb.ToString();
            return result;
        }

        protected virtual string ProductListToHtmlTableTemplateBisse(Order order, int languageId)
        {
            string result = string.Empty;

            var language = _languageService.GetLanguageById(languageId);

            var sb = new StringBuilder();

           
            #region Products

            sb.AppendLine("<tr><td><table width=\"580\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\" align=\"center\"><tr>");
            sb.AppendLine(string.Format("<td width=\"149\" style=\"border-bottom: 1px solid #d3d3d3; color: #6e6e6e; padding: 10px 0; font-family: Arial; font-size: 11px;\" align=\"left\">{0}</td>", string.Format(_localizationService.GetResource("Messages.Order.Product(s).SKU", languageId), "")));
            sb.AppendLine(string.Format("<td width=\"267\" style=\"border-bottom: 1px solid #d3d3d3; color: #6e6e6e; padding: 10px 0; font-family: Arial; font-size: 11px;\" align=\"left\">{0}</td>", string.Format(_localizationService.GetResource("Messages.Order.Product(s).Description", languageId), "")));
            sb.AppendLine(string.Format("<td width=\"68\" style=\"border-bottom: 1px solid #d3d3d3; color: #6e6e6e; padding: 10px 0; font-family: Arial; font-size: 11px;\" align=\"left\">{0}</td>", string.Format(_localizationService.GetResource("OrderDetails.Header.Quantity", languageId), "")));
            sb.AppendLine(string.Format("<td width=\"96\" style=\"border-bottom: 1px solid #d3d3d3; color: #6e6e6e; padding: 10px 0; font-family: Arial; font-size: 11px;\" align=\"left\">{0}</td></tr><tr>", string.Format(_localizationService.GetResource("OrderDetails.Header.Price", languageId), "")));
            //sb.AppendLine(string.Format("<td width=\"113\" style=\"border-bottom: 1px solid #d3d3d3; color: #6e6e6e; padding: 10px 0; font-family: Arial; font-size: 11px;\" align=\"left\">{0}</td></tr><tr>", string.Format(_localizationService.GetResource("Messages.Order.Product(s).STATUS", languageId), "")));

            var table = order.OrderProductVariants.ToList();
            for (int i = 0; i <= table.Count - 1; i++)
            {
                var opv = table[i];
                var productVariant = opv.ProductVariant;
                if (productVariant == null)
                    continue;
                sb.AppendLine("<tr>");
                //sku
                if (_catalogSettings.ShowProductSku)
                {
                    if (!String.IsNullOrEmpty(opv.ProductVariant.Sku))
                    {
                        string sku = string.Format(_localizationService.GetResource("Messages.Order.Product(s).SKU", languageId), HttpUtility.HtmlEncode(opv.ProductVariant.Sku));
                        sb.AppendLine(string.Format("<td style=\"border-bottom: 1px solid #d3d3d3; padding: 28px 0; color: #3d3d3d; font-family: Arial; font-size: 12px; line-height: 20px;\" valign=\"top\"> {0} </td>", sku));

                    }
                    else
                    {   //if sku is not given, build html 

                        sb.AppendLine(string.Format("<td style=\"border-bottom: 1px solid #d3d3d3; padding: 28px 0; color: #3d3d3d; font-family: Arial; font-size: 12px; line-height: 20px;\" valign=\"top\"> {0} </td>", string.Format(_localizationService.GetResource("Messages.Order.Product(s).SKU", languageId), "")));

                    }
                }
                else
                {   //if sku is not given, build html

                    sb.AppendLine(string.Format("<td style=\"border-bottom: 1px solid #d3d3d3; padding: 28px 0; color: #3d3d3d; font-family: Arial; font-size: 12px; line-height: 20px;\" valign=\"top\"> {0} </td>", string.Format(_localizationService.GetResource("Messages.Order.Product(s).SKU", languageId), "")));
                }

                string status = "";
                ProductSpecificationAttribute attr = productVariant.Product.ProductSpecificationAttributes.Where(x => x.SpecificationAttributeOption.SpecificationAttribute.Name == "Kargo").FirstOrDefault();

                if (attr != null)
                    status = attr.SpecificationAttributeOption.GetLocalized(x => x.Name);
                else
                    status = "";

                StringBuilder sbProductNameAndMark = new StringBuilder();
                string pictureUrl = _pictureService.GetPictureUrl(opv.ProductVariant.GetDefaultProductVariantPicture(_pictureService), _mediaSettings.ProductVariantPictureSize, true);
                string productName = string.IsNullOrWhiteSpace(opv.ProductVariant.GetLocalized(x => x.Name)) ? opv.ProductVariant.Product.GetLocalized(x => x.Name) : opv.ProductVariant.GetLocalized(x => x.Name);
                string manufacturerName = opv.ProductVariant.Product.ProductManufacturers.Count > 0 ? opv.ProductVariant.Product.ProductManufacturers.FirstOrDefault().Manufacturer.GetLocalized(x => x.Name) : "";
                // var formattedProductPrice = _priceFormatter.FormatPrice(order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax ? opv.PriceInclTax : opv.PriceExclTax, false, order.CustomerCurrencyCode, language, false);
                string unitPriceStr = string.Empty;
                switch (order.CustomerTaxDisplayType)
                {
                    //case TaxDisplayType.ExcludingTax:
                    //    {
                    //        var opvUnitPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(opv.UnitPriceExclTax, order.CurrencyRate);
                    //        unitPriceStr = _priceFormatter.FormatPrice(opvUnitPriceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, false);
                    //    }
                    //    break;
                    case TaxDisplayType.IncludingTax:
                    case TaxDisplayType.ExcludingTax:
                        {
                            var opvUnitPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(opv.UnitPriceInclTax, order.CurrencyRate);
                            unitPriceStr = _priceFormatter.FormatPrice(opvUnitPriceInclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, language, true);
                        }
                        break;
                }

                sb.AppendLine(string.Format("<td style=\"border-bottom: 1px solid #d3d3d3; padding: 15px 0;\" valign=\"top\"><table width=\"239\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\"><tr><td width=\"94\"><img src=\"{0}\" width=\"94\" height=\"123\" alt=\"\" border=\"0\" /></td><td width=\"145\" valign=\"top\" style=\"padding: 15px 0 0 0;\"><table width=\"110\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\" align=\"center\"><tr>", HttpUtility.HtmlEncode(pictureUrl)));
                sb.AppendLine(string.Format("<td style=\"font-size: 11px; line-height: 14px; padding: 0 0 12px 0;\"><strong> {0} </strong>", HttpUtility.HtmlEncode(productName)));
                
                if (!String.IsNullOrEmpty(opv.AttributeDescription))
                {
                    string dd = opv.AttributeDescription;
                    string dd1;
                    string[] lines = Regex.Split(dd, "<br />");
                    string[] color = null;
                    string[] size = null;
                    if (lines.Length == 0)
                    {
                        dd = null;
                        dd1 = null;
                    }
                    else if (lines.Length == 1)
                    {
                        dd = lines[0];
                        dd1 = null;
                       color  = Regex.Split(dd, ": ");
                       //size  = Regex.Split(dd1, ": ");
                    }
                    else
                    {
                        dd = lines[0];
                        dd1 = lines[1];
                        color = Regex.Split(dd, ": ");
                        size = Regex.Split(dd1, ": ");
                    }
                    sb.AppendLine(" <br/><table>");
                    if (dd!=null)
                    sb.AppendLine(string.Format("<tr><td width=\"90\" style=\"color:#000; font-size:11px; line-height:14px;\">{0}</td><td width=\"80\" style=\"color:#663300; font-size:10px; line-height:12px;\">{1}</td></tr>", color[0], color[1]));
                    if(dd1!=null)
                    sb.AppendLine(string.Format("<tr><td width=\"90\" style=\"color:#000; font-size:11px; line-height:14px;\">{0}</td><td width=\"80\" style=\"color:#663300; font-size:10px; line-height:12px;\">{1}</td></tr>", size[0], size[1]));
                    sb.AppendLine("</table>");
                }

                sb.AppendLine("</tr><tr><td><table width=\"110\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\" align=\"center\" style=\"color: #6e6e6e; font-size: 10px; line-height: 14px;\">");
                sbProductNameAndMark.AppendLine(string.Format("<p style=\"font-size: 11px; color: rgb(51, 51, 51); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif; margin: 1px 0pt; padding: 0pt;\">{0}</p>", _productAttributeFormatterService.FormatAttributes(opv.ProductVariant, opv.AttributesXml, "<span>{0}</span> :{1}", order.Customer, htmlEncode: false)));
                sb.AppendLine("</table></td></tr></table></td></tr></table></td>");
                sb.AppendLine(string.Format("<td style=\"border-bottom: 1px solid #d3d3d3; padding: 28px 0 28px 5px; color: #3d3d3d; font-family: Arial; font-size: 12px; line-height: 20px;\" valign=\"top\">{0}</td>", opv.Quantity.ToString()));
                sb.AppendLine(string.Format("<td style=\"border-bottom: 1px solid #d3d3d3; padding: 28px 0; color: #111; font-family: Arial; font-size: 15px; line-height: 20px;\" valign=\"top\" align=\"right\">{0}</td>", unitPriceStr));
                //sb.AppendLine(string.Format("<td style=\"border-bottom: 1px solid #d3d3d3; padding: 28px 0; color: #3d3d3d; font-family: Arial; font-size: 12px; line-height: 16px;\" valign=\"top\" align=\"right\">Ships in<br />{0} hours</td>", status));
                sb.AppendLine("</tr>");
            }
            sb.AppendLine("</tr>");
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
                case TaxDisplayType.IncludingTax:
                    {
                        //subtotal
                        var orderSubtotalExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubtotalExclTax, order.CurrencyRate);
                        cusSubTotal = _priceFormatter.FormatPrice(orderSubtotalExclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, language, false);
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
                //case TaxDisplayType.IncludingTax:
                //    {
                //        //subtotal
                //        var orderSubtotalInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubtotalInclTax, order.CurrencyRate);
                //        cusSubTotal = _priceFormatter.FormatPrice(orderSubtotalInclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, language, true);
                //        //discount (applied to order subtotal)
                //        var orderSubTotalDiscountInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubTotalDiscountInclTax, order.CurrencyRate);
                //        if (orderSubTotalDiscountInclTaxInCustomerCurrency > decimal.Zero)
                //        {
                //            cusSubTotalDiscount = _priceFormatter.FormatPrice(-orderSubTotalDiscountInclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, language, true);
                //            dislaySubTotalDiscount = true;
                //        }
                //        //shipping
                //        var orderShippingInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingInclTax, order.CurrencyRate);
                //        cusShipTotal = _priceFormatter.FormatShippingPrice(orderShippingInclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, language, true);
                //        //payment method additional fee
                //        var paymentMethodAdditionalFeeInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeInclTax, order.CurrencyRate);
                //        cusPaymentMethodAdditionalFee = _priceFormatter.FormatPaymentMethodAdditionalFee(paymentMethodAdditionalFeeInclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, language, true);
                //    }
                //    break;
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
                    string taxStr = _priceFormatter.FormatPrice(orderTaxInCustomerCurrency, false, order.CustomerCurrencyCode, language, false);
                    cusTaxTotal = taxStr;
                }
            }
            //checkout attribute

          
            //discount
            bool dislayDiscount = false;
            if (order.OrderDiscount > decimal.Zero)
            {
                var orderDiscountInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderDiscount, order.CurrencyRate);
                cusDiscount = _priceFormatter.FormatPrice(-orderDiscountInCustomerCurrency, false, order.CustomerCurrencyCode, language, false);
                cusSubTotalDiscount = cusDiscount;
                dislayDiscount = true;
            }

            //total
            var orderTotalInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
            cusTotal = _priceFormatter.FormatPrice(orderTotalInCustomerCurrency, false, order.CustomerCurrencyCode, language, false);
            sb.AppendLine("<td colspan=\"5\" height=\"3\" style=\"font-size: 0;\"><img src=\"" + _webHelper.GetAbsolutePath("/_img/content-separator.png") + "\" width=\"580\" height=\"3\" alt=\"\" border=\"0\" /></td></tr>");

            sb.AppendLine(string.Format("<tr><td colspan=\"5\" style=\"padding: 20px 0;\"><table width=\"366\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\" align=\"right\"><tr><td width=\"197\" align=\"right\" style=\"font-size: 11px;\">{0}</td><td width=\"169\" align=\"right\" style=\"color: #000; font-size: 14px; padding: 4px 0;\">{1}</td></tr><tr>", _localizationService.GetResource("Messages.Order.SubTotal", languageId), cusSubTotal));
            sb.AppendLine(string.Format("<td width=\"197\" align=\"right\" style=\"font-size: 11px;\">{0}</td><td width=\"169\" align=\"right\" style=\"color: #000; font-size: 14px; padding: 4px 0;\">{1}</td></tr><tr>", _localizationService.GetResource("Messages.Order.Shipping", languageId), cusShipTotal));
            //ToDo: Check other discount types
            if (!string.IsNullOrEmpty(cusSubTotalDiscount)) sb.AppendLine(string.Format("<td width=\"197\" align=\"right\" style=\"font-size: 11px;\">{0}</td><td width=\"169\" align=\"right\" style=\"color: #000; font-size: 14px; padding: 4px 0;\">{1}</td></tr><tr>", _localizationService.GetResource("Messages.Order.SubTotalDiscount", languageId), cusSubTotalDiscount));
            sb.AppendLine(FormatCheckoutAttributes("<td width=\"197\" align=\"right\" style=\"font-size: 11px;\">{0}</td><td width=\"169\" align=\"right\" style=\"color: #000; font-size: 14px; padding: 4px 0;\">{1}</td></tr><tr>"));
            sb.AppendLine(string.Format("<td width=\"197\" align=\"right\" style=\"font-size: 11px;\">{0}</td><td width=\"169\" align=\"right\" style=\"color: #000; font-size: 14px; padding: 4px 0;\">{1}</td></tr><tr>", _localizationService.GetResource("Messages.Order.Tax", languageId), cusTaxTotal));
            sb.AppendLine(string.Format("<td width=\"197\" align=\"right\" style=\"font-size: 11px;\">{0}</td><td width=\"169\" align=\"right\" style=\"color: #000; font-size: 14px; padding: 4px 0;\">{1}</td></tr></table></td></tr></table></td></tr>", _localizationService.GetResource("Messages.Order.Total", languageId), cusTotal));


            //////sb.AppendLine("<tr><td  height=\"151\" width=\"620\"><table style=\"border-left: 1px solid rgb(204, 204, 204); border-collapse: collapse; border-right: 1px solid rgb(204, 204, 204);                    border-bottom: 1px solid rgb(204, 204, 204);\" bgcolor=\"#f3f3f3\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" height=\"151\" width=\"620\"><tbody> <tr><td height=\"16\" width=\"502\"> </td> <td height=\"16\" width=\"100\"> </td> <td rowspan=\"6\" height=\"151\" width=\"18\"> </td> </tr> <tr>");//1.tr
            //////sb.AppendLine(string.Format("<td style=\"font-size: 10px; color: rgb(51, 51, 51); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif;\" align=\"right\" height=\"28\" width=\"450\">{0}</td>", _localizationService.GetResource("Messages.Order.SubTotal", languageId)));
            //////sb.AppendLine(string.Format(" <td style=\"font-size: 12px; color: rgb(0, 0, 0); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif;\"align=\"right\" height=\"28\" width=\"152\">{0}</td></tr>", cusSubTotal));//2.tr
            //////sb.AppendLine(string.Format("<tr><td style=\"font-size: 10px; color: rgb(51, 51, 51); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif;\" align=\"right\" height=\"28\" width=\"450\">{0}</td>", _localizationService.GetResource("Messages.Order.Shipping", languageId)));
            //////sb.AppendLine(string.Format(" <td style=\"font-size: 12px; color: rgb(0, 0, 0); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif;\"align=\"right\" height=\"28\" width=\"152\">{0}</td></tr>", cusShipTotal));//3.tr
            //////sb.AppendLine(string.Format("<tr><td style=\"font-size: 10px; color: rgb(51, 51, 51); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif;\" align=\"right\" height=\"28\" width=\"450\">{0}</td>", _localizationService.GetResource("Messages.Order.Tax", languageId)));
            //////sb.AppendLine(string.Format(" <td style=\"font-size: 12px; color: rgb(0, 0, 0); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif;\"align=\"right\" height=\"28\" width=\"152\">{0}</td></tr>", cusTaxTotal));//4.tr
            //////if (order.OrderDiscount > 0)
            //////{
            //////    sb.AppendLine(string.Format("<tr><td style=\"font-size: 10px; color: rgb(51, 51, 51); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif;\" align=\"right\" height=\"28\" width=\"450\">{0}</td>", _localizationService.GetResource("Messages.Order.SubTotalDiscount", languageId)));
            //////    sb.AppendLine(string.Format(" <td style=\"font-size: 12px; color: rgb(0, 0, 0); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif;\"align=\"right\" height=\"28\" width=\"152\">{0}</td></tr>", cusSubTotalDiscount));//tr
            //////}
            //////sb.AppendLine(string.Format("<tr><td style=\"font-size: 10px; color: rgb(51, 51, 51); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif;\" align=\"right\" height=\"23\" width=\"450\">{0}</td>", _localizationService.GetResource("Messages.Order.Total", languageId)));
            //////sb.AppendLine(string.Format("<td style=\"font-size: 12px; color: rgb(0, 0, 0); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif;\" align=\"right\" height=\"23\" width=\"152\">{0} </td></tr>", cusTotal));//6.tr
            ////////sb.AppendLine(string.Format("<tr><td style=\"font-size: 10px; color: rgb(51, 51, 51); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif;\" align=\"right\" height=\"25\" width=\"450\"> {0}: </td>",_localizationService.GetResource( "Messages.Order.Currency",languageId)));
            ////////sb.AppendLine(string.Format("<td style=\"font-size: 12px; color: rgb(0, 0, 0); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif;\" align=\"right\" height=\"25\" width=\"152\"> {0} </td> </tr>",order.CustomerCurrencyCode));
            ////////sb.AppendLine(string.Format("<tr><td style=\"font-size: 10px; color: rgb(51, 51, 51); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif;\"  align=\"right\" height=\"25\" width=\"450\"> {0}: </td>",_localizationService.GetResource( "Messages.Order.PaymentMethod",languageId)));
            ////////sb.AppendLine(string.Format("<td style=\"font-size: 12px; color: rgb(0, 0, 0); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif;\" align=\"right\" height=\"25\" width=\"152\"> {0} </td></tr>", order.PaymentMethodSystemName));
            //////sb.AppendLine(string.Format(" <tr><td style=\"font-size: 10px; color: rgb(51, 51, 51); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif;\"  align=\"right\" height=\"34\" width=\"450\"> {0} </td>", _localizationService.GetResource("Messages.Order.ShippingMethod", languageId)));
            //////sb.AppendLine(string.Format("<td style=\"font-size: 12px; color: rgb(0, 0, 0); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif;\" align=\"right\" height=\"34\" width=\"152\">{0}</td></tr>", order.ShippingMethod));
            //////sb.AppendLine("</tbody></table></td></tr>");
            ////////subtotal
            ////////sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, _localizationService.GetResource("Messages.Order.SubTotal", languageId), cusSubTotal));

            ////////discount (applied to order subtotal)
            //////if (dislaySubTotalDiscount)
            //////{
            //////    //sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, _localizationService.GetResource("Messages.Order.SubTotalDiscount", languageId), cusSubTotalDiscount));
            //////}


            ////////shipping
            //////if (dislayShipping)
            //////{
            //////    //sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, _localizationService.GetResource("Messages.Order.Shipping", languageId), cusShipTotal));
            //////}

            ////////payment method fee
            //////if (displayPaymentMethodFee)
            //////{
            //////    string paymentMethodFeeTitle = _localizationService.GetResource("Messages.Order.PaymentMethodAdditionalFee", languageId);
            //////    //sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, paymentMethodFeeTitle, cusPaymentMethodAdditionalFee));
            //////}

            ////////tax
            //////if (displayTax)
            //////{
            //////    //sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, _localizationService.GetResource("Messages.Order.Tax", languageId), cusTaxTotal));
            //////}
            //////if (displayTaxRates)
            //////{
            //////    foreach (var item in taxRates)
            //////    {
            //////        string taxRate = String.Format(_localizationService.GetResource("Messages.Order.TaxRateLine"), _priceFormatter.FormatTaxRate(item.Key));
            //////        string taxValue = _priceFormatter.FormatPrice(item.Value, true, order.CustomerCurrencyCode, language, false);
            //////        //sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, taxRate, taxValue));
            //////    }
            //////}

            ////////discount
            //////if (dislayDiscount)
            //////{
            //////    //sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, _localizationService.GetResource("Messages.Order.TotalDiscount", languageId), cusDiscount));
            //////}

            ////////gift cards
            //////var gcuhC = order.GiftCardUsageHistory;
            //////foreach (var gcuh in gcuhC)
            //////{
            //////    string giftCardText = String.Format(_localizationService.GetResource("Messages.Order.GiftCardInfo", languageId), HttpUtility.HtmlEncode(gcuh.GiftCard.GiftCardCouponCode));
            //////    string giftCardAmount = _priceFormatter.FormatPrice(-(_currencyService.ConvertCurrency(gcuh.UsedValue, order.CurrencyRate)), true, order.CustomerCurrencyCode, false, language);
            //////    //sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, giftCardText, giftCardAmount));
            //////}

            ////////reward points
            //////if (order.RedeemedRewardPointsEntry != null)
            //////{
            //////    string rpTitle = string.Format(_localizationService.GetResource("Messages.Order.RewardPoints", languageId), -order.RedeemedRewardPointsEntry.Points);
            //////    string rpAmount = _priceFormatter.FormatPrice(-(_currencyService.ConvertCurrency(order.RedeemedRewardPointsEntry.UsedAmount, order.CurrencyRate)), true, order.CustomerCurrencyCode, false, language);
            //////    //sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, rpTitle, rpAmount));
            //////}

            //total
            //sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, _localizationService.GetResource("Messages.Order.OrderTotal", languageId), cusTotal));
            #endregion

            //sb.AppendLine("</table>");
            result = sb.ToString();
            return result;
        }
       
        public virtual void AddOrderTokens(IList<Token> tokens, Order order, int languageId,bool ShowCheckOutAttribute)
        {
            //tokens.Add(new Token("Store.AccountNumber.Title", order.PaymentMethodSystemName == "Payments.PurchaseOrder" ? _localizationService.GetResource("Store.WireTransferNumber.Title", languageId).ToString() : ""));
            //tokens.Add(new Token("Store.AccountNumber", order.PaymentMethodSystemName == "Payments.PurchaseOrder" ? _localizationService.GetResource("Store.WireTransferNumber", languageId).ToString() : ""));
            //tokens.Add(new Token("Store.AccountNumber.Title2", order.PaymentMethodSystemName == "Payments.PurchaseOrder" ? _localizationService.GetResource("Store.WireTransferNumber.Title2", languageId).ToString() : ""));
            //tokens.Add(new Token("Store.AccountNumber2", order.PaymentMethodSystemName == "Payments.PurchaseOrder" ? _localizationService.GetResource("Store.WireTransferNumber2", languageId).ToString() : ""));
            tokens.Add(new Token("Store.AccountNumber.Title", _localizationService.GetResource("Store.WireTransferNumber.Title", languageId)));
            tokens.Add(new Token("Store.AccountNumber", _localizationService.GetResource("Store.WireTransferNumber", languageId)));
            tokens.Add(new Token("Store.AccountNumber.Title2", _localizationService.GetResource("Store.WireTransferNumber.Title2", languageId)));
            tokens.Add(new Token("Store.AccountNumber2", _localizationService.GetResource("Store.WireTransferNumber2", languageId)));

            tokens.Add(new Token("Order.OrderNumber", order.Id.ToString()));
            tokens.Add(new Token("Order.CustomerFullName", HttpUtility.HtmlEncode(string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName)), true));
            tokens.Add(new Token("Order.CustomerEmail", HttpUtility.HtmlEncode(order.BillingAddress.Email)));


            tokens.Add(new Token("Order.BillingFirstName", HttpUtility.HtmlEncode(order.BillingAddress.FirstName), true));
            tokens.Add(new Token("Order.BillingLastName", HttpUtility.HtmlEncode(order.BillingAddress.LastName), true));
            tokens.Add(new Token("Order.BillingPhoneNumber", HttpUtility.HtmlEncode(order.BillingAddress.PhoneNumber)));
            tokens.Add(new Token("Order.BillingEmail", HttpUtility.HtmlEncode(order.BillingAddress.Email)));
            tokens.Add(new Token("Order.BillingFaxNumber", HttpUtility.HtmlEncode(order.BillingAddress.FaxNumber)));
            tokens.Add(new Token("Order.BillingAddress1", HttpUtility.HtmlEncode(order.BillingAddress.Address1), true));
            tokens.Add(new Token("Order.BillingAddress2", HttpUtility.HtmlEncode(order.BillingAddress.Address2), true));
            tokens.Add(new Token("Order.BillingCity", HttpUtility.HtmlEncode(order.BillingAddress.City), true));
            tokens.Add(new Token("Order.BillingStateProvince", order.BillingAddress.StateProvince != null ? HttpUtility.HtmlEncode(order.BillingAddress.StateProvince.Name) : "", true));
            tokens.Add(new Token("Order.BillingZipPostalCode", HttpUtility.HtmlEncode(order.BillingAddress.ZipPostalCode)));
            tokens.Add(new Token("Order.BillingCountry", order.BillingAddress.Country != null ? HttpUtility.HtmlEncode(order.BillingAddress.Country.Name) : "", true));
            tokens.Add(new Token("Order.BillingTaxNo", order.BillingAddress.IsEnterprise && order.BillingAddress.TaxNo != null ? HttpUtility.HtmlEncode(order.BillingAddress.TaxNo) : "", true));
            tokens.Add(new Token("Order.BillingTaxOffice", order.BillingAddress.IsEnterprise && order.BillingAddress.TaxOffice != null ? HttpUtility.HtmlEncode(order.BillingAddress.TaxOffice) : "", true));
            tokens.Add(new Token("Order.BillingCompany", order.BillingAddress.IsEnterprise ? HttpUtility.HtmlEncode(order.BillingAddress.Company):"", true));
            
            tokens.Add(new Token("Order.ShippingMethod", order.ShippingMethod));
            tokens.Add(new Token("Order.ShippingFirstName", order.ShippingAddress != null ? HttpUtility.HtmlEncode(order.ShippingAddress.FirstName) : "", true));
            tokens.Add(new Token("Order.ShippingLastName", order.ShippingAddress != null ? HttpUtility.HtmlEncode(order.ShippingAddress.LastName) : "", true));
            tokens.Add(new Token("Order.ShippingPhoneNumber", order.ShippingAddress != null ? HttpUtility.HtmlEncode(order.ShippingAddress.PhoneNumber) : "", true));
            tokens.Add(new Token("Order.ShippingEmail", order.ShippingAddress != null ? HttpUtility.HtmlEncode(order.ShippingAddress.Email) : ""));
            tokens.Add(new Token("Order.ShippingFaxNumber", order.ShippingAddress != null ? HttpUtility.HtmlEncode(order.ShippingAddress.FaxNumber) : ""));
            tokens.Add(new Token("Order.ShippingAddress1", order.ShippingAddress != null ? HttpUtility.HtmlEncode(order.ShippingAddress.Address1) : "", true));
            tokens.Add(new Token("Order.ShippingAddress2", order.ShippingAddress != null ? HttpUtility.HtmlEncode(order.ShippingAddress.Address2) : "", true));
            tokens.Add(new Token("Order.ShippingCity", order.ShippingAddress != null ? HttpUtility.HtmlEncode(order.ShippingAddress.City) : "", true));
            tokens.Add(new Token("Order.ShippingStateProvince", order.ShippingAddress != null && order.ShippingAddress.StateProvince != null ? HttpUtility.HtmlEncode(order.ShippingAddress.StateProvince.Name) : "", true));
            tokens.Add(new Token("Order.ShippingZipPostalCode", order.ShippingAddress != null ? HttpUtility.HtmlEncode(order.ShippingAddress.ZipPostalCode) : ""));
            tokens.Add(new Token("Order.ShippingCountry", order.ShippingAddress != null && order.ShippingAddress.Country != null ? HttpUtility.HtmlEncode(order.ShippingAddress.Country.Name) : "", true));

            tokens.Add(new Token("Order.ShippingTaxNo", order.ShippingAddress != null && order.ShippingAddress.IsEnterprise && order.ShippingAddress.TaxNo != null ? HttpUtility.HtmlEncode(order.ShippingAddress.TaxNo) : "", true));
            tokens.Add(new Token("Order.ShippingTaxOffice", order.ShippingAddress != null && order.ShippingAddress.IsEnterprise && order.ShippingAddress.TaxOffice != null ? HttpUtility.HtmlEncode(order.ShippingAddress.TaxOffice) : "", true));
            tokens.Add(new Token("Order.ShippingCompany", order.ShippingAddress != null && order.ShippingAddress.IsEnterprise && order.ShippingAddress != null ? HttpUtility.HtmlEncode(order.ShippingAddress.Company) : ""));
            tokens.Add(new Token("Order.TrackingNumber", HttpUtility.HtmlEncode(order.TrackingNumber)));
            
            tokens.Add(new Token("Order.TrackingNumberURL", order.GetTrackingUrl()));
            //TODO:Do it well
            //if (order.ShippingMethod.Trim()=="UPS")
            //    tokens.Add(new Token("Order.TrackingNumberURL", @"http://www.ups.com.tr/WaybillSorgu.aspx?Waybill=" + order.TrackingNumber));
            //else if (order.ShippingMethod.Trim() == "DHL")
            //    tokens.Add(new Token("Order.TrackingNumberURL",@"http://www.dhl.com.tr/content/tr/en/express/tracking.shtml?brand=DHL&AWB=" + order.TrackingNumber));
            
            

            tokens.Add(new Token("Order.VatNumber", HttpUtility.HtmlEncode(order.VatNumber)));


            //TODO: add only one product token
            tokens.Add(new Token("Order.Product(s)", ProductListToHtmlTableTemplate(order, languageId), true));
            tokens.Add(new Token("Order.BisseProduct(s)", ProductListToHtmlTableTemplateBisse(order, languageId), true));
            tokens.Add(new Token("Order.SOProduct(s)", ProductListToHtmlTableTemplateSO(order, languageId), true));

            tokens.Add(new Token("Order.HtmlProduct(s)", ProductListToHtmlTableTemplateHtml(order, languageId, ShowCheckOutAttribute), true));

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

            //TODO add a method for getting URL
            tokens.Add(new Token("Order.OrderURLForCustomer", string.Format("{0}orderdetails/{1}", _webHelper.GetStoreLocation(false), order.Id)));
        }

        //TODO:AF- get picture  well. send product variant info!
        public virtual void AddProductTokens(IList<Token> tokens, Product product)
        {
            tokens.Add(new Token("Product.Name", product.Name));
            tokens.Add(new Token("Product.ShortDescription", product.ShortDescription, true));
            string pictureUrl = _pictureService.GetPictureUrl(product.ProductVariants.First().GetDefaultProductVariantPicture(_pictureService).Id, _mediaSettings.ProductThumbPictureSize, true);
            tokens.Add(new Token("Product.PictureUrl", pictureUrl, true));
            //TODO add a method for getting URL
            var productUrl = string.Format("{0}p/{1}/{2}", _webHelper.GetStoreLocation(false), product.Id, product.GetSeName());
            tokens.Add(new Token("Product.ProductURLForCustomer", productUrl, true));
        }

        public virtual void AddStoreTokens(IList<Token> tokens)
        {
            tokens.Add(new Token("Store.Name", _storeSettings.StoreName));
            tokens.Add(new Token("Store.URL", _storeSettings.StoreUrl, true));
            var defaultEmailAccount = _emailAccountService.GetEmailAccountById(_emailAccountSettings.DefaultEmailAccountId) ??
                                      _emailAccountService.GetAllEmailAccounts().FirstOrDefault();
            tokens.Add(new Token("Store.Email", defaultEmailAccount.Email));
            tokens.Add(new Token("Store.Path", _webHelper.GetAbsolutePath("/_img/"), true));
        }

        public virtual void AddCustomerTokens(IList<Token> tokens, Customer customer, string fullName)
        {
            tokens.Add(new Token("Customer.Email", customer.Email));
            tokens.Add(new Token("Customer.Username", customer.Username));
            tokens.Add(new Token("Customer.FullName", string.IsNullOrWhiteSpace(fullName) ? customer.GetFullName() : fullName));
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

    }
}
        #endregion