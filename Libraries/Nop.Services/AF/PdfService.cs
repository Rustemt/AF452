using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Html;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Payments;
using System.Globalization;
using System.Text;
using Nop.Core.Domain.Customers;

namespace Nop.Services.Common
{
    /// <summary>
    /// PDF service
    /// </summary>
    public partial class PdfService : IPdfService
    {

        /// <summary>
        /// Print an order to PDF
        /// </summary>
        /// <param name="orders">Orders</param>
        /// <param name="lang">Language</param>
        /// <param name="filePath">File path</param>
        public virtual void PrintInvoiceToPdf(IList<Order> orders, Language lang, string filePath)
        {
            if (orders == null)
                throw new ArgumentNullException("orders");

            if (lang == null)
                throw new ArgumentNullException("lang");

            if (String.IsNullOrEmpty(filePath))
                throw new ArgumentNullException("filePath");

            var pageSize = PageSize.A4;

            if (_pdfSettings.LetterPageSizeEnabled)
            {
                pageSize = PageSize.LETTER;
            }

            float headerHeight = 140;
            float addressesHeight = 115;
            float misc1Height = 24;
            float misc2Height = 24;
            float productsHeight = 130;//198;
            float orderTotalLeftMargin = 400;
            float orderTotalHeight = 70;
            float tableMargin = 18;

            var doc = new Document(pageSize);
            doc.SetMargins(30, 10, 35, 20);
            PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));
            doc.Open();

