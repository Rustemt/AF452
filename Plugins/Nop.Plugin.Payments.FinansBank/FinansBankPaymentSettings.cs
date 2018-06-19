using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.FinansBank
{
    public class FinansBankPaymentSettings : ISettings
    {
        public TransactMode TransactMode { get; set; }
        public bool UseTestServer { get; set; }
        public bool TestOrder { get; set; }
        
        public string ServiceUrl { get; set; }
        public string TestServiceUrl { get; set; }
        
        public string ClientId { get; set; }
        public string TestClientId { get; set; }
        
        public string Name { get; set; }
        public string TestName { get; set; }
        
        public string Password { get; set; }
        public string TestPassword { get; set; }


        public string StoreKey { get; set; }
        public string SuccessURL { get; set; }
        public string ErrorURL { get; set; }
        public string HostAddress3D { get; set; }// https://netpos.finansbank.com.tr/servlet/est3Dgate
        public string StoreType { get; set; }//3d, 3d_pay, ...
        public string StoreName { get; set; }
        
      

    }
}
