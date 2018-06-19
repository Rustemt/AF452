using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using Nop.Web.Models.Checkout;

namespace Nop.Web.Validators.Common
{
    public class CCPaymentInfoValidator : AbstractValidator<CCPaymentInfoModel>
    {
        public CCPaymentInfoValidator(ILocalizationService localizationService)
        {
            //useful links:
            //http://fluentvalidation.codeplex.com/wikipage?title=Custom&referringTitle=Documentation&ANCHOR#CustomValidator
            //http://benjii.me/2010/11/credit-card-validator-attribute-for-asp-net-mvc-3/

            RuleFor(x => x.CardholderName).NotEmpty().WithMessage(localizationService.GetResource("Payment.CardholderName.Required"));
            
            RuleFor(x => x.CardNumber).NotEmpty().WithMessage(localizationService.GetResource("Payment.CardNumber.Required"));
            RuleFor(x => x.CardNumber).Matches(@"^[0-9]{15,16}$").WithMessage(localizationService.GetResource("Payment.CardNumber.Wrong"));
            RuleFor(x => x.CardNumber).IsCreditCard().WithMessage(localizationService.GetResource("Payment.CardNumber.Wrong"));

            RuleFor(x => x.CardCode).NotEmpty().WithMessage(localizationService.GetResource("Payment.CardCode.Required"));
            RuleFor(x => x.CardCode).Matches(@"^[0-9]{3,4}$").WithMessage(localizationService.GetResource("Payment.CardCode.Wrong"));
        }}
}