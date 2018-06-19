using System.Data.Entity.ModelConfiguration;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.News;

namespace Nop.Data.Mapping.News
{
    public partial class NewsItemProductVariantMap : EntityTypeConfiguration<NewsItemProductVariant>
    {
        public NewsItemProductVariantMap()
        {
            this.ToTable("News_ProductVariant_Mapping");
            this.HasKey(pvp => pvp.Id);

            this.HasRequired(np => np.ProductVariant)
                .WithMany(pv => pv.NewsItemProductVariants)
                .HasForeignKey(np => np.ProductVariantId);

            this.HasRequired(np => np.NewsItem)
                .WithMany(pv => pv.NewsItemProductVariants)
                .HasForeignKey(np => np.NewsItemId);
        }
    }
}