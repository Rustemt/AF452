using System;
using System.Collections.Generic;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Catalog;



namespace Nop.Core.Domain.News
{
    /// <summary>
    /// Represents a news item
    /// </summary>
    public partial class NewsItem : BaseEntity
    {

        private ICollection<NewsItemPicture> _newsItemPictures;
        private ICollection<NewsItemProduct> _newsItemProducts;
        private ICollection<ExtraContent> _extraContents;
        private ICollection<NewsItemManufacturer> _newsItemManufacturers;
        private ICollection<NewsItemCategory> _newsItemCategories;
        private ICollection<NewsItemProductVariant> _newsItemProductVariants;
        
        /// <summary>
        /// Gets or sets the type
        /// </summary>
        public virtual NewsType SystemType
        {
            get
            {
                return (NewsType)this.SystemTypeId;
            }
            set
            {
                this.SystemTypeId = (int)value;
            }
        } 
        public virtual int SystemTypeId { get; set; }
        public virtual bool IsFeatured { get; set; }

        public string Url { get; set; } 


        /// <summary>
        /// Gets or sets the meta keywords
        /// </summary>
        public virtual string MetaKeywords { get; set; }

        /// <summary>
        /// Gets or sets the meta description
        /// </summary>
        public virtual string MetaDescription { get; set; }

        /// <summary>
        /// Gets or sets the meta title
        /// </summary>
        public virtual string MetaTitle { get; set; }

        /// <summary>
        /// Gets or sets the search-engine name
        /// </summary>
        public virtual string SeName { get; set; }

        /// <summary>
        /// Gets or DisplayOrder of entity creation
        /// </summary>
        public virtual int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the collection of NewsPicture
        /// </summary>
        public virtual ICollection<NewsItemPicture> NewsItemPictures
        {
            get { return _newsItemPictures ?? (_newsItemPictures = new List<NewsItemPicture>()); }
            protected set { _newsItemPictures = value; }
        }

        /// <summary>
        /// Gets or sets the collection of NewsProducts
        /// </summary>
        public virtual ICollection<NewsItemProduct> NewsItemProducts
        {
            get { return _newsItemProducts ?? (_newsItemProducts = new List<NewsItemProduct>()); }
            protected set { _newsItemProducts = value; }
        }
        public virtual ICollection<NewsItemProductVariant> NewsItemProductVariants
        {
            get { return _newsItemProductVariants ?? (_newsItemProductVariants = new List<NewsItemProductVariant>()); }
            protected set { _newsItemProductVariants = value; }
        }
        public virtual ICollection<ExtraContent> ExtraContents
        {
            get { return _extraContents ?? (_extraContents = new List<ExtraContent>()); }
            protected set { _extraContents = value; }
        }

        public virtual void RemoveExtraContent(ExtraContent extraContent)
        {
            if (this.ExtraContents.Contains(extraContent))
            {
                this.ExtraContents.Remove(extraContent);
            }
        }
        public virtual void AddExtraContent(ExtraContent extraContent)
        {
            if (!this.ExtraContents.Contains(extraContent))
                this.ExtraContents.Add(extraContent);
        }
        public virtual ICollection<NewsItemManufacturer>NewsItemManufacturers
        {
            get { return _newsItemManufacturers ?? (_newsItemManufacturers = new List<NewsItemManufacturer>()); }
            protected set { _newsItemManufacturers = value; }
        }
        public virtual ICollection<NewsItemCategory> NewsItemCategories
        {
            get { return _newsItemCategories ?? (_newsItemCategories = new List<NewsItemCategory>()); }
            protected set { _newsItemCategories = value; }
        }
    }
 
}   

    [Flags]
    public enum NewsType
    {
        News = 1,
        BannerNews = 2,
        HomeMainContent = 4,
        HomeBottomContent = 8,
        Interview = 16,
        ProductListBanner= 32,
        Catalog =64,
        LookBookSmall =128,
        LookBookMedium =256,
        LookBookLarge =512,
        Shop=1024,
		HomeSlider = 2048,
		HomeManufacturers = 4096,
		HomeContentBanner = 8192,
		HomeContentManufacturerBanner = 16384
    }