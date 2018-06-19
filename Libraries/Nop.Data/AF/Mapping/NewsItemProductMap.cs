using System.Data.Entity.ModelConfiguration;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.News;

namespace Nop.Data.Mapping.News
{
    public partial class NewsItemProductMap : EntityTypeConfiguration<NewsItemProduct>
    {
        public NewsItemProductMap()
        {
            this.ToTable("News_Product_Mapping");
            this.HasKey(pvp => pvp.Id);

            this.HasRequired(np => np.Product)
                .WithMany(p => p.NewsItemProducts)
                .HasForeignKey(np => np.ProductId);


            this.HasRequired(np => np.NewsItem)
                .WithMany(p => p.NewsItemProducts)
                .HasForeignKey(np => np.NewsItemId);
        }
    }
}