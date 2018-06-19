using Nop.Web.Framework;

namespace Nop.Plugin.DiscountRules.Customer.Models
{
    public class RequirementModel
    {
        [NopResourceDisplayName("Plugins.DiscountRules.Customer.Fields.Id")]
        public string CustomerIds { get; set; }

        public int DiscountId { get; set; }

        public int RequirementId { get; set; }
    }
}