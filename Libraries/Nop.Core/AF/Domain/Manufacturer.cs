using System;
using System.Collections.Generic;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.News;

namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a manufacturer
    /// </summary>
    public partial class Manufacturer
    {
        private ICollection<Discount> _appliedDiscounts;
        private ICollection<NewsItemManufacturer> _newsItemManufacturers;

        public virtual ICollection<Discount> AppliedDiscounts
        {
            get { return _appliedDiscounts ?? (_appliedDiscounts = new List<Discount>()); }
            protected set { _appliedDiscounts = value; }
        }

        public virtual ICollection<NewsItemManufacturer> NewsItemManufacturers
        {
            get { return _newsItemManufacturers ?? (_newsItemManufacturers = new List<NewsItemManufacturer>()); }
            protected set { _newsItemManufacturers = value; }
        }

		public virtual int MenuPictureId { get; set; }
		public virtual int MenuShowcasePictureId { get; set; }
    }
}
