using System.Data.Entity.ModelConfiguration;
using Nop.Core.Domain.Catalog;

namespace Nop.Data.Mapping.Catalog
{
    public partial class NewsItemCategoryMap : EntityTypeConfiguration<NewsItemCategory>
    {
        public NewsItemCategoryMap()
        {
            this.ToTable("News_Category_Mapping");
            this.HasKey(pc => pc.Id);
            
            this.HasRequired(pc => pc.Category)
                .WithMany(c => c.NewsItemCategories)
                .HasForeignKey(pc => pc.CategoryId);


            this.HasRequired(pc => pc.NewsItem)
                .WithMany(p => p.NewsItemCategories)
                .HasForeignKey(pc => pc.NewsItemId);
        }
    }
}
