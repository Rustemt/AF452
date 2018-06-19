using System.Collections.Generic;
using System.Web.Mvc;
using Nop.Web.Framework;

namespace Nop.Plugin.DiscountRules.ThreeProdsSameCat.Models
{
    public class RequirementModel
    {
        [NopResourceDisplayName("Plugins.DiscountRules.ThreeProdsSameCat.Fields.ThreeProdsSameCat")]
        public bool ThreeProdsSameCat { get; set; }

        public int DiscountId { get; set; }

        public int RequirementId { get; set; }

    }
}