using System;
using System.Collections.Generic;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.News;

namespace Nop.Core.Domain.Catalog
{
	public partial class ProductSummary : BaseEntity, ILocalizedEntity
	{
		public virtual string SeName { get; set; }
		public virtual int PictureId { get; set; }
		public virtual string Seofilename { get; set; }
		public virtual string ImageSePartUrl { get; set; }
	}
}