using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Seo;
using Nop.Web.Extensions;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Security;
using Nop.Web.Models.Common;
using Nop.Web.Models.Customer;
using Nop.Web.Models.Media;
using Nop.Web.Models.Order;




namespace Nop.Web.Controllers
{
    public class OrderController : BaseNopController
    {
        #region Fields

        private readonly IOrderService _orderService;
        private readonly IWorkContext _workContext;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IMeasureService _measureService;
        private readonly IPaymentService _paymentService;
        private readonly ILocalizationService _localizationService;
        private readonly IPdfService _pdfService;
        private readonly ICustomerService _customerService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IProductAttributeFormatter _productAttributeFormatter;

        private readonly LocalizationSettings _localizationSettings;
        private readonly MeasureSettings _measureSettings;
        private readonly OrderSettings _orderSettings;
        private readonly TaxSettings _taxSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly PdfSettings _pdfSettings;

        private readonly MediaSettings _mediaSettings;
        private readonly IPictureService _pictureService;

        #endregion

        #region Constructors

        public OrderController(IOrderService orderService, IWorkContext workContext,
            ICurrencyService currencyService, IPriceFormatter priceFormatter,
            IOrderProcessingService orderProcessingService,
            IDateTimeHelper dateTimeHelper, IMeasureService measureService,
            IPaymentService paymentService, ILocalizationService localizationService,
            IPdfService pdfService, ICustomerService customerService,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings,
            MeasureSettings measureSettings, CatalogSettings catalogSettings,
            OrderSettings orderSettings, TaxSettings taxSettings, PdfSettings pdfSettings,
            IPictureService pictureSevice, MediaSettings mediaSettings, IProductAttributeFormatter productAttributeFormatter)
        {
            this._orderService = orderService;
            this._workContext = workContext;
            this._currencyService = currencyService;
            this._priceFormatter = priceFormatter;
            this._orderProcessingService = orderProcessingService;
            this._dateTimeHelper = dateTimeHelper;
            this._measureService = measureService;
            this._paymentService = paymentService;
            this._localizationService = localizationService;
            this._pdfService = pdfService;
            this._customerService = customerService;
            this._workflowMessageService = workflowMessageService;
            this._productAttributeFormatter = productAttributeFormatter;

            this._localizationSettings = localizationSettings;
            this._measureSettings = measureSettings;
            this._catalogSettings = catalogSettings;
            this._orderSettings = orderSettings;
            this._taxSettings = taxSettings;
            this._pdfSettings = pdfSettings;
            this._pictureService = pictureSevice;
            this._mediaSettings = mediaSettings;
        }

        #endregion

        #region Utilities

        [NonAction]
        private OrderDetailsModel PrepareOrderDetailsModel(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");
            var model = new OrderDetailsModel();

            model.Id = order.Id;
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc);
            model.Status = order.OrderStatus;
            model.OrderStatus = order.OrderStatus.GetLocalizedEnum(_localizationService, _workContext);
            model.IsReOrderAllowed = _orderSettings.IsReOrderAllowed;
            model.IsReturnRequestAllowed = _orderProcessingService.IsReturnRequestAllowed(order);

            model.DisplayPdfInvoice = _pdfSettings.Enabled;

