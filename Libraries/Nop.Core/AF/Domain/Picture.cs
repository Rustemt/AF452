using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.News;

namespace Nop.Core.Domain.Media
{
    /// <summary>
    /// Represents a picture
    /// </summary>
    public partial class Picture : BaseEntity
    {

        private ICollection<ProductVariantPicture> _productVarientPictures;    
        private ICollection<NewsItemPicture> _newsItemPictures;
        /// <summary>
        /// Gets or sets the product variant pictures
        /// </summary>
        public virtual ICollection<ProductVariantPicture> ProductVariantPictures
        {
            get { return _productVarientPictures ?? (_productVarientPictures = new List<ProductVariantPicture>()); }
            set { _productVarientPictures = value; }
        }

    
        /// <summary>
        /// Gets or sets the product variant pictures
        /// </summary>
        public virtual ICollection<NewsItemPicture> NewsItemPictures
        {
            get { return _newsItemPictures ?? (_newsItemPictures = new List<NewsItemPicture>()); }
            set { _newsItemPictures = value; }
        }
    }
}
