using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Services.Payments;

namespace Nop.Services.Orders
{
    /// <summary>
    /// Order service interface
    /// </summary>
    public partial interface IOrderTotalCalculationService
    {


        /// <summary>
        /// Gets shopping cart subtotal
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <param name="discountAmount">Applied discount amount</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <param name="subTotalWithoutDiscount">Sub total (without discount)</param>
        /// <param name="subTotalWithDiscount">Sub total (with discount)</param>
        void GetShoppingCartSubTotal(IList<ShoppingCartItem> cart, 
            bool includingTax,
            out decimal discountAmount, out Discount appliedDiscount,
            out decimal subTotalWithoutDiscount, out decimal subTotalWithDiscount, ProcessPaymentRequest processPaymentRequest);

    

    }
}