            //fonts
            var titleFont = GetFont();
            titleFont.SetStyle(Font.BOLD);
            titleFont.Color = BaseColor.BLACK;
            var font = GetFont();
            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);

            int ordCount = orders.Count;
            int ordNum = 0;

            foreach (var order in orders)
            {
                var cell = new PdfPCell();

                #region Header

                var headerTable = new PdfPTable(1);
                headerTable.WidthPercentage = 100f;
                cell.Border = Rectangle.NO_BORDER;
                cell.FixedHeight = headerHeight;
                headerTable.AddCell(cell);
                doc.Add(headerTable);

                #endregion

                #region Addresses

                var addressTable = new PdfPTable(2);
                addressTable.WidthPercentage = 100f;
                addressTable.SetWidths(new[] { 50, 50 });

                //billing info
                cell = new PdfPCell();
                cell.FixedHeight = addressesHeight;
                cell.Border = Rectangle.NO_BORDER;
                //cell.AddElement(new Paragraph(_localizationService.GetResource("PDFInvoice.BillingInformation", lang.Id), titleFont));
                TextInfo textInfo = new CultureInfo("tr-TR", false).TextInfo;
                if (!String.IsNullOrEmpty(order.BillingAddress.Company))
                    cell.AddElement(new Paragraph("   " + String.Format(_localizationService.GetResource("PDFInvoice.Company", lang.Id), textInfo.ToTitleCase(order.BillingAddress.Company.ToLower())), font));

                cell.AddElement(new Paragraph(("   " + textInfo.ToTitleCase(order.BillingAddress.FirstName.ToLower()) + " " + textInfo.ToTitleCase(order.BillingAddress.LastName.ToLower())), font));
                cell.AddElement(new Paragraph("   " + order.BillingAddress.PhoneNumber, font));
                if (!String.IsNullOrEmpty(order.BillingAddress.FaxNumber))
                    cell.AddElement(new Paragraph("   " + order.BillingAddress.FaxNumber, font));
                cell.AddElement(new Paragraph("   " + textInfo.ToTitleCase(order.BillingAddress.Address1.ToLower()), font));
                if (!String.IsNullOrEmpty(order.BillingAddress.Address2))
                    cell.AddElement(new Paragraph("   " +textInfo.ToTitleCase( order.BillingAddress.Address2.ToLower()), font));

                cell.AddElement(new Paragraph("   " + String.Format("{0}, {1} {2}", textInfo.ToTitleCase(order.BillingAddress.City.ToLower()), order.BillingAddress.StateProvince != null ? textInfo.ToTitleCase(order.BillingAddress.StateProvince.GetLocalized(x => x.Name).ToLower()) : "", order.ShippingAddress.ZipPostalCode != null ? order.ShippingAddress.ZipPostalCode : ""), font));
                cell.AddElement(new Paragraph("   " + String.Format("{0}", textInfo.ToTitleCase(order.BillingAddress.Country != null ? order.BillingAddress.Country.GetLocalized(x => x.Name).ToLower() : "")), font));


                //payment method
                //var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(order.PaymentMethodSystemName);
                //string paymentMethodStr = paymentMethod != null ? paymentMethod.PluginDescriptor.FriendlyName : order.PaymentMethodSystemName;
                //if (!String.IsNullOrEmpty(paymentMethodStr))
                //{
                //    cell.AddElement(new Paragraph(" "));
                //    cell.AddElement(new Paragraph("   " + String.Format(_localizationService.GetResource("PDFInvoice.PaymentMethod", lang.Id), paymentMethodStr), font));
                //    cell.AddElement(new Paragraph());
                //}
                addressTable.AddCell(cell);

                //shipping info
                if (order.ShippingStatus != ShippingStatus.ShippingNotRequired)
                {
                    if (order.ShippingAddress == null)
                        throw new NopException(string.Format("Shipping is required, but address is not available. Order ID = {0}", order.Id));
                    cell = new PdfPCell();
                    cell.FixedHeight = addressesHeight;
                    cell.Border = Rectangle.NO_BORDER;

                    //cell.AddElement(new Paragraph(_localizationService.GetResource("PDFInvoice.ShippingInformation", lang.Id), titleFont));
                    if (!String.IsNullOrEmpty(order.ShippingAddress.Company))
                        cell.AddElement(new Paragraph("    " + textInfo.ToTitleCase( order.ShippingAddress.Company.ToLower()), font));

                    cell.AddElement(new Paragraph("    " + (textInfo.ToTitleCase(order.ShippingAddress.FirstName.ToLower()) + " " + textInfo.ToTitleCase(order.ShippingAddress.LastName.ToLower())), font));
                    cell.AddElement(new Paragraph("    " + order.ShippingAddress.PhoneNumber.ToUpper(), font));
                    if (!String.IsNullOrEmpty(order.ShippingAddress.FaxNumber))
                        cell.AddElement(new Paragraph("    " + order.ShippingAddress.FaxNumber, font));
                    cell.AddElement(new Paragraph("    " + textInfo.ToTitleCase(order.ShippingAddress.Address1.ToLower()), font));
                    if (!String.IsNullOrEmpty(order.ShippingAddress.Address2))
                        cell.AddElement(new Paragraph("    " + textInfo.ToTitleCase(order.ShippingAddress.Address2.ToLower()), font));
                    cell.AddElement(new Paragraph("    " + String.Format("{0}, {1} {2}", textInfo.ToTitleCase(order.ShippingAddress.City.ToLower()), order.ShippingAddress.StateProvince != null ? textInfo.ToTitleCase(order.ShippingAddress.StateProvince.GetLocalized(x => x.Name).ToLower()) : "", order.ShippingAddress.ZipPostalCode != null ? order.ShippingAddress.ZipPostalCode : ""), font));
                    
                    cell.AddElement(new Paragraph("    " + String.Format("{0}", order.ShippingAddress.Country != null ? textInfo.ToTitleCase( order.ShippingAddress.Country.GetLocalized(x => x.Name).ToLower()) : ""), font));
                    //cell.AddElement(new Paragraph(" "));
                    //cell.AddElement(new Paragraph("   " + String.Format(_localizationService.GetResource("PDFInvoice.ShippingMethod", lang.Id), order.ShippingMethod), font));
                    //cell.AddElement(new Paragraph());

                    addressTable.AddCell(cell);
                }
                else
                {
                    cell = new PdfPCell(new Phrase(" "));
                    cell.FixedHeight = addressesHeight;
                    cell.Border = Rectangle.NO_BORDER;
                    addressTable.AddCell(cell);
                }
                doc.Add(addressTable);

                #endregion

                #region misc1

                var miscTable1 = new PdfPTable(4);
                miscTable1.SpacingBefore = tableMargin;
                miscTable1.WidthPercentage = 100f;
                miscTable1.SetWidths(new[] { 38, 24, 20, 18 });

                //VAT number  
                cell = new PdfPCell();
                cell.FixedHeight = misc1Height;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.Border = Rectangle.NO_BORDER;

                if (!String.IsNullOrEmpty(order.BillingAddress.TaxOffice))
                    cell.AddElement(new Paragraph("   " + order.BillingAddress.TaxOffice+"/"+order.BillingAddress.TaxNo, font));
                if (!String.IsNullOrEmpty(order.VatNumber))
                    cell.AddElement(new Paragraph("   " + order.VatNumber, font));
                miscTable1.AddCell(cell);

                //Shipping Date
                cell = new PdfPCell();
                cell.HorizontalAlignment = Element.ALIGN_MIDDLE;
                cell.Border = Rectangle.NO_BORDER;
                cell.AddElement(new Paragraph(String.Format("{0:d/M/yyyy}", _dateTimeHelper.ConvertToUserTime(DateTime.UtcNow, DateTimeKind.Utc)), font));
                miscTable1.AddCell(cell);

                //invoice date
                cell = new PdfPCell();
                cell.HorizontalAlignment = Element.ALIGN_MIDDLE;
                cell.Border = Rectangle.NO_BORDER;
                cell.AddElement(new Paragraph(String.Format("{0:d/M/yyyy}", _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc)), font));
                miscTable1.AddCell(cell);

                //invoice no
                cell = new PdfPCell();
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Border = Rectangle.NO_BORDER;
                cell.AddElement(new Paragraph(" ", font));
                miscTable1.AddCell(cell);

                doc.Add(miscTable1);

                #endregion misc

                #region misc2

                var miscTable2 = new PdfPTable(3);
                miscTable2.SpacingBefore = tableMargin;
                miscTable2.WidthPercentage = 100f;
                miscTable2.SetWidths(new[] { 37, 38, 25 });

                //customer no
                cell = new PdfPCell();
                cell.FixedHeight = misc2Height;
                cell.Border = Rectangle.NO_BORDER;
                cell.AddElement(new Paragraph(order.Customer.Id.ToString(), font));
                miscTable2.AddCell(cell);
                //order no
                cell = new PdfPCell();
                cell.HorizontalAlignment = Element.ALIGN_MIDDLE;
                cell.Border = Rectangle.NO_BORDER;
                cell.AddElement(new Paragraph(order.Id.ToString(), font));
                miscTable2.AddCell(cell);
                //order date 
                cell = new PdfPCell();
                cell.HorizontalAlignment = Element.ALIGN_MIDDLE;
                cell.Border = Rectangle.NO_BORDER;
                cell.AddElement(new Paragraph(String.Format("{0:d/M/yyyy}", _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc)), font));
                miscTable2.AddCell(cell);

                doc.Add(miscTable2);
                #endregion misc2

                var mainTable = new PdfPTable(1);
                mainTable.SpacingBefore = tableMargin;
                mainTable.WidthPercentage = 100f;
                mainTable.SetWidths(new[] { 100 });

                #region Products
                //products

                var orderProductVariants = _orderService.GetAllOrderProductVariants(order.Id, null, null, null, null, null, null);

                var productsTable = new PdfPTable(7);
                productsTable.WidthPercentage = 100f;
                productsTable.SetWidths(new[] { 16, 34, 7, 7, 7, 14, 15 });


                for (int i = 0; i < orderProductVariants.Count; i++)
                {
                    var orderProductVariant = orderProductVariants[i];
                    var pv = orderProductVariant.ProductVariant;

                    //SKU
                    cell = new PdfPCell(new Phrase(pv.Sku, font));
                    cell.Border = Rectangle.NO_BORDER;

                    productsTable.AddCell(cell);

                    //product name
                    string name = "";
                    if (!String.IsNullOrEmpty(pv.GetLocalized(x => x.Name)))
                        name = pv.GetLocalized(x => x.Name);
                    else
                        name = pv.Product.GetLocalized(x => x.Name);
                    cell = new PdfPCell(new Phrase(name, font));
                    cell.Border = Rectangle.NO_BORDER;

                    productsTable.AddCell(cell);

                    //attributes text
                    cell = new PdfPCell(new Phrase(""));
                    cell.Border = Rectangle.NO_BORDER;
                    // var attributesParagraph = new Paragraph(HtmlHelper.ConvertHtmlToPlainText(orderProductVariant.AttributeDescription, true), font);

                    productsTable.AddCell(cell);

                    //tax

                    var taxRate = _taxService.GetTaxRate(pv, order.Customer);
                    cell = new PdfPCell(new Phrase(string.Format("%{0}", taxRate), font));
                    cell.Border = Rectangle.NO_BORDER;
                    productsTable.AddCell(cell);

                    //qty
                    cell = new PdfPCell(new Phrase(orderProductVariant.Quantity.ToString(), font));
                    cell.Border = Rectangle.NO_BORDER;
                    productsTable.AddCell(cell);

                    //price
                    string unitPrice = string.Empty;
                    switch (order.CustomerTaxDisplayType)
                    {
                        case TaxDisplayType.ExcludingTax:
                            {
                                var opvUnitPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderProductVariant.UnitPriceExclTax, order.CurrencyRate);
                                unitPrice = _priceFormatter.FormatPrice(opvUnitPriceExclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, lang, false);
                            }
                            break;
                        case TaxDisplayType.IncludingTax:
                            {
                                var opvUnitPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderProductVariant.UnitPriceExclTax, order.CurrencyRate);
                                unitPrice = _priceFormatter.FormatPrice(opvUnitPriceInclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, lang, false);
                            }
                            break;
                    }
                    cell = new PdfPCell(new Phrase(unitPrice, font));
                    cell.Border = Rectangle.NO_BORDER;
                    productsTable.AddCell(cell);



                    //total
                    string subTotal = string.Empty;
                    switch (order.CustomerTaxDisplayType)
                    {
                        case TaxDisplayType.ExcludingTax:
                            {
                                var opvPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderProductVariant.PriceExclTax, order.CurrencyRate);
                                subTotal = _priceFormatter.FormatPrice(opvPriceExclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, lang, false);
                            }
                            break;
                        case TaxDisplayType.IncludingTax:
                            {
                                var opvPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderProductVariant.PriceExclTax, order.CurrencyRate);
                                subTotal = _priceFormatter.FormatPrice(opvPriceInclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, lang, false);
                            }
                            break;
                    }
                    cell = new PdfPCell(new Phrase(subTotal, font));
                    cell.Border = Rectangle.NO_BORDER;
                    productsTable.AddCell(cell);
                }

                var mainCell = new PdfPCell(productsTable);
                mainCell.FixedHeight = productsHeight;
                mainCell.Border = Rectangle.NO_BORDER;

                //doc.Add(productsTable);

                #endregion

                #region Checkout attributes

                //if (!String.IsNullOrEmpty(order.CheckoutAttributeDescription))
                //{
                //    doc.Add(new Paragraph(" "));
                //    string attributes = HtmlHelper.ConvertHtmlToPlainText(order.CheckoutAttributeDescription, true);
                //    var pCheckoutAttributes = new Paragraph(attributes, font);
                //    pCheckoutAttributes.Alignment = Element.ALIGN_RIGHT;
                //    doc.Add(pCheckoutAttributes);
                //    doc.Add(new Paragraph(" "));
                //}

                #endregion

                #region Totals

                //subtotal
                var orderTotalsTable = new PdfPTable(1);
                //orderTotalsTable.WidthPercentage = 90f;
                //orderTotalsTable.SetWidths(new[] { 50, 50 });
                //switch (order.CustomerTaxDisplayType)
                //{
                //    case TaxDisplayType.ExcludingTax:
                //        {
                var orderSubtotalExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubtotalExclTax, order.CurrencyRate);
                string orderSubtotalExclTaxStr = _priceFormatter.FormatPrice(orderSubtotalExclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, lang, false);
                int totalLength=(_localizationService.GetResource("PDFInvoice.Sub-Total", lang.Id) + orderSubtotalExclTaxStr).Length;
                int sNumber = 0;
                if (totalLength <= 30)
                    sNumber = 30 - totalLength;
                string sSpace = new String(' ', sNumber);
                cell = new PdfPCell(new Phrase(String.Format("{0} {1}", _localizationService.GetResource("PDFInvoice.Sub-Total", lang.Id), sSpace + orderSubtotalExclTaxStr), font));
                cell.Border = Rectangle.NO_BORDER;
                cell.PaddingLeft = orderTotalLeftMargin;
                orderTotalsTable.AddCell(cell);
                //        }
                //        break;
                //    case TaxDisplayType.IncludingTax:
                //        {
                //            var orderSubtotalInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubtotalInclTax, order.CurrencyRate);
                //            string orderSubtotalInclTaxStr = _priceFormatter.FormatPrice(orderSubtotalInclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, lang, true);
                //            cell = new PdfPCell(new Phrase(String.Format("{0} {1}", _localizationService.GetResource("PDFInvoice.Sub-Total", lang.Id), orderSubtotalInclTaxStr), font));
                //            cell.Border = Rectangle.NO_BORDER;
                //            cell.PaddingLeft = orderTotalLeftMargin;
                //            orderTotalsTable.AddCell(cell);
                //        }
                //        break;
                //}
                //discount (applied to order subtotal)
                if (order.OrderSubTotalDiscountExclTax > decimal.Zero)
                {
                    switch (order.CustomerTaxDisplayType)
                    {
                        case TaxDisplayType.ExcludingTax:
                            {
                                var orderSubTotalDiscountExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubTotalDiscountExclTax, order.CurrencyRate);
                                string orderSubTotalDiscountInCustomerCurrencyStr = _priceFormatter.FormatPrice(-orderSubTotalDiscountExclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, lang, false);

                                totalLength = (_localizationService.GetResource("PDFInvoice.Discount", lang.Id) + orderSubTotalDiscountInCustomerCurrencyStr).Length;
                                sNumber = 0;
                                if (totalLength <= 32)
                                    sNumber = 32 - totalLength;
                                sSpace = new String(' ', sNumber);
                                cell = new PdfPCell(new Phrase(String.Format("{0} {1}", _localizationService.GetResource("PDFInvoice.Discount", lang.Id), sSpace + orderSubTotalDiscountInCustomerCurrencyStr), font));
                                cell.Border = Rectangle.NO_BORDER;
                                cell.PaddingLeft = orderTotalLeftMargin;
                                orderTotalsTable.AddCell(cell);

                            }
                            break;
                        case TaxDisplayType.IncludingTax:
                            {
                                var orderSubTotalDiscountInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubTotalDiscountInclTax, order.CurrencyRate);
                                string orderSubTotalDiscountInCustomerCurrencyStr = _priceFormatter.FormatPrice(-orderSubTotalDiscountInclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, lang, true);

                                totalLength = (_localizationService.GetResource("PDFInvoice.Discount", lang.Id) + orderSubTotalDiscountInCustomerCurrencyStr).Length;
                                sNumber = 0;
                                if (totalLength <= 32)
                                    sNumber = 32 - totalLength;
                                sSpace = new String(' ', sNumber);
                                cell = new PdfPCell(new Phrase(String.Format("{0} {1}", _localizationService.GetResource("PDFInvoice.Discount", lang.Id), sSpace + orderSubTotalDiscountInCustomerCurrencyStr), font));
                                cell.Border = Rectangle.NO_BORDER;
                                cell.PaddingLeft = orderTotalLeftMargin;
                                orderTotalsTable.AddCell(cell);


                            }
                            break;
                    }
                }

                //shipping
                if (order.ShippingStatus != ShippingStatus.ShippingNotRequired)
                {
                    switch (order.CustomerTaxDisplayType)
                    {
                        case TaxDisplayType.ExcludingTax:
                            {
                                var orderShippingExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingExclTax, order.CurrencyRate);
                                string orderShippingExclTaxStr = _priceFormatter.FormatShippingPrice(orderShippingExclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, lang, false);

                                totalLength = (_localizationService.GetResource("PDFInvoice.Shipping", lang.Id) + orderShippingExclTaxStr).Length;
                                sNumber = 0;
                                if (totalLength <= 32)
                                    sNumber = 32 - totalLength;
                                sSpace = new String(' ', sNumber);
                                cell = new PdfPCell(new Phrase(String.Format("{0} {1}", _localizationService.GetResource("PDFInvoice.Shipping", lang.Id), sSpace + orderShippingExclTaxStr), font));
                                cell.Border = Rectangle.NO_BORDER;
                                cell.PaddingLeft = orderTotalLeftMargin;
                                orderTotalsTable.AddCell(cell);

                            }
                            break;
                        case TaxDisplayType.IncludingTax:
                            {
                                var orderShippingInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingInclTax, order.CurrencyRate);
                                string orderShippingInclTaxStr = _priceFormatter.FormatShippingPrice(orderShippingInclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, lang, true);

                                totalLength = (_localizationService.GetResource("PDFInvoice.Shipping", lang.Id) + orderShippingInclTaxStr).Length;
                                sNumber = 0;
                                if (totalLength <= 32)
                                    sNumber = 32 - totalLength;
                                sSpace = new String(' ', sNumber);
                                cell = new PdfPCell(new Phrase(String.Format("{0} {1}", _localizationService.GetResource("PDFInvoice.Shipping", lang.Id), sSpace + orderShippingInclTaxStr), font));
                                cell.Border = Rectangle.NO_BORDER;
                                cell.PaddingLeft = orderTotalLeftMargin;
                                orderTotalsTable.AddCell(cell);
                            }
                            break;
                    }
                }

                //payment fee
                if (order.PaymentMethodAdditionalFeeExclTax > decimal.Zero)
                {
                    switch (order.CustomerTaxDisplayType)
                    {
                        case TaxDisplayType.ExcludingTax:
                            {
                                var paymentMethodAdditionalFeeExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeExclTax, order.CurrencyRate);
                                string paymentMethodAdditionalFeeExclTaxStr = _priceFormatter.FormatPaymentMethodAdditionalFee(paymentMethodAdditionalFeeExclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, lang, false);

                                totalLength = (_localizationService.GetResource("PDFInvoice.PaymentMethodAdditionalFee", lang.Id) + paymentMethodAdditionalFeeExclTaxStr).Length;
                                sNumber = 0;
                                if (totalLength <= 32)
                                    sNumber = 32 - totalLength;
                                sSpace = new String(' ', sNumber);
                                cell = new PdfPCell(new Phrase(String.Format("{0} {1}", _localizationService.GetResource("PDFInvoice.PaymentMethodAdditionalFee", lang.Id), sSpace + paymentMethodAdditionalFeeExclTaxStr), font));
                                cell.Border = Rectangle.NO_BORDER;
                                cell.PaddingLeft = orderTotalLeftMargin;
                                orderTotalsTable.AddCell(cell);

                            }
                            break;
                        case TaxDisplayType.IncludingTax:
                            {
                                var paymentMethodAdditionalFeeInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeInclTax, order.CurrencyRate);
                                string paymentMethodAdditionalFeeInclTaxStr = _priceFormatter.FormatPaymentMethodAdditionalFee(paymentMethodAdditionalFeeInclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, lang, true);


                                totalLength = (_localizationService.GetResource("PDFInvoice.PaymentMethodAdditionalFee", lang.Id) + paymentMethodAdditionalFeeInclTaxStr).Length;
                                sNumber = 0;
                                if (totalLength <= 32)
                                    sNumber = 32 - totalLength;
                                sSpace = new String(' ', sNumber);
                                cell = new PdfPCell(new Phrase(String.Format("{0} {1}", _localizationService.GetResource("PDFInvoice.PaymentMethodAdditionalFee", lang.Id),sSpace+ paymentMethodAdditionalFeeInclTaxStr), font));
                                cell.Border = Rectangle.NO_BORDER;
                                cell.PaddingLeft = orderTotalLeftMargin;
                                orderTotalsTable.AddCell(cell);
                            }
                            break;
                    }
                }

                //tax
                string taxStr = string.Empty;
                var taxRates = new SortedDictionary<decimal, decimal>();
                bool displayTax = true;
                bool displayTaxRates = true;
                if (_taxSettings.HideTaxInOrderSummary && order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    displayTax = false;
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
                        taxRates = order.TaxRatesDictionary;

                        displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Count > 0;
                        displayTax = !displayTaxRates;

                        var orderTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTax, order.CurrencyRate);
                        taxStr = _priceFormatter.FormatPrice(orderTaxInCustomerCurrency, false, order.CustomerCurrencyCode, false, lang);
                    }
                }
                if (displayTax)
                {

                    totalLength = (_localizationService.GetResource("PDFInvoice.Tax", lang.Id) + taxStr).Length;
                    sNumber = 0;
                    if (totalLength <= 32)
                        sNumber = 32 - totalLength;
                    sSpace = new String(' ', sNumber);
                    cell = new PdfPCell(new Phrase(String.Format("{0} {1}", _localizationService.GetResource("PDFInvoice.Tax", lang.Id), sSpace + taxStr), font));
                    cell.Border = Rectangle.NO_BORDER;
                    cell.PaddingLeft = orderTotalLeftMargin;
                    orderTotalsTable.AddCell(cell);


                }
                if (displayTaxRates)
                {
                    foreach (var item in taxRates)
                    {
                        string taxRate = String.Format(_localizationService.GetResource("PDFInvoice.TaxRate"), _priceFormatter.FormatTaxRate(item.Key));
                        string taxValue = _priceFormatter.FormatPrice(_currencyService.ConvertCurrency(item.Value, order.CurrencyRate), false, order.CustomerCurrencyCode, false, lang);

                        cell = new PdfPCell(new Phrase(String.Format("{0} {1}", taxRate, taxValue), font));
                        cell.Border = Rectangle.NO_BORDER;
                        cell.PaddingLeft = orderTotalLeftMargin;
                        orderTotalsTable.AddCell(cell);
                    }
                }

                //discount (applied to order total)
                if (order.OrderDiscount > decimal.Zero)
                {
                    var orderDiscountInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderDiscount, order.CurrencyRate);
                    string orderDiscountInCustomerCurrencyStr = _priceFormatter.FormatPrice(-orderDiscountInCustomerCurrency, false, order.CustomerCurrencyCode, false, lang);


                    totalLength = (_localizationService.GetResource("PDFInvoice.Discount", lang.Id) + orderDiscountInCustomerCurrencyStr).Length;
                    sNumber = 0;
                    if (totalLength <= 32)
                        sNumber = 32 - totalLength;
                    sSpace = new String(' ', sNumber);
                    cell = new PdfPCell(new Phrase(String.Format("{0} {1}", _localizationService.GetResource("PDFInvoice.Discount", lang.Id), sSpace + orderDiscountInCustomerCurrencyStr), font));
                    cell.Border = Rectangle.NO_BORDER;
                    cell.PaddingLeft = orderTotalLeftMargin;
                    orderTotalsTable.AddCell(cell);


                }

                ////gift cards
                //foreach (var gcuh in order.GiftCardUsageHistory)
                //{
                //    string gcTitle = string.Format(_localizationService.GetResource("PDFInvoice.GiftCardInfo", lang.Id), gcuh.GiftCard.GiftCardCouponCode);
                //    string gcAmountStr = _priceFormatter.FormatPrice(-(_currencyService.ConvertCurrency(gcuh.UsedValue, order.CurrencyRate)), false, order.CustomerCurrencyCode, false, lang);

                //    var p = new Paragraph(String.Format("{0} {1}", gcTitle, gcAmountStr), font);
                //    p.Alignment = Element.ALIGN_RIGHT;
                //    doc.Add(p);
                //}

                ////reward points
                //if (order.RedeemedRewardPointsEntry != null)
                //{
                //    string rpTitle = string.Format(_localizationService.GetResource("PDFInvoice.RewardPoints", lang.Id), -order.RedeemedRewardPointsEntry.Points);
                //    string rpAmount = _priceFormatter.FormatPrice(-(_currencyService.ConvertCurrency(order.RedeemedRewardPointsEntry.UsedAmount, order.CurrencyRate)), false, order.CustomerCurrencyCode, false, lang);

                //    var p = new Paragraph(String.Format("{0} {1}", rpTitle, rpAmount), font);
                //    p.Alignment = Element.ALIGN_RIGHT;
                //    doc.Add(p);
                //}

                //order total
                var orderTotalInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
                string orderTotalStr = _priceFormatter.FormatPrice(orderTotalInCustomerCurrency, false, order.CustomerCurrencyCode, false, lang);


                totalLength = (_localizationService.GetResource("PDFInvoice.OrderTotal", lang.Id) + orderTotalStr).Length;
                sNumber = 0;
                if (totalLength <= 32)
                    sNumber = 32 - totalLength;
                sSpace = new String(' ', sNumber);
                cell = new PdfPCell(new Phrase(String.Format("{0} {1}", _localizationService.GetResource("PDFInvoice.OrderTotal", lang.Id), sSpace + orderTotalStr), font));
                cell.Border = Rectangle.NO_BORDER;
                cell.PaddingLeft = orderTotalLeftMargin;
                orderTotalsTable.AddCell(cell);


                #endregion

                #region Order notes

                //if (_pdfSettings.RenderOrderNotes)
                //{
                //    var orderNotes = order.OrderNotes
                //        .Where(on => on.DisplayToCustomer)
                //        .OrderByDescending(on => on.CreatedOnUtc)
                //        .ToList();
                //    if (orderNotes.Count > 0)
                //    {
                //        doc.Add(new Paragraph(_localizationService.GetResource("PDFInvoice.OrderNotes", lang.Id), titleFont));

                //        doc.Add(new Paragraph(" "));

                //        var notesTable = new PdfPTable(2);
                //        notesTable.WidthPercentage = 100f;
                //        notesTable.SetWidths(new[] { 32, 70 });

                //        //created on
                //        cell = new PdfPCell(new Phrase(_localizationService.GetResource("PDFInvoice.OrderNotes.CreatedOn", lang.Id), font));
                //        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                //        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                //        notesTable.AddCell(cell);

                //        //note
                //        cell = new PdfPCell(new Phrase(_localizationService.GetResource("PDFInvoice.OrderNotes.Note", lang.Id), font));
                //        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                //        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                //        notesTable.AddCell(cell);

                //        foreach (var orderNote in orderNotes)
                //        {
                //            cell = new PdfPCell();
                //            cell.AddElement(new Paragraph(_dateTimeHelper.ConvertToUserTime(orderNote.CreatedOnUtc, DateTimeKind.Utc).ToString(), font));
                //            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                //            notesTable.AddCell(cell);

                //            cell = new PdfPCell();
                //            cell.AddElement(new Paragraph(HtmlHelper.ConvertHtmlToPlainText(HtmlHelper.FormatText(orderNote.Note, false, true, false, false, false, false), true), font));
                //            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                //            notesTable.AddCell(cell);
                //        }
                //        doc.Add(notesTable);
                //    }
                //}

                #endregion

                mainTable.AddCell(mainCell);



                var totalCell = new PdfPCell(orderTotalsTable);
                totalCell.FixedHeight = orderTotalHeight;
                totalCell.Border = Rectangle.NO_BORDER;


                mainTable.AddCell(totalCell);
                doc.Add(mainTable);
                //doc.Add(orderTotalsTable);

                #region TotalString

                //order total
                var orderTotalInCustomerCurrencyTotal = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
                //string orderTotalStr = _priceFormatter.FormatPrice(orderTotalInCustomerCurrency, false, order.CustomerCurrencyCode, false, lang);

                var mainPart = (int)Decimal.Floor(orderTotalInCustomerCurrencyTotal);
                var decimalPart = (int)((Math.Round(orderTotalInCustomerCurrencyTotal, 2) - mainPart) * 100);
                string totalString = "";
                if (decimalPart > 0)
                    totalString = string.Format("{0} {1} {2} {3}",
                        textInfo.ToTitleCase(NumberToWordsHelper.ToWritten(mainPart, lang).ToLower()),
                        textInfo.ToTitleCase(NumberToWordsHelper.CurrencyMainPartText(order.CustomerCurrencyCode, _localizationService).ToLower()),
                        textInfo.ToTitleCase(NumberToWordsHelper.ToWritten(decimalPart, lang).ToLower()),
                        textInfo.ToTitleCase(NumberToWordsHelper.CurrencyDecimalPartText(order.CustomerCurrencyCode, _localizationService).ToLower())
                        );
                else
                    totalString = string.Format("{0} {1}",
                   textInfo.ToTitleCase(NumberToWordsHelper.ToWritten(mainPart, lang).ToLower()),
                   textInfo.ToTitleCase(NumberToWordsHelper.CurrencyMainPartText(order.CustomerCurrencyCode, _localizationService).ToLower())
                    );

                var totalTable = new PdfPTable(2);
                totalTable.WidthPercentage = 100f;
                totalTable.SetWidths(new[] { 10, 90 });

                //total
                cell = new PdfPCell(new Phrase("", font));
                cell.Border = Rectangle.NO_BORDER;
                totalTable.AddCell(cell);

                cell = new PdfPCell(new Phrase(totalString, font));
                cell.Border = Rectangle.NO_BORDER;

                totalTable.AddCell(cell);

                doc.Add(totalTable);
                #endregion TotalString

                ordNum++;
                if (ordNum < ordCount)
                {
                    doc.NewPage();
                }
            }
            doc.Close();
        }
        public virtual void PrintInvoiceToPdfBisse(IList<Order> orders, Language lang, string filePath)
        {
            if (orders == null)
                throw new ArgumentNullException("orders");

            if (lang == null)
                throw new ArgumentNullException("lang");

            if (String.IsNullOrEmpty(filePath))
                throw new ArgumentNullException("filePath");

            var pageSize = PageSize.A4;

            if (_pdfSettings.LetterPageSizeEnabled)
            {
                pageSize = PageSize.LETTER;
            }


            float addressesHeight = 163;

            float orderTotalLeftMargin = 400;
            float orderTotalHeight = 70;


            var doc = new Document(pageSize);
            doc.SetMargins(50, 0, 30, 0);
            PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));
            doc.Open();

            //fonts
            var titleFont = GetFont();
            titleFont.SetStyle(Font.BOLD);
            titleFont.Color = BaseColor.BLACK;
            var font = GetFont();
            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);

            int ordCount = orders.Count;
            int ordNum = 0;

            foreach (var order in orders)
            {
                var cell = new PdfPCell();

                var addressTable = new PdfPTable(3);
                addressTable.WidthPercentage = 100f;

                addressTable.SetWidths(new[] { 55, 15, 30 });

                //billing info
                cell = new PdfPCell();
                cell.FixedHeight = addressesHeight;
                cell.Border = Rectangle.NO_BORDER;
                cell.PaddingLeft = 10;
                cell.PaddingTop = 30;
                //cell.AddElement(new Paragraph(_localizationService.GetResource("PDFInvoice.BillingInformation", lang.Id), titleFont));
                TextInfo textInfo = new CultureInfo("tr-TR", false).TextInfo;
                if (!String.IsNullOrEmpty(order.BillingAddress.Company))
                    cell.AddElement(new Paragraph(String.Format(_localizationService.GetResource("PDFInvoice.Company", lang.Id), textInfo.ToTitleCase(order.BillingAddress.Company.ToLower())), font));

                cell.AddElement(new Phrase((textInfo.ToTitleCase(order.BillingAddress.FirstName.ToLower()) + " " + textInfo.ToTitleCase(order.BillingAddress.LastName.ToLower())), font));
                if (!String.IsNullOrEmpty(order.BillingAddress.FaxNumber))
                    cell.AddElement(new Phrase(order.BillingAddress.FaxNumber, font));
                cell.AddElement(new Phrase(textInfo.ToTitleCase(order.BillingAddress.Address1.ToLower()), font));
                if (!String.IsNullOrEmpty(order.BillingAddress.Address2))
                    cell.AddElement(new Phrase(textInfo.ToTitleCase(order.BillingAddress.Address2.ToLower()), font));
                cell.AddElement(new Phrase(String.Format("{0}, {1} {2}", textInfo.ToTitleCase(order.BillingAddress.City.ToLower()), order.BillingAddress.StateProvince != null ? textInfo.ToTitleCase(order.BillingAddress.StateProvince.GetLocalized(x => x.Name).ToLower()) : "", order.ShippingAddress.ZipPostalCode != null ? order.ShippingAddress.ZipPostalCode : ""), font));
                cell.AddElement(new Phrase(String.Format("{0}", textInfo.ToTitleCase(order.BillingAddress.Country != null ? order.BillingAddress.Country.GetLocalized(x => x.Name).ToLower() : "")), font));
                cell.AddElement(new Phrase(order.BillingAddress.PhoneNumber, font));


                addressTable.AddCell(cell);
                cell = new PdfPCell();
                cell.Border = Rectangle.NO_BORDER;
                addressTable.AddCell(cell);

                cell = new PdfPCell();
                cell.Border = Rectangle.NO_BORDER;
                cell.FixedHeight = addressesHeight;

                cell.AddElement(new Paragraph(String.Format("{0:d/M/yyyy}", _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc)), font));
                cell.AddElement(new Paragraph(String.Format(_dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc).ToString("HH:mm")), font));
                cell.AddElement(new Paragraph(String.Format("{0:d/M/yyyy}", _dateTimeHelper.ConvertToUserTime(DateTime.UtcNow, DateTimeKind.Utc)), font));
                cell.PaddingTop = 90;
                cell.PaddingLeft = 55;
                addressTable.AddCell(cell);
                doc.Add(addressTable);

                var productsTable = new PdfPTable(5);
                productsTable.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.WidthPercentage = 100f;
                productsTable.SetWidths(new[] { 45, 7, 8, 15, 25 });
                cell = new PdfPCell();
                cell.PaddingLeft = 20;
                cell.PaddingTop = 0;
                if (false)
                {
                    cell = new PdfPCell();
                    cell.HorizontalAlignment = Element.ALIGN_MIDDLE;
                    cell.FixedHeight = 20;
                    cell.Border = Rectangle.NO_BORDER;
                    cell.AddElement(new Paragraph("AÇIKLAMA", font));
                    cell.PaddingLeft = 20;
                    productsTable.AddCell(cell);

                    cell = new PdfPCell();
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    cell.FixedHeight = 20;
                    cell.Border = Rectangle.NO_BORDER;
                    cell.AddElement(new Paragraph("MİKTAR", font));
                    productsTable.AddCell(cell);

                    cell = new PdfPCell();
                    cell.VerticalAlignment = Element.ALIGN_CENTER;
                    cell.FixedHeight = 20;
                    cell.Border = Rectangle.NO_BORDER;
                    cell.AddElement(new Paragraph("BİRİM FİYAT", font));
                    productsTable.AddCell(cell);

                    cell = new PdfPCell();
                    cell.VerticalAlignment = Element.ALIGN_CENTER;
                    cell.FixedHeight = 20;
                    cell.Border = Rectangle.NO_BORDER;
                    cell.AddElement(new Paragraph("TOPLAM", font));
                    productsTable.AddCell(cell);
                }
                #region products
                var orderProductVariants = _orderService.GetAllOrderProductVariants(order.Id, null, null, null, null, null, null);
                for (int i = 0; i < orderProductVariants.Count; i++)
                {
                    var orderProductVariant = orderProductVariants[i];
                    var pv = orderProductVariant.ProductVariant;

                    //SKU
                    string name = "";
                    if (!String.IsNullOrEmpty(pv.GetLocalized(x => x.Name)))
                        name = pv.GetLocalized(x => x.Name);
                    else
                        name = pv.Product.GetLocalized(x => x.Name);
                    cell = new PdfPCell(new Phrase(name, font));
                    cell.Border = Rectangle.NO_BORDER;

                    cell = new PdfPCell(new Phrase(pv.Sku + " " + name + " " + orderProductVariant.AttributeDescription.Replace("<br />", " "), font));
                    cell.Border = Rectangle.NO_BORDER;

                    productsTable.AddCell(cell);


                    cell = new PdfPCell();
                    cell.Border = Rectangle.NO_BORDER;
                    productsTable.AddCell(cell);
                    //qty
                    cell = new PdfPCell(new Phrase(orderProductVariant.Quantity.ToString(), font));
                    cell.Border = Rectangle.NO_BORDER;
                    productsTable.AddCell(cell);

                    //price
                    string unitPrice = string.Empty;
                    switch (order.CustomerTaxDisplayType)
                    {
                        case TaxDisplayType.ExcludingTax:
                            {
                                var opvUnitPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderProductVariant.UnitPriceExclTax, order.CurrencyRate);
                                unitPrice = _priceFormatter.FormatPrice(opvUnitPriceExclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, lang, false);
                            }
                            break;
                        case TaxDisplayType.IncludingTax:
                            {
                                var opvUnitPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderProductVariant.UnitPriceExclTax, order.CurrencyRate);
                                unitPrice = _priceFormatter.FormatPrice(opvUnitPriceInclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, lang, false);
                            }
                            break;
                    }
                    cell = new PdfPCell(new Phrase(unitPrice, font));
                    cell.HorizontalAlignment = Element.ALIGN_MIDDLE;
                    cell.Border = Rectangle.NO_BORDER;
                    productsTable.AddCell(cell);



                    //total
                    string subTotal = string.Empty;
                    switch (order.CustomerTaxDisplayType)
                    {
                        case TaxDisplayType.ExcludingTax:
                            {
                                var opvPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderProductVariant.PriceExclTax, order.CurrencyRate);
                                subTotal = _priceFormatter.FormatPrice(opvPriceExclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, lang, false);
                            }
                            break;
                        case TaxDisplayType.IncludingTax:
                            {
                                var opvPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderProductVariant.PriceExclTax, order.CurrencyRate);
                                subTotal = _priceFormatter.FormatPrice(opvPriceInclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, lang, false);
                            }
                            break;
                    }
                    cell = new PdfPCell(new Phrase(subTotal, font));
                    cell.Border = Rectangle.NO_BORDER;
                    productsTable.AddCell(cell);
                }

                #endregion




                #region Totals
                //subtotal
                var orderTotalsTable = new PdfPTable(3);
                orderTotalsTable.SetWidths(new[] { 35, 5, 60 });
                var orderSubtotalExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubtotalExclTax, order.CurrencyRate);
                string orderSubtotalExclTaxStr = _priceFormatter.FormatPrice(orderSubtotalExclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, lang, false);
                cell = new PdfPCell(new Paragraph(_localizationService.GetResource("PDFInvoice.Sub-Total", lang.Id), font));
                cell.Border = Rectangle.NO_BORDER;
                cell.VerticalAlignment = Element.ALIGN_RIGHT;
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                orderTotalsTable.AddCell(cell);

                cell = new PdfPCell();
                cell.Border = Rectangle.NO_BORDER;
                orderTotalsTable.AddCell(cell);

                cell = new PdfPCell(new Phrase(orderSubtotalExclTaxStr, font));
                cell.Border = Rectangle.NO_BORDER;
                cell.VerticalAlignment = Element.ALIGN_RIGHT;
                orderTotalsTable.AddCell(cell);

                if (order.OrderSubTotalDiscountExclTax > decimal.Zero)
                {
                    switch (order.CustomerTaxDisplayType)
                    {
                        case TaxDisplayType.ExcludingTax:
                            {
                                var orderSubTotalDiscountExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubTotalDiscountExclTax, order.CurrencyRate);
                                string orderSubTotalDiscountInCustomerCurrencyStr = _priceFormatter.FormatPrice(-orderSubTotalDiscountExclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, lang, false);


                                cell = new PdfPCell(new Paragraph(_localizationService.GetResource("PDFInvoice.Discount", lang.Id), font));
                                cell.Border = Rectangle.NO_BORDER;
                                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                orderTotalsTable.AddCell(cell);

                                cell = new PdfPCell();
                                cell.Border = Rectangle.NO_BORDER;
                                orderTotalsTable.AddCell(cell);

                                cell = new PdfPCell(new Phrase(orderSubTotalDiscountInCustomerCurrencyStr, font));
                                cell.Border = Rectangle.NO_BORDER;
                                cell.VerticalAlignment = Element.ALIGN_RIGHT;
                                orderTotalsTable.AddCell(cell);

                            }
                            break;
                        case TaxDisplayType.IncludingTax:
                            {
                                var orderSubTotalDiscountInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubTotalDiscountInclTax, order.CurrencyRate);
                                string orderSubTotalDiscountInCustomerCurrencyStr = _priceFormatter.FormatPrice(-orderSubTotalDiscountInclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, lang, true);


                                cell = new PdfPCell(new Paragraph(_localizationService.GetResource("PDFInvoice.Discount", lang.Id), font));
                                cell.Border = Rectangle.NO_BORDER;
                                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                orderTotalsTable.AddCell(cell);

                                cell = new PdfPCell();
                                cell.Border = Rectangle.NO_BORDER;
                                orderTotalsTable.AddCell(cell);

                                cell = new PdfPCell(new Phrase(orderSubTotalDiscountInCustomerCurrencyStr, font));
                                cell.Border = Rectangle.NO_BORDER;
                                cell.VerticalAlignment = Element.ALIGN_RIGHT;
                                orderTotalsTable.AddCell(cell);

                            }
                            break;
                    }
                }

                //shipping
                if (order.ShippingStatus != ShippingStatus.ShippingNotRequired)
                {
                    switch (order.CustomerTaxDisplayType)
                    {
                        case TaxDisplayType.ExcludingTax:
                            {
                                var orderShippingExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingExclTax, order.CurrencyRate);
                                string orderShippingExclTaxStr = _priceFormatter.FormatShippingPrice(orderShippingExclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, lang, false);

                                cell = new PdfPCell(new Paragraph(_localizationService.GetResource("PDFInvoice.Shipping", lang.Id), font));
                                cell.Border = Rectangle.NO_BORDER;
                                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                orderTotalsTable.AddCell(cell);

                                cell = new PdfPCell();
                                cell.Border = Rectangle.NO_BORDER;
                                orderTotalsTable.AddCell(cell);

                                cell = new PdfPCell(new Phrase(orderShippingExclTaxStr, font));
                                cell.Border = Rectangle.NO_BORDER;
                                cell.VerticalAlignment = Element.ALIGN_RIGHT;
                                orderTotalsTable.AddCell(cell);
                            }
                            break;
                        case TaxDisplayType.IncludingTax:
                            {
                                var orderShippingInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingInclTax, order.CurrencyRate);
                                string orderShippingInclTaxStr = _priceFormatter.FormatShippingPrice(orderShippingInclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, lang, true);

                                cell = new PdfPCell(new Paragraph(_localizationService.GetResource("PDFInvoice.Shipping", lang.Id), font));
                                cell.Border = Rectangle.NO_BORDER;
                                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                orderTotalsTable.AddCell(cell);

                                cell = new PdfPCell();
                                cell.Border = Rectangle.NO_BORDER;
                                orderTotalsTable.AddCell(cell);

                                cell = new PdfPCell(new Phrase(orderShippingInclTaxStr, font));
                                cell.Border = Rectangle.NO_BORDER;
                                cell.VerticalAlignment = Element.ALIGN_RIGHT;
                                orderTotalsTable.AddCell(cell);
                            }
                            break;
                    }
                }

                //payment fee
                if (order.PaymentMethodAdditionalFeeExclTax > decimal.Zero)
                {
                    switch (order.CustomerTaxDisplayType)
                    {
                        case TaxDisplayType.ExcludingTax:
                            {
                                var paymentMethodAdditionalFeeExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeExclTax, order.CurrencyRate);
                                string paymentMethodAdditionalFeeExclTaxStr = _priceFormatter.FormatPaymentMethodAdditionalFee(paymentMethodAdditionalFeeExclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, lang, false);

                                cell = new PdfPCell(new Paragraph(_localizationService.GetResource("PDFInvoice.PaymentMethodAdditionalFee", lang.Id), font));
                                cell.Border = Rectangle.NO_BORDER;
                                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                orderTotalsTable.AddCell(cell);

                                cell = new PdfPCell();
                                cell.Border = Rectangle.NO_BORDER;
                                orderTotalsTable.AddCell(cell);

                                cell = new PdfPCell(new Phrase(paymentMethodAdditionalFeeExclTaxStr, font));
                                cell.Border = Rectangle.NO_BORDER;
                                cell.VerticalAlignment = Element.ALIGN_RIGHT;
                                orderTotalsTable.AddCell(cell);

                            }
                            break;
                        case TaxDisplayType.IncludingTax:
                            {
                                var paymentMethodAdditionalFeeInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeInclTax, order.CurrencyRate);
                                string paymentMethodAdditionalFeeInclTaxStr = _priceFormatter.FormatPaymentMethodAdditionalFee(paymentMethodAdditionalFeeInclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, lang, true);

                                cell = new PdfPCell(new Paragraph(_localizationService.GetResource("PDFInvoice.PaymentMethodAdditionalFee", lang.Id), font));
                                cell.Border = Rectangle.NO_BORDER;
                                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                orderTotalsTable.AddCell(cell);

                                cell = new PdfPCell();
                                cell.Border = Rectangle.NO_BORDER;
                                orderTotalsTable.AddCell(cell);

                                cell = new PdfPCell(new Phrase(paymentMethodAdditionalFeeInclTaxStr, font));
                                cell.Border = Rectangle.NO_BORDER;
                                cell.VerticalAlignment = Element.ALIGN_RIGHT;
                                orderTotalsTable.AddCell(cell);
                            }
                            break;
                    }
                }

                //tax
                string taxStr = string.Empty;
                var taxRates = new SortedDictionary<decimal, decimal>();
                bool displayTax = true;
                bool displayTaxRates = true;
                if (_taxSettings.HideTaxInOrderSummary && order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    displayTax = false;
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
                        taxRates = order.TaxRatesDictionary;
                        displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Count > 0;
                        displayTax = !displayTaxRates;

                        var orderTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTax, order.CurrencyRate);
                        taxStr = _priceFormatter.FormatPrice(orderTaxInCustomerCurrency, false, order.CustomerCurrencyCode, false, lang);
                    }
                }
                if (displayTax)
                {

                    cell = new PdfPCell(new Paragraph(_localizationService.GetResource("PDFInvoice.Tax", lang.Id), font));
                    cell.Border = Rectangle.NO_BORDER;
                    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    orderTotalsTable.AddCell(cell);

                    cell = new PdfPCell();
                    cell.Border = Rectangle.NO_BORDER;
                    orderTotalsTable.AddCell(cell);

                    cell = new PdfPCell(new Phrase(taxStr, font));
                    cell.Border = Rectangle.NO_BORDER;
                    cell.VerticalAlignment = Element.ALIGN_RIGHT;
                    orderTotalsTable.AddCell(cell);

                }
                if (displayTaxRates)
                {
                    foreach (var item in taxRates)
                    {
                        string taxRate = String.Format(_localizationService.GetResource("PDFInvoice.TaxRate"), _priceFormatter.FormatTaxRate(item.Key));
                        string taxValue = _priceFormatter.FormatPrice(_currencyService.ConvertCurrency(item.Value, order.CurrencyRate), false, order.CustomerCurrencyCode, false, lang);

                        cell = new PdfPCell(new Phrase(String.Format("{0} {1}", taxRate, taxValue), font));
                        cell.Border = Rectangle.NO_BORDER;
                        cell.PaddingLeft = orderTotalLeftMargin;
                        orderTotalsTable.AddCell(cell);
                    }
                }

                //discount (applied to order total)
                if (order.OrderDiscount > decimal.Zero)
                {
                    var orderDiscountInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderDiscount, order.CurrencyRate);
                    string orderDiscountInCustomerCurrencyStr = _priceFormatter.FormatPrice(-orderDiscountInCustomerCurrency, false, order.CustomerCurrencyCode, false, lang);

                    cell = new PdfPCell(new Paragraph(_localizationService.GetResource("PDFInvoice.Discount", lang.Id), font));
                    cell.Border = Rectangle.NO_BORDER;
                    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    orderTotalsTable.AddCell(cell);

                    cell = new PdfPCell();
                    cell.Border = Rectangle.NO_BORDER;
                    orderTotalsTable.AddCell(cell);

                    cell = new PdfPCell(new Phrase(orderDiscountInCustomerCurrencyStr, font));
                    cell.Border = Rectangle.NO_BORDER;
                    cell.VerticalAlignment = Element.ALIGN_RIGHT;
                    orderTotalsTable.AddCell(cell);

                }

                //order total
                var orderTotalInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
                string orderTotalStr = _priceFormatter.FormatPrice(orderTotalInCustomerCurrency, false, order.CustomerCurrencyCode, false, lang);

                cell = new PdfPCell(new Paragraph(_localizationService.GetResource("PDFInvoice.OrderTotal", lang.Id), font));
                cell.Border = Rectangle.NO_BORDER;
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                orderTotalsTable.AddCell(cell);

                cell = new PdfPCell();
                cell.Border = Rectangle.NO_BORDER;
                cell.VerticalAlignment = Element.ALIGN_RIGHT;
                orderTotalsTable.AddCell(cell);

                cell = new PdfPCell(new Phrase(orderTotalStr, font));
                cell.Border = Rectangle.NO_BORDER;
                cell.VerticalAlignment = Element.ALIGN_RIGHT;
                orderTotalsTable.AddCell(cell);
                #endregion


                var summaryTable = new PdfPTable(2);
                summaryTable.WidthPercentage = 100f;
                summaryTable.SetWidths(new[] { 65, 35 });
                cell = new PdfPCell();
                cell.Border = Rectangle.NO_BORDER;
                summaryTable.AddCell(cell);
                var totalCell = new PdfPCell(orderTotalsTable);
                totalCell.Border = Rectangle.NO_BORDER;
                summaryTable.AddCell(totalCell);
                doc.Add(productsTable);
                doc.Add(summaryTable);


                totalCell.FixedHeight = orderTotalHeight;
                totalCell.Border = Rectangle.NO_BORDER;


                ordNum++;
                if (ordNum < ordCount)
                {
                    doc.NewPage();
                }
            }
            doc.Close();
        }
        public virtual string PrintInvoiceToHtml(IList<Order> orders, Language lang, string filePath)
        {
            TextInfo textInfo = new CultureInfo("tr-TR", false).TextInfo;
            var sb = new StringBuilder();
            foreach (var order in orders)
            {
            
            sb.AppendLine("<body> <style type=text/css media=print>    .nonprintable{visibility:hidden}    </style><table style=width:940px;margin-left:5px;margin-top:50px>");
            sb.AppendLine("<tr><td><table width=100%><tr>");

            #region Address
            sb.AppendLine(string.Format("<td width=55% style=vertical-align:top;text-transform:capitalize;>{0}<br />", textInfo.ToTitleCase(order.BillingAddress.FirstName.ToLower()) + " " + textInfo.ToTitleCase(order.BillingAddress.LastName.ToLower())));
            sb.AppendLine(string.Format("{0} {1}<br />", order.BillingAddress.Address1 , order.BillingAddress.Address2 ));
            sb.AppendLine(string.Format("{0} {1}<br />", order.BillingAddress.StateProvince.Name , order.BillingAddress.City));
            sb.AppendLine(string.Format("{0}<br />", order.BillingAddress.Country.Name));
            sb.AppendLine(string.Format("{0}<br /></td >", order.BillingAddress.PhoneNumber));
            #endregion
            sb.AppendLine("<td width=15% class=nonprintable>Maliye logo</td>");
            #region InvoiceInfo
            sb.AppendLine("<td style=width:30%;><table width=100%>");
            sb.AppendLine("<tr><td colspan=3 class=nonprintable>İRSALİYELİ FATURA</td></tr>");
            sb.AppendLine("<tr style=height:55px class=nonprintable><td>SIRA</td></tr>");
            sb.AppendLine(string.Format("<tr style=vertical-align:bottom><td class=nonprintable>Düzenleme Tarihi</td><td class=nonprintable>:</td><td>{0}</td></tr>", String.Format("{0:d/M/yyyy}", _dateTimeHelper.ConvertToUserTime(DateTime.UtcNow, DateTimeKind.Utc))));
            sb.AppendLine(string.Format("<tr style=vertical-align:bottom><td class=nonprintable>Düzenleme Saati</td><td class=nonprintable>:</td><td>{0}</td></tr>", String.Format("{0:t}", _dateTimeHelper.ConvertToUserTime(DateTime.UtcNow, DateTimeKind.Utc))));
            sb.AppendLine(string.Format("<tr style=vertical-align:bottom><td class=nonprintable>Fiili Sevk Tarihi</td><td class=nonprintable>:</td><td>{0}</td></tr>", String.Format("{0:d/M/yyyy}", _dateTimeHelper.ConvertToUserTime(DateTime.UtcNow, DateTimeKind.Utc))));  
            sb.AppendLine("</table></td>");
            #endregion
            sb.AppendLine("</tr></table></td></tr>");
            sb.AppendLine("<tr><td><table style=width:100%;margin-top:25px>");
            sb.AppendLine("<tr style=height:20px class=nonprintable><th>AÇIKLAMA</th><th>MİKTAR</th><th>BİRİM FİYAT</th><th>TOPLAM</th></tr>");
            #region Products
            var orderProductVariants = _orderService.GetAllOrderProductVariants(order.Id, null, null, null, null, null, null);
            for (int i = 0; i < orderProductVariants.Count; i++)
            {
                var orderProductVariant = orderProductVariants[i];
                var pv = orderProductVariant.ProductVariant;
                sb.AppendLine("<tr><td width=50%>");
                //sku
                sb.AppendLine(string.Format("{0} ", pv.Sku));
                //product name
                string name = "";
                if (!String.IsNullOrEmpty(pv.GetLocalized(x => x.Name)))
                    name = pv.GetLocalized(x => x.Name);
                else
                    name = pv.Product.GetLocalized(x => x.Name);
                sb.AppendLine(string.Format("{0} ", name));
                sb.AppendLine(string.Format("<span style=text-transform:capitalize>{0}</span></td>", orderProductVariant.AttributeDescription));
                //quantity
                sb.AppendLine(string.Format("<td style=text-align:right; width:10%>{0}</td>", orderProductVariant.Quantity.ToString()));
                //price
                string unitPrice = string.Empty;
                switch (order.CustomerTaxDisplayType)
                {
                    case TaxDisplayType.ExcludingTax:
                        {
                            var opvUnitPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderProductVariant.UnitPriceExclTax, order.CurrencyRate);
                            unitPrice = _priceFormatter.FormatPrice(opvUnitPriceExclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, lang, false);
                        }
                        break;
                    case TaxDisplayType.IncludingTax:
                        {
                            var opvUnitPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderProductVariant.UnitPriceExclTax, order.CurrencyRate);
                            unitPrice = _priceFormatter.FormatPrice(opvUnitPriceInclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, lang, false);
                        }
                        break;
                }
                sb.AppendLine(string.Format("<td style=text-align:right; width:15%>{0}</td>", unitPrice));
                //total
                string subTotal = string.Empty;
                switch (order.CustomerTaxDisplayType)
                {
                    case TaxDisplayType.ExcludingTax:
                        {
                            var opvPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderProductVariant.PriceExclTax, order.CurrencyRate);
                            subTotal = _priceFormatter.FormatPrice(opvPriceExclTaxInCustomerCurrency, false, order.CustomerCurrencyCode,lang, false);
                        }
                        break;
                    case TaxDisplayType.IncludingTax:
                        {
                            var opvPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderProductVariant.PriceExclTax, order.CurrencyRate);
                            subTotal = _priceFormatter.FormatPrice(opvPriceInclTaxInCustomerCurrency, false, order.CustomerCurrencyCode, lang, false);
                        }
                        break;
                }
                sb.AppendLine(string.Format("<td style=text-align:right; width:25%>{0}</td>", subTotal));
                sb.AppendLine("</tr>");
            }
            #endregion
            sb.AppendLine("<tr style=vertical-align:bottom;><td ></td><td ></td><td></td>");
            sb.AppendLine("<td><table width=100%><tbody>");

            sb.AppendLine(string.Format("<tr><td style='text-align:right; vertical-align:top; width:40%; font-weight:bold;'>AraToplam:</td><td style='padding-left:1em; text-align:left; vertical-align:top; width:30%'></td><td style='text-align:right; vertical-align:top; width:40%'>{0}</td></tr>", _priceFormatter.FormatPrice(order.OrderSubtotalExclTax, false, order.CustomerCurrencyCode, false, lang)));
                 //tax
                string taxStr = string.Empty;
                var taxRates = new SortedDictionary<decimal, decimal>();
                bool displayTax = true;
                bool displayTaxRates = true;
                if (_taxSettings.HideTaxInOrderSummary && order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    displayTax = false;
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
                        taxRates = order.TaxRatesDictionary;

                        displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Count > 0;
                        displayTax = !displayTaxRates;

                        var orderTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTax, order.CurrencyRate);
                        taxStr = _priceFormatter.FormatPrice(orderTaxInCustomerCurrency, false, order.CustomerCurrencyCode, false, lang);
                    }
                }
            sb.AppendLine(string.Format("<tr><td style='text-align:right; vertical-align:top; width:40%; font-weight:bold;'>KDV:</td><td style='padding-left:1em; text-align:left; vertical-align:top; width:30%'></td><td style='text-align:right; vertical-align:top; width:40%'>{0}</td></tr>", taxStr));
            sb.AppendLine(FormatCheckoutAttributes("<tr><td style='text-align:right; vertical-align:top; width:40%; font-weight:bold;'>{0}:</td><td style='padding-left:1em; text-align:left; vertical-align:top; width:30%'></td><td style='text-align:right; vertical-align:top; width:40%'>{1}</td></tr>"));
            sb.AppendLine(string.Format("<tr><td style='text-align:right; vertical-align:top; width:40%; font-weight:bold;'>Kargo:</td><td style='padding-left:1em; text-align:left; vertical-align:top; width:30%'></td><td style='text-align:right; vertical-align:top; width:40%'>{0}</td></tr>", _priceFormatter.FormatPrice(order.OrderShippingInclTax, false, order.CustomerCurrencyCode, false, lang)));
            sb.AppendLine(string.Format("<tr><td style='text-align:right; vertical-align:top; width:40%; font-weight:bold;'>İndirim:</td><td style='padding-left:1em; text-align:left; vertical-align:top; width:30%'></td><td style='text-align:right; vertical-align:top; width:40%'>{0}</td></tr>", _priceFormatter.FormatPrice(order.OrderDiscount, false, order.CustomerCurrencyCode, false, lang)));
            sb.AppendLine(string.Format("<tr><td style='text-align:right; vertical-align:top; width:40%; font-weight:bold;'>Toplam:</td><td style='padding-left:1em; text-align:left; vertical-align:top; width:30%'></td><td style='text-align:right; vertical-align:top; width:40%'>{0}</td></tr>",_priceFormatter.FormatPrice(order.OrderTotal, false, order.CustomerCurrencyCode, false, lang )));
            sb.AppendLine("</tbody></table></td>");
            sb.AppendLine("</tr>");
            sb.AppendLine("</table></td></tr>");
            sb.AppendLine("</table>    <a href=javascript:window.print() class=nonprintable>print</a></body>");
            }


            string result = sb.ToString();
            return result;
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
                                ca.GetLocalized(a => a.TextPrompt, _workContext.WorkingLanguage.Id).ToUpper() + ":", priceAdjustmentStr);

                            result += caAttribute;

                        }
                    }
                }
            }

            return result;
        }
    }



    public static class NumberToWordsHelper
    {
        static string[] ones = new string[] { "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine" };
        static string[] teens = new string[] { "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
        static string[] tens = new string[] { "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };
        static string[] thousandsGroups = { "", " Thousand", " Million", " Billion" };

        private static string FriendlyInteger(int n, string leftDigits, int thousands)
        {
            if (n == 0)
            {
                return leftDigits;
            }
            string friendlyInt = leftDigits;
            if (friendlyInt.Length > 0)
            {
                friendlyInt += " ";
            }

            if (n < 10)
            {
                friendlyInt += ones[n];
            }
            else if (n < 20)
            {
                friendlyInt += teens[n - 10];
            }
            else if (n < 100)
            {
                friendlyInt += FriendlyInteger(n % 10, tens[n / 10 - 2], 0);
            }
            else if (n < 1000)
            {
                friendlyInt += FriendlyInteger(n % 100, (ones[n / 100] + " Hundred"), 0);
            }
            else
            {
                friendlyInt += FriendlyInteger(n % 1000, FriendlyInteger(n / 1000, "", thousands + 1), 0);
            }

            return friendlyInt + thousandsGroups[thousands];
        }

        public static string ToWritten(int number, Language language)
        {
            switch (language.LanguageCulture.ToLower())
            {
                case "en-us": return ToWrittenEN(number);
                case "tr-tr": return ToWrittenTR(number);
                default: return ToWrittenTR(number);
            
            }
        }

        public static string ToWrittenEN(int n)
        {
            if (n == 0)
            {
                return "Zero";
            }
            else if (n < 0)
            {
                return "Negative " + ToWrittenEN(-n);
            }

            return FriendlyInteger(n, "", 0);
        }

        public static string ToWrittenTR(int number)
        {
            string n = number.ToString();
            int max_basamak_say = 18; //18 basamaklı-katrilyon, daha da artırılabilir
            string[] birler =  { "", "bir", "iki", "üç", "dört", "beş", "altı", 
                       "yedi", "sekiz", "dokuz"};
            string[] onlar = { "", "on", "yirmi", "otuz", "kırk", "elli", "altmış", 
                       "yetmiş", "seksen", "doksan"};
            //Eğer 18 basamaktan daha fazla kullanacaksanız 
            //katrilyondan önce o basamakları da ekleyin
            string[] binler = { "katrilyon", "trilyon", "milyar", "milyon", "bin", "" };
            int i, uz;
            int[] bas = new int[3];
            string sonuç = "", ara_sonuç = "";
            uz = n.Length;
            //sayının kullanılmayan basamaklarını sıfırla doldur
            n = n.PadLeft(max_basamak_say, '0');
            //sayıyı üçerli basamaklar halinde ele al
            for (i = 0; i <= max_basamak_say / 3 - 1; i++)
            {
                //üçlü basamaktaki birinci sayı yani yüzler basamağı
                bas[0] = int.Parse(n.Substring(i * 3, 1));
                //üçlü basamaktaki ikinci sayı yani onlar basamağı
                bas[1] = int.Parse(n.Substring((i * 3) + 1, 1));
                //üçlü basamaktaki üçüncü sayı yani birler basamağı
                bas[2] = int.Parse(n.Substring((i * 3) + 2, 1));
                if (bas[0] == 0)
                    ara_sonuç = ""; //yüzler basamağı boş
                else
                    if (bas[0] == 1)
                        ara_sonuç = "yüz"; //yüzler basamağında 1 varsa 1 yüz olmaz sadece yüz
                    else
                        ara_sonuç = birler[bas[0]] + "yüz"; //yüzler basamağındaki sayı ve yüz 
                //ikiyüz gibi
                //yüzler+onlar+birler basamağını birleştir
                ara_sonuç = ara_sonuç + onlar[bas[1]] + birler[bas[2]];
                //basamak değeri oluşmadıysa yani 000 ise binler basamağını ekle
                if (ara_sonuç != "")
                    ara_sonuç = ara_sonuç + binler[i];
                //birbin olmaz
                if ((i > 1) && (ara_sonuç == "birbin"))
                    ara_sonuç = "bin";
                if (ara_sonuç != "")
                    sonuç = sonuç + ara_sonuç + " ";
            }
            if (sonuç.Trim() == "")
                sonuç = "sıfır";
            return sonuç.Trim();
        }

        public static string CurrencyMainPartText(string currencyCode, ILocalizationService localizationSerice)
        {
            return localizationSerice.GetResource("Currency.MainText." + currencyCode);
        }
        public static string CurrencyDecimalPartText(string currencyCode, ILocalizationService localizationSerice)
        {
            return localizationSerice.GetResource("Currency.DecimalText." + currencyCode);
        }


    }



}