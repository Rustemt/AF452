using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity.ModelConfiguration;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Payments;

namespace Nop.AF.Mapping
{
    public class BinMap : EntityTypeConfiguration<Bin>
    {
        public BinMap()
        {
            this.ToTable("Bin");
            this.HasKey(cpv => cpv.Id);
            //this.HasRequired(cpv => cpv.Customer)
            //    .WithMany(c => c.CustomerProductVariantQuotes)
            //    .HasForeignKey(cpv => cpv.CustomerId);
        }
    }
}
