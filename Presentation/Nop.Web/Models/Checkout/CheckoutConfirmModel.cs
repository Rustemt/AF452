using System.Collections.Generic;
using Nop.Web.Framework.Mvc;
using Nop.Web.Models.Common;

namespace Nop.Web.Models.Checkout
{
    //AF
    public class CheckoutConfirmModel : BaseNopModel
    {
        public CheckoutConfirmModel()
        {
            Warnings = new List<string>();
        }

        public string MinOrderTotalWarning { get; set; }

        public IList<string> Warnings { get; set; }
        public AddressModel BillingAddress { get; set; }
        public AddressModel ShippingAddress { get; set; }
        public bool ShowWireTransferData { get; set; }
        public bool IsPayment3DEnabled {get;set;}
        //public string Title = @T("Payment.3D.Title").Text;
        //public string Message = @T("Payment.3D.Message").Text;
        //public string LoadingContent = @T("Payment.3D.LoadingContent").Text;

    }
}