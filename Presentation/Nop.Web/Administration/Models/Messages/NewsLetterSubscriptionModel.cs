using System;
using System.Web.Mvc;
using FluentValidation.Attributes;
using Nop.Admin.Validators.Messages;
using Nop.Core.Domain.Localization;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Admin.Models.Messages
{
    [Validator(typeof(NewsLetterSubscriptionValidator))]
    public class NewsLetterSubscriptionModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.Promotions.NewsLetterSubscriptions.Fields.Email")]
        [AllowHtml]
        public string Email { get; set; }

        [NopResourceDisplayName("Admin.Promotions.NewsLetterSubscriptions.Fields.Active")]
        public bool Active { get; set; }

        [NopResourceDisplayName("Admin.Promotions.NewsLetterSubscriptions.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [NopResourceDisplayName("Admin.Promotions.NewsLetterSubscriptions.Fields.LanguageId")]
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.Polls.Fields.Language")]
        [AllowHtml]
        public string LanguageName { get; set; }
    }
}