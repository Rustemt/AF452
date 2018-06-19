using System;
using AF.Nop.Plugins.XmlUpdate.Domain;
using System.Data.Entity.ModelConfiguration;

namespace AF.Nop.Plugins.XmlUpdate.Data
{
    public class XmlPropertyMap : EntityTypeConfiguration<XmlProperty>
    {
        public const string TABLE_NAME = "AF_XmlProviderProperty";

        public XmlPropertyMap()
        {
            ToTable(TABLE_NAME);
            
            HasKey(m => m.Id);
            Property(m => m.ProviderId).IsRequired();
            Property(m => m.Name).HasMaxLength(60);
            Property(m => m.ProductProperty).HasMaxLength(60).IsRequired();
            Property(m => m.Enabled).IsRequired();

            //HasRequired<XmlProvider>(m => m.Provider)
            //    .WithMany()
            //    .HasForeignKey(ft => ft.ProviderId)
            //    .WillCascadeOnDelete(true)
            //;
        }
    }
}
