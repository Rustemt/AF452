using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Models.Newsletter;

//AF
namespace Nop.Web.Validators.Newsletter
{
    public class NewsletterBoxValidator : AbstractValidator<NewsletterBoxModel>
    {
        public NewsletterBoxValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.FirstName).NotEmpty().WithMessage(localizationService.GetResource("ContactUs.FirstName.Required"));
            RuleFor(x => x.LastName).NotEmpty().WithMessage(localizationService.GetResource("ContactUs.LastName.Required"));
            RuleFor(x => x.Email).NotEmpty().WithMessage(localizationService.GetResource("ContactUs.Email.Required"));
            RuleFor(x => x.Email).EmailAddress().WithMessage(localizationService.GetResource("Common.WrongEmail"));
            RuleFor(x => x.CampaignMail).EmailAddress().WithMessage(localizationService.GetResource("Common.WrongEmail"));
            RuleFor(x => x.CampaignMail).NotEmpty().WithMessage(localizationService.GetResource("ContactUs.Email.Required"));

            RuleFor(x => x.Email1).EmailAddress().WithMessage(localizationService.GetResource("Common.WrongEmail"));
            RuleFor(x => x.Email2).EmailAddress().WithMessage(localizationService.GetResource("Common.WrongEmail"));
            RuleFor(x => x.Email3).EmailAddress().WithMessage(localizationService.GetResource("Common.WrongEmail"));
            RuleFor(x => x.Email4).EmailAddress().WithMessage(localizationService.GetResource("Common.WrongEmail"));
            RuleFor(x => x.Email5).EmailAddress().WithMessage(localizationService.GetResource("Common.WrongEmail"));
           
        }
    }
}