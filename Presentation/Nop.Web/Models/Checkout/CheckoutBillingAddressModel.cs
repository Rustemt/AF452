using System.Collections.Generic;
using Nop.Web.Framework.Mvc;
using Nop.Web.Models.Common;
using System.Web.Mvc;

namespace Nop.Web.Models.Checkout
{
    public class CheckoutBillingAddressModel : BaseNopModel
    {
        public CheckoutBillingAddressModel()
        {
            ExistingAddresses = new List<AddressModel>();
            NewAddress = new AddressModel();
            BillingAddressActions = new List<SelectListItem>();
        }

        public IList<AddressModel> ExistingAddresses { get; set; }

        public AddressModel NewAddress { get; set; }

        public IList<SelectListItem> BillingAddressActions { get; set; }

        public bool HasDefault { get; set; }
    }
}