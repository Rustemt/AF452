using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Models.Common;

namespace Nop.Web.Validators.Common
{
    public class PriceOfferValidator : AbstractValidator<PriceOfferModel>
    {
        public PriceOfferValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage(localizationService.GetResource("ContactUs.Email.Required"));
            RuleFor(x => x.Email).EmailAddress().WithMessage(localizationService.GetResource("Common.WrongEmail"));
            RuleFor(x => x.FirstName).NotEmpty().WithMessage(localizationService.GetResource("ContactUs.FirstName.Required"));
            //RuleFor(x => x.LastName).NotEmpty().WithMessage(localizationService.GetResource("ContactUs.LastName.Required"));
            // RuleFor(x => x.Phone).NotEmpty().WithMessage(localizationService.GetResource("ContactUs.Phone.Required")).Length(10,999).WithMessage(localizationService.GetResource("ContactUs.Phone.Required"));
        }}

}