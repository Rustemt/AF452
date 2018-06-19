using Nop.Core.Domain.Localization;

namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product variant attribute value
    /// </summary>
    public partial class ProductVariantAttributeValue : BaseEntity, ILocalizedEntity
    {
        public virtual int ProductAttributeOptionId { get; set; }
      
    }

}
