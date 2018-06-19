using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Plugins;
using Nop.Services.Discounts;
using Nop.Services.Localization;

namespace Nop.Plugin.DiscountRules.HasSomeProducts
{
    public partial class HasSomeProductsDiscountRequirementRule : BasePlugin, IDiscountRequirementRule
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

            if (String.IsNullOrWhiteSpace(request.DiscountRequirement.RestrictedProductVariantIds))
                return true;

            if (request.Customer == null)
                return false;

            var restrictedProductVariantIds = new List<int>();
            int adet = 1;
            try
            {
                string kisit = request.DiscountRequirement.RestrictedProductVariantIds;
                string _adet = kisit.Substring(kisit.IndexOf("-") + 1);
                adet = Convert.ToInt32(_adet);
                kisit = kisit.Replace("-", "");
                restrictedProductVariantIds = kisit
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToList();
            }
            catch
            {
                //error parsing
                return false;
            }
            if (restrictedProductVariantIds.Count == 0)
                return false;

            //cart
            var cart = request.Customer.ShoppingCartItems.Where(x => x.ShoppingCartType == ShoppingCartType.ShoppingCart);

            int bulunanAdet = 0;
            foreach (var restrictedPvId in restrictedProductVariantIds)
            {
                foreach (var sci in cart)
                {
                    if (restrictedPvId == sci.ProductVariantId)
                    {
                        bulunanAdet = bulunanAdet + sci.Quantity;
                        break;
                    }
                }
            }

            if (bulunanAdet >= adet)
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
            string result = "Plugins/DiscountRulesHasSomeProducts/Configure/?discountId=" + discountId;
            if (discountRequirementId.HasValue)
                result += string.Format("&discountRequirementId={0}", discountRequirementId.Value);
            return result;
        }

        public override void Install()
        {
            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasSomeProducts.Fields.ProductVariants", "Restricted product variants");
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasSomeProducts.Fields.ProductVariants.Hint", "The comma-separated list of product variant identifiers (e.g. 77, 123, 156). You can find a product variant ID on its details page.");
            base.Install();
        }

        public override void Uninstall()
        {
            //locales
            this.DeletePluginLocaleResource("Plugins.DiscountRules.HasSomeProducts.Fields.ProductVariants");
            this.DeletePluginLocaleResource("Plugins.DiscountRules.HasSomeProducts.Fields.ProductVariants.Hint");
            base.Uninstall();
        }
    }
}