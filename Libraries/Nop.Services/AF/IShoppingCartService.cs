using System;
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;

namespace Nop.Services.Orders
{
    /// <summary>
    /// Shopping cart service
    /// </summary>
    public partial interface IShoppingCartService
    {
        /// <summary>
        /// Add a product variant to shopping cart
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="productVariant">Product variant</param>
        /// <param name="shoppingCartType">Shopping cart type</param>
        /// <param name="selectedAttributes">Selected attributes</param>
        /// <param name="customerEnteredPrice">The price enter by a customer</param>
        /// <param name="quantity">Quantity</param>
        /// <returns>Warnings</returns>
        IList<string> AddToCart(Customer customer, ProductVariant productVariant,
            ShoppingCartType shoppingCartType, string selectedAttributes,
            decimal customerEnteredPrice, int quantity, bool automaticallyAddRequiredProductVariantsIfEnabled, string CustomerComment);

        IList<string> AddToCart(Customer customer, ProductVariant productVariant,
          ShoppingCartType shoppingCartType, string selectedAttributes,
          decimal customerEnteredPrice, int quantity, bool automaticallyAddRequiredProductVariantsIfEnabled, bool forceAttributeSelection);


        IList<string> AddToCartFromWishList(Customer customer, ProductVariant productVariant,
            ShoppingCartType shoppingCartType, string selectedAttributes,
            decimal customerEnteredPrice, int quantity, bool automaticallyAddRequiredProductVariantsIfEnabled, string CustomerComment);

        IList<string> GetShoppingCartItemWarnings(Customer customer, ShoppingCartType shoppingCartType,
            ProductVariant productVariant, string selectedAttributes, decimal customerEnteredPrice,
            int quantity, bool automaticallyAddRequiredProductVariantsIfEnabled,bool fromWishList, bool forceAttributeSelection=false);

        IList<string> AddToCartByList(IEnumerable<ShoppingCartItem> shoppingCartItem,Customer customer,ShoppingCartType shoppingCartType,int quantity, bool automaticallyAddRequiredProductVariantsIfEnabled);


        /// <summary>
        /// Updates the shopping cart item
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="shoppingCartItemId">Shopping cart item identifier</param>
        /// <param name="newQuantity">New shopping cart item quantity</param>
        /// <param name="resetCheckoutData">A value indicating whether to reset checkout data</param>
        /// <returns>Warnings</returns>
        IList<string> UpdateShoppingCartItem(Customer customer, int shoppingCartItemId,
            int newQuantity, bool resetCheckoutData, string userComment);
    }
}
