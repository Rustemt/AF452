using System;
using System.Collections.Generic;
using FluentValidation.Attributes;
using Nop.Web.Framework.Mvc;
using Nop.Web.Validators.News;
using Nop.Web.Models.Media;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Common;

namespace Nop.Web.Models.News
{
    [Validator(typeof(NewsItemValidator))]
    public class NewsItemModel : BaseNopEntityModel
    {
        public NewsItemModel()
        {
            Tags = new List<string>();
            Comments = new List<NewsCommentModel>();
            AddNewComment = new AddNewsCommentModel();
            PictureModels = new List<PictureModel>();
            ProductModels = new List<ProductModel>();
            ExtraContentModels = new List<ExtraContentModel>(); 
        }

        public string SeName { get; set; }

        public string Title { get; set; }

        public string Short { get; set; }

        public string Full { get; set; }

        public bool AllowComments { get; set; }

        public int NumberOfComments { get; set; }
        public int PreviousNewsItemId { get; set; }
        public string PreviousNewsItemSeName { get; set; }
        public string PreviousNewsItemPictureUrl { get; set; }
        public int NextNewsItemId { get; set; }
        public string NextNewsItemSeName { get; set; }
        public string NextNewsItemPictureUrl { get; set; }
        public int SystemType { get; set; }
        public string MetaKeywords { get; set; }
        public bool IsGuest { get; set; }

        public string MetaDescription { get; set; }

        public string MetaTitle { get; set; }

        public int DisplayOrder { get; set; }


        public DateTime CreatedOn { get; set; }

        public IList<string> Tags { get; set; }

        public IList<NewsCommentModel> Comments { get; set; }
        public AddNewsCommentModel AddNewComment { get; set; }
        public IList<PictureModel> PictureModels { get; set; }
        public PictureModel DefaultPictureModel { get; set; }

        public IList<ProductModel> ProductModels { get; set; }
        public IList<ExtraContentModel> ExtraContentModels { get; set; }
    }
}