using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.YapiKredi
{
    public class YapiKrediPaymentSettings : ISettings
    {
        public TransactMode TransactMode { get; set; }
        public bool UseTestServer { get; set; }
        public string ServiceUrl { get; set; }
        public string TestServiceUrl { get; set; }
        public string MerchantId { get; set; }
        public string TestMerchantId { get; set; }
        public string TerminalId { get; set; }
        public string TestTerminalId { get; set; }
        public string Password { get; set; }
        public string TestPassword { get; set; }
        public bool TestOrder { get; set; }
        public string HostAddress3D { get; set; }
        public string MerchantReturnURL { get; set; }
        public string VftCode { get; set; }
        public string PosnetId { get; set; }
        public string EncKey { get; set; }


    }
}
