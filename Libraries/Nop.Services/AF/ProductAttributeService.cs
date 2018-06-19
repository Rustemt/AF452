using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Events;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Product attribute service
    /// </summary>
    public partial class ProductAttributeService : IProductAttributeService
    {
        #region Constants
        private const string PRODUCTATTRIBUTEOPTION_BY_ID_KEY = "Nop.productattributeoptions.id-{0}";
        private const string PRODUCTATTRIBUTEOPTION_PATTERN_KEY = "Nop.productattributeoptions.";
        #endregion

        #region Fields
        private readonly IRepository<ProductAttributeOption> _productAttributeOptionRepository;
        #endregion

        #region Ctor
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="productAttributeRepository">Product attribute repository</param>
        /// <param name="productVariantAttributeRepository">Product variant attribute mapping repository</param>
        /// <param name="productVariantAttributeCombinationRepository">Product variant attribute combination repository</param>
        /// <param name="productVariantAttributeValueRepository">Product variant attribute value repository</param>
        public ProductAttributeService(
            ICacheManager cacheManager,
            IRepository<ProductAttribute> productAttributeRepository,
            IRepository<ProductVariantAttribute> productVariantAttributeRepository,
            IRepository<ProductVariantAttributeCombination> productVariantAttributeCombinationRepository,
            IRepository<ProductVariantAttributeValue> productVariantAttributeValueRepository,
            IEventPublisher eventPublisher,
            IRepository<ProductAttributeOption> productAttributeOptionRepository
            )
        {
            this._cacheManager = cacheManager;
            this._productAttributeRepository = productAttributeRepository;
            this._productVariantAttributeRepository = productVariantAttributeRepository;
            this._productVariantAttributeCombinationRepository = productVariantAttributeCombinationRepository;
            this._productVariantAttributeValueRepository = productVariantAttributeValueRepository;
            this._productAttributeOptionRepository = productAttributeOptionRepository;
            _eventPublisher = eventPublisher;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Gets a product attribute option
        /// </summary>
        /// <param name="productAttributeOptionId">The product attribute option identifier</param>
        /// <returns>product attribute option</returns>
        public virtual ProductAttributeOption GetProductAttributeOptionById(int productAttributeOptionId)
        {
            //if (productAttributeOptionId == 0)
            //    return null;

            string key = string.Format(PRODUCTATTRIBUTEOPTION_BY_ID_KEY, productAttributeOptionId);
            return _cacheManager.Get(key, () =>
            {
                var sao = _productAttributeOptionRepository.GetById(productAttributeOptionId);
                return sao;
            });
        }

        /// <summary>
        /// Gets a product attribute option by product attribute id
        /// </summary>
        /// <param name="productAttributeId">The product attribute identifier</param>
        /// <returns>product attribute option</returns>
        public virtual IList<ProductAttributeOption> GetProductAttributeOptionsByProductAttribute(int productAttributeId)
        {
            var query = from sao in _productAttributeOptionRepository.Table
                        orderby sao.DisplayOrder
                        where sao.ProductAttributeId == productAttributeId
                        select sao;
            var productAttributeOptions = query.ToList();
            return productAttributeOptions;
        }

        /// <summary>
        /// Deletes a product attribute option
        /// </summary>
        /// <param name="specificationAttributeOption">The specification attribute option</param>
        public virtual void DeleteProductAttributeOption(ProductAttributeOption productAttributeOption)
        {
            if (productAttributeOption == null)
                throw new ArgumentNullException("productAttributeOption");

            _productAttributeOptionRepository.Delete(productAttributeOption);

            _cacheManager.RemoveByPattern(PRODUCTATTRIBUTEOPTION_PATTERN_KEY);
            _eventPublisher.EntityDeleted(productAttributeOption);

        }

        /// <summary>
        /// Inserts a product attribute option
        /// </summary>
        /// <param name="productAttributeOption">The product attribute option</param>
        public virtual void InsertProductAttributeOption(ProductAttributeOption productAttributeOption)
        {
            if (productAttributeOption == null)
                throw new ArgumentNullException("productAttributeOption");

            _productAttributeOptionRepository.Insert(productAttributeOption);

            _cacheManager.RemoveByPattern(PRODUCTATTRIBUTEOPTION_PATTERN_KEY);
            _eventPublisher.EntityInserted(productAttributeOption);

        }

        /// <summary>
        /// Updates the specification attribute
        /// </summary>
        /// <param name="productAttributeOption">The product attribute option</param>
        public virtual void UpdateProductAttributeOption(ProductAttributeOption productAttributeOption)
        {
            if (productAttributeOption == null)
                throw new ArgumentNullException("productAttributeOption");

            _productAttributeOptionRepository.Update(productAttributeOption);

            _cacheManager.RemoveByPattern(PRODUCTATTRIBUTEOPTION_PATTERN_KEY);
            _eventPublisher.EntityUpdated(productAttributeOption);

        }


        public virtual IList<ProductVariantAttribute> GetProductVariantAttributesByIds(IList<int> productVariantAttributeIds)
        {
            if (productVariantAttributeIds == null || productVariantAttributeIds.Count == 0)
                return null;
            var query = _productVariantAttributeRepository.Table;

            query = query.Where(pva => productVariantAttributeIds.Contains(pva.Id));
            query = query.OrderBy(pva => pva.DisplayOrder);
            return query.ToList();
        }

       


        #endregion
    }
}
