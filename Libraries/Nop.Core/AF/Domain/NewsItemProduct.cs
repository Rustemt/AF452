using Nop.Core.Domain.News;
using Nop.Core.Domain.Catalog;
namespace Nop.Core.Domain.News
{
    /// <summary>
    /// Represents a product manufacturer mapping
    /// </summary>
    public partial class NewsItemProduct : BaseEntity
    {
        /// <summary>
        /// Gets or sets the product identifier
        /// </summary>
        public virtual int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the manufacturer identifier
        /// </summary>
        public virtual int NewsItemId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the product is featured
        /// </summary>
        public virtual bool IsFeaturedProduct { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public virtual int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the manufacturer
        /// </summary>
        public virtual NewsItem NewsItem { get; set; }

        /// <summary>
        /// Gets or sets the product
        /// </summary>
        public virtual Product Product { get; set; }
    }

}
