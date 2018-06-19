using System.Collections.Generic;
using System.Web.Mvc;
using Nop.Web.Framework.Mvc;
using Nop.Web.Models.Media;

namespace Nop.Web.Models.Catalog
{
    public class ManufacturerModel : ProductListModel
    {
        public bool DisplayCategoryBreadcrumb { get; set; }
        public IList<CategoryModel> CategoryBreadcrumb { get; set; }
        public bool IsGuest { get; set; }

        public ManufacturerModel()
        {
            CategoryBreadcrumb = new List<CategoryModel>();
        }

    }

    public partial class ManufacturerProductsModel280 : ProductListModel280
    {
        public ManufacturerProductsModel280()
        {
            //SubCategories = new List<SubCategoryModel280>();
            CategoryBreadcrumb = new List<CategoryModel>();
        }

        public bool DisplayCategoryBreadcrumb { get; set; }
        public IList<CategoryModel> CategoryBreadcrumb { get; set; }
        //public IList<SubCategoryModel280> SubCategories { get; set; }

        #region Nested Classes

        //public partial class SubCategoryModel280 : BaseNopEntityModel
        //{
        //    public SubCategoryModel280()
        //    {
        //        PictureModel = new PictureModel();
        //    }

        //    public string Name { get; set; }

        //    public string SeName { get; set; }

        //    public PictureModel PictureModel { get; set; }
        //}

        #endregion
    }
}