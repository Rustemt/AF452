using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using Telerik.Web.Mvc;

namespace Nop.Admin.Models.Customers
{
    public class ProductVariantQuoteListModel : BaseNopModel
    {
        public GridModel<ProductVariantQuoteModel> ProductVariantQuotes { get; set; }

        [NopResourceDisplayName("Admin.Customer.CustpmerProductQuotes.List.SearchEmail")]
        public string SearchEmail { get; set; }

        [NopResourceDisplayName("Admin.Customer.CustpmerProductQuotes.List.SearchSku")]
        public string SearchSku { get; set; }

        [NopResourceDisplayName("Admin.Customer.CustpmerProductQuotes.List.SearchProductName")]
        public string SearchProductName { get; set; }

         [NopResourceDisplayName("Admin.Customer.CustpmerProductQuotes.List.SearchDescription")]
        public string SearchDescription { get; set; }

         [NopResourceDisplayName("Admin.Customer.CustpmerProductQuotes.List.SearchPrice")]
         public string SearchPrice { get; set; }

         [NopResourceDisplayName("Admin.Customer.CustpmerProductQuotes.List.SearchPriceWithDiscount")]
         public string SearchPriceWithDiscount { get; set; }

         [NopResourceDisplayName("Admin.Customer.CustpmerProductQuotes.List.SearchRequestsDateFrom")]
         [UIHint("DateNullable")]
         public DateTime? SearchRequestDateFrom { get; set; }

         [NopResourceDisplayName("Admin.Customer.CustpmerProductQuotes.List.SearchRequestDateTo")]
         [UIHint("DateNullable")]
         public DateTime? SearchRequestDateTo { get; set; }
    }
}