using System.Data.Entity.ModelConfiguration;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;

namespace Nop.Data.Mapping.Catalog
{
    public partial class CustomerProductVariantQuoteMap : EntityTypeConfiguration<CustomerProductVariantQuote>
    {
        public CustomerProductVariantQuoteMap()
        {
            this.ToTable("Customer_ProductVariantQuote_Mapping");
            this.HasKey(cpv => cpv.Id);


            this.HasRequired(cpv => cpv.Customer)
                .WithMany(c => c.CustomerProductVariantQuotes)
                .HasForeignKey(cpv => cpv.CustomerId);

            this.HasRequired(cpv => cpv.ProductVariant)
                .WithMany(c => c.CustomerProductVariantQuotes)
                .HasForeignKey(cpv => cpv.ProductVariantId);
        }
    }
}