using System.Data.Entity.ModelConfiguration;
using Nop.Core.Domain.Catalog;

namespace Nop.Data.Mapping.Catalog
{
    public partial class ProductVariantPictureMap : EntityTypeConfiguration<ProductVariantPicture>
    {
        public ProductVariantPictureMap()
        {
            this.ToTable("ProductVariant_Picture_Mapping");
            this.HasKey(pvp => pvp.Id);
            
            this.HasRequired(pvp => pvp.Picture)
                .WithMany(p => p.ProductVariantPictures)
                .HasForeignKey(pvp => pvp.PictureId);

            this.HasRequired(pvp => pvp.ProductVariant)
                .WithMany(p => p.ProductVariantPictures)
                .HasForeignKey(pvp => pvp.ProductVariantId);
        }
    }
}