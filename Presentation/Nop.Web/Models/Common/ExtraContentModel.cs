using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.News;
using System.Web.Mvc;
using Nop.Web.Framework;

namespace Nop.Web.Models.Common
{
    public class ExtraContentModel : BaseEntity
    {
        [NopResourceDisplayName("ExtraContent.Fields.Title")]
        [AllowHtml]
        public string Title { get; set; }
        [NopResourceDisplayName("ExtraContent.Fields.FullDescription")]
        [AllowHtml]
        public string FullDescription { get; set; }
        [NopResourceDisplayName("ExtraContent.Fields.DisplayOrder")]
        [AllowHtml]
        public int DisplayOrder { get; set; }
    }
}