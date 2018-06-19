using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nop.Web.Framework.Mvc;
using Nop.Web.Models.News;

namespace Nop.Web.Models.Common
{
    public enum ContentType
    {
        MainContent,
        PromoContent,
        CategoryMenuContent,
        CategoryHomeContent,
    }

    public class ContentItemModel : BaseNopModel
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string ImagePath { get; set; }
        public string Url { get; set; }
        public string Price { get; set; }
		public string MetaDescription { get; set; }
        public ContentType ContentType { get; set; }

        public ContentItemModel()
        {
            Title = string.Empty;
            Content = string.Empty;
            ImagePath = string.Empty;
            Url  = string.Empty;
            Price = string.Empty;
			MetaDescription = string.Empty;
        }

    }
    public class HomeModel : BaseNopModel
    {
        public List<ContentItemModel> MainContents { get; set; }
        public List<ContentItemModel> BottomContents { get; set; }

        public NewsItemModel Announcement{ get; set; }

        public bool IsGuest{ get; set; }
    }
}