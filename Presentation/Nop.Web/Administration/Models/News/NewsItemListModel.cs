using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using Telerik.Web.Mvc;

namespace Nop.Admin.Models.News
{
    public class NewsItemListModel : BaseNopModel
    {
        public NewsItemListModel()
            {
                AvailableSystemTypeNames = new List<SelectListItem>();
            }

        public GridModel<NewsItemModel> NewsItems { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.List.SearchTitle")]
        public string SearchTitle { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.List.SystemTypeName")]
        public int SearchSystemTypeId { get; set; }


        public IList<SelectListItem> AvailableSystemTypeNames { get; set; }
       
    }
}