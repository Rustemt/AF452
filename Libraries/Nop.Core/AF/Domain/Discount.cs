using System;
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;

namespace Nop.Core.Domain.Discounts
{
    /// <summary>
    /// Represents a discount
    /// </summary>
    public partial class Discount
    {
        private ICollection<Manufacturer> _appliedToManufacturers;

        public virtual bool? ShowInCatalog { get; set; }
        public virtual bool? IsCumulative { get; set; }
        public virtual ICollection<Manufacturer> AppliedToManufacturers
        {
            get { return _appliedToManufacturers ?? (_appliedToManufacturers = new List<Manufacturer>()); }
            protected set { _appliedToManufacturers = value; }
        }
    }
}
