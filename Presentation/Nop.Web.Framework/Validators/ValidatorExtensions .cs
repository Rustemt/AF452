using FluentValidation;
using FluentValidation.Mvc;
using System.Web.Mvc;
using FluentValidation.Internal;
using FluentValidation.Validators;
using System.Collections.Generic;

namespace Nop.Web.Framework.Validators
{
    public static class MyValidatorExtensions
    {
        public static IRuleBuilderOptions<T, string> IsCreditCard<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new CreditCardPropertyValidator());
        }
    }
    public class EqualToValueFluentValidationPropertyValidator : FluentValidationPropertyValidator
    {
        public EqualToValueFluentValidationPropertyValidator(ModelMetadata metadata, ControllerContext controllerContext, PropertyRule rule, IPropertyValidator validator)
            : base(metadata, controllerContext, rule, validator)
        {
        }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            if (!this.ShouldGenerateClientSideRules())
            {
                yield break;
            }
            var validator = (EqualValidator)Validator;

            var errorMessage = new MessageFormatter()
                .AppendPropertyName(Rule.GetDisplayName())
                .AppendArgument("ValueToCompare", validator.ValueToCompare)
                .BuildMessage(validator.ErrorMessageSource.GetString());

            var rule = new ModelClientValidationRule();
            rule.ErrorMessage = errorMessage;
            rule.ValidationType = "equaltovalue";
            rule.ValidationParameters["valuetocompare"] = validator.ValueToCompare;
            yield return rule;
        }
    }

}
