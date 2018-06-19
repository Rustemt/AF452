using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nop.Web.Models.Common;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.News;
using Nop.Services.Media;
using Nop.Web.Models.Catalog;
using System.Globalization;
using Nop.Services.Localization;

namespace Nop.Web.Extensions
{
    public static class Common
    {
        public static string ToContentString(this ContentType type)
        {
            switch (type)
            {
                case ContentType.MainContent:
                    return "HomeMain";
                case ContentType.PromoContent:
                    return "HomeBottom";
                case ContentType.CategoryMenuContent:
                    return "MenuCategory";
                case ContentType.CategoryHomeContent:
                    return "Category";
                default:
                    return "HomeMain";
            }
        }


      

        public static IEnumerable<ContentItemModel> ToContentItems(this IEnumerable<Product> products, IPictureService pictureService)
        {
            List<ContentItemModel> items = new List<ContentItemModel>();
            foreach (var product in products)
            {
                var productModel = product.ToModel();
                items.Add(new ContentItemModel() {
                    Title = productModel.ShortDescription,
                    Content = productModel.FullDescription,
                    ContentType = ContentType.MainContent,
                    ImagePath = product.ProductPictures.Count > 0 ? pictureService.GetPictureUrl(product.ProductPictures.FirstOrDefault().Picture) : null,
                    Url = product.AdminComment,
                    Price = product.ProductVariants.FirstOrDefault().Price == 0 ? "" : product.ProductVariants.FirstOrDefault().Price.ToString()
                });
            }
            return items;
        }

        public static IEnumerable<ContentItemModel> ToContentItems(this IEnumerable<NewsItem> news, IPictureService pictureService)
        {
            List<ContentItemModel> items = new List<ContentItemModel>();
            foreach (var newsItem in news)
            {
               
                items.Add(new ContentItemModel()
                {
                    Title = newsItem.Title,
                    Content = newsItem.Full,
                    ContentType = ContentType.MainContent,
                    ImagePath = newsItem.NewsItemPictures.Count > 0 ? pictureService.GetPictureUrl(newsItem.NewsItemPictures.FirstOrDefault().Picture) : null,
                    Url = newsItem.Short,
					MetaDescription = newsItem.MetaDescription
                 });
            }
            return items;
        }



        public static string ToPriceHtml(this ProductModel.ProductVariantModel.ProductVariantPriceModel model)
        {
            return model.Price;
        }

        public static string ManufacturerName(this Product product)
        {
            if (product.ProductManufacturers.Count == 0) return "";
            return product.ProductManufacturers.First().Manufacturer.GetLocalized(x => x.Name);
        }
    }
}
