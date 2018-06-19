using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.News;


namespace Nop.Services.Catalog
{
    /// <summary>
    /// Category service interface
    /// </summary>
    public partial interface ICategoryService
    {
        IList<Product> GetRandomCrossSellProductsByCategoryId(List<int> categoryId, int count, int productId);
        IList<Product> GetRandomProductsByCategoryId(int categoryId, int count, int productId);
        IList<NewsItemCategory> GetNewsItemCategoriesByNewsItemId(int newsItemId, bool showHidden = false);
        void InsertNewsItemCategory(NewsItemCategory newsItemCategory);
        NewsItemCategory GetNewsItemCategoryById(int newsItemCategoryId);
        void UpdateNewsItemCategory(NewsItemCategory newsItemCategory);
        void DeleteNewsItemCategory(NewsItemCategory newsItemCategory);
        NewsItem GetCategoryNewsItemByCategoryId(int categoryId,int languageId);
        //NewsItem GetNewsItemByCategoryId(int categoryId);
        NewsItemCategory GetNewsItemCategoryByCategoryId(int categoryId);
        Category GetCategoryBySeName(string SeName);
        IPagedList<NewsItem> GetCategoryNewsItemsByCategoryId(NewsType? newsType, int categoryId, int languageId, int pageIndex, int pageSize);
        IPagedList<NewsItem> GetCategoryNewsItemsByCategoryIdDefault(NewsType? newsType, int categoryId, int languageId, int pageIndex, int pageSize);
        IList<NewsItem> GetRandomCategoryNewsItemsByCategoryId(NewsType? newsType, int categoryId, int languageId, int count, int newsItemId);
        //IList<Product> GetAllProductsByCategoryId(int categoryId, int productId);
        
    }
}
