using System.Collections.Generic;
using Nop.Core.Domain.Catalog;

namespace Nop.Core.Domain.Directory
{
    /// <summary>
    /// Represents a currency
    /// </summary>
    public partial class Currency : BaseEntity
    {
        private ICollection<ProductVariant> _productVariants;
       

        public virtual ICollection<ProductVariant> ProductVariants
        {
            get { return _productVariants ?? (_productVariants = new List<ProductVariant>()); }
            protected set { _productVariants = value; }
        }

    }

}
