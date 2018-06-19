using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Services.Media;
using Nop.Core.Domain.Directory;
using System.Globalization;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Configuration;
using Nop.Core.Infrastructure;

namespace Nop.Services.Catalog
{
    public static class Extensions
    {
        public static IEnumerable<int> To1Dimension(this IEnumerable<IEnumerable<int>> values)
        {
            List<int> list = new List<int>();

            foreach (var item in values)
            {
                foreach (var inneritem in item)
                {
                    list.Add(inneritem);
                }
            }
            return list;
        }
        public static Manufacturer GetDefaultManufacturer(this Product source)
        {
            if (source.ProductManufacturers.Count == 0) return null;
            return source.ProductManufacturers.FirstOrDefault().Manufacturer;
        }
        public static Picture GetDefaultProductVariantPicture(this ProductVariant source, IPictureService pictureService)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            if (pictureService == null)
                throw new ArgumentNullException("pictureService");

            var picture = pictureService.GetPictureById(source.PictureId);
            if (picture == null)
                picture = pictureService.GetPicturesByProductVariantId(source.Id, 1).FirstOrDefault();
            if (picture == null)
                return source.Product.GetDefaultProductPicture(pictureService);
            return picture;
        }
        
        public static IList<Picture> GetProductVariantPictures(this ProductVariant source, IPictureService pictureService)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            if (pictureService == null)
                throw new ArgumentNullException("pictureService");

            var pictures = pictureService.GetPicturesByProductVariantId(source.Id);

            if (pictures.Count > 0)
            {
                return pictures;
            }
            pictures = pictureService.GetPicturesByProductId(source.ProductId);
            if (pictures.Count > 0)
            {
                return pictures;
            }
            return null;

        }


        public static Picture GetProductPicture(this Product source, IPictureService pictureService)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (pictureService == null)
                throw new ArgumentNullException("pictureService");
            var picture = source.ProductVariants.FirstOrDefault().GetDefaultProductVariantPicture(pictureService);
            return picture;
        }
        public static Category GetDefaultProductCategory(this Product source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            var pc = source.ProductCategories.FirstOrDefault(x => x.Category.Published);
            if (pc == null) return null;
            return pc.Category;
        }

        public static Category GetPublishDefaultProductCategory(this Product source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            var pc = source.ProductCategories.Where(x => x.Category.Published == true).Where(x => x.Category.Deleted == false).OrderBy(x => x.DisplayOrder).FirstOrDefault();
            if (pc == null) return null;
            return pc.Category;
        }

        public static string GetCurrencySymbol(this Currency cusrrency)
        {
            return new CultureInfo(cusrrency.DisplayLocale).NumberFormat.CurrencySymbol;
        }

        public static bool CallforPriceRequested(this Customer customer, ProductVariant productVariant)
        {
            if (customer == null)
                return false;
            if (productVariant == null)
                return false;
            if (!productVariant.CallForPrice)
                return false;
            var quote = customer.CustomerProductVariantQuotes.FirstOrDefault(x => x.ProductVariantId == productVariant.Id);
            if (quote == null)
                return false;
            return quote.ActivateDate.HasValue;
        }

        public static bool CallforPriceRequested(this ProductVariant productVariant, Customer customer)
        {

            if (customer == null)
                return false;
            if (productVariant == null)
                return false;
            if (!productVariant.CallForPrice)
                return false;
            var quote = customer.CustomerProductVariantQuotes.FirstOrDefault(x => x.ProductVariantId == productVariant.Id);
            if (quote == null)
                return false;
            return quote.ActivateDate.HasValue;
        }

        public static string GetTrackingUrl(this Order order)
        {
            if (order == null) return "";
            if (string.IsNullOrWhiteSpace(order.TrackingNumber)) return "";
            if (string.IsNullOrWhiteSpace(order.ShippingMethod)) return "";
            if (order.ShippingStatus == Core.Domain.Shipping.ShippingStatus.NotYetShipped ||
                order.ShippingStatus == Core.Domain.Shipping.ShippingStatus.ShippingNotRequired) return "";

            string url="";
            ISettingService settingService = EngineContext.Current.Resolve<ISettingService>();
            if (order.ShippingMethod.Trim().ToLower() == "ups")
                url = settingService.GetSettingByKey<string>("upssettings.trackingurl");
            else if (order.ShippingMethod.Trim().ToLower() == "dhl")
                url = settingService.GetSettingByKey<string>("dhlsettings.trackingurl");
            else if (order.ShippingMethod.Trim().ToLower() == "aras kargo")
                url = settingService.GetSettingByKey<string>("arassettings.trackingurl");
            
            if (!string.IsNullOrWhiteSpace(url))
            {
                url = string.Format(url, order.TrackingNumber);
            }
            return url;
            
        }
    
    }
}
