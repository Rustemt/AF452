
namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a specification attribute option filter
    /// </summary>
    public class ProductAttributeOptionFilter
    {
        /// <summary>
        /// Gets or sets the specification attribute identifier
        /// </summary>
        public virtual int ProductAttributeId { get; set; }

        /// <summary>
        /// Gets or sets the specification attribute name
        /// </summary>
        public virtual string ProductAttributeName { get; set; }

        /// <summary>
        /// Gets or sets the specification attribute display order
        /// </summary>
        public virtual int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the specification attribute option identifier
        /// </summary>
        public virtual int ProductAttributeOptionId { get; set; }

        /// <summary>
        /// Gets or sets the specification attribute option name
        /// </summary>
        public virtual string ProductAttributeOptionName { get; set; }

        /// <summary>
        /// Gets or sets the specification attribute option display order
        /// </summary>
        public virtual int ProductAttributeOptionDisplayOrder { get; set; }

        public virtual string CssStyle { get; set; }


    }
}
