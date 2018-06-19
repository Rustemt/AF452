using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Models.Common;

namespace Nop.Web.Validators.Common
{
    public class CustomerSupportValidator:AbstractValidator<CustomerSupportModel>
    {
        public CustomerSupportValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.FirstName).NotEmpty().WithMessage(localizationService.GetResource("ContactUs.FirstName.Required"));
            RuleFor(x => x.LastName).NotEmpty().WithMessage(localizationService.GetResource("ContactUs.Lastname.Required"));
            RuleFor(x => x.Email).NotEmpty().WithMessage(localizationService.GetResource("ContactUs.Email.Required"));
            RuleFor(x => x.Email).EmailAddress().WithMessage(localizationService.GetResource("Common.WrongEmail"));
            RuleFor(x => x.Explanation).NotEmpty().WithMessage(localizationService.GetResource("ContactUs.Enquiry.Required"));
            RuleFor(x => x.Subject).NotEqual("").WithMessage(localizationService.GetResource("CustomerSupport.Subject.Select.Required"));
        }
    }
}