using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using Nop.Web.Models.Media;

namespace Nop.Web.Models.Catalog
{
    public class ProductModel : BaseNopEntityModel
    {

        public ProductModel()
        {
            ProductPrice = new ProductPriceModel();
            DefaultPictureModel = new PictureModel();
            PictureModels = new List<PictureModel>();
            ProductVariantModels = new List<ProductVariantModel>();
            SpecificationAttributeModels = new List<ProductSpecificationModel>();
            AttributeSelectionModel = new List<AttributeModel>();
             

            
        }
        

        //public ProductModel(Product entity)
        //    : this()
        //{
        //    Entity = entity;
        //}

        public string Name { get; set; }

        public int ManufacturerId { get; set; }
        public string Manufacturer { get; set; }
        public string ManufacturerSeName { get; set; }
        public int ManufacturerOrder { get; set; }
        public List<ProductManufacturerModel> ProductManufacturers { get; set; }

        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int CategoryOrder { get; set; }
        public List<ProductCategoryModel> ProductCategories { get; set; }


        public string ShortDescription { get; set; }

        public string FullDescription { get; set; }

        public string MetaKeywords { get; set; }

        public string MetaDescription { get; set; }

        public string MetaTitle { get; set; }

        public string SeName { get; set; }



        //price
        public ProductPriceModel ProductPrice { get; set; }
        
        public bool IsGuest { get; set; }
        //picture(s)
        public bool DefaultPictureZoomEnabled { get; set; }
        public PictureModel DefaultPictureModel { get; set; }
        public ProductVariantModel DefaultVariantModel { get; set; }
        public IList<PictureModel> PictureModels { get; set; }
        public IList<ProductVariantModel> ProductVariantModels { get; set; }
        public IList<ProductSpecificationModel> SpecificationAttributeModels { get; set; }
        public IList<AttributeModel> AttributeSelectionModel { get; set; }
        public ProductModel PreviousProduct { get; set; }
        public ProductModel NextProduct { get; set; }

        public DateTime CreatedOnUtc { get; set; }

 
		#region Nested Classes

        public class ProductPriceModel : BaseNopModel
        {
            public string OldPrice { get; set; }

            public string Price {get;set;}

            public bool DisableBuyButton { get; set; }

            public bool CallForPrice { get; set; }

            public decimal PriceValue { get; set; }
        }

        public class ProductBreadcrumbModel : BaseNopModel
        {
            public ProductBreadcrumbModel()
            {
                CategoryBreadcrumb = new List<CategoryModel>();
            }

            public int ProductId { get; set; }
            public string ProductName { get; set; }
            public string ProductSeName { get; set; }
            public bool DisplayBreadcrumb { get; set; }
            public IList<CategoryModel> CategoryBreadcrumb { get; set; }
            public int PreviousProductId { get; set; }
            public string PreviousProductSeName { get; set; }
            public int NextProductId { get; set; }
            public string NextProductSeName { get; set; }
            public string PreviousListUrl { get; set; }

        }
        
        public class ProductVariantModel : BaseNopEntityModel
        {
            public ProductVariantModel()
            {
                GiftCard = new GiftCardModel();
                ProductVariantPrice = new ProductVariantPriceModel();
                DefaultPictureModel = new PictureModel();
                AddToCart = new AddToCartModel();
                ProductVariantAttributes = new List<ProductVariantAttributeModel>();
                PictureModels = new List<PictureModel>();
                AttributeValueIds = new List<int>();
                AttributeValueIdCombinations = new List<IList<int>>(); 
                Active = true;
            }
            //public ProductVariant Entity { get; set; } 

            public string Name { get; set; }

            public bool ShowSku { get; set; }

            public string Sku { get; set; }

            public string Description { get; set; }

            public bool ShowManufacturerPartNumber { get; set; }

            public string ManufacturerPartNumber { get; set; }

            public string DownloadSampleUrl { get; set; }

            public GiftCardModel GiftCard { get; set; }

            public string StockAvailablity { get; set; }

            public int Stock { get; set; }

            public ProductVariantPriceModel ProductVariantPrice { get; set; }

            public AddToCartModel AddToCart { get; set; }

            public PictureModel DefaultPictureModel { get; set; }
            public IList<PictureModel> PictureModels { get; set; }
            public int OrderMaximumQuantity { get; set; }

            public IList<ProductVariantAttributeModel> ProductVariantAttributes { get; set; }
            public IList<IList<int>> AttributeValueIdCombinations { get; set; }
            public IList<int> AttributeValueIds { get; set; }
            public bool Active { get; set; }
            public bool HidePriceIfCallforPrice { get; set; }
            public bool HideDiscount { get; set; }
            public int CallForPriceMessageTemplateId { get; set; }

            public int ManageInventoryMethodId { get; set; }

            #region Nested Classes

            public class AddToCartModel : BaseNopModel
            {
                public int ProductVariantId { get; set; }

                [NopResourceDisplayName("Products.Qty")]
                public int EnteredQuantity { get; set; }

                [NopResourceDisplayName("Products.EnterProductPrice")]
                public bool CustomerEntersPrice { get; set; }
                [NopResourceDisplayName("Products.EnterProductPrice")]
                public decimal CustomerEnteredPrice { get; set; }
                public String CustomerEnteredPriceRange { get; set; }
                
                public bool DisableBuyButton { get; set; }
                public bool DisableWishlistButton { get; set; }
            }

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

            public class GiftCardModel : BaseNopModel
            {
                public bool IsGiftCard { get; set; }

                [NopResourceDisplayName("Products.GiftCard.RecipientName")]
                [AllowHtml]
                public string RecipientName { get; set; }
                [NopResourceDisplayName("Products.GiftCard.RecipientEmail")]
                [AllowHtml]
                public string RecipientEmail { get; set; }
                [NopResourceDisplayName("Products.GiftCard.SenderName")]
                [AllowHtml]
                public string SenderName { get; set; }
                [NopResourceDisplayName("Products.GiftCard.SenderEmail")]
                [AllowHtml]
                public string SenderEmail { get; set; }
                [NopResourceDisplayName("Products.GiftCard.Message")]
                [AllowHtml]
                public string Message { get; set; }

                public GiftCardType GiftCardType { get; set; }
            }

            public class TierPriceModel : BaseNopModel
            {
                public string Price { get; set; }

                public int Quantity { get; set; }
            }

            public class ProductVariantAttributeModel : BaseNopEntityModel
            {
                public ProductVariantAttributeModel()
                {
                    Values = new List<ProductVariantAttributeValueModel>();
                }

                public int ProductVariantId { get; set; }

                public int ProductAttributeId { get; set; }

                public string Name { get; set; }

                public string Description { get; set; }

                public string TextPrompt { get; set; }

                public bool IsRequired { get; set; }

                public AttributeControlType AttributeControlType { get; set; }

                public IList<ProductVariantAttributeValueModel> Values { get; set; }

            }

            public class ProductVariantAttributeValueModel : BaseNopEntityModel
            {
                public string Name { get; set; }

                public string PriceAdjustment { get; set; }

                public decimal PriceAdjustmentValue { get; set; }

                public bool IsPreSelected { get; set; }

                public int ProductAttributeOptionId { get; set; }
            }
            #endregion
        }

        public class ProductCategoryModel
        {
            public int Id {get;set;}
            public string Name { get; set; }
            public int DisplayOrder { get; set; }
            public string Sename { get; set; }
        }
        public class ProductManufacturerModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int DisplayOrder { get; set; }
            public string Sename { get; set; }
        }
		#endregion
    }
}