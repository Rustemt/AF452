using System.Collections.Generic;
using System.Web.Mvc;
using Nop.Web.Framework.Mvc;
using Nop.Web.Models.Media;
using Nop.Web.Models.News;

namespace Nop.Web.Models.Catalog
{
    public abstract class ProductListModel : BaseNopEntityModel
    {
        public ProductListModel()
        {
            PictureModel = new PictureModel();
            FeaturedProducts = new List<ProductModel>();
            Products = new List<ProductModel>();
            AllProducts = new List<ProductModel>();
            PagingFilteringContext = new CatalogExtendedPagingFilteringModel();
            AvailableSortOptions = new List<SelectListItem>();
            AvailableViewModes = new List<SelectListItem>();
            ProductAttributeModels = new List<AttributeModel>();
            NewsItemModel = new NewsItemModel();
        }

        public string Name { get; set; }

        public string Description { get; set; }

        public string MetaKeywords { get; set; }

        public string MetaDescription { get; set; }

        public string MetaTitle { get; set; }

        public string SeName { get; set; }

        public int NewsItemStartPoint { get; set; }

        public NewsItemModel NewsItemModel { get; set; }
        public PictureModel PictureModel { get; set; }
        public CatalogExtendedPagingFilteringModel PagingFilteringContext { get; set; }
        public bool AllowProductFiltering { get; set; }
        public IList<SelectListItem> AvailableSortOptions { get; set; }
        public bool AllowProductViewModeChanging { get; set; }
        public IList<SelectListItem> AvailableViewModes { get; set; }

        public IList<ProductModel> FeaturedProducts { get; set; }
        public IList<ProductModel> Products { get; set; }
        public IList<ProductModel> AllProducts { get; set; }
        public IList<AttributeModel> ProductAttributeModels { get; set; }
    }

    public class RecentlyAddedProductsModel : ProductListModel
    {
        public bool IsGuest { get; set; }
    }
    public class RecentlyAddedProductsModel280 : ProductListModel280
    {
        public bool IsGuest { get; set; }
        public int RecentlyAddedTotalProduct { get; set; }
    }

    public abstract class ProductListModel280 : BaseNopEntityModel
    {
        public ProductListModel280()
        {
            PictureModel = new PictureModel();
            Products = new List<ProductOverviewModel280>();
            PagingFilteringContext = new CatalogPagingFilteringModel280();
        }
         
        public string Name { get; set; }
        public string Description { get; set; }
        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }
        public string MetaTitle { get; set; }
        public string SeName { get; set; }

        public int NewsItemStartPoint { get; set; }
        public PictureModel PictureModel { get; set; }
        public NewsItemModel NewsItemModel { get; set; }
        public CatalogPagingFilteringModel280 PagingFilteringContext { get; set; }
        public IList<ProductOverviewModel280> Products { get; set; }
        public bool IsGuest { get; set; }
        public int RecentlyAddedTotalProduct { get; set; }
    }

}