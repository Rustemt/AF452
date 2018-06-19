using System.Collections.Generic;
using Nop.Web.Framework.Mvc;
using Nop.Web.Models.Common;
using System.Linq;
using Nop.Web.Validators.Common;
using FluentValidation.Attributes;

namespace Nop.Web.Models.Checkout
{
    [Validator(typeof(CheckoutAddressValidator))]
    public class CheckoutAddressesModel : BaseNopModel
    {
        public CheckoutAddressesModel()
        {

        }
       
        public CheckoutBillingAddressModel CheckoutBillingAddressModel { get; set; }
        public CheckoutShippingAddressModel CheckoutShippingAddressModel { get; set; }
        public int? BillingAddressId { get; set; }
        public int? ShippingAddressId { get; set; }

    }
}