using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Events;
using Nop.Core.Domain.Discounts;
using Nop.Services.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Messages;

namespace Nop.Services.Customers
{
    /// <summary>
    /// Customer service
    /// </summary>
    public partial class CustomerService : ICustomerService
    {
        #region Fields
        private readonly IRepository<CustomerProductVariantQuote> _customerProductVariantQuoteRepository;
       

        #endregion

        #region Methods


        public virtual void MigrateCustomerProductVariantQuotes(Customer fromCustomer, Customer toCustomer)
        {
            if (fromCustomer == null)
                throw new ArgumentNullException("fromCustomer");
            if (toCustomer == null)
                throw new ArgumentNullException("toCustomer");

            if (fromCustomer.Id == toCustomer.Id)
                return; //the same customer

            foreach (var customerProductVariantQuote in fromCustomer.CustomerProductVariantQuotes)
            {
                if (customerProductVariantQuote.ProductVariant.CallforPriceRequested(toCustomer)) continue;
                var quote = new CustomerProductVariantQuote(){
                                 CustomerId = toCustomer.Id,
                                 ProductVariantId = customerProductVariantQuote.ProductVariant.Id
                             };
                this.InsertCustomerProductVariantQuote(quote);
            }
        }
        public virtual void MigrateCustomerProductVariantQuotes(string fromCustomerEmail, Customer toCustomer)
        {
            if (fromCustomerEmail == null)
                throw new ArgumentNullException("fromCustomer");
            if (toCustomer == null)
                throw new ArgumentNullException("toCustomer");

            var quotes = this.GetAllProductVariantQuotes(null, null, fromCustomerEmail, null, null, 0, int.MaxValue);
            foreach (var customerProductVariantQuote in quotes)
            {
                //if (customerProductVariantQuote.ProductVariant.CallforPriceRequested(toCustomer)) continue;
                customerProductVariantQuote.CustomerId = toCustomer.Id;
                this.UpdateCustomerProductVariantQuote(customerProductVariantQuote);
                //var quote = new CustomerProductVariantQuote()
                //{
                //    CustomerId = toCustomer.Id,
                //    ProductVariantId = customerProductVariantQuote.ProductVariant.Id,
                //    ActivateDate = customerProductVariantQuote.ActivateDate,
                //    Description=customerProductVariantQuote.Description,
                //    DiscountPercentage=customerProductVariantQuote.DiscountPercentage,
                //    Email=customerProductVariantQuote.Email,
                //    Enquiry=customerProductVariantQuote.Enquiry,
                //    PhoneNumber=customerProductVariantQuote.PhoneNumber,
                //    PriceWithDiscount=customerProductVariantQuote.PriceWithDiscount,
                //    PriceWithoutDiscount=customerProductVariantQuote.PriceWithoutDiscount,
                //    RequestDate=customerProductVariantQuote.RequestDate
                //};
                //this.InsertCustomerProductVariantQuote(quote);
            }
        }


        /// <summary>
        /// Inserts a product variant picture
        /// </summary>
        /// <param name="productPicture">Product variant picture</param>
        public virtual void InsertCustomerProductVariantQuote(CustomerProductVariantQuote customerProductVariantQuote)
        {
            if (customerProductVariantQuote == null)
                throw new ArgumentNullException("customerProductVariantQuote");
            _customerProductVariantQuoteRepository.Insert(customerProductVariantQuote);
        }

        public virtual void DeleteCustomerProductVariantQuote(CustomerProductVariantQuote customerProductVariantQuote)
        {
            if (customerProductVariantQuote == null)
                throw new ArgumentNullException("customerProductVariantQuote");
            _customerProductVariantQuoteRepository.Delete(customerProductVariantQuote);
            
        }

        public virtual void UpdateCustomerProductVariantQuote(CustomerProductVariantQuote customerProductVariantQuote)
        {
            if (customerProductVariantQuote == null)
                throw new ArgumentNullException("customerProductVariantQuote");
            _customerProductVariantQuoteRepository.Update(customerProductVariantQuote);

        }

        public virtual CustomerProductVariantQuote GetCustomerProductVariantQuoteByVariantId(int customerId, int variantId)
        {
            var quote = _customerProductVariantQuoteRepository.Table.Where(x => x.CustomerId == customerId && x.ProductVariantId == variantId && x.ProductVariantId != 0).FirstOrDefault();
            return quote;
        }

        public virtual CustomerProductVariantQuote GetCustomerProductVariantQuoteByVariantId(string customerEmail, int variantId)
        {
            var quote = _customerProductVariantQuoteRepository.Table.Where(x => x.Email == customerEmail && x.ProductVariantId == variantId && x.ProductVariantId != 0).FirstOrDefault();
            return quote;
        }

        public List<CustomerProductVariantQuote> GetProductVariantQuoteByCustomerId(int customerId)
        {
            var quote = _customerProductVariantQuoteRepository.Table.Where(x => x.CustomerId == customerId && x.RequestDate != null && x.ActivateDate != null && x.ProductVariantId != 0).ToList();
            return quote;
        }

        public virtual IPagedList<CustomerProductVariantQuote> GetAllProductVariantQuotes(DateTime? requestFrom,
            DateTime? requestTo, string email, string description ,string sku, int pageIndex, int pageSize)
        {
            var query = _customerProductVariantQuoteRepository.Table.Where(x => x.ProductVariantId != 0);
            if (requestFrom.HasValue)
                query = query.Where(c => requestFrom.Value <= c.RequestDate);
            if (requestTo.HasValue)
                query = query.Where(c => requestTo.Value >= c.RequestDate);
            if (!String.IsNullOrWhiteSpace(sku))
                query = query.Where(c => c.ProductVariant.Sku.Contains(sku));
            if (!String.IsNullOrWhiteSpace(email))
                query = query.Where(c => c.Email.Contains(email));
            if (!String.IsNullOrWhiteSpace(description))
                query = query.Where(c => c.Description.Contains(description));

            query = query.OrderByDescending(c => c.Id);

            var pvqs = new PagedList<CustomerProductVariantQuote>(query, pageIndex, pageSize);
            return pvqs;
        }
        

        public CustomerProductVariantQuote GetCustomerProductVariantQuoteById(int productVariantQuoteId)
        {
            var quote = _customerProductVariantQuoteRepository.Table.Where(x => x.Id == productVariantQuoteId).FirstOrDefault();
            return quote;
        }


        public virtual IList<NewsLetterSubscription> GetAllOrdererGuest()
        {

            IRepository<NewsLetterSubscription> _newsletterRepository = Nop.Core.Infrastructure.EngineContext.Current.Resolve<IRepository<NewsLetterSubscription>>();

            var query = (from newsletter in _newsletterRepository.Table
                         where newsletter.RegistrationType == "CheckOut"
                         select newsletter).Distinct().ToList();

            return query;
        
        
        }


        #endregion
    }
}