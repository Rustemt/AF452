using System.Data.Entity.ModelConfiguration;
using Nop.Core.Domain.Catalog;

namespace Nop.Data.Mapping.Catalog
{
    public partial class NewsItemManufacturerMap : EntityTypeConfiguration<NewsItemManufacturer>
    {
        public NewsItemManufacturerMap()
        {
            this.ToTable("News_Manufacturer_Mapping");
            this.HasKey(pm => pm.Id);
            
            this.HasRequired(pm => pm.Manufacturer)
                .WithMany(m => m.NewsItemManufacturers)
                .HasForeignKey(pm => pm.ManufacturerId);


            this.HasRequired(pm => pm.NewsItem)
                .WithMany(p => p.NewsItemManufacturers)
                .HasForeignKey(pm => pm.NewsItemId);
        }
    }
}