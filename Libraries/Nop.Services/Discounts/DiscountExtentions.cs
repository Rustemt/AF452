using System;
using System.Collections.Generic;
using Nop.Core.Domain.Discounts;
using System.Linq;

namespace Nop.Services.Discounts
{
    public static class DiscountExtentions
    {
        public static Discount GetPreferredDiscount(this IList<Discount> discounts,
            decimal amount)
        {
            Discount preferredDiscount = null;
            decimal maximumDiscountValue = decimal.Zero;
            foreach (var discount in discounts)
            {
                decimal currentDiscountValue = discount.GetDiscountAmount(amount);
                if (currentDiscountValue > maximumDiscountValue)
                {
                    maximumDiscountValue = currentDiscountValue;
                    preferredDiscount = discount;
                }
            }

            return preferredDiscount;
        }


        //AF
        //Include cummulative ones
        public static List<Discount> GetPreferredDiscounts(this IList<Discount> discounts,
           decimal amount)
        {
            List<Discount> preferredDiscounts = new List<Discount>();
            Discount preferredDiscount = null;
            decimal maximumDiscountValue = decimal.Zero;

            if (discounts.Where(d => d.DiscountRequirements.Where(dr => dr.QuoteRequired).Count() > 0).Count() > 0)
            {
                preferredDiscounts.Add(GetPreferredDiscount(discounts, amount));
                return preferredDiscounts;
            }



            foreach (var discount in discounts)
            {
                if (discount.IsCumulative.HasValue && discount.IsCumulative.Value)
                {
                    preferredDiscounts.Add(discount);
                    continue;
                }
                decimal currentDiscountValue = discount.GetDiscountAmount(amount);
                if (currentDiscountValue > maximumDiscountValue)
                {
                    maximumDiscountValue = currentDiscountValue;
                    preferredDiscount = discount;
                }
            }
            if (preferredDiscount != null)
                preferredDiscounts.Add(preferredDiscount);
            return preferredDiscounts;
        }


        //AF
        //First percentage discount rates are applied then amount discounts are applied
        public static decimal GetDiscountAmount(this IList<Discount> discounts,
           decimal amount)
        {
            if (discounts == null) return 0;
            decimal result = decimal.Zero;
            decimal tempAmount = amount;
            decimal totalAmountDiscount = 0;
            foreach (var discount in discounts)
            {
                if (discount.UsePercentage)
                {
                    tempAmount = tempAmount - discount.GetDiscountAmount(tempAmount);
                }
                else
                {
                    totalAmountDiscount = discount.DiscountAmount;
                }
            }
            result = amount - tempAmount + totalAmountDiscount;

            if (result < decimal.Zero)
                result = decimal.Zero;

            return result;
        }

        public static bool ContainsDiscount(this IList<Discount> discounts,
            Discount discount)
        {
            if (discounts == null)
                throw new ArgumentNullException("discounts");

            if (discount == null)
                throw new ArgumentNullException("discount");

            foreach (var dis1 in discounts)
                if (discount.Id == dis1.Id)
                    return true;

            return false;
        }
    }
}
