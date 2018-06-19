using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using System.Web.Mvc;

namespace Nop.Admin.Models.News
{
    public class ExtraContentModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.ExtraContent.Fields.FullDescription")]
        [AllowHtml]
        public virtual string FullDescription { get; set; }

        [NopResourceDisplayName("Admin.ExtraContent.Fields.Title")]
        [AllowHtml]
        public virtual string Title { get; set; }

        [NopResourceDisplayName("Admin.ExtraContent.Fields.DisplayOrder")]
        [AllowHtml]
        public virtual int DisplayOrder { get; set; }
    }
}