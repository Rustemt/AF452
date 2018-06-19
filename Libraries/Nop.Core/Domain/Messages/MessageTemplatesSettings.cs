using Nop.Core.Configuration;

namespace Nop.Core.Domain.Messages
{
    public class MessageTemplatesSettings : ISettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether to replace message tokens according to case invariant rules
        /// </summary>
        public bool CaseInvariantReplacement { get; set; }

        /// <summary>
        /// Gets or sets a color1 in  hex format ("#hhhhhh") to use in workflow message formatting
        /// </summary>
        public string Color1 { get; set; }

        /// <summary>
        /// Gets or sets a color2 in  hex format ("#hhhhhh") to use in workflow message formatting
        /// </summary>
        public string Color2 { get; set; }

        /// <summary>
        /// Gets or sets a color3 in  hex format ("#hhhhhh") to use in workflow message formatting
        /// </summary>
        public string Color3 { get; set; }


        //public string trHeader { get; set; }
        //public string trProduct { get; set; }
        //public string tdProductName { get; set; }
        //public string tdProductUnitPrice { get; set; }
        //public string tdProductQuantity { get; set; }
        //public string tdProductSubTotal { get; set; }
        //public string tdCheckoutAttributeInfo1 { get; set; }
        //public string tdCheckoutAttributeInfo2 { get; set; }
        //public string trHeader { get; set; }
        //public string trHeader { get; set; }
        //public string trHeader { get; set; }

    }

}
