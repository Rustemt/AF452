using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using iTextSharp.text.pdf;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Media;
using Nop.Services.Messages;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Nop.Services.Localization;
using Nop.Services.Common;
using Nop.Core.Domain.Common;
using Nop.Services.Logging;
using Nop.Core.Infrastructure;
using Nop.Services.Tax;
using Nop.Services.Seo;
using Nop.Services.Configuration;
using Nop.Core.Domain.Messages;
using Nop.Services.Directory;
using Nop.Core.Domain.Directory;




namespace Nop.Services.ExportImport
{
    /// <summary>
    /// Export manager
    /// </summary>
    public partial class ExportManager : IExportManager
    {
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IProductService _productService;
        private readonly IPictureService _pictureService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly StoreInformationSettings _storeInformationSettings;
        private readonly ILanguageService _languageService;
        private readonly IAddressService _addressService;
        private readonly IWebHelper _webHelper;
        private readonly ITaxService _taxService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ICurrencyService _currencyService;
        private readonly IWorkContext _workContext;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly ISettingService _settingService;
        private readonly CurrencySettings _currencySettings;
        

        public ExportManager(ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IProductService productService,
            IPictureService pictureService,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            StoreInformationSettings storeInformationSettings, ILanguageService languageService, IAddressService addressService,
            IWebHelper webHelper, ITaxService taxService, IPriceCalculationService priceCalculationService, ICurrencyService currencyService,
            IWorkContext workContext, ISpecificationAttributeService specificationAttributeService, ISettingService settingService
            , CurrencySettings currencySettings)
        {
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._productService = productService;
            this._pictureService = pictureService;
            this._newsLetterSubscriptionService = newsLetterSubscriptionService;
            this._storeInformationSettings = storeInformationSettings;
            this._languageService = languageService;
            this._addressService = addressService;
            this._webHelper = webHelper;
            this._taxService = taxService;
            this._priceCalculationService = priceCalculationService;
            this._currencyService = currencyService;
            this._workContext = workContext;
            this._specificationAttributeService = specificationAttributeService;
            this._settingService = settingService;
            this._currencySettings = currencySettings;
        }

        #region Utilities

