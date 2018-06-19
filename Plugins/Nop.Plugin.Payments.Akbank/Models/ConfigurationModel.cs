using System.ComponentModel;
using System.Web.Mvc;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Payments.Akbank.Models
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

        [DisplayName("3D Client ID")]
        public string _3dclientid { get; set; }
        [DisplayName("3D Store Key")]
        public string _3dStoreKey { get; set; }
        [DisplayName("3D İslem Tipi")]
        public string _3dislemtipi { get; set; }
        [DisplayName("3D Store Type")]
        public string _3dstoretype { get; set; }
        [DisplayName("3D Url")]
        public string _3durl { get; set; }
        [DisplayName("3D Ok Url")]
        public string _3dokUrl { get; set; }
        [DisplayName("3D Fail Url")]
        public string _3dfailUrl { get; set; }
    }
}