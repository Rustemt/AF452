using System;
using System.Collections.Generic;
using Nop.Web.Framework.Mvc;
using Nop.Web.Models.Media;
using Nop.Web.Models.Customer;

namespace Nop.Web.Models.ShoppingCart
{
    public class WishlistModel : BaseNopModel
    {
        public WishlistModel()
        {
            Items = new List<ShoppingCartItemModel>();
            Warnings = new List<string>();
        }

        public Guid CustomerGuid { get; set; }

        public string CustomerFullname { get; set; }

        public bool EmailWishlistEnabled { get; set; }

        public bool ShowSku { get; set; }

        public bool ShowProductImages { get; set; }

        public bool IsEditable { get; set; }

        public CustomerNavigationModel Navigationmodel;

        public IList<ShoppingCartItemModel> Items { get; set; }

        public IList<string> Warnings { get; set; }
        
		#region Nested Classes

        public class ShoppingCartItemModel : BaseNopEntityModel
        {
            public ShoppingCartItemModel()
            {
                Picture = new PictureModel();
                Warnings = new List<string>();
            }
            public string Sku { get; set; }

            public PictureModel Picture {get;set;}

            public int ProductId { get; set; }

            public string ProductName { get; set; }

            public string ProductSeName { get; set; }

            public string UnitPrice { get; set; }

            public string SubTotal { get; set; }

            public string Discount { get; set; }

            public int Quantity { get; set; }
            
            public string AttributeInfo { get; set; }

            public string RecurringInfo { get; set; }

            public IList<string> Warnings { get; set; }

            public string Manufacturer { get; set; }

            public string OldPrice { get; set; }

            public string Created { get; set; }

            public int VariantId { get; set; }

            public string CustomerComment { get; set; }

            public decimal UnitPriceDecimal { get; set; }

            public decimal OldPriceDecimal { get; set; }

            public bool CallForPrice { get; set; }

            public bool HasStock { get; set; }

            public string HasStockText { get; set; }
        }

		#endregion
    }
}