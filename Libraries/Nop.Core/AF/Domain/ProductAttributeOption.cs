using System.Collections.Generic;
using Nop.Core.Domain.Localization;

namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product attribute option
    /// </summary>
    public partial class ProductAttributeOption : BaseEntity, ILocalizedEntity
    {
        /// <summary>
        /// Gets or sets the specification attribute identifier
        /// </summary>
        public virtual int ProductAttributeId { get; set; }

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public virtual int DisplayOrder { get; set; }


        public virtual string CssStyle { get; set; }
        
        /// <summary>
        /// Gets or sets the specification attribute
        /// </summary>
        public virtual ProductAttribute ProductAttribute { get; set; }

    }
}
