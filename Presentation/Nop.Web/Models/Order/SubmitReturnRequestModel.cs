using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation.Attributes;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using Nop.Web.Models.Media;
using Nop.Web.Validators.Customer;

namespace Nop.Web.Models.Order
{
    [Validator(typeof(SubmitReturnRequestModelValidator))]
    public class SubmitReturnRequestModel : BaseNopModel
    {
        public SubmitReturnRequestModel()
        {
            Items = new List<OrderProductVariantModel>();
            AvailableReturnReasons = new List<SelectListItem>();
            AvailableReturnActions= new List<SelectListItem>();
        }

        public int OrderId { get; set; }
        
        public IList<OrderProductVariantModel> Items { get; set; }
        
        [AllowHtml]
        [NopResourceDisplayName("ReturnRequests.ReturnReason")]
        public string ReturnReason { get; set; }
        public IList<SelectListItem> AvailableReturnReasons { get; set; }

        [AllowHtml]
        [NopResourceDisplayName("ReturnRequests.ReturnAction")]
        public string ReturnAction { get; set; }
        public IList<SelectListItem> AvailableReturnActions { get; set; }

        [AllowHtml]
        [NopResourceDisplayName("ReturnRequests.Comments")]
        public string Comments { get; set; }

        public string Result { get; set; }

        
        
        #region Nested classes

        public class OrderProductVariantModel : BaseNopEntityModel
        {
            public int ProductId { get; set; }

            public string ProductName { get; set; }

            public string ProductSeName { get; set; }

            public string AttributeInfo { get; set; }

            public string UnitPrice { get; set; }

            public int Quantity { get; set; }

            public IList<SelectListItem> SelectListProductQuantity { get; set; }

            public PictureModel Picture { get; set; }

            public string Sku { get; set; }

            public string ManufacturerName { get; set; }

            public OrderProductVariantModel()
            {
                SelectListProductQuantity= new List<SelectListItem>();
            }
        }

        #endregion
    }

}