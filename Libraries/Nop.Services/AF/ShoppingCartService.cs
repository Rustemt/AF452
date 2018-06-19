using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Core.Events;

namespace Nop.Services.Orders
{
    /// <summary>
    /// Shopping cart service
    /// </summary>
    public partial class ShoppingCartService : IShoppingCartService
    {
        #region Fields

        #endregion

        #region Methods

        /// <summary>
        /// Updates the shopping cart item
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="shoppingCartItemId">Shopping cart item identifier</param>
        /// <param name="newQuantity">New shopping cart item quantity</param>
        /// <param name="resetCheckoutData">A value indicating whether to reset checkout data</param>
        /// <returns>Warnings</returns>
        /// <summary>
        /// Updates the shopping cart item
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="shoppingCartItemId">Shopping cart item identifier</param>
        /// <param name="newQuantity">New shopping cart item quantity</param>
        /// <param name="resetCheckoutData">A value indicating whether to reset checkout data</param>
        /// <returns>Warnings</returns>
        public virtual IList<string> UpdateShoppingCartItem(Customer customer, int shoppingCartItemId,
            int newQuantity, bool resetCheckoutData, string CustomerComment)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var warnings = new List<string>();

            var shoppingCartItem = customer.ShoppingCartItems.Where(sci => sci.Id == shoppingCartItemId).FirstOrDefault();
            if (shoppingCartItem != null)
            {
                if (resetCheckoutData)
                {
                    //reset checkout data
                    _customerService.ResetCheckoutData(customer, false);
                }
                if (newQuantity > 0)
                {
                    //check warnings
                    warnings.AddRange(GetShoppingCartItemWarnings(customer, shoppingCartItem.ShoppingCartType,
                        shoppingCartItem.ProductVariant, shoppingCartItem.AttributesXml,
                        shoppingCartItem.CustomerEnteredPrice, newQuantity, false));
                    if (warnings.Count == 0)
                    {
                        //if everything is OK, then update a shopping cart item
                        shoppingCartItem.Quantity = newQuantity;
                        shoppingCartItem.UpdatedOnUtc = DateTime.UtcNow;
                        shoppingCartItem.CustomerComment = CustomerComment;
                        _customerService.UpdateCustomer(customer);

                        //event notification
                        _eventPublisher.EntityUpdated(shoppingCartItem);
                    }
                }
                else
                {
                    //delete a shopping cart item
                    //customer.ShoppingCartItems.Remove(shoppingCartItem);
                    //_customerService.UpdateCustomer(customer);
                    //_sciRepository.Delete(shoppingCartItem);
                    //if (resetCheckoutData)
                    //    _customerService.ResetCheckoutData(customer, false);
                    DeleteShoppingCartItem(shoppingCartItem, resetCheckoutData);
                }
            }

            return warnings;
        }


        /// <summary>
        /// Add a product variant to shopping cart
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="productVariant">Product variant</param>
        /// <param name="shoppingCartType">Shopping cart type</param>
        /// <param name="selectedAttributes">Selected attributes</param>
        /// <param name="customerEnteredPrice">The price enter by a customer</param>
        /// <param name="quantity">Quantity</param>
        /// <param name="automaticallyAddRequiredProductVariantsIfEnabled">Automatically add required product variants if enabled</param>
        /// <returns>Warnings</returns>
        public virtual IList<string> AddToCart(Customer customer, ProductVariant productVariant,
            ShoppingCartType shoppingCartType, string selectedAttributes,
            decimal customerEnteredPrice, int quantity, bool automaticallyAddRequiredProductVariantsIfEnabled, string CustomerComment)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            if (productVariant == null)
                throw new ArgumentNullException("productVariant");

