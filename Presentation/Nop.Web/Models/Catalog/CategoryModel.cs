using System.Collections.Generic;
using System.Web.Mvc;
using Nop.Web.Framework.Mvc;
using Nop.Web.Models.Media;
using Nop.Web.Models.News;

namespace Nop.Web.Models.Catalog
{
    public class CategoryModel : ProductListModel
    {
        public CategoryModel():base()
        {
            ManufacturerModels = new List<ManufacturerModel>();
            SubCategories = new List<SubCategoryModel>();
            CategoryBreadcrumb = new List<CategoryModel>();
        }

        public bool Selected { get; set; }

        public IList<ManufacturerModel> ManufacturerModels { get; set; }
        public IList<CategoryModel> CategoryBreadcrumb { get; set; }
        public IList<SubCategoryModel> SubCategories { get; set; }
        public bool DisplayCategoryBreadcrumb { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsGuest { get; set; }

		#region Nested Classes
        
        public class SubCategoryModel : BaseNopEntityModel
        {
            public SubCategoryModel()
            {
                PictureModel = new PictureModel();
            }

            public string Name { get; set; }

            public string SeName { get; set; }

            public PictureModel PictureModel { get; set; }
        }

		#endregion
    }


    public partial class CategoryProductsModel280 : ProductListModel280
    {
        public CategoryProductsModel280()
        {
            SubCategories = new List<SubCategoryModel280>();
            CategoryBreadcrumb = new List<CategoryModel>();
        }

        public bool DisplayCategoryBreadcrumb { get; set; }
        public IList<CategoryModel> CategoryBreadcrumb { get; set; }
        public IList<SubCategoryModel280> SubCategories { get; set; }

        #region Nested Classes

        public partial class SubCategoryModel280 : BaseNopEntityModel
        {
            public SubCategoryModel280()
            {
                PictureModel = new PictureModel();
            }

            public string Name { get; set; }

            public string SeName { get; set; }

            public PictureModel PictureModel { get; set; }
        }

        #endregion
    }

}