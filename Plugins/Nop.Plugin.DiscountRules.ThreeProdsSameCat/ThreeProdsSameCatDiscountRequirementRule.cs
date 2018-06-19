using System;
using System.Linq;
using Nop.Core;
using Nop.Core.Plugins;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Web.Framework;
using System.Collections.Generic;
using Nop.Core.Domain.Orders;

namespace Nop.Plugin.DiscountRules.ThreeProdsSameCat
{
    public partial class ThreeProdsSameCatDiscountRequirementRule : BasePlugin, IDiscountRequirementRule
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

            if (request.Customer == null)
                return false;

            if (request.ProductVariant == null)
                return false;

            var restrictedCategoryIds = new List<int>();
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
                return false;
            }


            //cart
            var cart = request.Customer.ShoppingCartItems.Where(x => x.ShoppingCartType == ShoppingCartType.ShoppingCart);
            
            bool found = false;
            int counter = 0;
            foreach (var restrictedCatId in restrictedCategoryIds)
            {
                foreach (var sci in cart)
                {
                    foreach (var pc in sci.ProductVariant.Product.ProductCategories)
                        if (restrictedCatId == pc.CategoryId)
                        {
                            counter++;
                            if (counter >= 3)
                            {
                                found = true;
                                break;
                            }                            
                        }

                }

                counter = 0;

                if (found)
                {
                    break;
                }
            }

            if (found)
                return true;

            return false;

            //var quote = request.Customer.CustomerProductVariantQuotes.FirstOrDefault(x => x.ProductVariantId == request.ProductVariant.Id);

            //if(quote==null)
            //    return StatefulStorage.PerSession.Get<bool?>("SkipQuoteDiscountActivationCheck").HasValue;
            //return quote.ActivateDate.HasValue || (StatefulStorage.PerSession.Get<bool?>("SkipQuoteDiscountActivationCheck").HasValue);

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
            string result = "Plugins/DiscountRulesThreeProdsSameCat/Configure/?discountId=" + discountId;
            if (discountRequirementId.HasValue)
                result += string.Format("&discountRequirementId={0}", discountRequirementId.Value);
            return result;
        }

        public override void Install()
        {
            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.ThreeProdsSameCat.Fields.ThreeProdsSameCat.Hint", "Discount will be applied if customer has at least 3 products from the same category.");
            base.Install();
        }

        public override void Uninstall()
        {
            //locales
            this.DeletePluginLocaleResource("Plugins.DiscountRules.ThreeProdsSameCat.Fields.ThreeProdsSameCat.Hint");
            base.Uninstall();
        }
    }
}