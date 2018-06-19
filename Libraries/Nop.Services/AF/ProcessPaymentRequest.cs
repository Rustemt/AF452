using System;
using Nop.Core.Domain.Catalog;

namespace Nop.Services.Payments
{
    /// <summary>
    /// Represents a payment info holder
    /// </summary>
    //[Serializable]
    public partial class ProcessPaymentRequest
    {
        public string TrackingNumber { get; set; }
    
    }
}
