using FluentValidation.Attributes;
using Nop.Web.Framework.Mvc;
using Nop.Web.Validators.Newsletter;
using Nop.Web.Framework;
using System.Collections.Generic;
using System.Web.Mvc;
//AF
namespace Nop.Web.Models.Newsletter
{
    [Validator(typeof(NewsletterDynastyValidator))]
    public class NewsletterDynastyModel : BaseNopModel
    {
        public string RootEmail { get; set; }
        
        [NopResourceDisplayName("Account.Fields.Email")]
        public string Email1 { get; set; }
        [NopResourceDisplayName("Account.Fields.Email")]
        public string Email2 { get; set; }
        [NopResourceDisplayName("Account.Fields.Email")]
        public string Email3 { get; set; }
        [NopResourceDisplayName("Account.Fields.Email")]
        public string Email4 { get; set; }
        [NopResourceDisplayName("Account.Fields.Email")]
        public string Email5 { get; set; }

       


    }

    
}