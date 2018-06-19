using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Topics;
using Nop.Services.Localization;
using Nop.Services.Seo;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Common;
using Nop.Web.Models.Topics;
using Nop.Web.Models.News;
using System.Linq;

namespace Nop.Web.Extensions
{
    public static class MappingExtensions
    {
        //category
        public static CategoryModel ToModel(this Category entity)
        {
            if (entity == null)
                return null;

            var model = new CategoryModel()
                            {
                                Id = entity.Id,
                                Name = entity.GetLocalized(x => x.Name),
                                Description = entity.GetLocalized(x => x.Description),
                                MetaKeywords = entity.GetLocalized(x => x.MetaKeywords),
                                MetaDescription = entity.GetLocalized(x => x.MetaDescription),
                                MetaTitle = entity.GetLocalized(x => x.MetaTitle),
                                SeName = entity.GetSeName(),
                                DisplayOrder = entity.DisplayOrder,

                            };
            return model;
        }

        //manufacturer
        public static ManufacturerModel ToModel(this Manufacturer entity)
        {
            if (entity == null)
                return null;

            var model = new ManufacturerModel()
                            {
                                Id = entity.Id,
                                Name = entity.GetLocalized(x => x.Name),
                                Description = entity.GetLocalized(x => x.Description),
                                MetaKeywords = entity.GetLocalized(x => x.MetaKeywords),
                                MetaDescription = entity.GetLocalized(x => x.MetaDescription),
                                MetaTitle = entity.GetLocalized(x => x.MetaTitle),
                                SeName = entity.GetSeName(),
                            };
            return model;
        }

        //language
        public static LanguageModel ToModel(this Language entity)
        {
            if (entity == null)
                return null;

            var model = new LanguageModel()
                            {
                                Id = entity.Id,
                                Name = entity.Name,
                                FlagImageFileName = entity.FlagImageFileName,
                            };
            return model;
        }


        //currency
        public static CurrencyModel ToModel(this Currency entity)
        {
            if (entity == null)
                return null;

            var model = new CurrencyModel()
                            {
                                Id = entity.Id,
                                Name = entity.Name,
                            };
            return model;
        }

        //product
        public static ProductModel ToModel(this Product entity, bool extended=false)
        {
            if (entity == null)
                return null;

            var model = new ProductModel()
                            {
                                Id = entity.Id,
                                Name = entity.GetLocalized(x => x.Name),
                                ShortDescription = entity.GetLocalized(x => x.ShortDescription),
                                FullDescription = entity.GetLocalized(x => x.FullDescription),
                                MetaKeywords = entity.GetLocalized(x => x.MetaKeywords),
                                MetaDescription = entity.GetLocalized(x => x.MetaDescription),
                                MetaTitle = entity.GetLocalized(x => x.MetaTitle),
                                SeName = entity.GetSeName(),
                                CreatedOnUtc = entity.CreatedOnUtc
                            };
            if (extended)
            {
                var pm = entity.ProductManufacturers.FirstOrDefault();
                if(pm!=null)
                {
                    model.Manufacturer = pm.Manufacturer.GetLocalized(x=>x.Name);
                    model.ManufacturerId = pm.ManufacturerId;
                    model.ManufacturerOrder = pm.DisplayOrder;
                    model.ManufacturerSeName = pm.Manufacturer.GetLocalized(x=>x.SeName);
                }
                model.ProductManufacturers = entity.ProductManufacturers.Select(x => new ProductModel.ProductManufacturerModel() { Id = x.ManufacturerId, Name= x.Manufacturer.GetLocalized(m => m.Name), DisplayOrder = x.DisplayOrder }).ToList();
                var pc = entity.ProductCategories.FirstOrDefault();
                if (pc != null)
                {
                    model.CategoryName = pc.Category.GetLocalized(x => x.Name);
                    model.CategoryId = pc.CategoryId;
                    model.CategoryOrder = pc.DisplayOrder;
                }
                model.ProductCategories = entity.ProductCategories.Select(x => new ProductModel.ProductCategoryModel() { Id = x.CategoryId, Name= x.Category.GetLocalized(c => c.Name), DisplayOrder = x.DisplayOrder }).ToList();
            }
            return model;
        }

        //address
        public static AddressModel ToModel(this Address entity)
        {
            if (entity == null)
                return null;

            var model = new AddressModel()
                            {
                                Id = entity.Id,
                                FirstName = entity.FirstName,
                                LastName = entity.LastName,
                                Email = entity.Email,
                                Company = entity.Company,
                                CountryId = entity.CountryId,
                                CountryName = entity.Country != null ? entity.Country.Name : null,
                                StateProvinceId = entity.StateProvinceId,
                                StateProvinceName = entity.StateProvince != null ? entity.StateProvince.Name : null,
                                City = entity.City,
                                Address1 = entity.Address1,
                                Address2 = entity.Address2,
                                ZipPostalCode = entity.ZipPostalCode,
                                PhoneNumber = entity.PhoneNumber,
                                FaxNumber = entity.FaxNumber,
                                Name = entity.Name,
                                TaxNo = entity.TaxNo,
                                TaxOffice = entity.TaxOffice,
                                CivilNo = entity.CivilNo,
                                Title = entity.Title,
                                EmailDisabled = true,
                                DefaultBillingAddress = entity.DefaultBillingAddress,
                                DefaultShippingAddress = entity.DefaultShippingAddress,
                                IsEnterprise = entity.IsEnterprise

                            };
            return model;
        }

