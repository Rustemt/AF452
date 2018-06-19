using System.Web.Mvc;
using FluentValidation.Attributes;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using Nop.Web.Validators.Common;
using System.Collections.Generic;

namespace Nop.Web.Models.Common
{
    //AF
    [Validator(typeof(CustomerSupportValidator))]
    public class CustomerSupportModel : BaseNopModel
    {
        [AllowHtml]
        [NopResourceDisplayName("CustomerSupport.Email")]
        public string Email { get; set; }

        [AllowHtml]
        [NopResourceDisplayName("CustomerSupport.FirstName")]
        public string FirstName { get; set; }
        [AllowHtml]
        [NopResourceDisplayName("CustomerSupport.LastName")]
        public string LastName { get; set; }

        [AllowHtml]
        [NopResourceDisplayName("CustomerSupport.SubjectExplanation")]
        public string Explanation { get; set; }

        [NopResourceDisplayName("CustomerSupport.Fields.Subject")]
        public string Subject { get; set; }

        public List<SelectListItem> Subjects { get; set; }

    }
}