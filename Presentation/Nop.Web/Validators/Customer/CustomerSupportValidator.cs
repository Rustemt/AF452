using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Models.Customer;
using Nop.Web.Models.Common;

namespace Nop.Web.Validators.Customer
{
    public class CustomerSupportValidator : AbstractValidator<CustomerSupportModel>
    {
         public CustomerSupportValidator(ILocalizationService localizationService)
         {
             RuleFor(x => x.FirstName).NotEmpty().WithMessage(localizationService.GetResource("Account.Fields.FirstName.Required"));
             RuleFor(x => x.LastName).NotEmpty().WithMessage(localizationService.GetResource("Account.Fields.LastName.Required"));
             RuleFor(x => x.Subject).NotEmpty().WithMessage(localizationService.GetResource("Help.Subject.Required"));
             RuleFor(x => x.Explanation).NotEmpty().WithMessage(localizationService.GetResource("Help.Explanation.Required"));
             RuleFor(x => x.Email).NotEmpty().WithMessage(localizationService.GetResource("Help.Email.Required"));
             RuleFor(x => x.Email).EmailAddress().WithMessage(localizationService.GetResource("Common.Email.Wrong"));
         }
        
    }
}