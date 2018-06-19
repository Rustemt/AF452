using System;
using System.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.News;
using Nop.Core.Events;
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Core.Infrastructure;
using Nop.Data;
using System.Data.SqlClient;
using System.Data;

namespace Nop.Services.News
{
    /// <summary>
    /// News service
    /// </summary>
    public partial class NewsService : INewsService
    {
        private readonly IRepository<NewsItemPicture> _newsItemPictureRepository;
        private readonly IRepository<NewsItemProduct> _newsItemProductRepository;
        private readonly IRepository<NewsItemProductVariant> _newsItemProductVariantRepository;
        private readonly IRepository<ProductVariant> _productVariantRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<NewsItemExtraContent> _newsItemExtraContentRepository;
		private readonly IRepository<Picture> _pictureRepository;
		private readonly ICacheManager _cacheManagerPersistent;
		private readonly IDbContext _dbContext;

        #region Constants
       
        private const string NEWSITEMPRODUCT_ALLBYNEWSITEMID_KEY = "Nop.productnews.allbynewsitemid-{0}-{1}";
        private const string NEWSITEMPRODUCTVARIANTS_ALLBYNEWSITEMID_KEY = "Nop.productvariantsnews.allbynewsitemid-{0}-{1}";
        private const string NEWSITEMPRODUCTVARIANT_ALLBYNEWSITEMID_KEY = "Nop.productvariantnews.allbynewsitemid-{0}-{1}";
        private const string NEWSITEMPRODUCT_ALLBYPRODUCTID_KEY = "Nop.productnews.allbyproductid-{0}-{1}";
        private const string NEWSITEMPRODUCT_BY_ID_KEY = "Nop.productnewsitem.id-{0}";
        private const string NEWSITEMPRODUCTVARIANT_BY_ID_KEY = "Nop.productvariantnewsitem.id-{0}";
        private const string NEWSITEMPRODUCT_PATTERN_KEY = "Nop.productnewsitem.";
        private const string NEWSITEMPRODUCTVARIANT_PATTERN_KEY = "Nop.productvariantnewsitem.";
        private const string MEDIA_PICTURES_FOR_NEWS = "MediaPicturesForNews";
        private const string NEWS_ITEM_PICTURES = "NewsItemPictures";
		private const string NEWSITEMPICTURES = "NewsItemPictures";
		
        #endregion

        #region Ctor

		public NewsService(IDbContext dbContext, IRepository<Picture> pictureRepository, IRepository<NewsItem> newsItemRepository, ICacheManager cacheManager, IEventPublisher eventPublisher, IRepository<ProductVariant> productVariantRepository, IRepository<NewsItemProductVariant> newsItemProductVariantRepository, 
            IRepository<NewsItemPicture> newsItemPictureRepository, IRepository<Product> productRepository, IRepository<NewsItemProduct> newsItemProductRepository, IRepository<NewsItemExtraContent> newsItemExtraContentRepository)
        {
            _newsItemRepository = newsItemRepository;
            _cacheManager = cacheManager;
            _eventPublisher = eventPublisher;
            _newsItemPictureRepository = newsItemPictureRepository;
            _productRepository = productRepository;
            _productVariantRepository = productVariantRepository;
            _newsItemProductRepository = newsItemProductRepository;
            _newsItemExtraContentRepository = newsItemExtraContentRepository;
            _newsItemProductVariantRepository = newsItemProductVariantRepository;
			_pictureRepository = pictureRepository;
			_cacheManagerPersistent = EngineContext.Current.ContainerManager.Resolve<ICacheManager>("nop_cache_static");
			_dbContext = dbContext;
        }

        #endregion

