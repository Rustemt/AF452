using System;
using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Plugins;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Catalog;
using System.Collections.Generic;
using Nop.Core.Infrastructure;

namespace Nop.Plugin.DiscountRules.HasCartAmount
{
    public partial class HasCartAmountDiscountRequirementRule : BasePlugin, IDiscountRequirementRule
    {
         IPriceCalculationService _priceCalculationService;
        ILocalizationService _localizationService;
        public HasCartAmountDiscountRequirementRule(IPriceCalculationService priceCalculationService)
        {
            this._priceCalculationService = priceCalculationService;
        }


        /// <summary>
        /// Check discount requirement
        /// </summary>
        /// <param name="request">Object that contains all information required to check the requirement (Current customer, discount, etc)</param>
        /// <returns>true - requirement is met; otherwise, false</returns>
        public bool CheckRequirement(CheckDiscountRequirementRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            if (request.DiscountRequirement == null)
                throw new NopException("Discount requirement is not set");
            
            if (request.DiscountRequirement.SpentAmount == decimal.Zero)
                return true;

            if (request.Customer == null)
                return false;

            //cart
           
            var cart = request.Customer.ShoppingCartItems.Where(x => x.ShoppingCartType == ShoppingCartType.ShoppingCart);
            decimal spentAmount = decimal.Zero;
            var restrictedManufacturerIds = new List<int>();
            if (!string.IsNullOrWhiteSpace(request.DiscountRequirement.RestrictedManufacturerIds))
            {
                try
                {
                    restrictedManufacturerIds = request.DiscountRequirement.RestrictedManufacturerIds
                        .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => Convert.ToInt32(x))
                        .ToList();
                }
                catch
                {
                    //error parsing
                }

            }
            //categories
            var restrictedCategoryIds = new List<int>();
            if (!string.IsNullOrWhiteSpace(request.DiscountRequirement.RestrictedCategoryIds))
            {
                try
                {
                    restrictedCategoryIds = request.DiscountRequirement.RestrictedCategoryIds
                        .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => Convert.ToInt32(x))
                        .ToList();
                }
                catch
                {
                    //error parsing
                }

            }

            if (restrictedManufacturerIds.Count > 0)
            {
                spentAmount = cart.Where(c => restrictedManufacturerIds.Contains(c.ProductVariant.Product.GetDefaultManufacturer().Id)).Sum(c => _priceCalculationService.GetSubTotal(c, true, request.DiscountRequirement.Discount));
            }
            else if (restrictedCategoryIds.Count > 0)
            {
                spentAmount = cart.Where(c => restrictedCategoryIds.Contains(c.ProductVariant.Product.GetDefaultProductCategory().Id)).Sum(c => _priceCalculationService.GetSubTotal(c, true, request.DiscountRequirement.Discount));
            }
            else
            {
                spentAmount = cart.Sum(c => _priceCalculationService.GetSubTotal(c, true, request.DiscountRequirement.Discount));
            }
            return spentAmount >= request.DiscountRequirement.SpentAmount;
        }

        public bool CheckRequirement(CheckDiscountRequirementRequest request, out string message)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            if (request.DiscountRequirement == null)
                throw new NopException("Discount requirement is not set");

            message = "";
            _localizationService = EngineContext.Current.Resolve<ILocalizationService>();

            if (request.DiscountRequirement.SpentAmount == decimal.Zero)
                return true;

            if (request.Customer == null || request.Customer.IsGuest())
                return false;

            //cart

            var cart = request.Customer.ShoppingCartItems.Where(x => x.ShoppingCartType == ShoppingCartType.ShoppingCart);
            decimal spentAmount = decimal.Zero;
            var restrictedItemIds = new List<int>();
            if (!string.IsNullOrWhiteSpace(request.DiscountRequirement.RestrictedManufacturerIds))
            {
                try
                {
                    restrictedItemIds = request.DiscountRequirement.RestrictedManufacturerIds
                        .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => Convert.ToInt32(x))
                        .ToList();
                }
                catch
                {
                    //error parsing
                }

            }
            if (restrictedItemIds.Count > 0)
            {
                spentAmount = cart.Where(c => restrictedItemIds.Contains(c.ProductVariant.Product.GetDefaultManufacturer().Id)).Sum(c => _priceCalculationService.GetSubTotal(c, true, request.DiscountRequirement.Discount));

                //_orderTotalCalculationService
            }
            else
            {
                spentAmount = cart.Sum(c => _priceCalculationService.GetSubTotal(c, true, request.DiscountRequirement.Discount));

            }
            if (spentAmount < request.DiscountRequirement.SpentAmount)
                message = _localizationService.GetResource("Plugins.DiscountRules.HasCartAmount.NotEnoughAmount");
            return spentAmount >= request.DiscountRequirement.SpentAmount;
        }

        /// <summary>
        /// Get URL for rule configuration
        /// </summary>
        /// <param name="discountId">Discount identifier</param>
        /// <param name="discountRequirementId">Discount requirement identifier (if editing)</param>
        /// <returns>URL</returns>
        public string GetConfigurationUrl(int discountId, int? discountRequirementId)
        {
            //configured in RouteProvider.cs
            string result = "Plugins/DiscountRulesHasCartAmount/Configure/?discountId=" + discountId;
            if (discountRequirementId.HasValue)
                result += string.Format("&discountRequirementId={0}", discountRequirementId.Value);
            return result;
        }

        public override void Install()
        {
            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasCartAmount.Fields.Amount", "Required spent amount");
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasCartAmount.Fields.Amount.Hint", "Discount will be applied if customer has spent/purchased x.xx amount.");
            base.Install();
        }

        public override void Uninstall()
        {
            //locales
            this.DeletePluginLocaleResource("Plugins.DiscountRules.HasCartAmount.Fields.Amount");
            this.DeletePluginLocaleResource("Plugins.DiscountRules.HasCartAmount.Fields.Amount.Hint");
            base.Uninstall();
        }
    }
}