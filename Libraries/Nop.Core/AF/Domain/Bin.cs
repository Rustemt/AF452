using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nop.Core.Domain.Payments
{
    public partial class Bin : BaseEntity
    {
        public virtual int BinCode { get; set; }

        public virtual int BankCode { get; set; }

        public virtual string BankName { get; set; }

        public virtual string Type { get; set; }

        public virtual string EffectiveCardType { get; set; }

        public virtual string SubType { get; set; }

        public virtual string Virtual { get; set; }

        public virtual string Prepaid { get; set; }

        public virtual string Installments { get; set; }

        public Bin()
        {

        }
    }
}
