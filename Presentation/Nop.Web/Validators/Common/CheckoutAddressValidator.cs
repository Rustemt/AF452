using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Models.Common;
using Nop.Web.Models.Checkout;

namespace Nop.Web.Validators.Common
{
    public class CheckoutAddressValidator : AbstractValidator<CheckoutAddressesModel>
    {
        public CheckoutAddressValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.BillingAddressId)
                .NotNull()
                .WithMessage(localizationService.GetResource("Checkout.BillingAddress.Required"));
            RuleFor(x => x.ShippingAddressId)
                 .NotNull()
                 .WithMessage(localizationService.GetResource("Checkout.ShippingAddress.Required"));
         
        }
    }
}