using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nop.Core.Domain.Localization;


namespace Nop.Core.Domain.News
{
    public partial class NewsItemExtraContent : BaseEntity
    {
        public virtual int ExtraContentId { get; set; }
        
        public virtual int NewsItemId { get; set; }
       
        public virtual int DisplayOrder { get; set; }

        public virtual NewsItem NewsItem { get; set; }

        public virtual ExtraContent ExtraContent { get; set; }
    }
}