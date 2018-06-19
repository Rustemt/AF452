using System;
using System.Text;
using System.Web;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Html;
using Nop.Services.Localization;
using Nop.Core.Domain.Localization;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Product attribute formatter
    /// </summary>
    public partial class ProductAttributeFormatter : IProductAttributeFormatter
    {

        public string FormatAttributes(ProductVariant productVariant, string attributes, string wrapperHtml, Customer customer, string serapator = "<br />", bool htmlEncode = true, bool renderPrices = true, bool renderProductAttributes = true, bool renderGiftCardAttributes = true)
        {
            var result = new StringBuilder();

            //attributes
            if (renderProductAttributes)
            {
                var pvaCollection = _productAttributeParser.ParseProductVariantAttributes(attributes);
                for (int i = 0; i < pvaCollection.Count; i++)
                {
                    var pva = pvaCollection[i];
                    var valuesStr = _productAttributeParser.ParseValues(attributes, pva.Id);
                    for (int j = 0; j < valuesStr.Count; j++)
                    {
                        string valueStr = valuesStr[j];
                        string pvaAttribute = string.Empty;
                        if (!pva.ShouldHaveValues())
                        {
                            if (pva.AttributeControlType == AttributeControlType.MultilineTextbox)
                            {
                                pvaAttribute = string.Format(wrapperHtml, pva.ProductAttribute.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id), HtmlHelper.FormatText(valueStr, false, true, true, false, false, false));
                            }
                            else
                            {
                                pvaAttribute = string.Format(wrapperHtml, pva.ProductAttribute.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id), valueStr);
                            }
                        }
                        else
                        {
                            int pvaId = 0;
                            if (int.TryParse(valueStr, out pvaId))
                            {
                                var pvaValue = _productAttributeService.GetProductVariantAttributeValueById(pvaId);
                                if (pvaValue != null)
                                {
                                    pvaAttribute = string.Format(wrapperHtml, pva.ProductAttribute.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id), pvaValue.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id));
                                    if (renderPrices)
                                    {
                                        decimal taxRate = decimal.Zero;
                                        decimal priceAdjustmentBase = _taxService.GetProductPrice(productVariant, pvaValue.PriceAdjustment, customer, out taxRate);
                                        decimal priceAdjustment = _currencyService.ConvertFromPrimaryStoreCurrency(priceAdjustmentBase, _workContext.WorkingCurrency);
                                        if (priceAdjustmentBase > 0)
                                        {
                                            string priceAdjustmentStr = _priceFormatter.FormatPrice(priceAdjustment, false, false);
                                            pvaAttribute += string.Format(" [+{0}]", priceAdjustmentStr);
                                        }
                                        else if (priceAdjustmentBase < decimal.Zero)
                                        {
                                            string priceAdjustmentStr = _priceFormatter.FormatPrice(-priceAdjustment, false, false);
                                            pvaAttribute += string.Format(" [-{0}]", priceAdjustmentStr);
                                        }
                                    }
                                }
                            }
                        }

                        if (!String.IsNullOrEmpty(pvaAttribute))
                        {
                            if (i != 0 || j != 0)
                            {
                                result.Append(serapator);
                            }

                            //we don't encode multiline textbox input
                            if (htmlEncode &&
                                pva.AttributeControlType != AttributeControlType.MultilineTextbox)
                            {
                                result.Append(HttpUtility.HtmlEncode(pvaAttribute));
                            }
                            else
                            {
                                result.Append(pvaAttribute);
                            }
                        }
                    }
                }
            }

            //gift cards
            if (renderGiftCardAttributes)
            {
                if (productVariant.IsGiftCard)
                {
                    string giftCardRecipientName = string.Empty;
                    string giftCardRecipientEmail = string.Empty;
                    string giftCardSenderName = string.Empty;
                    string giftCardSenderEmail = string.Empty;
                    string giftCardMessage = string.Empty;
                    _productAttributeParser.GetGiftCardAttribute(attributes, out giftCardRecipientName, out giftCardRecipientEmail,
                        out giftCardSenderName, out giftCardSenderEmail, out giftCardMessage);

                    if (!String.IsNullOrEmpty(result.ToString()))
                    {
                        result.Append(serapator);
                    }

                    if (htmlEncode)
                    {
                        result.Append(HttpUtility.HtmlEncode(string.Format(_localizationService.GetResource("GiftCardAttribute.For"), giftCardRecipientName)));
                        result.Append(serapator);
                        result.Append(HttpUtility.HtmlEncode(string.Format(_localizationService.GetResource("GiftCardAttribute.From"), giftCardSenderName)));
                    }
                    else
                    {
                        result.Append(string.Format(_localizationService.GetResource("GiftCardAttribute.For"), giftCardRecipientName));
                        //result.Append(string.Format(_localizationService.GetResource("GiftCardAttribute.ForEmail"), giftCardRecipientEmail));
                        result.Append(serapator);
                        result.Append(string.Format(_localizationService.GetResource("GiftCardAttribute.From"), giftCardSenderName));
                    }
                }
            }
            return result.ToString();
        }

    }
       
}
