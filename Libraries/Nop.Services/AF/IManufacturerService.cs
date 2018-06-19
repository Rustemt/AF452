using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.News;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Manufacturer service
    /// </summary>
    public partial interface IManufacturerService
    {
      
        /// <summary>
        /// Gets a manufacturer
        /// </summary>
        /// <returns>Manufacturer</returns>
        Manufacturer GetManufacturerBySeName(string SeName);

        void InsertNewsItemManufacturer(NewsItemManufacturer newsItemManufacturer);
        IList<NewsItemManufacturer> GetNewsItemManufacturersByNewsItemtId(int newsItemId, bool showHidden = false);
        NewsItemManufacturer GetNewsItemManufacturerById(int newsItemManufacturerId);
        void UpdateNewsItemManufacturer(NewsItemManufacturer newsItemManufacturer);
        void DeleteNewsItemManufacturer(NewsItemManufacturer newsItemManufacturer);
        NewsItem GetManufacturerNewsItemByManufacturerId(int manufacturerId,int languageId);
        NewsItemManufacturer GetNewsItemManufactureryByManufacturerId(int manufacturerId);
        IList<Manufacturer> GetManufacturersByIds(IList<int> manufacturerIds, bool showHidden = false);
        IList<Manufacturer> GetManufacturerByCategoryIds(IList<int> categoryIds);
    }
}
