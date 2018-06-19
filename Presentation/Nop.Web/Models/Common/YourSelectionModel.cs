using Nop.Web.Framework.Mvc;
using System.Collections;
using System.Collections.Generic;
using Nop.Web.Models.Catalog;

namespace Nop.Web.Models.Common
{
    public class YourSelectionModel : BaseNopModel
    {
        public List<ManufacturerModel> Brands { get; set; }
        public List<YourSelectionSection> Sections { get; set; }
        public CatalogPagingFilteringModel CatalogPagingFilteringModel { get; set; }
        public YourSelectionModel()
        {
            Brands = new List<ManufacturerModel>();
            Sections = new List<YourSelectionSection>();
        }



        public class YourSelectionItem
        {
            public int Id { get; set; }
            public string Name { get; set; }

        }
        public class YourSelectionSection
        {
            public List<YourSelectionItem> items { get; set; }

            public YourSelectionSection()
            {
                items = new List<YourSelectionItem>();
            }
        }
    }





}