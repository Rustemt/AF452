using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using System.Collections.Generic;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Price calculation service
    /// </summary>
    public partial interface IPriceCalculationService
    {

        decimal GetDiscountAmount(ProductVariant productVariant, Customer customer, bool showHiddenDiscounts = true);
        decimal GetDiscountAmount(ProductVariant productVariant, Customer customer, decimal additionalCharge, bool showHiddenDiscounts = true);
        decimal GetDiscountAmount(ProductVariant productVariant, Customer customer, decimal additionalCharge, out IList<Discount> appliedDiscounts, bool showHiddenDiscounts = true);
        decimal GetDiscountAmount(ProductVariant productVariant, Customer customer, decimal additionalCharge, int quantity, out IList<Discount> appliedDiscount, bool showHiddenDiscounts = true);
        decimal GetDiscountAmount(ProductVariant productVariant, bool showHiddenDiscounts = true);  
        
        decimal GetFinalPrice(ProductVariant productVariant, bool includeDiscounts, bool showHiddenDiscounts = true);
        decimal GetFinalPrice(ProductVariant productVariant, Customer customer, bool includeDiscounts, bool showHiddenDiscounts = true);
        decimal GetFinalPrice(ProductVariant productVariant, Customer customer, decimal additionalCharge, bool includeDiscounts, bool showHiddenDiscounts = true);
        decimal GetFinalPrice(ProductVariant productVariant, Customer customer, decimal additionalCharge, bool includeDiscounts, int quantity, bool showHiddenDiscounts = true);

        decimal GetFinalPrice(ProductVariant productVariant, bool includeDiscounts, out IList<Discount> appliedDiscounts, bool showHiddenDiscounts = true);
        decimal GetFinalPrice(ProductVariant productVariant, Customer customer, bool includeDiscounts, out IList<Discount> appliedDiscounts, bool showHiddenDiscounts = true);
        decimal GetFinalPrice(ProductVariant productVariant, Customer customer, decimal additionalCharge, bool includeDiscounts, out IList<Discount> appliedDiscounts, bool showHiddenDiscounts = true);
        decimal GetFinalPrice(ProductVariant productVariant, Customer customer, decimal additionalCharge, bool includeDiscounts, int quantity, out IList<Discount> appliedDiscounts, bool showHiddenDiscounts = true);


        decimal GetSubTotal(ShoppingCartItem shoppingCartItem, bool includeDiscounts, Discount exludedDiscount);
    }
}
