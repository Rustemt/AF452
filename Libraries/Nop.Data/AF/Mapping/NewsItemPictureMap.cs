using System.Data.Entity.ModelConfiguration;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.News;

namespace Nop.Data.Mapping.News
{
    public partial class NewsItemPictureMap : EntityTypeConfiguration<NewsItemPicture>
    {
        public NewsItemPictureMap()
        {
            this.ToTable("News_Picture_Mapping");
            this.HasKey(pvp => pvp.Id);
            
            this.HasRequired(pvp => pvp.Picture)
                .WithMany(p => p.NewsItemPictures)
                .HasForeignKey(pvp => pvp.PictureId);

            this.HasRequired(pvp => pvp.NewsItem)
                .WithMany(p => p.NewsItemPictures)
                .HasForeignKey(pvp => pvp.NewsItemId);

            this.Ignore(p => p.NewsItemPictureType);
        }
    }
}