using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nop.Web.Models.Catalog
{
    public class NewsItemProductsModel : ProductListModel
    {
        public NewsItemProductsModel()
        {
          
        }
        public string Title { get; set; }
        public string SeName { get; set; }
    }
}