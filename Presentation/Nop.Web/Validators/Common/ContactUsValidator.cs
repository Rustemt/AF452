using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Models.Common;

namespace Nop.Web.Validators.Common
{
    public class ContactUsValidator : AbstractValidator<ContactUsModel>
    {
        public ContactUsValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage(localizationService.GetResource("ContactUs.Email.Required"));
            RuleFor(x => x.Email).EmailAddress().WithMessage(localizationService.GetResource("Common.WrongEmail"));
            RuleFor(x => x.Enquiry).NotEmpty().WithMessage(localizationService.GetResource("ContactUs.Enquiry.Required"));
            RuleFor(x => x.Subject).NotEmpty().WithMessage(localizationService.GetResource("ContactUs.Subject.Required"));
            RuleFor(x => x.FirstName).NotEmpty().WithMessage(localizationService.GetResource("ContactUs.FirstName.Required"));
            RuleFor(x => x.LastName).NotEmpty().WithMessage(localizationService.GetResource("ContactUs.LastName.Required"));
        }}

}