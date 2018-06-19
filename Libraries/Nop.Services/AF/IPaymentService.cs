using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Orders;

namespace Nop.Services.Payments
{
    /// <summary>
    /// Payment service interface
    /// </summary>
    public partial interface IPaymentService
    {
        Bin GetBinByCCNo(string CreditCartNo);
        bool BinExists(string CreditCartNo);
        PreProcessPaymentResult PreProcessPayment(ProcessPaymentRequest preProcessPaymentRequest);
        IList<KeyValuePair<string, string>> GetPaymentOptions(ProcessPaymentRequest preProcessPaymentRequest);

    }
        
    /*
     public partial interface ICustomerService
    {
        void InsertCustomerProductVariantQuote(CustomerProductVariantQuote customerProductVariantQuote);
        void DeleteCustomerProductVariantQuote(CustomerProductVariantQuote customerProductVariantQuote);
        void UpdateCustomerProductVariantQuote(CustomerProductVariantQuote customerProductVariantQuote);
        CustomerProductVariantQuote GetCustomerProductVariantQuoteByVariantId(int customerId,int productVariant);
    }
     
     */
}
