using System.Collections.Generic;
using Nop.Web.Framework.Mvc;
using Nop.Web.Models.Common;

namespace Nop.Web.Models.Customer
{
    public class CustomerAddressListModel : BaseNopModel
    {
        public CustomerAddressListModel()
        {
            Addresses = new List<AddressModel>();
        }
        public AddressModel SelectedShippingAddressModel { get; set; }
        public AddressModel SelectedBillingAddressModel { get; set; }
        public AddressModel SelectedAddressModel { get; set; }
        public IList<AddressModel> Addresses { get; set; }
        public CustomerNavigationModel NavigationModel { get; set; }
    }
}