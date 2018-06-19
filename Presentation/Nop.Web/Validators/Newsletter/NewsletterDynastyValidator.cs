using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Models.Newsletter;

//AF
namespace Nop.Web.Validators.Newsletter
{
    public class NewsletterDynastyValidator : AbstractValidator<NewsletterDynastyModel>
    {
        public NewsletterDynastyValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.RootEmail).NotEmpty().WithMessage(localizationService.GetResource("ContactUs.Email.Required"));
            RuleFor(x => x.RootEmail).EmailAddress().WithMessage(localizationService.GetResource("Common.WrongEmail"));
            RuleFor(x => x.Email1).EmailAddress().WithMessage(localizationService.GetResource("Common.WrongEmail"));
            RuleFor(x => x.Email2).EmailAddress().WithMessage(localizationService.GetResource("Common.WrongEmail"));
            RuleFor(x => x.Email3).EmailAddress().WithMessage(localizationService.GetResource("Common.WrongEmail"));
            RuleFor(x => x.Email4).EmailAddress().WithMessage(localizationService.GetResource("Common.WrongEmail"));
            RuleFor(x => x.Email5).EmailAddress().WithMessage(localizationService.GetResource("Common.WrongEmail"));
        }
    }
}