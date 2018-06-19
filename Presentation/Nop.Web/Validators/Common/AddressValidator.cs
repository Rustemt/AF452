using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Models.Common;

namespace Nop.Web.Validators.Common
{
    public class AddressValidator : AbstractValidator<AddressModel>
    {
        public AddressValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.FirstName)
                .NotNull()
                .WithMessage(localizationService.GetResource("Address.Fields.FirstName.Required"));
            RuleFor(x => x.LastName)
                .NotNull()
                .WithMessage(localizationService.GetResource("Address.Fields.LastName.Required"));
            RuleFor(x => x.Name)
              .NotNull()
              .WithMessage(localizationService.GetResource("Address.Fields.Name.Required"));
            RuleFor(x => x.Address1)
               .NotNull()
               .WithMessage(localizationService.GetResource("Address.Fields.Address.Required"));
            RuleFor(x => x.City)
                .NotNull()
                .WithMessage(localizationService.GetResource("Address.Fields.City.Required"));
            RuleFor(x => x.CountryId)
                .NotNull()
                .WithMessage(localizationService.GetResource("Address.Fields.Country.Required"));
            RuleFor(x => x.PhoneNumber).NotEmpty()
              .WithMessage(localizationService.GetResource("Address.Fields.PhoneNumber.Required"));
            RuleFor(x => x.PhoneNumber).Length(10,999)
                .WithMessage(localizationService.GetResource("Address.Fields.PhoneNumber.Valid"));
            RuleFor(x => x.CivilNo)
                .Matches(@"^\d{11}$|^()$")
                .WithMessage(localizationService.GetResource("Address.Fields.CivilNo.Required"));
            //.When(x => !x.IsEnterprise);
           //RuleFor(x => x.Company)
           //    .NotNull()
           //    .WithMessage(localizationService.GetResource("Address.Fields.Company.Required"));
           //    //.When(x => x.IsEnterprise);
           //RuleFor(x => x.TaxNo)
           //    .NotNull()
           //    .WithMessage(localizationService.GetResource("Address.Fields.Tax.Required"));
           //    //.When(x => x.IsEnterprise);
           //RuleFor(x => x.TaxOffice)
           //    .NotNull()
           //    .WithMessage(localizationService.GetResource("Address.Fields.TaxOffice.Required"));
               //.When(x => x.IsEnterprise);

        }
    }
}