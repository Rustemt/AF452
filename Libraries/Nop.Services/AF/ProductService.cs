using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.AFEntities;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;
using Nop.Services.Messages;
using Nop.Core.Events;
using Nop.Services.Localization;
using Nop.Data;
using Nop.Core.Domain.Common;
using Nop.Core.Domain;
using Nop.Core.Infrastructure;
using System.Net;
using Nop.Services.Logging;
using Nop.Services.Seo;
using AlternativeDataAccess;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Product service
    /// </summary>
    public partial class ProductService : IProductService
    {
        #region Constants
        private const string SPECIFICATIONATTRIBUTE_BY_ID_KEY = "Nop.specificationattributes.id-{0}";
        #endregion

        #region Fields
        private readonly IRepository<SpecificationAttribute> _specificationAttributeRepository;
        private readonly IRepository<ProductVariantPicture> _productVariantPictureRepository;
		private readonly ICacheManager _cacheManagerPersistent;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="productRepository">Product repository</param>
        /// <param name="productVariantRepository">Product variant repository</param>
        /// <param name="relatedProductRepository">Related product repository</param>
        /// <param name="crossSellProductRepository">Cross-sell product repository</param>
        /// <param name="tierPriceRepository">Tier price repository</param>
        /// <param name="productPictureRepository">Product picture repository</param>
        /// <param name="productAttributeService">Product attribute service</param>
        /// <param name="productAttributeParser">Product attribute parser service</param>
        /// <param name="workflowMessageService">Workflow message service</param>
        /// <param name="localizationSettings">Localization settings</param>
        public ProductService(ICacheManager cacheManager,
            IRepository<Product> productRepository,
            IRepository<ProductVariant> productVariantRepository,
            IRepository<RelatedProduct> relatedProductRepository,
            IRepository<CrossSellProduct> crossSellProductRepository,
            IRepository<TierPrice> tierPriceRepository,
            IRepository<ProductPicture> productPictureRepository,
            IRepository<LocalizedProperty> localizedPropertyRepository,
            IProductAttributeService productAttributeService,
            IProductAttributeParser productAttributeParser,
            ILanguageService languageService,
            IWorkflowMessageService workflowMessageService,
            IDataProvider dataProvider, IDbContext dbContext,
            LocalizationSettings localizationSettings,
            IRepository<SpecificationAttribute> specificationAttributeRepository,
            IRepository<ProductVariantPicture> productVariantPictureRepository,
            CommonSettings commonSettings,
            IEventPublisher eventPublisher)
        {
            this._cacheManager = cacheManager;
            this._productRepository = productRepository;
            this._productVariantRepository = productVariantRepository;
            this._relatedProductRepository = relatedProductRepository;
            this._crossSellProductRepository = crossSellProductRepository;
            this._tierPriceRepository = tierPriceRepository;
            this._productPictureRepository = productPictureRepository;
            this._localizedPropertyRepository = localizedPropertyRepository;
            this._productAttributeService = productAttributeService;
            this._productAttributeParser = productAttributeParser;
            this._languageService = languageService;
            this._workflowMessageService = workflowMessageService;
            this._dataProvider = dataProvider;
            this._dbContext = dbContext;
            this._localizationSettings = localizationSettings;
            this._commonSettings = commonSettings;
            this._eventPublisher = eventPublisher;
            this._specificationAttributeRepository = specificationAttributeRepository;
            this._productVariantPictureRepository = productVariantPictureRepository;
			this._cacheManagerPersistent = EngineContext.Current.ContainerManager.Resolve<ICacheManager>("nop_cache_static");
        }

        #endregion

        #region public methods

        public virtual int GetVariantStockQuantity(ProductVariant productVariant, string attributesXml)
        {
            if (productVariant == null)
                throw new ArgumentNullException("productVariant");
            switch (productVariant.ManageInventoryMethod)
            {
                case ManageInventoryMethod.DontManageStock:
                    return 0;
                case ManageInventoryMethod.ManageStock:
                    return productVariant.StockQuantity;
                case ManageInventoryMethod.ManageStockByAttributes:

                    var combination = _productAttributeParser.FindProductVariantAttributeCombination(productVariant, attributesXml);
                    if (combination == null) return 0;
                    return combination.StockQuantity;
                default:
                    return 0;
            }
        }

        public virtual IPagedList<Product> SearchProducts(int categoryId, int manufacturerId, bool? featuredProducts,
           decimal? priceMin, decimal? priceMax, int productTagId,
           string keywords, bool searchDescriptions, int languageId,
           IList<int> filteredSpecs, ProductSortingEnumAF orderBy,
           int pageIndex, int pageSize, bool showHidden = false)
        {
            bool searchLocalizedValue = false;
            if (languageId > 0)
            {
                if (showHidden)
                {
                    searchLocalizedValue = true;
                }
                else
                {
                    //ensure that we have at least two published languages
                    var totalPublishedLanguages = _languageService.GetAllLanguages(false).Count;
                    searchLocalizedValue = totalPublishedLanguages >= 2;
                }
            }

            if (_commonSettings.UseStoredProceduresIfSupported && _dataProvider.StoredProceduredSupported)
            {
                //stored procedures are enabled and supported by the database. 
                //It's much faster than the LINQ implementation below 

                var pTotalRecords = new SqlParameter { ParameterName = "TotalRecords", Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int };

                string commaSeparatedSpecIds = "";
                if (filteredSpecs != null)
                {
                    ((List<int>)filteredSpecs).Sort();
                    for (int i = 0; i < filteredSpecs.Count; i++)
                    {
                        commaSeparatedSpecIds += filteredSpecs[i].ToString();
                        if (i != filteredSpecs.Count - 1)
                        {
                            commaSeparatedSpecIds += ",";
                        }
                    }
                }

                //some databases don't support int.MaxValue
                if (pageSize == int.MaxValue || pageSize == 0)
                    pageSize = int.MaxValue - 1;

                var products = _dbContext.ExecuteStoredProcedureList<Product>(
                    //"EXEC [ProductLoadAllPaged] @CategoryId, @ManufacturerId, @ProductTagId, @FeaturedProducts, @PriceMin, @PriceMax, @Keywords, @SearchDescriptions, @FilteredSpecs, @LanguageId, @OrderBy, @PageIndex, @PageSize, @ShowHidden, @TotalRecords",
                    "ProductLoadAllPaged",
                    new SqlParameter { ParameterName = "CategoryId", Value = categoryId, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "ManufacturerId", Value = manufacturerId, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "ProductTagId", Value = productTagId, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "FeaturedProducts", Value = featuredProducts.HasValue ? (object)featuredProducts.Value : DBNull.Value, SqlDbType = SqlDbType.Bit },
                    new SqlParameter { ParameterName = "PriceMin", Value = priceMin.HasValue ? (object)priceMin.Value : DBNull.Value, SqlDbType = SqlDbType.Decimal },
                    new SqlParameter { ParameterName = "PriceMax", Value = priceMax.HasValue ? (object)priceMax.Value : DBNull.Value, SqlDbType = SqlDbType.Decimal },
                    new SqlParameter { ParameterName = "Keywords", Value = keywords != null ? (object)keywords : DBNull.Value, SqlDbType = SqlDbType.NVarChar },
                    new SqlParameter { ParameterName = "SearchDescriptions", Value = searchDescriptions, SqlDbType = SqlDbType.Bit },
                    new SqlParameter { ParameterName = "FilteredSpecs", Value = commaSeparatedSpecIds != null ? (object)commaSeparatedSpecIds : DBNull.Value, SqlDbType = SqlDbType.NVarChar },
                    new SqlParameter { ParameterName = "LanguageId", Value = searchLocalizedValue ? languageId : 0, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "OrderBy", Value = (int)orderBy, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "PageIndex", Value = pageIndex, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "PageSize", Value = pageSize, SqlDbType = SqlDbType.Int },
                    new SqlParameter { ParameterName = "ShowHidden", Value = showHidden, SqlDbType = SqlDbType.Bit },
                    pTotalRecords);
                int totalRecords = (pTotalRecords.Value != DBNull.Value) ? Convert.ToInt32(pTotalRecords.Value) : 0;
                return new PagedList<Product>(products, pageIndex, pageSize, totalRecords);
            }
            else
            {
                //stored procedures aren't supported. Use LINQ

                #region Search products

                //products
                var query = _productRepository.Table;
                query = query.Where(p => !p.Deleted);
                if (!showHidden)
                {
                    query = query.Where(p => p.Published);
                }

                //searching by keyword
                if (!String.IsNullOrWhiteSpace(keywords))
                {
                    query = from p in query
                            join lp in _localizedPropertyRepository.Table on p.Id equals lp.EntityId into p_lp
                            from lp in p_lp.DefaultIfEmpty()
                            from pv in p.ProductVariants.DefaultIfEmpty()
                            where (p.Name.Contains(keywords)) ||
                                  (searchDescriptions && p.ShortDescription.Contains(keywords)) ||
                                  (searchDescriptions && p.FullDescription.Contains(keywords)) ||
                                  (pv.Name.Contains(keywords)) ||
                                  (searchDescriptions && pv.Description.Contains(keywords)) ||
                                //localized values
                                  (searchLocalizedValue && lp.LanguageId == languageId && lp.LocaleKeyGroup == "Product" && lp.LocaleKey == "Name" && lp.LocaleValue.Contains(keywords)) ||
                                  (searchDescriptions && searchLocalizedValue && lp.LanguageId == languageId && lp.LocaleKeyGroup == "Product" && lp.LocaleKey == "ShortDescription" && lp.LocaleValue.Contains(keywords)) ||
                                  (searchDescriptions && searchLocalizedValue && lp.LanguageId == languageId && lp.LocaleKeyGroup == "Product" && lp.LocaleKey == "FullDescription" && lp.LocaleValue.Contains(keywords))
                            select p;
                }

                //product variants
                //The function 'CurrentUtcDateTime' is not supported by SQL Server Compact. 
                //That's why we pass the date value
                var nowUtc = DateTime.UtcNow;
                query = from p in query
                        from pv in p.ProductVariants.DefaultIfEmpty()
                        where
                            //deleted
                            (showHidden || !pv.Deleted) &&
                            //published
                            (showHidden || pv.Published) &&
                            //price min
                            (
                                !priceMin.HasValue
                                ||
                            //special price (specified price and valid date range)
                                ((pv.SpecialPrice.HasValue && ((!pv.SpecialPriceStartDateTimeUtc.HasValue || pv.SpecialPriceStartDateTimeUtc.Value < nowUtc) && (!pv.SpecialPriceEndDateTimeUtc.HasValue || pv.SpecialPriceEndDateTimeUtc.Value > nowUtc))) && (pv.SpecialPrice >= priceMin.Value))
                                ||
                            //regular price (price isn't specified or date range isn't valid)
                                ((!pv.SpecialPrice.HasValue || ((pv.SpecialPriceStartDateTimeUtc.HasValue && pv.SpecialPriceStartDateTimeUtc.Value > nowUtc) || (pv.SpecialPriceEndDateTimeUtc.HasValue && pv.SpecialPriceEndDateTimeUtc.Value < nowUtc))) && (pv.Price >= priceMin.Value))
                            ) &&
                            //price max
                            (
                                !priceMax.HasValue
                                ||
                            //special price (specified price and valid date range)
                                ((pv.SpecialPrice.HasValue && ((!pv.SpecialPriceStartDateTimeUtc.HasValue || pv.SpecialPriceStartDateTimeUtc.Value < nowUtc) && (!pv.SpecialPriceEndDateTimeUtc.HasValue || pv.SpecialPriceEndDateTimeUtc.Value > nowUtc))) && (pv.SpecialPrice <= priceMax.Value))
                                ||
                            //regular price (price isn't specified or date range isn't valid)
                                ((!pv.SpecialPrice.HasValue || ((pv.SpecialPriceStartDateTimeUtc.HasValue && pv.SpecialPriceStartDateTimeUtc.Value > nowUtc) || (pv.SpecialPriceEndDateTimeUtc.HasValue && pv.SpecialPriceEndDateTimeUtc.Value < nowUtc))) && (pv.Price <= priceMax.Value))
                            ) &&
                            //available dates
                            (showHidden || (!pv.AvailableStartDateTimeUtc.HasValue || pv.AvailableStartDateTimeUtc.Value < nowUtc)) &&
                            (showHidden || (!pv.AvailableEndDateTimeUtc.HasValue || pv.AvailableEndDateTimeUtc.Value > nowUtc))
                        select p;


                //search by specs
                if (filteredSpecs != null && filteredSpecs.Count > 0)
                {
                    query = from p in query
                            where !filteredSpecs
                                       .Except(
                                           p.ProductSpecificationAttributes.Where(psa => psa.AllowFiltering).Select(
                                               psa => psa.SpecificationAttributeOptionId))
                                       .Any()
                            select p;
                }

                //category filtering
                if (categoryId > 0)
                {
                    query = from p in query
                            from pc in p.ProductCategories.Where(pc => pc.CategoryId == categoryId)
                            where (!featuredProducts.HasValue || featuredProducts.Value == pc.IsFeaturedProduct)
                            select p;
                }

                //manufacturer filtering
                if (manufacturerId > 0)
                {
                    query = from p in query
                            from pm in p.ProductManufacturers.Where(pm => pm.ManufacturerId == manufacturerId)
                            where (!featuredProducts.HasValue || featuredProducts.Value == pm.IsFeaturedProduct)
                            select p;
                }

                //related products filtering
                //if (relatedToProductId > 0)
                //{
                //    query = from p in query
                //            join rp in _relatedProductRepository.Table on p.Id equals rp.ProductId2
                //            where (relatedToProductId == rp.ProductId1)
                //            select p;
                //}

                //tag filtering
                if (productTagId > 0)
                {
                    query = from p in query
                            from pt in p.ProductTags.Where(pt => pt.Id == productTagId)
                            select p;
                }

                //only distinct products (group by ID)
                //if we use standard Distinct() method, then all fields will be compared (low performance)
                //it'll not work in SQL Server Compact when searching products by a keyword)
                query = from p in query
                        group p by p.Id
                            into pGroup
                            orderby pGroup.Key
                            select pGroup.FirstOrDefault();

                //sort products
                if (orderBy == ProductSortingEnumAF.Position && categoryId > 0)
                {
                    //category position
                    query = query.OrderBy(p => p.ProductCategories.FirstOrDefault().DisplayOrder);
                }
                else if (orderBy == ProductSortingEnumAF.Position && manufacturerId > 0)
                {
                    //manufacturer position
                    query = query.OrderBy(p => p.ProductManufacturers.FirstOrDefault().DisplayOrder);
                }
                else if (orderBy == ProductSortingEnumAF.Position)
                {
                    query = query.OrderBy(p => p.Name);
                }
                else if (orderBy == ProductSortingEnumAF.Name)
                {
                    query = query.OrderBy(p => p.Name);
                }
                else if (orderBy == ProductSortingEnumAF.PriceAscending)
                {
                    query = query.OrderBy(p => p.ProductVariants.FirstOrDefault().Price);
                }
                else if (orderBy == ProductSortingEnumAF.PriceDescending)
                {
                    query = query.OrderByDescending(p => p.ProductVariants.FirstOrDefault().Price);
                }
                else if (orderBy == ProductSortingEnumAF.CreatedOn)
                    query = query.OrderByDescending(p => p.CreatedOnUtc);
                else
                    query = query.OrderBy(p => p.Name);

                var products = new PagedList<Product>(query, pageIndex, pageSize);
                return products;

                #endregion
            }
        }



        public virtual IPagedList<Product> SearchProductsExt(IList<int> categoryIds,
    IList<int> manufacturerIds, bool? featuredProducts,
    decimal? priceMin, decimal? priceMax, int productTagId,
    string keywords, bool searchDescriptions, int languageId,
    IList<int> filteredSpecs, ProductSortingEnum orderBy,
    int pageIndex, int pageSize,
    bool loadFilterableSpecificationAttributeOptionIds, out IList<int> filterableSpecificationAttributeOptionIds,
    bool showHidden = false)
        {
            filterableSpecificationAttributeOptionIds = new List<int>();
            bool searchLocalizedValue = false;
            if (languageId > 0)
            {
                if (showHidden)
                {
                    searchLocalizedValue = true;
                }
                else
                {
                    //ensure that we have at least two published languages
                    var totalPublishedLanguages = _languageService.GetAllLanguages(false).Count;
                    searchLocalizedValue = totalPublishedLanguages >= 2;
                }
            }




            #region Use stored procedure

            //pass categry identifiers as comma-delimited string
            string commaSeparatedCategoryIds = "";
            if (categoryIds != null)
            {
                //for (int i = 0; i < categoryIds.Count; i++)
                //{
                //    commaSeparatedCategoryIds += categoryIds[i].ToString();
                //    if (i != categoryIds.Count - 1)
                //    {
                //        commaSeparatedCategoryIds += ",";
                //    }
                //}

                commaSeparatedCategoryIds = String.Join(",", categoryIds);

            }
            string commaSeparatedManufacturerIds = "";
            if (manufacturerIds != null)
            {
                //for (int i = 0; i < manufacturerIds.Count; i++)
                //{
                //    commaSeparatedManufacturerIds += manufacturerIds[i].ToString();
                //    if (i != manufacturerIds.Count - 1)
                //    {
                //        commaSeparatedManufacturerIds += ",";
                //    }
                //}
                commaSeparatedManufacturerIds = String.Join(",", manufacturerIds);
            }

            //pass specification identifiers as comma-delimited string
            string commaSeparatedSpecIds = "";
            if (filteredSpecs != null)
            {
                ((List<int>)filteredSpecs).Sort();
                for (int i = 0; i < filteredSpecs.Count; i++)
                {
                    commaSeparatedSpecIds += filteredSpecs[i].ToString();
                    if (i != filteredSpecs.Count - 1)
                    {
                        commaSeparatedSpecIds += ",";
                    }
                }
            }

            //some databases don't support int.MaxValue
            if (pageSize == int.MaxValue)
                pageSize = int.MaxValue - 1;

            //prepare parameters
            var pCategoryIds = new SqlParameter();
            pCategoryIds.ParameterName = "CategoryIds";
            pCategoryIds.Value = commaSeparatedCategoryIds != null ? (object)commaSeparatedCategoryIds : DBNull.Value;
            pCategoryIds.DbType = DbType.String;

            var pManufacturerIds = new SqlParameter();
            pManufacturerIds.ParameterName = "ManufacturerIds";
            pManufacturerIds.Value = commaSeparatedManufacturerIds != null ? (object)commaSeparatedManufacturerIds : DBNull.Value;
            pManufacturerIds.DbType = DbType.String;

            var pProductTagId = new SqlParameter();
            pProductTagId.ParameterName = "ProductTagId";
            pProductTagId.Value = productTagId;
            pProductTagId.DbType = DbType.Int32;

            var pFeaturedProducts = new SqlParameter();
            pFeaturedProducts.ParameterName = "FeaturedProducts";
            pFeaturedProducts.Value = featuredProducts.HasValue ? (object)featuredProducts.Value : DBNull.Value;
            pFeaturedProducts.DbType = DbType.Boolean;

            var pPriceMin = new SqlParameter();
            pPriceMin.ParameterName = "PriceMin";
            pPriceMin.Value = priceMin.HasValue ? (object)priceMin.Value : DBNull.Value;
            pPriceMin.DbType = DbType.Decimal;

            var pPriceMax = new SqlParameter();
            pPriceMax.ParameterName = "PriceMax";
            pPriceMax.Value = priceMax.HasValue ? (object)priceMax.Value : DBNull.Value;
            pPriceMax.DbType = DbType.Decimal;

            var pKeywords = new SqlParameter();
            pKeywords.ParameterName = "Keywords";
            pKeywords.Value = keywords != null ? (object)keywords : DBNull.Value;
            pKeywords.DbType = DbType.String;

            var pSearchDescriptions = new SqlParameter();
            pSearchDescriptions.ParameterName = "SearchDescriptions";
            pSearchDescriptions.Value = searchDescriptions;
            pSearchDescriptions.DbType = DbType.Boolean;

            var pUseFullTextSearch = new SqlParameter();
            pUseFullTextSearch.ParameterName = "UseFullTextSearch";
            pUseFullTextSearch.Value = false;//_commonSettings.UseFullTextSearch;
            pUseFullTextSearch.DbType = DbType.Boolean;

            var pFullTextMode = new SqlParameter();
            pFullTextMode.ParameterName = "FullTextMode";
            pFullTextMode.Value = 0;//(int)_commonSettings.FullTextMode;
            pFullTextMode.DbType = DbType.Int32;

            var pFilteredSpecs = new SqlParameter();
            pFilteredSpecs.ParameterName = "FilteredSpecs";
            pFilteredSpecs.Value = commaSeparatedSpecIds != null ? (object)commaSeparatedSpecIds : DBNull.Value;
            pFilteredSpecs.DbType = DbType.String;

            var pLanguageId = new SqlParameter();
            pLanguageId.ParameterName = "LanguageId";
            pLanguageId.Value = searchLocalizedValue ? languageId : 0;
            pLanguageId.DbType = DbType.Int32;

            var pOrderBy = new SqlParameter();
            pOrderBy.ParameterName = "OrderBy";
            pOrderBy.Value = (int)orderBy;
            pOrderBy.DbType = DbType.Int32;

            var pPageIndex = new SqlParameter();
            pPageIndex.ParameterName = "PageIndex";
            pPageIndex.Value = pageIndex;
            pPageIndex.DbType = DbType.Int32;

            var pPageSize = new SqlParameter();
            pPageSize.ParameterName = "PageSize";
            pPageSize.Value = pageSize;
            pPageSize.DbType = DbType.Int32;

            var pShowHidden = new SqlParameter();
            pShowHidden.ParameterName = "ShowHidden";
            pShowHidden.Value = showHidden;
            pShowHidden.DbType = DbType.Boolean;

            var pLoadFilterableSpecificationAttributeOptionIds = new SqlParameter();
            pLoadFilterableSpecificationAttributeOptionIds.ParameterName = "LoadFilterableSpecificationAttributeOptionIds";
            pLoadFilterableSpecificationAttributeOptionIds.Value = loadFilterableSpecificationAttributeOptionIds;
            pLoadFilterableSpecificationAttributeOptionIds.DbType = DbType.Boolean;

            var pFilterableSpecificationAttributeOptionIds = new SqlParameter();
            pFilterableSpecificationAttributeOptionIds.ParameterName = "FilterableSpecificationAttributeOptionIds";
            pFilterableSpecificationAttributeOptionIds.Direction = ParameterDirection.Output;
            pFilterableSpecificationAttributeOptionIds.Size = int.MaxValue - 1;
            pFilterableSpecificationAttributeOptionIds.DbType = DbType.String;

            var pTotalRecords = new SqlParameter();
            pTotalRecords.ParameterName = "TotalRecords";
            pTotalRecords.Direction = ParameterDirection.Output;
            pTotalRecords.DbType = DbType.Int32;

            //invoke stored procedure
            var products = _dbContext.ExecuteStoredProcedureList<Product>(
                "ProductLoadAllPagedExt",
                pCategoryIds,
                pManufacturerIds,
                pProductTagId,
                pFeaturedProducts,
                pPriceMin,
                pPriceMax,
                pKeywords,
                pSearchDescriptions,
                pUseFullTextSearch,
                pFullTextMode,
                pFilteredSpecs,
                pLanguageId,
                pOrderBy,
                pPageIndex,
                pPageSize,
                pShowHidden,
                pLoadFilterableSpecificationAttributeOptionIds,
                pFilterableSpecificationAttributeOptionIds,
                pTotalRecords);
            //get filterable specification attribute option identifier
            string filterableSpecificationAttributeOptionIdsStr = (pFilterableSpecificationAttributeOptionIds.Value != DBNull.Value) ? (string)pFilterableSpecificationAttributeOptionIds.Value : "";
            if (loadFilterableSpecificationAttributeOptionIds &&
                !string.IsNullOrWhiteSpace(filterableSpecificationAttributeOptionIdsStr))
            {
                filterableSpecificationAttributeOptionIds = filterableSpecificationAttributeOptionIdsStr
                   .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                   .Select(x => Convert.ToInt32(x.Trim()))
                   .ToList();
            }
            //return products
            int totalRecords = (pTotalRecords.Value != DBNull.Value) ? Convert.ToInt32(pTotalRecords.Value) : 0;
            return new PagedList<Product>(products, pageIndex, pageSize, totalRecords);

            #endregion


        }


        public virtual IPagedList<ProductVariant> SearchProductVariantsExt(IList<int> categoryIds,
   IList<int> manufacturerIds, bool? featuredProducts,
   decimal? priceMin, decimal? priceMax, int productTagId,
   string keywords, bool searchDescriptions, int languageId,
   IList<int> filteredSpecs, IList<int> filteredAttributes, ProductSortingEnum orderBy,
   int pageIndex, int pageSize,
   bool loadFilterableSpecificationAttributeOptionIds, out IList<int> filterableSpecificationAttributeOptionIds,
            bool loadFilterableAttributeOptionIds, out IList<int> filterableAttributeOptionIds,
   bool showHidden = false)
        {
            filterableSpecificationAttributeOptionIds = new List<int>();
            filterableAttributeOptionIds = new List<int>();
            bool searchLocalizedValue = false;
            if (languageId > 0)
            {
                if (showHidden)
                {
                    searchLocalizedValue = true;
                }
                else
                {
                    //ensure that we have at least two published languages
                    var totalPublishedLanguages = _languageService.GetAllLanguages(false).Count;
                    searchLocalizedValue = totalPublishedLanguages >= 2;
                }
            }


            //pass categry identifiers as comma-delimited string
            string commaSeparatedCategoryIds = "";
            if (categoryIds != null)
            {
                //for (int i = 0; i < categoryIds.Count; i++)
                //{
                //    commaSeparatedCategoryIds += categoryIds[i].ToString();
                //    if (i != categoryIds.Count - 1)
                //    {
                //        commaSeparatedCategoryIds += ",";
                //    }
                //}

                commaSeparatedCategoryIds = String.Join(",", categoryIds);

            }
            string commaSeparatedManufacturerIds = "";
            if (manufacturerIds != null)
            {
                //for (int i = 0; i < manufacturerIds.Count; i++)
                //{
                //    commaSeparatedManufacturerIds += manufacturerIds[i].ToString();
                //    if (i != manufacturerIds.Count - 1)
                //    {
                //        commaSeparatedManufacturerIds += ",";
                //    }
                //}
                commaSeparatedManufacturerIds = String.Join(",", manufacturerIds);
            }

            //pass specification identifiers as comma-delimited string
            string commaSeparatedSpecIds = "";
            if (filteredSpecs != null)
            {
                ((List<int>)filteredSpecs).Sort();
                for (int i = 0; i < filteredSpecs.Count; i++)
                {
                    commaSeparatedSpecIds += filteredSpecs[i].ToString();
                    if (i != filteredSpecs.Count - 1)
                    {
                        commaSeparatedSpecIds += ",";
                    }
                }
            }

            //pass product attributes identifiers as comma-delimited string
            string commaSeparatedAttributeIds = "";
            if (filteredAttributes != null)
            {
                ((List<int>)filteredAttributes).Sort();
                for (int i = 0; i < filteredAttributes.Count; i++)
                {
                    commaSeparatedAttributeIds += filteredAttributes[i].ToString();
                    if (i != filteredAttributes.Count - 1)
                    {
                        commaSeparatedAttributeIds += ",";
                    }
                }
            }


            //some databases don't support int.MaxValue
            if (pageSize == int.MaxValue)
                pageSize = int.MaxValue - 1;

            //prepare parameters
            var pCategoryIds = new SqlParameter();
            pCategoryIds.ParameterName = "CategoryIds";
            pCategoryIds.Value = commaSeparatedCategoryIds != null ? (object)commaSeparatedCategoryIds : DBNull.Value;
            pCategoryIds.DbType = DbType.String;

            var pManufacturerIds = new SqlParameter();
            pManufacturerIds.ParameterName = "ManufacturerIds";
            pManufacturerIds.Value = commaSeparatedManufacturerIds != null ? (object)commaSeparatedManufacturerIds : DBNull.Value;
            pManufacturerIds.DbType = DbType.String;

            var pProductTagId = new SqlParameter();
            pProductTagId.ParameterName = "ProductTagId";
            pProductTagId.Value = productTagId;
            pProductTagId.DbType = DbType.Int32;

            var pFeaturedProducts = new SqlParameter();
            pFeaturedProducts.ParameterName = "FeaturedProducts";
            pFeaturedProducts.Value = featuredProducts.HasValue ? (object)featuredProducts.Value : DBNull.Value;
            pFeaturedProducts.DbType = DbType.Boolean;

            var pPriceMin = new SqlParameter();
            pPriceMin.ParameterName = "PriceMin";
            pPriceMin.Value = priceMin.HasValue ? (object)priceMin.Value : DBNull.Value;
            pPriceMin.DbType = DbType.Decimal;

            var pPriceMax = new SqlParameter();
            pPriceMax.ParameterName = "PriceMax";
            pPriceMax.Value = priceMax.HasValue ? (object)priceMax.Value : DBNull.Value;
            pPriceMax.DbType = DbType.Decimal;

            var pKeywords = new SqlParameter();
            pKeywords.ParameterName = "Keywords";
            pKeywords.Value = keywords != null ? (object)keywords : DBNull.Value;
            pKeywords.DbType = DbType.String;

            var pSearchDescriptions = new SqlParameter();
            pSearchDescriptions.ParameterName = "SearchDescriptions";
            pSearchDescriptions.Value = searchDescriptions;
            pSearchDescriptions.DbType = DbType.Boolean;

            var pUseFullTextSearch = new SqlParameter();
            pUseFullTextSearch.ParameterName = "UseFullTextSearch";
            pUseFullTextSearch.Value = false;//_commonSettings.UseFullTextSearch;
            pUseFullTextSearch.DbType = DbType.Boolean;

            var pFullTextMode = new SqlParameter();
            pFullTextMode.ParameterName = "FullTextMode";
            pFullTextMode.Value = 0;//(int)_commonSettings.FullTextMode;
            pFullTextMode.DbType = DbType.Int32;

            var pFilteredSpecs = new SqlParameter();
            pFilteredSpecs.ParameterName = "FilteredSpecs";
            pFilteredSpecs.Value = commaSeparatedSpecIds != null ? (object)commaSeparatedSpecIds : DBNull.Value;
            pFilteredSpecs.DbType = DbType.String;

            var pFilteredAttributes = new SqlParameter();
            pFilteredAttributes.ParameterName = "FilteredAttributes";
            pFilteredAttributes.Value = commaSeparatedAttributeIds != null ? (object)commaSeparatedAttributeIds : DBNull.Value;
            pFilteredAttributes.DbType = DbType.String;

            var pLanguageId = new SqlParameter();
            pLanguageId.ParameterName = "LanguageId";
            pLanguageId.Value = searchLocalizedValue ? languageId : 0;
            pLanguageId.DbType = DbType.Int32;

            var pOrderBy = new SqlParameter();
            pOrderBy.ParameterName = "OrderBy";
            pOrderBy.Value = (int)orderBy;
            pOrderBy.DbType = DbType.Int32;

            var pPageIndex = new SqlParameter();
            pPageIndex.ParameterName = "PageIndex";
            pPageIndex.Value = pageIndex;
            pPageIndex.DbType = DbType.Int32;

            var pPageSize = new SqlParameter();
            pPageSize.ParameterName = "PageSize";
            pPageSize.Value = pageSize;
            pPageSize.DbType = DbType.Int32;

            var pShowHidden = new SqlParameter();
            pShowHidden.ParameterName = "ShowHidden";
            pShowHidden.Value = showHidden;
            pShowHidden.DbType = DbType.Boolean;

            var pLoadFilterableSpecificationAttributeOptionIds = new SqlParameter();
            pLoadFilterableSpecificationAttributeOptionIds.ParameterName = "LoadFilterableSpecificationAttributeOptionIds";
            pLoadFilterableSpecificationAttributeOptionIds.Value = loadFilterableSpecificationAttributeOptionIds;
            pLoadFilterableSpecificationAttributeOptionIds.DbType = DbType.Boolean;

            var pFilterableSpecificationAttributeOptionIds = new SqlParameter();
            pFilterableSpecificationAttributeOptionIds.ParameterName = "FilterableSpecificationAttributeOptionIds";
            pFilterableSpecificationAttributeOptionIds.Direction = ParameterDirection.Output;
            pFilterableSpecificationAttributeOptionIds.Size = int.MaxValue - 1;
            pFilterableSpecificationAttributeOptionIds.DbType = DbType.String;

            var pLoadFilterableAttributeOptionIds = new SqlParameter();
            pLoadFilterableAttributeOptionIds.ParameterName = "LoadFilterableAttributeOptionIds";
            pLoadFilterableAttributeOptionIds.Value = loadFilterableAttributeOptionIds;
            pLoadFilterableAttributeOptionIds.DbType = DbType.Boolean;

            var pFilterableAttributeOptionIds = new SqlParameter();
            pFilterableAttributeOptionIds.ParameterName = "FilterableAttributeOptionIds";
            pFilterableAttributeOptionIds.Direction = ParameterDirection.Output;
            pFilterableAttributeOptionIds.Size = int.MaxValue - 1;
            pFilterableAttributeOptionIds.DbType = DbType.String;

            var pTotalRecords = new SqlParameter();
            pTotalRecords.ParameterName = "TotalRecords";
            pTotalRecords.Direction = ParameterDirection.Output;
            pTotalRecords.DbType = DbType.Int32;

            //invoke stored procedure
            var productVariants = _dbContext.ExecuteStoredProcedureList<ProductVariant>(
                "ProductVariantLoadAllPagedExt",
                pCategoryIds,
                pManufacturerIds,
                pProductTagId,
                pFeaturedProducts,
                pPriceMin,
                pPriceMax,
                pKeywords,
                pSearchDescriptions,
                pUseFullTextSearch,
                pFullTextMode,
                pFilteredSpecs,
                pFilteredAttributes,
                pLanguageId,
                pOrderBy,
                pPageIndex,
                pPageSize,
                pShowHidden,
                pLoadFilterableSpecificationAttributeOptionIds,
                pFilterableSpecificationAttributeOptionIds,
                pLoadFilterableAttributeOptionIds,
                pFilterableAttributeOptionIds,
                pTotalRecords);
            //get filterable specification attribute option identifier
            string filterableSpecificationAttributeOptionIdsStr = (pFilterableSpecificationAttributeOptionIds.Value != DBNull.Value) ? (string)pFilterableSpecificationAttributeOptionIds.Value : "";
            if (loadFilterableSpecificationAttributeOptionIds &&
                !string.IsNullOrWhiteSpace(filterableSpecificationAttributeOptionIdsStr))
            {
                filterableSpecificationAttributeOptionIds = filterableSpecificationAttributeOptionIdsStr
                   .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                   .Select(x => Convert.ToInt32(x.Trim()))
                   .ToList();
            }

            //get filterable  attribute option identifier
            string filterableAttributeOptionIdsStr = (pFilterableAttributeOptionIds.Value != DBNull.Value) ? (string)pFilterableAttributeOptionIds.Value : "";
            if (loadFilterableAttributeOptionIds &&
                !string.IsNullOrWhiteSpace(filterableAttributeOptionIdsStr))
            {
                filterableAttributeOptionIds = filterableAttributeOptionIdsStr
                   .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                   .Select(x => Convert.ToInt32(x.Trim()))
                   .ToList();
            }

            //return products
            int totalRecords = (pTotalRecords.Value != DBNull.Value) ? Convert.ToInt32(pTotalRecords.Value) : 0;
            return new PagedList<ProductVariant>(productVariants, pageIndex, pageSize, totalRecords);




        }


        public virtual IPagedList<Product> SearchProductsFilterable(IList<int> categoryIds,
         int manufacturerId, bool? featuredProducts,
         decimal? priceMin, decimal? priceMax, int productTagId,
         string keywords, bool searchDescriptions, bool searchProductTags, int languageId,
         IList<int> filteredSpecs, IList<int> filteredProductVariantAttributes, IList<int> filteredManufacturers, ProductSortingEnum orderBy,
         int pageIndex, int pageSize,
         bool loadAvailableFilters,
            out IList<int> filterableSpecificationAttributeOptionIds,
            out IList<int> filterableProductVariantAttributeIds,
            out IList<int> filterableManufacturerIds,
         bool showHidden = false, int fullTextMode = 0, bool useFullTextSearch = true)
        {
            filterableSpecificationAttributeOptionIds = new List<int>();
            filterableProductVariantAttributeIds = new List<int>();
            filterableManufacturerIds = new List<int>();
            bool searchLocalizedValue = false;
            if (languageId > 0)
            {
                //if (showHidden)
                //{
                //    searchLocalizedValue = true;
                //}
                //else
                //{
                //    //ensure that we have at least two published languages
                //    var totalPublishedLanguages = _languageService.GetAllLanguages(false).Count;
                //    searchLocalizedValue = totalPublishedLanguages >= 2;
                //}

                searchLocalizedValue = true;
            }


            #region Use stored procedure

            //pass category identifiers as comma-delimited string
            string commaSeparatedCategoryIds = "";
            if (categoryIds != null)
            {
                for (int i = 0; i < categoryIds.Count; i++)
                {
                    commaSeparatedCategoryIds += categoryIds[i].ToString();
                    if (i != categoryIds.Count - 1)
                    {
                        commaSeparatedCategoryIds += ",";
                    }
                }
            }

            //pass specification identifiers as comma-delimited string
            string commaSeparatedSpecIds = "";
            if (filteredSpecs != null)
            {
                ((List<int>)filteredSpecs).Sort();
                for (int i = 0; i < filteredSpecs.Count; i++)
                {
                    commaSeparatedSpecIds += filteredSpecs[i].ToString();
                    if (i != filteredSpecs.Count - 1)
                    {
                        commaSeparatedSpecIds += ",";
                    }
                }
            }

            //pass productvariant attribute identifiers as comma-delimited string
            string commaSeparatedProductVariantAttributeIds = "";
            if (filteredProductVariantAttributes != null)
            {
                ((List<int>)filteredProductVariantAttributes).Sort();
                for (int i = 0; i < filteredProductVariantAttributes.Count; i++)
                {
                    commaSeparatedProductVariantAttributeIds += filteredProductVariantAttributes[i].ToString();
                    if (i != filteredProductVariantAttributes.Count - 1)
                    {
                        commaSeparatedProductVariantAttributeIds += ",";
                    }
                }
            }

            //pass specification identifiers as comma-delimited string
            string commaSeparatedManufacturerIds = "";
            if (filteredManufacturers != null)
            {
                ((List<int>)filteredManufacturers).Sort();
                for (int i = 0; i < filteredManufacturers.Count; i++)
                {
                    commaSeparatedManufacturerIds += filteredManufacturers[i].ToString();
                    if (i != filteredManufacturers.Count - 1)
                    {
                        commaSeparatedManufacturerIds += ",";
                    }
                }
            }

            //some databases don't support int.MaxValue
            if (pageSize == int.MaxValue)
                pageSize = int.MaxValue - 1;

            //prepare parameters
            var pCategoryIds = new SqlParameter();
            pCategoryIds.ParameterName = "CategoryIds";
            pCategoryIds.Value = commaSeparatedCategoryIds != null ? (object)commaSeparatedCategoryIds : DBNull.Value;
            pCategoryIds.DbType = DbType.String;

            var pManufacturerId = new SqlParameter();
            pManufacturerId.ParameterName = "ManufacturerId";
            pManufacturerId.Value = manufacturerId;
            pManufacturerId.DbType = DbType.Int32;

            var pProductTagId = new SqlParameter();
            pProductTagId.ParameterName = "ProductTagId";
            pProductTagId.Value = productTagId;
            pProductTagId.DbType = DbType.Int32;

            var pFeaturedProducts = new SqlParameter();
            pFeaturedProducts.ParameterName = "FeaturedProducts";
            pFeaturedProducts.Value = featuredProducts.HasValue ? (object)featuredProducts.Value : DBNull.Value;
            pFeaturedProducts.DbType = DbType.Boolean;

            var pPriceMin = new SqlParameter();
            pPriceMin.ParameterName = "PriceMin";
            pPriceMin.Value = priceMin.HasValue ? (object)priceMin.Value : DBNull.Value;
            pPriceMin.DbType = DbType.Decimal;

            var pPriceMax = new SqlParameter();
            pPriceMax.ParameterName = "PriceMax";
            pPriceMax.Value = priceMax.HasValue ? (object)priceMax.Value : DBNull.Value;
            pPriceMax.DbType = DbType.Decimal;

            var pKeywords = new SqlParameter();
            pKeywords.ParameterName = "Keywords";
            pKeywords.Value = keywords != null ? (object)keywords : DBNull.Value;
            pKeywords.DbType = DbType.String;

            var pSearchDescriptions = new SqlParameter();
            pSearchDescriptions.ParameterName = "SearchDescriptions";
            pSearchDescriptions.Value = searchDescriptions;
            pSearchDescriptions.DbType = DbType.Boolean;

            var pSearchProductTags = new SqlParameter();
            pSearchProductTags.ParameterName = "SearchProductTags";
            pSearchProductTags.Value = searchProductTags;
            pSearchProductTags.DbType = DbType.Boolean;

            var pUseFullTextSearch = new SqlParameter();
            pUseFullTextSearch.ParameterName = "UseFullTextSearch";
            pUseFullTextSearch.Value = useFullTextSearch;
            pUseFullTextSearch.DbType = DbType.Boolean;

            var pFullTextMode = new SqlParameter();
            pFullTextMode.ParameterName = "FullTextMode";
            pFullTextMode.Value = fullTextMode;
            pFullTextMode.DbType = DbType.Int32;

            var pFilteredSpecs = new SqlParameter();
            pFilteredSpecs.ParameterName = "FilteredSpecs";
            pFilteredSpecs.Value = commaSeparatedSpecIds != null ? (object)commaSeparatedSpecIds : DBNull.Value;
            pFilteredSpecs.DbType = DbType.String;

            var pFilteredProductVariantAttributes = new SqlParameter();
            pFilteredProductVariantAttributes.ParameterName = "FilteredProductVariantAttributes";
            pFilteredProductVariantAttributes.Value = commaSeparatedProductVariantAttributeIds != null ? (object)commaSeparatedProductVariantAttributeIds : DBNull.Value;
            pFilteredProductVariantAttributes.DbType = DbType.String;

            var pFilteredManufacturers = new SqlParameter();
            pFilteredManufacturers.ParameterName = "FilteredManufacturers";
            pFilteredManufacturers.Value = commaSeparatedManufacturerIds != null ? (object)commaSeparatedManufacturerIds : DBNull.Value;
            pFilteredManufacturers.DbType = DbType.String;

            var pLanguageId = new SqlParameter();
            pLanguageId.ParameterName = "LanguageId";
            pLanguageId.Value = searchLocalizedValue ? languageId : 0;
            pLanguageId.DbType = DbType.Int32;

            var pOrderBy = new SqlParameter();
            pOrderBy.ParameterName = "OrderBy";
            pOrderBy.Value = (int)orderBy;
            pOrderBy.DbType = DbType.Int32;

            var pPageIndex = new SqlParameter();
            pPageIndex.ParameterName = "PageIndex";
            pPageIndex.Value = pageIndex;
            pPageIndex.DbType = DbType.Int32;

            var pPageSize = new SqlParameter();
            pPageSize.ParameterName = "PageSize";
            pPageSize.Value = pageSize;
            pPageSize.DbType = DbType.Int32;

            var pShowHidden = new SqlParameter();
            pShowHidden.ParameterName = "ShowHidden";
            pShowHidden.Value = showHidden;
            pShowHidden.DbType = DbType.Boolean;

            var pLoadAvailableFilters = new SqlParameter();
            pLoadAvailableFilters.ParameterName = "LoadAvailableFilters";
            pLoadAvailableFilters.Value = loadAvailableFilters;
            pLoadAvailableFilters.DbType = DbType.Boolean;

            var pFilterableSpecificationAttributeOptionIds = new SqlParameter();
            pFilterableSpecificationAttributeOptionIds.ParameterName = "FilterableSpecificationAttributeOptionIds";
            pFilterableSpecificationAttributeOptionIds.Direction = ParameterDirection.Output;
            pFilterableSpecificationAttributeOptionIds.Size = int.MaxValue - 1;
            pFilterableSpecificationAttributeOptionIds.DbType = DbType.String;

            var pFilterableProductVariantAttributeIds = new SqlParameter();
            pFilterableProductVariantAttributeIds.ParameterName = "FilterableProductVariantAttributeIds";
            pFilterableProductVariantAttributeIds.Direction = ParameterDirection.Output;
            pFilterableProductVariantAttributeIds.Size = int.MaxValue - 1;
            pFilterableProductVariantAttributeIds.DbType = DbType.String;

            var pFilterableManufacturerIds = new SqlParameter();
            pFilterableManufacturerIds.ParameterName = "FilterableManufacturerIds";
            pFilterableManufacturerIds.Direction = ParameterDirection.Output;
            pFilterableManufacturerIds.Size = int.MaxValue - 1;
            pFilterableManufacturerIds.DbType = DbType.String;

            var pTotalRecords = new SqlParameter();
            pTotalRecords.ParameterName = "TotalRecords";
            pTotalRecords.Direction = ParameterDirection.Output;
            pTotalRecords.DbType = DbType.Int32;

            //invoke stored procedure
            var products = _dbContext.ExecuteStoredProcedureList<Product>(
                "ProductLoadAllPagedFilterable",
                pCategoryIds,
                pManufacturerId,
                pProductTagId,
                pFeaturedProducts,
                pPriceMin,
                pPriceMax,
                pKeywords,
                pSearchDescriptions,
                pSearchProductTags,
                pUseFullTextSearch,
                pFullTextMode,
                pFilteredSpecs,
                pFilteredProductVariantAttributes,
                pFilteredManufacturers,
                pLanguageId,
                pOrderBy,
                pPageIndex,
                pPageSize,
                pShowHidden,
                pLoadAvailableFilters,
                pFilterableSpecificationAttributeOptionIds,
                pFilterableProductVariantAttributeIds,
                pFilterableManufacturerIds,
                pTotalRecords);
            //get filterable specification attribute option identifier
            string filterableSpecificationAttributeOptionIdsStr = (pFilterableSpecificationAttributeOptionIds.Value != DBNull.Value) ? (string)pFilterableSpecificationAttributeOptionIds.Value : "";
            if (loadAvailableFilters &&
                !string.IsNullOrWhiteSpace(filterableSpecificationAttributeOptionIdsStr))
            {
                filterableSpecificationAttributeOptionIds = filterableSpecificationAttributeOptionIdsStr
                   .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                   .Select(x => Convert.ToInt32(x.Trim()))
                   .ToList();
            }

            //get filterable attributes identifier
            string filterableProductVariantAttributeIdsStr = (pFilterableProductVariantAttributeIds.Value != DBNull.Value) ? (string)pFilterableProductVariantAttributeIds.Value : "";
            if (loadAvailableFilters &&
                !string.IsNullOrWhiteSpace(filterableProductVariantAttributeIdsStr))
            {
                filterableProductVariantAttributeIds = filterableProductVariantAttributeIdsStr
                   .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                   .Select(x => Convert.ToInt32(x.Trim()))
                   .ToList();
            }

            //get filterable manufacturers
            string filterableManufacturerIdsStr = (pFilterableManufacturerIds.Value != DBNull.Value) ? (string)pFilterableManufacturerIds.Value : "";
            if (loadAvailableFilters &&
                !string.IsNullOrWhiteSpace(filterableManufacturerIdsStr))
            {
                filterableManufacturerIds = filterableManufacturerIdsStr
                   .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                   .Select(x => Convert.ToInt32(x.Trim()))
                   .ToList();
            }
            //return products
            int totalRecords = (pTotalRecords.Value != DBNull.Value) ? Convert.ToInt32(pTotalRecords.Value) : 0;
            return new PagedList<Product>(products, pageIndex, pageSize, totalRecords);

            #endregion


        }

        public virtual IPagedList<Product> SearchProductsFilterable(decimal? priceMin, decimal? priceMax, 
            int languageId, ProductSortingEnum orderBy,
         int pageIndex, int pageSize, bool loadAvailableFilters,         
         bool showHidden = false, int fullTextMode = 0, bool useFullTextSearch = true)
        {            
            bool searchLocalizedValue = false;
            searchLocalizedValue = true;

            #region Use stored procedure

            //pass category identifiers as comma-delimited string
            string commaSeparatedCategoryIds = "";
            //if (categoryIds != null)
            //{
            //    for (int i = 0; i < categoryIds.Count; i++)
            //    {
            //        commaSeparatedCategoryIds += categoryIds[i].ToString();
            //        if (i != categoryIds.Count - 1)
            //        {
            //            commaSeparatedCategoryIds += ",";
            //        }
            //    }
            //}

            //pass specification identifiers as comma-delimited string
            string commaSeparatedSpecIds = "";
            //if (filteredSpecs != null)
            //{
            //    ((List<int>)filteredSpecs).Sort();
            //    for (int i = 0; i < filteredSpecs.Count; i++)
            //    {
            //        commaSeparatedSpecIds += filteredSpecs[i].ToString();
            //        if (i != filteredSpecs.Count - 1)
            //        {
            //            commaSeparatedSpecIds += ",";
            //        }
            //    }
            //}

            //pass productvariant attribute identifiers as comma-delimited string
            string commaSeparatedProductVariantAttributeIds = "";
            //if (filteredProductVariantAttributes != null)
            //{
            //    ((List<int>)filteredProductVariantAttributes).Sort();
            //    for (int i = 0; i < filteredProductVariantAttributes.Count; i++)
            //    {
            //        commaSeparatedProductVariantAttributeIds += filteredProductVariantAttributes[i].ToString();
            //        if (i != filteredProductVariantAttributes.Count - 1)
            //        {
            //            commaSeparatedProductVariantAttributeIds += ",";
            //        }
            //    }
            //}

            //pass specification identifiers as comma-delimited string
            string commaSeparatedManufacturerIds = "";
            //if (filteredManufacturers != null)
            //{
            //    ((List<int>)filteredManufacturers).Sort();
            //    for (int i = 0; i < filteredManufacturers.Count; i++)
            //    {
            //        commaSeparatedManufacturerIds += filteredManufacturers[i].ToString();
            //        if (i != filteredManufacturers.Count - 1)
            //        {
            //            commaSeparatedManufacturerIds += ",";
            //        }
            //    }
            //}

            //some databases don't support int.MaxValue
            if (pageSize == int.MaxValue)
                pageSize = int.MaxValue - 1;

            //prepare parameters
            var pCategoryIds = new SqlParameter();
            pCategoryIds.ParameterName = "CategoryIds";
            pCategoryIds.Value = commaSeparatedCategoryIds != null ? (object)commaSeparatedCategoryIds : DBNull.Value;
            pCategoryIds.DbType = DbType.String;

            var pManufacturerId = new SqlParameter();
            pManufacturerId.ParameterName = "ManufacturerId";
            pManufacturerId.Value = 0;
            pManufacturerId.DbType = DbType.Int32;

            var pProductTagId = new SqlParameter();
            pProductTagId.ParameterName = "ProductTagId";
            pProductTagId.Value = 0;
            pProductTagId.DbType = DbType.Int32;

            var pFeaturedProducts = new SqlParameter();
            pFeaturedProducts.ParameterName = "FeaturedProducts";
            pFeaturedProducts.Value = DBNull.Value;
            pFeaturedProducts.DbType = DbType.Boolean;

            var pPriceMin = new SqlParameter();
            pPriceMin.ParameterName = "PriceMin";
            pPriceMin.Value = priceMin.HasValue ? (object)priceMin.Value : DBNull.Value;
            pPriceMin.DbType = DbType.Decimal;

            var pPriceMax = new SqlParameter();
            pPriceMax.ParameterName = "PriceMax";
            pPriceMax.Value = priceMax.HasValue ? (object)priceMax.Value : DBNull.Value;
            pPriceMax.DbType = DbType.Decimal;

            var pKeywords = new SqlParameter();
            pKeywords.ParameterName = "Keywords";
            pKeywords.Value = DBNull.Value;
            pKeywords.DbType = DbType.String;

            var pSearchDescriptions = new SqlParameter();
            pSearchDescriptions.ParameterName = "SearchDescriptions";
            pSearchDescriptions.Value = false;
            pSearchDescriptions.DbType = DbType.Boolean;

            var pSearchProductTags = new SqlParameter();
            pSearchProductTags.ParameterName = "SearchProductTags";
            pSearchProductTags.Value = false;
            pSearchProductTags.DbType = DbType.Boolean;

            var pUseFullTextSearch = new SqlParameter();
            pUseFullTextSearch.ParameterName = "UseFullTextSearch";
            pUseFullTextSearch.Value = useFullTextSearch;
            pUseFullTextSearch.DbType = DbType.Boolean;

            var pFullTextMode = new SqlParameter();
            pFullTextMode.ParameterName = "FullTextMode";
            pFullTextMode.Value = fullTextMode;
            pFullTextMode.DbType = DbType.Int32;

            var pFilteredSpecs = new SqlParameter();
            pFilteredSpecs.ParameterName = "FilteredSpecs";
            pFilteredSpecs.Value = commaSeparatedSpecIds != null ? (object)commaSeparatedSpecIds : DBNull.Value;
            pFilteredSpecs.DbType = DbType.String;

            var pFilteredProductVariantAttributes = new SqlParameter();
            pFilteredProductVariantAttributes.ParameterName = "FilteredProductVariantAttributes";
            pFilteredProductVariantAttributes.Value = commaSeparatedProductVariantAttributeIds != null ? (object)commaSeparatedProductVariantAttributeIds : DBNull.Value;
            pFilteredProductVariantAttributes.DbType = DbType.String;

            var pFilteredManufacturers = new SqlParameter();
            pFilteredManufacturers.ParameterName = "FilteredManufacturers";
            pFilteredManufacturers.Value = commaSeparatedManufacturerIds != null ? (object)commaSeparatedManufacturerIds : DBNull.Value;
            pFilteredManufacturers.DbType = DbType.String;

            var pLanguageId = new SqlParameter();
            pLanguageId.ParameterName = "LanguageId";
            pLanguageId.Value = searchLocalizedValue ? languageId : 0;
            pLanguageId.DbType = DbType.Int32;

            var pOrderBy = new SqlParameter();
            pOrderBy.ParameterName = "OrderBy";
            pOrderBy.Value = (int)orderBy;
            pOrderBy.DbType = DbType.Int32;

            var pPageIndex = new SqlParameter();
            pPageIndex.ParameterName = "PageIndex";
            pPageIndex.Value = pageIndex;
            pPageIndex.DbType = DbType.Int32;

            var pPageSize = new SqlParameter();
            pPageSize.ParameterName = "PageSize";
            pPageSize.Value = pageSize;
            pPageSize.DbType = DbType.Int32;

            var pShowHidden = new SqlParameter();
            pShowHidden.ParameterName = "ShowHidden";
            pShowHidden.Value = showHidden;
            pShowHidden.DbType = DbType.Boolean;

            var pLoadAvailableFilters = new SqlParameter();
            pLoadAvailableFilters.ParameterName = "LoadAvailableFilters";
            pLoadAvailableFilters.Value = false;
            pLoadAvailableFilters.DbType = DbType.Boolean;

            var pFilterableSpecificationAttributeOptionIds = new SqlParameter();
            pFilterableSpecificationAttributeOptionIds.ParameterName = "FilterableSpecificationAttributeOptionIds";
            pFilterableSpecificationAttributeOptionIds.Direction = ParameterDirection.Output;
            pFilterableSpecificationAttributeOptionIds.Size = int.MaxValue - 1;
            pFilterableSpecificationAttributeOptionIds.DbType = DbType.String;

            var pFilterableProductVariantAttributeIds = new SqlParameter();
            pFilterableProductVariantAttributeIds.ParameterName = "FilterableProductVariantAttributeIds";
            pFilterableProductVariantAttributeIds.Direction = ParameterDirection.Output;
            pFilterableProductVariantAttributeIds.Size = int.MaxValue - 1;
            pFilterableProductVariantAttributeIds.DbType = DbType.String;

            var pFilterableManufacturerIds = new SqlParameter();
            pFilterableManufacturerIds.ParameterName = "FilterableManufacturerIds";
            pFilterableManufacturerIds.Direction = ParameterDirection.Output;
            pFilterableManufacturerIds.Size = int.MaxValue - 1;
            pFilterableManufacturerIds.DbType = DbType.String;

            var pTotalRecords = new SqlParameter();
            pTotalRecords.ParameterName = "TotalRecords";
            pTotalRecords.Direction = ParameterDirection.Output;
            pTotalRecords.DbType = DbType.Int32;

            //invoke stored procedure
            var products = _dbContext.ExecuteStoredProcedureList<Product>(
                "ProductLoadAllPagedFilterableP",
                pCategoryIds,
                pManufacturerId,
                pProductTagId,
                pFeaturedProducts,
                pPriceMin,
                pPriceMax,
                pKeywords,
                pSearchDescriptions,
                pSearchProductTags,
                pUseFullTextSearch,
                pFullTextMode,
                pFilteredSpecs,
                pFilteredProductVariantAttributes,
                pFilteredManufacturers,
                pLanguageId,
                pOrderBy,
                pPageIndex,
                pPageSize,
                pShowHidden,
                pLoadAvailableFilters,
                pFilterableSpecificationAttributeOptionIds,
                pFilterableProductVariantAttributeIds,
                pFilterableManufacturerIds,
                pTotalRecords);
            
            //return products
            int totalRecords = (pTotalRecords.Value != DBNull.Value) ? Convert.ToInt32(pTotalRecords.Value) : 0;
            return new PagedList<Product>(products, pageIndex, pageSize, totalRecords);

            #endregion


        }


        public virtual IPagedList<Product> SearchProductsFilterable(IList<int> categoryIds,
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
       bool showHidden = false, int fullTextMode = 0, bool useFullTextSearch = true)
        {
            filterableSpecificationAttributeOptionIds = new List<int>();
            filterableProductVariantAttributeIds = new List<int>();
            filterableManufacturerIds = new List<int>();
            filterableCategoryIds = new List<int>();
            bool searchLocalizedValue = false;
            if (languageId > 0)
            {
                //if (showHidden)
                //{
                //    searchLocalizedValue = true;
                //}
                //else
                //{
                //    //ensure that we have at least two published languages
                //    var totalPublishedLanguages = _languageService.GetAllLanguages(false).Count;
                //    searchLocalizedValue = totalPublishedLanguages >= 2;
                //}

                searchLocalizedValue = true;
            }


            #region Use stored procedure

            //pass category identifiers as comma-delimited string
            string commaSeparatedCategoryIds = "";
            if (categoryIds != null)
            {
                for (int i = 0; i < categoryIds.Count; i++)
                {
                    commaSeparatedCategoryIds += categoryIds[i].ToString();
                    if (i != categoryIds.Count - 1)
                    {
                        commaSeparatedCategoryIds += ",";
                    }
                }
            }

            //pass specification identifiers as comma-delimited string
            string commaSeparatedSpecIds = "";
            if (filteredSpecs != null)
            {
                ((List<int>)filteredSpecs).Sort();
                for (int i = 0; i < filteredSpecs.Count; i++)
                {
                    commaSeparatedSpecIds += filteredSpecs[i].ToString();
                    if (i != filteredSpecs.Count - 1)
                    {
                        commaSeparatedSpecIds += ",";
                    }
                }
            }

            //pass productvariant attribute identifiers as comma-delimited string
            string commaSeparatedProductVariantAttributeIds = "";
            if (filteredProductVariantAttributes != null)
            {
                ((List<int>)filteredProductVariantAttributes).Sort();
                for (int i = 0; i < filteredProductVariantAttributes.Count; i++)
                {
                    commaSeparatedProductVariantAttributeIds += filteredProductVariantAttributes[i].ToString();
                    if (i != filteredProductVariantAttributes.Count - 1)
                    {
                        commaSeparatedProductVariantAttributeIds += ",";
                    }
                }
            }

            //pass specification identifiers as comma-delimited string
            string commaSeparatedManufacturerIds = "";
            if (filteredManufacturers != null)
            {
                ((List<int>)filteredManufacturers).Sort();
                for (int i = 0; i < filteredManufacturers.Count; i++)
                {
                    commaSeparatedManufacturerIds += filteredManufacturers[i].ToString();
                    if (i != filteredManufacturers.Count - 1)
                    {
                        commaSeparatedManufacturerIds += ",";
                    }
                }
            }

            //some databases don't support int.MaxValue
            if (pageSize == int.MaxValue)
                pageSize = int.MaxValue - 1;

            //prepare parameters
            var pCategoryIds = new SqlParameter();
            pCategoryIds.ParameterName = "CategoryIds";
            pCategoryIds.Value = commaSeparatedCategoryIds != null ? (object)commaSeparatedCategoryIds : DBNull.Value;
            pCategoryIds.DbType = DbType.String;

            var pManufacturerId = new SqlParameter();
            pManufacturerId.ParameterName = "ManufacturerId";
            pManufacturerId.Value = manufacturerId;
            pManufacturerId.DbType = DbType.Int32;

            var pProductTagId = new SqlParameter();
            pProductTagId.ParameterName = "ProductTagId";
            pProductTagId.Value = productTagId;
            pProductTagId.DbType = DbType.Int32;

            var pFeaturedProducts = new SqlParameter();
            pFeaturedProducts.ParameterName = "FeaturedProducts";
            pFeaturedProducts.Value = featuredProducts.HasValue ? (object)featuredProducts.Value : DBNull.Value;
            pFeaturedProducts.DbType = DbType.Boolean;

            var pPriceMin = new SqlParameter();
            pPriceMin.ParameterName = "PriceMin";
            pPriceMin.Value = priceMin.HasValue ? (object)priceMin.Value : DBNull.Value;
            pPriceMin.DbType = DbType.Decimal;

            var pPriceMax = new SqlParameter();
            pPriceMax.ParameterName = "PriceMax";
            pPriceMax.Value = priceMax.HasValue ? (object)priceMax.Value : DBNull.Value;
            pPriceMax.DbType = DbType.Decimal;

            var pKeywords = new SqlParameter();
            pKeywords.ParameterName = "Keywords";
            pKeywords.Value = keywords != null ? (object)keywords : DBNull.Value;
            pKeywords.DbType = DbType.String;

            var pSearchDescriptions = new SqlParameter();
            pSearchDescriptions.ParameterName = "SearchDescriptions";
            pSearchDescriptions.Value = searchDescriptions;
            pSearchDescriptions.DbType = DbType.Boolean;

            var pSearchProductTags = new SqlParameter();
            pSearchProductTags.ParameterName = "SearchProductTags";
            pSearchProductTags.Value = searchProductTags;
            pSearchProductTags.DbType = DbType.Boolean;

            var pUseFullTextSearch = new SqlParameter();
            pUseFullTextSearch.ParameterName = "UseFullTextSearch";
            pUseFullTextSearch.Value = useFullTextSearch;
            pUseFullTextSearch.DbType = DbType.Boolean;

            var pFullTextMode = new SqlParameter();
            pFullTextMode.ParameterName = "FullTextMode";
            pFullTextMode.Value = fullTextMode;
            pFullTextMode.DbType = DbType.Int32;

            var pFilteredSpecs = new SqlParameter();
            pFilteredSpecs.ParameterName = "FilteredSpecs";
            pFilteredSpecs.Value = commaSeparatedSpecIds != null ? (object)commaSeparatedSpecIds : DBNull.Value;
            pFilteredSpecs.DbType = DbType.String;

            var pFilteredProductVariantAttributes = new SqlParameter();
            pFilteredProductVariantAttributes.ParameterName = "FilteredProductVariantAttributes";
            pFilteredProductVariantAttributes.Value = commaSeparatedProductVariantAttributeIds != null ? (object)commaSeparatedProductVariantAttributeIds : DBNull.Value;
            pFilteredProductVariantAttributes.DbType = DbType.String;

            var pFilteredManufacturers = new SqlParameter();
            pFilteredManufacturers.ParameterName = "FilteredManufacturers";
            pFilteredManufacturers.Value = commaSeparatedManufacturerIds != null ? (object)commaSeparatedManufacturerIds : DBNull.Value;
            pFilteredManufacturers.DbType = DbType.String;

            var pLanguageId = new SqlParameter();
            pLanguageId.ParameterName = "LanguageId";
            pLanguageId.Value = searchLocalizedValue ? languageId : 0;
            pLanguageId.DbType = DbType.Int32;

            var pOrderBy = new SqlParameter();
            pOrderBy.ParameterName = "OrderBy";
            pOrderBy.Value = (int)orderBy;
            pOrderBy.DbType = DbType.Int32;

            var pPageIndex = new SqlParameter();
            pPageIndex.ParameterName = "PageIndex";
            pPageIndex.Value = pageIndex;
            pPageIndex.DbType = DbType.Int32;

            var pPageSize = new SqlParameter();
            pPageSize.ParameterName = "PageSize";
            pPageSize.Value = pageSize;
            pPageSize.DbType = DbType.Int32;

            var pShowHidden = new SqlParameter();
            pShowHidden.ParameterName = "ShowHidden";
            pShowHidden.Value = showHidden;
            pShowHidden.DbType = DbType.Boolean;

            var pLoadAvailableFilters = new SqlParameter();
            pLoadAvailableFilters.ParameterName = "LoadAvailableFilters";
            pLoadAvailableFilters.Value = loadAvailableFilters;
            pLoadAvailableFilters.DbType = DbType.Boolean;

            var pFilterableSpecificationAttributeOptionIds = new SqlParameter();
            pFilterableSpecificationAttributeOptionIds.ParameterName = "FilterableSpecificationAttributeOptionIds";
            pFilterableSpecificationAttributeOptionIds.Direction = ParameterDirection.Output;
            pFilterableSpecificationAttributeOptionIds.Size = int.MaxValue - 1;
            pFilterableSpecificationAttributeOptionIds.DbType = DbType.String;

            var pFilterableProductVariantAttributeIds = new SqlParameter();
            pFilterableProductVariantAttributeIds.ParameterName = "FilterableProductVariantAttributeIds";
            pFilterableProductVariantAttributeIds.Direction = ParameterDirection.Output;
            pFilterableProductVariantAttributeIds.Size = int.MaxValue - 1;
            pFilterableProductVariantAttributeIds.DbType = DbType.String;

            var pFilterableManufacturerIds = new SqlParameter();
            pFilterableManufacturerIds.ParameterName = "FilterableManufacturerIds";
            pFilterableManufacturerIds.Direction = ParameterDirection.Output;
            pFilterableManufacturerIds.Size = int.MaxValue - 1;
            pFilterableManufacturerIds.DbType = DbType.String;

            var pFilterableCategoryIds = new SqlParameter();
            pFilterableCategoryIds.ParameterName = "FilterableCategoryIds";
            pFilterableCategoryIds.Direction = ParameterDirection.Output;
            pFilterableCategoryIds.Size = int.MaxValue - 1;
            pFilterableCategoryIds.DbType = DbType.String;

            var pMaxPrice = new SqlParameter();
            pMaxPrice.ParameterName = "MaxPrice";
            pMaxPrice.Direction = ParameterDirection.Output;
            pMaxPrice.DbType = DbType.Decimal;


            var pTotalRecords = new SqlParameter();
            pTotalRecords.ParameterName = "TotalRecords";
            pTotalRecords.Direction = ParameterDirection.Output;
            pTotalRecords.DbType = DbType.Int32;

            //invoke stored procedure
            var products = _dbContext.ExecuteStoredProcedureList<Product>(
                "ProductLoadAllPagedFilterableP",
                pCategoryIds,
                pManufacturerId,
                pProductTagId,
                pFeaturedProducts,
                pPriceMin,
                pPriceMax,
                pKeywords,
                pSearchDescriptions,
                pSearchProductTags,
                pUseFullTextSearch,
                pFullTextMode,
                pFilteredSpecs,
                pFilteredProductVariantAttributes,
                pFilteredManufacturers,
                pLanguageId,
                pOrderBy,
                pPageIndex,
                pPageSize,
                pShowHidden,
                pLoadAvailableFilters,
                pFilterableSpecificationAttributeOptionIds,
                pFilterableProductVariantAttributeIds,
                pFilterableManufacturerIds,
                pFilterableCategoryIds,
                pMaxPrice,
                pTotalRecords);
            //get filterable specification attribute option identifier
            string filterableSpecificationAttributeOptionIdsStr = (pFilterableSpecificationAttributeOptionIds.Value != DBNull.Value) ? (string)pFilterableSpecificationAttributeOptionIds.Value : "";
            if (loadAvailableFilters &&
                !string.IsNullOrWhiteSpace(filterableSpecificationAttributeOptionIdsStr))
            {
                filterableSpecificationAttributeOptionIds = filterableSpecificationAttributeOptionIdsStr
                   .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                   .Select(x => Convert.ToInt32(x.Trim()))
                   .ToList();
            }

            //get filterable attributes identifier
            string filterableProductVariantAttributeIdsStr = (pFilterableProductVariantAttributeIds.Value != DBNull.Value) ? (string)pFilterableProductVariantAttributeIds.Value : "";
            if (loadAvailableFilters &&
                !string.IsNullOrWhiteSpace(filterableProductVariantAttributeIdsStr))
            {
                filterableProductVariantAttributeIds = filterableProductVariantAttributeIdsStr
                   .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                   .Select(x => Convert.ToInt32(x.Trim()))
                   .ToList();
            }

            //get filterable manufacturers
            string filterableManufacturerIdsStr = (pFilterableManufacturerIds.Value != DBNull.Value) ? (string)pFilterableManufacturerIds.Value : "";
            if (loadAvailableFilters &&
                !string.IsNullOrWhiteSpace(filterableManufacturerIdsStr))
            {
                filterableManufacturerIds = filterableManufacturerIdsStr
                   .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                   .Select(x => Convert.ToInt32(x.Trim()))
                   .ToList();
            }

            //get filterable categories
            string filterableCategoryIdsStr = (pFilterableCategoryIds.Value != DBNull.Value) ? (string)pFilterableCategoryIds.Value : "";
            if (loadAvailableFilters &&
                !string.IsNullOrWhiteSpace(filterableCategoryIdsStr))
            {
                filterableCategoryIds = filterableCategoryIdsStr
                   .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                   .Select(x => Convert.ToInt32(x.Trim()))
                   .ToList();
            }

            maxPrice = (pMaxPrice.Value != DBNull.Value) ? (decimal?)pMaxPrice.Value : (decimal?)null;

            //return products
            int totalRecords = (pTotalRecords.Value != DBNull.Value) ? Convert.ToInt32(pTotalRecords.Value) : 0;
            return new PagedList<Product>(products, pageIndex, pageSize, totalRecords);

            #endregion


        }

        //public virtual SpecificationAttribute GetSpecificationAttributeById(int specificationAttributeId)
        //{
        //    if (specificationAttributeId == 0)
        //        return null;

        //    string key = string.Format(SPECIFICATIONATTRIBUTE_BY_ID_KEY, specificationAttributeId);
        //    return _cacheManager.Get(key, () =>
        //    {
        //        var sa = _specificationAttributeRepository.GetById(specificationAttributeId);
        //        return sa;
        //    });
        //}

        /// <summary>
        /// Adjusts inventory
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        /// <param name="decrease">A value indicating whether to increase or descrease product variant stock quantity</param>
        /// <param name="quantity">Quantity</param>
        /// <param name="attributesXml">Attributes in XML format</param>
        public virtual void AdjustInventory(ProductVariant productVariant, bool decrease,
            int quantity, string attributesXml, bool clearCache = false)
        {
            if (productVariant == null)
                throw new ArgumentNullException("productVariant");

            var prevStockQuantity = productVariant.StockQuantity;

            switch (productVariant.ManageInventoryMethod)
            {
                case ManageInventoryMethod.DontManageStock:
                    {
                        //do nothing
                        return;
                    }
                case ManageInventoryMethod.ManageStock:
                    {
                        int newStockQuantity = 0;
                        if (decrease)
                            newStockQuantity = productVariant.StockQuantity - quantity;
                        else
                            newStockQuantity = productVariant.StockQuantity + quantity;

                        bool newPublished = productVariant.Published;
                        bool newDisableBuyButton = productVariant.DisableBuyButton;
                        bool newDisableWishlistButton = productVariant.DisableWishlistButton;

                        //check if minimum quantity is reached
                        if (decrease)
                        {
                            if (productVariant.MinStockQuantity >= newStockQuantity)
                            {
                                switch (productVariant.LowStockActivity)
                                {
                                    case LowStockActivity.DisableBuyButton:
                                        newDisableBuyButton = true;
                                        newDisableWishlistButton = true;
                                        break;
                                    case LowStockActivity.Unpublish:
                                        newPublished = false;
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }

                        productVariant.StockQuantity = newStockQuantity;
                        productVariant.DisableBuyButton = newDisableBuyButton;
                        productVariant.DisableWishlistButton = newDisableWishlistButton;
                        productVariant.Published = newPublished;
                        UpdateProductVariant(productVariant);

                        //send email notification
                        if (decrease && productVariant.NotifyAdminForQuantityBelow > newStockQuantity)
                            _workflowMessageService.SendQuantityBelowStoreOwnerNotification(productVariant, _localizationSettings.DefaultAdminLanguageId);

                        if (decrease)
                        {
                            var product = productVariant.Product;
                            bool allProductVariantsUnpublished = true;
                            foreach (var pv2 in GetProductVariantsByProductId(product.Id))
                            {
                                if (pv2.Published)
                                {
                                    allProductVariantsUnpublished = false;
                                    break;
                                }
                            }

                            if (allProductVariantsUnpublished)
                            {
                                product.Published = false;
                                UpdateProduct(product);
                            }
                        }
                    }
                    break;
                case ManageInventoryMethod.ManageStockByAttributes:
                    {
                        var combination = _productAttributeParser.FindProductVariantAttributeCombination(productVariant, attributesXml);
                        if (combination != null)
                        {
                            int newStockQuantity = 0;
                            if (decrease)
                                newStockQuantity = combination.StockQuantity - quantity;
                            else
                                newStockQuantity = combination.StockQuantity + quantity;

                            combination.StockQuantity = newStockQuantity;
                            _productAttributeService.UpdateProductVariantAttributeCombination(combination);
                        }
                    }
                    break;
                default:
                    break;
            }
            //TODO:AF do it well!
            if (clearCache)
            {
                _cacheManager.RemoveByPattern(string.Format("Nop.productmodel.id-{0}.", productVariant.ProductId));
                _cacheManager.RemoveByPattern(string.Format("Nop.categorymodel.id-{0}.", productVariant.Product.GetDefaultProductCategory().Id));
            }

            //TODO send back in stock notifications?
            //if (productVariant.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
            //    productVariant.BackorderMode == BackorderMode.NoBackorders &&
            //    productVariant.AllowBackInStockSubscriptions &&
            //    productVariant.StockQuantity > 0 &&
            //    prevStockQuantity <= 0 &&
            //    productVariant.Published &&
            //    !productVariant.Deleted)
            //{
            //    //_backInStockSubscriptionService.SendNotificationsToSubscribers(productVariant);
            //}
        }


        /// <summary>
        /// Get product variants by product identifiers
        /// </summary>
        /// <param name="productIds">Product identifiers</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Product variants</returns>
        public IList<ProductVariant> GetProductVariantsByProductIds(int[] productIds, bool showHidden = false)
        {
            if (productIds == null || productIds.Length == 0)
                return new List<ProductVariant>();

            var query = _productVariantRepository.Table;
            if (!showHidden)
            {
                query = query.Where(pv => pv.Published);
            }
            if (!showHidden)
            {
                //The function 'CurrentUtcDateTime' is not supported by SQL Server Compact. 
                //That's why we pass the date value
                var nowUtc = DateTime.UtcNow;
                query = query.Where(pv =>
                        !pv.AvailableStartDateTimeUtc.HasValue ||
                        pv.AvailableStartDateTimeUtc <= nowUtc);
                query = query.Where(pv =>
                        !pv.AvailableEndDateTimeUtc.HasValue ||
                        pv.AvailableEndDateTimeUtc >= nowUtc);
            }
            query = query.Where(pv => !pv.Deleted);
            query = query.Where(pv => productIds.Contains(pv.ProductId));
            query = query.OrderBy(pv => pv.DisplayOrder);

            var productVariants = query.ToList();
            return productVariants;
        }

        public IList<Product> GetProductsByIds(int[] productIds, bool showHidden = false)
        {
            if (productIds == null || productIds.Length == 0)
                return new List<Product>();

            var query = _productRepository.Table;
            if (!showHidden)
            {
                query = query.Where(p => p.Published);
            }

            query = query.Where(p => !p.Deleted);
            query = query.Where(p => productIds.Contains(p.Id));
            //query = query.OrderBy(pv => pv.DisplayOrder);

            var products = query.ToList();
            return products;
        }
        //TODO: recode!!
        public void RequestBulkCatalog()
        {
            var logger = EngineContext.Current.Resolve<ILogger>();
            logger.Information("RequestBulkCatalog Started");
            string storeUrl = EngineContext.Current.Resolve<StoreInformationSettings>().StoreUrl;

            var manufacturerService = EngineContext.Current.Resolve<IManufacturerService>();
            var manufacturers = manufacturerService.GetAllManufacturers();

            var categorySerive = EngineContext.Current.Resolve<ICategoryService>();

            var categories = categorySerive.GetAllCategories();
            var urlMans = manufacturers.Select(x => { return x.GetSeName(); });
            var urlCats = categories.Select(x => { return "c/" + x.Id; }).ToList();
            //string[] urls =urlCat.

            urlCats.AddRange(urlMans);
            urlCats.AddRange(new string[] { "catalog/ManufacturerAll", "newproducts" });
            foreach (var url in urlCats)
            {
                using (var wc = new WebClient())
                {
                    wc.DownloadString(new Uri(storeUrl + url));
                }
                using (var wce = new WebClient())
                {
                    wce.DownloadString(new Uri(storeUrl + "en/" + url));
                }
            }
            logger.Information("RequestBulkCatalog ended.");
        }
        

        public IList<Product> GetSimilarProductsById(int productId, int count, int? categoryId = null)
        {
            var query = _productRepository.Table;
            var product = _productRepository.GetById(productId);
            var specificationOptions = product.ProductSpecificationAttributes.Select(x => x.SpecificationAttributeOption).ToList();
            var specificationAttributes = specificationOptions.Select(x => x.SpecificationAttribute).ToList();

            query = query.Where(p => !p.Deleted);
            query = query.Where(p => p.Published);
            query = query.Where(p => p.Id != productId);

            if (!categoryId.HasValue)
            {
                Category category = product.GetDefaultProductCategory();
                categoryId = category.Id;
            }

            query = from p in query
                    from pc in p.ProductCategories.Where(pc => pc.CategoryId == categoryId.Value)
                    select p;

            var products = query.Distinct().ToList();
            var relatedProducts = new List<Product>();
            var relatedProductsDict = new Dictionary<Product, int>();
           
            if (!(products == null || specificationAttributes == null || products.Count() == 0 || specificationAttributes.Count() == 0))
            {
                foreach (var p in products)
                {
                    int temp = p.ProductVariants.Where(x => x.Deleted == false).Count();

                    if(!(temp > 0))
                        continue;

                    if ( !relatedProductsDict.ContainsKey(p))
                    {
                        relatedProductsDict.Add(p, 0);
                        foreach (var so in specificationOptions)
                        {
                            if (p.ProductSpecificationAttributes.Select(x => x.SpecificationAttributeOption).Contains(so))
                            {
                                relatedProductsDict[p] += 1;
                            }
                        }
                    }
                }
            }

            var sortedList = relatedProductsDict.OrderByDescending(key => key.Value).ToList();
            if (sortedList.Count() > count)
                sortedList = sortedList.Take(count).ToList();

            foreach (var rp in sortedList)
                relatedProducts.Add(rp.Key);

            return relatedProducts;
        }

        #endregion public methods

        #region ProductVariant pictures

        /// <summary>
        /// Deletes a product picture
        /// </summary>
        /// <param name="productPicture">Product variant picture</param>
        public virtual void DeleteProductVariantPicture(ProductVariantPicture productVariantPicture)
        {
            if (productVariantPicture == null)
                throw new ArgumentNullException("productVariantPicture");

            _productVariantPictureRepository.Delete(productVariantPicture);
			_cacheManagerPersistent.Remove(PictureRepository.NOP_CACHE_PRODUCTPICTUREVARIANTS);
		}

        /// <summary>
        /// Gets a product variant pictures by product identifier
        /// </summary>
        /// <param name="productVariantId">The product identifier</param>
        /// <returns>Product variant pictures</returns>
        public virtual IList<ProductVariantPicture> GetProductVariantPicturesByProductVariantId(int productVariantId)
        {
            var query = from pvp in _productVariantPictureRepository.Table
                        where pvp.ProductVariantId == productVariantId
                        orderby pvp.DisplayOrder
                        select pvp;
            var productVariantPictures = query.ToList();
            return productVariantPictures;
        }

        /// <summary>
        /// Gets a product picture
        /// </summary>
        /// <param name="productPictureId">Product variant picture identifer</param>
        /// <returns>Product variant picture</returns>
        public virtual ProductVariantPicture GetProductVariantPictureById(int productVariantPictureId)
        {
            if (productVariantPictureId == 0)
                return null;

            var pvp = _productVariantPictureRepository.GetById(productVariantPictureId);
            return pvp;
        }

        /// <summary>
        /// Inserts a product variant picture
        /// </summary>
        /// <param name="productPicture">Product variant picture</param>
        public virtual void InsertProductVariantPicture(ProductVariantPicture productVariantPicture)
        {
            if (productVariantPicture == null)
                throw new ArgumentNullException("productVariantPicture");

            _productVariantPictureRepository.Insert(productVariantPicture);
			_cacheManagerPersistent.Remove(PictureRepository.NOP_CACHE_PRODUCTPICTUREVARIANTS);
        }

        /// <summary>
        /// Updates a product variant picture
        /// </summary>
        /// <param name="productPicture">Product variant picture</param>
        public virtual void UpdateProductVariantPicture(ProductVariantPicture productVariantPicture)
        {
            if (productVariantPicture == null)
                throw new ArgumentNullException("productVariantPicture");

            _productVariantPictureRepository.Update(productVariantPicture);
			_cacheManagerPersistent.Remove(PictureRepository.NOP_CACHE_PRODUCTPICTUREVARIANTS);
		}

        #endregion public methods

        #region private methods
        //private IEnumerable<int> To1Dimension(IEnumerable<IEnumerable<int>> values)
        //{
        //    List<int> list = new List<int>();

        //    foreach (var item in values)
        //    {
        //        foreach (var inneritem in item)
        //        {
        //            list.Add(inneritem);
        //        }
        //    }
        //    return list;
        //}

        #endregion private methods
    }
}
