using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.News;
using Nop.Core.Events;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Manufacturer service
    /// </summary>
    public partial class ManufacturerService : IManufacturerService
    {

        private const string MANUFACTURER_BY_SENAME = "Nop.manufacturer.sename-{0}";
        private const string NEWSITEMMANUFACTURERS_PATTERN_KEY = "Nop.newsitemmanufacturer.";
        private const string NEWSITEMMANUFACTURERS_BY_ID_KEY = "Nop.newsitemmanufacturer.id-{0}";
        private const string MANUFACTURERNEWSITEM_BY_ID_KEY = "Nop.newsitem.id-{0}-{1}";
        private const string NEWSITEMMANUFACTURERS_ALLBYNEWSITEMID_KEY = "Nop.newsitemmanufacturer.allbynewsitemid-{0}-{1}";
        

        private readonly IRepository<NewsItemManufacturer> _newsItemManufacturerRepository;
        private readonly IRepository<NewsItem> _newsItemRepository;
        private readonly IRepository<ProductCategory> _productCategoryRepository;
        private readonly IRepository<Category> _categoryRepository;


         public ManufacturerService(ICacheManager cacheManager,
            IRepository<Manufacturer> manufacturerRepository,
            IRepository<ProductManufacturer> productManufacturerRepository,
            IRepository<Product> productRepository,
            IEventPublisher eventPublisher, 
            IRepository<NewsItemManufacturer> newsItemManufacturerRepository, 
            IRepository<NewsItem> newsItemRepository,
            IRepository<ProductCategory> productCategoryRepository,
            IRepository<Category> categoryRepository)
         {
             _cacheManager = cacheManager;
             _manufacturerRepository = manufacturerRepository;
             _productManufacturerRepository = productManufacturerRepository;
             _productRepository = productRepository;
             _eventPublisher = eventPublisher;
             _newsItemManufacturerRepository = newsItemManufacturerRepository;
             _newsItemRepository = newsItemRepository;
             _productCategoryRepository = productCategoryRepository;
             _categoryRepository = categoryRepository;

         }
        #region Methods

     

        /// <summary>
        /// Gets a manufacturer
        /// </summary>
        /// <param name="manufacturerId">Manufacturer identifier</param>
        /// <returns>Manufacturer</returns>
        public virtual Manufacturer GetManufacturerBySeName(string SeName)
        {
            if (string.IsNullOrWhiteSpace(SeName))
                return null;

            string key = string.Format(MANUFACTURER_BY_SENAME, SeName);
            return _cacheManager.Get(key, () =>
            {
                var query = from cr in _manufacturerRepository.Table
                            orderby cr.Id
                            where cr.SeName == SeName
                            select cr;
                var manufacturer = query.FirstOrDefault();
                return manufacturer;
            });
        }
        public virtual IList<NewsItemManufacturer> GetNewsItemManufacturersByNewsItemtId(int newsItemId, bool showHidden = false)
        {
            if (newsItemId == 0)
                return new List<NewsItemManufacturer>();

            string key = string.Format(NEWSITEMMANUFACTURERS_ALLBYNEWSITEMID_KEY, showHidden, newsItemId);
            return _cacheManager.Get(key, () =>
                                              {
                                                  var query = from pm in _newsItemManufacturerRepository.Table
                                                              where pm.NewsItemId == newsItemId
                                                              orderby pm.CreatedOnUtc
                                                              select pm;
                                                  var newsItemManufacturers = query.ToList();
                                                  return newsItemManufacturers;
                                              });
        }
        public virtual void InsertNewsItemManufacturer(NewsItemManufacturer newsItemManufacturer)
        {
            if (newsItemManufacturer == null)
                throw new ArgumentNullException("newsItemManufacturer");

            _newsItemManufacturerRepository.Insert(newsItemManufacturer);

            //cache
            _cacheManager.RemoveByPattern(MANUFACTURERS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(NEWSITEMMANUFACTURERS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(newsItemManufacturer);
        }
        public virtual NewsItemManufacturer GetNewsItemManufacturerById(int newsItemManufacturerId)
        {
            if (newsItemManufacturerId == 0)
                return null;

            string key = string.Format(NEWSITEMMANUFACTURERS_BY_ID_KEY, newsItemManufacturerId);
            return _cacheManager.Get(key, () =>
            {
                return _newsItemManufacturerRepository.GetById(newsItemManufacturerId);
            });
        }
        public virtual void UpdateNewsItemManufacturer(NewsItemManufacturer newsItemManufacturer)
        {
            if (newsItemManufacturer == null)
                throw new ArgumentNullException("newsItemManufacturer");

            _newsItemManufacturerRepository.Update(newsItemManufacturer);

            //cache
            _cacheManager.RemoveByPattern(MANUFACTURERS_PATTERN_KEY);
            //event notification
            _eventPublisher.EntityUpdated(newsItemManufacturer);
        }
        public virtual void DeleteNewsItemManufacturer(NewsItemManufacturer newsItemManufacturer)
        {
            if (newsItemManufacturer == null)
                throw new ArgumentNullException("newsItemManufacturer");

            _newsItemManufacturerRepository.Delete(newsItemManufacturer);

            //cache
            _cacheManager.RemoveByPattern(MANUFACTURERS_PATTERN_KEY);
            //event notification
            _eventPublisher.EntityDeleted(newsItemManufacturer);
        }
        public virtual NewsItem GetManufacturerNewsItemByManufacturerId(int manufacturerId,int languageId)
        {

            if (manufacturerId == 0)
                return null;

            string key = string.Format(MANUFACTURERNEWSITEM_BY_ID_KEY, languageId, manufacturerId);
            return _cacheManager.Get(key, () =>
            {
                var query = from pc in _newsItemRepository.Table
                            join c in _newsItemManufacturerRepository.Table on pc.Id equals c.NewsItemId
                            where c.ManufacturerId == manufacturerId && c.NewsItem.LanguageId==languageId  && c.NewsItem.Published == true
                            orderby pc.CreatedOnUtc descending 
                            select pc;
                var newsItem = query.FirstOrDefault();
                return newsItem;
            });

        }
        public virtual NewsItemManufacturer GetNewsItemManufactureryByManufacturerId(int manufacturerId)
        {
            var query = (from pc in _newsItemManufacturerRepository.Table
                         orderby pc.CreatedOnUtc descending
                         where pc.ManufacturerId == manufacturerId
                         select pc).FirstOrDefault();
            return query;
        }

        public virtual IList<Manufacturer> GetManufacturersByIds(IList<int> manufacturerIds, bool showHidden = false)
        {
            if (manufacturerIds == null || manufacturerIds.Count == 0)
                return new List<Manufacturer>();

            var query = _manufacturerRepository.Table;
            if (!showHidden)
            {
                query = query.Where(m => m.Published);
            }

            query = query.Where(m => !m.Deleted);
            query = query.Where(m => manufacturerIds.Contains(m.Id));
            query = query.OrderBy(m => m.DisplayOrder);

            var manufacturers = query.ToList();
            return manufacturers;
        }

        public virtual IList<Manufacturer> GetManufacturerByCategoryIds(IList<int> categoryIds)
        {

            if (categoryIds == null || categoryIds.Count == 0)
                return new List<Manufacturer>();

                var query = (from m in _manufacturerRepository.Table
                            join pm in _productManufacturerRepository.Table on m.Id equals pm.ManufacturerId
                            join pc in _productCategoryRepository.Table on pm.ProductId equals pc.ProductId
                            join c in _categoryRepository.Table on pc.CategoryId equals c.Id
                            join p in _productRepository.Table on pm.ProductId equals p.Id
                            where categoryIds.Contains(pc.CategoryId) 
                            && !m.Deleted 
                            && m.Published
                            && !c.Deleted
                            && c.Published
                            && !p.Deleted 
                            && p.Published
                            select  m).Distinct();
              var manufacturers = query.ToList();

              return manufacturers;


        }
        
        #endregion
    }
}
