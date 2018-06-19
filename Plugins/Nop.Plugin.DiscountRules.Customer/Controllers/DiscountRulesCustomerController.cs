using System;
using System.Linq;
using System.Web.Mvc;
using Nop.Core.Domain.Discounts;
using Nop.Plugin.DiscountRules.Customer.Models;
using Nop.Services.Discounts;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.DiscountRules.Customer.Controllers
{
    [AdminAuthorize]
    public class DiscountRulesCustomerController : Controller
    {
        private readonly IDiscountService _discountService;

        public DiscountRulesCustomerController(IDiscountService discountService)
        {
            this._discountService = discountService;
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
            model.CustomerIds = discountRequirement != null ? discountRequirement.RestrictedToCustomers : string.Empty;

            //add a prefix
            ViewData.TemplateInfo.HtmlFieldPrefix = string.Format("DiscountRulesCustomer{0}", discountRequirementId.HasValue ? discountRequirementId.Value.ToString() : "0");

            return View("Nop.Plugin.DiscountRules.Customer.Views.DiscountRulesCustomer.Configure", model);
        }

        [HttpPost]
        public ActionResult Configure(int discountId, int? discountRequirementId, string customerIds)
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
                discountRequirement.RestrictedToCustomers = customerIds;
                _discountService.UpdateDiscount(discount);
            }
            else
            {
                //save new rule
                discountRequirement = new DiscountRequirement()
                {
                    DiscountRequirementRuleSystemName = "DiscountRequirement.Customer",
                    RestrictedToCustomers = customerIds,
                };
                discount.DiscountRequirements.Add(discountRequirement);
                _discountService.UpdateDiscount(discount);
            }
            return Json(new { Result = true, NewRequirementId = discountRequirement.Id }, JsonRequestBehavior.AllowGet);
        }
        
    }
}