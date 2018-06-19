using Nop.Web.Framework;

namespace Nop.Plugin.DiscountRules.HasManufacturer.Models
{
    public class RequirementModel
    {
        [NopResourceDisplayName("Plugins.DiscountRules.HasManufacturer.Fields.Manufacturers")]
        public string RelatedItems { get; set; }

        public int DiscountId { get; set; }

        public int RequirementId { get; set; }
    }
}