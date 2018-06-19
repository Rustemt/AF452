using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Events;
using Nop.Core.Plugins;
using Nop.Services.Customers;
using Nop.Core.Domain.Catalog;
using Nop.Services.Payments;

namespace Nop.Services.Discounts
{
    /// <summary>
    /// Discount service
    /// </summary>
    public partial class DiscountService : IDiscountService
    {
       

        #region Methods

        /// <summary>
        /// Check discount requirements
        /// </summary>
        /// <param name="discount">Discount</param>
        /// <param name="customer">Customer</param>
        /// <returns>true - requirement is met; otherwise, false</returns>
        public virtual bool IsDiscountValid(Discount discount, Customer customer, ProductVariant productVariant)
        {
            if (discount == null)
                throw new ArgumentNullException("discount");

            //check coupon code
            if (discount.RequiresCouponCode)
            {
                if (String.IsNullOrEmpty(discount.CouponCode))
                    return false;

                string couponCodeToValidate = string.Empty;
                if (customer != null)
                    couponCodeToValidate = customer.DiscountCouponCode;
                if (!IsDiscountCouponCodeValid(discount, couponCodeToValidate))
                    return false;
            }

            //check date range
            DateTime now = DateTime.UtcNow;
            if (discount.StartDateUtc.HasValue)
            {
                DateTime startDate = DateTime.SpecifyKind(discount.StartDateUtc.Value, DateTimeKind.Utc);
                if (startDate.CompareTo(now) > 0)
                    return false;
            }
            if (discount.EndDateUtc.HasValue)
            {
                DateTime endDate = DateTime.SpecifyKind(discount.EndDateUtc.Value, DateTimeKind.Utc);
                if (endDate.CompareTo(now) < 0)
                    return false;
            }

            if (!CheckDiscountLimitations(discount, customer))
                return false;

            //discount requirements
            var requirements = discount.DiscountRequirements;
            foreach (var req in requirements)
            {
                var requirementRule = LoadDiscountRequirementRuleBySystemName(req.DiscountRequirementRuleSystemName);
                if (requirementRule == null)
                    continue;
                var request = new CheckDiscountRequirementRequest()
                {
                    DiscountRequirement = req,
                    Customer = customer,
                    ProductVariant = productVariant
                };
                if (!requirementRule.CheckRequirement(request))
                    return false;
            }
            return true;
        }

        public virtual bool IsDiscountValid(Discount discount, Customer customer, ProcessPaymentRequest processPaymentRequest)
        {
            if (discount == null)
                throw new ArgumentNullException("discount");

            //check coupon code
            if (discount.RequiresCouponCode)
            {
                if (String.IsNullOrEmpty(discount.CouponCode))
                    return false;

                string couponCodeToValidate = string.Empty;
                if (customer != null)
                    couponCodeToValidate = customer.DiscountCouponCode;
                if (!IsDiscountCouponCodeValid(discount, couponCodeToValidate))
                    return false;
            }

            //check date range
            DateTime now = DateTime.UtcNow;
            if (discount.StartDateUtc.HasValue)
            {
                DateTime startDate = DateTime.SpecifyKind(discount.StartDateUtc.Value, DateTimeKind.Utc);
                if (startDate.CompareTo(now) > 0)
                    return false;
            }
            if (discount.EndDateUtc.HasValue)
            {
                DateTime endDate = DateTime.SpecifyKind(discount.EndDateUtc.Value, DateTimeKind.Utc);
                if (endDate.CompareTo(now) < 0)
                    return false;
            }

            if (!CheckDiscountLimitations(discount, customer))
                return false;

            //discount requirements
            var requirements = discount.DiscountRequirements;
            foreach (var req in requirements)
            {
                var requirementRule = LoadDiscountRequirementRuleBySystemName(req.DiscountRequirementRuleSystemName);
                if (requirementRule == null)
                    continue;
                var request = new CheckDiscountRequirementRequest()
                {
                    DiscountRequirement = req,
                    Customer = customer,
                    ProcessPaymentRequest = processPaymentRequest
                };
                if (!requirementRule.CheckRequirement(request))
                    return false;
            }
            return true;
 
        }

        public virtual bool IsDiscountValid(Discount discount, Customer customer, string couponCodeToValidate, out string message)
        {
            if (discount == null)
                throw new ArgumentNullException("discount");
            message = "";
            //check coupon code
            if (discount.RequiresCouponCode)
            {
                if (String.IsNullOrEmpty(discount.CouponCode))
                {
                    return false;
                }

                if (!IsDiscountCouponCodeValid(discount, couponCodeToValidate))
                    return false;
            }

            //check date range
            DateTime now = DateTime.UtcNow;
            if (discount.StartDateUtc.HasValue)
            {
                DateTime startDate = DateTime.SpecifyKind(discount.StartDateUtc.Value, DateTimeKind.Utc);
                if (startDate.CompareTo(now) > 0)
                    return false;
            }
            if (discount.EndDateUtc.HasValue)
            {
                DateTime endDate = DateTime.SpecifyKind(discount.EndDateUtc.Value, DateTimeKind.Utc);
                if (endDate.CompareTo(now) < 0)
                    return false;
            }

            if (!CheckDiscountLimitations(discount, customer))
                return false;

            //discount requirements
            var requirements = discount.DiscountRequirements;
            foreach (var req in requirements)
            {
                var requirementRule = LoadDiscountRequirementRuleBySystemName(req.DiscountRequirementRuleSystemName);
                if (requirementRule == null)
                    continue;
                var request = new CheckDiscountRequirementRequest()
                {
                    DiscountRequirement = req,
                    Customer = customer,
                    IsForCoupon = true
                };
                if (!requirementRule.CheckRequirement(request,out message))
                {
                    return false;
                }
            }
            return true;
        }

        public virtual bool IsDiscountCouponCodeValid(Discount discount, string couponCodeToValidate)
        {
            if (couponCodeToValidate == null) return false;
            int index = couponCodeToValidate.IndexOf('-');
            if (index > 0)
                couponCodeToValidate = couponCodeToValidate.Substring(0, index);
            return discount.CouponCode.Equals(couponCodeToValidate, StringComparison.InvariantCultureIgnoreCase);
        }

        #endregion
    }

    public enum DiscountValidationStatus
    {
        Void,
        Valid,
        CouponCodeInvalid,
        DateInvalid,
        NumberOfUseInvalid,
        RequirementsNotMet
    }

}
