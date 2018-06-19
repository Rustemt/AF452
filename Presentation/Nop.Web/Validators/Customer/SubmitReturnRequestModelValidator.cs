using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Models.Order;

namespace Nop.Web.Validators.Customer
{
    public class SubmitReturnRequestModelValidator : AbstractValidator<SubmitReturnRequestModel>
    {
        public SubmitReturnRequestModelValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Comments).NotEmpty().WithMessage(localizationService.GetResource("ReturnRequest.Comments.Required"));
        }}
}