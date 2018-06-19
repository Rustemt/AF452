using System.Data.Entity.ModelConfiguration;
using Nop.Core.Domain.Messages;

//AF
namespace Nop.Data.Mapping.Messages
{
    public partial class NewsLetterSubscriptionMap : EntityTypeConfiguration<NewsLetterSubscription>
    {
        public NewsLetterSubscriptionMap()
        {
            this.ToTable("NewsLetterSubscription");
            this.HasKey(nls => nls.Id);

            this.Property(nls => nls.Email).IsRequired().HasMaxLength(255);
            this.Property(nls => nls.FirstName).HasMaxLength(255);
            this.Property(nls => nls.LastName).HasMaxLength(255);
            this.Property(nls => nls.Gender).HasMaxLength(1);
            this.HasRequired(nls => nls.Language)
                .WithMany()
                .HasForeignKey(nls => nls.LanguageId);
        }
    }
}