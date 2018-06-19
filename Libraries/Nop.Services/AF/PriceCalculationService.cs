using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Services.Discounts;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Price calculation service
    /// </summary>
    public partial class PriceCalculationService : IPriceCalculationService
    {
        protected virtual IList<Discount> GetPreferredDiscount(ProductVariant productVariant,
           Customer customer, decimal additionalCharge = decimal.Zero, int quantity = 1, bool showHiddenDiscounts=true)
        {
            if (_catalogSettings.IgnoreDiscounts)
                return null;

            var allowedDiscounts = GetAllowedDiscounts(productVariant, customer, showHiddenDiscounts);
            decimal finalPriceWithoutDiscount = GetFinalPrice(productVariant, customer, additionalCharge, false, quantity);
            var preferredDiscounts = allowedDiscounts.GetPreferredDiscounts(finalPriceWithoutDiscount);
            return preferredDiscounts;
        }

        protected virtual IList<Discount> GetPreferredDiscount(ProductVariant productVariant,
          Customer customer,  Discount excludedDiscount, decimal additionalCharge = decimal.Zero, int quantity = 1, bool showHiddenDiscounts = true)
        {
            if (_catalogSettings.IgnoreDiscounts)
                return null;

            var allowedDiscounts = GetAllowedDiscounts(productVariant, customer, showHiddenDiscounts, excludedDiscount);
            decimal finalPriceWithoutDiscount = GetFinalPrice(productVariant, customer, additionalCharge, false, quantity);
            var preferredDiscounts = allowedDiscounts.GetPreferredDiscounts(finalPriceWithoutDiscount);
            
            if(excludedDiscount != null)
                preferredDiscounts.Remove(excludedDiscount);
            return preferredDiscounts;
        }


        protected virtual IList<Discount> GetAllowedDiscounts(ProductVariant productVariant,
        Customer customer,
            bool showHiddenDiscounts, Discount excludedDiscount)
        {
            var allowedDiscounts = new List<Discount>();
            if (_catalogSettings.IgnoreDiscounts)
                return allowedDiscounts;

            foreach (var discount in productVariant.AppliedDiscounts)
            {
                if (excludedDiscount.Id == discount.Id) continue;
                if (_discountService.IsDiscountValid(discount, customer, productVariant) &&
                    discount.DiscountType == DiscountType.AssignedToSkus &&
                    !allowedDiscounts.ContainsDiscount(discount) && (!discount.ShowInCatalog.HasValue || (showHiddenDiscounts || discount.ShowInCatalog.Value)))
                    allowedDiscounts.Add(discount);
            }

            var productCategories = _categoryService.GetProductCategoriesByProductId(productVariant.ProductId);
            if (productCategories != null)
            {
                foreach (var productCategory in productCategories)
                {
                    var categoryDiscounts = productCategory.Category.AppliedDiscounts;
                    foreach (var discount in categoryDiscounts)
                    {
                        if (excludedDiscount.Id == discount.Id) continue;
                        if (_discountService.IsDiscountValid(discount, customer, productVariant) &&
                            discount.DiscountType == DiscountType.AssignedToCategories &&
                            !allowedDiscounts.ContainsDiscount(discount) && (!discount.ShowInCatalog.HasValue || (showHiddenDiscounts || discount.ShowInCatalog.Value)))
                            allowedDiscounts.Add(discount);
                    }
                }
            }

            var productManufacturers = _manufacturerService.GetProductManufacturersByProductId(productVariant.ProductId);
            if (productManufacturers != null)
            {
                foreach (var productManufacturer in productManufacturers)
                {
                    var manufacturerDiscounts = productManufacturer.Manufacturer.AppliedDiscounts;
                    foreach (var discount in manufacturerDiscounts)
                    {
                        if (excludedDiscount.Id == discount.Id) continue;
                        if (_discountService.IsDiscountValid(discount, customer, productVariant) &&
                            discount.DiscountType == DiscountType.AssignedToManufacturers &&
                            !allowedDiscounts.ContainsDiscount(discount) && (!discount.ShowInCatalog.HasValue || (showHiddenDiscounts || discount.ShowInCatalog.Value)))
                            allowedDiscounts.Add(discount);
                    }
                }
            }

            //AF
            var globalDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToAll);
            foreach (var discount in globalDiscounts)
            {
                if (excludedDiscount.Id == discount.Id) continue;
                if (_discountService.IsDiscountValid(discount, customer, productVariant) &&
                    !allowedDiscounts.ContainsDiscount(discount) && (!discount.ShowInCatalog.HasValue || (showHiddenDiscounts || discount.ShowInCatalog.Value)))
                    allowedDiscounts.Add(discount);
            }
            return allowedDiscounts;

        }

        protected virtual IList<Discount> GetAllowedDiscounts(ProductVariant productVariant, Customer customer, bool showHiddenDiscounts)
        {
            var allowedDiscounts = new List<Discount>();

            if (_catalogSettings.IgnoreDiscounts)
                return allowedDiscounts;

            foreach (var discount in productVariant.AppliedDiscounts)
            {
                if (_discountService.IsDiscountValid(discount, customer, productVariant) &&
                    discount.DiscountType == DiscountType.AssignedToSkus &&
                    !allowedDiscounts.ContainsDiscount(discount) && (!discount.ShowInCatalog.HasValue || (showHiddenDiscounts || discount.ShowInCatalog.Value)))
                    allowedDiscounts.Add(discount);
            }

            var productCategories = _categoryService.GetProductCategoriesByProductId(productVariant.ProductId);
            if (productCategories != null)
            {
                foreach (var productCategory in productCategories)
                {
                    var categoryDiscounts = productCategory.Category.AppliedDiscounts;
					//var categoryDiscounts = globalDiscounts.Where(x => x.AppliedToCategories.Contains(productCategory.Category));
					
                    foreach (var discount in categoryDiscounts)
                    {
                        if (_discountService.IsDiscountValid(discount, customer, productVariant) &&
                            discount.DiscountType == DiscountType.AssignedToCategories &&
                            !allowedDiscounts.ContainsDiscount(discount) && (!discount.ShowInCatalog.HasValue || (showHiddenDiscounts || discount.ShowInCatalog.Value)))
                            allowedDiscounts.Add(discount);
                    }
                }
            }

            var productManufacturers = _manufacturerService.GetProductManufacturersByProductId(productVariant.ProductId);
            if (productManufacturers != null)
            {
                foreach (var productManufacturer in productManufacturers)
                {
                    var manufacturerDiscounts = productManufacturer.Manufacturer.AppliedDiscounts;
                    foreach (var discount in manufacturerDiscounts)
                    {
                        if (_discountService.IsDiscountValid(discount, customer, productVariant) &&
                            discount.DiscountType == DiscountType.AssignedToManufacturers &&
                            !allowedDiscounts.ContainsDiscount(discount) && (!discount.ShowInCatalog.HasValue || (showHiddenDiscounts || discount.ShowInCatalog.Value)))
                            allowedDiscounts.Add(discount);
                    }
                }
            }

            //AF
            var globalDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToAll);
            foreach (var discount in globalDiscounts)
            {
                if (_discountService.IsDiscountValid(discount, customer, productVariant) &&
                    !allowedDiscounts.ContainsDiscount(discount) && (!discount.ShowInCatalog.HasValue || (showHiddenDiscounts || discount.ShowInCatalog.Value)))
                    allowedDiscounts.Add(discount);
            }
            return allowedDiscounts;

        }

        #region Methods


        public virtual decimal GetSubTotal(ShoppingCartItem shoppingCartItem, bool includeDiscounts, Discount exludedDiscount)
        {
            return GetUnitPrice(shoppingCartItem, includeDiscounts, exludedDiscount) * shoppingCartItem.Quantity;
        }
        public virtual decimal GetUnitPrice(ShoppingCartItem shoppingCartItem, bool includeDiscounts, Discount excludedDiscount)
        {
            var customer = shoppingCartItem.Customer;
            decimal finalPrice = decimal.Zero;
            var productVariant = shoppingCartItem.ProductVariant;
            if (productVariant != null)
            {
                decimal attributesTotalPrice = decimal.Zero;

                var pvaValues = _productAttributeParser.ParseProductVariantAttributeValues(shoppingCartItem.AttributesXml);
                if (pvaValues != null)
                {
                    foreach (var pvaValue in pvaValues)
                    {
                        attributesTotalPrice += pvaValue.PriceAdjustment;
                    }
                }

                if (productVariant.CustomerEntersPrice)
                {
                    finalPrice = shoppingCartItem.CustomerEnteredPrice;
                }
                else
                {
                    finalPrice = GetFinalPrice(productVariant,
                                               customer,
                                               attributesTotalPrice,
                                               includeDiscounts,
                                               shoppingCartItem.Quantity, excludedDiscount);
                }
            }

            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                finalPrice = Math.Round(finalPrice, 2);

            return finalPrice;
        }
        



        /// <summary>
        /// Gets the final price
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for final price computation</param>
        /// <returns>Final price</returns>
        public virtual decimal GetFinalPrice(ProductVariant productVariant,
            bool includeDiscounts, bool showHiddenDiscounts = true)
        {
            var customer = _workContext.CurrentCustomer;
            return GetFinalPrice(productVariant, customer, includeDiscounts, showHiddenDiscounts);
        }

        /// <summary>
        /// Gets the final price
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for final price computation</param>
        /// <returns>Final price</returns>
        public virtual decimal GetFinalPrice(ProductVariant productVariant,
            bool includeDiscounts,Customer customer, bool showHiddenDiscounts = true)
        {
            return GetFinalPrice(productVariant, customer, includeDiscounts, showHiddenDiscounts);
        }

        /// <summary>
        /// Gets the final price
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        /// <param name="customer">The customer</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for final price computation</param>
        /// <returns>Final price</returns>
        public virtual decimal GetFinalPrice(ProductVariant productVariant,
            Customer customer,
            bool includeDiscounts,
            bool showHiddenDiscounts=true)
        {
            return GetFinalPrice(productVariant, customer, decimal.Zero, includeDiscounts, showHiddenDiscounts);
        }

        /// <summary>
        /// Gets the final price
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        /// <param name="customer">The customer</param>
        /// <param name="additionalCharge">Additional charge</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for final price computation</param>
        /// <returns>Final price</returns>
        public virtual decimal GetFinalPrice(ProductVariant productVariant,
            Customer customer,
            decimal additionalCharge,
            bool includeDiscounts,
            bool showHiddenDiscounts=true)
        {
            return GetFinalPrice(productVariant, customer, additionalCharge,
                includeDiscounts, 1, showHiddenDiscounts);
        }

        /// <summary>
        /// Gets the final price
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        /// <param name="customer">The customer</param>
        /// <param name="additionalCharge">Additional charge</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for final price computation</param>
        /// <param name="quantity">Shopping cart item quantity</param>
        /// <returns>Final price</returns>
        public virtual decimal GetFinalPrice(ProductVariant productVariant,
            Customer customer,
            decimal additionalCharge,
            bool includeDiscounts,
            int quantity,
            bool showHiddenDiscounts = true)
        {
            //initial price

            decimal result = productVariant.Price;
          

            //special price
            var specialPrice = GetSpecialPrice(productVariant);
            if (specialPrice.HasValue)
                result = specialPrice.Value;

            //tier prices)
            if (!_catalogSettings.IgnoreTierPrices && productVariant.TierPrices.Count > 0)
            {
                decimal? tierPrice = GetMinimumTierPrice(productVariant, customer, quantity);
                if (tierPrice.HasValue)
                    result = Math.Min(result, tierPrice.Value);
            }

            //discount + additional charge
            if (includeDiscounts)
            {
                IList<Discount> appliedDiscounts = null;
                decimal discountAmount = GetDiscountAmount(productVariant, customer, additionalCharge, quantity, out appliedDiscounts, showHiddenDiscounts);
                result = result + additionalCharge - discountAmount;
            }
            else
            {
                result = result + additionalCharge;
            }
            if (result < decimal.Zero)
                result = decimal.Zero;
            return result;
        }

        #region getfinalprice with applied discount list
        /// <summary>
        /// Gets the final price
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for final price computation</param>
        /// <returns>Final price</returns>
        public virtual decimal GetFinalPrice(ProductVariant productVariant,
            bool includeDiscounts, out IList<Discount> appliedDiscounts, bool showHiddenDiscounts = true)
        {
            var customer = _workContext.CurrentCustomer;
            return GetFinalPrice(productVariant, customer, includeDiscounts, out appliedDiscounts, showHiddenDiscounts);
        }

        /// <summary>
        /// Gets the final price
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for final price computation</param>
        /// <returns>Final price</returns>
        public virtual decimal GetFinalPrice(ProductVariant productVariant,
            bool includeDiscounts, Customer customer, out IList<Discount> appliedDiscounts, bool showHiddenDiscounts = true)
        {
            return GetFinalPrice(productVariant, customer, includeDiscounts, out appliedDiscounts, showHiddenDiscounts);
        }

        /// <summary>
        /// Gets the final price
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        /// <param name="customer">The customer</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for final price computation</param>
        /// <returns>Final price</returns>
        public virtual decimal GetFinalPrice(ProductVariant productVariant,
            Customer customer,
            bool includeDiscounts,
             out IList<Discount> appliedDiscounts,
            bool showHiddenDiscounts = true)
        {
            return GetFinalPrice(productVariant, customer, decimal.Zero, includeDiscounts, out appliedDiscounts, showHiddenDiscounts);
        }

        /// <summary>
        /// Gets the final price
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        /// <param name="customer">The customer</param>
        /// <param name="additionalCharge">Additional charge</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for final price computation</param>
        /// <returns>Final price</returns>
        public virtual decimal GetFinalPrice(ProductVariant productVariant,
            Customer customer,
            decimal additionalCharge,
            bool includeDiscounts,
             out IList<Discount> appliedDiscounts,
            bool showHiddenDiscounts = true)
        {
            return GetFinalPrice(productVariant, customer, additionalCharge,
                includeDiscounts, 1, out appliedDiscounts, showHiddenDiscounts);
        }

        /// <summary>
        /// Gets the final price
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        /// <param name="customer">The customer</param>
        /// <param name="additionalCharge">Additional charge</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for final price computation</param>
        /// <param name="quantity">Shopping cart item quantity</param>
        /// <returns>Final price</returns>
        public virtual decimal GetFinalPrice(ProductVariant productVariant,
            Customer customer,
            decimal additionalCharge,
            bool includeDiscounts,
            int quantity,
             out IList<Discount> appliedDiscounts,
            bool showHiddenDiscounts = true)
        {
            //initial price
            decimal result = productVariant.Price;
         

            appliedDiscounts = null;

            //special price
            var specialPrice = GetSpecialPrice(productVariant);
            if (specialPrice.HasValue)
                result = specialPrice.Value;

            //tier prices)
            if (!_catalogSettings.IgnoreTierPrices && productVariant.TierPrices.Count > 0)
            {
                decimal? tierPrice = GetMinimumTierPrice(productVariant, customer, quantity);
                if (tierPrice.HasValue)
                    result = Math.Min(result, tierPrice.Value);
            }

            //discount + additional charge
            if (includeDiscounts)
            {
                appliedDiscounts = null;
                decimal discountAmount = GetDiscountAmount(productVariant, customer, additionalCharge, quantity, out appliedDiscounts, showHiddenDiscounts);
                result = result + additionalCharge - discountAmount;
            }
            else
            {
                result = result + additionalCharge;
            }
            if (result < decimal.Zero)
                result = decimal.Zero;
            return result;
        }



  #endregion getfinalprice with applied discount list


        public virtual decimal GetFinalPrice(ProductVariant productVariant,
          Customer customer,
          decimal additionalCharge,
          bool includeDiscounts,
          int quantity,
          Discount excludedDiscount,
          bool showHiddenDiscounts = true)
        {
            //initial price
            decimal result = productVariant.Price;

            //special price
            var specialPrice = GetSpecialPrice(productVariant);
            if (specialPrice.HasValue)
                result = specialPrice.Value;

            //tier prices)
            if (!_catalogSettings.IgnoreTierPrices && productVariant.TierPrices.Count > 0)
            {
                decimal? tierPrice = GetMinimumTierPrice(productVariant, customer, quantity);
                if (tierPrice.HasValue)
                    result = Math.Min(result, tierPrice.Value);
            }

            //discount + additional charge
            if (includeDiscounts)
            {
                IList<Discount> appliedDiscounts = null;
                decimal discountAmount = GetDiscountAmount(productVariant, customer, additionalCharge, quantity, out appliedDiscounts,excludedDiscount, showHiddenDiscounts);
                result = result + additionalCharge - discountAmount;
            }
            else
            {
                result = result + additionalCharge;
            }
            if (result < decimal.Zero)
                result = decimal.Zero;
            return result;
        }


        /// <summary>
        /// Gets discount amount
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        /// <returns>Discount amount</returns>
        public virtual decimal GetDiscountAmount(ProductVariant productVariant, bool showHiddenDiscounts = true)
        {
            var customer = _workContext.CurrentCustomer;
            return GetDiscountAmount(productVariant, customer, decimal.Zero, showHiddenDiscounts);
        }

        /// <summary>
        /// Gets discount amount
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        /// <param name="customer">The customer</param>
        /// <returns>Discount amount</returns>
        public virtual decimal GetDiscountAmount(ProductVariant productVariant,
            Customer customer, bool showHiddenDiscounts = true)
        {
            return GetDiscountAmount(productVariant, customer, decimal.Zero, showHiddenDiscounts);
        }

        /// <summary>
        /// Gets discount amount
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        /// <param name="customer">The customer</param>
        /// <param name="additionalCharge">Additional charge</param>
        /// <returns>Discount amount</returns>
        public virtual decimal GetDiscountAmount(ProductVariant productVariant,
            Customer customer,
            decimal additionalCharge,
            bool showHiddenDiscounts = true)
        {
            IList<Discount> appliedDiscounts = null;
            return GetDiscountAmount(productVariant, customer, additionalCharge, out appliedDiscounts, showHiddenDiscounts);
        }

        /// <summary>
        /// Gets discount amount
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        /// <param name="customer">The customer</param>
        /// <param name="additionalCharge">Additional charge</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <returns>Discount amount</returns>
        public virtual decimal GetDiscountAmount(ProductVariant productVariant,
            Customer customer,
            decimal additionalCharge,
            out IList<Discount> appliedDiscounts,
            bool showHiddenDiscounts = true)
        {
            return GetDiscountAmount(productVariant, customer, additionalCharge, 1, out appliedDiscounts, showHiddenDiscounts);
        }

        /// <summary>
        /// Gets discount amount
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        /// <param name="customer">The customer</param>
        /// <param name="additionalCharge">Additional charge</param>
        /// <param name="quantity">Product quantity</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <returns>Discount amount</returns>
        public virtual decimal GetDiscountAmount(ProductVariant productVariant,
            Customer customer,
            decimal additionalCharge,
            int quantity,
            out IList<Discount> appliedDiscounts,
            bool showHiddenDiscounts = true)
        {
           
            decimal appliedDiscountAmount = decimal.Zero;
            appliedDiscounts = null;
            //we don't apply discounts to products with price entered by a customer
            if (productVariant.CustomerEntersPrice)
                return appliedDiscountAmount;

            appliedDiscounts = GetPreferredDiscount(productVariant, customer, additionalCharge, quantity, showHiddenDiscounts);
            if (appliedDiscounts != null && appliedDiscounts.Count>0)
            {
                decimal finalPriceWithoutDiscount = GetFinalPrice(productVariant, customer, additionalCharge, false, quantity);
                appliedDiscountAmount = appliedDiscounts.GetDiscountAmount(finalPriceWithoutDiscount);
            }

            return appliedDiscountAmount;
        }


        public virtual decimal GetDiscountAmount(ProductVariant productVariant,
           Customer customer,
           decimal additionalCharge,
           int quantity,
           out IList<Discount> appliedDiscounts,
            Discount excludedDiscount,
           bool showHiddenDiscounts = true)
        {

            decimal appliedDiscountAmount = decimal.Zero;
            appliedDiscounts = null;
            //we don't apply discounts to products with price entered by a customer
            if (productVariant.CustomerEntersPrice)
                return appliedDiscountAmount;

            appliedDiscounts = GetPreferredDiscount(productVariant, customer,excludedDiscount, additionalCharge, quantity, showHiddenDiscounts);
            if (appliedDiscounts != null && appliedDiscounts.Count > 0)
            {
                decimal finalPriceWithoutDiscount = GetFinalPrice(productVariant, customer, additionalCharge, false, quantity);
                appliedDiscountAmount = appliedDiscounts.GetDiscountAmount(finalPriceWithoutDiscount);
            }

            return appliedDiscountAmount;
        }

        #endregion
    }
}
