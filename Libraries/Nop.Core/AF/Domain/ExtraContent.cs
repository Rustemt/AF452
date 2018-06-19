using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.News;

namespace Nop.Core.Domain.News
{
    public partial class ExtraContent : BaseEntity
    {
        public virtual string FullDescription { get; set; }

        public virtual string Title { get; set; }

        public virtual int DisplayOrder { get; set; }
    }
}
