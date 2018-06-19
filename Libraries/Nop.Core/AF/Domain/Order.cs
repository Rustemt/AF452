using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nop.Core.Domain.Orders
{
    public partial class Order
    {
        #region Properties

        /// <summary>
        /// Gets or sets the order identifier
        /// </summary>
        public virtual int? Installment { get; set; }

        /// <summary>
        /// Gets or sets the customer identifier
        /// </summary>
        public virtual string PaymentCampaignNotes { get; set; }

        #endregion Properties

    }
}