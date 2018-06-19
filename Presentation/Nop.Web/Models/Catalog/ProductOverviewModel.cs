using System.Collections.Generic;
using Nop.Web.Framework.Mvc;
using Nop.Web.Models.Media;

namespace Nop.Web.Models.Catalog
{
    public partial class ProductOverviewModel280 : BaseNopEntityModel
    {
        public ProductOverviewModel280()
        {
            ProductPrice = new ProductPriceModel280();
            DefaultPictureModel = new PictureModel();
            SpecificationAttributeModels = new List<ProductSpecificationModel280>();
        }

        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }
        public string Manufacturer { get; set; }
        public string SeName { get; set; }
        public int VariantId { get; set; }
        public int Stock { get; set; }
        public bool HideDiscount { get; set; }

        //price
        public ProductPriceModel280 ProductPrice { get; set; }
        //picture
        public PictureModel DefaultPictureModel { get; set; }
        //specification attributes
        public IList<ProductSpecificationModel280> SpecificationAttributeModels { get; set; }

		#region Nested Classes

        public partial class ProductPriceModel280 : BaseNopModel
        {
            public string OldPrice { get; set; }

            public string Price {get;set;}

            public string PriceWithDiscount { get; set; }

            public bool DisableBuyButton { get; set; }

            public bool AvailableForPreOrder { get; set; }

            public bool ForceRedirectionAfterAddingToCart { get; set; }
            
            public bool CallForPrice { get; set; }

            public bool HidePriceIfCallforPrice { get; set; }
            
        }

		#endregion
    }
}