﻿using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Models.Customer;
using Nop.Core.Domain.Customers;

namespace Nop.Web.Validators.Customer
{
    public class LoginValidator : AbstractValidator<LoginModel>
    {
        public LoginValidator(ILocalizationService localizationService, CustomerSettings customerSettings)
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage(localizationService.GetResource("Account.Login.Email.Required"));
            RuleFor(x => x.Email).EmailAddress().WithMessage(localizationService.GetResource("Common.WrongEmail"));
            RuleFor(x => x.Password).NotEmpty().WithMessage(localizationService.GetResource("Account.Login.Password.Required"));
            RuleFor(x => x.Password).Length(customerSettings.PasswordMinLength, 999).WithMessage(string.Format(localizationService.GetResource("Account.Fields.Password.LengthValidation"), customerSettings.PasswordMinLength));

        }
    }
}