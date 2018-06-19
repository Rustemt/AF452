using System.Collections.Generic;
using System.Web.Mvc;
using Nop.Web.Framework.Mvc;
using Nop.Web.Models.Media;

namespace Nop.Web.Models.Catalog
{
    //AF
    public class CategoryManufacturerModel : BaseNopEntityModel
    {
        public IList<ManufacturerModel> Manufacturers { get; set; }
        public CategoryModel Category { get; set; }
        public bool IsGuest { get; set; }
        public CategoryManufacturerModel()
        {
            Manufacturers = new List<ManufacturerModel>();
            Category = new CategoryModel();
        }

    }
}