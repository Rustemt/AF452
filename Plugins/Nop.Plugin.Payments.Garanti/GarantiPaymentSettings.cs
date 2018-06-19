using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.Garanti
{
    public class GarantiPaymentSettings : ISettings
    {
        public string Mode { get; set; }
        public string Version { get; set; }
        public string TerminalId { get; set; }
        public string ProvisionUserId { get; set; }
        public string ProvisionPassword { get; set; }
        public string UserId { get; set; }
        public string MerchantId {get;set;}
        public string Type {get;set;}
        public string HostAddress { get; set; }
        public string MotoInd { get; set; }
        public string CardholderPresentCode { get; set; }
        public string StoreKey { get; set; }
        public string SuccessURL { get; set; }
        public string ErrorURL { get; set; }
        public string HostAddress3D { get; set; }// https://sanalposprov.garanti.com.tr/servlet/gt3dengine
        public string SecurityLevel3D { get; set; }

        public string TestMode { get; set; }
        public string TestVersion { get; set; }
        public string TestTerminalId { get; set; }
        public string TestProvisionUserId { get; set; }
        public string TestProvisionPassword { get; set; }
        public string TestUserId { get; set; }
        public string TestMerchantId { get; set; }
        public string TestType { get; set; }
        public string TestHostAddress { get; set; }
        public string TestHostAddress3D { get; set; } // http://sanalposprovtest.garanti.com.tr/servlet/gt3dengine
        public string TestMotoInd { get; set; }
        public string TestCardholderPresentCode { get; set; }
        public string TestStoreKey { get; set; }
        public string TestSuccessURL { get; set; }
        public string TestErrorURL { get; set; }
        public string TestSecurityLevel3D { get; set; }



       
        public bool TestOrder { get; set; }

    }
}
