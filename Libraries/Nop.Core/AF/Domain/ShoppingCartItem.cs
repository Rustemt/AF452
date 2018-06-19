using System;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;

namespace Nop.Core.Domain.Orders
{
   
    public partial class ShoppingCartItem : BaseEntity
    {
        public virtual string CustomerComment { get; set;}
    }
}
