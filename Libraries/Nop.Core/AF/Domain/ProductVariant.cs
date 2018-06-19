using System;
using System.Collections.Generic;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.News;

namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product variant
    /// </summary>
    public partial class ProductVariant : BaseEntity, ILocalizedEntity
    {
        private ICollection<ProductVariantPicture> _productVariantPictures;
        private ICollection<CustomerProductVariantQuote> _customerProductVariantQuotes;

        /// <summary>
        /// Gets or sets the collection of ProductPicture
        /// </summary>
        public virtual ICollection<ProductVariantPicture> ProductVariantPictures
        {
            get { return _productVariantPictures ?? (_productVariantPictures = new List<ProductVariantPicture>()); }
            protected set { _productVariantPictures = value; }
        }

        public virtual ICollection<CustomerProductVariantQuote> CustomerProductVariantQuotes
        {
            get { return _customerProductVariantQuotes ?? (_customerProductVariantQuotes = new List<CustomerProductVariantQuote>()); }
            protected set { _customerProductVariantQuotes = value; }
        }

        public virtual int? CurrencyId
        { get; set; }
        public virtual Currency Currency { get; set; }

        public virtual decimal? CurrencyPrice { get; set; }

        /// <summary>
        /// Gets or sets the old price
        /// </summary>
        public virtual decimal? CurrencyOldPrice { get; set; }

        /// <summary>
        /// Gets or sets the product cost
        /// </summary>
        public virtual decimal? CurrencyProductCost { get; set; }

        public bool DisplayPriceIfCallforPrice { get; set; }

        public bool HideDiscount { get; set; }

        public int MessageTemplateId { get; set; }

        public virtual MessageTemplate MessageTemplate { get; set; }

        private ICollection<NewsItemProductVariant> _newsItemProductVariants;

        /// <summary>
        /// Gets or sets the product specification attribute
        /// </summary>
        public virtual ICollection<NewsItemProductVariant> NewsItemProductVariants
        {
            get { return _newsItemProductVariants ?? (_newsItemProductVariants = new List<NewsItemProductVariant>()); }
            protected set { _newsItemProductVariants = value; }
        }

    }
}
