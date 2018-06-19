using System;
using System.Linq;
using System.Web.Mvc;
using Nop.Core.Domain.Discounts;
using Nop.Plugin.DiscountRules.QuoteRequested.Models;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.DiscountRules.QuoteRequested.Controllers
{
    [AdminAuthorize]
    public class DiscountRulesQuoteRequestedController : Controller
    {
        private readonly IDiscountService _discountService;
        private readonly ICustomerService _customerService;

        public DiscountRulesQuoteRequestedController(IDiscountService discountService,
            ICustomerService customerService)
        {
            this._discountService = discountService;
            this._customerService = customerService;
        }

         [HttpGet]
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

            //add a prefix
            ViewData.TemplateInfo.HtmlFieldPrefix = string.Format("DiscountRulesQuoteRequested{0}", discountRequirementId.HasValue ? discountRequirementId.Value.ToString() : "0");

            return View("Nop.Plugin.DiscountRules.QuoteRequested.Views.DiscountRulesQuoteRequested.Configure", model);

        }

        [HttpPost]
        [ActionName("Configure")]
        public ActionResult ConfigurePost(int discountId, int? discountRequirementId)
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
                _discountService.UpdateDiscount(discount);
            }
            else
            {
                //save new rule
                discountRequirement = new DiscountRequirement()
                {
                    DiscountRequirementRuleSystemName = "DiscountRequirement.QuoteRequested",
                    QuoteRequired = true
                };
                discount.DiscountRequirements.Add(discountRequirement);
                _discountService.UpdateDiscount(discount);
            }
            return Json(new { Result = true, NewRequirementId = discountRequirement.Id }, JsonRequestBehavior.AllowGet);
        }
        
    }
}