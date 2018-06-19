using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Nop.Web.Framework.Mvc;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework;
using Nop.Admin.Models.Catalog;
using Telerik.Web.Mvc;
using Nop.Core.Domain.News;

namespace Nop.Admin.Models.News
{
    public partial class NewsItemModel : BaseNopEntityModel
    {
        
        [NopResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.SystemType")]
        public int SystemTypeId { get; set; }
        public NewsType SystemType { get; set; }
        public bool IsFeatured { get; set; }
        public string Url { get; set; }
        
        //pictures
        public NewsItemPictureModel AddPictureModel { get; set; }
        public IList<NewsItemPictureModel> NewsItemPictureModels { get; set; }
        public IList<SelectListItem> SystemTypes { get; set; }
        public IList<NewsItemExtraContentModel> NewsItemExtraContentModels { get; set; }
        public NewsItemModel()
        {
            SystemTypes = NewsType.News.ToSelectList(true).ToList();
            AvailableCategories = new List<SelectListItem>();
            AvailableManufacturers = new List<SelectListItem>();
        }


        //categories
        public int NumberOfAvailableCategories { get; set; }

        //manufacturers
        public int NumberOfAvailableManufacturers { get; set; }

        #region Nested classes
        
        public class NewsItemPictureModel : BaseNopEntityModel
        {
       
            public int NewsItemId { get; set; }

            [UIHint("Picture")]
            [NopResourceDisplayName("Admin.Catalog.Products.Pictures.Fields.Picture")]
            public int PictureId { get; set; }

            [NopResourceDisplayName("Admin.Catalog.Products.Pictures.Fields.Picture")]
            public string PictureUrl { get; set; }

            [NopResourceDisplayName("Admin.Catalog.Products.Pictures.Fields.DisplayOrder")]
            public int DisplayOrder { get; set; }

            [NopResourceDisplayName("Admin.News.Pictures.Type")]
            public int PictureTypeId { get; set; } 
            public NewsItemPictureType PictureType { get; set; }
           
 
            public IList<SelectListItem> NewsItemPictureTypes { get; set; }
              
            public NewsItemPictureModel()
        {
            NewsItemPictureTypes = NewsItemPictureType.Standard.ToSelectList(true).ToList();
        }

        }

        public class NewsItemProductModel : BaseNopEntityModel
        {
            public int NewsItemId { get; set; }

            public int ProductId { get; set; }

            [NopResourceDisplayName("Admin.Catalog.Manufacturers.Products.Fields.Product")]
            public string ProductName { get; set; }

            [NopResourceDisplayName("Admin.Catalog.Manufacturers.Products.Fields.IsFeaturedProduct")]
            public bool IsFeaturedProduct { get; set; }

            [NopResourceDisplayName("Admin.Catalog.Manufacturers.Products.Fields.DisplayOrder")]
            //we don't name it DisplayOrder because Telerik has a small bug 
            //"if we have one more editor with the same name on a page, it doesn't allow editing"
            //in our case it's category.DisplayOrder
            public int DisplayOrder1 { get; set; }
        }

        public class AddNewsItemProductModel : BaseNopModel
        {
            public AddNewsItemProductModel()
            {
                AvailableCategories = new List<SelectListItem>();
                AvailableManufacturers = new List<SelectListItem>();
            }
            public GridModel<ProductModel> Products { get; set; }

            [NopResourceDisplayName("Admin.Catalog.Products.List.SearchProductName")]
            [AllowHtml]
            public string SearchProductName { get; set; }
            [NopResourceDisplayName("Admin.Catalog.Products.List.SearchCategory")]
            public int SearchCategoryId { get; set; }
            [NopResourceDisplayName("Admin.Catalog.Products.List.SearchManufacturer")]
            public int SearchManufacturerId { get; set; }

            public IList<SelectListItem> AvailableCategories { get; set; }
            public IList<SelectListItem> AvailableManufacturers { get; set; }

            public int NewsItemId { get; set; }

            public int[] SelectedProductIds { get; set; }
        }
        public IList<SelectListItem> AvailableCategories { get; set; }
        public IList<SelectListItem> AvailableManufacturers { get; set; }
        #endregion

    }
}
