using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Catalog;

namespace Nop.Services.Discounts
{
    /// <summary>
    /// Discount service interface
    /// </summary>
    public partial interface IDiscountService
    {
        /// <summary>
        /// Check discount requirements
        /// </summary>
        /// <param name="discount">Discount</param>
        /// <param name="customer">Customer</param>
        /// <returns>true - requirement is met; otherwise, false</returns>
        bool IsDiscountValid(Discount discount, Customer customer, ProductVariant productVariant);
        bool IsDiscountValid(Discount discount, Customer customer, string couponCodeToValidate, out string message);
        bool IsDiscountCouponCodeValid(Discount discount, string couponCodeToValidate);
		//ICollection<Discount> AppliedDiscountsToCategory(int categoryId);
    }
}