        /// <summary>
        /// Gets all news
        /// </summary>
        /// <param name="languageId">Language identifier; 0 if you want to get all records</param>
        /// <param name="dateFrom">Filter by created date; null if you want to get all records</param>
        /// <param name="dateTo">Filter by created date; null if you want to get all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>News items</returns>
        public virtual IPagedList<NewsItem> GetAllNews(int languageId,
            DateTime? dateFrom, DateTime? dateTo, NewsType? systemTypes, int pageIndex, int pageSize, bool showHidden = false)
        {
            var query = _newsItemRepository.Table;
            if (dateFrom.HasValue)
                query = query.Where(n => dateFrom.Value <= n.CreatedOnUtc);
            if (dateTo.HasValue)
                query = query.Where(n => dateTo.Value >= n.CreatedOnUtc);
            if (languageId > 0)
                query = query.Where(n => languageId == n.LanguageId);
            if (!showHidden)
                query = query.Where(n => n.Published);
            if (systemTypes.HasValue)
            {
                var typeId = (int)systemTypes.Value;
                query = query.Where(n => (n.SystemTypeId & typeId) == n.SystemTypeId);
            }
            query = query.OrderByDescending(b => b.CreatedOnUtc);

            var news = new PagedList<NewsItem>(query, pageIndex, pageSize);
            return news;
        }

        public virtual IPagedList<NewsItem> GetAllNewsSearch( int pageIndex, int pageSize,string searchTitle, int searchSystemTypeId)
        {
            var query = _newsItemRepository.Table;
            if (!string.IsNullOrEmpty(searchTitle))
                query = query.Where(n => n.Title.Contains(searchTitle));
            if (searchSystemTypeId != 0)
                query = query.Where(n => searchSystemTypeId == n.SystemTypeId);
            //if (dateFrom.HasValue)
            //    query = query.Where(n => dateFrom.Value <= n.CreatedOnUtc);
            //if (dateTo.HasValue)
            //    query = query.Where(n => dateTo.Value >= n.CreatedOnUtc);
            //if (languageId > 0)
            //    query = query.Where(n => languageId == n.LanguageId);
            //if (!showHidden)
            //    query = query.Where(n => n.Published);
            //if (systemTypes.HasValue)
            //{
            //    var typeId = (int)systemTypes.Value;
            //    query = query.Where(n => (n.SystemTypeId & typeId) == n.SystemTypeId);
            //}
            query = query.OrderByDescending(b => b.CreatedOnUtc);

            var news = new PagedList<NewsItem>(query, pageIndex, pageSize);
            return news;
        }
        #region NewsItem pictures

        /// <summary>
        /// Deletes a newsItem picture
        /// </summary>
        /// <param name="newsItemPicture">NewsItem picture</param>
        public virtual void DeleteNewsItemPicture(NewsItemPicture newsItemPicture)
        {
            if (newsItemPicture == null)
                throw new ArgumentNullException("newsItemPicture");

            _newsItemPictureRepository.Delete(newsItemPicture);
            _cacheManagerPersistent.Remove(MEDIA_PICTURES_FOR_NEWS);
            _cacheManagerPersistent.Remove(NEWS_ITEM_PICTURES);
			_cacheManagerPersistent.Remove(NEWSITEMPICTURES);			

            //event notification
            _eventPublisher.EntityDeleted(newsItemPicture);
        }

        /// <summary>
        /// Gets a newsItem pictures by newsItem identifier
        /// </summary>
        /// <param name="newsItemId">The newsItem identifier</param>
        /// <returns>NewsItem pictures</returns>
        public virtual IList<NewsItemPicture> GetNewsItemPicturesByNewsItemId(int newsItemId, NewsItemPictureType? newsItemPictureType)
        {
			var pics = _cacheManagerPersistent.Get(NEWSITEMPICTURES, () =>
			{
				var query = from p in _newsItemPictureRepository.Table
							orderby p.DisplayOrder
							select p;
				return query.ToList();
			});

			pics = pics.Where(p => p.NewsItemId == newsItemId).ToList();
			if (newsItemPictureType.HasValue)
			{
				int typeId = (int)newsItemPictureType.Value;
				pics = pics.Where(p => p.NewsItemPictureTypeId == typeId).ToList();
			}
			return pics;

			//var query = _newsItemPictureRepository.Table;
			//query = query.Where(p => p.NewsItemId == newsItemId);
			//if (newsItemPictureType.HasValue)
			//{
			//	int typeId =(int)newsItemPictureType.Value;
			//	query = query.Where(p => p.NewsItemPictureTypeId == typeId);
			//}
			//query = query.OrderBy(p => p.DisplayOrder);
			//var newsItemPictures = query.ToList();
			//return newsItemPictures;
        }


