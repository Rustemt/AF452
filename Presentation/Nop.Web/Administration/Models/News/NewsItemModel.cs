using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation.Attributes;
using Nop.Admin.Validators.News;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Localization;
using System.Collections.Generic;

namespace Nop.Admin.Models.News
{
    [Validator(typeof(NewsItemValidator))]
    public partial class NewsItemModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.Language")]
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.Language")]
        [AllowHtml]
        public string LanguageName { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.Title")]
        [AllowHtml]
        public string Title { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.Short")]
        [AllowHtml]
        public string Short { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.Full")]
        [AllowHtml]
        public string Full { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.AllowComments")]
        public bool AllowComments { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.Published")]
        public bool Published { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.Comments")]
        public int Comments { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [NopResourceDisplayName("Admin.Catalog.NewsItems.Fields.MetaKeywords")]
        [AllowHtml]
        public string MetaKeywords { get; set; }

        [NopResourceDisplayName("Admin.Catalog.NewsItems.Fields.MetaDescription")]
        [AllowHtml]
        public string MetaDescription { get; set; }

        [NopResourceDisplayName("Admin.Catalog.NewsItems.Fields.MetaTitle")]
        [AllowHtml]
        public string MetaTitle { get; set; }

        [NopResourceDisplayName("Admin.Catalog.NewsItems.Fields.SeName")]
        [AllowHtml]
        public string SeName { get; set; }

        [NopResourceDisplayName("Admin.Catalog.NewsItems.Fields.DisplayOrder")]
        [AllowHtml]
        public int DisplayOrder { get; set; }

        public IList<NewsItemLocalizedModel> Locales { get; set; }

        #region Nested classes

        public class NewsItemCategoryModel : BaseNopEntityModel
        {
            [NopResourceDisplayName("Admin.Catalog.Products.Categories.Fields.Category")]
            [UIHint("NewsItemCategory")]
            public string Category { get; set; }

            public int NewsItemId { get; set; }

            public int CategoryId { get; set; }

        }

        public class NewsItemManufacturerModel : BaseNopEntityModel
        {
            [NopResourceDisplayName("Admin.Catalog.Products.Manufacturers.Fields.Manufacturer")]
            [UIHint("NewsItemManufacturer")]
            public string Manufacturer { get; set; }

            public int NewsItemId { get; set; }

            public int ManufacturerId { get; set; }

        }

        #endregion

    }
    public class NewsItemLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.NewsItems.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Catalog.NewsItems.Fields.ShortDescription")]
        [AllowHtml]
        public string ShortDescription { get; set; }

        [NopResourceDisplayName("Admin.Catalog.NewsItems.Fields.FullDescription")]
        [AllowHtml]
        public string FullDescription { get; set; }

        [NopResourceDisplayName("Admin.Catalog.NewsItems.Fields.MetaKeywords")]
        [AllowHtml]
        public string MetaKeywords { get; set; }

        [NopResourceDisplayName("Admin.Catalog.NewsItems.Fields.MetaDescription")]
        [AllowHtml]
        public string MetaDescription { get; set; }

        [NopResourceDisplayName("Admin.Catalog.NewsItems.Fields.MetaTitle")]
        [AllowHtml]
        public string MetaTitle { get; set; }

        [NopResourceDisplayName("Admin.Catalog.NewsItems.Fields.SeName")]
        [AllowHtml]
        public string SeName { get; set; }
    }
}