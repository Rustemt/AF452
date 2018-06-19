using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nop.Admin.Models.Catalog;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Web.Framework.Mvc;
//using Nop.Web.Models.Media;

namespace Nop.Admin.Models.Customers
{
    public class ProductVariantQuoteModel : BaseNopEntityModel
    {

        public virtual int CustomerId { get; set; }

        public virtual string Email { get; set; }

        public virtual int ProductVariantId { get; set; }

        public virtual string PhoneNumber { get; set; }

        public virtual string Name { get; set; }

        public virtual string Enquiry { get; set; }

        public virtual string Description { get; set; }

        public virtual Customer Customer { get; set; }

        public virtual DateTime RequestDate { get; set; }

        public virtual DateTime? ActivateDate { get; set; }

        public virtual string ManufacturerName { get; set; }

         public virtual string ProductName { get; set; }

         public virtual string Sku { get; set; }

         public virtual string DiscountPercentage { get; set; }

         public virtual string PriceWithDiscount { get; set; }

         public virtual string PriceWithoutDiscount { get; set; }

        public ProductVariantPriceModel ProductVariantPrice { get; set; }

        //properties used for filtering (customer list page)
        public string SearchEmail { get; set; }
        public string SearchSku { get; set; }
        public string SearchProductName { get; set; }
        public string SearchDescription { get; set; }
        public string SearchPrice { get; set; }
        public string SearchPriceWithDiscount { get; set; }
        public DateTime SearchActivateDateFrom { get; set; }
        public DateTime SearchActivateDateTo { get; set; }

        public class ProductVariantPriceModel : BaseNopModel
        {
            public string OldPrice { get; set; }
            public string Price { get; set; }
            public string PriceWithDiscount { get; set; }
            public string DiscountPrice { get; set; }
            public decimal PriceValue { get; set; }
            public decimal PriceWithDiscountValue { get; set; }
            public decimal DiscountValue { get; set; }
            public string DiscountPercentage { get; set; }
            public bool CustomerEntersPrice { get; set; }
            public bool CallForPrice { get; set; }
            public int ProductVariantId { get; set; }
            public bool HidePrices { get; set; }
            public bool DynamicPriceUpdate { get; set; }
            public string Currency { get; set; }
        }

    }
}