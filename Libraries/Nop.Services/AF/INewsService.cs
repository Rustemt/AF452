using System;
using Nop.Core;
using Nop.Core.Domain.News;
using System.Collections.Generic;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Catalog;

namespace Nop.Services.News
{
    /// <summary>
    /// News service interface
    /// </summary>
    public partial interface INewsService
    {
       
        IPagedList<NewsItem> GetAllNews(int languageId,
        DateTime? dateFrom, DateTime? dateTo, NewsType? systemTypes, int pageIndex, int pageSize, bool showHidden = false);
       
        void DeleteNewsItemPicture(Nop.Core.Domain.News.NewsItemPicture newsItemPicture);
        Nop.Core.Domain.News.NewsItemPicture GetNewsItemPictureById(int newsItemPictureId);
        IList<NewsItemPicture> GetNewsItemPicturesByNewsItemId(int newsItemId, NewsItemPictureType? newsItemPictureType);
        void InsertNewsItemPicture(Nop.Core.Domain.News.NewsItemPicture newsItemPicture);
        void UpdateNewsItemPicture(Nop.Core.Domain.News.NewsItemPicture newsItemPicture);

        void DeleteNewsItemProduct(Nop.Core.Domain.News.NewsItemProduct newsItemProduct);
        Nop.Core.Domain.News.NewsItemProduct GetNewsItemProductById(int newsItemProductId);
        System.Collections.Generic.IList<Nop.Core.Domain.News.NewsItemProduct> GetNewsItemProductsByNewsItemId(int newsItemId, bool showHidden = false);
        void InsertNewsItemProduct(Nop.Core.Domain.News.NewsItemProduct newsItemProduct);
        void UpdateNewsItemProduct(Nop.Core.Domain.News.NewsItemProduct newsItemProduct);

        void DeleteNewsItemProductVariant(Nop.Core.Domain.News.NewsItemProductVariant newsItemProductVariant);
        Nop.Core.Domain.News.NewsItemProductVariant GetNewsItemProductVariantById(int newsItemProductVariantId);
        System.Collections.Generic.IList<Nop.Core.Domain.News.NewsItemProductVariant> GetNewsItemProductVariantsByNewsItemId(int newsItemId, bool showHidden = false);
        void InsertNewsItemProductVariant(Nop.Core.Domain.News.NewsItemProductVariant newsItemProductVariant);
        void UpdateNewsItemProductVariant(Nop.Core.Domain.News.NewsItemProductVariant newsItemProductVariant);

        void DeleteNewsItemExtraContent(Nop.Core.Domain.News.NewsItemExtraContent newsItemExtraContent);
        Nop.Core.Domain.News.NewsItemExtraContent GetNewsItemExtraContentById(int newsItemExtraContentId);
        IList<NewsItemExtraContent> GetNewsItemExtraContentsByNewsItemId(int newsItemId);
        void InsertNewsItemExtraContent(Nop.Core.Domain.News.NewsItemExtraContent newsItemExtraContent);
        void UpdateNewsItemExtraContent(Nop.Core.Domain.News.NewsItemExtraContent newsItemExtraContent);
        IPagedList<NewsItem> GetAllNewsSearch(int pageIndex, int pageSize, string searchTitle, int searchSystemTypeId);
        IPagedList<NewsItem> GetAllNewsSearch(int pageIndex, int pageSize, string title, NewsType? newsType, int? languageId, bool published, DateTime? startTime, DateTime? endTime);

		Picture GetMediaPictureByNewsItemPictureId(int pictureId);
		IList<ProductSummary> GetNewsItemProductsSummaryByNewsItemId(int newsItemId);
    }
}