        protected virtual void WriteCategories(XmlWriter xmlWriter, int parentCategoryId)
        {
            var categories = _categoryService.GetAllCategoriesByParentCategoryId(parentCategoryId, true);
            if (categories != null && categories.Count > 0)
            {
                foreach (var category in categories)
                {
                    xmlWriter.WriteStartElement("Category");
                    xmlWriter.WriteElementString("Id", null, category.Id.ToString());
                    xmlWriter.WriteElementString("Name", null, category.Name);
                    xmlWriter.WriteElementString("Description", null, category.Description);
                    xmlWriter.WriteElementString("CategoryTemplateId", null, category.CategoryTemplateId.ToString());
                    xmlWriter.WriteElementString("MetaKeywords", null, category.MetaKeywords);
                    xmlWriter.WriteElementString("MetaDescription", null, category.MetaDescription);
                    xmlWriter.WriteElementString("MetaTitle", null, category.MetaTitle);
                    xmlWriter.WriteElementString("SeName", null, category.SeName);
                    xmlWriter.WriteElementString("ParentCategoryId", null, category.ParentCategoryId.ToString());
                    xmlWriter.WriteElementString("PictureId", null, category.PictureId.ToString());
                    xmlWriter.WriteElementString("PageSize", null, category.PageSize.ToString());
                    xmlWriter.WriteElementString("AllowCustomersToSelectPageSize", null, category.AllowCustomersToSelectPageSize.ToString());
                    xmlWriter.WriteElementString("PageSizeOptions", null, category.PageSizeOptions);
                    xmlWriter.WriteElementString("PriceRanges", null, category.PriceRanges);
                    xmlWriter.WriteElementString("ShowOnHomePage", null, category.ShowOnHomePage.ToString());
                    xmlWriter.WriteElementString("Published", null, category.Published.ToString());
                    xmlWriter.WriteElementString("Deleted", null, category.Deleted.ToString());
                    xmlWriter.WriteElementString("DisplayOrder", null, category.DisplayOrder.ToString());
                    xmlWriter.WriteElementString("CreatedOnUtc", null, category.CreatedOnUtc.ToString());
                    xmlWriter.WriteElementString("UpdatedOnUtc", null, category.UpdatedOnUtc.ToString());


                    xmlWriter.WriteStartElement("Products");
                    var productCategories = _categoryService.GetProductCategoriesByCategoryId(category.Id, true);
                    foreach (var productCategory in productCategories)
                    {
                        var product = productCategory.Product;
                        if (product != null && !product.Deleted)
                        {
                            xmlWriter.WriteStartElement("ProductCategory");
                            xmlWriter.WriteElementString("ProductCategoryId", null, productCategory.Id.ToString());
                            xmlWriter.WriteElementString("ProductId", null, productCategory.ProductId.ToString());
                            xmlWriter.WriteElementString("IsFeaturedProduct", null, productCategory.IsFeaturedProduct.ToString());
                            xmlWriter.WriteElementString("DisplayOrder", null, productCategory.DisplayOrder.ToString());
                            xmlWriter.WriteEndElement();
                        }
                    }
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("SubCategories");
                    WriteCategories(xmlWriter, category.Id);
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Export manufacturer list to xml
        /// </summary>
        /// <param name="manufacturers">Manufacturers</param>
        /// <returns>Result in XML format</returns>
        public virtual string ExportManufacturersToXml(IList<Manufacturer> manufacturers)
        {
            var sb = new StringBuilder();
            var stringWriter = new StringWriter(sb);
            var xmlWriter = new XmlTextWriter(stringWriter);
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("Manufacturers");
            xmlWriter.WriteAttributeString("Version", NopVersion.CurrentVersion);

            foreach (var manufacturer in manufacturers)
            {
                xmlWriter.WriteStartElement("Manufacturer");

                xmlWriter.WriteElementString("ManufacturerId", null, manufacturer.Id.ToString());
                xmlWriter.WriteElementString("Name", null, manufacturer.Name);
                xmlWriter.WriteElementString("Description", null, manufacturer.Description);
                xmlWriter.WriteElementString("ManufacturerTemplateId", null, manufacturer.ManufacturerTemplateId.ToString());
                xmlWriter.WriteElementString("MetaKeywords", null, manufacturer.MetaKeywords);
                xmlWriter.WriteElementString("MetaDescription", null, manufacturer.MetaDescription);
                xmlWriter.WriteElementString("MetaTitle", null, manufacturer.MetaTitle);
                xmlWriter.WriteElementString("SEName", null, manufacturer.SeName);
                xmlWriter.WriteElementString("PictureId", null, manufacturer.PictureId.ToString());
                xmlWriter.WriteElementString("PageSize", null, manufacturer.PageSize.ToString());
                xmlWriter.WriteElementString("AllowCustomersToSelectPageSize", null, manufacturer.AllowCustomersToSelectPageSize.ToString());
                xmlWriter.WriteElementString("PageSizeOptions", null, manufacturer.PageSizeOptions);
                xmlWriter.WriteElementString("PriceRanges", null, manufacturer.PriceRanges);
                xmlWriter.WriteElementString("Published", null, manufacturer.Published.ToString());
                xmlWriter.WriteElementString("Deleted", null, manufacturer.Deleted.ToString());
                xmlWriter.WriteElementString("DisplayOrder", null, manufacturer.DisplayOrder.ToString());
                xmlWriter.WriteElementString("CreatedOnUtc", null, manufacturer.CreatedOnUtc.ToString());
                xmlWriter.WriteElementString("UpdatedOnUtc", null, manufacturer.UpdatedOnUtc.ToString());

                xmlWriter.WriteStartElement("Products");
                var productManufacturers = _manufacturerService.GetProductManufacturersByManufacturerId(manufacturer.Id, true);
                if (productManufacturers != null)
                {
                    foreach (var productManufacturer in productManufacturers)
                    {
                        var product = productManufacturer.Product;
                        if (product != null && !product.Deleted)
                        {
                            xmlWriter.WriteStartElement("ProductManufacturer");
                            xmlWriter.WriteElementString("ProductManufacturerId", null, productManufacturer.Id.ToString());
                            xmlWriter.WriteElementString("ProductId", null, productManufacturer.ProductId.ToString());
                            xmlWriter.WriteElementString("IsFeaturedProduct", null, productManufacturer.IsFeaturedProduct.ToString());
                            xmlWriter.WriteElementString("DisplayOrder", null, productManufacturer.DisplayOrder.ToString());
                            xmlWriter.WriteEndElement();
                        }
                    }
                }
                xmlWriter.WriteEndElement();


                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
            return stringWriter.ToString();
        }

        /// <summary>
        /// Export category list to xml
        /// </summary>
        /// <returns>Result in XML format</returns>
        public virtual string ExportCategoriesToXml()
        {
            var sb = new StringBuilder();
            var stringWriter = new StringWriter(sb);
            var xmlWriter = new XmlTextWriter(stringWriter);
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("Categories");
            xmlWriter.WriteAttributeString("Version", NopVersion.CurrentVersion);
            WriteCategories(xmlWriter, 0);
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
            return stringWriter.ToString();
        }



        public virtual string RetargetingExportProductsToXml(IList<Product> products)
        {
            var sb = new StringBuilder();
            var stringWriter = new StringWriter(sb);
            var xmlWriter = new XmlTextWriter(stringWriter);
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("Urunler");


            ISettingService settingService = EngineContext.Current.Resolve<ISettingService>();

            
            foreach (var product in products)
            {
                //var productCategories = _categoryService.GetProductCategoriesByProductId(product.Id).FirstOrDefault();
                //var productManufacturers = _manufacturerService.GetProductManufacturersByProductId(product.Id).FirstOrDefault();
                //var productVariant = _productService.GetProductVariantsByProductId(product.Id);

                IList<Core.Domain.Media.Picture> pictures = new List<Core.Domain.Media.Picture>();

                var productVariant = product.ProductVariants.Where(x => x.Deleted == false).Where(x => x.Published == true);

                foreach (var variant in productVariant) 
                {
                    pictures = variant.GetProductVariantPictures(_pictureService);

                    xmlWriter.WriteStartElement("Urun");

                    if (variant.Id.ToString() !=null)
                    {
                        xmlWriter.WriteElementString("urun_id", variant.Id.ToString());
                    }
                    
                    if (variant.Name != null)
                    {
                        xmlWriter.WriteElementString("baslik", variant.Name);
                    }
                    else
                    {
                        xmlWriter.WriteElementString("baslik", product.Name);
                    }


                    if (variant.Product.ProductCategories.Count > 0 )
                    {
                        xmlWriter.WriteElementString("kategori", variant.Product.ProductCategories.FirstOrDefault().Category.Name); 
                    }


                    if (variant.Product.ProductManufacturers.Count > 0)
                    {
                        xmlWriter.WriteElementString("marka", variant.Product.ProductManufacturers.FirstOrDefault().Manufacturer.Name); 
                    }

                    else
                    {
                        xmlWriter.WriteElementString("marka", "");
                    }

                    if (variant.Currency == null)
                    {
                        variant.Currency = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId);
                        variant.CurrencyId = variant.Currency.Id;
                        xmlWriter.WriteElementString("birim", "TRY");
                    }

                    else
                    {
                        xmlWriter.WriteElementString("birim", variant.Currency.CurrencyCode);
                    }

                    if (pictures != null)
                    {
                        xmlWriter.WriteElementString("urun_url", null,
                            _pictureService.GetPictureUrl(pictures.FirstOrDefault()));
                    }
                    else
                    {
                        xmlWriter.WriteElementString("urun_url", "");
                    }

                    var url = string.Format("{0}p/{1}/{2}{3}", _webHelper.GetStoreLocation(false), product.Id,product.GetSeName(),_settingService.GetSettingByKey<string>("Affiliate.Parameter"));
                    xmlWriter.WriteStartElement("linkhref");
                    xmlWriter.WriteCData(url.ToString());
                    xmlWriter.WriteEndElement();

                   
                    if (variant.CurrencyId == null)
                    {
                        xmlWriter.WriteElementString("eskifiyat", variant.Price.ToString());
                    }

                    else if (variant.CurrencyId != null &&variant.CurrencyId.ToString() ==
                             settingService.GetSettingByKey<string>("currencysettings.primarystorecurrencyid"))
                    {
                        xmlWriter.WriteElementString("eskifiyat", variant.Price.ToString());
                    }

                    else
                    {
                        xmlWriter.WriteElementString("eskifiyat", variant.CurrencyPrice.ToString());
                    }


                    decimal taxRate = decimal.Zero;
                    decimal finalPriceWithDiscountBase = _taxService.GetProductPrice(variant,_priceCalculationService.GetFinalPrice(variant,true,false),out taxRate);
                    var currency = _currencyService.GetCurrencyByCode(variant.Currency.CurrencyCode);
                    decimal finalPriceWithDiscount = _currencyService.ConvertFromPrimaryExchangeRateCurrency(finalPriceWithDiscountBase, currency);


                    if (finalPriceWithDiscountBase.ToString() != null)
                    {
                        xmlWriter.WriteElementString("fiyat",finalPriceWithDiscount.ToString());
                    }

                    xmlWriter.WriteEndElement();
                }

            }
        

            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Close();

            return stringWriter.ToString();
        }

      
        /// <summary>
        /// Export product list to xml
        /// </summary>
        /// <param name="products">Products</param>
        /// <returns>Result in XML format</returns>
        public virtual string ExportProductsToXml(IList<Product> products)
        {
            try
            {
                var webHelper = EngineContext.Current.Resolve<IWebHelper>();
                var productAttributeParser = EngineContext.Current.Resolve<IProductAttributeParser>();
                var sb = new StringBuilder();
                var stringWriter = new StringWriter(sb);
                var xmlWriter = new XmlTextWriter(stringWriter);
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("Products");
                xmlWriter.WriteAttributeString("Version", NopVersion.CurrentVersion);

                foreach (var product in products)
                {
                    xmlWriter.WriteStartElement("Product");

                    xmlWriter.WriteElementString("ProductId", null, product.Id.ToString());
                    xmlWriter.WriteElementString("Name", null, product.Name);
                    xmlWriter.WriteElementString("ShortDescription", null, product.ShortDescription);
                    xmlWriter.WriteElementString("FullDescription", null, product.FullDescription);
                    xmlWriter.WriteElementString("AdminComment", null, product.AdminComment);
                    xmlWriter.WriteElementString("ProductTemplateId", null, product.ProductTemplateId.ToString());
                    xmlWriter.WriteElementString("ShowOnHomePage", null, product.ShowOnHomePage.ToString());
                    xmlWriter.WriteElementString("MetaKeywords", null, product.MetaKeywords);
                    xmlWriter.WriteElementString("MetaDescription", null, product.MetaDescription);
                    xmlWriter.WriteElementString("MetaTitle", null, product.MetaTitle);
                    xmlWriter.WriteElementString("SEName", null, product.SeName);
                    xmlWriter.WriteElementString("AllowCustomerReviews", null, product.AllowCustomerReviews.ToString());
                    xmlWriter.WriteElementString("Published", null, product.Published.ToString());
                    xmlWriter.WriteElementString("CreatedOnUtc", null, product.CreatedOnUtc.ToString());
                    xmlWriter.WriteElementString("UpdatedOnUtc", null, product.UpdatedOnUtc.ToString());

                    xmlWriter.WriteStartElement("ProductVariants");
                    var productVariants = _productService.GetProductVariantsByProductId(product.Id, true);
                    if (productVariants != null)
                    {
                        foreach (var productVariant in productVariants)
                        {
                            xmlWriter.WriteStartElement("ProductVariant");
                            xmlWriter.WriteElementString("ProductVariantId", null, productVariant.Id.ToString());
                            xmlWriter.WriteElementString("ProductId", null, productVariant.ProductId.ToString());
                            xmlWriter.WriteElementString("Name", null, productVariant.Name);
                            xmlWriter.WriteElementString("SKU", null, productVariant.Sku);
                            xmlWriter.WriteElementString("Description", null, productVariant.Description);
                            xmlWriter.WriteElementString("AdminComment", null, productVariant.AdminComment);
                            xmlWriter.WriteElementString("ManufacturerPartNumber", null, productVariant.ManufacturerPartNumber);
                            xmlWriter.WriteElementString("Gtin", null, productVariant.Gtin);
                            xmlWriter.WriteElementString("IsGiftCard", null, productVariant.IsGiftCard.ToString());
                            xmlWriter.WriteElementString("GiftCardType", null, productVariant.GiftCardType.ToString());
                            xmlWriter.WriteElementString("RequireOtherProducts", null, productVariant.RequireOtherProducts.ToString());
                            xmlWriter.WriteElementString("RequiredProductVariantIds", null, productVariant.RequiredProductVariantIds);
                            xmlWriter.WriteElementString("AutomaticallyAddRequiredProductVariants", null, productVariant.AutomaticallyAddRequiredProductVariants.ToString());
                            xmlWriter.WriteElementString("IsDownload", null, productVariant.IsDownload.ToString());
                            xmlWriter.WriteElementString("DownloadId", null, productVariant.DownloadId.ToString());
                            xmlWriter.WriteElementString("UnlimitedDownloads", null, productVariant.UnlimitedDownloads.ToString());
                            xmlWriter.WriteElementString("MaxNumberOfDownloads", null, productVariant.MaxNumberOfDownloads.ToString());
                            if (productVariant.DownloadExpirationDays.HasValue)
                                xmlWriter.WriteElementString("DownloadExpirationDays", null, productVariant.DownloadExpirationDays.ToString());
                            else
                                xmlWriter.WriteElementString("DownloadExpirationDays", null, string.Empty);
                            xmlWriter.WriteElementString("DownloadActivationType", null, productVariant.DownloadActivationType.ToString());
                            xmlWriter.WriteElementString("HasSampleDownload", null, productVariant.HasSampleDownload.ToString());
                            xmlWriter.WriteElementString("SampleDownloadId", null, productVariant.SampleDownloadId.ToString());
                            xmlWriter.WriteElementString("HasUserAgreement", null, productVariant.HasUserAgreement.ToString());
                            xmlWriter.WriteElementString("UserAgreementText", null, productVariant.UserAgreementText);
                            xmlWriter.WriteElementString("IsRecurring", null, productVariant.IsRecurring.ToString());
                            xmlWriter.WriteElementString("RecurringCycleLength", null, productVariant.RecurringCycleLength.ToString());
                            xmlWriter.WriteElementString("RecurringCyclePeriodId", null, productVariant.RecurringCyclePeriodId.ToString());
                            xmlWriter.WriteElementString("RecurringTotalCycles", null, productVariant.RecurringTotalCycles.ToString());
                            xmlWriter.WriteElementString("IsShipEnabled", null, productVariant.IsShipEnabled.ToString());
                            xmlWriter.WriteElementString("IsFreeShipping", null, productVariant.IsFreeShipping.ToString());
                            xmlWriter.WriteElementString("AdditionalShippingCharge", null, productVariant.AdditionalShippingCharge.ToString());
                            xmlWriter.WriteElementString("IsTaxExempt", null, productVariant.IsTaxExempt.ToString());
                            xmlWriter.WriteElementString("TaxCategoryId", null, productVariant.TaxCategoryId.ToString());
                            xmlWriter.WriteElementString("ManageInventoryMethodId", null, productVariant.ManageInventoryMethodId.ToString());
                            xmlWriter.WriteElementString("StockQuantity", null, productVariant.StockQuantity.ToString());
                            xmlWriter.WriteElementString("DisplayStockAvailability", null, productVariant.DisplayStockAvailability.ToString());
                            xmlWriter.WriteElementString("DisplayStockQuantity", null, productVariant.DisplayStockQuantity.ToString());
                            xmlWriter.WriteElementString("MinStockQuantity", null, productVariant.MinStockQuantity.ToString());
                            xmlWriter.WriteElementString("LowStockActivityId", null, productVariant.LowStockActivityId.ToString());
                            xmlWriter.WriteElementString("NotifyAdminForQuantityBelow", null, productVariant.NotifyAdminForQuantityBelow.ToString());
                            xmlWriter.WriteElementString("BackorderModeId", null, productVariant.BackorderModeId.ToString());
                            xmlWriter.WriteElementString("AllowBackInStockSubscriptions", null, productVariant.AllowBackInStockSubscriptions.ToString());
                            xmlWriter.WriteElementString("OrderMinimumQuantity", null, productVariant.OrderMinimumQuantity.ToString());
                            xmlWriter.WriteElementString("OrderMaximumQuantity", null, productVariant.OrderMaximumQuantity.ToString());
                            xmlWriter.WriteElementString("DisableBuyButton", null, productVariant.DisableBuyButton.ToString());
                            xmlWriter.WriteElementString("DisableWishlistButton", null, productVariant.DisableWishlistButton.ToString());
                            xmlWriter.WriteElementString("CallForPrice", null, productVariant.CallForPrice.ToString());
                            xmlWriter.WriteElementString("Price", null, productVariant.Price.ToString());
                            xmlWriter.WriteElementString("OldPrice", null, productVariant.OldPrice.ToString());
                            xmlWriter.WriteElementString("ProductCost", null, productVariant.ProductCost.ToString());
                            xmlWriter.WriteElementString("SpecialPrice", null, productVariant.SpecialPrice.HasValue ? productVariant.SpecialPrice.ToString() : "");
                            xmlWriter.WriteElementString("SpecialPriceStartDateTimeUtc", null, productVariant.SpecialPriceStartDateTimeUtc.HasValue ? productVariant.SpecialPriceStartDateTimeUtc.ToString() : "");
                            xmlWriter.WriteElementString("SpecialPriceEndDateTimeUtc", null, productVariant.SpecialPriceEndDateTimeUtc.HasValue ? productVariant.SpecialPriceEndDateTimeUtc.ToString() : "");
                            xmlWriter.WriteElementString("CustomerEntersPrice", null, productVariant.CustomerEntersPrice.ToString());
                            xmlWriter.WriteElementString("MinimumCustomerEnteredPrice", null, productVariant.MinimumCustomerEnteredPrice.ToString());
                            xmlWriter.WriteElementString("MaximumCustomerEnteredPrice", null, productVariant.MaximumCustomerEnteredPrice.ToString());
                            xmlWriter.WriteElementString("Weight", null, productVariant.Weight.ToString());
                            xmlWriter.WriteElementString("Length", null, productVariant.Length.ToString());
                            xmlWriter.WriteElementString("Width", null, productVariant.Width.ToString());
                            xmlWriter.WriteElementString("Height", null, productVariant.Height.ToString());
                            xmlWriter.WriteElementString("PictureId", null, productVariant.PictureId.ToString());
                            xmlWriter.WriteElementString("Published", null, productVariant.Published.ToString());
                            xmlWriter.WriteElementString("Deleted", null, productVariant.Deleted.ToString());
                            xmlWriter.WriteElementString("DisplayOrder", null, productVariant.DisplayOrder.ToString());
                            xmlWriter.WriteElementString("CreatedOnUtc", null, productVariant.CreatedOnUtc.ToString());
                            xmlWriter.WriteElementString("UpdatedOnUtc", null, productVariant.UpdatedOnUtc.ToString());

                            //pictures 
                            var pictures = productVariant.GetProductVariantPictures(_pictureService);
                            if (pictures != null)
                            {
                                xmlWriter.WriteStartElement("Pictures");

                                foreach (var picture in pictures)
                                {
                                    xmlWriter.WriteElementString("PicturePath", null, _pictureService.GetPictureUrl(picture));
                                }
                                xmlWriter.WriteEndElement();
                            }


                            xmlWriter.WriteStartElement("ProductDiscounts");
                            var discounts = productVariant.AppliedDiscounts;
                            foreach (var discount in discounts)
                            {
                                xmlWriter.WriteElementString("DiscountId", null, discount.Id.ToString());
                            }
                            xmlWriter.WriteEndElement();


                            xmlWriter.WriteStartElement("TierPrices");
                            var tierPrices = productVariant.TierPrices;
                            foreach (var tierPrice in tierPrices)
                            {
                                xmlWriter.WriteElementString("TierPriceId", null, tierPrice.Id.ToString());
                                xmlWriter.WriteElementString("CustomerRoleId", null, tierPrice.CustomerRoleId.HasValue ? tierPrice.CustomerRoleId.ToString() : "0");
                                xmlWriter.WriteElementString("Quantity", null, tierPrice.Quantity.ToString());
                                xmlWriter.WriteElementString("Price", null, tierPrice.Price.ToString());
                            }
                            xmlWriter.WriteEndElement();

                            xmlWriter.WriteStartElement("ProductAttributes");
                            var productVariantAttributes = productVariant.ProductVariantAttributes;
                            foreach (var productVariantAttribute in productVariantAttributes)
                            {
                                xmlWriter.WriteStartElement("ProductVariantAttribute");
                                xmlWriter.WriteElementString("ProductVariantAttributeId", null, productVariantAttribute.Id.ToString());
                                xmlWriter.WriteElementString("ProductAttributeId", null, productVariantAttribute.ProductAttributeId.ToString());
                                xmlWriter.WriteElementString("TextPrompt", null, productVariantAttribute.TextPrompt);
                                xmlWriter.WriteElementString("IsRequired", null, productVariantAttribute.IsRequired.ToString());
                                xmlWriter.WriteElementString("AttributeControlTypeId", null, productVariantAttribute.AttributeControlTypeId.ToString());
                                xmlWriter.WriteElementString("DisplayOrder", null, productVariantAttribute.DisplayOrder.ToString());


                                xmlWriter.WriteStartElement("ProductVariantAttributeValues");
                                var productVariantAttributeValues = productVariantAttribute.ProductVariantAttributeValues;
                                foreach (var productVariantAttributeValue in productVariantAttributeValues)
                                {
                                    xmlWriter.WriteElementString("ProductVariantAttributeValueId", null, productVariantAttributeValue.Id.ToString());
                                    xmlWriter.WriteElementString("Name", null, productVariantAttributeValue.Name);
                                    xmlWriter.WriteElementString("PriceAdjustment", null, productVariantAttributeValue.PriceAdjustment.ToString());
                                    xmlWriter.WriteElementString("WeightAdjustment", null, productVariantAttributeValue.WeightAdjustment.ToString());
                                    xmlWriter.WriteElementString("IsPreSelected", null, productVariantAttributeValue.IsPreSelected.ToString());
                                    xmlWriter.WriteElementString("DisplayOrder", null, productVariantAttributeValue.DisplayOrder.ToString());
                                }
                                xmlWriter.WriteEndElement();


                                xmlWriter.WriteEndElement();
                            }
                            xmlWriter.WriteEndElement();

                            //product attribute combinations 
                            var combinations = productVariant.ProductVariantAttributeCombinations;
                            if (combinations != null)
                            {
                                xmlWriter.WriteStartElement("ProductAttributeCombinations");
                                foreach (var comb in combinations)
                                {
                                    xmlWriter.WriteStartElement("Combination");
                                    //xmlWriter.WriteRaw(comb.AttributesXml);

                                    var pvaValues = productAttributeParser.ParseProductVariantAttributeValues(comb.AttributesXml);
                                    foreach (var pvav in pvaValues)
                                    {
                                        xmlWriter.WriteStartElement("ProductVariantAttribute");

                                        //xmlWriter.WriteAttributeString("value", pvav.Name);
                                        xmlWriter.WriteAttributeString("name", pvav.ProductVariantAttribute.ProductAttribute.Name);
                                        xmlWriter.WriteAttributeString("Id", pvav.Id.ToString());
                                        xmlWriter.WriteValue(pvav.Name);
                                        xmlWriter.WriteEndElement();

                                    }

                                    xmlWriter.WriteElementString("Stock", null, comb.StockQuantity.ToString());
                                    xmlWriter.WriteEndElement();

                                }
                                xmlWriter.WriteEndElement();
                            }

                            xmlWriter.WriteEndElement();
                        }
                    }
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("ProductPictures");
                    var productPictures = product.ProductPictures;
                    foreach (var productPicture in productPictures)
                    {
                        xmlWriter.WriteStartElement("ProductPicture");
                        xmlWriter.WriteElementString("ProductPictureId", null, productPicture.Id.ToString());
                        xmlWriter.WriteElementString("PictureId", null, productPicture.PictureId.ToString());
                        xmlWriter.WriteElementString("DisplayOrder", null, productPicture.DisplayOrder.ToString());
                        xmlWriter.WriteEndElement();
                    }
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("ProductCategories");
                    var productCategories = _categoryService.GetProductCategoriesByProductId(product.Id);
                    if (productCategories != null)
                    {
                        foreach (var productCategory in productCategories)
                        {
                            xmlWriter.WriteStartElement("ProductCategory");
                            xmlWriter.WriteAttributeString("Name", productCategory.Category.Name);
                            xmlWriter.WriteElementString("ProductCategoryId", null, productCategory.Id.ToString());
                            xmlWriter.WriteElementString("CategoryId", null, productCategory.CategoryId.ToString());
                            xmlWriter.WriteElementString("IsFeaturedProduct", null, productCategory.IsFeaturedProduct.ToString());
                            xmlWriter.WriteElementString("DisplayOrder", null, productCategory.DisplayOrder.ToString());
                            xmlWriter.WriteEndElement();
                        }
                    }
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("ProductManufacturers");
                    var productManufacturers = _manufacturerService.GetProductManufacturersByProductId(product.Id);
                    if (productManufacturers != null)
                    {
                        foreach (var productManufacturer in productManufacturers)
                        {
                            xmlWriter.WriteStartElement("ProductManufacturer");
                            xmlWriter.WriteElementString("ProductManufacturerId", null, productManufacturer.Id.ToString());
                            xmlWriter.WriteElementString("ManufacturerId", null, productManufacturer.ManufacturerId.ToString());
                            xmlWriter.WriteElementString("IsFeaturedProduct", null, productManufacturer.IsFeaturedProduct.ToString());
                            xmlWriter.WriteElementString("DisplayOrder", null, productManufacturer.DisplayOrder.ToString());
                            xmlWriter.WriteEndElement();
                        }
                    }
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("ProductSpecificationAttributes");
                    var productSpecificationAttributes = product.ProductSpecificationAttributes;
                    foreach (var productSpecificationAttribute in productSpecificationAttributes)
                    {
                        xmlWriter.WriteStartElement("ProductSpecificationAttribute");
                        xmlWriter.WriteElementString("ProductSpecificationAttributeId", null, productSpecificationAttribute.Id.ToString());
                        xmlWriter.WriteElementString("SpecificationAttributeOptionId", null, productSpecificationAttribute.SpecificationAttributeOptionId.ToString());
                        xmlWriter.WriteElementString("AllowFiltering", null, productSpecificationAttribute.AllowFiltering.ToString());
                        xmlWriter.WriteElementString("ShowOnProductPage", null, productSpecificationAttribute.ShowOnProductPage.ToString());
                        xmlWriter.WriteElementString("DisplayOrder", null, productSpecificationAttribute.DisplayOrder.ToString());
                        xmlWriter.WriteEndElement();
                    }
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteEndElement();
                }

                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndDocument();
                xmlWriter.Close();
                return stringWriter.ToString();
            }
            catch (Exception ex)
            {
                return String.Empty;
            }

        }

        public virtual string ExportProductsToXmlForN11(IList<Product> products)
        {
            try
            {
                var webHelper = EngineContext.Current.Resolve<IWebHelper>();
                var productAttributeParser = EngineContext.Current.Resolve<IProductAttributeParser>();
                var sb = new StringBuilder();
                var stringWriter = new StringWriter(sb);
                var xmlWriter = new XmlTextWriter(stringWriter);
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("Products");
                xmlWriter.WriteAttributeString("Version", NopVersion.CurrentVersion);
                string brandName="";
                foreach (var product in products)
                {
                    xmlWriter.WriteStartElement("Product");
                    if (product.ProductSpecificationAttributes.Where(x => x.SpecificationAttributeOption.SpecificationAttributeId == 11).FirstOrDefault() != null)
                    {
                        brandName = product.ProductSpecificationAttributes.Where(x => x.SpecificationAttributeOption.SpecificationAttributeId == 11).FirstOrDefault().SpecificationAttributeOption.Name;
                    }

                    xmlWriter.WriteElementString("ProductId", null,  product.Id.ToString());
                    xmlWriter.WriteElementString("Name", null, brandName + " " + product.Name);
                    //xmlWriter.WriteElementString("ShortDescription", null, product.ShortDescription);
                    //xmlWriter.WriteElementString("FullDescription", null, product.FullDescription);
                    //xmlWriter.WriteElementString("AdminComment", null, product.AdminComment);
                    //xmlWriter.WriteElementString("ProductTemplateId", null, product.ProductTemplateId.ToString());
                    //xmlWriter.WriteElementString("ShowOnHomePage", null, product.ShowOnHomePage.ToString());
                    xmlWriter.WriteElementString("MetaKeywords", null, product.MetaKeywords);
                    xmlWriter.WriteElementString("MetaDescription", null, product.MetaDescription);
                    xmlWriter.WriteElementString("MetaTitle", null, product.MetaTitle);
                    //xmlWriter.WriteElementString("SEName", null, product.SeName);
                    //xmlWriter.WriteElementString("AllowCustomerReviews", null, product.AllowCustomerReviews.ToString());
                    xmlWriter.WriteElementString("Published", null, product.Published.ToString());
                    //xmlWriter.WriteElementString("CreatedOnUtc", null, product.CreatedOnUtc.ToString());
                    //xmlWriter.WriteElementString("UpdatedOnUtc", null, product.UpdatedOnUtc.ToString());

                    xmlWriter.WriteStartElement("ProductVariants");
                    var productVariants = _productService.GetProductVariantsByProductId(product.Id, true);
                    if (productVariants != null)
                    {
                        foreach (var productVariant in productVariants)
                        {
                            xmlWriter.WriteStartElement("ProductVariant");
                            //xmlWriter.WriteElementString("ProductVariantId", null, productVariant.Id.ToString());
                            //xmlWriter.WriteElementString("ProductId", null, productVariant.ProductId.ToString());
                            //xmlWriter.WriteElementString("Name", null, productVariant.Name);
                            xmlWriter.WriteElementString("SKU", null, productVariant.Sku);
                            //xmlWriter.WriteElementString("Description", null, productVariant.Description);
                            //xmlWriter.WriteElementString("AdminComment", null, productVariant.AdminComment);
                            //xmlWriter.WriteElementString("ManufacturerPartNumber", null, productVariant.ManufacturerPartNumber);
                            //xmlWriter.WriteElementString("Gtin", null, productVariant.Gtin);
                            //xmlWriter.WriteElementString("IsGiftCard", null, productVariant.IsGiftCard.ToString());
                            //xmlWriter.WriteElementString("GiftCardType", null, productVariant.GiftCardType.ToString());
                            //xmlWriter.WriteElementString("RequireOtherProducts", null, productVariant.RequireOtherProducts.ToString());
                            //xmlWriter.WriteElementString("RequiredProductVariantIds", null, productVariant.RequiredProductVariantIds);
                            //xmlWriter.WriteElementString("AutomaticallyAddRequiredProductVariants", null, productVariant.AutomaticallyAddRequiredProductVariants.ToString());
                            //xmlWriter.WriteElementString("IsDownload", null, productVariant.IsDownload.ToString());
                            //xmlWriter.WriteElementString("DownloadId", null, productVariant.DownloadId.ToString());
                            //xmlWriter.WriteElementString("UnlimitedDownloads", null, productVariant.UnlimitedDownloads.ToString());
                            //xmlWriter.WriteElementString("MaxNumberOfDownloads", null, productVariant.MaxNumberOfDownloads.ToString());
                            //if (productVariant.DownloadExpirationDays.HasValue)
                            //    xmlWriter.WriteElementString("DownloadExpirationDays", null, productVariant.DownloadExpirationDays.ToString());
                            //else
                            //    xmlWriter.WriteElementString("DownloadExpirationDays", null, string.Empty);
                            //xmlWriter.WriteElementString("DownloadActivationType", null, productVariant.DownloadActivationType.ToString());
                            //xmlWriter.WriteElementString("HasSampleDownload", null, productVariant.HasSampleDownload.ToString());
                            //xmlWriter.WriteElementString("SampleDownloadId", null, productVariant.SampleDownloadId.ToString());
                            //xmlWriter.WriteElementString("HasUserAgreement", null, productVariant.HasUserAgreement.ToString());
                            //xmlWriter.WriteElementString("UserAgreementText", null, productVariant.UserAgreementText);
                            //xmlWriter.WriteElementString("IsRecurring", null, productVariant.IsRecurring.ToString());
                            //xmlWriter.WriteElementString("RecurringCycleLength", null, productVariant.RecurringCycleLength.ToString());
                            //xmlWriter.WriteElementString("RecurringCyclePeriodId", null, productVariant.RecurringCyclePeriodId.ToString());
                            //xmlWriter.WriteElementString("RecurringTotalCycles", null, productVariant.RecurringTotalCycles.ToString());
                            //xmlWriter.WriteElementString("IsShipEnabled", null, productVariant.IsShipEnabled.ToString());
                            //xmlWriter.WriteElementString("IsFreeShipping", null, productVariant.IsFreeShipping.ToString());
                            //xmlWriter.WriteElementString("AdditionalShippingCharge", null, productVariant.AdditionalShippingCharge.ToString());
                            //xmlWriter.WriteElementString("IsTaxExempt", null, productVariant.IsTaxExempt.ToString());
                            //xmlWriter.WriteElementString("TaxCategoryId", null, productVariant.TaxCategoryId.ToString());
                            //xmlWriter.WriteElementString("ManageInventoryMethodId", null, productVariant.ManageInventoryMethodId.ToString());
                            xmlWriter.WriteElementString("StockQuantity", null, productVariant.StockQuantity.ToString());
                            int stokForKombinasyonsuzUrunIcın = productVariant.StockQuantity; // N11 TALEBİ ÜZERİNE KOMBİNASYONSUZ (VARYANTSIZ) URUNLERDE DE STOK KOMBINASYON TAGI ALTINDA GELSIN...
                            //xmlWriter.WriteElementString("DisplayStockAvailability", null, productVariant.DisplayStockAvailability.ToString());
                            //xmlWriter.WriteElementString("DisplayStockQuantity", null, productVariant.DisplayStockQuantity.ToString());
                            //xmlWriter.WriteElementString("MinStockQuantity", null, productVariant.MinStockQuantity.ToString());
                            //xmlWriter.WriteElementString("LowStockActivityId", null, productVariant.LowStockActivityId.ToString());
                            //xmlWriter.WriteElementString("NotifyAdminForQuantityBelow", null, productVariant.NotifyAdminForQuantityBelow.ToString());
                            //xmlWriter.WriteElementString("BackorderModeId", null, productVariant.BackorderModeId.ToString());
                            //xmlWriter.WriteElementString("AllowBackInStockSubscriptions", null, productVariant.AllowBackInStockSubscriptions.ToString());
                            //xmlWriter.WriteElementString("OrderMinimumQuantity", null, productVariant.OrderMinimumQuantity.ToString());
                            //xmlWriter.WriteElementString("OrderMaximumQuantity", null, productVariant.OrderMaximumQuantity.ToString());
                            //xmlWriter.WriteElementString("DisableBuyButton", null, productVariant.DisableBuyButton.ToString());
                            //xmlWriter.WriteElementString("DisableWishlistButton", null, productVariant.DisableWishlistButton.ToString());
                            //xmlWriter.WriteElementString("CallForPrice", null, productVariant.CallForPrice.ToString());
                            xmlWriter.WriteElementString("Price", null, productVariant.Price.ToString());
                            xmlWriter.WriteElementString("Tax", null, _taxService.GetTaxRate(productVariant, productVariant.TaxCategoryId, null).ToString());

                            
                            decimal taxRate = decimal.Zero;
                            decimal finalPriceWithDiscountBase = _taxService.GetProductPrice(productVariant, _priceCalculationService.GetFinalPrice(productVariant, true, false), out taxRate);
                            decimal finalPriceWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithDiscountBase, _workContext.WorkingCurrency);

                            if (finalPriceWithDiscount.ToString() != null)
                            {
                                xmlWriter.WriteElementString("DiscountPrice",null, finalPriceWithDiscount.ToString());
                            }
                            
                            xmlWriter.WriteElementString("OldPrice", null, productVariant.OldPrice.ToString());
                            //xmlWriter.WriteElementString("ProductCost", null, productVariant.ProductCost.ToString());
                            //xmlWriter.WriteElementString("SpecialPrice", null, productVariant.SpecialPrice.HasValue ? productVariant.SpecialPrice.ToString() : "");
                            //xmlWriter.WriteElementString("SpecialPriceStartDateTimeUtc", null, productVariant.SpecialPriceStartDateTimeUtc.HasValue ? productVariant.SpecialPriceStartDateTimeUtc.ToString() : "");
                            //xmlWriter.WriteElementString("SpecialPriceEndDateTimeUtc", null, productVariant.SpecialPriceEndDateTimeUtc.HasValue ? productVariant.SpecialPriceEndDateTimeUtc.ToString() : "");
                            //xmlWriter.WriteElementString("CustomerEntersPrice", null, productVariant.CustomerEntersPrice.ToString());
                            //xmlWriter.WriteElementString("MinimumCustomerEnteredPrice", null, productVariant.MinimumCustomerEnteredPrice.ToString());
                            //xmlWriter.WriteElementString("MaximumCustomerEnteredPrice", null, productVariant.MaximumCustomerEnteredPrice.ToString());
                            //xmlWriter.WriteElementString("Weight", null, productVariant.Weight.ToString());
                            //xmlWriter.WriteElementString("Length", null, productVariant.Length.ToString());
                            //xmlWriter.WriteElementString("Width", null, productVariant.Width.ToString());
                            //xmlWriter.WriteElementString("Height", null, productVariant.Height.ToString());
                            //xmlWriter.WriteElementString("PictureId", null, productVariant.PictureId.ToString());
                            //xmlWriter.WriteElementString("Published", null, productVariant.Published.ToString());
                            //xmlWriter.WriteElementString("Deleted", null, productVariant.Deleted.ToString());
                            //xmlWriter.WriteElementString("DisplayOrder", null, productVariant.DisplayOrder.ToString());
                            //xmlWriter.WriteElementString("CreatedOnUtc", null, productVariant.CreatedOnUtc.ToString());
                            //xmlWriter.WriteElementString("UpdatedOnUtc", null, productVariant.UpdatedOnUtc.ToString());

                            //pictures 
                            var pictures = productVariant.GetProductVariantPictures(_pictureService);
                            if (pictures != null)
                            {
                                xmlWriter.WriteStartElement("Pictures");

                                foreach (var picture in pictures)
                                {
                                    xmlWriter.WriteElementString("PicturePath", null, _pictureService.GetPictureUrl(picture));
                                }
                                xmlWriter.WriteEndElement();
                            }


                            //xmlWriter.WriteStartElement("ProductDiscounts");
                            //var discounts = productVariant.AppliedDiscounts;
                            //foreach (var discount in discounts)
                            //{
                            //    xmlWriter.WriteElementString("DiscountId", null, discount.Id.ToString());
                            //}
                            //xmlWriter.WriteEndElement();


                            //xmlWriter.WriteStartElement("TierPrices");
                            //var tierPrices = productVariant.TierPrices;
                            //foreach (var tierPrice in tierPrices)
                            //{
                            //    xmlWriter.WriteElementString("TierPriceId", null, tierPrice.Id.ToString());
                            //    xmlWriter.WriteElementString("CustomerRoleId", null, tierPrice.CustomerRoleId.HasValue ? tierPrice.CustomerRoleId.ToString() : "0");
                            //    xmlWriter.WriteElementString("Quantity", null, tierPrice.Quantity.ToString());
                            //    xmlWriter.WriteElementString("Price", null, tierPrice.Price.ToString());
                            //}
                            //xmlWriter.WriteEndElement();

                            //xmlWriter.WriteStartElement("ProductAttributes");
                            //var productVariantAttributes = productVariant.ProductVariantAttributes;
                            //foreach (var productVariantAttribute in productVariantAttributes)
                            //{
                            //    xmlWriter.WriteStartElement("ProductVariantAttribute");
                            //    xmlWriter.WriteElementString("ProductVariantAttributeId", null, productVariantAttribute.Id.ToString());
                            //    xmlWriter.WriteElementString("ProductAttributeId", null, productVariantAttribute.ProductAttributeId.ToString());
                            //    xmlWriter.WriteElementString("TextPrompt", null, productVariantAttribute.TextPrompt);
                            //    xmlWriter.WriteElementString("IsRequired", null, productVariantAttribute.IsRequired.ToString());
                            //    xmlWriter.WriteElementString("AttributeControlTypeId", null, productVariantAttribute.AttributeControlTypeId.ToString());
                            //    xmlWriter.WriteElementString("DisplayOrder", null, productVariantAttribute.DisplayOrder.ToString());


                            //    xmlWriter.WriteStartElement("ProductVariantAttributeValues");
                            //    var productVariantAttributeValues = productVariantAttribute.ProductVariantAttributeValues;
                            //    foreach (var productVariantAttributeValue in productVariantAttributeValues)
                            //    {
                            //        xmlWriter.WriteElementString("ProductVariantAttributeValueId", null, productVariantAttributeValue.Id.ToString());
                            //        xmlWriter.WriteElementString("Name", null, productVariantAttributeValue.Name);
                            //        xmlWriter.WriteElementString("PriceAdjustment", null, productVariantAttributeValue.PriceAdjustment.ToString());
                            //        xmlWriter.WriteElementString("WeightAdjustment", null, productVariantAttributeValue.WeightAdjustment.ToString());
                            //        xmlWriter.WriteElementString("IsPreSelected", null, productVariantAttributeValue.IsPreSelected.ToString());
                            //        xmlWriter.WriteElementString("DisplayOrder", null, productVariantAttributeValue.DisplayOrder.ToString());
                            //    }
                            //    xmlWriter.WriteEndElement();


                            //    xmlWriter.WriteEndElement();
                            //}
                            //xmlWriter.WriteEndElement();

                            //product attribute combinations 
                            var combinations = productVariant.ProductVariantAttributeCombinations;
                            if (combinations != null)
                            {
                                if (combinations.Count > 0)
                                { 
                                    xmlWriter.WriteStartElement("ProductAttributeCombinations");
                                    foreach (var comb in combinations)
                                    {
                                        xmlWriter.WriteStartElement("Combination");
                                        //xmlWriter.WriteRaw(comb.AttributesXml);

                                        var pvaValues = productAttributeParser.ParseProductVariantAttributeValues(comb.AttributesXml);
                                        foreach (var pvav in pvaValues)
                                        {
                                            xmlWriter.WriteStartElement("ProductVariantAttribute");

                                            //xmlWriter.WriteAttributeString("value", pvav.Name);
                                            xmlWriter.WriteAttributeString("name", pvav.ProductVariantAttribute.ProductAttribute.Name);
                                            //30.05.2014 tarihinden n11 Zafer Çoban dan Serdar'ın forward ettiği maile binayen değişiklik başlangıcı.
                                            //xmlWriter.WriteAttributeString("Id", pvav.Id.ToString());
                                            //xmlWriter.WriteValue(pvav.Name);
                                            xmlWriter.WriteAttributeString("value", pvav.Name.ToString());
                                            //30.05.2014 tarihinden n11 Zafer Çoban dan Serdar'ın forward ettiği maile binayen değişiklik bitişi.
                                            xmlWriter.WriteEndElement();

                                        }

                                        xmlWriter.WriteElementString("Stock", null, comb.StockQuantity.ToString());
                                        xmlWriter.WriteEndElement();

                                    }
                                    xmlWriter.WriteEndElement();
                                }
                                else  // KOMBINASYONSUZ (VARYANTSIZ) OLSA BILE STOK ADETI KOMBINASYON TAGI ALTINDA GELMELI (N11 TALEBI)
                                {
                                    //stokForKombinasyonsuzUrunIcın
                                    xmlWriter.WriteStartElement("ProductAttributeCombinations");
                                    xmlWriter.WriteStartElement("Combination");
                                    xmlWriter.WriteElementString("Stock", null, stokForKombinasyonsuzUrunIcın.ToString());
                                    xmlWriter.WriteEndElement();
                                    xmlWriter.WriteEndElement();
                                }
                            }
                            xmlWriter.WriteEndElement();
                        }
                    }
                    xmlWriter.WriteEndElement();

                    //xmlWriter.WriteStartElement("ProductPictures");
                    //var productPictures = product.ProductPictures;
                    //foreach (var productPicture in productPictures)
                    //{
                    //    xmlWriter.WriteStartElement("ProductPicture");
                    //    xmlWriter.WriteElementString("ProductPictureId", null, productPicture.Id.ToString());
                    //    xmlWriter.WriteElementString("PictureId", null, productPicture.PictureId.ToString());
                    //    xmlWriter.WriteElementString("DisplayOrder", null, productPicture.DisplayOrder.ToString());
                    //    xmlWriter.WriteEndElement();
                    //}
                    //xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("ProductCategories");
                    var productCategories = _categoryService.GetProductCategoriesByProductId(product.Id);
                    if (productCategories != null)
                    {
                        foreach (var productCategory in productCategories)
                        {
                            xmlWriter.WriteStartElement("ProductCategory");
                            xmlWriter.WriteAttributeString("Name", productCategory.Category.Name);
                            //xmlWriter.WriteElementString("ProductCategoryId", null, productCategory.Id.ToString());
                            //xmlWriter.WriteElementString("CategoryId", null, productCategory.CategoryId.ToString());
                            //xmlWriter.WriteElementString("IsFeaturedProduct", null, productCategory.IsFeaturedProduct.ToString());
                            //xmlWriter.WriteElementString("DisplayOrder", null, productCategory.DisplayOrder.ToString());
                            xmlWriter.WriteEndElement();
                        }
                    }
                    xmlWriter.WriteEndElement();

                    //xmlWriter.WriteStartElement("ProductManufacturers");
                    //var productManufacturers = _manufacturerService.GetProductManufacturersByProductId(product.Id);
                    //if (productManufacturers != null)
                    //{
                    //    foreach (var productManufacturer in productManufacturers)
                    //    {
                    //        xmlWriter.WriteStartElement("ProductManufacturer");
                    //        xmlWriter.WriteElementString("ProductManufacturerId", null, productManufacturer.Id.ToString());
                    //        xmlWriter.WriteElementString("ManufacturerId", null, productManufacturer.ManufacturerId.ToString());
                    //        xmlWriter.WriteElementString("IsFeaturedProduct", null, productManufacturer.IsFeaturedProduct.ToString());
                    //        xmlWriter.WriteElementString("DisplayOrder", null, productManufacturer.DisplayOrder.ToString());
                    //        xmlWriter.WriteEndElement();
                    //    }
                    //}
                    //xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("ProductSpecificationAttributes");
                    var productSpecificationAttributes = product.ProductSpecificationAttributes;
                    foreach (var productSpecificationAttribute in productSpecificationAttributes)
                    {
                        xmlWriter.WriteStartElement("ProductSpecificationAttribute");
                        //xmlWriter.WriteElementString("ProductSpecificationAttributeId", null, productSpecificationAttribute.Id.ToString());
                        //xmlWriter.WriteElementString("SpecificationAttributeOptionId", null, productSpecificationAttribute.SpecificationAttributeOptionId.ToString());

                        xmlWriter.WriteElementString("ProductSpecificationAttributeId", null, productSpecificationAttribute.SpecificationAttributeOption.SpecificationAttribute.Name.ToString());
                        xmlWriter.WriteElementString("SpecificationAttributeOptionId", null, productSpecificationAttribute.SpecificationAttributeOption.Name.ToString());

                        //xmlWriter.WriteElementString("AllowFiltering", null, productSpecificationAttribute.AllowFiltering.ToString());
                        //xmlWriter.WriteElementString("ShowOnProductPage", null, productSpecificationAttribute.ShowOnProductPage.ToString());
                        //xmlWriter.WriteElementString("DisplayOrder", null, productSpecificationAttribute.DisplayOrder.ToString());
                        xmlWriter.WriteEndElement();
                    }
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteEndElement();
                }

                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndDocument();
                xmlWriter.Close();
                return stringWriter.ToString();
            }
            catch (Exception ex)
            {
                return String.Empty;
            }

        }

        //public virtual string ExportProductsToXmlForGG(IList<Product> products)
        //{
        //    try
        //    {
        //        var webHelper = EngineContext.Current.Resolve<IWebHelper>();
        //        var productAttributeParser = EngineContext.Current.Resolve<IProductAttributeParser>();
        //        var sb = new StringBuilder();
        //        var stringWriter = new StringWriter(sb);
        //        var xmlWriter = new XmlTextWriter(stringWriter);
        //        xmlWriter.WriteStartDocument();
        //        xmlWriter.WriteStartElement("Products");
        //        xmlWriter.WriteAttributeString("Version", NopVersion.CurrentVersion);
        //        string brandName = "";
        //        foreach (var product in products)
        //        {
        //            xmlWriter.WriteStartElement("Product");
        //            if (product.ProductSpecificationAttributes.Where(x => x.SpecificationAttributeOption.SpecificationAttributeId == 11).FirstOrDefault() != null)
        //            {
        //                brandName = product.ProductSpecificationAttributes.Where(x => x.SpecificationAttributeOption.SpecificationAttributeId == 11).FirstOrDefault().SpecificationAttributeOption.Name;
        //            }

        //            xmlWriter.WriteElementString("ProductId", null, product.Id.ToString());
        //            xmlWriter.WriteElementString("Name", null, brandName + " " + product.Name);
        //            xmlWriter.WriteElementString("ShortDescription", null, product.ShortDescription);
        //            xmlWriter.WriteElementString("FullDescription", null, product.FullDescription);
        //            xmlWriter.WriteElementString("AdminComment", null, product.AdminComment);
        //            xmlWriter.WriteElementString("ProductTemplateId", null, product.ProductTemplateId.ToString());
        //            xmlWriter.WriteElementString("ShowOnHomePage", null, product.ShowOnHomePage.ToString());
        //            xmlWriter.WriteElementString("MetaKeywords", null, product.MetaKeywords);
        //            xmlWriter.WriteElementString("MetaDescription", null, product.MetaDescription);
        //            xmlWriter.WriteElementString("MetaTitle", null, product.MetaTitle);
        //            xmlWriter.WriteElementString("SEName", null, product.SeName);
        //            xmlWriter.WriteElementString("AllowCustomerReviews", null, product.AllowCustomerReviews.ToString());
        //            xmlWriter.WriteElementString("Published", null, product.Published.ToString());
        //            xmlWriter.WriteElementString("CreatedOnUtc", null, product.CreatedOnUtc.ToString());
        //            xmlWriter.WriteElementString("UpdatedOnUtc", null, product.UpdatedOnUtc.ToString());

        //            xmlWriter.WriteStartElement("ProductVariants");
        //            var productVariants = _productService.GetProductVariantsByProductId(product.Id, true);
        //            if (productVariants != null)
        //            {
        //                foreach (var productVariant in productVariants)
        //                {
        //                    xmlWriter.WriteStartElement("ProductVariant");
        //                    xmlWriter.WriteElementString("ProductVariantId", null, productVariant.Id.ToString());
        //                    xmlWriter.WriteElementString("ProductId", null, productVariant.ProductId.ToString());
        //                    xmlWriter.WriteElementString("Name", null, productVariant.Name);
        //                    xmlWriter.WriteElementString("SKU", null, productVariant.Sku);
        //                    xmlWriter.WriteElementString("Description", null, productVariant.Description);
        //                    xmlWriter.WriteElementString("AdminComment", null, productVariant.AdminComment);
        //                    xmlWriter.WriteElementString("ManufacturerPartNumber", null, productVariant.ManufacturerPartNumber);
        //                    xmlWriter.WriteElementString("Gtin", null, productVariant.Gtin);
        //                    xmlWriter.WriteElementString("IsGiftCard", null, productVariant.IsGiftCard.ToString());
        //                    xmlWriter.WriteElementString("GiftCardType", null, productVariant.GiftCardType.ToString());
        //                    xmlWriter.WriteElementString("RequireOtherProducts", null, productVariant.RequireOtherProducts.ToString());
        //                    xmlWriter.WriteElementString("RequiredProductVariantIds", null, productVariant.RequiredProductVariantIds);
        //                    xmlWriter.WriteElementString("AutomaticallyAddRequiredProductVariants", null, productVariant.AutomaticallyAddRequiredProductVariants.ToString());
        //                    xmlWriter.WriteElementString("IsDownload", null, productVariant.IsDownload.ToString());
        //                    xmlWriter.WriteElementString("DownloadId", null, productVariant.DownloadId.ToString());
        //                    xmlWriter.WriteElementString("UnlimitedDownloads", null, productVariant.UnlimitedDownloads.ToString());
        //                    xmlWriter.WriteElementString("MaxNumberOfDownloads", null, productVariant.MaxNumberOfDownloads.ToString());
        //                    if (productVariant.DownloadExpirationDays.HasValue)
        //                        xmlWriter.WriteElementString("DownloadExpirationDays", null, productVariant.DownloadExpirationDays.ToString());
        //                    else
        //                        xmlWriter.WriteElementString("DownloadExpirationDays", null, string.Empty);
        //                    xmlWriter.WriteElementString("DownloadActivationType", null, productVariant.DownloadActivationType.ToString());
        //                    xmlWriter.WriteElementString("HasSampleDownload", null, productVariant.HasSampleDownload.ToString());
        //                    xmlWriter.WriteElementString("SampleDownloadId", null, productVariant.SampleDownloadId.ToString());
        //                    xmlWriter.WriteElementString("HasUserAgreement", null, productVariant.HasUserAgreement.ToString());
        //                    xmlWriter.WriteElementString("UserAgreementText", null, productVariant.UserAgreementText);
        //                    xmlWriter.WriteElementString("IsRecurring", null, productVariant.IsRecurring.ToString());
        //                    xmlWriter.WriteElementString("RecurringCycleLength", null, productVariant.RecurringCycleLength.ToString());
        //                    xmlWriter.WriteElementString("RecurringCyclePeriodId", null, productVariant.RecurringCyclePeriodId.ToString());
        //                    xmlWriter.WriteElementString("RecurringTotalCycles", null, productVariant.RecurringTotalCycles.ToString());
        //                    xmlWriter.WriteElementString("IsShipEnabled", null, productVariant.IsShipEnabled.ToString());
        //                    xmlWriter.WriteElementString("IsFreeShipping", null, productVariant.IsFreeShipping.ToString());
        //                    xmlWriter.WriteElementString("AdditionalShippingCharge", null, productVariant.AdditionalShippingCharge.ToString());
        //                    xmlWriter.WriteElementString("IsTaxExempt", null, productVariant.IsTaxExempt.ToString());
        //                    xmlWriter.WriteElementString("TaxCategoryId", null, productVariant.TaxCategoryId.ToString());
        //                    xmlWriter.WriteElementString("ManageInventoryMethodId", null, productVariant.ManageInventoryMethodId.ToString());
        //                    xmlWriter.WriteElementString("StockQuantity", null, productVariant.StockQuantity.ToString());
        //                    xmlWriter.WriteElementString("DisplayStockAvailability", null, productVariant.DisplayStockAvailability.ToString());
        //                    xmlWriter.WriteElementString("DisplayStockQuantity", null, productVariant.DisplayStockQuantity.ToString());
        //                    xmlWriter.WriteElementString("MinStockQuantity", null, productVariant.MinStockQuantity.ToString());
        //                    xmlWriter.WriteElementString("LowStockActivityId", null, productVariant.LowStockActivityId.ToString());
        //                    xmlWriter.WriteElementString("NotifyAdminForQuantityBelow", null, productVariant.NotifyAdminForQuantityBelow.ToString());
        //                    xmlWriter.WriteElementString("BackorderModeId", null, productVariant.BackorderModeId.ToString());
        //                    xmlWriter.WriteElementString("AllowBackInStockSubscriptions", null, productVariant.AllowBackInStockSubscriptions.ToString());
        //                    xmlWriter.WriteElementString("OrderMinimumQuantity", null, productVariant.OrderMinimumQuantity.ToString());
        //                    xmlWriter.WriteElementString("OrderMaximumQuantity", null, productVariant.OrderMaximumQuantity.ToString());
        //                    xmlWriter.WriteElementString("DisableBuyButton", null, productVariant.DisableBuyButton.ToString());
        //                    xmlWriter.WriteElementString("DisableWishlistButton", null, productVariant.DisableWishlistButton.ToString());
        //                    xmlWriter.WriteElementString("CallForPrice", null, productVariant.CallForPrice.ToString());
        //                    xmlWriter.WriteElementString("Price", null, productVariant.Price.ToString());
        //                    xmlWriter.WriteElementString("Tax", null, _taxService.GetTaxRate(productVariant, productVariant.TaxCategoryId, null).ToString());

        //                    decimal taxRate = decimal.Zero;
        //                    decimal finalPriceWithDiscountBase = _taxService.GetProductPrice(productVariant, _priceCalculationService.GetFinalPrice(productVariant, true, false), out taxRate);
        //                    decimal finalPriceWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithDiscountBase, _workContext.WorkingCurrency);

        //                    if (finalPriceWithDiscount.ToString() != null)
        //                    {
        //                        xmlWriter.WriteElementString("DiscountPrice", null, finalPriceWithDiscount.ToString());
        //                    }

        //                    xmlWriter.WriteElementString("OldPrice", null, productVariant.OldPrice.ToString());
        //                    xmlWriter.WriteElementString("ProductCost", null, productVariant.ProductCost.ToString());
        //                    xmlWriter.WriteElementString("SpecialPrice", null, productVariant.SpecialPrice.HasValue ? productVariant.SpecialPrice.ToString() : "");
        //                    xmlWriter.WriteElementString("SpecialPriceStartDateTimeUtc", null, productVariant.SpecialPriceStartDateTimeUtc.HasValue ? productVariant.SpecialPriceStartDateTimeUtc.ToString() : "");
        //                    xmlWriter.WriteElementString("SpecialPriceEndDateTimeUtc", null, productVariant.SpecialPriceEndDateTimeUtc.HasValue ? productVariant.SpecialPriceEndDateTimeUtc.ToString() : "");
        //                    xmlWriter.WriteElementString("CustomerEntersPrice", null, productVariant.CustomerEntersPrice.ToString());
        //                    xmlWriter.WriteElementString("MinimumCustomerEnteredPrice", null, productVariant.MinimumCustomerEnteredPrice.ToString());
        //                    xmlWriter.WriteElementString("MaximumCustomerEnteredPrice", null, productVariant.MaximumCustomerEnteredPrice.ToString());
        //                    xmlWriter.WriteElementString("Weight", null, productVariant.Weight.ToString());
        //                    xmlWriter.WriteElementString("Length", null, productVariant.Length.ToString());
        //                    xmlWriter.WriteElementString("Width", null, productVariant.Width.ToString());
        //                    xmlWriter.WriteElementString("Height", null, productVariant.Height.ToString());
        //                    xmlWriter.WriteElementString("PictureId", null, productVariant.PictureId.ToString());
        //                    xmlWriter.WriteElementString("Published", null, productVariant.Published.ToString());
        //                    xmlWriter.WriteElementString("Deleted", null, productVariant.Deleted.ToString());
        //                    xmlWriter.WriteElementString("DisplayOrder", null, productVariant.DisplayOrder.ToString());
        //                    xmlWriter.WriteElementString("CreatedOnUtc", null, productVariant.CreatedOnUtc.ToString());
        //                    xmlWriter.WriteElementString("UpdatedOnUtc", null, productVariant.UpdatedOnUtc.ToString());

        //                    //pictures 
        //                    var pictures = productVariant.GetProductVariantPictures(_pictureService);
        //                    if (pictures != null)
        //                    {
        //                        xmlWriter.WriteStartElement("Pictures");

        //                        foreach (var picture in pictures)
        //                        {
        //                            xmlWriter.WriteElementString("PicturePath", null, _pictureService.GetPictureUrl(picture));
        //                        }
        //                        xmlWriter.WriteEndElement();
        //                    }


        //                    xmlWriter.WriteStartElement("ProductDiscounts");
        //                    var discounts = productVariant.AppliedDiscounts;
        //                    foreach (var discount in discounts)
        //                    {
        //                        xmlWriter.WriteElementString("DiscountId", null, discount.Id.ToString());
        //                    }
        //                    xmlWriter.WriteEndElement();


        //                    xmlWriter.WriteStartElement("TierPrices");
        //                    var tierPrices = productVariant.TierPrices;
        //                    foreach (var tierPrice in tierPrices)
        //                    {
        //                        xmlWriter.WriteElementString("TierPriceId", null, tierPrice.Id.ToString());
        //                        xmlWriter.WriteElementString("CustomerRoleId", null, tierPrice.CustomerRoleId.HasValue ? tierPrice.CustomerRoleId.ToString() : "0");
        //                        xmlWriter.WriteElementString("Quantity", null, tierPrice.Quantity.ToString());
        //                        xmlWriter.WriteElementString("Price", null, tierPrice.Price.ToString());
        //                    }
        //                    xmlWriter.WriteEndElement();

        //                    xmlWriter.WriteStartElement("ProductAttributes");
        //                    var productVariantAttributes = productVariant.ProductVariantAttributes;
        //                    foreach (var productVariantAttribute in productVariantAttributes)
        //                    {
        //                        xmlWriter.WriteStartElement("ProductVariantAttribute");
        //                        xmlWriter.WriteElementString("ProductVariantAttributeId", null, productVariantAttribute.Id.ToString());
        //                        xmlWriter.WriteElementString("ProductAttributeId", null, productVariantAttribute.ProductAttributeId.ToString());
        //                        xmlWriter.WriteElementString("TextPrompt", null, productVariantAttribute.TextPrompt);
        //                        xmlWriter.WriteElementString("IsRequired", null, productVariantAttribute.IsRequired.ToString());
        //                        xmlWriter.WriteElementString("AttributeControlTypeId", null, productVariantAttribute.AttributeControlTypeId.ToString());
        //                        xmlWriter.WriteElementString("DisplayOrder", null, productVariantAttribute.DisplayOrder.ToString());


        //                        xmlWriter.WriteStartElement("ProductVariantAttributeValues");
        //                        var productVariantAttributeValues = productVariantAttribute.ProductVariantAttributeValues;
        //                        foreach (var productVariantAttributeValue in productVariantAttributeValues)
        //                        {
        //                            xmlWriter.WriteElementString("ProductVariantAttributeValueId", null, productVariantAttributeValue.Id.ToString());
        //                            xmlWriter.WriteElementString("Name", null, productVariantAttributeValue.Name);
        //                            xmlWriter.WriteElementString("PriceAdjustment", null, productVariantAttributeValue.PriceAdjustment.ToString());
        //                            xmlWriter.WriteElementString("WeightAdjustment", null, productVariantAttributeValue.WeightAdjustment.ToString());
        //                            xmlWriter.WriteElementString("IsPreSelected", null, productVariantAttributeValue.IsPreSelected.ToString());
        //                            xmlWriter.WriteElementString("DisplayOrder", null, productVariantAttributeValue.DisplayOrder.ToString());
        //                        }
        //                        xmlWriter.WriteEndElement();


        //                        xmlWriter.WriteEndElement();
        //                    }
        //                    xmlWriter.WriteEndElement();

        //                    //product attribute combinations 
        //                    var combinations = productVariant.ProductVariantAttributeCombinations;
        //                    if (combinations != null)
        //                    {
        //                        xmlWriter.WriteStartElement("ProductAttributeCombinations");
        //                        foreach (var comb in combinations)
        //                        {
        //                            xmlWriter.WriteStartElement("Combination");
        //                            //xmlWriter.WriteRaw(comb.AttributesXml);

        //                            var pvaValues = productAttributeParser.ParseProductVariantAttributeValues(comb.AttributesXml);
        //                            foreach (var pvav in pvaValues)
        //                            {
        //                                xmlWriter.WriteStartElement("Attribute");

        //                                //xmlWriter.WriteAttributeString("value", pvav.Name);
        //                                xmlWriter.WriteAttributeString("name", pvav.ProductVariantAttribute.ProductAttribute.Name);
        //                                xmlWriter.WriteAttributeString("Id", pvav.Id.ToString());
        //                                xmlWriter.WriteValue(pvav.Name);
        //                                xmlWriter.WriteEndElement();

        //                            }

        //                            xmlWriter.WriteElementString("Stock", null, comb.StockQuantity.ToString());
        //                            xmlWriter.WriteEndElement();

        //                        }
        //                        xmlWriter.WriteEndElement();
        //                    }

        //                    xmlWriter.WriteEndElement();
        //                }
        //            }
        //            xmlWriter.WriteEndElement();

        //            xmlWriter.WriteStartElement("ProductPictures");
        //            var productPictures = product.ProductPictures;
        //            foreach (var productPicture in productPictures)
        //            {
        //                xmlWriter.WriteStartElement("ProductPicture");
        //                xmlWriter.WriteElementString("ProductPictureId", null, productPicture.Id.ToString());
        //                xmlWriter.WriteElementString("PictureId", null, productPicture.PictureId.ToString());
        //                xmlWriter.WriteElementString("DisplayOrder", null, productPicture.DisplayOrder.ToString());
        //                xmlWriter.WriteEndElement();
        //            }
        //            xmlWriter.WriteEndElement();

        //            xmlWriter.WriteStartElement("ProductCategories");
        //            var productCategories = _categoryService.GetProductCategoriesByProductId(product.Id);
        //            if (productCategories != null)
        //            {
        //                foreach (var productCategory in productCategories)
        //                {
        //                    xmlWriter.WriteStartElement("ProductCategory");
        //                    xmlWriter.WriteAttributeString("Name", productCategory.Category.Name);
        //                    xmlWriter.WriteElementString("ProductCategoryId", null, productCategory.Id.ToString());
        //                    xmlWriter.WriteElementString("CategoryId", null, productCategory.CategoryId.ToString());
        //                    xmlWriter.WriteElementString("IsFeaturedProduct", null, productCategory.IsFeaturedProduct.ToString());
        //                    xmlWriter.WriteElementString("DisplayOrder", null, productCategory.DisplayOrder.ToString());
        //                    xmlWriter.WriteEndElement();
        //                }
        //            }
        //            xmlWriter.WriteEndElement();

        //            xmlWriter.WriteStartElement("ProductManufacturers");
        //            var productManufacturers = _manufacturerService.GetProductManufacturersByProductId(product.Id);
        //            if (productManufacturers != null)
        //            {
        //                foreach (var productManufacturer in productManufacturers)
        //                {
        //                    xmlWriter.WriteStartElement("ProductManufacturer");
        //                    xmlWriter.WriteElementString("ProductManufacturerId", null, productManufacturer.Id.ToString());
        //                    xmlWriter.WriteElementString("ManufacturerId", null, productManufacturer.ManufacturerId.ToString());
        //                    xmlWriter.WriteElementString("IsFeaturedProduct", null, productManufacturer.IsFeaturedProduct.ToString());
        //                    xmlWriter.WriteElementString("DisplayOrder", null, productManufacturer.DisplayOrder.ToString());
        //                    xmlWriter.WriteEndElement();
        //                }
        //            }
        //            xmlWriter.WriteEndElement();

        //            xmlWriter.WriteStartElement("ProductSpecificationAttributes");
        //            var productSpecificationAttributes = product.ProductSpecificationAttributes;
        //            foreach (var productSpecificationAttribute in productSpecificationAttributes)
        //            {
        //                xmlWriter.WriteStartElement("ProductSpecificationAttribute");
        //                //xmlWriter.WriteElementString("ProductSpecificationAttributeId", null, productSpecificationAttribute.Id.ToString());
        //                //xmlWriter.WriteElementString("SpecificationAttributeOptionId", null, productSpecificationAttribute.SpecificationAttributeOptionId.ToString());

        //                xmlWriter.WriteElementString("ProductSpecificationAttributeId", null, productSpecificationAttribute.SpecificationAttributeOption.SpecificationAttribute.Name.ToString());
        //                xmlWriter.WriteElementString("SpecificationAttributeOptionId", null, productSpecificationAttribute.SpecificationAttributeOption.Name.ToString());

        //                xmlWriter.WriteElementString("AllowFiltering", null, productSpecificationAttribute.AllowFiltering.ToString());
        //                xmlWriter.WriteElementString("ShowOnProductPage", null, productSpecificationAttribute.ShowOnProductPage.ToString());
        //                xmlWriter.WriteElementString("DisplayOrder", null, productSpecificationAttribute.DisplayOrder.ToString());
        //                xmlWriter.WriteEndElement();
        //            }
        //            xmlWriter.WriteEndElement();

        //            xmlWriter.WriteEndElement();
        //        }

        //        xmlWriter.WriteEndElement();
        //        xmlWriter.WriteEndDocument();
        //        xmlWriter.Close();
        //        return stringWriter.ToString();
        //    }
        //    catch (Exception ex)
        //    {
        //        return String.Empty;
        //    }

        //}
        
        public virtual string ExportProductsToXmlForGG(IList<Product> products)
        {
            try
            {
                var webHelper = EngineContext.Current.Resolve<IWebHelper>();
                var productAttributeParser = EngineContext.Current.Resolve<IProductAttributeParser>();
                var sb = new StringBuilder();
                var stringWriter = new StringWriter(sb);
                var xmlWriter = new XmlTextWriter(stringWriter);
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("Products");
                xmlWriter.WriteAttributeString("Version", NopVersion.CurrentVersion);
                string brandName = "";
                foreach (var product in products)
                {
                    xmlWriter.WriteStartElement("Product");
                    if (product.ProductSpecificationAttributes.Where(x => x.SpecificationAttributeOption.SpecificationAttributeId == 11).FirstOrDefault() != null)
                    {
                        brandName = product.ProductSpecificationAttributes.Where(x => x.SpecificationAttributeOption.SpecificationAttributeId == 11).FirstOrDefault().SpecificationAttributeOption.Name;
                    }

                    xmlWriter.WriteElementString("ProductId", null, product.Id.ToString());
                    xmlWriter.WriteElementString("Name", null, brandName + " " + product.Name);
                    //xmlWriter.WriteElementString("ShortDescription", null, product.ShortDescription);
                    //xmlWriter.WriteElementString("FullDescription", null, product.FullDescription);
                    //xmlWriter.WriteElementString("AdminComment", null, product.AdminComment);
                    //xmlWriter.WriteElementString("ProductTemplateId", null, product.ProductTemplateId.ToString());
                    //xmlWriter.WriteElementString("ShowOnHomePage", null, product.ShowOnHomePage.ToString());
                    //xmlWriter.WriteElementString("MetaKeywords", null, product.MetaKeywords);
                    xmlWriter.WriteElementString("MetaDescription", null, product.MetaDescription);
                    xmlWriter.WriteElementString("MetaTitle", null, product.MetaTitle);
                    //xmlWriter.WriteElementString("SEName", null, product.SeName);
                    //xmlWriter.WriteElementString("AllowCustomerReviews", null, product.AllowCustomerReviews.ToString());
                    //xmlWriter.WriteElementString("Published", null, product.Published.ToString());
                    //xmlWriter.WriteElementString("CreatedOnUtc", null, product.CreatedOnUtc.ToString());
                    //xmlWriter.WriteElementString("UpdatedOnUtc", null, product.UpdatedOnUtc.ToString());

                    xmlWriter.WriteStartElement("ProductVariants");
                    var productVariants = _productService.GetProductVariantsByProductId(product.Id, true);
                    if (productVariants != null)
                    {
                        foreach (var productVariant in productVariants)
                        {
                            xmlWriter.WriteStartElement("ProductVariant");
                            xmlWriter.WriteElementString("ProductVariantId", null, productVariant.Id.ToString());
                            xmlWriter.WriteElementString("ProductId", null, productVariant.ProductId.ToString());
                            xmlWriter.WriteElementString("Name", null, productVariant.Name);
                            xmlWriter.WriteElementString("SKU", null, productVariant.Sku);
                            //xmlWriter.WriteElementString("Description", null, productVariant.Description);
                            //xmlWriter.WriteElementString("AdminComment", null, productVariant.AdminComment);
                            xmlWriter.WriteElementString("ManufacturerPartNumber", null, productVariant.ManufacturerPartNumber);
                            xmlWriter.WriteElementString("Gtin", null, productVariant.Gtin);
                            //xmlWriter.WriteElementString("IsGiftCard", null, productVariant.IsGiftCard.ToString());
                            //xmlWriter.WriteElementString("GiftCardType", null, productVariant.GiftCardType.ToString());
                            //xmlWriter.WriteElementString("RequireOtherProducts", null, productVariant.RequireOtherProducts.ToString());
                            //xmlWriter.WriteElementString("RequiredProductVariantIds", null, productVariant.RequiredProductVariantIds);
                            //xmlWriter.WriteElementString("AutomaticallyAddRequiredProductVariants", null, productVariant.AutomaticallyAddRequiredProductVariants.ToString());
                            //xmlWriter.WriteElementString("IsDownload", null, productVariant.IsDownload.ToString());
                            //xmlWriter.WriteElementString("DownloadId", null, productVariant.DownloadId.ToString());
                            //xmlWriter.WriteElementString("UnlimitedDownloads", null, productVariant.UnlimitedDownloads.ToString());
                            //xmlWriter.WriteElementString("MaxNumberOfDownloads", null, productVariant.MaxNumberOfDownloads.ToString());
                            //if (productVariant.DownloadExpirationDays.HasValue)
                            //    xmlWriter.WriteElementString("DownloadExpirationDays", null, productVariant.DownloadExpirationDays.ToString());
                            //else
                            //    xmlWriter.WriteElementString("DownloadExpirationDays", null, string.Empty);
                            //xmlWriter.WriteElementString("DownloadActivationType", null, productVariant.DownloadActivationType.ToString());
                            //xmlWriter.WriteElementString("HasSampleDownload", null, productVariant.HasSampleDownload.ToString());
                            //xmlWriter.WriteElementString("SampleDownloadId", null, productVariant.SampleDownloadId.ToString());
                            //xmlWriter.WriteElementString("HasUserAgreement", null, productVariant.HasUserAgreement.ToString());
                            //xmlWriter.WriteElementString("UserAgreementText", null, productVariant.UserAgreementText);
                            //xmlWriter.WriteElementString("IsRecurring", null, productVariant.IsRecurring.ToString());
                            //xmlWriter.WriteElementString("RecurringCycleLength", null, productVariant.RecurringCycleLength.ToString());
                            //xmlWriter.WriteElementString("RecurringCyclePeriodId", null, productVariant.RecurringCyclePeriodId.ToString());
                            //xmlWriter.WriteElementString("RecurringTotalCycles", null, productVariant.RecurringTotalCycles.ToString());
                            //xmlWriter.WriteElementString("IsShipEnabled", null, productVariant.IsShipEnabled.ToString());
                            //xmlWriter.WriteElementString("IsFreeShipping", null, productVariant.IsFreeShipping.ToString());
                            //xmlWriter.WriteElementString("AdditionalShippingCharge", null, productVariant.AdditionalShippingCharge.ToString());
                            //xmlWriter.WriteElementString("IsTaxExempt", null, productVariant.IsTaxExempt.ToString());
                            //xmlWriter.WriteElementString("TaxCategoryId", null, productVariant.TaxCategoryId.ToString());
                            //xmlWriter.WriteElementString("ManageInventoryMethodId", null, productVariant.ManageInventoryMethodId.ToString());
                            xmlWriter.WriteElementString("StockQuantity", null, productVariant.StockQuantity.ToString());
                            //xmlWriter.WriteElementString("DisplayStockAvailability", null, productVariant.DisplayStockAvailability.ToString());
                            //xmlWriter.WriteElementString("DisplayStockQuantity", null, productVariant.DisplayStockQuantity.ToString());
                            //xmlWriter.WriteElementString("MinStockQuantity", null, productVariant.MinStockQuantity.ToString());
                            //xmlWriter.WriteElementString("LowStockActivityId", null, productVariant.LowStockActivityId.ToString());
                            //xmlWriter.WriteElementString("NotifyAdminForQuantityBelow", null, productVariant.NotifyAdminForQuantityBelow.ToString());
                            //xmlWriter.WriteElementString("BackorderModeId", null, productVariant.BackorderModeId.ToString());
                            //xmlWriter.WriteElementString("AllowBackInStockSubscriptions", null, productVariant.AllowBackInStockSubscriptions.ToString());
                            //xmlWriter.WriteElementString("OrderMinimumQuantity", null, productVariant.OrderMinimumQuantity.ToString());
                            //xmlWriter.WriteElementString("OrderMaximumQuantity", null, productVariant.OrderMaximumQuantity.ToString());
                            //xmlWriter.WriteElementString("DisableBuyButton", null, productVariant.DisableBuyButton.ToString());
                            //xmlWriter.WriteElementString("DisableWishlistButton", null, productVariant.DisableWishlistButton.ToString());
                            //xmlWriter.WriteElementString("CallForPrice", null, productVariant.CallForPrice.ToString());
                            xmlWriter.WriteElementString("Price", null, productVariant.Price.ToString());
                            xmlWriter.WriteElementString("Tax", null, _taxService.GetTaxRate(productVariant, productVariant.TaxCategoryId, null).ToString());

                            decimal taxRate = decimal.Zero;
                            decimal finalPriceWithDiscountBase = _taxService.GetProductPrice(productVariant, _priceCalculationService.GetFinalPrice(productVariant, true, false), out taxRate);
                            decimal finalPriceWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithDiscountBase, _workContext.WorkingCurrency);

                            if (finalPriceWithDiscount.ToString() != null)
                            {
                                xmlWriter.WriteElementString("DiscountPrice", null, finalPriceWithDiscount.ToString());
                            }

                            xmlWriter.WriteElementString("OldPrice", null, productVariant.OldPrice.ToString());
                            //xmlWriter.WriteElementString("ProductCost", null, productVariant.ProductCost.ToString());
                            xmlWriter.WriteElementString("SpecialPrice", null, productVariant.SpecialPrice.HasValue ? productVariant.SpecialPrice.ToString() : "");
                            xmlWriter.WriteElementString("SpecialPriceStartDateTimeUtc", null, productVariant.SpecialPriceStartDateTimeUtc.HasValue ? productVariant.SpecialPriceStartDateTimeUtc.ToString() : "");
                            xmlWriter.WriteElementString("SpecialPriceEndDateTimeUtc", null, productVariant.SpecialPriceEndDateTimeUtc.HasValue ? productVariant.SpecialPriceEndDateTimeUtc.ToString() : "");
                            //xmlWriter.WriteElementString("CustomerEntersPrice", null, productVariant.CustomerEntersPrice.ToString());
                            //xmlWriter.WriteElementString("MinimumCustomerEnteredPrice", null, productVariant.MinimumCustomerEnteredPrice.ToString());
                            //xmlWriter.WriteElementString("MaximumCustomerEnteredPrice", null, productVariant.MaximumCustomerEnteredPrice.ToString());
                            xmlWriter.WriteElementString("Weight", null, productVariant.Weight.ToString());
                            xmlWriter.WriteElementString("Length", null, productVariant.Length.ToString());
                            xmlWriter.WriteElementString("Width", null, productVariant.Width.ToString());
                            xmlWriter.WriteElementString("Height", null, productVariant.Height.ToString());
                            //xmlWriter.WriteElementString("PictureId", null, productVariant.PictureId.ToString());
                            //xmlWriter.WriteElementString("Published", null, productVariant.Published.ToString());
                            //xmlWriter.WriteElementString("Deleted", null, productVariant.Deleted.ToString());
                            //xmlWriter.WriteElementString("DisplayOrder", null, productVariant.DisplayOrder.ToString());
                            //xmlWriter.WriteElementString("CreatedOnUtc", null, productVariant.CreatedOnUtc.ToString());
                            //xmlWriter.WriteElementString("UpdatedOnUtc", null, productVariant.UpdatedOnUtc.ToString());

                            //pictures 
                            var pictures = productVariant.GetProductVariantPictures(_pictureService);
                            if (pictures != null)
                            {
                                xmlWriter.WriteStartElement("Pictures");

                                foreach (var picture in pictures)
                                {
                                    xmlWriter.WriteElementString("PicturePath", null, _pictureService.GetPictureUrl(picture));
                                }
                                xmlWriter.WriteEndElement();
                            }


                            //xmlWriter.WriteStartElement("ProductDiscounts");
                            //var discounts = productVariant.AppliedDiscounts;
                            //foreach (var discount in discounts)
                            //{
                            //    xmlWriter.WriteElementString("DiscountId", null, discount.Id.ToString());
                            //}
                            //xmlWriter.WriteEndElement();


                            //xmlWriter.WriteStartElement("TierPrices");
                            //var tierPrices = productVariant.TierPrices;
                            //foreach (var tierPrice in tierPrices)
                            //{
                            //    xmlWriter.WriteElementString("TierPriceId", null, tierPrice.Id.ToString());
                            //    xmlWriter.WriteElementString("CustomerRoleId", null, tierPrice.CustomerRoleId.HasValue ? tierPrice.CustomerRoleId.ToString() : "0");
                            //    xmlWriter.WriteElementString("Quantity", null, tierPrice.Quantity.ToString());
                            //    xmlWriter.WriteElementString("Price", null, tierPrice.Price.ToString());
                            //}
                            //xmlWriter.WriteEndElement();

                            //xmlWriter.WriteStartElement("ProductAttributes");
                            //var productVariantAttributes = productVariant.ProductVariantAttributes;
                            //foreach (var productVariantAttribute in productVariantAttributes)
                            //{
                            //    xmlWriter.WriteStartElement("ProductVariantAttribute");
                            //    xmlWriter.WriteElementString("ProductVariantAttributeId", null, productVariantAttribute.Id.ToString());
                            //    xmlWriter.WriteElementString("ProductAttributeId", null, productVariantAttribute.ProductAttributeId.ToString());
                            //    xmlWriter.WriteElementString("TextPrompt", null, productVariantAttribute.TextPrompt);
                            //    xmlWriter.WriteElementString("IsRequired", null, productVariantAttribute.IsRequired.ToString());
                            //    xmlWriter.WriteElementString("AttributeControlTypeId", null, productVariantAttribute.AttributeControlTypeId.ToString());
                            //    xmlWriter.WriteElementString("DisplayOrder", null, productVariantAttribute.DisplayOrder.ToString());


                            //    xmlWriter.WriteStartElement("ProductVariantAttributeValues");
                            //    var productVariantAttributeValues = productVariantAttribute.ProductVariantAttributeValues;
                            //    foreach (var productVariantAttributeValue in productVariantAttributeValues)
                            //    {
                            //        xmlWriter.WriteElementString("ProductVariantAttributeValueId", null, productVariantAttributeValue.Id.ToString());
                            //        xmlWriter.WriteElementString("Name", null, productVariantAttributeValue.Name);
                            //        xmlWriter.WriteElementString("PriceAdjustment", null, productVariantAttributeValue.PriceAdjustment.ToString());
                            //        xmlWriter.WriteElementString("WeightAdjustment", null, productVariantAttributeValue.WeightAdjustment.ToString());
                            //        xmlWriter.WriteElementString("IsPreSelected", null, productVariantAttributeValue.IsPreSelected.ToString());
                            //        xmlWriter.WriteElementString("DisplayOrder", null, productVariantAttributeValue.DisplayOrder.ToString());
                            //    }
                            //    xmlWriter.WriteEndElement();


                            //    xmlWriter.WriteEndElement();
                            //}
                            //xmlWriter.WriteEndElement();

                            //product attribute combinations 
                            var combinations = productVariant.ProductVariantAttributeCombinations;
                            if (combinations != null)
                            {
                                xmlWriter.WriteStartElement("ProductAttributeCombinations");
                                foreach (var comb in combinations)
                                {
                                    xmlWriter.WriteStartElement("Combination");
                                    //xmlWriter.WriteRaw(comb.AttributesXml);

                                    var pvaValues = productAttributeParser.ParseProductVariantAttributeValues(comb.AttributesXml);
                                    foreach (var pvav in pvaValues)
                                    {
                                        xmlWriter.WriteStartElement("Attribute");

                                        //xmlWriter.WriteAttributeString("value", pvav.Name);
                                        xmlWriter.WriteAttributeString("name", pvav.ProductVariantAttribute.ProductAttribute.Name);
                                        //xmlWriter.WriteAttributeString("Id", pvav.Id.ToString());
                                        xmlWriter.WriteValue(pvav.Name);
                                        xmlWriter.WriteEndElement();

                                    }

                                    xmlWriter.WriteElementString("Stock", null, comb.StockQuantity.ToString());
                                    xmlWriter.WriteEndElement();

                                }
                                xmlWriter.WriteEndElement();
                            }

                            xmlWriter.WriteEndElement();
                        }
                    }
                    xmlWriter.WriteEndElement();

                    //xmlWriter.WriteStartElement("ProductPictures");
                    //var productPictures = product.ProductPictures;
                    //foreach (var productPicture in productPictures)
                    //{
                    //    xmlWriter.WriteStartElement("ProductPicture");
                    //    xmlWriter.WriteElementString("ProductPictureId", null, productPicture.Id.ToString());
                    //    xmlWriter.WriteElementString("PictureId", null, productPicture.PictureId.ToString());
                    //    xmlWriter.WriteElementString("DisplayOrder", null, productPicture.DisplayOrder.ToString());
                    //    xmlWriter.WriteEndElement();
                    //}
                    //xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("ProductCategories");
                    var productCategories = _categoryService.GetProductCategoriesByProductId(product.Id);
                    if (productCategories != null)
                    {
                        foreach (var productCategory in productCategories)
                        {
                            xmlWriter.WriteStartElement("ProductCategory");
                            xmlWriter.WriteAttributeString("Name", productCategory.Category.Name);
                            //xmlWriter.WriteElementString("ProductCategoryId", null, productCategory.Id.ToString());
                            //xmlWriter.WriteElementString("CategoryId", null, productCategory.CategoryId.ToString());
                            //xmlWriter.WriteElementString("IsFeaturedProduct", null, productCategory.IsFeaturedProduct.ToString());
                            //xmlWriter.WriteElementString("DisplayOrder", null, productCategory.DisplayOrder.ToString());
                            xmlWriter.WriteEndElement();
                        }
                    }
                    xmlWriter.WriteEndElement();

                    //xmlWriter.WriteStartElement("ProductManufacturers");
                    //var productManufacturers = _manufacturerService.GetProductManufacturersByProductId(product.Id);
                    //if (productManufacturers != null)
                    //{
                    //    foreach (var productManufacturer in productManufacturers)
                    //    {
                    //        xmlWriter.WriteStartElement("ProductManufacturer");
                    //        xmlWriter.WriteElementString("ProductManufacturerId", null, productManufacturer.Id.ToString());
                    //        xmlWriter.WriteElementString("ManufacturerId", null, productManufacturer.ManufacturerId.ToString());
                    //        xmlWriter.WriteElementString("IsFeaturedProduct", null, productManufacturer.IsFeaturedProduct.ToString());
                    //        xmlWriter.WriteElementString("DisplayOrder", null, productManufacturer.DisplayOrder.ToString());
                    //        xmlWriter.WriteEndElement();
                    //    }
                    //}
                    //xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("ProductSpecificationAttributes");
                    var productSpecificationAttributes = product.ProductSpecificationAttributes;
                    foreach (var productSpecificationAttribute in productSpecificationAttributes)
                    {
                        xmlWriter.WriteStartElement("ProductSpecificationAttribute");
                        //xmlWriter.WriteElementString("ProductSpecificationAttributeId", null, productSpecificationAttribute.Id.ToString());
                        //xmlWriter.WriteElementString("SpecificationAttributeOptionId", null, productSpecificationAttribute.SpecificationAttributeOptionId.ToString());

                        xmlWriter.WriteElementString("ProductSpecificationAttributeId", null, productSpecificationAttribute.SpecificationAttributeOption.SpecificationAttribute.Name.ToString());
                        xmlWriter.WriteElementString("SpecificationAttributeOptionId", null, productSpecificationAttribute.SpecificationAttributeOption.Name.ToString());

                        xmlWriter.WriteElementString("AllowFiltering", null, productSpecificationAttribute.AllowFiltering.ToString());
                        xmlWriter.WriteElementString("ShowOnProductPage", null, productSpecificationAttribute.ShowOnProductPage.ToString());
                        xmlWriter.WriteElementString("DisplayOrder", null, productSpecificationAttribute.DisplayOrder.ToString());
                        xmlWriter.WriteEndElement();
                    }
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteEndElement();
                }

                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndDocument();
                xmlWriter.Close();
                return stringWriter.ToString();
            }
            catch (Exception ex)
            {
                return String.Empty;
            }

        }


        /// <summary>
        /// Export order list to xml
        /// </summary>
        /// <param name="orders">Orders</param>
        /// <returns>Result in XML format</returns>
        public virtual string ExportOrdersToXml(IList<Order> orders)
        {
            var sb = new StringBuilder();
            var stringWriter = new StringWriter(sb);
            var xmlWriter = new XmlTextWriter(stringWriter);
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("Orders");
            xmlWriter.WriteAttributeString("Version", NopVersion.CurrentVersion);


            foreach (var order in orders)
            {
                xmlWriter.WriteStartElement("Order");

                xmlWriter.WriteElementString("OrderId", null, order.Id.ToString());
                xmlWriter.WriteElementString("OrderGuid", null, order.OrderGuid.ToString());
                xmlWriter.WriteElementString("CustomerId", null, order.CustomerId.ToString());
                xmlWriter.WriteElementString("CustomerLanguageId", null, order.CustomerLanguageId.ToString());
                xmlWriter.WriteElementString("CustomerTaxDisplayTypeId", null, order.CustomerTaxDisplayTypeId.ToString());
                xmlWriter.WriteElementString("CustomerIp", null, order.CustomerIp);
                xmlWriter.WriteElementString("OrderSubtotalInclTax", null, order.OrderSubtotalInclTax.ToString());
                xmlWriter.WriteElementString("OrderSubtotalExclTax", null, order.OrderSubtotalExclTax.ToString());
                xmlWriter.WriteElementString("OrderSubTotalDiscountInclTax", null, order.OrderSubTotalDiscountInclTax.ToString());
                xmlWriter.WriteElementString("OrderSubTotalDiscountExclTax", null, order.OrderSubTotalDiscountExclTax.ToString());
                xmlWriter.WriteElementString("OrderShippingInclTax", null, order.OrderShippingInclTax.ToString());
                xmlWriter.WriteElementString("OrderShippingExclTax", null, order.OrderShippingExclTax.ToString());
                xmlWriter.WriteElementString("PaymentMethodAdditionalFeeInclTax", null, order.PaymentMethodAdditionalFeeInclTax.ToString());
                xmlWriter.WriteElementString("PaymentMethodAdditionalFeeExclTax", null, order.PaymentMethodAdditionalFeeExclTax.ToString());
                xmlWriter.WriteElementString("TaxRates", null, order.TaxRates);
                xmlWriter.WriteElementString("OrderTax", null, order.OrderTax.ToString());
                xmlWriter.WriteElementString("OrderTotal", null, order.OrderTotal.ToString());
                xmlWriter.WriteElementString("RefundedAmount", null, order.RefundedAmount.ToString());
                xmlWriter.WriteElementString("OrderDiscount", null, order.OrderDiscount.ToString());
                xmlWriter.WriteElementString("CurrencyRate", null, order.CurrencyRate.ToString());
                xmlWriter.WriteElementString("CustomerCurrencyCode", null, order.CustomerCurrencyCode);
                xmlWriter.WriteElementString("OrderWeight", null, order.OrderWeight.ToString());
                xmlWriter.WriteElementString("AffiliateId", null, order.AffiliateId.ToString());
                xmlWriter.WriteElementString("OrderStatusId", null, order.OrderStatusId.ToString());
                xmlWriter.WriteElementString("AllowStoringCreditCardNumber", null, order.AllowStoringCreditCardNumber.ToString());
                xmlWriter.WriteElementString("CardType", null, order.CardType);
                xmlWriter.WriteElementString("CardName", null, order.CardName);
                xmlWriter.WriteElementString("CardNumber", null, order.CardNumber);
                xmlWriter.WriteElementString("MaskedCreditCardNumber", null, order.MaskedCreditCardNumber);
                xmlWriter.WriteElementString("CardCvv2", null, order.CardCvv2);
                xmlWriter.WriteElementString("CardExpirationMonth", null, order.CardExpirationMonth);
                xmlWriter.WriteElementString("CardExpirationYear", null, order.CardExpirationYear);
                xmlWriter.WriteElementString("PaymentMethodSystemName", null, order.PaymentMethodSystemName);
                xmlWriter.WriteElementString("AuthorizationTransactionId", null, order.AuthorizationTransactionId);
                xmlWriter.WriteElementString("AuthorizationTransactionCode", null, order.AuthorizationTransactionCode);
                xmlWriter.WriteElementString("AuthorizationTransactionResult", null, order.AuthorizationTransactionResult);
                xmlWriter.WriteElementString("CaptureTransactionId", null, order.CaptureTransactionId);
                xmlWriter.WriteElementString("CaptureTransactionResult", null, order.CaptureTransactionResult);
                xmlWriter.WriteElementString("SubscriptionTransactionId", null, order.SubscriptionTransactionId);
                xmlWriter.WriteElementString("PurchaseOrderNumber", null, order.PurchaseOrderNumber);
                xmlWriter.WriteElementString("PaymentStatusId", null, order.PaymentStatusId.ToString());
                xmlWriter.WriteElementString("PaidDateUtc", null, (order.PaidDateUtc == null) ? string.Empty : order.PaidDateUtc.Value.ToString());
                xmlWriter.WriteElementString("ShippingStatusId", null, order.ShippingStatusId.ToString());
                xmlWriter.WriteElementString("ShippingMethod", null, order.ShippingMethod);
                xmlWriter.WriteElementString("ShippingRateComputationMethodSystemName", null, order.ShippingRateComputationMethodSystemName);
                xmlWriter.WriteElementString("ShippedDateUtc", null, (order.ShippedDateUtc == null) ? string.Empty : order.ShippedDateUtc.Value.ToString());
                xmlWriter.WriteElementString("TrackingNumber", null, order.TrackingNumber);
                xmlWriter.WriteElementString("VatNumber", null, order.VatNumber);
                xmlWriter.WriteElementString("Deleted", null, order.Deleted.ToString());
                xmlWriter.WriteElementString("CreatedOnUtc", null, order.CreatedOnUtc.ToString());

                var orderProductVariants = order.OrderProductVariants;
                if (orderProductVariants.Count > 0)
                {
                    xmlWriter.WriteStartElement("OrderProductVariants");
                    foreach (var orderProductVariant in orderProductVariants)
                    {
                        xmlWriter.WriteStartElement("OrderProductVariant");
                        xmlWriter.WriteElementString("OrderProductVariantId", null, orderProductVariant.Id.ToString());
                        xmlWriter.WriteElementString("ProductVariantId", null, orderProductVariant.ProductVariantId.ToString());

                        var productVariant = orderProductVariant.ProductVariant;
                        if (productVariant != null)
                            xmlWriter.WriteElementString("ProductVariantName", null, productVariant.FullProductName);


                        xmlWriter.WriteElementString("UnitPriceInclTax", null, orderProductVariant.UnitPriceInclTax.ToString());
                        xmlWriter.WriteElementString("UnitPriceExclTax", null, orderProductVariant.UnitPriceExclTax.ToString());
                        xmlWriter.WriteElementString("PriceInclTax", null, orderProductVariant.PriceInclTax.ToString());
                        xmlWriter.WriteElementString("PriceExclTax", null, orderProductVariant.PriceExclTax.ToString());
                        xmlWriter.WriteElementString("AttributeDescription", null, orderProductVariant.AttributeDescription);
                        xmlWriter.WriteElementString("AttributesXml", null, orderProductVariant.AttributesXml);
                        xmlWriter.WriteElementString("Quantity", null, orderProductVariant.Quantity.ToString());
                        xmlWriter.WriteElementString("DiscountAmountInclTax", null, orderProductVariant.DiscountAmountInclTax.ToString());
                        xmlWriter.WriteElementString("DiscountAmountExclTax", null, orderProductVariant.DiscountAmountExclTax.ToString());
                        xmlWriter.WriteElementString("DownloadCount", null, orderProductVariant.DownloadCount.ToString());
                        xmlWriter.WriteElementString("IsDownloadActivated", null, orderProductVariant.IsDownloadActivated.ToString());
                        xmlWriter.WriteElementString("LicenseDownloadId", null, orderProductVariant.LicenseDownloadId.ToString());
                        xmlWriter.WriteEndElement();
                    }
                    xmlWriter.WriteEndElement();
                }

                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
            return stringWriter.ToString();
        }

        /// <summary>
        /// Export orders to XLSX
        /// </summary>
        /// <param name="filePath">File path to use</param>
        /// <param name="orders">Orders</param>
        public virtual void ExportOrdersToXlsx(string filePath, IList<Order> orders)
        {
            var newFile = new FileInfo(filePath);
            // ok, we can run the real code of the sample now
            using (var xlPackage = new ExcelPackage(newFile))
            {
                // uncomment this line if you want the XML written out to the outputDir
                //xlPackage.DebugMode = true; 

                // get handle to the existing worksheet
                var worksheet = xlPackage.Workbook.Worksheets.Add("Orders");
                //Create Headers and format them
                var properties = new string[]
                    {
                        //order properties
                        "OrderId",
                        "OrderGuid",
                        "CustomerId",
                        "OrderSubtotalInclTax",
                        "OrderSubtotalExclTax",
                        "OrderSubTotalDiscountInclTax",
                        "OrderSubTotalDiscountExclTax",
                        "OrderShippingInclTax",
                        "OrderShippingExclTax",
                        "PaymentMethodAdditionalFeeInclTax",
                        "PaymentMethodAdditionalFeeExclTax",
                        "TaxRates",
                        "OrderTax",
                        "OrderTotal",
                        "RefundedAmount",
                        "OrderDiscount",
                        "CurrencyRate",
                        "CustomerCurrencyCode",
                        "OrderWeight",
                        "AffiliateId",
                        "OrderStatusId",
                        "PaymentMethodSystemName",
                        "PurchaseOrderNumber",
                        "PaymentStatusId",
                        "ShippingStatusId",
                        "ShippingMethod",
                        "ShippingRateComputationMethodSystemName",
                        "VatNumber",
                        "CreatedOnUtc",
                        //billing address
                        "BillingFirstName",
                        "BillingLastName",
                        "BillingEmail",
                        "BillingCompany",
                        "BillingCountry",
                        "BillingStateProvince",
                        "BillingCity",
                        "BillingAddress1",
                        "BillingAddress2",
                        "BillingZipPostalCode",
                        "BillingPhoneNumber",
                        "BillingFaxNumber",
                        //shipping address
                        "ShippingFirstName",
                        "ShippingLastName",
                        "ShippingEmail",
                        "ShippingCompany",
                        "ShippingCountry",
                        "ShippingStateProvince",
                        "ShippingCity",
                        "ShippingAddress1",
                        "ShippingAddress2",
                        "ShippingZipPostalCode",
                        "ShippingPhoneNumber",
                        "ShippingFaxNumber",
                    };
                for (int i = 0; i < properties.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = properties[i];
                    worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(184, 204, 228));
                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                }


                int row = 2;
                foreach (var order in orders)
                {
                    int col = 1;

                    //order properties
                    worksheet.Cells[row, col].Value = order.Id;
                    col++;

                    worksheet.Cells[row, col].Value = order.OrderGuid;
                    col++;

                    worksheet.Cells[row, col].Value = order.CustomerId;
                    col++;

                    worksheet.Cells[row, col].Value = order.OrderSubtotalInclTax;
                    col++;

                    worksheet.Cells[row, col].Value = order.OrderSubtotalExclTax;
                    col++;

                    worksheet.Cells[row, col].Value = order.OrderSubTotalDiscountInclTax;
                    col++;

                    worksheet.Cells[row, col].Value = order.OrderSubTotalDiscountExclTax;
                    col++;

                    worksheet.Cells[row, col].Value = order.OrderShippingInclTax;
                    col++;

                    worksheet.Cells[row, col].Value = order.OrderShippingExclTax;
                    col++;

                    worksheet.Cells[row, col].Value = order.PaymentMethodAdditionalFeeInclTax;
                    col++;

                    worksheet.Cells[row, col].Value = order.PaymentMethodAdditionalFeeExclTax;
                    col++;

                    worksheet.Cells[row, col].Value = order.TaxRates;
                    col++;

                    worksheet.Cells[row, col].Value = order.OrderTax;
                    col++;

                    worksheet.Cells[row, col].Value = order.OrderTotal;
                    col++;

                    worksheet.Cells[row, col].Value = order.RefundedAmount;
                    col++;

                    worksheet.Cells[row, col].Value = order.OrderDiscount;
                    col++;

                    worksheet.Cells[row, col].Value = order.CurrencyRate;
                    col++;

                    worksheet.Cells[row, col].Value = order.CustomerCurrencyCode;
                    col++;

                    worksheet.Cells[row, col].Value = order.OrderWeight;
                    col++;

                    worksheet.Cells[row, col].Value = order.AffiliateId.HasValue ? order.AffiliateId.Value : 0;
                    col++;

                    worksheet.Cells[row, col].Value = order.OrderStatusId;
                    col++;

                    worksheet.Cells[row, col].Value = order.PaymentMethodSystemName;
                    col++;

                    worksheet.Cells[row, col].Value = order.PurchaseOrderNumber;
                    col++;

                    worksheet.Cells[row, col].Value = order.PaymentStatusId;
                    col++;

                    worksheet.Cells[row, col].Value = order.ShippingStatusId;
                    col++;

                    worksheet.Cells[row, col].Value = order.ShippingMethod;
                    col++;

                    worksheet.Cells[row, col].Value = order.ShippingRateComputationMethodSystemName;
                    col++;

                    worksheet.Cells[row, col].Value = order.CreatedOnUtc.ToOADate();
                    col++;


                    //billing address
                    worksheet.Cells[row, col].Value = order.BillingAddress != null ? order.BillingAddress.FirstName : "";
                    col++;

                    worksheet.Cells[row, col].Value = order.BillingAddress != null ? order.BillingAddress.LastName : "";
                    col++;

                    worksheet.Cells[row, col].Value = order.BillingAddress != null ? order.BillingAddress.Email : "";
                    col++;

                    worksheet.Cells[row, col].Value = order.BillingAddress != null ? order.BillingAddress.Company : "";
                    col++;

                    worksheet.Cells[row, col].Value = order.BillingAddress != null && order.BillingAddress.Country != null ? order.BillingAddress.Country.Name : "";
                    col++;

                    worksheet.Cells[row, col].Value = order.BillingAddress != null && order.BillingAddress.StateProvince != null ? order.BillingAddress.StateProvince.Name : "";
                    col++;

                    worksheet.Cells[row, col].Value = order.BillingAddress != null ? order.BillingAddress.City : "";
                    col++;

                    worksheet.Cells[row, col].Value = order.BillingAddress != null ? order.BillingAddress.Address1 : "";
                    col++;

                    worksheet.Cells[row, col].Value = order.BillingAddress != null ? order.BillingAddress.Address2 : "";
                    col++;

                    worksheet.Cells[row, col].Value = order.BillingAddress != null ? order.BillingAddress.ZipPostalCode : "";
                    col++;

                    worksheet.Cells[row, col].Value = order.BillingAddress != null ? order.BillingAddress.PhoneNumber : "";
                    col++;

                    worksheet.Cells[row, col].Value = order.BillingAddress != null ? order.BillingAddress.FaxNumber : "";
                    col++;

                    //shipping address
                    worksheet.Cells[row, col].Value = order.ShippingAddress != null ? order.ShippingAddress.FirstName : "";
                    col++;

                    worksheet.Cells[row, col].Value = order.ShippingAddress != null ? order.ShippingAddress.LastName : "";
                    col++;

                    worksheet.Cells[row, col].Value = order.ShippingAddress != null ? order.ShippingAddress.Email : "";
                    col++;

                    worksheet.Cells[row, col].Value = order.ShippingAddress != null ? order.ShippingAddress.Company : "";
                    col++;

                    worksheet.Cells[row, col].Value = order.ShippingAddress != null && order.ShippingAddress.Country != null ? order.ShippingAddress.Country.Name : "";
                    col++;

                    worksheet.Cells[row, col].Value = order.ShippingAddress != null && order.ShippingAddress.StateProvince != null ? order.ShippingAddress.StateProvince.Name : "";
                    col++;

                    worksheet.Cells[row, col].Value = order.ShippingAddress != null ? order.ShippingAddress.City : "";
                    col++;

                    worksheet.Cells[row, col].Value = order.ShippingAddress != null ? order.ShippingAddress.Address1 : "";
                    col++;

                    worksheet.Cells[row, col].Value = order.ShippingAddress != null ? order.ShippingAddress.Address2 : "";
                    col++;

                    worksheet.Cells[row, col].Value = order.ShippingAddress != null ? order.ShippingAddress.ZipPostalCode : "";
                    col++;

                    worksheet.Cells[row, col].Value = order.ShippingAddress != null ? order.ShippingAddress.PhoneNumber : "";
                    col++;

                    worksheet.Cells[row, col].Value = order.ShippingAddress != null ? order.ShippingAddress.FaxNumber : "";
                    col++;

                    //next row
                    row++;
                }








                // we had better add some document properties to the spreadsheet 

                // set some core property values
                xlPackage.Workbook.Properties.Title = string.Format("{0} orders", _storeInformationSettings.StoreName);
                xlPackage.Workbook.Properties.Author = _storeInformationSettings.StoreName;
                xlPackage.Workbook.Properties.Subject = string.Format("{0} orders", _storeInformationSettings.StoreName);
                xlPackage.Workbook.Properties.Keywords = string.Format("{0} orders", _storeInformationSettings.StoreName);
                xlPackage.Workbook.Properties.Category = "Orders";
                xlPackage.Workbook.Properties.Comments = string.Format("{0} orders", _storeInformationSettings.StoreName);

                // set some extended property values
                xlPackage.Workbook.Properties.Company = _storeInformationSettings.StoreName;
                xlPackage.Workbook.Properties.HyperlinkBase = new Uri(_storeInformationSettings.StoreUrl);

                // save the new spreadsheet
                xlPackage.Save();
            }
        }

        /// <summary>
        /// Export customer list to XLSX
        /// </summary>
        /// <param name="filePath">File path to use</param>
        /// <param name="customers">Customers</param>
        public virtual void ExportCustomersToXlsx(string filePath, IList<Customer> customers)
        {
            var newFile = new FileInfo(filePath);
            // ok, we can run the real code of the sample now
            using (var xlPackage = new ExcelPackage(newFile))
            {
                // uncomment this line if you want the XML written out to the outputDir
                //xlPackage.DebugMode = true; 

                // get handle to the existing worksheet
                var worksheet = xlPackage.Workbook.Worksheets.Add("Customers");
                //Create Headers and format them
                var properties = new string[]
                    {
                        "CustomerId",
                        "CustomerGuid",
                        "Email",
                        "Username",
                        "PasswordStr",//why can't we use 'Password' name?
                        "PasswordFormatId",
                        "PasswordSalt",
                        "LanguageId",
                        "CurrencyId",
                        "TaxDisplayTypeId",
                        "IsTaxExempt",
                        "VatNumber",
                        "VatNumberStatusId",
                        "TimeZoneId",
                        "AffiliateId",
                        "Active",
                        "IsGuest",
                        "IsRegistered",
                        "IsAdministrator",
                        "IsForumModerator",
                        "FirstName",
                        "LastName",
                        "Gender",
                        "Company",
                        "StreetAddress",
                        "StreetAddress2",
                        "ZipPostalCode",
                        "City",
                        "CountryId",
                        "StateProvinceId",
                        "Phone",
                        "Fax",
                        "AvatarPictureId",
                        "ForumPostCount",
                        "Signature",
                    };
                for (int i = 0; i < properties.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = properties[i];
                    worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(184, 204, 228));
                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                }


                int row = 2;
                foreach (var customer in customers)
                {
                    int col = 1;

                    worksheet.Cells[row, col].Value = customer.Id;
                    col++;

                    worksheet.Cells[row, col].Value = customer.CustomerGuid;
                    col++;

                    worksheet.Cells[row, col].Value = customer.Email;
                    col++;

                    worksheet.Cells[row, col].Value = customer.Username;
                    col++;

                    worksheet.Cells[row, col].Value = customer.Password;
                    col++;

                    worksheet.Cells[row, col].Value = customer.PasswordFormatId;
                    col++;

                    worksheet.Cells[row, col].Value = customer.PasswordSalt;
                    col++;

                    worksheet.Cells[row, col].Value = customer.LanguageId.HasValue ? customer.LanguageId.Value : 0;
                    col++;

                    worksheet.Cells[row, col].Value = customer.CurrencyId.HasValue ? customer.CurrencyId.Value : 0;
                    col++;

                    worksheet.Cells[row, col].Value = customer.TaxDisplayTypeId;
                    col++;

                    worksheet.Cells[row, col].Value = customer.IsTaxExempt;
                    col++;

                    worksheet.Cells[row, col].Value = customer.VatNumber;
                    col++;

                    worksheet.Cells[row, col].Value = customer.VatNumberStatusId;
                    col++;

                    worksheet.Cells[row, col].Value = customer.TimeZoneId;
                    col++;

                    worksheet.Cells[row, col].Value = customer.AffiliateId.HasValue ? customer.AffiliateId.Value : 0;
                    col++;

                    worksheet.Cells[row, col].Value = customer.Active;
                    col++;

                    //roles
                    worksheet.Cells[row, col].Value = customer.IsGuest();
                    col++;

                    worksheet.Cells[row, col].Value = customer.IsRegistered();
                    col++;

                    worksheet.Cells[row, col].Value = customer.IsAdmin();
                    col++;

                    worksheet.Cells[row, col].Value = customer.IsForumModerator();
                    col++;

                    //attributes
                    var firstName = customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName);
                    var lastName = customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName);
                    var gender = customer.GetAttribute<string>(SystemCustomerAttributeNames.Gender);
                    var company = customer.GetAttribute<string>(SystemCustomerAttributeNames.Company);
                    var streetAddress = customer.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress);
                    var streetAddress2 = customer.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress2);
                    var zipPostalCode = customer.GetAttribute<string>(SystemCustomerAttributeNames.ZipPostalCode);
                    var city = customer.GetAttribute<string>(SystemCustomerAttributeNames.City);
                    var countryId = customer.GetAttribute<int>(SystemCustomerAttributeNames.CountryId);
                    var stateProvinceId = customer.GetAttribute<int>(SystemCustomerAttributeNames.StateProvinceId);
                    var phone = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone);
                    var fax = customer.GetAttribute<string>(SystemCustomerAttributeNames.Fax);

                    var avatarPictureId = customer.GetAttribute<int>(SystemCustomerAttributeNames.AvatarPictureId);
                    var forumPostCount = customer.GetAttribute<int>(SystemCustomerAttributeNames.ForumPostCount);
                    var signature = customer.GetAttribute<string>(SystemCustomerAttributeNames.Signature);

                    worksheet.Cells[row, col].Value = firstName;
                    col++;

                    worksheet.Cells[row, col].Value = lastName;
                    col++;

                    worksheet.Cells[row, col].Value = gender;
                    col++;

                    worksheet.Cells[row, col].Value = company;
                    col++;

                    worksheet.Cells[row, col].Value = streetAddress;
                    col++;

                    worksheet.Cells[row, col].Value = streetAddress2;
                    col++;

                    worksheet.Cells[row, col].Value = zipPostalCode;
                    col++;

                    worksheet.Cells[row, col].Value = city;
                    col++;

                    worksheet.Cells[row, col].Value = countryId;
                    col++;

                    worksheet.Cells[row, col].Value = stateProvinceId;
                    col++;

                    worksheet.Cells[row, col].Value = phone;
                    col++;

                    worksheet.Cells[row, col].Value = fax;
                    col++;

                    worksheet.Cells[row, col].Value = avatarPictureId;
                    col++;

                    worksheet.Cells[row, col].Value = forumPostCount;
                    col++;

                    worksheet.Cells[row, col].Value = signature;
                    col++;

                    row++;
                }








                // we had better add some document properties to the spreadsheet 

                // set some core property values
                xlPackage.Workbook.Properties.Title = string.Format("{0} customers", _storeInformationSettings.StoreName);
                xlPackage.Workbook.Properties.Author = _storeInformationSettings.StoreName;
                xlPackage.Workbook.Properties.Subject = string.Format("{0} customers", _storeInformationSettings.StoreName);
                xlPackage.Workbook.Properties.Keywords = string.Format("{0} customers", _storeInformationSettings.StoreName);
                xlPackage.Workbook.Properties.Category = "Customers";
                xlPackage.Workbook.Properties.Comments = string.Format("{0} customers", _storeInformationSettings.StoreName);

                // set some extended property values
                xlPackage.Workbook.Properties.Company = _storeInformationSettings.StoreName;
                xlPackage.Workbook.Properties.HyperlinkBase = new Uri(_storeInformationSettings.StoreUrl);

                // save the new spreadsheet
                xlPackage.Save();
            }
        }

        public virtual void ExportCustomersToXlsxForNebim(string filePath, IList<Customer> customers)
        {
            var newFile = new FileInfo(filePath);
            // ok, we can run the real code of the sample now
            using (var xlPackage = new ExcelPackage(newFile))
            {
                // uncomment this line if you want the XML written out to the outputDir
                //xlPackage.DebugMode = true; 

                // get handle to the existing worksheet
                var worksheet = xlPackage.Workbook.Worksheets.Add("Customers");
                //Create Headers and format them
                var properties = new string[]
                    {
                        "PerakendeMusteriKodu",//Id
                        "Adi",//FirstName
                        "Soyadi",//LastName
                        "Ofis",//Office
                        "TCNo",
                        "Mail",//NewsLetter
                        "SMS",
                        "AdresTipi",//billing/shipping
                        "Adres",
                        "Ilce",
                        "Il",
                        "Ulke",
                        "PostaKodu",
                         "AdresTipi2",//billing/shipping
                        "Adres2",
                        "Ilce2",
                        "Il2",
                        "Ulke2",
                        "PostaKodu2",
                        "CepTelefonu",
                        "Email",
                        "Cinsiyet",//1-erkek  2-Kadın   3-Bilinmiyor.
                        "Ozellik01",// registered customer :1, 10 admin
                        "Ozellik01Adi"// registered customer :Kayıtlı Müşteri
                       // "Dil"
                       
                     
                    };
                for (int i = 0; i < properties.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = properties[i];
                    worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(184, 204, 228));
                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                }

                Address address, shippingAddress, billingAddress = null;
                string firstName = "", lastName = "", gender = "", civilNo = "", phone = "",email = "";

                int row = 2;
                foreach (var customer in customers)
                {
                    int col = 1;
                    firstName = customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName);
                    lastName = customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName);
                    gender = customer.GetAttribute<string>(SystemCustomerAttributeNames.Gender);
                    if (gender == "M")
                        gender = "1";
                    else if (gender == "F")
                        gender = "2";
                    else
                        gender = "3";

                    var newsletter = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmail(customer.Email);
                    bool subscribedToNewsletters = newsletter != null && newsletter.Active;

                    if (customer.ShippingAddress != null)
                        shippingAddress = customer.ShippingAddress;
                    else
                        shippingAddress = customer.Addresses.LastOrDefault();

                    if (customer.BillingAddress != null)
                        billingAddress = customer.BillingAddress;
                    else
                        billingAddress = customer.Addresses.LastOrDefault();

                    if (!string.IsNullOrWhiteSpace(customer.Email))
                    {
                        email = customer.Email;
                    }
                    else if (billingAddress != null)
                    {
                        email = billingAddress.Email;
                    }
                    else //invalid customer data
                    {
                        continue;
                    }

                    civilNo = "";
                    address = customer.Addresses.LastOrDefault(x => !string.IsNullOrWhiteSpace(x.CivilNo));
                    if (address != null)
                        civilNo = address.CivilNo;

                    phone = "";
                    address = customer.Addresses.LastOrDefault(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
                    if (address != null)
                        phone = address.PhoneNumber;

                    worksheet.Cells[row, col].Value = customer.Id;
                    col++;

                    worksheet.Cells[row, col].Value = firstName;
                    col++;
                    worksheet.Cells[row, col].Value = lastName;
                    col++;

                    worksheet.Cells[row, col].Value = "1";//office
                    col++;
                    worksheet.Cells[row, col].Value = civilNo ;
                    col++;
                    worksheet.Cells[row, col].Value = subscribedToNewsletters ? 1 : 0;
                    col++;
                    worksheet.Cells[row, col].Value = 0;
                    col++;
                    worksheet.Cells[row, col].Value = 1;
                    col++;
                    worksheet.Cells[row, col].Value = shippingAddress == null ? "" : shippingAddress.Address1 + " " + shippingAddress.Address2;
                    col++;
                    worksheet.Cells[row, col].Value = shippingAddress == null ? "" : shippingAddress.City;
                    col++;
                    worksheet.Cells[row, col].Value = shippingAddress == null ? "" : shippingAddress.StateProvince == null ? "" : shippingAddress.StateProvince.Name;
                    col++;
                    worksheet.Cells[row, col].Value = shippingAddress == null ? "" : shippingAddress.Country == null ? "" : shippingAddress.Country.Name;
                    col++;

                    worksheet.Cells[row, col].Value = shippingAddress == null ? "" : shippingAddress.ZipPostalCode;
                    col++;

                    worksheet.Cells[row, col].Value = 3;
                    col++;

                    worksheet.Cells[row, col].Value = billingAddress == null ? "" : billingAddress.Address1 + " " + billingAddress.Address2;
                    col++;
                    worksheet.Cells[row, col].Value = billingAddress == null ? "" : billingAddress.City;
                    col++;
                    worksheet.Cells[row, col].Value = billingAddress == null ? "" : billingAddress.StateProvince == null ? "" : billingAddress.StateProvince.Name;
                    col++;
                    worksheet.Cells[row, col].Value = billingAddress == null ? "" : billingAddress.Country == null ? "" : billingAddress.Country.Name;
                    col++;
                    worksheet.Cells[row, col].Value = billingAddress == null ? "" : billingAddress.ZipPostalCode;
                    col++;

                    worksheet.Cells[row, col].Value = phone;
                    col++;

                    worksheet.Cells[row, col].Value = email;
                    col++;

                    worksheet.Cells[row, col].Value = gender;
                    col++;

                    worksheet.Cells[row, col].Value = 1;
                    col++;

                    worksheet.Cells[row, col].Value = customer.IsRegistered();
                    col++;

                    //worksheet.Cells[row, col].Value = customer.Language.UniqueSeoCode;
                    //col++;

                    row++;
                }

                // we had better add some document properties to the spreadsheet 

                // set some core property values
                xlPackage.Workbook.Properties.Title = string.Format("{0} customers", _storeInformationSettings.StoreName);
                xlPackage.Workbook.Properties.Author = _storeInformationSettings.StoreName;
                xlPackage.Workbook.Properties.Subject = string.Format("{0} customers", _storeInformationSettings.StoreName);
                xlPackage.Workbook.Properties.Keywords = string.Format("{0} customers", _storeInformationSettings.StoreName);
                xlPackage.Workbook.Properties.Category = "Customers";
                xlPackage.Workbook.Properties.Comments = string.Format("{0} customers", _storeInformationSettings.StoreName);

                // set some extended property values
                xlPackage.Workbook.Properties.Company = _storeInformationSettings.StoreName;
                xlPackage.Workbook.Properties.HyperlinkBase = new Uri(_storeInformationSettings.StoreUrl);

                // save the new spreadsheet
                xlPackage.Save();
            }
        }


        public virtual void ExportAddressToXlsx(string filePath, IList<NewsLetterSubscription> customers)
        {
            var newFile = new FileInfo(filePath);
            // ok, we can run the real code of the sample now
            using (var xlPackage = new ExcelPackage(newFile))
            {
                
                var worksheet = xlPackage.Workbook.Worksheets.Add("Customers");
               
                var properties = new string[]
                    {
                        "Email",
                        "Tarih"
                        
                    };
                for (int i = 0; i < properties.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = properties[i];
                    worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(184, 204, 228));
                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                }


                int row = 2;
                foreach (var item in customers)
                {
                    int col = 1;

                    worksheet.Cells[row, col].Value = item.Email;
                    col++;
                    worksheet.Cells[row, col].Value = item.CreatedOnUtc.ToShortDateString();

                    row++;
                }

                // we had better add some document properties to the spreadsheet 

                // set some core property values
                xlPackage.Workbook.Properties.Title = string.Format("{0} customers", _storeInformationSettings.StoreName);
                xlPackage.Workbook.Properties.Author = _storeInformationSettings.StoreName;
                xlPackage.Workbook.Properties.Subject = string.Format("{0} customers", _storeInformationSettings.StoreName);
                xlPackage.Workbook.Properties.Keywords = string.Format("{0} customers", _storeInformationSettings.StoreName);
                xlPackage.Workbook.Properties.Category = "Customers";
                xlPackage.Workbook.Properties.Comments = string.Format("{0} customers", _storeInformationSettings.StoreName);

                // set some extended property values
                xlPackage.Workbook.Properties.Company = _storeInformationSettings.StoreName;
                xlPackage.Workbook.Properties.HyperlinkBase = new Uri(_storeInformationSettings.StoreUrl);

                // save the new spreadsheet
                xlPackage.Save();
            }
        }

        /// <summary>
        /// Export customer list to xml
        /// </summary>
        /// <param name="customers">Customers</param>
        /// <returns>Result in XML format</returns>
        public virtual string ExportCustomersToXml(IList<Customer> customers)
        {
            var sb = new StringBuilder();
            var stringWriter = new StringWriter(sb);
            var xmlWriter = new XmlTextWriter(stringWriter);
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("Customers");
            xmlWriter.WriteAttributeString("Version", NopVersion.CurrentVersion);

            foreach (var customer in customers)
            {
                xmlWriter.WriteStartElement("Customer");
                xmlWriter.WriteElementString("CustomerId", null, customer.Id.ToString());
                xmlWriter.WriteElementString("CustomerGuid", null, customer.CustomerGuid.ToString());
                xmlWriter.WriteElementString("Email", null, customer.Email);
                xmlWriter.WriteElementString("Username", null, customer.Username);
                xmlWriter.WriteElementString("Password", null, customer.Password);
                xmlWriter.WriteElementString("PasswordFormatId", null, customer.PasswordFormatId.ToString());
                xmlWriter.WriteElementString("PasswordSalt", null, customer.PasswordSalt);
                xmlWriter.WriteElementString("LanguageId", null, customer.LanguageId.HasValue ? customer.LanguageId.ToString() : "0");
                xmlWriter.WriteElementString("CurrencyId", null, customer.CurrencyId.HasValue ? customer.CurrencyId.ToString() : "0");
                xmlWriter.WriteElementString("TaxDisplayTypeId", null, customer.TaxDisplayTypeId.ToString());
                xmlWriter.WriteElementString("IsTaxExempt", null, customer.IsTaxExempt.ToString());
                xmlWriter.WriteElementString("VatNumber", null, customer.VatNumber);
                xmlWriter.WriteElementString("VatNumberStatusId", null, customer.VatNumberStatusId.ToString());
                xmlWriter.WriteElementString("TimeZoneId", null, customer.TimeZoneId);
                xmlWriter.WriteElementString("AffiliateId", null, customer.AffiliateId.HasValue ? customer.AffiliateId.ToString() : "0");
                xmlWriter.WriteElementString("Active", null, customer.Active.ToString());


                xmlWriter.WriteElementString("IsGuest", null, customer.IsGuest().ToString());
                xmlWriter.WriteElementString("IsRegistered", null, customer.IsRegistered().ToString());
                xmlWriter.WriteElementString("IsAdministrator", null, customer.IsAdmin().ToString());
                xmlWriter.WriteElementString("IsForumModerator", null, customer.IsForumModerator().ToString());

                xmlWriter.WriteElementString("FirstName", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName));
                xmlWriter.WriteElementString("LastName", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName));
                xmlWriter.WriteElementString("Gender", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.Gender));
                xmlWriter.WriteElementString("Company", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.Company));

                xmlWriter.WriteElementString("CountryId", null, customer.GetAttribute<int>(SystemCustomerAttributeNames.CountryId).ToString());
                xmlWriter.WriteElementString("StreetAddress", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress));
                xmlWriter.WriteElementString("StreetAddress2", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress2));
                xmlWriter.WriteElementString("ZipPostalCode", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.ZipPostalCode));
                xmlWriter.WriteElementString("City", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.City));
                xmlWriter.WriteElementString("CountryId", null, customer.GetAttribute<int>(SystemCustomerAttributeNames.CountryId).ToString());
                xmlWriter.WriteElementString("StateProvinceId", null, customer.GetAttribute<int>(SystemCustomerAttributeNames.StateProvinceId).ToString());
                xmlWriter.WriteElementString("Phone", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone));
                xmlWriter.WriteElementString("Fax", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.Fax));

                var newsletter = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmail(customer.Email);
                bool subscribedToNewsletters = newsletter != null && newsletter.Active;
                xmlWriter.WriteElementString("Newsletter", null, subscribedToNewsletters.ToString());

                xmlWriter.WriteElementString("AvatarPictureId", null, customer.GetAttribute<int>(SystemCustomerAttributeNames.AvatarPictureId).ToString());
                xmlWriter.WriteElementString("ForumPostCount", null, customer.GetAttribute<int>(SystemCustomerAttributeNames.ForumPostCount).ToString());
                xmlWriter.WriteElementString("Signature", null, customer.GetAttribute<string>(SystemCustomerAttributeNames.Signature));

                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
            return stringWriter.ToString();
        }

        /// <summary>
        /// Export language resources to xml
        /// </summary>
        /// <param name="language">Language</param>
        /// <returns>Result in XML format</returns>
        public virtual string ExportLanguageToXml(Language language)
        {
            if (language == null)
                throw new ArgumentNullException("language");
            var sb = new StringBuilder();
            var stringWriter = new StringWriter(sb);
            var xmlWriter = new XmlTextWriter(stringWriter);
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("Language");
            xmlWriter.WriteAttributeString("Name", language.Name);

            var resources = language.LocaleStringResources.OrderBy(x => x.ResourceName).ToList();
            foreach (var resource in resources)
            {
                xmlWriter.WriteStartElement("LocaleResource");
                xmlWriter.WriteAttributeString("Name", resource.ResourceName);
                xmlWriter.WriteElementString("Value", null, resource.ResourceValue);
                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
            return stringWriter.ToString();
        }


        /// <summary>
        /// Export products to XLSX
        /// </summary>
        /// <param name="filePath">File path to use</param>
        /// <param name="products">Products</param>
        public virtual void ExportProductsToXlsxWithDefaultLanguage(string filePath, IList<Product> products)
        {
            var newFile = new FileInfo(filePath);
            // ok, we can run the real code of the sample now
            using (var xlPackage = new ExcelPackage(newFile))
            {
                // uncomment this line if you want the XML written out to the outputDir
                //xlPackage.DebugMode = true; 

                // get handle to the existing worksheet
                var worksheet = xlPackage.Workbook.Worksheets.Add("Products");
                //Create Headers and format them 
                var properties = new string[]
                {
                    "Name",
                    "ShortDescription",
                    "FullDescription",
                    "ProductTemplateId",
                    "ShowOnHomePage",
                    "MetaKeywords",
                    "MetaDescription",
                    "MetaTitle",
                    "SeName",
                    "AllowCustomerReviews",
                    "Published",
                    "SKU",
                    "ManufacturerPartNumber",
                    "Gtin",
                    "IsGiftCard",
                    "GiftCardTypeId",
                    "RequireOtherProducts",
                    "RequiredProductVariantIds",
                    "AutomaticallyAddRequiredProductVariants",
                    "IsDownload",
                    "DownloadId",
                    "UnlimitedDownloads",
                    "MaxNumberOfDownloads",
                    "DownloadActivationTypeId",
                    "HasSampleDownload",
                    "SampleDownloadId",
                    "HasUserAgreement",
                    "UserAgreementText",
                    "IsRecurring",
                    "RecurringCycleLength",
                    "RecurringCyclePeriodId",
                    "RecurringTotalCycles",
                    "IsShipEnabled",
                    "IsFreeShipping",
                    "AdditionalShippingCharge",
                    "IsTaxExempt",
                    "TaxCategoryId",
                    "ManageInventoryMethodId",
                    "StockQuantity",
                    "DisplayStockAvailability",
                    "DisplayStockQuantity",
                    "MinStockQuantity",
                    "LowStockActivityId",
                    "NotifyAdminForQuantityBelow",
                    "BackorderModeId",
                    "AllowBackInStockSubscriptions",
                    "OrderMinimumQuantity",
                    "OrderMaximumQuantity",
                    "DisableBuyButton",
                    "DisableWishlistButton",
                    "CallForPrice",
                    "Price",
                    "OldPrice",
                    "ProductCost",
                    "SpecialPrice",
                    "SpecialPriceStartDateTimeUtc",
                    "SpecialPriceEndDateTimeUtc",
                    "CustomerEntersPrice",
                    "MinimumCustomerEnteredPrice",
                    "MaximumCustomerEnteredPrice",
                    "Weight",
                    "Length",
                    "Width",
                    "Height",
                    "CreatedOnUtc",
                    "CategoryIds",
                    "ManufacturerIds",
                    "Picture1",
                    "Picture2",
                    "Picture3",
                    "CurrencyId",
                    "CurrencyPrice",
                    "CurrencyOldPrice",
                    "CurrencyProductCost",
                    "DisplayPriceIfCallforPrice",
                    "HideDiscount",
                    "SpecificationIds",
                    "SpecificationIdDescriptions"
                };
                for (int i = 0; i < properties.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = properties[i];
                    worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(184, 204, 228));
                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                }


                int row = 2;
                foreach (var p in products)
                {
                    var productVariants = _productService.GetProductVariantsByProductId(p.Id, true);
                    foreach (var pv in productVariants)
                    {
                        int col = 1;

                        worksheet.Cells[row, col].Value = p.Name;
                        col++;

                        worksheet.Cells[row, col].Value = p.ShortDescription;
                        col++;

                        worksheet.Cells[row, col].Value = p.FullDescription;
                        col++;

                        worksheet.Cells[row, col].Value = p.ProductTemplateId;
                        col++;

                        worksheet.Cells[row, col].Value = p.ShowOnHomePage;
                        col++;

                        worksheet.Cells[row, col].Value = p.MetaKeywords;
                        col++;

                        worksheet.Cells[row, col].Value = p.MetaDescription;
                        col++;

                        worksheet.Cells[row, col].Value = p.MetaTitle;
                        col++;

                        worksheet.Cells[row, col].Value = p.SeName;
                        col++;

                        worksheet.Cells[row, col].Value = p.AllowCustomerReviews;
                        col++;

                        worksheet.Cells[row, col].Value = p.Published;
                        col++;

                        worksheet.Cells[row, col].Value = pv.Sku;
                        col++;

                        worksheet.Cells[row, col].Value = pv.ManufacturerPartNumber;
                        col++;

                        worksheet.Cells[row, col].Value = pv.Gtin;
                        col++;

                        worksheet.Cells[row, col].Value = pv.IsGiftCard;
                        col++;

                        worksheet.Cells[row, col].Value = pv.GiftCardTypeId;
                        col++;

                        worksheet.Cells[row, col].Value = pv.RequireOtherProducts;
                        col++;

                        worksheet.Cells[row, col].Value = pv.RequiredProductVariantIds;
                        col++;

                        worksheet.Cells[row, col].Value = pv.AutomaticallyAddRequiredProductVariants;
                        col++;

                        worksheet.Cells[row, col].Value = pv.IsDownload;
                        col++;

                        worksheet.Cells[row, col].Value = pv.DownloadId;
                        col++;

                        worksheet.Cells[row, col].Value = pv.UnlimitedDownloads;
                        col++;

                        worksheet.Cells[row, col].Value = pv.MaxNumberOfDownloads;
                        col++;

                        worksheet.Cells[row, col].Value = pv.DownloadActivationTypeId;
                        col++;

                        worksheet.Cells[row, col].Value = pv.HasSampleDownload;
                        col++;

                        worksheet.Cells[row, col].Value = pv.SampleDownloadId;
                        col++;

                        worksheet.Cells[row, col].Value = pv.HasUserAgreement;
                        col++;

                        worksheet.Cells[row, col].Value = pv.UserAgreementText;
                        col++;

                        worksheet.Cells[row, col].Value = pv.IsRecurring;
                        col++;

                        worksheet.Cells[row, col].Value = pv.RecurringCycleLength;
                        col++;

                        worksheet.Cells[row, col].Value = pv.RecurringCyclePeriodId;
                        col++;

                        worksheet.Cells[row, col].Value = pv.RecurringTotalCycles;
                        col++;

                        worksheet.Cells[row, col].Value = pv.IsShipEnabled;
                        col++;

                        worksheet.Cells[row, col].Value = pv.IsFreeShipping;
                        col++;

                        worksheet.Cells[row, col].Value = pv.AdditionalShippingCharge;
                        col++;

                        worksheet.Cells[row, col].Value = pv.IsTaxExempt;
                        col++;

                        worksheet.Cells[row, col].Value = pv.TaxCategoryId;
                        col++;

                        worksheet.Cells[row, col].Value = pv.ManageInventoryMethodId;
                        col++;

                        worksheet.Cells[row, col].Value = pv.StockQuantity;
                        col++;

                        worksheet.Cells[row, col].Value = pv.DisplayStockAvailability;
                        col++;

                        worksheet.Cells[row, col].Value = pv.DisplayStockQuantity;
                        col++;

                        worksheet.Cells[row, col].Value = pv.MinStockQuantity;
                        col++;

                        worksheet.Cells[row, col].Value = pv.LowStockActivityId;
                        col++;

                        worksheet.Cells[row, col].Value = pv.NotifyAdminForQuantityBelow;
                        col++;

                        worksheet.Cells[row, col].Value = pv.BackorderModeId;
                        col++;

                        worksheet.Cells[row, col].Value = pv.AllowBackInStockSubscriptions;
                        col++;

                        worksheet.Cells[row, col].Value = pv.OrderMinimumQuantity;
                        col++;

                        worksheet.Cells[row, col].Value = pv.OrderMaximumQuantity;
                        col++;

                        //worksheet.Cells[row, col].Value = pv.AllowedQuantities;
                        //col++;

                        worksheet.Cells[row, col].Value = pv.DisableBuyButton;
                        col++;

                        worksheet.Cells[row, col].Value = pv.DisableWishlistButton;
                        col++;

                        worksheet.Cells[row, col].Value = pv.CallForPrice;
                        col++;

                        worksheet.Cells[row, col].Value = pv.Price;
                        col++;

                        worksheet.Cells[row, col].Value = pv.OldPrice;
                        col++;

                        worksheet.Cells[row, col].Value = pv.ProductCost;
                        col++;

                        worksheet.Cells[row, col].Value = pv.SpecialPrice;
                        col++;

                        worksheet.Cells[row, col].Value = pv.SpecialPriceStartDateTimeUtc;
                        col++;

                        worksheet.Cells[row, col].Value = pv.SpecialPriceEndDateTimeUtc;
                        col++;

                        worksheet.Cells[row, col].Value = pv.CustomerEntersPrice;
                        col++;

                        worksheet.Cells[row, col].Value = pv.MinimumCustomerEnteredPrice;
                        col++;

                        worksheet.Cells[row, col].Value = pv.MaximumCustomerEnteredPrice;
                        col++;

                        worksheet.Cells[row, col].Value = pv.Weight;
                        col++;

                        worksheet.Cells[row, col].Value = pv.Length;
                        col++;

                        worksheet.Cells[row, col].Value = pv.Width;
                        col++;

                        worksheet.Cells[row, col].Value = pv.Height;
                        col++;

                        worksheet.Cells[row, col].Value = pv.CreatedOnUtc.ToOADate();
                        col++;

                        //category identifiers
                        string categoryIds = null;
                        foreach (var pc in _categoryService.GetProductCategoriesByProductId(p.Id))
                        {
                            categoryIds += pc.CategoryId;
                            categoryIds += ";";
                        }
                        worksheet.Cells[row, col].Value = categoryIds;
                        col++;

                        //manufacturer identifiers
                        string manufacturerIds = null;
                        foreach (var pm in _manufacturerService.GetProductManufacturersByProductId(p.Id))
                        {
                            manufacturerIds += pm.ManufacturerId;
                            manufacturerIds += ";";
                        }
                        worksheet.Cells[row, col].Value = manufacturerIds;
                        col++;

                        //pictures (up to 3 pictures)
                        string picture1 = null;
                        string picture2 = null;
                        string picture3 = null;
                        var pictures = _pictureService.GetPicturesByProductId(p.Id, 3);
                        for (int i = 0; i < pictures.Count; i++)
                        {
                            string pictureLocalPath = _pictureService.GetPictureLocalPath(pictures[i]);
                            switch (i)
                            {
                                case 0:
                                    picture1 = pictureLocalPath;
                                    break;
                                case 1:
                                    picture2 = pictureLocalPath;
                                    break;
                                case 2:
                                    picture3 = pictureLocalPath;
                                    break;
                            }
                        }
                        worksheet.Cells[row, col].Value = picture1;
                        col++;
                        worksheet.Cells[row, col].Value = picture2;
                        col++;
                        worksheet.Cells[row, col].Value = picture3;
                        col++;

                        worksheet.Cells[row, col].Value = pv.CurrencyId;
                        col++;

                        worksheet.Cells[row, col].Value = pv.CurrencyPrice;
                        col++;

                        worksheet.Cells[row, col].Value = pv.CurrencyOldPrice;
                        col++;

                        worksheet.Cells[row, col].Value = pv.CurrencyProductCost;
                        col++;

                        worksheet.Cells[row, col].Value = pv.DisplayPriceIfCallforPrice;
                        col++;

                        worksheet.Cells[row, col].Value = pv.HideDiscount;
                        col++;


                        var productSpecificationAttributes = p.ProductSpecificationAttributes;
                        var sb = new StringBuilder();
                        foreach (var psa in productSpecificationAttributes)
                        {
                            sb.Append(string.Format("{0}:{1};",
                                psa.SpecificationAttributeOption.SpecificationAttribute.Id,
                                psa.SpecificationAttributeOptionId
                               ));
                        }

                        worksheet.Cells[row, col].Value = sb.ToString();
                        col++;

                        sb = new StringBuilder();
                        foreach (var psa in productSpecificationAttributes)
                        {
                            sb.Append(string.Format("{0}({1}):{2}({3});",
                                psa.SpecificationAttributeOption.SpecificationAttribute.Id,
                                psa.SpecificationAttributeOption.SpecificationAttribute.Name,
                                psa.SpecificationAttributeOptionId,
                                psa.SpecificationAttributeOption.Name));
                        }
                        worksheet.Cells[row, col].Value = sb.ToString();
                        col++;

                        //NEW CODE - modifying export to export specifications for each product
                        //are there any product specification attributes? if so then we'll write them to the specifiations column in the form of XML
                        //if (p.ProductSpecificationAttributes.Count > 0) {
                        //get prodcut specification attributes so we can export them to the excel file
                        sb = new StringBuilder();
                        var stringWriterProductSpecifications = new StringWriter(sb);
                        var xmlWriter = new XmlTextWriter(stringWriterProductSpecifications);
                        xmlWriter.WriteStartDocument();
                        //ProductSpecificationAttributes
                        xmlWriter.WriteStartElement("psas");

                        foreach (var productSpecificationAttribute in productSpecificationAttributes)
                        {
                            //ProductSpecificationAttribute
                            xmlWriter.WriteStartElement("psa");
                            //ProductSpecificationAttributeId
                            xmlWriter.WriteElementString("id", null, productSpecificationAttribute.Id.ToString());
                            //SpecificationAttributeOptionId
                            xmlWriter.WriteElementString("saoid", null, productSpecificationAttribute.SpecificationAttributeOptionId.ToString());
                            //CustomValue
                            xmlWriter.WriteElementString("cv", null, "");
                            //AllowFiltering
                            xmlWriter.WriteElementString("af", null, productSpecificationAttribute.AllowFiltering.ToString());
                            //ShowOnProductPage
                            xmlWriter.WriteElementString("sopp", null, productSpecificationAttribute.ShowOnProductPage.ToString());
                            //DisplayOrder
                            xmlWriter.WriteElementString("do", null, productSpecificationAttribute.DisplayOrder.ToString());
                            xmlWriter.WriteEndElement();
                        }
                        xmlWriter.WriteEndElement();
                        xmlWriter.WriteEndDocument();
                        xmlWriter.Close();

                        //string that represents all the specification data for this product, let's stick this in a new row in the excel file
                        worksheet.Cells[row, col].Value = stringWriterProductSpecifications.ToString();
                        col++;
                        //END OF NEW CODE




                        row++;
                    }
                }








                // we had better add some document properties to the spreadsheet 

                // set some core property values
                xlPackage.Workbook.Properties.Title = string.Format("{0} products", _storeInformationSettings.StoreName);
                xlPackage.Workbook.Properties.Author = _storeInformationSettings.StoreName;
                xlPackage.Workbook.Properties.Subject = string.Format("{0} products", _storeInformationSettings.StoreName);
                xlPackage.Workbook.Properties.Keywords = string.Format("{0} products", _storeInformationSettings.StoreName);
                xlPackage.Workbook.Properties.Category = "Products";
                xlPackage.Workbook.Properties.Comments = string.Format("{0} products", _storeInformationSettings.StoreName);

                // set some extended property values
                xlPackage.Workbook.Properties.Company = _storeInformationSettings.StoreName;
                xlPackage.Workbook.Properties.HyperlinkBase = new Uri(_storeInformationSettings.StoreUrl);

                // save the new spreadsheet
                xlPackage.Save();
            }
        }

        /// <summary>
        /// Export products to XLSX
        /// </summary>
        /// <param name="filePath">File path to use</param>
        /// <param name="products">Products</param>
        public virtual void ExportProductsToXlsx(string filePath, IList<Product> products)
        {
            var newFile = new FileInfo(filePath);
            // ok, we can run the real code of the sample now
            using (var xlPackage = new ExcelPackage(newFile))
            {
                // uncomment this line if you want the XML written out to the outputDir
                //xlPackage.DebugMode = true; 

                // get handle to the existing worksheet
                var worksheet = xlPackage.Workbook.Worksheets.Add("Products");
                //Create Headers and format them 
                var properties = new string[]
                {
                    "Name",
                    "ShortDescription",
                    "FullDescription",
                    "ProductTemplateId",
                    "ShowOnHomePage",
                    "MetaKeywords",
                    "MetaDescription",
                    "MetaTitle",
                    "SeName",
                    "AllowCustomerReviews",
                    "Published",
                    "SKU",
                    "ManufacturerPartNumber",
                    "Gtin",
                    "IsGiftCard",
                    "GiftCardTypeId",
                    "RequireOtherProducts",
                    "RequiredProductVariantIds",
                    "AutomaticallyAddRequiredProductVariants",
                    "IsDownload",
                    "DownloadId",
                    "UnlimitedDownloads",
                    "MaxNumberOfDownloads",
                    "DownloadActivationTypeId",
                    "HasSampleDownload",
                    "SampleDownloadId",
                    "HasUserAgreement",
                    "UserAgreementText",
                    "IsRecurring",
                    "RecurringCycleLength",
                    "RecurringCyclePeriodId",
                    "RecurringTotalCycles",
                    "IsShipEnabled",
                    "IsFreeShipping",
                    "AdditionalShippingCharge",
                    "IsTaxExempt",
                    "TaxCategoryId",
                    "ManageInventoryMethodId",
                    "StockQuantity",
                    "DisplayStockAvailability",
                    "DisplayStockQuantity",
                    "MinStockQuantity",
                    "LowStockActivityId",
                    "NotifyAdminForQuantityBelow",
                    "BackorderModeId",
                    "AllowBackInStockSubscriptions",
                    "OrderMinimumQuantity",
                    "OrderMaximumQuantity",
                    "DisableBuyButton",
                    "DisableWishlistButton",
                    "CallForPrice",
                    "Price",
                    "OldPrice",
                    "ProductCost",
                    "SpecialPrice",
                    "SpecialPriceStartDateTimeUtc",
                    "SpecialPriceEndDateTimeUtc",
                    "CustomerEntersPrice",
                    "MinimumCustomerEnteredPrice",
                    "MaximumCustomerEnteredPrice",
                    "Weight",
                    "Length",
                    "Width",
                    "Height",
                    "CreatedOnUtc",
                    "CategoryIds",
                    "ManufacturerIds",
                    "Picture1",
                    "Picture2",
                    "Picture3",
                    "CurrencyId",
                    "CurrencyPrice",
                    "CurrencyOldPrice",
                    "CurrencyProductCost",
                    "DisplayPriceIfCallforPrice",
                    "HideDiscount"

                };
                for (int i = 0; i < properties.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = properties[i];
                    worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(184, 204, 228));
                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                }

                /*
                foreach (var language in languageService.GetAllLanguages(true))
            {
                    locale.Name = product.GetLocalized(x => x.Name, languageId, false, false);
                    locale.ShortDescription = product.GetLocalized(x => x.ShortDescription, languageId, false, false);
                    locale.FullDescription = product.GetLocalized(x => x.FullDescription, languageId, false, false);
                    locale.MetaKeywords = product.GetLocalized(x => x.MetaKeywords, languageId, false, false);
                    locale.MetaDescription = product.GetLocalized(x => x.MetaDescription, languageId, false, false);
                    locale.MetaTitle = product.GetLocalized(x => x.MetaTitle, languageId, false, false);
                    locale.SeName = product.GetLocalized(x => x.SeName, languageId, false, false);
               
                  }
                 */




                int row = 2;
                foreach (var p in products)
                {
                    var productVariants = _productService.GetProductVariantsByProductId(p.Id, true);
                    foreach (var pv in productVariants)
                    {
                        int col = 1;
                        string name = "", shortDescription = "", fullDescription = "", metaKeywords = "", metaDescription = "", metaTitle = "", seName = "";

                        foreach (var language in _languageService.GetAllLanguages(true))
                        {
                            name += string.Format("###{0}{1}", language.UniqueSeoCode, p.GetLocalized(x => x.Name, language.Id, false, false));
                            shortDescription += string.Format("###{0}{1}", language.UniqueSeoCode, p.GetLocalized(x => x.ShortDescription, language.Id, false, false));
                            fullDescription += string.Format("###{0}{1}", language.UniqueSeoCode, p.GetLocalized(x => x.FullDescription, language.Id, false, false));
                            metaKeywords += string.Format("###{0}{1}", language.UniqueSeoCode, p.GetLocalized(x => x.MetaKeywords, language.Id, false, false));
                            metaDescription += string.Format("###{0}{1}", language.UniqueSeoCode, p.GetLocalized(x => x.MetaDescription, language.Id, false, false));
                            metaTitle += string.Format("###{0}{1}", language.UniqueSeoCode, p.GetLocalized(x => x.MetaTitle, language.Id, false, false));
                            seName += string.Format("###{0}{1}", language.UniqueSeoCode, p.GetLocalized(x => x.SeName, language.Id, false, false));

                        }


                        worksheet.Cells[row, col].Value = name;
                        col++;

                        worksheet.Cells[row, col].Value = shortDescription;
                        col++;

                        worksheet.Cells[row, col].Value = fullDescription;
                        col++;

                        worksheet.Cells[row, col].Value = p.ProductTemplateId;
                        col++;

                        worksheet.Cells[row, col].Value = p.ShowOnHomePage;
                        col++;

                        worksheet.Cells[row, col].Value = metaKeywords;
                        col++;

                        worksheet.Cells[row, col].Value = metaDescription;
                        col++;

                        worksheet.Cells[row, col].Value = metaTitle;
                        col++;

                        worksheet.Cells[row, col].Value = seName;
                        col++;

                        worksheet.Cells[row, col].Value = p.AllowCustomerReviews;
                        col++;

                        worksheet.Cells[row, col].Value = p.Published;
                        col++;

                        worksheet.Cells[row, col].Value = pv.Sku;
                        col++;

                        worksheet.Cells[row, col].Value = pv.ManufacturerPartNumber;
                        col++;

                        worksheet.Cells[row, col].Value = pv.Gtin;
                        col++;

                        worksheet.Cells[row, col].Value = pv.IsGiftCard;
                        col++;

                        worksheet.Cells[row, col].Value = pv.GiftCardTypeId;
                        col++;

                        worksheet.Cells[row, col].Value = pv.RequireOtherProducts;
                        col++;

                        worksheet.Cells[row, col].Value = pv.RequiredProductVariantIds;
                        col++;

                        worksheet.Cells[row, col].Value = pv.AutomaticallyAddRequiredProductVariants;
                        col++;

                        worksheet.Cells[row, col].Value = pv.IsDownload;
                        col++;

                        worksheet.Cells[row, col].Value = pv.DownloadId;
                        col++;

                        worksheet.Cells[row, col].Value = pv.UnlimitedDownloads;
                        col++;

                        worksheet.Cells[row, col].Value = pv.MaxNumberOfDownloads;
                        col++;

                        worksheet.Cells[row, col].Value = pv.DownloadActivationTypeId;
                        col++;

                        worksheet.Cells[row, col].Value = pv.HasSampleDownload;
                        col++;

                        worksheet.Cells[row, col].Value = pv.SampleDownloadId;
                        col++;

                        worksheet.Cells[row, col].Value = pv.HasUserAgreement;
                        col++;

                        worksheet.Cells[row, col].Value = pv.UserAgreementText;
                        col++;

                        worksheet.Cells[row, col].Value = pv.IsRecurring;
                        col++;

                        worksheet.Cells[row, col].Value = pv.RecurringCycleLength;
                        col++;

                        worksheet.Cells[row, col].Value = pv.RecurringCyclePeriodId;
                        col++;

                        worksheet.Cells[row, col].Value = pv.RecurringTotalCycles;
                        col++;

                        worksheet.Cells[row, col].Value = pv.IsShipEnabled;
                        col++;

                        worksheet.Cells[row, col].Value = pv.IsFreeShipping;
                        col++;

                        worksheet.Cells[row, col].Value = pv.AdditionalShippingCharge;
                        col++;

                        worksheet.Cells[row, col].Value = pv.IsTaxExempt;
                        col++;

                        worksheet.Cells[row, col].Value = pv.TaxCategoryId;
                        col++;

                        worksheet.Cells[row, col].Value = pv.ManageInventoryMethodId;
                        col++;

                        worksheet.Cells[row, col].Value = pv.StockQuantity;
                        col++;

                        worksheet.Cells[row, col].Value = pv.DisplayStockAvailability;
                        col++;

                        worksheet.Cells[row, col].Value = pv.DisplayStockQuantity;
                        col++;

                        worksheet.Cells[row, col].Value = pv.MinStockQuantity;
                        col++;

                        worksheet.Cells[row, col].Value = pv.LowStockActivityId;
                        col++;

                        worksheet.Cells[row, col].Value = pv.NotifyAdminForQuantityBelow;
                        col++;

                        worksheet.Cells[row, col].Value = pv.BackorderModeId;
                        col++;

                        worksheet.Cells[row, col].Value = pv.AllowBackInStockSubscriptions;
                        col++;

                        worksheet.Cells[row, col].Value = pv.OrderMinimumQuantity;
                        col++;

                        worksheet.Cells[row, col].Value = pv.OrderMaximumQuantity;
                        col++;

                        worksheet.Cells[row, col].Value = pv.DisableBuyButton;
                        col++;

                        worksheet.Cells[row, col].Value = pv.DisableWishlistButton;
                        col++;

                        worksheet.Cells[row, col].Value = pv.CallForPrice;
                        col++;

                        worksheet.Cells[row, col].Value = pv.Price;
                        col++;

                        worksheet.Cells[row, col].Value = pv.OldPrice;
                        col++;

                        worksheet.Cells[row, col].Value = pv.ProductCost;
                        col++;

                        worksheet.Cells[row, col].Value = pv.SpecialPrice;
                        col++;

                        worksheet.Cells[row, col].Value = pv.SpecialPriceStartDateTimeUtc;
                        col++;

                        worksheet.Cells[row, col].Value = pv.SpecialPriceEndDateTimeUtc;
                        col++;

                        worksheet.Cells[row, col].Value = pv.CustomerEntersPrice;
                        col++;

                        worksheet.Cells[row, col].Value = pv.MinimumCustomerEnteredPrice;
                        col++;

                        worksheet.Cells[row, col].Value = pv.MaximumCustomerEnteredPrice;
                        col++;

                        worksheet.Cells[row, col].Value = pv.Weight;
                        col++;

                        worksheet.Cells[row, col].Value = pv.Length;
                        col++;

                        worksheet.Cells[row, col].Value = pv.Width;
                        col++;

                        worksheet.Cells[row, col].Value = pv.Height;
                        col++;

                        worksheet.Cells[row, col].Value = pv.CreatedOnUtc.ToOADate();
                        col++;

                        //category identifiers
                        string categoryIds = null;
                        foreach (var pc in _categoryService.GetProductCategoriesByProductId(p.Id))
                        {
                            categoryIds += pc.CategoryId;
                            categoryIds += ";";
                        }
                        worksheet.Cells[row, col].Value = categoryIds;
                        col++;

                        //manufacturer identifiers
                        string manufacturerIds = null;
                        foreach (var pm in _manufacturerService.GetProductManufacturersByProductId(p.Id))
                        {
                            manufacturerIds += pm.ManufacturerId;
                            manufacturerIds += ";";
                        }
                        worksheet.Cells[row, col].Value = manufacturerIds;
                        col++;

                        //pictures (up to 3 pictures)
                        string picture1 = null;
                        string picture2 = null;
                        string picture3 = null;
                        var pictures = _pictureService.GetPicturesByProductId(p.Id, 3);
                        for (int i = 0; i < pictures.Count; i++)
                        {
                            string pictureLocalPath = _pictureService.GetPictureLocalPath(pictures[i]);
                            switch (i)
                            {
                                case 0:
                                    picture1 = pictureLocalPath;
                                    break;
                                case 1:
                                    picture2 = pictureLocalPath;
                                    break;
                                case 2:
                                    picture3 = pictureLocalPath;
                                    break;
                            }
                        }
                        worksheet.Cells[row, col].Value = picture1;
                        col++;
                        worksheet.Cells[row, col].Value = picture2;
                        col++;
                        worksheet.Cells[row, col].Value = picture3;
                        col++;
                        /*
                         "CurrencyId",
                    "CurrencyPrice",
                    "CurrencyOldPrice",
                    "CurrencyProductCost",
                    "DisplayPriceIfCallforPrice",
                    "HideDiscount"
                          */
                        worksheet.Cells[row, col].Value = pv.CurrencyId;
                        col++;

                        worksheet.Cells[row, col].Value = pv.CurrencyPrice;
                        col++;

                        worksheet.Cells[row, col].Value = pv.CurrencyOldPrice;
                        col++;

                        worksheet.Cells[row, col].Value = pv.CurrencyProductCost;
                        col++;

                        worksheet.Cells[row, col].Value = pv.DisplayPriceIfCallforPrice;
                        col++;

                        worksheet.Cells[row, col].Value = pv.HideDiscount;
                        col++;

                        row++;
                    }
                }








                // we had better add some document properties to the spreadsheet 

                // set some core property values
                xlPackage.Workbook.Properties.Title = string.Format("{0} products", _storeInformationSettings.StoreName);
                xlPackage.Workbook.Properties.Author = _storeInformationSettings.StoreName;
                xlPackage.Workbook.Properties.Subject = string.Format("{0} products", _storeInformationSettings.StoreName);
                xlPackage.Workbook.Properties.Keywords = string.Format("{0} products", _storeInformationSettings.StoreName);
                xlPackage.Workbook.Properties.Category = "Products";
                xlPackage.Workbook.Properties.Comments = string.Format("{0} products", _storeInformationSettings.StoreName);

                // set some extended property values
                xlPackage.Workbook.Properties.Company = _storeInformationSettings.StoreName;
                xlPackage.Workbook.Properties.HyperlinkBase = new Uri(_storeInformationSettings.StoreUrl);

                // save the new spreadsheet
                xlPackage.Save();
            }
        }


        public virtual void ExportProductsToXlsxForNebim(string filePath, IList<Product> products)
        {
            var newFile = new FileInfo(filePath);

            IProductAttributeParser productAttributeParser = EngineContext.Current.Resolve<IProductAttributeParser>();
            ITaxService taxService = EngineContext.Current.Resolve<ITaxService>();
            IWorkContext workContext = EngineContext.Current.Resolve<IWorkContext>();
            ISpecificationAttributeService specificationAttributeService = EngineContext.Current.Resolve<ISpecificationAttributeService>();

            // ok, we can run the real code of the sample now
            using (var xlPackage = new ExcelPackage(newFile))
            {
                // uncomment this line if you want the XML written out to the outputDir
                //xlPackage.DebugMode = true; 

                // get handle to the existing worksheet
                var worksheet = xlPackage.Workbook.Worksheets.Add("Ürün Kartı");
                //Create Headers and format them 
                var properties = new string[]
                {
                    "Product Code",//sku
                   // "Stock",
                    "EanKodu",
                    "Product Name(TR)",
                    "Product Name(EN)",
                    "Item Dim Type Code",
                    "Color Code",
                    "Color Name(TR)",
                    "Color Code(EN)",
                    "Item Dim1 Code",
                    "Item Dim2 Code",
                    "Unit Of Measure Code1",
                    "Country Code",
                    "Purc Vat Rate",//tax rate
                    "Sell Vat Rate",//tax rate
                    "Item AccountGr Code",//tax rate
                    "Product Hierarchy Level01",//category root
                    "Product Hierarchy Level02",//category 
                    "Product Hierarchy Level03",//category
                    "Attribute Code01",//manufacturer Id
                    "Attribute Code01 Adı",//manufacturer name
                    "Attribute Code02",//
                    "Attribute Code02 Adı",//
                    "Attribute Code03",//
                    "Attribute Code03 Adı",//
                    "USD",
                    "TL",
                    "EURO",
                    "CHF"

                };
                for (int i = 0; i < properties.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = properties[i];
                    worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(184, 204, 228));
                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                }



                int row = 2;
                foreach (var p in products)
                {
                    var productVariants = _productService.GetProductVariantsByProductId(p.Id, true);
                    foreach (var pv in productVariants)
                    {
                        try
                        {
                            string nameTr = "", nameEn = "";
                            int dimTypeCode = 0;
                            string colorCode = "";
                            string colorNameTr = "", colorNameEn = "";
                            string dim1Code = "", dim2Code = "";
                            string taxRate = taxService.GetTaxRate(pv.TaxCategoryId, workContext.CurrentCustomer).ToString();
                            Category categoryLeaf = null, categoryLeafParent = null, categoryLeafRoot = null;
                            string categor1 = "", categor2 = "", categor3 = "";
                            Manufacturer manufacturer = null;
                            string manufacturerName = "", manufacturerCode = "";
                            string attribute2Code = "", attribute2Name = "", attribute3Code = "", attribute3Name = "";
                            decimal? USD = null, TL = null, EURO = null, CHF = null;
                            string eanCode = null;

                            nameTr = pv.GetLocalized(x => x.Name, 2);
                            if (string.IsNullOrWhiteSpace(nameTr)) nameTr = p.GetLocalized(x => x.Name, 2);
                            nameEn = pv.GetLocalized(x => x.Name, 1);
                            if (string.IsNullOrWhiteSpace(nameEn)) nameEn = p.GetLocalized(x => x.Name, 1);

                            categoryLeaf = p.GetDefaultProductCategory();
                            if (categoryLeaf != null && categoryLeaf.ParentCategoryId != 0)
                            {
                                categoryLeafParent = _categoryService.GetCategoryById(categoryLeaf.ParentCategoryId);

                                if (categoryLeafParent != null && categoryLeafParent.ParentCategoryId != 0)
                                {
                                    categoryLeafRoot = _categoryService.GetCategoryById(categoryLeafParent.ParentCategoryId);
                                }
                            }
                            if (categoryLeaf != null && categoryLeafParent == null)
                            {
                                categor1 = categoryLeaf.GetLocalized(x => x.Name, 2);
                            }
                            else if (categoryLeaf != null && categoryLeafParent != null && categoryLeafRoot == null)
                            {
                                categor1 = categoryLeafParent.GetLocalized(x => x.Name, 2);
                                categor2 = categoryLeaf.GetLocalized(x => x.Name, 2);
                            }
                            else if (categoryLeaf != null && categoryLeafParent != null && categoryLeafRoot != null)
                            {
                                categor1 = categoryLeafRoot.GetLocalized(x => x.Name, 2);
                                categor2 = categoryLeafParent.GetLocalized(x => x.Name, 2);
                                categor3 = categoryLeaf.GetLocalized(x => x.Name, 2);
                            }

                            manufacturer = p.GetDefaultManufacturer();
                            if (manufacturer != null)
                            {
                                manufacturerName = manufacturer.Name;
                                manufacturerCode = manufacturer.Id.ToString();
                            }

                            var psas = specificationAttributeService.GetProductSpecificationAttributesByProductId(pv.ProductId);
                            var saKoleksiyon = psas.FirstOrDefault(x => x.SpecificationAttributeOption.SpecificationAttribute.Name.ToLower() == "koleksiyon");
                            var saAltkategoriler = psas.FirstOrDefault(x => x.SpecificationAttributeOption.SpecificationAttribute.Name.ToLower() == "alt kategoriler");
                            if (saKoleksiyon != null)
                            {
                                attribute2Code = saKoleksiyon.SpecificationAttributeOptionId.ToString();
                                attribute2Name = saKoleksiyon.SpecificationAttributeOption.Name;
                            }
                            if (saAltkategoriler != null)
                            {
                                attribute3Code = saAltkategoriler.SpecificationAttributeOptionId.ToString();
                                attribute3Name = saAltkategoriler.SpecificationAttributeOption.Name;
                            }
                            if (pv.CurrencyId.HasValue)
                            {
                                switch (pv.CurrencyId.Value)
                                {
                                    case 12://tl    
                                        TL = pv.Price;
                                        break;
                                    case 1://$    
                                        USD = pv.CurrencyPrice;
                                        break;
                                    case 6://Euro    
                                        EURO = pv.CurrencyPrice;
                                        break;
                                    case 13://CHF   
                                        CHF = pv.CurrencyPrice;
                                        break;
                                    default:
                                        TL = pv.Price;
                                        break;
                                }
                            }
                            else
                            {
                                TL = pv.Price;
                            }


                            var pvas = pv.ProductVariantAttributes;
                            var combs = pv.ProductVariantAttributeCombinations;
                            //   var pvaValues = productAttributeParser.ParseProductVariantAttributeValues(comb.AttributesXml);


                            if (!pvas.Any())
                            {
                                dimTypeCode = 0;
                                colorCode = "STD";
                                colorNameTr = "";
                                colorNameEn = "";
                            }

                            else if (!combs.Any())
                            {
                                if (pvas.Any(x => x.ProductAttribute.Name == "RENK"))//only color attribute
                                {
                                    dimTypeCode = 1;
                                    var value = pvas.FirstOrDefault(x => x.ProductAttribute.Name == "RENK").ProductVariantAttributeValues.FirstOrDefault();

                                    if (value != null)
                                    {
                                        colorNameTr = value.GetLocalized(pvav => pvav.Name, 2);
                                        colorNameEn = value.GetLocalized(pvav => pvav.Name, 1);
                                        colorCode = value.ProductAttributeOptionId.ToString();
                                    }

                                }
                                else // add default color so have +1 attributes.
                                {
                                    colorCode = "STD";
                                    colorNameTr = "";
                                    colorNameEn = "";
                                    dimTypeCode = 2;
                                    var nonColorAtributes = pvas.Where(x => x.ProductAttribute.Name != "RENK").ToList();
                                    var first = nonColorAtributes.FirstOrDefault();
                                    if (first != null)
                                    {
                                        var value = first.ProductVariantAttributeValues.FirstOrDefault();
                                        dim1Code = value == null ? "" : value.GetLocalized(x => x.Name);
                                    }

                                }
                            }
                            //add xls row
                            if (!combs.Any())
                            {
                                this.WriteXlsxRowForNebim(worksheet, row, pv.Sku,pv.Gtin, nameTr, nameEn, dimTypeCode.ToString(), colorCode, colorNameTr, colorNameEn, dim1Code, dim2Code, "AD", "TR", taxRate, taxRate, taxRate, categor1, categor2, categor3, manufacturerCode, manufacturerName, attribute2Code, attribute2Name, attribute3Code, attribute3Name, USD, TL, EURO, CHF);
                                row++;
                                continue;
                            }

                            if (combs.Any())//has combinations
                            {
                                //add rows for each combination
                                foreach (var comb in combs)
                                {
                                    if(comb.ProductVariantBarcode != null)
                                        eanCode = comb.ProductVariantBarcode.ToString();

                                    var pvaValues = productAttributeParser.ParseProductVariantAttributeValues(comb.AttributesXml);
                                    var pvavColor = pvaValues.FirstOrDefault(x => x.ProductVariantAttribute.ProductAttribute.Name.ToUpper() == "RENK");
                                    var pvavDims = pvaValues.Where(x => x.ProductVariantAttribute.ProductAttribute.Name.ToUpper() == "BEDEN").ToList();

                                    if (pvavColor != null && pvavDims.Count == 0)
                                    {
                                        dimTypeCode = 1;
                                        colorNameTr = pvavColor.GetLocalized(pvav => pvav.Name, 2);
                                        colorNameEn = pvavColor.GetLocalized(pvav => pvav.Name, 1);
                                        colorCode = pvavColor.ProductAttributeOptionId.ToString();
                                    }
                                    else if (pvavColor != null && pvavDims.Count > 0)
                                    {
                                        var dim1 = pvavDims.First();
                                        dimTypeCode = 2;
                                        colorNameTr = pvavColor.GetLocalized(pvav => pvav.Name, 2);
                                        colorNameEn = pvavColor.GetLocalized(pvav => pvav.Name, 1);
                                        colorCode = pvavColor.ProductAttributeOptionId.ToString();
                                        dim1Code = dim1.Name;//not localized!
                                    }
                                    else//no color attribute
                                    {
                                        dimTypeCode = 2;
                                        colorNameTr = "";
                                        colorNameEn = "";
                                        var dim1 = pvavDims.FirstOrDefault();
                                        dim1Code = dim1 == null ? null : dim1.Name;//not localized!
                                        colorCode = dim1Code == null ? null : "STD";


                                    }
                                    this.WriteXlsxRowForNebim(worksheet, row, pv.Sku,eanCode, nameTr, nameEn, dimTypeCode.ToString(), colorCode, colorNameTr, colorNameEn, dim1Code, dim2Code, "AD", "TR", taxRate, taxRate, taxRate, categor1, categor2, categor3, manufacturerCode, manufacturerName, attribute2Code, attribute2Name, attribute3Code, attribute3Name, USD, TL, EURO, CHF);
                                    row++;
                                }

                            }
                        }
                        catch (Exception ex)
                        {
                            EngineContext.Current.Resolve<ILogger>().Error("NebimExport: productid='" + p.Id + "'   variantId:'" + pv.Id + "'", ex);
                        }

                        //else if (pvas.Any(x => x.ProductAttribute.Name == "RENK"))//only color attribute
                        //{
                        //    dimTypeCode = pvas.Count;
                        //    colorNameTr = pvas.FirstOrDefault(x => x.ProductAttribute.Name=="RENK").
                        //}
                        //else // add default color so have +1 attributes.
                        //{
                        //    colorCode = "STD";
                        //    dimTypeCode = pvas.Count + 1;
                        //} 
                    }
                }


                // we had better add some document properties to the spreadsheet 

                // set some core property values
                xlPackage.Workbook.Properties.Title = string.Format("{0} products", _storeInformationSettings.StoreName);
                xlPackage.Workbook.Properties.Author = _storeInformationSettings.StoreName;
                xlPackage.Workbook.Properties.Subject = string.Format("{0} products", _storeInformationSettings.StoreName);
                xlPackage.Workbook.Properties.Keywords = string.Format("{0} products", _storeInformationSettings.StoreName);
                xlPackage.Workbook.Properties.Category = "Products";
                xlPackage.Workbook.Properties.Comments = string.Format("{0} products", _storeInformationSettings.StoreName);

                // set some extended property values
                xlPackage.Workbook.Properties.Company = _storeInformationSettings.StoreName;
                xlPackage.Workbook.Properties.HyperlinkBase = new Uri(_storeInformationSettings.StoreUrl);

                // save the new spreadsheet
                xlPackage.Save();
            }
        }
        
        #region WriteXlsxRowForNebim
        private void WriteXlsxRowForNebim(ExcelWorksheet worksheet, int row,
            string productCode,
            string eanCode,
            string productNameTR,
            string productNameEN,
            string itemDimTypeCode,
            string colorCode,
            string colorNameTR,
            string colorCodeEN,
            string itemDim1Code,
            string itemDim2Code,
            string unitOfMeasureCode1,
            string countryCode,
            string purcVatRate,
            string sellVatRate,
            string itemAccountGrCode,
            string productHierarchyLevel01,
            string productHierarchyLevel02,
            string productHierarchyLevel03,
            string attributeCode01,
            string attributeCode01Name,
            string attributeCode02,
            string attributeCode02Name,
            string attributeCode03,
            string attributeCode03Name,
            decimal? USD,
            decimal? TL,
            decimal? EURO,
            decimal? CHF
            )
        {
            int col = 1;
            worksheet.Cells[row, col].Value = productCode; col++;
            worksheet.Cells[row, col].Value = eanCode; col++;
            worksheet.Cells[row, col].Value = productNameTR; col++;
            worksheet.Cells[row, col].Value = productNameEN; col++;
            worksheet.Cells[row, col].Value = itemDimTypeCode; col++;
            worksheet.Cells[row, col].Value = colorCode; col++;
            worksheet.Cells[row, col].Value = colorNameTR; col++;
            worksheet.Cells[row, col].Value = colorCodeEN; col++;
            worksheet.Cells[row, col].Value = itemDim1Code; col++;
            worksheet.Cells[row, col].Value = itemDim2Code; col++;
            worksheet.Cells[row, col].Value = unitOfMeasureCode1; col++;
            worksheet.Cells[row, col].Value = countryCode; col++;
            worksheet.Cells[row, col].Value = purcVatRate; col++;
            worksheet.Cells[row, col].Value = sellVatRate; col++;
            worksheet.Cells[row, col].Value = itemAccountGrCode; col++;
            worksheet.Cells[row, col].Value = productHierarchyLevel01; col++;
            worksheet.Cells[row, col].Value = productHierarchyLevel02; col++;
            worksheet.Cells[row, col].Value = productHierarchyLevel03; col++;
            worksheet.Cells[row, col].Value = attributeCode01; col++;
            worksheet.Cells[row, col].Value = attributeCode01Name; col++;
            worksheet.Cells[row, col].Value = attributeCode02; col++;
            worksheet.Cells[row, col].Value = attributeCode02Name; col++;
            worksheet.Cells[row, col].Value = attributeCode03; col++;
            worksheet.Cells[row, col].Value = attributeCode03Name; col++;
            worksheet.Cells[row, col].Value = USD; col++;
            worksheet.Cells[row, col].Value = TL; col++;
            worksheet.Cells[row, col].Value = EURO; col++;
            worksheet.Cells[row, col].Value = CHF; col++;
        }
        #endregion WriteXlsxRowForNebim


        #endregion

    }
}
