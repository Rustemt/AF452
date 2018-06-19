using System;
using System.Web.Routing;
using Nop.Core.Domain.Orders;
using Nop.Core.Plugins;
using System.Collections.Generic;

namespace Nop.Services.Payments
{
    /// <summary>
    /// Provides an interface for creating payment gateways & methods
    /// </summary>
    public partial interface IPaymentMethod : IPlugin
    {
        #region Methods

        PreProcessPaymentResult PreProcessPayment(ProcessPaymentRequest preProcessPaymentRequest);

        #endregion

      
    }

    public partial interface ICampaignedPaymentMethod
    {
        #region Methods

        IList<KeyValuePair<string, string>> GetPaymentOptions(ProcessPaymentRequest processPaymentRequest);

        #endregion


    }
}
