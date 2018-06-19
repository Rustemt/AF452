using System.Data.Entity.ModelConfiguration;
using Nop.Core.Domain.News;

namespace Nop.Data.Mapping.News
{
    public partial class NewsItemMap : EntityTypeConfiguration<NewsItem>
    {
        public NewsItemMap()
        {
            this.ToTable("News");
            this.HasKey(bp => bp.Id);
            this.Property(bp => bp.Title).IsRequired().IsMaxLength();
            this.Property(bp => bp.Short).IsRequired().IsMaxLength();
            this.Property(bp => bp.Full).IsRequired().IsMaxLength();
            this.Property(bp => bp.MetaKeywords).HasMaxLength(400);
            this.Property(bp => bp.MetaDescription);
            this.Property(bp => bp.MetaTitle).HasMaxLength(400);
            this.Property(bp => bp.SeName).HasMaxLength(200);
            
            this.Ignore(bp => bp.SystemType);
            
            this.HasRequired(bp => bp.Language)
                .WithMany()
                .HasForeignKey(bp => bp.LanguageId).WillCascadeOnDelete(true);

            this.HasMany<ExtraContent>(c => c.ExtraContents)
               .WithMany()
               .Map(m => m.ToTable("News_ExtraContent_Mapping"));
            
        }
    }
}