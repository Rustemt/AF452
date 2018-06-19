using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Nop.Core.Domain.Affiliates;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Catalog;
using System.Web.Mvc;
using Nop.Core.Domain.Media;
using System.Web.Routing;

namespace Nop.Core.Domain.Custom
{
    public partial class SalesAgreement : BaseEntity
    {
        public SalesAgreement()
        {
            Items = new List<ShoppingCartItem>();
            Warnings = new List<string>();
            DiscountBox = new DiscountBox();
            GiftCardBox = new GiftCardBox();
            CheckoutAttributes = new List<CheckoutAttribute>();
            OrderReviewData = new OrderReviewData();
            OrderTotal = new OrderTotals();

            ButtonPaymentMethodActionNames = new List<string>();
            ButtonPaymentMethodControllerNames = new List<string>();
            ButtonPaymentMethodRouteValues = new List<RouteValueDictionary>();
        }

        public bool ShowSku { get; set; }
        public bool ShowProductImages { get; set; }
        public bool IsEditable { get; set; }
        public IList<ShoppingCartItem> Items { get; set; }

        public string CheckoutAttributeInfo { get; set; }
        public IList<CheckoutAttribute> CheckoutAttributes { get; set; }

        public IList<string> Warnings { get; set; }
        public string MinOrderSubtotalWarning { get; set; }
        public bool TermsOfServiceEnabled { get; set; }

        public DiscountBox DiscountBox { get; set; }
        public GiftCardBox GiftCardBox { get; set; }
        public OrderReviewData OrderReviewData { get; set; }
        public OrderTotals OrderTotal { get; set; }

        public IList<string> ButtonPaymentMethodActionNames { get; set; }
        public IList<string> ButtonPaymentMethodControllerNames { get; set; }
        public IList<RouteValueDictionary> ButtonPaymentMethodRouteValues { get; set; }

    }
    #region Related Classes

    public partial class ShoppingCartItem : BaseEntity
    {
        public ShoppingCartItem()
        {
            Picture = new Picture();
            AllowedQuantities = new List<SelectListItem>();
            Warnings = new List<string>();
        }
        public string Sku { get; set; }

        public Picture Picture { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public string ProductSeName { get; set; }

        public string UnitPrice { get; set; }

        public string SubTotal { get; set; }

        public string Discount { get; set; }

        public int Quantity { get; set; }
        public List<SelectListItem> AllowedQuantities { get; set; }

        public string AttributeInfo { get; set; }

        public string RecurringInfo { get; set; }

        public IList<string> Warnings { get; set; }

    }

    public partial class CheckoutAttribute : BaseEntity
    {
        public CheckoutAttribute()
        {
            Values = new List<CheckoutAttributeValue>();
        }

        public string Name { get; set; }

        public string DefaultValue { get; set; }

        public string TextPrompt { get; set; }

        public bool IsRequired { get; set; }

        /// <summary>
        /// Selected day value for datepicker
        /// </summary>
        public int? SelectedDay { get; set; }
        /// <summary>
        /// Selected month value for datepicker
        /// </summary>
        public int? SelectedMonth { get; set; }
        /// <summary>
        /// Selected year value for datepicker
        /// </summary>
        public int? SelectedYear { get; set; }

        public AttributeControlType AttributeControlType { get; set; }

        public IList<CheckoutAttributeValue> Values { get; set; }
    }

    public partial class CheckoutAttributeValue : BaseEntity
    {
        public string Name { get; set; }

        public string ColorSquaresRgb { get; set; }

        public string PriceAdjustment { get; set; }

        public bool IsPreSelected { get; set; }
    }

    public partial class DiscountBox : BaseEntity
    {
        public bool Display { get; set; }
        public string Message { get; set; }
        public string CurrentCode { get; set; }
    }

    public partial class GiftCardBox : BaseEntity
    {
        public bool Display { get; set; }
        public string Message { get; set; }
    }

    public partial class OrderReviewData : BaseEntity
    {
        public OrderReviewData()
        {
            this.BillingAddress = new Address();
            this.ShippingAddress = new Address();
        }
        public bool Display { get; set; }

        public Address BillingAddress { get; set; }

        public string Billing_Country { get; set; }
        public string Billing_SatateProvision { get; set; }
        public string Shipping_Country { get; set; }
        public string Shipping_SatateProvision { get; set; }

        public bool IsShippable { get; set; }
        public Address ShippingAddress { get; set; }
        public string ShippingMethod { get; set; }

        public string PaymentMethod { get; set; }

    }


    public partial class OrderTotals : BaseEntity
    {
        public OrderTotals()
        {
            TaxRates = new List<TaxRate>();
            GiftCards = new List<GiftCard>();
        }
        public bool IsEditable { get; set; }

        public string SubTotal { get; set; }

        public string SubTotalDiscount { get; set; }
        public bool AllowRemovingSubTotalDiscount { get; set; }

        public string Shipping { get; set; }
        public bool RequiresShipping { get; set; }
        public string SelectedShippingMethod { get; set; }

        public string PaymentMethodAdditionalFee { get; set; }

        public string Tax { get; set; }
        public IList<TaxRate> TaxRates { get; set; }
        public bool DisplayTax { get; set; }
        public bool DisplayTaxRates { get; set; }


        public IList<GiftCard> GiftCards { get; set; }

        public string OrderTotalDiscount { get; set; }
        public bool AllowRemovingOrderTotalDiscount { get; set; }
        public int RedeemedRewardPoints { get; set; }
        public string RedeemedRewardPointsAmount { get; set; }
        public string OrderTotal { get; set; }
        public string cavalue { get; set; }
        #region Nested classes

        public partial class TaxRate : BaseEntity
        {
            public string Rate { get; set; }
            public string Value { get; set; }
        }

        public partial class GiftCard : BaseEntity
        {
            public string CouponCode { get; set; }
            public string Amount { get; set; }
            public string Remaining { get; set; }
        }
        #endregion
    }
    #endregion
}
