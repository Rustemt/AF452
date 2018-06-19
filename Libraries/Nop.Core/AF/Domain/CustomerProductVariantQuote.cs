using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using System;
namespace Nop.Core.Domain.Discounts
{
    /// <summary>
    /// Represents a discount requirement
    /// </summary>
    public partial class CustomerProductVariantQuote : BaseEntity
    {
        /// <summary>
        /// Gets or sets the discount identifier
        /// </summary>
        public virtual int CustomerId { get; set; }

        public virtual string Email { get; set; }

        public virtual int ProductVariantId { get; set; }

        public virtual string PhoneNumber { get; set; }

        public virtual string Name { get; set; }

        public virtual string Enquiry { get; set; }

        public virtual string Description { get; set; }

        public virtual ProductVariant ProductVariant { get; set; }

        public virtual Customer Customer { get; set; }

        public virtual DateTime RequestDate { get; set; }

        public virtual DateTime? ActivateDate { get; set; }

        public virtual string DiscountPercentage { get; set; }

        public virtual string PriceWithDiscount { get; set; }

        public virtual string PriceWithoutDiscount  { get; set; }

        public CustomerProductVariantQuote()
        {
            RequestDate = DateTime.Now;
            //ActivateDate = RequestDate;
        }


    }
}
