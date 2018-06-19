using System.Collections.Generic;
using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Web.Models.Catalog
{
    public class SearchModel : ProductListModel
    {
        public SearchModel()
        {
        }

        public string Warning { get; set; }

        /// <summary>
        /// Query string
        /// </summary>
        [NopResourceDisplayName("Search.SearchTerm")]
        [AllowHtml]
        public string Q { get; set; }
        /// <summary>
        /// Category ID
        /// </summary>
        [NopResourceDisplayName("Search.Category")]
        public int Cid { get; set; }
        /// <summary>
        /// Manufacturer ID
        /// </summary>
        [NopResourceDisplayName("Search.Manufacturer")]
        public int Mid { get; set; }
        /// <summary>
        /// Price - From 
        /// </summary>
        [AllowHtml]
        public string Pf { get; set; }
        /// <summary>
        /// Price - To
        /// </summary>
        [AllowHtml]
        public string Pt { get; set; }
        /// <summary>
        /// A value indicating whether to search in descriptions
        /// </summary>
        [NopResourceDisplayName("Search.SearchInDescriptions")]
        public bool Sid { get; set; }
        /// <summary>
        /// A value indicating whether to search in descriptions
        /// </summary>
        [NopResourceDisplayName("Search.AdvancedSearch")]
        public bool As { get; set; }

        /// <summary>
        /// Search Result Title, YOU SEARCHED FOR
        /// </summary>
        public string SearchReturnTitle { get; set; }
    }

    public class SearchProductsModel280 : ProductListModel280
    {
        public SearchProductsModel280()
        {
        }

        public string Warning { get; set; }

        /// <summary>
        /// Query string
        /// </summary>
        [NopResourceDisplayName("Search.SearchTerm")]
        [AllowHtml]
        public string Q { get; set; }
        /// <summary>
        /// Category ID
        /// </summary>
        [NopResourceDisplayName("Search.Category")]
        public int Cid { get; set; }
        /// <summary>
        /// Manufacturer ID
        /// </summary>
        [NopResourceDisplayName("Search.Manufacturer")]
        public int Mid { get; set; }
        /// <summary>
        /// Price - From 
        /// </summary>
        [AllowHtml]
        public string Pf { get; set; }
        /// <summary>
        /// Price - To
        /// </summary>
        [AllowHtml]
        public string Pt { get; set; }
        /// <summary>
        /// A value indicating whether to search in descriptions
        /// </summary>
        [NopResourceDisplayName("Search.SearchInDescriptions")]
        public bool Sid { get; set; }
        /// <summary>
        /// A value indicating whether to search in descriptions
        /// </summary>
        [NopResourceDisplayName("Search.AdvancedSearch")]
        public bool As { get; set; }

        /// <summary>
        /// Search Result Title, YOU SEARCHED FOR
        /// </summary>
        public string SearchReturnTitle { get; set; }
    }

}