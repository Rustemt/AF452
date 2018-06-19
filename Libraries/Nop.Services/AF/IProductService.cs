using System;
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.AFEntities;
using Nop.Core;
namespace Nop.Services.Catalog
{
    public partial interface IProductService
    {
        Nop.Core.IPagedList<Nop.Core.Domain.Catalog.Product> SearchProducts(int categoryId, int manufacturerId, bool? featuredProducts,
           decimal? priceMin, decimal? priceMax, int productTagId,
           string keywords, bool searchDescriptions, int languageId,
           IList<int> filteredSpecs, ProductSortingEnumAF orderBy,
           int pageIndex, int pageSize, bool showHidden = false);
        void DeleteProductVariantPicture(Nop.Core.Domain.Catalog.ProductVariantPicture productVariantPicture);
        Nop.Core.Domain.Catalog.ProductVariantPicture GetProductVariantPictureById(int productVariantPictureId);
        System.Collections.Generic.IList<Nop.Core.Domain.Catalog.ProductVariantPicture> GetProductVariantPicturesByProductVariantId(int productVariantId);
        void InsertProductVariantPicture(Nop.Core.Domain.Catalog.ProductVariantPicture productVariantPicture);
        void UpdateProductVariantPicture(Nop.Core.Domain.Catalog.ProductVariantPicture productVariantPicture);
        int GetVariantStockQuantity(ProductVariant productVariant, string attributesXml);
        void AdjustInventory(ProductVariant productVariant, bool decrease, int quantity, string attributesXml, bool clearCache = false);
        void RequestBulkCatalog();

        IPagedList<Product> SearchProductsExt(IList<int> categoryIds,
    IList<int> manufacturerIds, bool? featuredProducts,
    decimal? priceMin, decimal? priceMax, int productTagId,
    string keywords, bool searchDescriptions, int languageId,
    IList<int> filteredSpecs, ProductSortingEnum orderBy,
    int pageIndex, int pageSize,
    bool loadFilterableSpecificationAttributeOptionIds, out IList<int> filterableSpecificationAttributeOptionIds,
    bool showHidden = false);

        IPagedList<ProductVariant> SearchProductVariantsExt(IList<int> categoryIds,
   IList<int> manufacturerIds, bool? featuredProducts,
   decimal? priceMin, decimal? priceMax, int productTagId,
   string keywords, bool searchDescriptions, int languageId,
   IList<int> filteredSpecs, IList<int> filteredAttributes, ProductSortingEnum orderBy,
   int pageIndex, int pageSize,
   bool loadFilterableSpecificationAttributeOptionIds, out IList<int> filterableSpecificationAttributeOptionIds,
            bool loadFilterableAttributeOptionIds, out IList<int> filterableAttributeOptionIds,
   bool showHidden = false);


        IPagedList<Product> SearchProductsFilterable(IList<int> categoryIds,
               int manufacturerId, bool? featuredProducts,
               decimal? priceMin, decimal? priceMax, int productTagId,
               string keywords, bool searchDescriptions, bool searchProductTags, int languageId,
               IList<int> filteredSpecs, IList<int> filteredProductVariantAttributes, IList<int> filteredManufacturers, ProductSortingEnum orderBy,
               int pageIndex, int pageSize,
               bool loadAvailableFilters,
                  out IList<int> filterableSpecificationAttributeOptionIds,
                  out IList<int> filterableProductVariantAttributeIds,
                  out IList<int> filterableManufacturerIds,
               bool showHidden = false, int fullTextMode = 0, bool useFullTextSearch = true);

        IPagedList<Product> SearchProductsFilterable(IList<int> categoryIds,
            int manufacturerId, bool? featuredProducts,
            decimal? priceMin, decimal? priceMax, int productTagId,
            string keywords, bool searchDescriptions, bool searchProductTags, int languageId,
            IList<int> filteredSpecs, IList<int> filteredProductVariantAttributes, IList<int> filteredManufacturers, ProductSortingEnum orderBy,
            int pageIndex, int pageSize,
            bool loadAvailableFilters,
               out IList<int> filterableSpecificationAttributeOptionIds,
               out IList<int> filterableProductVariantAttributeIds,
               out IList<int> filterableManufacturerIds,
               out IList<int> filterableCategoryIds,
               out decimal? maxPrice,
            bool showHidden = false, int fullTextMode = 0, bool useFullTextSearch = true);

        IPagedList<Product> SearchProductsFilterable(decimal? priceMin, decimal? priceMax, 
            int languageId, ProductSortingEnum orderBy,
         int pageIndex, int pageSize, bool loadAvailableFilters,
         bool showHidden = false, int fullTextMode = 0, bool useFullTextSearch = true);

        IList<ProductVariant> GetProductVariantsByProductIds(int[] productIds, bool showHidden = false);
        IList<Product> GetProductsByIds(int[] productIds, bool showHidden = false);
        IList<Product> GetSimilarProductsById(int productId, int count, int? categoryId = null);
    }
}
