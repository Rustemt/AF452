using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.News;

namespace Nop.Services.News
{
    /// <summary>
    /// Extensions
    /// </summary>
    public static class NewsExtensions
    {
        /// <summary>
        /// Returns a ProductManufacturer that has the specified values
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="productId">Product identifier</param>
        /// <param name="manufacturerId">Manufacturer identifier</param>
        /// <returns>A ProductManufacturer that has the specified values; otherwise null</returns>
        public static NewsItemProduct FindNewsItemProduct(this IList<NewsItemProduct> source,
            int productId, int newsItemId)
        {
            foreach (var newsItemProduct in source)
                if (newsItemProduct.ProductId == productId && newsItemProduct.NewsItemId== newsItemId)
                    return newsItemProduct;

            return null;
        }
        public static NewsItemProductVariant FindNewsItemProductVariant(this IList<NewsItemProductVariant> source,
            int productVariantId, int newsItemId)
        {
            foreach (var newsItemProductVariant in source)
                if (newsItemProductVariant.ProductVariantId == productVariantId && newsItemProductVariant.NewsItemId == newsItemId)
                    return newsItemProductVariant;

            return null;
        }
    }
}
