
using System;
using Nop.Core.Domain.News;


namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product manufacturer mapping
    /// </summary>
    public partial class NewsItemManufacturer : BaseEntity
    {
        /// <summary>
        /// Gets or sets the newsitem identifier
        /// </summary>
        public virtual int NewsItemId { get; set; }

        /// <summary>
        /// Gets or sets the manufacturer identifier
        /// </summary>
        public virtual int ManufacturerId { get; set; }


        /// <summary>
        /// Gets or sets the manufacturer
        /// </summary>
        public virtual Manufacturer Manufacturer { get; set; }

        /// <summary>
        /// Gets or sets the newsitem
        /// </summary>
        public virtual NewsItem NewsItem { get; set; }

        public DateTime CreatedOnUtc { get; set; }
    }

}