            var warnings = new List<string>();
            if (shoppingCartType == ShoppingCartType.ShoppingCart && !_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart, customer))
            {
                warnings.Add("Shopping cart is disabled");
                return warnings;
            }
            if (shoppingCartType == ShoppingCartType.Wishlist && !_permissionService.Authorize(StandardPermissionProvider.EnableWishlist, customer))
            {
                warnings.Add("Wishlist is disabled");
                return warnings;
            }


            //reset checkout info
            _customerService.ResetCheckoutData(customer, false);

            var cart = customer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == shoppingCartType).ToList();

            ShoppingCartItem shoppingCartItem = FindShoppingCartItemInTheCart(cart,
                shoppingCartType, productVariant, selectedAttributes, customerEnteredPrice);

            if (shoppingCartItem != null)
            {
                //update existing shopping cart item
                int newQuantity = shoppingCartItem.Quantity + quantity;
                warnings.AddRange(GetShoppingCartItemWarnings(customer, shoppingCartType, productVariant,
                    selectedAttributes, customerEnteredPrice, newQuantity, automaticallyAddRequiredProductVariantsIfEnabled));

                if (warnings.Count == 0)
                {
                    shoppingCartItem.AttributesXml = selectedAttributes;
                    shoppingCartItem.Quantity = newQuantity;
                    shoppingCartItem.CustomerComment = CustomerComment;
                    shoppingCartItem.UpdatedOnUtc = DateTime.UtcNow;
                    _customerService.UpdateCustomer(customer);

                    //event notification
                    _eventPublisher.EntityUpdated(shoppingCartItem);
                }
            }
            else
            {
                //new shopping cart item
                warnings.AddRange(GetShoppingCartItemWarnings(customer, shoppingCartType, productVariant,
                    selectedAttributes, customerEnteredPrice, quantity, automaticallyAddRequiredProductVariantsIfEnabled));
                if (warnings.Count == 0)
                {
                    //maximum items validation
                    switch (shoppingCartType)
                    {
                        case ShoppingCartType.ShoppingCart:
                            {
                                if (cart.Count >= _shoppingCartSettings.MaximumShoppingCartItems)
                                    return warnings;
                            }
                            break;
                        case ShoppingCartType.Wishlist:
                            {
                                if (cart.Count >= _shoppingCartSettings.MaximumWishlistItems)
                                    return warnings;
                            }
                            break;
                        default:
                            break;
                    }

                    DateTime now = DateTime.UtcNow;
                    shoppingCartItem = new ShoppingCartItem()
                    {
                        ShoppingCartType = shoppingCartType,
                        ProductVariant = productVariant,
                        AttributesXml = selectedAttributes,
                        CustomerEnteredPrice = customerEnteredPrice,
                        Quantity = quantity,
                        CreatedOnUtc = now,
                        UpdatedOnUtc = now
                    };
                    customer.ShoppingCartItems.Add(shoppingCartItem);
                    _customerService.UpdateCustomer(customer);

                    //event notification
                    _eventPublisher.EntityInserted(shoppingCartItem);
                }
            }

            return warnings;
        }

        public virtual IList<string> AddToCartFromWishList(Customer customer, ProductVariant productVariant,
            ShoppingCartType shoppingCartType, string selectedAttributes,
            decimal customerEnteredPrice, int quantity, bool automaticallyAddRequiredProductVariantsIfEnabled, string CustomerComment)
        {
            //Point
            if (customer == null)
                throw new ArgumentNullException("customer");

            if (productVariant == null)
                throw new ArgumentNullException("productVariant");

            var warnings = new List<string>();
            if (shoppingCartType == ShoppingCartType.ShoppingCart && !_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart, customer))
            {
                warnings.Add("Shopping cart is disabled");
                return warnings;
            }
            if (shoppingCartType == ShoppingCartType.Wishlist && !_permissionService.Authorize(StandardPermissionProvider.EnableWishlist, customer))
            {
                warnings.Add("Wishlist is disabled");
                return warnings;
            }


            //reset checkout info
            _customerService.ResetCheckoutData(customer, false);

            var cart = customer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == shoppingCartType).ToList();

            ShoppingCartItem shoppingCartItem = FindShoppingCartItemInTheCart(cart,
                shoppingCartType, productVariant, selectedAttributes, customerEnteredPrice);

            if (shoppingCartItem != null)
            {
                //update existing shopping cart item
                int newQuantity = shoppingCartItem.Quantity + quantity;
                warnings.AddRange(GetShoppingCartItemWarnings(customer, shoppingCartType, productVariant,
                    selectedAttributes, customerEnteredPrice, newQuantity, automaticallyAddRequiredProductVariantsIfEnabled,true));

                if (warnings.Count == 0)
                {
                    shoppingCartItem.AttributesXml = selectedAttributes;
                    shoppingCartItem.Quantity = newQuantity;
                    shoppingCartItem.CustomerComment = CustomerComment;
                    shoppingCartItem.UpdatedOnUtc = DateTime.UtcNow;
                    _customerService.UpdateCustomer(customer);

                    //event notification
                    _eventPublisher.EntityUpdated(shoppingCartItem);
                }
            }
            else
            {
                //new shopping cart item
                warnings.AddRange(GetShoppingCartItemWarnings(customer, shoppingCartType, productVariant,
                    selectedAttributes, customerEnteredPrice, quantity, automaticallyAddRequiredProductVariantsIfEnabled));
                if (warnings.Count == 0)
                {
                    //maximum items validation
                    switch (shoppingCartType)
                    {
                        case ShoppingCartType.ShoppingCart:
                            {
                                if (cart.Count >= _shoppingCartSettings.MaximumShoppingCartItems)
                                    return warnings;
                            }
                            break;
                        case ShoppingCartType.Wishlist:
                            {
                                if (cart.Count >= _shoppingCartSettings.MaximumWishlistItems)
                                    return warnings;
                            }
                            break;
                        default:
                            break;
                    }

                    DateTime now = DateTime.UtcNow;
                    shoppingCartItem = new ShoppingCartItem()
                    {
                        ShoppingCartType = shoppingCartType,
                        ProductVariant = productVariant,
                        AttributesXml = selectedAttributes,
                        CustomerEnteredPrice = customerEnteredPrice,
                        Quantity = quantity,
                        CreatedOnUtc = now,
                        UpdatedOnUtc = now
                    };
                    customer.ShoppingCartItems.Add(shoppingCartItem);
                    _customerService.UpdateCustomer(customer);

                    //event notification
                    _eventPublisher.EntityInserted(shoppingCartItem);
                }
            }

            return warnings;
        }

        public virtual IList<string> GetShoppingCartItemWarnings(Customer customer, ShoppingCartType shoppingCartType,
            ProductVariant productVariant, string selectedAttributes, decimal customerEnteredPrice,
            int quantity, bool automaticallyAddRequiredProductVariantsIfEnabled, bool fromWishList)
        {
            //WishList
            if (!fromWishList)
            {
                var warningsFromShoppingCart = GetShoppingCartItemWarnings(customer, shoppingCartType, productVariant, selectedAttributes,
                                           customerEnteredPrice, quantity,
                                           automaticallyAddRequiredProductVariantsIfEnabled);
                return warningsFromShoppingCart;
            }


            if (productVariant == null)
                throw new ArgumentNullException("productVariant");
            var warnings = new List<string>();
            //TODO: include some warnings
            if (shoppingCartType == ShoppingCartType.Wishlist)
                return warnings;
            string productName = string.IsNullOrWhiteSpace(productVariant.GetLocalized(x=>x.Name)) ? productVariant.Product.GetLocalized(x=>x.Name) : productVariant.GetLocalized(x=>x.Name);
            var product = productVariant.Product;
            if (product == null)
            {
                warnings.Add(string.Format(_localizationService.GetResource("WishList.CannotLoadProduct"), productVariant.ProductId));
                return warnings;
            }

            if (product.Deleted || productVariant.Deleted)
            {
                warnings.Add(_localizationService.GetResource("WishList.ProductDeleted"));
                return warnings;
            }

            if (!product.Published || !productVariant.Published)
            {
                warnings.Add(_localizationService.GetResource("WishList.ProductUnpublished"));
            }

            if (shoppingCartType == ShoppingCartType.ShoppingCart && productVariant.DisableBuyButton)
            {
                warnings.Add(_localizationService.GetResource("WishList.BuyingDisabled"));
            }

            if (shoppingCartType == ShoppingCartType.Wishlist && productVariant.DisableWishlistButton)
            {
                warnings.Add(_localizationService.GetResource("WishList.WishlistDisabled"));
            }

            if (shoppingCartType == ShoppingCartType.ShoppingCart &&
                productVariant.CallForPrice && !productVariant.CallforPriceRequested(customer))
            {
                warnings.Add(_localizationService.GetResource("WishList.CallForPrice"));
            }

            if (productVariant.CustomerEntersPrice)
            {
                if (customerEnteredPrice < productVariant.MinimumCustomerEnteredPrice ||
                    customerEnteredPrice > productVariant.MaximumCustomerEnteredPrice)
                {
                    decimal minimumCustomerEnteredPrice = _currencyService.ConvertFromPrimaryStoreCurrency(productVariant.MinimumCustomerEnteredPrice, _workContext.WorkingCurrency);
                    decimal maximumCustomerEnteredPrice = _currencyService.ConvertFromPrimaryStoreCurrency(productVariant.MaximumCustomerEnteredPrice, _workContext.WorkingCurrency);
                    warnings.Add(string.Format(_localizationService.GetResource("WishList.CustomerEnteredPrice.RangeError"),
                        _priceFormatter.FormatPrice(minimumCustomerEnteredPrice, false, false),
                        _priceFormatter.FormatPrice(maximumCustomerEnteredPrice, false, false)));
                }
            }

            if (quantity < productVariant.OrderMinimumQuantity)
            {
                warnings.Add(string.Format(_localizationService.GetResource("WishList.MinimumQuantity"),productName, productVariant.OrderMinimumQuantity));
            }

            if (quantity > productVariant.OrderMaximumQuantity)
            {
                warnings.Add(string.Format(_localizationService.GetResource("WishList.MaximumQuantity"), productName, productVariant.OrderMaximumQuantity));
            }

            switch (productVariant.ManageInventoryMethod)
            {
                case ManageInventoryMethod.DontManageStock:
                    {
                    }
                    break;
                case ManageInventoryMethod.ManageStock:
                    {
                        if ((BackorderMode)productVariant.BackorderMode == BackorderMode.NoBackorders)
                        {
                            if (productVariant.StockQuantity < quantity)
                            {
                                int maximumQuantityCanBeAdded = productVariant.StockQuantity;
                                if (maximumQuantityCanBeAdded <= 0)
                                    warnings.Add(_localizationService.GetResource("WishList.OutOfStock"));
                                else
                                    warnings.Add(string.Format(_localizationService.GetResource("WishList.QuantityExceedsStock"),productName, maximumQuantityCanBeAdded));
                            }
                        }
                    }
                    break;
                case ManageInventoryMethod.ManageStockByAttributes:
                    {
                        var combinations = productVariant.ProductVariantAttributeCombinations;
                        ProductVariantAttributeCombination combination = null;
                        foreach (var comb1 in combinations)
                            if (_productAttributeParser.AreProductAttributesEqual(comb1.AttributesXml, selectedAttributes))
                                combination = comb1;
                        if (combination != null)
                        {
                            if (!combination.AllowOutOfStockOrders)
                            {
                                if (combination.StockQuantity < quantity)
                                {
                                    int maximumQuantityCanBeAdded = combination.StockQuantity;
                                    if (maximumQuantityCanBeAdded <= 0)
                                        warnings.Add(_localizationService.GetResource("WishList.OutOfStock"));
                                    else
                                        warnings.Add(string.Format(_localizationService.GetResource("WishList.QuantityExceedsStock"), productName, maximumQuantityCanBeAdded));
                                }
                            }
                        }
                    }
                    break;
                default:
                    break;
            }

            //availability dates
            bool availableStartDateError = false;
            if (productVariant.AvailableStartDateTimeUtc.HasValue)
            {
                DateTime now = DateTime.UtcNow;
                DateTime availableStartDateTime = DateTime.SpecifyKind(productVariant.AvailableStartDateTimeUtc.Value, DateTimeKind.Utc);
                if (availableStartDateTime.CompareTo(now) > 0)
                {
                    warnings.Add(_localizationService.GetResource("WishList.NotAvailable"));
                    availableStartDateError = true;
                }
            }
            if (productVariant.AvailableEndDateTimeUtc.HasValue && !availableStartDateError)
            {
                DateTime now = DateTime.UtcNow;
                DateTime availableEndDateTime = DateTime.SpecifyKind(productVariant.AvailableEndDateTimeUtc.Value, DateTimeKind.Utc);
                if (availableEndDateTime.CompareTo(now) < 0)
                {
                    warnings.Add(_localizationService.GetResource("WishList.NotAvailable"));
                }
            }

            //selected attributes
            warnings.AddRange(GetShoppingCartItemAttributeWarnings(shoppingCartType, productVariant, selectedAttributes));

            //gift cards
            warnings.AddRange(GetShoppingCartItemGiftCardWarnings(shoppingCartType, productVariant, selectedAttributes));

            //required product variants
            warnings.AddRange(GetRequiredProductVariantWarnings(customer, shoppingCartType, productVariant, automaticallyAddRequiredProductVariantsIfEnabled));

            return warnings;
        }



        public virtual IList<string> AddToCartByList(IEnumerable<ShoppingCartItem> Items, Customer customer, ShoppingCartType shoppingCartType, int quantity, bool automaticallyAddRequiredProductVariantsIfEnabled)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var warnings = new List<string>();
            if (shoppingCartType == ShoppingCartType.ShoppingCart && !_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart, customer))
            {
                warnings.Add("Shopping cart is disabled");
                return warnings;
            }
            if (shoppingCartType == ShoppingCartType.Wishlist && !_permissionService.Authorize(StandardPermissionProvider.EnableWishlist, customer))
            {
                warnings.Add("Wishlist is disabled");
                return warnings;
            }

            if (Items == null)
            {
                warnings.Add("Common.Error.Header");
            }

            
            var cart = customer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == shoppingCartType).ToList();

            foreach (var item in Items)
            {
                ShoppingCartItem shoppingCartItem = FindShoppingCartItemInTheCart(cart,
                    ShoppingCartType.ShoppingCart, item.ProductVariant, item.AttributesXml);

                if (shoppingCartItem != null)
                {
                    var newQuantity = quantity + shoppingCartItem.Quantity;
                    warnings.AddRange(GetShoppingCartItemWarnings(customer, shoppingCartType, item.ProductVariant,
                        item.AttributesXml, item.CustomerEnteredPrice, newQuantity, automaticallyAddRequiredProductVariantsIfEnabled, true));
                }
            }

            if (warnings.Count <= 0)
            {
                foreach (var item in Items)
                {
                    warnings.AddRange(AddToCartFromWishList(customer, item.ProductVariant, shoppingCartType, item.AttributesXml, item.CustomerEnteredPrice, 1, automaticallyAddRequiredProductVariantsIfEnabled,""));
                }
            }
            return warnings;
        }

        public virtual IList<string> AddToCart(Customer customer, ProductVariant productVariant,
         ShoppingCartType shoppingCartType, string selectedAttributes,
         decimal customerEnteredPrice, int quantity, bool automaticallyAddRequiredProductVariantsIfEnabled, bool forceAttributeSelection)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            if (productVariant == null)
                throw new ArgumentNullException("productVariant");

            var warnings = new List<string>();
            if (shoppingCartType == ShoppingCartType.ShoppingCart && !_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart, customer))
            {
                warnings.Add("Shopping cart is disabled");
                return warnings;
            }
            if (shoppingCartType == ShoppingCartType.Wishlist && !_permissionService.Authorize(StandardPermissionProvider.EnableWishlist, customer))
            {
                warnings.Add("Wishlist is disabled");
                return warnings;
            }


            //reset checkout info
            _customerService.ResetCheckoutData(customer, false);

            var cart = customer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == shoppingCartType).ToList();

            ShoppingCartItem shoppingCartItem = FindShoppingCartItemInTheCart(cart,
                shoppingCartType, productVariant, selectedAttributes, customerEnteredPrice);

            if (shoppingCartItem != null)
            {
                //update existing shopping cart item
                int newQuantity = shoppingCartItem.Quantity + quantity;
                warnings.AddRange(GetShoppingCartItemWarnings(customer, shoppingCartType, productVariant,
                    selectedAttributes, customerEnteredPrice, newQuantity, automaticallyAddRequiredProductVariantsIfEnabled, false, forceAttributeSelection));

                if (warnings.Count == 0)
                {
                    shoppingCartItem.AttributesXml = selectedAttributes;
                    shoppingCartItem.Quantity = newQuantity;
                    shoppingCartItem.UpdatedOnUtc = DateTime.UtcNow;
                    _customerService.UpdateCustomer(customer);

                    //event notification
                    _eventPublisher.EntityUpdated(shoppingCartItem);
                }
            }
            else
            {
                //new shopping cart item
                warnings.AddRange(GetShoppingCartItemWarnings(customer, shoppingCartType, productVariant,
                    selectedAttributes, customerEnteredPrice, quantity, automaticallyAddRequiredProductVariantsIfEnabled, false, true));
                if (warnings.Count == 0)
                {
                    //maximum items validation
                    switch (shoppingCartType)
                    {
                        case ShoppingCartType.ShoppingCart:
                            {
                                if (cart.Count >= _shoppingCartSettings.MaximumShoppingCartItems)
                                    return warnings;
                            }
                            break;
                        case ShoppingCartType.Wishlist:
                            {
                                if (cart.Count >= _shoppingCartSettings.MaximumWishlistItems)
                                    return warnings;
                            }
                            break;
                        default:
                            break;
                    }

                    DateTime now = DateTime.UtcNow;
                    shoppingCartItem = new ShoppingCartItem()
                    {
                        ShoppingCartType = shoppingCartType,
                        ProductVariant = productVariant,
                        AttributesXml = selectedAttributes,
                        CustomerEnteredPrice = customerEnteredPrice,
                        Quantity = quantity,
                        CreatedOnUtc = now,
                        UpdatedOnUtc = now
                    };
                    customer.ShoppingCartItems.Add(shoppingCartItem);
                    _customerService.UpdateCustomer(customer);

                    //event notification
                    _eventPublisher.EntityInserted(shoppingCartItem);
                }
            }

            return warnings;
        }

        public virtual IList<string> GetShoppingCartItemWarnings(Customer customer, ShoppingCartType shoppingCartType,
          ProductVariant productVariant, string selectedAttributes, decimal customerEnteredPrice,
          int quantity, bool automaticallyAddRequiredProductVariantsIfEnabled, bool fromWishList, bool forceAttributeSelection)
        {
            if (productVariant == null)
                throw new ArgumentNullException("productVariant");

            var warnings = new List<string>();
            //TODO: include some warnings
            //if (shoppingCartType == ShoppingCartType.Wishlist)
            //    return warnings;

            var product = productVariant.Product;
            if (product == null)
            {
                warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.CannotLoadProduct"), productVariant.ProductId));
                return warnings;
            }

            if (product.Deleted || productVariant.Deleted)
            {
                warnings.Add(_localizationService.GetResource("ShoppingCart.ProductDeleted"));
                return warnings;
            }

            if (!product.Published || !productVariant.Published)
            {
                warnings.Add(_localizationService.GetResource("ShoppingCart.ProductUnpublished"));
            }

            if (shoppingCartType == ShoppingCartType.ShoppingCart && productVariant.DisableBuyButton)
            {
                warnings.Add(_localizationService.GetResource("ShoppingCart.BuyingDisabled"));
            }

            if (shoppingCartType == ShoppingCartType.Wishlist && productVariant.DisableWishlistButton)
            {
                warnings.Add(_localizationService.GetResource("ShoppingCart.WishlistDisabled"));
            }

            if (shoppingCartType == ShoppingCartType.ShoppingCart &&
                productVariant.CallForPrice && !productVariant.CallforPriceRequested(customer))
            {
                warnings.Add(_localizationService.GetResource("ShoppingCart.CallForPriceNotAllowed"));
            }

            if (productVariant.CustomerEntersPrice)
            {
                if (customerEnteredPrice < productVariant.MinimumCustomerEnteredPrice ||
                    customerEnteredPrice > productVariant.MaximumCustomerEnteredPrice)
                {
                    decimal minimumCustomerEnteredPrice = _currencyService.ConvertFromPrimaryStoreCurrency(productVariant.MinimumCustomerEnteredPrice, _workContext.WorkingCurrency);
                    decimal maximumCustomerEnteredPrice = _currencyService.ConvertFromPrimaryStoreCurrency(productVariant.MaximumCustomerEnteredPrice, _workContext.WorkingCurrency);
                    warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.CustomerEnteredPrice.RangeError"),
                        _priceFormatter.FormatPrice(minimumCustomerEnteredPrice, false, false),
                        _priceFormatter.FormatPrice(maximumCustomerEnteredPrice, false, false)));
                }
            }

            if (quantity < productVariant.OrderMinimumQuantity)
            {
                warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.MinimumQuantity"), productVariant.OrderMinimumQuantity));
            }

            if (quantity > productVariant.OrderMaximumQuantity)
            {
                warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.MaximumQuantity"), productVariant.OrderMaximumQuantity));
            }

            switch (productVariant.ManageInventoryMethod)
            {
                case ManageInventoryMethod.DontManageStock:
                    {
                    }
                    break;
                case ManageInventoryMethod.ManageStock:
                    {
                        if ((BackorderMode)productVariant.BackorderMode == BackorderMode.NoBackorders)
                        {
                            if (productVariant.StockQuantity < quantity)
                            {
                                int maximumQuantityCanBeAdded = productVariant.StockQuantity;
                                if (maximumQuantityCanBeAdded <= 0)
                                    warnings.Add(_localizationService.GetResource("ShoppingCart.OutOfStock"));
                                else
                                    warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.QuantityExceedsStock"), maximumQuantityCanBeAdded));
                            }
                        }
                    }
                    break;
                case ManageInventoryMethod.ManageStockByAttributes:
                    {
                        var combinations = productVariant.ProductVariantAttributeCombinations;
                        ProductVariantAttributeCombination combination = null;
                        foreach (var comb1 in combinations)
                            if (_productAttributeParser.AreProductAttributesEqual(comb1.AttributesXml, selectedAttributes))
                                combination = comb1;
                        if (combination != null)
                        {
                            if (!combination.AllowOutOfStockOrders)
                            {
                                if (combination.StockQuantity < quantity)
                                {
                                    int maximumQuantityCanBeAdded = combination.StockQuantity;
                                    if (maximumQuantityCanBeAdded <= 0)
                                        warnings.Add(_localizationService.GetResource("ShoppingCart.OutOfStock"));
                                    else
                                        warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.QuantityExceedsStock"), maximumQuantityCanBeAdded));
                                }
                            }
                        }
                        else if (forceAttributeSelection)
                        {
                            warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.AttributesNotSelected")));
                        }
                    }
                    break;
                default:
                    break;
            }

            //availability dates
            bool availableStartDateError = false;
            if (productVariant.AvailableStartDateTimeUtc.HasValue)
            {
                DateTime now = DateTime.UtcNow;
                DateTime availableStartDateTime = DateTime.SpecifyKind(productVariant.AvailableStartDateTimeUtc.Value, DateTimeKind.Utc);
                if (availableStartDateTime.CompareTo(now) > 0)
                {
                    warnings.Add(_localizationService.GetResource("ShoppingCart.NotAvailable"));
                    availableStartDateError = true;
                }
            }
            if (productVariant.AvailableEndDateTimeUtc.HasValue && !availableStartDateError)
            {
                DateTime now = DateTime.UtcNow;
                DateTime availableEndDateTime = DateTime.SpecifyKind(productVariant.AvailableEndDateTimeUtc.Value, DateTimeKind.Utc);
                if (availableEndDateTime.CompareTo(now) < 0)
                {
                    warnings.Add(_localizationService.GetResource("ShoppingCart.NotAvailable"));
                }
            }

            //selected attributes
            warnings.AddRange(GetShoppingCartItemAttributeWarnings(shoppingCartType, productVariant, selectedAttributes));

            //gift cards
            warnings.AddRange(GetShoppingCartItemGiftCardWarnings(shoppingCartType, productVariant, selectedAttributes));

            //required product variants
            warnings.AddRange(GetRequiredProductVariantWarnings(customer, shoppingCartType, productVariant, automaticallyAddRequiredProductVariantsIfEnabled));

            return warnings;
        }
        
        #endregion
    }
}
