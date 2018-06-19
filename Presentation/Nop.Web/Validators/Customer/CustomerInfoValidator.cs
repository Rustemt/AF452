using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Models.Customer;
using Nop.Core.Domain.Customers;

namespace Nop.Web.Validators.Customer
{
    public class CustomerInfoValidator : AbstractValidator<CustomerInfoModel>
    {
        public CustomerInfoValidator(ILocalizationService localizationService, CustomerSettings customerSettings)
        {
            RuleFor(x => x.FirstName).NotEmpty().WithMessage(localizationService.GetResource("Account.Fields.FirstName.Required"));
            RuleFor(x => x.LastName).NotEmpty().WithMessage(localizationService.GetResource("Account.Fields.LastName.Required"));
            //Zorunluluk Kalırıldı mail
            //RuleFor(x => x.Email).NotEmpty().WithMessage(localizationService.GetResource("Account.Fields.Email.Required"));
            RuleFor(x => x.Email).EmailAddress().WithMessage(localizationService.GetResource("Common.WrongEmail"));
            RuleFor(x => x.NewEmail).EmailAddress().WithMessage(localizationService.GetResource("Common.WrongEmail"));
            //RuleFor(x => x.NewEmail).NotEmpty().WithMessage(localizationService.GetResource("Account.Fields.Email.Required"));
            RuleFor(x => x.ConfirmNewEmail).Equal(x => x.NewEmail).WithMessage(localizationService.GetResource("Account.NewEmail.EnteredEmailsDoNotMatch"));
            RuleFor(x => x.ConfirmNewPassword).Equal(x => x.NewPassword).WithMessage(localizationService.GetResource("Account.ChangePassword.Fields.NewPassword.EnteredPasswordsDoNotMatch"));
            //RuleFor(x => x.Password).NotEmpty().When(x => x.NewPassword != null ).WithMessage(localizationService.GetResource("Account.Fields.OldPassword.Required"));
            //Remove to check whether user wants to change password
            //RuleFor(x => x.NewPassword).Length(customerSettings.PasswordMinLength, 999).WithMessage(string.Format(localizationService.GetResource("Account.Fields.Password.LengthValidation"), customerSettings.PasswordMinLength));
            RuleFor(x => x.NewPassword).Matches("(.{6,})|(^[0-9]?$)").WithMessage(string.Format(localizationService.GetResource("Account.Fields.Password.LengthValidation"), customerSettings.PasswordMinLength));

           
           
        }}
}