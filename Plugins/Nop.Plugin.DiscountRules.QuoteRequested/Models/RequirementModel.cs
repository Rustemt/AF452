using System.Collections.Generic;
using System.Web.Mvc;
using Nop.Web.Framework;

namespace Nop.Plugin.DiscountRules.QuoteRequested.Models
{
    public class RequirementModel
    {
        [NopResourceDisplayName("Plugins.DiscountRules.QuoteRequested.Fields.QuoteRequested")]
        public bool QuoteRequested { get; set; }

        public int DiscountId { get; set; }

        public int RequirementId { get; set; }

    }
}