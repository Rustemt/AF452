using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using Nop.Core.Domain.Affiliates;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Discounts;

namespace Nop.Core.Domain.Customers
{
    /// <summary>
    /// Represents a customer
    /// </summary>
    public partial class Customer : BaseEntity
    {
        private ICollection<CustomerProductVariantQuote> _customerProductVariantQuotes;

        public virtual ICollection<CustomerProductVariantQuote> CustomerProductVariantQuotes
        {
            get { return _customerProductVariantQuotes ?? (_customerProductVariantQuotes = new List<CustomerProductVariantQuote>()); }
            protected set { _customerProductVariantQuotes = value; }
        }

        /// <summary>
        /// Default billing address
        /// </summary>
        public virtual Address DefaultBillingAddress { get; set; }

        /// <summary>
        /// Default shipping address
        /// </summary>
        public virtual Address DefaultShippingAddress { get; set; }

    }
}