            //shipping info
            model.ShippingStatus = order.ShippingStatus.GetLocalizedEnum(_localizationService, _workContext);
            if (order.ShippingStatus == ShippingStatus.Shipped)
                model.OrderStatus = model.ShippingStatus;
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
                    model.Tax = _priceFormatter.FormatPrice(orderTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, false);

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
                model.OrderTotalDiscount = _priceFormatter.FormatPrice(-orderDiscountInCustomerCurrency, true, order.CustomerCurrencyCode, false, _workContext.WorkingLanguage);


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
            model.ShowSku = _catalogSettings.ShowProductSku;
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
                };

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
            }

            //Order Items add Return Request
            var returnRequests = _orderService.SearchReturnRequests(_workContext.CurrentCustomer.Id, 0, null);
            model.HasReturnRequest = false;
            foreach (var returnRequest in returnRequests)
            {
                var opv = _orderService.GetOrderProductVariantById(returnRequest.OrderProductVariantId);

                if (opv.OrderId == model.Id)
                {

                    model.HasReturnRequest = true;
                    var pv = opv.ProductVariant;
                    var request = new OrderDetailsModel.OrderProductVariantModel.OrderProductVariantReturnRequest()
                                      {
                                          RequestAction = returnRequest.RequestedAction,
                                          ReturnRequestStatusId = returnRequest.ReturnRequestStatusId,
                                          ReturnRequestStatus = returnRequest.ReturnRequestStatus.GetLocalizedEnum(_localizationService, _workContext),
                                          ReturnRequestDate = returnRequest.UpdatedOnUtc
                                      };
                    model.Items.Where(x => x.ProductId == pv.ProductId).FirstOrDefault().ItemsReturnRequest = request;
                }
            }
            return model;
        }

        [NonAction]
        private SubmitReturnRequestModel PrepareReturnRequestModel(SubmitReturnRequestModel model, Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (model == null)
                throw new ArgumentNullException("model");

            model.OrderId = order.Id;

            //return reasons
            if (_orderSettings.ReturnRequestReasons != null)
                foreach (var rrr in _orderSettings.ReturnRequestReasons)
                {
                    string sRrr = _localizationService.GetResource(rrr);

                    model.AvailableReturnReasons.Add(new SelectListItem()
                        {
                            Text = sRrr,
                            Value = sRrr
                        });
                }

            //return actions
            if (_orderSettings.ReturnRequestActions != null)
                foreach (var rra in _orderSettings.ReturnRequestActions)
                {
                    string sRra = _localizationService.GetResource(rra);
                    model.AvailableReturnActions.Add(new SelectListItem()
                    {
                        Text = sRra,
                        Value = sRra
                    });
                }

            //products
            var orderProductVariants = _orderService.GetAllOrderProductVariants(order.Id, null, null, null, null, null, null);
            foreach (var opv in orderProductVariants)
            {
                var opvModel = new SubmitReturnRequestModel.OrderProductVariantModel()
                {
                    Id = opv.Id,
                    ProductId = opv.ProductVariant.ProductId,
                    ProductSeName = opv.ProductVariant.Product.GetSeName(),
                    AttributeInfo = opv.AttributeDescription,
                    Quantity = opv.Quantity
                };

                for (int i = 1; i <= opvModel.Quantity; i++)
                {
                    opvModel.SelectListProductQuantity.Add(new SelectListItem()
                                                               {
                                                                   Value = i.ToString(),
                                                                   Text = i.ToString()
                                                               }
                                                            );
                }
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
                opvModel.ManufacturerName = opv.ProductVariant.Product.ManufacturerName();
                opvModel.Sku = opv.ProductVariant.Sku;
                //product name
                if (!String.IsNullOrEmpty(opv.ProductVariant.GetLocalized(x => x.Name)))
                    opvModel.ProductName = string.Format("{0} ({1})", opv.ProductVariant.Product.GetLocalized(x => x.Name), opv.ProductVariant.GetLocalized(x => x.Name));
                else
                    opvModel.ProductName = opv.ProductVariant.Product.GetLocalized(x => x.Name);
                model.Items.Add(opvModel);

                //unit price
                switch (order.CustomerTaxDisplayType)
                {
                    case TaxDisplayType.ExcludingTax:
                        {
                            var opvUnitPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(opv.UnitPriceExclTax, order.CurrencyRate);
                            opvModel.UnitPrice = _priceFormatter.FormatPrice(opvUnitPriceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, false);

                        }
                        break;
                    case TaxDisplayType.IncludingTax:
                        {
                            var opvUnitPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(opv.UnitPriceInclTax, order.CurrencyRate);
                            opvModel.UnitPrice = _priceFormatter.FormatPrice(opvUnitPriceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, true);
                        }
                        break;
                }
            }
            return model;
        }

        #endregion

        #region Order details

        [NopHttpsRequirement(SslRequirement.Yes)]
        public ActionResult OrderInfo(int orderId)
        {
            var order = _orderService.GetOrderById(orderId);
            //ViewBag.Status = order.OrderStatus.GetLocalizedEnum(_localizationService, _workContext);

            if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
                return new HttpUnauthorizedResult();

            var model = PrepareOrderDetailsModel(order);
            return PartialView(model);
        }

        [NopHttpsRequirement(SslRequirement.Yes)]
        public ActionResult Details(int orderId)
        {
            Order order = _orderService.GetOrderById(orderId);

            if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
                return new HttpUnauthorizedResult();
            ViewBag.Msg = TempData["ReturnRequestSuccess"] ?? new MessageModel();
            var model = PrepareOrderDetailsModel(order);
            CustomerNavigationModel navModel = new CustomerNavigationModel();
            ViewData["OrderDetail"] = model;
            //foreach (var sci in model.Items)//aa
            //{
            //    sci.AttributeInfo = _productAttributeFormatter.FormatAttributes(order.OrderProductVariants.Where(x => x.ProductVariant.ProductId == sci.ProductId).FirstOrDefault().ProductVariant, sci.AttributeInfo, "<h2>{0} <span>{1}</span></h2>", _workContext.CurrentCustomer, htmlEncode: false);
            //}
            ViewData["NavModel"] = navModel;
            return View(model);
        }


        //[NopHttpsRequirement(SslRequirement.Yes)]
        //public ActionResult OrderDetail(int orderId)
        //{
        //    Order order = _orderService.GetOrderById(orderId);
        //    if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
        //        return new HttpUnauthorizedResult();
        //    var model = PrepareOrderDetailsModel(order);
        //    return View("Order", order);
        //}

        [NopHttpsRequirement(SslRequirement.Yes)]
        public ActionResult PrintOrderDetails(int orderId)
        {
            var order = _orderService.GetOrderById(orderId);
            if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
                return new HttpUnauthorizedResult();
            var model = PrepareOrderDetailsModel(order);
            model.PrintMode = true;
            return View("Details", model);
        }

        public ActionResult GetPdfInvoice(int orderId)
        {
            var order = _orderService.GetOrderById(orderId);
            if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
                return new HttpUnauthorizedResult();

            var orders = new List<Order>();
            orders.Add(order);
            string fileName = string.Format("order_{0}_{1}.pdf", order.OrderGuid, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));
            string filePath = string.Format("{0}content\\files\\ExportImport\\{1}", this.Request.PhysicalApplicationPath, fileName);
            _pdfService.PrintOrdersToPdf(orders, _workContext.WorkingLanguage, filePath);
            var pdfBytes = System.IO.File.ReadAllBytes(filePath);
            return File(pdfBytes, "application/pdf", fileName);
        }


        [HttpPost, ActionName("Details")]
        [FormValueRequired("reorder")]
        public ActionResult Reorder(int orderId)
        {
            var order = _orderService.GetOrderById(orderId);
            if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
                return new HttpUnauthorizedResult();

            _orderProcessingService.ReOrder(order);
            return RedirectToRoute("ShoppingCart");
        }

        #endregion

        #region Return requests

        [NopHttpsRequirement(SslRequirement.Yes)]
        public ActionResult ReturnRequest(int orderId)
        {
            var order = _orderService.GetOrderById(orderId);
            if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
                return new HttpUnauthorizedResult();

            if (!_orderProcessingService.IsReturnRequestAllowed(order))
                return RedirectToAction("Index", "Home");
            CustomerNavigationModel navModel = new CustomerNavigationModel();
            ViewData["NavModel"] = navModel;
            var model = new SubmitReturnRequestModel();
            model = PrepareReturnRequestModel(model, order);
            ViewBag.Msg = new MessageModel();
            return View(model);
        }

        [HttpPost, ActionName("ReturnRequest")]
        [ValidateInput(false)]
        public ActionResult ReturnRequestSubmit(int orderId, SubmitReturnRequestModel model, FormCollection form)
        {
            //TempData["ReturnRequestSuccess"] = null;
            CustomerNavigationModel navModel = new CustomerNavigationModel();
            ViewData["NavModel"] = navModel;
            MessageModel msg = new MessageModel();
            var order = _orderService.GetOrderById(orderId);
            if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
                return new HttpUnauthorizedResult();

            if (!_orderProcessingService.IsReturnRequestAllowed(order))
                return RedirectToAction("Index", "Home");

            int count = 0;
            foreach (var opv in order.OrderProductVariants)
            {
                int quantity = 0; //parse quantity
                bool selection = false;
                foreach (string formKey in form.AllKeys)
                {
                    if (formKey.Equals(string.Format("quantity{0}", opv.Id), StringComparison.InvariantCultureIgnoreCase))
                    {
                        int.TryParse(form[formKey], out quantity);
                        continue;
                    }
                    if (formKey.Equals(string.Format("selection{0}", opv.Id), StringComparison.InvariantCultureIgnoreCase))
                    {
                        bool.TryParse(form[formKey], out selection);
                        continue;
                    }
                }
                if (selection && quantity > 0)
                {
                    var rr = new ReturnRequest()
                    {
                        OrderProductVariantId = opv.Id,
                        Quantity = quantity,
                        CustomerId = _workContext.CurrentCustomer.Id,
                        ReasonForReturn = model.ReturnReason,
                        RequestedAction = model.ReturnAction,
                        CustomerComments = model.Comments,
                        StaffNotes = string.Empty,
                        ReturnRequestStatus = ReturnRequestStatus.Pending,
                        CreatedOnUtc = DateTime.UtcNow,
                        UpdatedOnUtc = DateTime.UtcNow
                    };
                    _workContext.CurrentCustomer.ReturnRequests.Add(rr);
                    _customerService.UpdateCustomer(_workContext.CurrentCustomer);
                    //notify store owner here (email)
                    _workflowMessageService.SendNewReturnRequestStoreOwnerNotification(rr, opv, _localizationSettings.DefaultAdminLanguageId);
                    _workflowMessageService.SendNewReturnRequestCustomerNotification(_workContext.CurrentCustomer.GetFullName(), _workContext.CurrentCustomer.Email, model.OrderId.ToString(), _workContext.WorkingLanguage.Id);
                    count++;
                }
            }

            if (count > 0)
            {
                msg.Successful = true;
                msg.MessageList.Add(_localizationService.GetResource("ReturnRequests.Submitted", _workContext.WorkingLanguage.Id));
                TempData["ReturnRequestSuccess"] = msg;
                return RedirectToAction("Details", new { orderId = orderId });
            }
            else
            {
                msg.Successful = false;
                msg.MessageList.Add(_localizationService.GetResource("ReturnRequests.NoItemsSubmitted", _workContext.WorkingLanguage.Id));
                ViewBag.Msg = msg;
            }
            model = PrepareReturnRequestModel(new SubmitReturnRequestModel(), order);
            ViewBag.Msg = msg;
            return View("ReturnRequest", model);
        }


        #endregion

    }
}