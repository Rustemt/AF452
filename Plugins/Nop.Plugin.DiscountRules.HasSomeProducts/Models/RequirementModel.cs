using Nop.Web.Framework;

namespace Nop.Plugin.DiscountRules.HasSomeProducts.Models
{
    public class RequirementModel
    {
        [NopResourceDisplayName("Plugins.DiscountRules.HasSomeProducts.Fields.ProductVariants")]
        public string ProductVariants { get; set; }

        public int DiscountId { get; set; }

        public int RequirementId { get; set; }
    }
}