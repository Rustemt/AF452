using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.Akbank
{
    public class AkbankPaymentSettings : ISettings
    {
        public TransactMode TransactMode { get; set; }
        public bool UseTestServer { get; set; }
        public string ServiceUrl { get; set; }
        public string TestServiceUrl { get; set; }
        public string ClientId { get; set; }
        public string TestClientId { get; set; }
        public string Name { get; set; }
        public string TestName { get; set; }
        public string Password { get; set; }
        public string TestPassword { get; set; }
        public bool TestOrder { get; set; }


        public string _3dokUrl { get; set; }
        public string _3dfailUrl { get; set; }
        public string _3dStoreKey { get; set; }

        public string _3dclientid { get; set; }
        public string _3dislemtipi { get; set; }
        public string _3dstoretype { get; set; }
        public string _3durl { get; set; }
    }
}
