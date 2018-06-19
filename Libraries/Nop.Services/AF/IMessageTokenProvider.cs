using System.Collections.Generic;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Orders;

namespace Nop.Services.Messages
{
    public partial interface IMessageTokenProvider
    {
        void AddWishListTokens(IList<Token> tokens,  IList<ShoppingCartItem> wishListItems);
        void AddVariantTokens(IList<Token> tokens, ProductVariant productVariant);
        void AddCustomerTokens(IList<Token> tokens, Customer customer, string fullName);
    }
}
