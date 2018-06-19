using Nop.Web.Framework.Mvc;
using System.Collections.Generic;

namespace Nop.Web.Models.Checkout
{
    public class CheckoutProgressModel : BaseNopModel
    {
        public CheckoutProgressStep CheckoutProgressStep { get; set; }

        public IList<string> Warnings { get; set; }

        public string OrderTotal { get; set; }

        public CheckoutProgressModel()
        {
            Warnings = new List<string>();
        }
    }

    public enum CheckoutProgressStep
    {
        Cart,
        Address,
        Shipping,
        Payment,
        Confirm,
        Complete
    }

    public class CheckoutPayment3DResponse
    {
        public string UrlToRedirect{get;set;}
    }
}