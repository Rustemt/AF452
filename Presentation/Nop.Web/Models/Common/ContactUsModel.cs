using System.Web.Mvc;
using FluentValidation.Attributes;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using Nop.Web.Validators.Common;

namespace Nop.Web.Models.Common
{
    //AF
    [Validator(typeof(ContactUsValidator))]
    public class ContactUsModel : BaseNopModel
    {
        [AllowHtml]
        [NopResourceDisplayName("ContactUs.Email")]
        public string Email { get; set; }

        [AllowHtml]
        [NopResourceDisplayName("ContactUs.FirstName")]
        public string FirstName { get; set; }

        [AllowHtml]
        [NopResourceDisplayName("ContactUs.LastName")]
        public string LastName { get; set; }

        [AllowHtml]
        [NopResourceDisplayName("ContactUs.Phone")]
        public string Phone { get; set; }

        [AllowHtml]
        [NopResourceDisplayName("ContactUs.Subject")]
        public string Subject { get; set; }

        [AllowHtml]
        [NopResourceDisplayName("ContactUs.Enquiry")]
        public string Enquiry { get; set; }

        [AllowHtml]
        [NopResourceDisplayName("ContactUs.DisplayCaptcha")]
        public bool DisplayCaptcha { get; set; }
    }
}