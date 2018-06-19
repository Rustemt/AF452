using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity.ModelConfiguration;
using Nop.Core.Domain.News;

namespace Nop.Data.AF.Mapping
{
    namespace Nop.Data.AF.Mapping
    {
        public partial class ExtraContentMap : EntityTypeConfiguration<ExtraContent>
        {
            public ExtraContentMap()
            {
                this.ToTable("ExtraContent");
                this.HasKey(a => a.Id);
            }
        }
    }
}
