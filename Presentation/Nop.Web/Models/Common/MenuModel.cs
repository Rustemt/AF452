using Nop.Web.Framework.Mvc;
using System.Collections;
using System.Collections.Generic;

namespace Nop.Web.Models.Common
{
    public class MenuModel : List<HeaderItem>
    {
        
    }
    public class CategoryMenuModel
    {
        public IList<SubHeaderItem> SubHeaders { get; set; }
        public IList<MenuItem> Brands { get; set; }
        public ContentItemModel ContentItem { get; set; }
        public bool HasMenu { get; set; }
        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }
        public string MetaTitle { get; set; }
        public string Name { get; set; }
        public string SeName { get; set; }
        public bool IsGuest { get; set; }
        public CategoryMenuModel()
        {
            SubHeaders = new List<SubHeaderItem>();
            Brands = new List<MenuItem>();
        }
    }

    public class HeaderItem : MenuItem
    {
        public IList<SubHeaderItem> SubHeaders { get; set; }
        public IList<MenuItem> Brands { get; set; }
        public ContentItemModel ContentItem { get; set; }
        public MenuItem AllBrands { get; set; }
        public bool HasMenu { get; set; }


        public HeaderItem()
        {
            SubHeaders = new List<SubHeaderItem>();
            Brands = new List<MenuItem>();
        }

    }
    public class SubHeaderItem : MenuItem
    {
        public IList<MenuItem> Items { get; set; }
        public bool HasMore { get; set; }
    }

    public class MenuItem : BaseNopModel
    {
        public string Name { get; set; }

        public string Url { get; set; }

        public string Title { get; set; }

    }

}