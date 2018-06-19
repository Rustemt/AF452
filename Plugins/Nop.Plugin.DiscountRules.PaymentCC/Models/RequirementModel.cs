using Nop.Web.Framework;

namespace Nop.Plugin.DiscountRules.PaymentCC.Models
{
    public class RequirementModel
    {
        [NopResourceDisplayName("Plugins.DiscountRules.PaymentCC.Fields.Bins")]
        public string Bins { get; set; }

        public int DiscountId { get; set; }

        public int RequirementId { get; set; }
    }
}