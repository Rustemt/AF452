using System;
using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Messages;

namespace Nop.Services.Customers
{
    /// <summary>
    /// Customer service interface
    /// </summary>
    public partial interface ICustomerService
    {
        IList<NewsLetterSubscription> GetAllOrdererGuest();
        void InsertCustomerProductVariantQuote(CustomerProductVariantQuote customerProductVariantQuote);
        void DeleteCustomerProductVariantQuote(CustomerProductVariantQuote customerProductVariantQuote);
        void UpdateCustomerProductVariantQuote(CustomerProductVariantQuote customerProductVariantQuote);
        CustomerProductVariantQuote GetCustomerProductVariantQuoteByVariantId(int customerId,int productVariant);
        CustomerProductVariantQuote GetCustomerProductVariantQuoteByVariantId(string customerEmail, int variantId);
        List<CustomerProductVariantQuote> GetProductVariantQuoteByCustomerId(int customerId);
        void MigrateCustomerProductVariantQuotes(Customer fromCustomer, Customer toCustomer);
        void MigrateCustomerProductVariantQuotes(string fromCustomerEmail, Customer toCustomer);
        CustomerProductVariantQuote GetCustomerProductVariantQuoteById(int productVariantQuoteId);
        IPagedList<CustomerProductVariantQuote> GetAllProductVariantQuotes(DateTime? requestFrom, DateTime? requestTo, string email, string description, string sku, int pageIndex, int pageSize);
    }
}