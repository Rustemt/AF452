using System.Collections.Generic;
using System.Linq;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Media;

namespace Nop.Services.AFServices
{
    public class ContentService : IContentService
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IPictureService _pictureService;

        public ContentService(IProductService productService, ICategoryService categoryService, IPictureService pictureService)
        {
            this._productService = productService;
            this._categoryService = categoryService;
            this._pictureService = pictureService;
        }

        public IEnumerable<ProductCategory> GetContent(string contentName, int count = 0)
        {
            var topLevelCategories = _categoryService.GetAllCategoriesByParentCategoryId(0, showHidden: true);
            Category contentCategory = (from c in topLevelCategories
                                        where c.Name == "Content"
                                        select c).First();

            var contentSubCategories = _categoryService.GetAllCategoriesByParentCategoryId(contentCategory.Id, showHidden: true);

            //            var homeMainCategory = contentSubCategories.

            var category = (from c in contentSubCategories
                                    where c.Name == contentName
                                    select c).First();

            var contentProducts = _categoryService.GetProductCategoriesByCategoryId(category.Id, true);
            if (count == 0)
                return contentProducts.Take(contentCategory.PageSize);
            else
                return contentProducts.Take(count);
        }

        public IEnumerable<ProductCategory> GetContent(string contentName, int categoryId, int count = 0)
        {
            var categoryProducts = GetContent(contentName, count);
            List<ProductCategory> items = new List<ProductCategory>();

            foreach (var categoryProduct in categoryProducts)
            {
                var product = categoryProduct.Product;
                foreach (var categoryproduct2 in product.ProductCategories)
                {
                    if (categoryproduct2.CategoryId == categoryId)
                        items.Add(categoryProduct);
                }
            }
            return items;
        }
    }
}