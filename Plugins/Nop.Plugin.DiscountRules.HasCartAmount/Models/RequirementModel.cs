using Nop.Web.Framework;

namespace Nop.Plugin.DiscountRules.HasCartAmount.Models
{
    public class RequirementModel
    {
        [NopResourceDisplayName("Plugins.DiscountRules.HasCartAmount.Fields.Amount")]
        public decimal SpentAmount { get; set; }

        public int DiscountId { get; set; }

        public int RequirementId { get; set; }

        [NopResourceDisplayName("Plugins.DiscountRules.HasCartAmount.Fields.Manufacturers")]
        public string RestrictedManufacturerIds { get; set; }

        [NopResourceDisplayName("Plugins.DiscountRules.HasCartAmount.Fields.Categories")]
        public string RestrictedCategoryIds { get; set; }
    }
}