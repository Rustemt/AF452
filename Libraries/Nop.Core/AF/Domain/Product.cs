using System;
using System.Collections.Generic;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.News;

namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product
    /// </summary>
    public partial class Product : BaseEntity, ILocalizedEntity
    {

        private ICollection<NewsItemProduct> _newsItemProducts;

        /// <summary>
        /// Gets or sets the product specification attribute
        /// </summary>
        public virtual ICollection<NewsItemProduct> NewsItemProducts
        {
            get { return _newsItemProducts ?? (_newsItemProducts = new List<NewsItemProduct>()); }
            protected set { _newsItemProducts = value; }
        }

		public virtual int? DisplayOrderOnHomePage { get; set; }
    }
}