using System;
using System.Linq;
using System.Web.Mvc;
using Nop.Core.Domain.Discounts;
using Nop.Plugin.DiscountRules.HasCartAmount.Models;
using Nop.Services.Discounts;
using Nop.Web.Framework.Controllers;
using System.Globalization;
using System.Threading;

namespace Nop.Plugin.DiscountRules.HasCartAmount.Controllers
{
    [AdminAuthorize]
    public class DiscountRulesHasCartAmountController : Controller
    {
        private readonly IDiscountService _discountService;

        public DiscountRulesHasCartAmountController(IDiscountService discountService)
        {
            this._discountService = discountService;
            var culture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }

        public ActionResult Configure(int discountId, int? discountRequirementId)
        {
            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new ArgumentException("Discount could not be loaded");

            DiscountRequirement discountRequirement = null;
            if (discountRequirementId.HasValue)
            {
                discountRequirement = discount.DiscountRequirements.Where(dr => dr.Id == discountRequirementId.Value).FirstOrDefault();
                if (discountRequirement == null)
                    return Content("Failed to load requirement.");
            }

            var model = new RequirementModel();
            model.RequirementId = discountRequirementId.HasValue ? discountRequirementId.Value : 0;
            model.DiscountId = discountId;
            model.SpentAmount = discountRequirement != null ? discountRequirement.SpentAmount : decimal.Zero;
            model.RestrictedManufacturerIds = discountRequirement != null
                                                  ? discountRequirement.RestrictedManufacturerIds
                                                  : "";
            model.RestrictedCategoryIds = discountRequirement != null
                                                ? discountRequirement.RestrictedCategoryIds
                                                : "";

            //add a prefix
            ViewData.TemplateInfo.HtmlFieldPrefix = string.Format("DiscountRulesHasCartAmount{0}", discountRequirementId.HasValue ? discountRequirementId.Value.ToString() : "0");

            return View("Nop.Plugin.DiscountRules.HasCartAmount.Views.DiscountRulesHasCartAmount.Configure", model);
        }

        [HttpPost]
        public ActionResult Configure(int discountId, int? discountRequirementId, decimal spentAmount, string restrictedManufacturerIds, string restrictedCategoryIds)
        {
            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new ArgumentException("Discount could not be loaded");

            DiscountRequirement discountRequirement = null;
            if (discountRequirementId.HasValue)
                discountRequirement = discount.DiscountRequirements.Where(dr => dr.Id == discountRequirementId.Value).FirstOrDefault();

            if (discountRequirement != null)
            {
                //update existing rule
                discountRequirement.SpentAmount = spentAmount;
                discountRequirement.RestrictedManufacturerIds = string.IsNullOrWhiteSpace(restrictedManufacturerIds) ? null : restrictedManufacturerIds;
                discountRequirement.RestrictedCategoryIds = string.IsNullOrWhiteSpace(restrictedCategoryIds) ? null : restrictedCategoryIds;
                _discountService.UpdateDiscount(discount);
            }
            else
            {
                //save new rule
                discountRequirement = new DiscountRequirement()
                {
                    DiscountRequirementRuleSystemName = "DiscountRequirement.HasCartAmount",
                    SpentAmount = spentAmount,
                    RestrictedManufacturerIds = string.IsNullOrWhiteSpace(restrictedManufacturerIds) ? null : restrictedManufacturerIds,
                    RestrictedCategoryIds = string.IsNullOrWhiteSpace(restrictedCategoryIds) ? null : restrictedCategoryIds
                };
                discount.DiscountRequirements.Add(discountRequirement);
                _discountService.UpdateDiscount(discount);
            }
            return Json(new { Result = true, NewRequirementId = discountRequirement.Id }, JsonRequestBehavior.AllowGet);
        }
        
    }
}