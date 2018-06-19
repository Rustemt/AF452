using Nop.Web.Framework.Mvc;

namespace Nop.Web.Models.Newsletter
{
    public class SubscriptionActivationModel : BaseNopModel
    {
        public string Result { get; set; }
        public string RegistrationType { get; set; }
    }
}