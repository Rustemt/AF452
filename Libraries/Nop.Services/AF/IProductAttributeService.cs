using System;
using Nop.Core.Domain.Catalog;
using System.Collections.Generic;
namespace Nop.Services.Catalog
{
    public partial interface IProductAttributeService
    {
        void DeleteProductAttributeOption(Nop.Core.Domain.Catalog.ProductAttributeOption productAttributeOption);
        ProductAttributeOption GetProductAttributeOptionById(int productAttributeOptionId);
        IList<ProductAttributeOption> GetProductAttributeOptionsByProductAttribute(int productAttributeId);
        void InsertProductAttributeOption(Nop.Core.Domain.Catalog.ProductAttributeOption productAttributeOption);
        void UpdateProductAttributeOption(Nop.Core.Domain.Catalog.ProductAttributeOption productAttributeOption);
        IList<ProductVariantAttribute> GetProductVariantAttributesByIds(IList<int> productVariantAttributeIds);
    }
}
