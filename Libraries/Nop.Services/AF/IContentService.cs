using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
namespace Nop.Services.AFServices
{
    public interface IContentService
    {
        IEnumerable<ProductCategory> GetContent(string contentName, int count = 0);

        IEnumerable<ProductCategory> GetContent(string contentName, int categoryId, int count = 0);
    }
}
