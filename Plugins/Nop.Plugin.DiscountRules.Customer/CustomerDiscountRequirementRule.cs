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

namespace Nop.Plugin.DiscountRules.Customer
{
    public partial class CustomerDiscountRequirementRule : BasePlugin, IDiscountRequirementRule
    {
         IPriceCalculationService _priceCalculationService;
         public CustomerDiscountRequirementRule(IPriceCalculationService priceCalculationService)
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

            if (request.Customer == null || request.Customer.IsGuest())
                return false;

            var restrictedCustomerIds = new List<int>();
            try
            {
                restrictedCustomerIds = request.DiscountRequirement.RestrictedToCustomers
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToList();
            }
            catch
            {
                //error parsing
                return false;
            }
            if (restrictedCustomerIds.Count == 0)
                return false;

            return restrictedCustomerIds.Contains(request.Customer.Id);

       
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
            string result = "Plugins/DiscountRulesCustomer/Configure/?discountId=" + discountId;
            if (discountRequirementId.HasValue)
                result += string.Format("&discountRequirementId={0}", discountRequirementId.Value);
            return result;
        }

        public override void Install()
        {
            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.Customer.Fields.Customer", "Customer Id(s)");
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.Customer.Fields.Customer.Hint", "Customer id(s) comma seperated");
            base.Install();
        }

        public override void Uninstall()
        {
            //locales
            this.DeletePluginLocaleResource("Plugins.DiscountRules.Customer.Fields.Customer");
            this.DeletePluginLocaleResource("Plugins.DiscountRules.Customer.Fields.Customer.Hint");
            base.Uninstall();
        }
    }
}