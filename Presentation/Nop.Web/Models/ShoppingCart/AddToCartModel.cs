using System.Collections.Generic;
using System.Web.Routing;
using Nop.Core.Domain.Catalog;
using Nop.Web.Framework.Mvc;
using Nop.Web.Models.Media;
using Nop.Core.Domain.Orders;

namespace Nop.Web.Models.ShoppingCart
{
    public class AddToCartModel : BaseNopModel
    {

        public int ProductId { get; set; }
        public int VariantId { get;set; }
        public int Quantity { get; set; }
        public int ShoppingCartType { get; set; }
        public string RecipientName { get; set; }
        public string RecipientEmail { get; set; }
        public string YourName  { get; set; }
        public string YourEmail { get; set; }
        public string Message { get; set; }

        public IList<AttributeModel> Attributes { get; set; }

        public AddToCartModel()
        {
            Attributes = new List<AttributeModel>();
        }

		#region Nested Classes
        
        public class AttributeModel 
        {
            public int ProductAttributeId {get;set;}
            public int ProductVariantAttributeId {get;set;}
            public string ProductVariantAttributeValueId { get; set; }
          
        }
       
		#endregion Nested Classes
    }
}