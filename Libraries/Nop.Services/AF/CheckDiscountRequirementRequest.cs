using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Catalog;
using Nop.Services.Payments;

namespace Nop.Services.Discounts
{
    /// <summary>
    /// Represents a discount requirement request
    /// </summary>
    public partial class CheckDiscountRequirementRequest
    {
        /// <summary>
        /// Gets or sets the ProductVariant
        /// </summary>
        public ProductVariant ProductVariant { get; set; }

        public ProcessPaymentRequest ProcessPaymentRequest { get; set; }

        public bool IsForCoupon { get; set; }

    }
}

