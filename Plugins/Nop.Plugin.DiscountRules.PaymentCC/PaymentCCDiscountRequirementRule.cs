using System;
using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Plugins;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using System.Collections.Generic;
using System.Web;
using Nop.Services.Payments;
using System.Text.RegularExpressions;

namespace Nop.Plugin.DiscountRules.PaymentCC
{
    public partial class PaymentCCDiscountRequirementRule : BasePlugin, IDiscountRequirementRule
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

            if (request.Customer == null || request.Customer.IsGuest())
                return false;
            
            if (request.IsForCoupon)
                return true;

            request.ProcessPaymentRequest = HttpContext.Current.Session["OrderPaymentInfo"] as ProcessPaymentRequest;

            if (request.ProcessPaymentRequest == null)
                return false;

            if (string.IsNullOrWhiteSpace(request.ProcessPaymentRequest.CreditCardNumber))
                return false;

            if (!Regex.IsMatch(request.ProcessPaymentRequest.CreditCardNumber, @"^[0-9]{15,16}$"))
                return false;


            var bins = new List<int>();
            try
            {
                bins = request.DiscountRequirement.Bins
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToList();
            }
            catch
            {
                //error parsing
                return false;
            }
            if (bins.Count == 0)
                return false;

            foreach (var bin in bins)
            {
                if (request.ProcessPaymentRequest.CreditCardNumber.StartsWith(bin.ToString()))
                    return true;
            }
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
            string result = "Plugins/DiscountRulesPaymentCC/Configure/?discountId=" + discountId;
            if (discountRequirementId.HasValue)
                result += string.Format("&discountRequirementId={0}", discountRequirementId.Value);
            return result;
        }

        public override void Install()
        {
            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.PaymentCC.Fields.Bins", "Required bin numbers");
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.PaymentCC.Fields.Amount.Hint", "Discount will be applied if customer has paid via CC-bin.");
            base.Install();
        }

        public override void Uninstall()
        {
            //locales
            this.DeletePluginLocaleResource("Plugins.DiscountRules.PaymentCC.Fields.Bins");
            this.DeletePluginLocaleResource("Plugins.DiscountRules.PaymentCC.Fields.Bins.Hint");
            base.Uninstall();
        }
    }
}