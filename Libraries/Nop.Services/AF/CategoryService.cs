using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.News;
using Nop.Core.Events;
using Nop.Core.Domain.Localization;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Category service
    /// </summary>
    public partial class CategoryService
    {
        #region Constants
        private const string NEWSITEMCATEGORIES_ALLBYNEWSITEMID_KEY = "Nop.newsitemcategory.allbynewsitemid-{0}-{1}";
        private const string NEWSITEMCATEGORIES_BY_ID_KEY = "Nop.newsitemcategory.id-{0}";
        private const string NEWSITEMCATEGORIES_PATTERN_KEY = "Nop.newsitemcategory.";
        private const string CATEGORYNEWSITEM_BY_ID_KEY = "Nop.newsitem.id-{0}";
        private const string CATEGORY_BY_SENAME = "Nop.category.sename-{0}";
        #endregion

        #region Fields
        private readonly IRepository<NewsItemCategory> _newsItemCategoryRepository;
        private readonly IRepository<NewsItem> _newsItemRepository;
        private readonly IRepository<LocalizedProperty> _localizedPropertyRepository;
        #endregion
        #region Methods

        public CategoryService(ICacheManager cacheManager,
            IRepository<Category> categoryRepository,
            IRepository<ProductCategory> productCategoryRepository,
            IRepository<Product> productRepository,
            IEventPublisher eventPublisher,
            IRepository<NewsItemCategory> newsItemCategoryRepository,
            IRepository<NewsItem> newsItemRepository,
            IRepository<LocalizedProperty> localizedPropertyRepository)
        {
            this._cacheManager = cacheManager;
            this._categoryRepository = categoryRepository;
            this._productCategoryRepository = productCategoryRepository;
            this._productRepository = productRepository;
            _eventPublisher = eventPublisher;
            this._newsItemCategoryRepository = newsItemCategoryRepository;
            this._newsItemRepository = newsItemRepository;
            this._localizedPropertyRepository = localizedPropertyRepository;
        }

        public virtual Category GetCategoryBySeName(string SeName)
        {
            if (string.IsNullOrWhiteSpace(SeName))
                return null;

            string key = string.Format(CATEGORY_BY_SENAME, SeName);
            return _cacheManager.Get(key, () =>
            {
                var query = from lp in _localizedPropertyRepository.Table
                            join c in _categoryRepository.Table on lp.EntityId equals c.Id
                            where
                            lp.LocaleKeyGroup == "Category" &&
                            lp.LocaleKey == "SeName" &&
                            lp.LocaleValue == SeName
                            select c;
                var category = query.FirstOrDefault();
                return category;
            });
        }
        
        
        public virtual IList<Product> GetRandomProductsByCategoryId(int categoryId, int count ,int productId)
        {
            if (categoryId == 0)
                return new List<Product>();

            string key = string.Format(PRODUCTCATEGORIES_ALLBYCATEGORYID_KEY, false, categoryId);
            //return _cacheManager.Get(key, () =>
            //{
                var query = (from pc in _productCategoryRepository.Table
                             join p in _productRepository.Table on pc.ProductId equals p.Id
                             where pc.CategoryId == categoryId &&
                                  !p.Deleted && pc.ProductId != productId &&
                             (false || p.Published)
                             orderby pc.DisplayOrder
                             select pc.Product).OrderBy(x => Guid.NewGuid()).Take(count).ToList();
                var productCategories = query.ToList();
                if (productCategories.Count() < count)
                {
                    var categoryProduct = GetProductCategoriesByProductId(productId).FirstOrDefault();

                    if (categoryProduct != null)
                    {
                        var category = GetCategoryById(categoryProduct.CategoryId);
                        if (category != null)
                        {
                            var itemQuery = from cr in _categoryRepository.Table
                                            where cr.ParentCategoryId == category.ParentCategoryId
                                            select cr.Id;   
                            query = (from pc in _productCategoryRepository.Table
                                         join p in _productRepository.Table on pc.ProductId equals p.Id
                                     where itemQuery.Contains(pc.CategoryId) &&
                                               !p.Deleted && pc.ProductId != productId &&
                                               (false || p.Published)
                                         orderby pc.DisplayOrder
                                         select pc.Product).OrderBy(x => Guid.NewGuid()).Take(count).ToList();
                            productCategories = query.ToList();
                        }
                    }
                }

                return productCategories;
            //});
        }
        //public virtual IList<Product> GetAllProductsByCategoryId(int categoryId, int productId)
        //{
        //    if (categoryId == 0)
        //        return new List<Product>();

        //    string key = string.Format(PRODUCTCATEGORIES_ALLBYCATEGORYID_KEY, false, categoryId);
        //    //return _cacheManager.Get(key, () =>
        //    //{
        //    var query = (from pc in _productCategoryRepository.Table
        //                 join p in _productRepository.Table on pc.ProductId equals p.Id
        //                 where pc.CategoryId == categoryId &&
        //                      !p.Deleted && pc.ProductId != productId &&
        //                 (false || p.Published)
        //                 orderby pc.DisplayOrder
        //                 select pc.Product).OrderBy(x => Guid.NewGuid()).ToList();
        //    var productCategories = query.ToList();
        //    //if (productCategories.Count() < count)
        //    //{
        //    //    var categoryProduct = GetProductCategoriesByProductId(productId).FirstOrDefault();

        //    //    if (categoryProduct != null)
        //    //    {
        //    //        var category = GetCategoryById(categoryProduct.CategoryId);
        //    //        if (category != null)
        //    //        {
        //    //            var itemQuery = from cr in _categoryRepository.Table
        //    //                            where cr.ParentCategoryId == category.ParentCategoryId
        //    //                            select cr.Id;
        //    //            query = (from pc in _productCategoryRepository.Table
        //    //                     join p in _productRepository.Table on pc.ProductId equals p.Id
        //    //                     where itemQuery.Contains(pc.CategoryId) &&
        //    //                               !p.Deleted && pc.ProductId != productId &&
        //    //                               (false || p.Published)
        //    //                     orderby pc.DisplayOrder
        //    //                     select pc.Product).OrderBy(x => Guid.NewGuid()).Take(count).ToList();
        //    //            productCategories = query.ToList();
        //    //        }
        //    //    }
        //    //}

        //    return productCategories;
        //    //});
        //}
        
        public virtual IList<NewsItemCategory> GetNewsItemCategoriesByNewsItemId(int newsItemId, bool showHidden = false)
        {
            if (newsItemId == 0)
                return new List<NewsItemCategory>();
          
            string key = string.Format(NEWSITEMCATEGORIES_ALLBYNEWSITEMID_KEY, showHidden, newsItemId);
            return _cacheManager.Get(key, () =>
            {
                var query = from pc in _newsItemCategoryRepository.Table
                            where pc.NewsItemId == newsItemId
                            orderby pc.CreatedOnUtc
                            select pc;
                var newsItemCategories = query.ToList();

                return newsItemCategories;
            });
        }

        public virtual void InsertNewsItemCategory(NewsItemCategory newsItemCategory)
        {
            if (newsItemCategory == null)
                throw new ArgumentNullException("newsItemCategory");

            _newsItemCategoryRepository.Insert(newsItemCategory);

            //cache
            _cacheManager.RemoveByPattern(CATEGORIES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(NEWSITEMCATEGORIES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(newsItemCategory);
        }

        public virtual NewsItemCategory GetNewsItemCategoryById(int newsItemCategoryId)
        {
            if (newsItemCategoryId == 0)
                return null;

            string key = string.Format(NEWSITEMCATEGORIES_BY_ID_KEY, newsItemCategoryId);
            return _cacheManager.Get(key, () =>
            {
                return _newsItemCategoryRepository.GetById(newsItemCategoryId);
            });
        }
        public virtual NewsItemCategory GetNewsItemCategoryByCategoryId(int categoryId)
        {
            var query = (from pc in _newsItemCategoryRepository.Table
                         orderby pc.CreatedOnUtc descending
                         where pc.CategoryId == categoryId
                         select pc).FirstOrDefault();
            return query;
        }

        public virtual void UpdateNewsItemCategory(NewsItemCategory newsItemCategory)
        {
            if (newsItemCategory == null)
                throw new ArgumentNullException("newsItemCategory");

            _newsItemCategoryRepository.Update(newsItemCategory);

            //cache
            _cacheManager.RemoveByPattern(CATEGORIES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(NEWSITEMCATEGORIES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(newsItemCategory);
        }
        public virtual void DeleteNewsItemCategory(NewsItemCategory newsItemCategory)
        {
            if (newsItemCategory == null)
                throw new ArgumentNullException("newsItemCategory");

            _newsItemCategoryRepository.Delete(newsItemCategory);

            //cache
            _cacheManager.RemoveByPattern(CATEGORIES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(NEWSITEMCATEGORIES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(newsItemCategory);
        }
        public virtual NewsItem GetCategoryNewsItemByCategoryId(int categoryId, int languageId)
        {

            if (categoryId == 0)
                return null;

            string key = string.Format(CATEGORYNEWSITEM_BY_ID_KEY, categoryId);
            return _cacheManager.Get(key, () =>
            {
                 var query = (from pc in _newsItemRepository.Table
                            join c in _newsItemCategoryRepository.Table on pc.Id equals c.NewsItemId
                            where c.CategoryId == categoryId && c.NewsItem.LanguageId == languageId && c.NewsItem.Published == true
                             orderby c.CreatedOnUtc descending 
                            select pc ).FirstOrDefault();
                return query;
            });

        }
        public virtual IPagedList<NewsItem> GetCategoryNewsItemsByCategoryId(NewsType? newsType, int categoryId, int languageId, int pageIndex, int pageSize)
        {

            if (categoryId == 0)
                return null;

            int? type = null;
            if (newsType.HasValue)
                type = (int)newsType.Value;

            string key = string.Format(CATEGORYNEWSITEM_BY_ID_KEY, categoryId);
            return _cacheManager.Get(key, () =>
            {
                var query = (from pc in _newsItemRepository.Table
                             join c in _newsItemCategoryRepository.Table on pc.Id equals c.NewsItemId
                             where c.CategoryId == categoryId && 
                             c.NewsItem.LanguageId == languageId && 
                             c.NewsItem.Published == true &&
                             (!type.HasValue || pc.SystemTypeId == (int)type.Value)
                             orderby c.CreatedOnUtc descending
                             select pc);
                var news = new PagedList<NewsItem>(query, pageIndex, pageSize);
                return news;
            });

        }

        public virtual IPagedList<NewsItem> GetCategoryNewsItemsByCategoryIdDefault(NewsType? newsType, int categoryId, int languageId,int pageIndex, int pageSize)
        {
            if (categoryId == 0)
            {

                int? type = null;
                if (newsType.HasValue)
                    type = (int)newsType.Value;

                string key = string.Format(CATEGORYNEWSITEM_BY_ID_KEY, categoryId);
                return _cacheManager.Get(key, () =>
                {
                    var query = (from pc in _newsItemRepository.Table
                                 join c in _newsItemCategoryRepository.Table on pc.Id equals c.NewsItemId
                                 where
                                 c.NewsItem.LanguageId == languageId &&
                                 c.NewsItem.Published == true &&
                                 (!type.HasValue || pc.SystemTypeId == (int)type.Value)
                                 orderby c.CreatedOnUtc descending
                                 select pc);
                    var news = new PagedList<NewsItem>(query, pageIndex, pageSize);
                    return news;
                });
            }

            else
            {
                int? type = null;
                if (newsType.HasValue)
                    type = (int)newsType.Value;

                string key = string.Format(CATEGORYNEWSITEM_BY_ID_KEY, categoryId);
                return _cacheManager.Get(key, () =>
                {
                    var query = (from pc in _newsItemRepository.Table
                                 join c in _newsItemCategoryRepository.Table on pc.Id equals c.NewsItemId
                                 where c.CategoryId == categoryId &&
                                 c.NewsItem.LanguageId == languageId &&
                                 c.NewsItem.Published == true &&
                                 (!type.HasValue || pc.SystemTypeId == (int)type.Value)
                                 orderby c.CreatedOnUtc descending
                                 select pc);
                    var news = new PagedList<NewsItem>(query, pageIndex, pageSize);
                    return news;
                });
            }

        }

        public virtual IList<Product> GetRandomCrossSellProductsByCategoryId(List<int> categoryId, int count, int productId)
        {
            if (categoryId.Count == 0)
                return new List<Product>();

            string key = string.Format(PRODUCTCATEGORIES_ALLBYCATEGORYID_KEY, false, categoryId);
            //return _cacheManager.Get(key, () =>
            //{
            var query = (from pc in _productCategoryRepository.Table
                         join p in _productRepository.Table on pc.ProductId equals p.Id
                         where categoryId.Contains(pc.CategoryId) &&
                              !p.Deleted && pc.ProductId != productId &&
                         (false || p.Published)
                         select pc.Product).OrderBy(x => Guid.NewGuid()).Take(count).ToList();
            var productCategories = query.ToList();

            return productCategories;
        }

        public virtual IList<NewsItem> GetRandomCategoryNewsItemsByCategoryId(NewsType? newsType, int categoryId, int languageId, int count,int newsItemId)
        {
            if (categoryId == 0)
                return null;
            int? type = null;
            if (newsType.HasValue)
                type = (int)newsType.Value;

            string key = string.Format(CATEGORYNEWSITEM_BY_ID_KEY, categoryId);
            return _cacheManager.Get(key, () =>
            {
                var query = (from pc in _newsItemRepository.Table
                             join c in _newsItemCategoryRepository.Table on pc.Id equals c.NewsItemId
                             where c.CategoryId == categoryId && 
                             c.NewsItem.LanguageId == languageId && 
                             c.NewsItem.Published == true && 
                             pc.Id != newsItemId &&
                             (!type.HasValue || pc.SystemTypeId == (int)type.Value)
                             orderby c.CreatedOnUtc ascending
                             select pc).OrderBy(x => Guid.NewGuid()).Take(count).ToList();
                return query;
            });
        }
        #endregion
    }
}
