using System.Collections.Generic;
using Nop.Core.Domain.Localization;

namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product attribute
    /// </summary>
    public partial class ProductAttribute : BaseEntity, ILocalizedEntity
    {
        private ICollection<ProductAttributeOption> _productAttributeOptions;

        /// <summary>
        /// Gets the product attributes options
        /// </summary>
        public virtual ICollection<ProductAttributeOption> ProductAttributeOptions
        {
            get { return _productAttributeOptions ?? (_productAttributeOptions = new List<ProductAttributeOption>()); }
            protected set { _productAttributeOptions = value; }
        }
        
    }
}