		public virtual Picture GetMediaPictureByNewsItemPictureId(int pictureId)
		{
			var pics = _cacheManagerPersistent.Get(MEDIA_PICTURES_FOR_NEWS, () =>
			{
				var query = from p in _pictureRepository.Table
							join np in _newsItemPictureRepository.Table on p.Id equals np.PictureId
							select p;
				return query.ToList();
			});

			pics = pics.Where(p => p.Id == pictureId).ToList();
			return pics.FirstOrDefault();
		}

        /// <summary>
        /// Gets a newsItem picture
        /// </summary>
        /// <param name="newsItemPictureId">NewsItem picture identifier</param>
        /// <returns>NewsItem picture</returns>
        public virtual NewsItemPicture GetNewsItemPictureById(int newsItemPictureId)
        {
            if (newsItemPictureId == 0)
                return null;

            var pp = _newsItemPictureRepository.GetById(newsItemPictureId);
            return pp;
        }

        /// <summary>
        /// Inserts a newsItem picture
        /// </summary>
        /// <param name="newsItemPicture">NewsItem picture</param>
        public virtual void InsertNewsItemPicture(NewsItemPicture newsItemPicture)
        {
            if (newsItemPicture == null)
                throw new ArgumentNullException("newsItemPicture");

            _newsItemPictureRepository.Insert(newsItemPicture);
            _cacheManagerPersistent.Remove(MEDIA_PICTURES_FOR_NEWS);
            _cacheManagerPersistent.Remove(NEWS_ITEM_PICTURES);
			_cacheManagerPersistent.Remove(NEWSITEMPICTURES);

            //event notification
            _eventPublisher.EntityInserted(newsItemPicture);
        }

        /// <summary>
        /// Updates a newsItem picture
        /// </summary>
        /// <param name="newsItemPicture">NewsItem picture</param>
        public virtual void UpdateNewsItemPicture(NewsItemPicture newsItemPicture)
        {
            if (newsItemPicture == null)
                throw new ArgumentNullException("newsItemPicture");

            _newsItemPictureRepository.Update(newsItemPicture);
			_cacheManagerPersistent.Remove(MEDIA_PICTURES_FOR_NEWS);
			_cacheManagerPersistent.Remove(NEWSITEMPICTURES);

            //event notification
            _eventPublisher.EntityUpdated(newsItemPicture);
        }

        #endregion
       
        #region products

