using System.Data.Entity.ModelConfiguration;
using Nop.Core.Domain.Catalog;

namespace Nop.Data.Mapping.Catalog
{
    public partial class ProductAttributeOptionMap : EntityTypeConfiguration<ProductAttributeOption>
    {
        public ProductAttributeOptionMap()
        {
            this.ToTable("ProductAttributeOption");
            this.HasKey(sao => sao.Id);
            this.Property(sao => sao.Name).IsRequired();
            
            this.HasRequired(sao => sao.ProductAttribute)
                .WithMany(sa => sa.ProductAttributeOptions)
                .HasForeignKey(sao => sao.ProductAttributeId);
        }
    }
}