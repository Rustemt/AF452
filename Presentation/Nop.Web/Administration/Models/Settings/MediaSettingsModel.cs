using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Admin.Models.Settings
{
    public class MediaSettingsModel : BaseNopModel
    {
        [NopResourceDisplayName("Admin.Configuration.Settings.Media.PicturesStoredIntoDatabase")]
        public bool PicturesStoredIntoDatabase { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.Media.AvatarPictureSize")]
        public int AvatarPictureSize { get; set; }
        [NopResourceDisplayName("Admin.Configuration.Settings.Media.ProductThumbPictureSize")]
        public int ProductThumbPictureSize { get; set; }
        [NopResourceDisplayName("Admin.Configuration.Settings.Media.ProductDetailsPictureSize")]
        public int ProductDetailsPictureSize { get; set; }
        [NopResourceDisplayName("Admin.Configuration.Settings.Media.ProductThumbPictureSizeOnProductDetailsPage")]
        public int ProductThumbPictureSizeOnProductDetailsPage { get; set; }
        [NopResourceDisplayName("Admin.Configuration.Settings.Media.ProductVariantPictureSize")]
        public int ProductVariantPictureSize { get; set; }
        [NopResourceDisplayName("Admin.Configuration.Settings.Media.CategoryThumbPictureSize")]
        public int CategoryThumbPictureSize { get; set; }
        [NopResourceDisplayName("Admin.Configuration.Settings.Media.ManufacturerThumbPictureSize")]
        public int ManufacturerThumbPictureSize { get; set; }
        [NopResourceDisplayName("Admin.Configuration.Settings.Media.CartThumbPictureSize")]
        public int CartThumbPictureSize { get; set; }
        [NopResourceDisplayName("Admin.Configuration.Settings.Media.MiniCartThumbPictureSize")]
        public int MiniCartThumbPictureSize { get; set; }
        [NopResourceDisplayName("Admin.Configuration.Settings.Media.NewsItemThumbPictureSize")]
        public int NewsItemThumbPictureSize { get; set; }
        [NopResourceDisplayName("Admin.Configuration.Settings.Media.NewsItemPictureSize")]
        public int NewsItemPictureSize { get; set; }
        [NopResourceDisplayName("Admin.Configuration.Settings.Media.NewsItemDetailPictureSize")]
        public int NewsItemDetailPictureSize { get; set; }
        [NopResourceDisplayName("Admin.Configuration.Settings.Media.NewsItemProductPictureSize")]
        public int NewsItemProductPictureSize { get; set; }
        
        [NopResourceDisplayName("Admin.Configuration.Settings.Media.MaximumImageSize")]
        public int MaximumImageSize { get; set; }
    }
}