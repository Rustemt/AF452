using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;

namespace Nop.Services.Common
{
    /// <summary>
    /// Customer service interface
    /// </summary>
    public partial interface IPdfService
    {
        /// <summary>
        /// Print an order inoive to PDF
        /// </summary>
        /// <param name="orders">Orders</param>
        /// <param name="lang">Language</param>
        /// <param name="filePath">File path</param>
        void PrintInvoiceToPdf(IList<Order> orders, Language lang, string filePath);
        string PrintInvoiceToHtml(IList<Order> orders, Language lang, string filePath);
        void PrintInvoiceToPdfBisse(IList<Order> orders, Language lang, string filePath);
    }
}