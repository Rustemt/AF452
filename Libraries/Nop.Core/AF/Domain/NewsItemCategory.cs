using System;
using Nop.Core.Domain.News;

namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product category mapping
    /// </summary>
    public partial class NewsItemCategory : BaseEntity
    {
        /// <summary>
        /// Gets or sets the product identifier
        /// </summary>
        public virtual int NewsItemId { get; set; }

        /// <summary>
        /// Gets or sets the category identifier
        /// </summary>
        public virtual int CategoryId { get; set; }

        /// <summary>
        /// Gets the category
        /// </summary>
        public virtual Category Category { get; set; }

        /// <summary>
        /// Gets the product
        /// </summary>
        public virtual NewsItem NewsItem { get; set; }
       
        public DateTime CreatedOnUtc { get; set; }
    }
}
