using Nop.Core.Domain.Media;
using Nop.Core.Domain.News;

namespace Nop.Core.Domain.News
{
    /// <summary>
    /// Represents a product picture mapping
    /// </summary>
    public partial class NewsItemPicture : BaseEntity
    {
        /// <summary>
        /// Gets or sets the product identifier
        /// </summary>
        public virtual int NewsItemId { get; set; }

        /// <summary>
        /// Gets or sets the picture identifier
        /// </summary>
        public virtual int PictureId { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public virtual int DisplayOrder { get; set; }
        
        /// <summary>
        /// Gets the picture
        /// </summary>
        public virtual Picture Picture { get; set; }

        /// <summary>
        /// Gets the product variant
        /// </summary>
        public virtual NewsItem NewsItem { get; set; }

        public virtual int NewsItemPictureTypeId { get; set; }

        public virtual NewsItemPictureType NewsItemPictureType
        {
            get
            {
                return (NewsItemPictureType)this.NewsItemPictureTypeId;
            }
            set
            {
                this.NewsItemPictureTypeId = (int)value;
            }
        }
        public virtual bool? Located { get; set; }

        public virtual string LocationInfo { get; set; }
    }


    public enum NewsItemPictureType 
    {
        Main=0,
        Standard=1,
        Thumb=2
    }
}
