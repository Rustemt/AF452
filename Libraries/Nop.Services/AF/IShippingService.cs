using System.Collections.Generic;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;

namespace Nop.Services.Shipping
{
    /// <summary>
    /// Shipping service interface
    /// </summary>
    public partial interface IShippingService
    {
        decimal GetShoppingCartCheckoutAttibutesWeight(IList<ShoppingCartItem> cart); 
    }
}
