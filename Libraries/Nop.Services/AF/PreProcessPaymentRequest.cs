using Nop.Core.Domain.Orders;

namespace Nop.Services.Payments
{
    /// <summary>
    /// Represents a PreProcessPaymentRequest
    /// </summary>
    public partial class PreProcessPaymentRequest
    {
        /// <summary>
        /// Gets or sets an order. Used when order is already saved (payment gateways that redirect a customer to a third-party URL)
        /// </summary>
        public Order Order { get; set; }

        public bool RequiresRedirection { get; set; }

        public string RedirectURL { get; set; }

    }
}
