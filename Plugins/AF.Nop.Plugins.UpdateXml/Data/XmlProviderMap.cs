using System;
using AF.Nop.Plugins.XmlUpdate.Domain;
using System.Data.Entity.ModelConfiguration;

namespace AF.Nop.Plugins.XmlUpdate.Data
{
    public class XmlProviderMap : EntityTypeConfiguration<XmlProvider>
    {
        public const string TABLE_NAME = "AF_XmlProvider";

        public XmlProviderMap()
        {
            ToTable(TABLE_NAME);
            
            HasKey(m => m.Id);
            Property(m => m.Name).HasMaxLength(60).IsRequired();
            Property(m => m.Url).HasMaxLength(400).IsRequired();
            Property(m => m.AuthType).IsRequired();
            Property(m => m.Username);
            Property(m => m.Password);
            Property(m => m.XmlRootNode).IsRequired();
            Property(m => m.XmlItemNode).IsRequired();
            Property(m => m.Enabled).IsRequired();

            this.HasMany<XmlProperty>(c => c.Properties)
                .WithRequired(t => t.Provider)
                .HasForeignKey(t => t.ProviderId)
                .WillCascadeOnDelete(true)
                //.Map(m => m.ToTable(XmlProviderPropertyMap.TABLE_NAME))
            ;
        }
    }
}
