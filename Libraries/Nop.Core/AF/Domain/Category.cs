using System;
using System.Collections.Generic;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Localization;

namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a category
    /// </summary>
    public partial class Category
    {
        private ICollection<NewsItemCategory> _newsItemCategories;
        
        /// <summary>
        /// Gets or sets the collection of ProductCategory
        /// </summary>
        public virtual ICollection<NewsItemCategory> NewsItemCategories
        {
            get { return _newsItemCategories ?? (_newsItemCategories = new List<NewsItemCategory>()); }
            protected set { _newsItemCategories = value; }
        }
       
    }
}
