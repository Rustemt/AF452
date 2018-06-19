using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Plugins;
using Nop.Services.Discounts;
using Nop.Services.Localization;

namespace Nop.Plugin.DiscountRules.HasManufacturer
{
    public partial class HasManufacturerDiscountRequirementRule : BasePlugin, IDiscountRequirementRule
    {
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

            if (String.IsNullOrWhiteSpace(request.DiscountRequirement.RestrictedManufacturerIds))
                return true;

            if (request.Customer == null)
                return false;

            var restrictedItemIds = new List<int>();
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
                return false;
            }
            if (restrictedItemIds.Count == 0)
                return false;

            //cart
            var cart = request.Customer.ShoppingCartItems.Where(x => x.ShoppingCartType == ShoppingCartType.ShoppingCart);
            

            bool found = false;
            foreach (var restrictedPvId in restrictedItemIds)
            {
                foreach (var sci in cart)
                {
                    foreach(var pm in sci.ProductVariant.Product.ProductManufacturers)
                        if (restrictedPvId == pm.ManufacturerId)
                        {
                            found = true;
                            break;
                        }
                }

                if (found)
                {
                    break;
                }
            }

            if (found)
                return true;

            return false;
        }
        public bool CheckRequirement(CheckDiscountRequirementRequest request, out string message)
        {
            message = "";
            return this.CheckRequirement(request);
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
            string result = "Plugins/DiscountRulesHasManufacturer/Configure/?discountId=" + discountId;
            if (discountRequirementId.HasValue)
                result += string.Format("&discountRequirementId={0}", discountRequirementId.Value);
            return result;
        }

        public override void Install()
        {
            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasManufacturer.Fields.Items", "Restricted items");
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasManufacturer.Fields.Items.Hint", "The comma-separated list of item identifiers (e.g. 77, 123, 156).");
            base.Install();
        }

        public override void Uninstall()
        {
            //locales
            this.DeletePluginLocaleResource("Plugins.DiscountRules.HasManufacturer.Fields.Items");
            this.DeletePluginLocaleResource("Plugins.DiscountRules.HasManufacturer.Fields.Items.Hint");
            base.Uninstall();
        }
    }
}