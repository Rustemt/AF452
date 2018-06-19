namespace Nop.Core.Domain.Discounts
{
    /// <summary>
    /// Represents a discount requirement
    /// </summary>
    public partial class DiscountRequirement : BaseEntity
    {
        /// <summary>
        /// Gets or sets the discount identifier
        /// </summary>
        public virtual bool QuoteRequired { get; set; }


        /// <summary>
        /// Gets or sets the restricted customer identifiers (comma separated)
        /// </summary>
        public virtual string RestrictedToCustomers { get; set; }

        public virtual string RestrictedCategoryIds { get; set; }

        public virtual string RestrictedManufacturerIds { get; set; }

        public virtual string Bins { get; set; }
        


    }
}