        public virtual void DeleteNewsItemProduct(NewsItemProduct newsItemProduct)
        {
            if (newsItemProduct == null)
                throw new ArgumentNullException("newsItemProduct");

            _newsItemProductRepository.Delete(newsItemProduct);

            //cache
            _cacheManager.RemoveByPattern(NEWS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(NEWSITEMPRODUCT_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(newsItemProduct);
        }

        public virtual void DeleteNewsItemProductVariant(NewsItemProductVariant newsItemProductVariant)
        {
            if (newsItemProductVariant == null)
                throw new ArgumentNullException("newsItemProductVariant");

            _newsItemProductVariantRepository.Delete(newsItemProductVariant);

            //cache
            _cacheManager.RemoveByPattern(NEWS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(NEWSITEMPRODUCTVARIANT_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(newsItemProductVariant);
        }

        public virtual IList<NewsItemProduct> GetNewsItemProductsByNewsItemId(int newsItemId, bool showHidden = false)
        {
            if (newsItemId == 0)
                return new List<NewsItemProduct>();

            string key = string.Format(NEWSITEMPRODUCTVARIANTS_ALLBYNEWSITEMID_KEY, showHidden, newsItemId);
            return _cacheManager.Get(key, () =>
            {
                var query = from pm in _newsItemProductRepository.Table
                            join p in _productRepository.Table on pm.ProductId equals p.Id
                            where pm.NewsItemId == newsItemId &&
                                  !p.Deleted &&
                                  (showHidden || p.Published)
                            orderby pm.DisplayOrder
                            select pm;
                var newsItemproducts = query.ToList();
                return newsItemproducts;
            });
        }

        public virtual IList<NewsItemProductVariant> GetNewsItemProductVariantsByNewsItemId(int newsItemId, bool showHidden = false)
        {
            if (newsItemId == 0)
                return new List<NewsItemProductVariant>();

            string key = string.Format(NEWSITEMPRODUCTVARIANT_ALLBYNEWSITEMID_KEY, showHidden, newsItemId);
            return _cacheManager.Get(key, () =>
            {
                var query = from pm in _newsItemProductVariantRepository.Table
                            join pv in _productVariantRepository.Table on pm.ProductVariantId equals pv.Id
                            join p in _productRepository.Table on pv.ProductId equals p.Id
                            where pm.NewsItemId == newsItemId &&
                                  !pv.Deleted &&
                                  (showHidden || pv.Published) &&
                                  (showHidden || p.Published)
                            orderby pm.DisplayOrder
                            select pm;
                var newsItemproducts = query.ToList();
                return newsItemproducts;
            });
        }

        public virtual NewsItemProduct GetNewsItemProductById(int newsItemProductId)
        {
            if (newsItemProductId == 0)
                return null;

            string key = string.Format(NEWSITEMPRODUCT_BY_ID_KEY, newsItemProductId);
            return _cacheManager.Get(key, () =>
            {
                return _newsItemProductRepository.GetById(newsItemProductId);
            });
        }

        public virtual NewsItemProductVariant GetNewsItemProductVariantById(int newsItemProductVariantId)
        {
            if (newsItemProductVariantId == 0)
                return null;

            string key = string.Format(NEWSITEMPRODUCTVARIANT_BY_ID_KEY, newsItemProductVariantId);
            return _cacheManager.Get(key, () =>
            {
                return _newsItemProductVariantRepository.GetById(newsItemProductVariantId);
            });
        }

        public virtual void InsertNewsItemProduct(NewsItemProduct newsItemProduct)
        {
            if (newsItemProduct == null)
                throw new ArgumentNullException("newsItemProduct");

            _newsItemProductRepository.Insert(newsItemProduct);

            //cache
            _cacheManager.RemoveByPattern(NEWS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(NEWSITEMPRODUCT_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(newsItemProduct);
        }

        public virtual void InsertNewsItemProductVariant(NewsItemProductVariant newsItemProductVariant)
        {
            if (newsItemProductVariant == null)
                throw new ArgumentNullException("newsItemProductVariant");

            _newsItemProductVariantRepository.Insert(newsItemProductVariant);

            //cache
            _cacheManager.RemoveByPattern(NEWS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(NEWSITEMPRODUCTVARIANT_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(newsItemProductVariant);
        }

        public virtual void UpdateNewsItemProduct(NewsItemProduct newsItemProduct)
        {
            if (newsItemProduct == null)
                throw new ArgumentNullException("newsItemProduct");

            _newsItemProductRepository.Update(newsItemProduct);

            //cache
            _cacheManager.RemoveByPattern(NEWS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(NEWSITEMPRODUCT_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(newsItemProduct);
        }

        public virtual void UpdateNewsItemProductVariant(NewsItemProductVariant newsItemProductVariant)
        {
            if (newsItemProductVariant == null)
                throw new ArgumentNullException("newsItemProductVariant");

            _newsItemProductVariantRepository.Update(newsItemProductVariant);

            //cache
            _cacheManager.RemoveByPattern(NEWS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(NEWSITEMPRODUCTVARIANT_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(newsItemProductVariant);
        }

        #endregion

        #region NewsItem ExtraContents

        public virtual void DeleteNewsItemExtraContent(NewsItemExtraContent newsItemExtraContent)
        {
            if (newsItemExtraContent == null)
                throw new ArgumentNullException("newsItemExtraContent");

            _newsItemExtraContentRepository.Delete(newsItemExtraContent);

            //event notification
            _eventPublisher.EntityDeleted(newsItemExtraContent);
        }

        public virtual IList<NewsItemExtraContent> GetNewsItemExtraContentsByNewsItemId(int newsItemId)
        {
            var query = _newsItemExtraContentRepository.Table;
            query = query.Where(p => p.NewsItemId == newsItemId);
            query = query.OrderBy(p => p.DisplayOrder);
            var newsItemExtraContents = query.ToList();
            return newsItemExtraContents;
        }

        public virtual NewsItemExtraContent GetNewsItemExtraContentById(int newsItemExtraContentId)
        {
            if (newsItemExtraContentId == 0)
                return null;

            var pp = _newsItemExtraContentRepository.GetById(newsItemExtraContentId);
            return pp;
        }

        public virtual void InsertNewsItemExtraContent(NewsItemExtraContent newsItemExtraContent)
        {
            if (newsItemExtraContent == null)
                throw new ArgumentNullException("newsItemExtraContent");

            _newsItemExtraContentRepository.Insert(newsItemExtraContent);

            //event notification
            _eventPublisher.EntityInserted(newsItemExtraContent);
        }

        public virtual void UpdateNewsItemExtraContent(NewsItemExtraContent newsItemExtraContent)
        {
            if (newsItemExtraContent == null)
                throw new ArgumentNullException("newsItemExtraContent");

            _newsItemExtraContentRepository.Update(newsItemExtraContent);

            //event notification
            _eventPublisher.EntityUpdated(newsItemExtraContent);
        }

        #endregion

        #region Search

        public virtual IPagedList<NewsItem> GetAllNewsSearch(int pageIndex, int pageSize, string title, NewsType? newsType, int? languageId, bool published, DateTime? startTime, DateTime? endTime)
        {
            int? newsTypeId = null;
            if (newsType.HasValue)
                newsTypeId = (int)newsType.Value;

            var query = _newsItemRepository.Table;
            if (startTime.HasValue)
                query = query.Where(n => startTime.Value <= n.CreatedOnUtc);
            if (endTime.HasValue)
                query = query.Where(n => endTime.Value >= n.CreatedOnUtc);
            if (newsTypeId.HasValue)
                query = query.Where(n => n.SystemTypeId == newsTypeId.Value);
            if (languageId.HasValue)
                query = query.Where(n => n.LanguageId == languageId.Value);
            if (published)
                query = query.Where(n => n.Published == published);
            if (!string.IsNullOrEmpty(title))
                query = query.Where(n => n.Title.Contains(title));

            query = query.OrderByDescending(o => o.CreatedOnUtc);

            var newsItems = query.ToList();

            return new PagedList<NewsItem>(newsItems, pageIndex, pageSize);
        }
        
        #endregion


		public virtual IList<ProductSummary> GetNewsItemProductsSummaryByNewsItemId(int newsItemId)
		{
			if (newsItemId == 0)
				return new List<ProductSummary>();

			return _dbContext.ExecuteStoredProcedureList<ProductSummary>(
				"exec GetNewsItemProductsSummaryByNewsItemId @newsItemId",
				new SqlParameter { ParameterName = "newsItemId", Value = newsItemId, SqlDbType = SqlDbType.Int });
		}
    }
}
