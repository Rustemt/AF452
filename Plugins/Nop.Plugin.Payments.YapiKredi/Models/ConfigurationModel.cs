using System.ComponentModel;
using System.Web.Mvc;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Payments.YapiKredi.Models
{
    public class ConfigurationModel : BaseNopModel
    {

        [DisplayName("Use Test Server")]
        public bool UseTestServer { get; set; }

        [DisplayName("Service Url")]
        public string ServiceUrl { get; set; }

        [DisplayName("Test Service Url")]
        public string TestServiceUrl { get; set; }

        [DisplayName("Merchant Id")]
        public string MerchantId { get; set; }

        [DisplayName("Test Merchant ID")]
        public string TestMerchantId { get; set; }

        [DisplayName("TerminalId")]
        public string TerminalId { get; set; }
        
        [DisplayName("Test TerminalId")]
        public string TestTerminalId { get; set; }

        [DisplayName("Password")]
        public string Password { get; set; }
        
        [DisplayName("Test Password")]
        public string TestPassword { get; set; }

        [DisplayName("TestOrder")]
        public bool TestOrder { get; set; }



        [DisplayName("HostAddress3D")]
        public string HostAddress3D { get; set; }
        
        [DisplayName("Return URL")]
        public string MerchantReturnURL { get; set; }

        [DisplayName("Vtf Kodu")]
        public string VftCode { get; set; }

        [DisplayName("PosnetId")]
        public string PosnetId { get; set; }

        [DisplayName("Enc Key")]
        public string EncKey { get; set; }

    }
}