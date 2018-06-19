using System.ComponentModel;
using System.Web.Mvc;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Payments.IsBank.Models
{
    public class ConfigurationModel : BaseNopModel
    {

        [DisplayName("Use Test Server")]
        public bool UseTestServer { get; set; }

        [DisplayName("Service Url")]
        public string ServiceUrl { get; set; }

        [DisplayName("Test Service Url")]
        public string TestServiceUrl { get; set; }

        [DisplayName("Client ID")]
        public string ClientId { get; set; }

        [DisplayName("Test Client ID")]
        public string TestClientId { get; set; }

        [DisplayName("Name")]
        public string Name { get; set; }
        
        [DisplayName("Test Name")]
        public string TestName { get; set; }

        [DisplayName("Password")]
        public string Password { get; set; }
        
        [DisplayName("Test Password")]
        public string TestPassword { get; set; }

        [DisplayName("TestOrder")]
        public bool TestOrder { get; set; }

        [DisplayName("Store Key")]
        public string StoreKey { get; set; }
         [DisplayName("SuccessURL")]
        public string SuccessURL { get; set; }
         [DisplayName("ErrorURL")]
        public string ErrorURL { get; set; }
         [DisplayName("HostAddress3D")]
        public string HostAddress3D { get; set; }// https://netpos.finansbank.com.tr/servlet/est3Dgate
         [DisplayName("StoreType")]
        public string StoreType { get; set; }//3d, 3d_pay, ...
         [DisplayName("StoreName")]
        public string StoreName { get; set; }

    }
}