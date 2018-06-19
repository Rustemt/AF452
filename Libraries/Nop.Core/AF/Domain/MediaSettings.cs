
using Nop.Core.Configuration;

namespace Nop.Core.Domain.Media
{
    public partial class MediaSettings : ISettings
    {
        public int MiniCartThumbPictureSize { get; set; }
        public int NewsItemThumbPictureSize { get; set; }
        public int NewsItemPictureSize { get; set; }
        public int NewsItemDetailPictureSize { get; set; }
        public int NewsItemProductPictureSize { get; set; }

    }
}