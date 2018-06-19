using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation.Attributes;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using Nop.Web.Validators.Common;

namespace Nop.Web.Models.Checkout
{
    [Validator(typeof(CCPaymentInfoValidator))]
    public class CCPaymentInfoModel : BaseNopModel
    {
        public CCPaymentInfoModel()
        {
            CreditCardTypes = new List<SelectListItem>();
            ExpireMonths = new List<SelectListItem>();
            ExpireYears = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Payment.SelectCreditCard")]
        [AllowHtml]
        public string CreditCardType { get; set; }
        [NopResourceDisplayName("Payment.SelectCreditCard")]
        public IList<SelectListItem> CreditCardTypes { get; set; }

        [NopResourceDisplayName("Payment.CardholderName")]
        [AllowHtml]
        public string CardholderName { get; set; }

        [NopResourceDisplayName("Payment.CardNumber")]
        [AllowHtml]
        public string CardNumber { get; set; }

        [NopResourceDisplayName("Payment.ExpirationDate")]
        [AllowHtml]
        public string ExpireMonth { get; set; }
        [NopResourceDisplayName("Payment.ExpirationDate")]
        [AllowHtml]
        public string ExpireYear { get; set; }
        public IList<SelectListItem> ExpireMonths { get; set; }
        public IList<SelectListItem> ExpireYears { get; set; }

        [NopResourceDisplayName("Payment.CardCode")]
        [AllowHtml]
        public string CardCode { get; set; }

        [NopResourceDisplayName("Payment.Installment")]
        [AllowHtml]
        public string Installment { get; set; }

        public IList<SelectListItem> Installments { get; set; }

        [NopResourceDisplayName("Payment.CCOption")]
        public string CCOption { get; set; }


    }
}