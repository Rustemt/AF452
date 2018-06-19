using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Events;
using Nop.Core.Plugins;
using Nop.Services.Catalog;
using Nop.Services.Logging;
using Nop.Services.Orders;

namespace Nop.Services.Shipping
{
    /// <summary>
    /// Shipping service
    /// </summary>
    public partial class ShippingService : IShippingService
    {

        #region Methods
        public virtual decimal GetShoppingCartCheckoutAttibutesWeight(IList<ShoppingCartItem> cart)
        {
            Customer customer = cart.GetCustomer();
            decimal totalWeight = decimal.Zero;
            //checkout attributes
            if (customer != null)
            {
                if (!String.IsNullOrEmpty(customer.CheckoutAttributes))
                {
                    var caValues = _checkoutAttributeParser.ParseCheckoutAttributeValues(customer.CheckoutAttributes);
                    foreach (var caValue in caValues)
                        totalWeight += caValue.WeightAdjustment;
                }
            }
            return totalWeight;
        }

        #endregion
    }
}
