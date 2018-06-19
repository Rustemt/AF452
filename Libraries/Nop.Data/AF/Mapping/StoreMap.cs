using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity.ModelConfiguration;
using Nop.Core.Domain.Common;

namespace Nop.Data.AF.Mapping
{
    public class StoreMap : EntityTypeConfiguration<Store>
    {
        public StoreMap()
        {
            this.ToTable("Store");
            this.HasKey(cpv => cpv.Id);
        }
    }
}
