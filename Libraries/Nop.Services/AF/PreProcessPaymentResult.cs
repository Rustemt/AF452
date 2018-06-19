using System.Collections.Generic;
using Nop.Core.Domain.Payments;
using System.Collections.Specialized;

namespace Nop.Services.Payments
{
    /// <summary>
    /// Represents a ProcessPaymentResult
    /// </summary>
    public partial class PreProcessPaymentResult
    {
        public IList<string> Errors { get; set; }

        public PreProcessPaymentResult() 
        {
            this.Errors = new List<string>();
        }

        public bool Success
        {
            get { return (this.Errors.Count == 0); }
        }

        public void AddError(string error)
        {
            this.Errors.Add(error);
        }
        /// <summary>
        /// Gets or sets an order. Used when order is already saved (payment gateways that redirect a customer to a third-party URL)
        /// </summary>
        /// 

        public bool RequiresRedirection { get; set; }

        public string RedirectURL { get; set; }
        public string QueryString { get; set; }
        public string ResponseContent { get; set; }

        public string StoreKey { get; set; }
        public string OkUrl { get; set; }
        public string FailUrl { get; set; }
        public string RandomToken { get; set; }
        public string Hash { get; set; }
        public string CardType { get; set; }
        public string Lang { get; set; }
        public string Currency { get; set; }
        public string StoreName {get; set;}

        public string ChargeType { get; set; }

        public string StoreType { get; set; }//3Dmodel (3d, 3d_pay, 3d_hosting)
        public string ClientId { get; set; }
        public string OrderId { get; set; }
        public string Amount { get; set; }
        public string Installment { get; set; }

        // -- GARANTIYE OZEL
        public string UserName { get; set; }
        public string MerchantId { get; set; }
        public string TerminalUserPass { get; set; }
        public string TerminalId { get; set; }
        public string TerminalUserId { get; set; }
        public string TerminalProvUserId { get; set; }
        public string ApiVersion { get; set; }
        public string CustomerEmailAddress { get; set; }
        public string CustomerIpAddress { get; set; }
        // -- GARANTIYE OZEL

        // -- YKB YE OZEL
        public string Mid { get; set; }
        public string PosnetID { get; set; }
        public string PosnetData { get; set; }
        public string PosnetData2 { get; set; }
        public string Digest { get; set; }
        public string VftCode { get; set; }
        // -- YKB YE OZEL


        public string CreditCardNumber { get; set; }
        public string CreditCardExpireYear { get; set; }
        public string CreditCardExpireMonth { get; set; }
        public string CreditCardCvv2 { get; set; }
        public string CustomerId { get;set; }



        

        public NameValueCollection Form { get; set; }

        public ProcessPaymentRequest PaymentRequest { get; set; }


    }
}
