using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.Media;

namespace Nop.Services.Media
{
    /// <summary>
    /// Picture service interface
    /// </summary>
    public partial interface IPictureService
    {
        IList<Picture> GetPicturesByProductVariantId(int productVariantId);
        IList<Picture> GetPicturesByProductVariantId(int productVariantId, int recordsToReturn);
       // IList<Picture> GetPicturesByNewsItemId(int newsItemId);
       // IList<Picture> GetPicturesByNewsItemId(int newsItemId, int recordsToReturn);
    }
}