        public static Address ToEntity(this AddressModel model)
        {
            if (model == null)
                return null;

            var entity = new Address();
            return ToEntity(model, entity);
        }

        public static Address ToEntity(this AddressModel model, Address destination)
        {
            if (model == null)
                return destination;
            destination.Id = model.Id;
            destination.FirstName = model.FirstName;
            destination.LastName = model.LastName;
            destination.Email = model.Email;
            destination.Company = model.Company;
            destination.CountryId = model.CountryId;
            destination.StateProvinceId = model.StateProvinceId;
            destination.City = model.City;
            destination.Address1 = model.Address1;
            destination.Address2 = model.Address2;
            destination.ZipPostalCode = model.ZipPostalCode;
            destination.PhoneNumber = model.PhoneNumber;
            destination.FaxNumber = model.FaxNumber;
            destination.Name = model.Name;
            destination.CivilNo = model.CivilNo;
            destination.TaxNo = model.TaxNo;
            destination.TaxOffice = model.TaxOffice;
            destination.Title = model.Title;
            destination.DefaultShippingAddress = model.DefaultShippingAddress;
            destination.DefaultBillingAddress = model.DefaultBillingAddress;
            destination.IsEnterprise = model.IsEnterprise;

            return destination;
        }

        //topics
        public static TopicModel ToModel(this Topic entity)
        {
            if (entity == null)
                return null;

            var model = new TopicModel()
                            {
                                Id = entity.Id,
                                SystemName = entity.SystemName,
                                IncludeInSitemap = entity.IncludeInSitemap,
                                IsPasswordProtected = entity.IsPasswordProtected,
                                Title = entity.GetLocalized(x => x.Title),
                                Body = entity.GetLocalized(x => x.Body),
                                MetaKeywords = entity.GetLocalized(x => x.MetaKeywords),
                                MetaDescription = entity.GetLocalized(x => x.MetaDescription),
                                MetaTitle = entity.GetLocalized(x => x.MetaTitle),
                            };
            return model;
        }
       
        public static ExtraContentModel ToModel(this ExtraContentModel entity)
        {
            if (entity == null)
                return null;

            var model = new ExtraContentModel()
                            {
                                Id = entity.Id,
                                Title = entity.Title,
                                DisplayOrder = entity.DisplayOrder,
                                FullDescription = entity.FullDescription,

                            };
            return model;
            //AF
            //public static IEnumerable<ContentItemModel> ToContentItems(this IEnumerable<ProductCategory> items, IPictureService pictureService)
            //{
            //    return items.Select(x => new ContentItemModel()
            //    {
            //        Title = x.Product.ShortDescription,
            //        Content = x.Product.FullDescription,
            //        ContentType = ContentType.MainContent,
            //        ImagePath = x.Product.ProductPictures.Count > 0 ? pictureService.GetPictureUrl(x.Product.ProductPictures.FirstOrDefault().Picture) : null,
            //        Url = x.Product.AdminComment,
            //        Price = ""//TODO:Mustafa set price
            //    });

            //}
            //public static ContentItemModel ToModel(this ProductCategory entity)
            //{
            //    if (entity == null || entity.Product == null)
            //        return null;

            //    var model = new ContentItemModel()
            //    {
            //        Title = entity.Product.ShortDescription,
            //        Content = entity.Product.FullDescription,
            //        ImagePath = x.Product.ProductPictures.Count > 0 ? pictureService.GetPictureUrl(x.Product.ProductPictures.FirstOrDefault().Picture) : null,
            //        Url = x.Product.AdminComment,
            //        Price = ""//TODO:Mustafa set price
            //    };
            //    return model;

            //}

        }


        public static CategoryProductsModel280 ToModel280(this Category entity)
        {
            if (entity == null)
                return null;

            var model = new CategoryProductsModel280()
            {
                Id = entity.Id,
                Name = entity.GetLocalized(x => x.Name),
                Description = entity.GetLocalized(x => x.Description),
                MetaKeywords = entity.GetLocalized(x => x.MetaKeywords),
                MetaDescription = entity.GetLocalized(x => x.MetaDescription),
                MetaTitle = entity.GetLocalized(x => x.MetaTitle),
                SeName = entity.GetSeName(),
            };
            return model;
        }

        public static ManufacturerProductsModel280 ToModel280(this Manufacturer entity)
        {
            if (entity == null)
                return null;

            var model = new ManufacturerProductsModel280()
            {
                Id = entity.Id,
                Name = entity.GetLocalized(x => x.Name),
                Description = entity.GetLocalized(x => x.Description),
                MetaKeywords = entity.GetLocalized(x => x.MetaKeywords),
                MetaDescription = entity.GetLocalized(x => x.MetaDescription),
                MetaTitle = entity.GetLocalized(x => x.MetaTitle),
                SeName = entity.GetSeName(),
            };
            return model;
        }

    }
}