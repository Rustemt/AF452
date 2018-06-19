using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Caching;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain;
using Nop.Core.Domain.AFEntities;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Orders;
using Nop.Core.Infrastructure;
using Nop.Services.AFServices;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Tax;
using Nop.Services.Topics;
using Nop.Services.Seo;
using Nop.Web.Extensions;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Infrastructure;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Common;
using Nop.Web.Models.Media;
using Nop.Services.News;
using Nop.Services.Logging;
using Nop.Web.Models.News;
using Nop.Web.Infrastructure.Cache;
using System.Web.UI;
using Nop.Web.Models.ShoppingCart;
using Nop.Services.Configuration;
using AlternativeDataAccess;
using System.Web;
using System.IO;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Text;
using Nop.Web.Framework.Localization;

namespace Nop.Web.Controllers
{
	public class CatalogController : BaseNopController
	{

		#region Constants
		//caching by category, language, currency
		private const string CATEGORY_MODEL_BY_ID_KEY = "Nop.categorymodel.id-{0}.lgg-{1}.cur-{2}";
		private const string CATEGORY_MAIN_MODEL_BY_ID_KEY = "Nop.categorymainmodel.id-{0}.lgg-{1}.cur-{2}";
		private const string MANUFACTURER_MODEL_BY_ID_KEY = "Nop.manufacturermodel.id-{0}.lgg-{1}.cur-{2}";
		private const string RECENTLY_ADDED_MODEL_KEY = "Nop.recentlyaddedmodel.lgg-{0}.cur-{1}";

		private const string PRODUCT_MODEL_BY_ID_KEY = "Nop.productmodel.id-{0}.lgg-{1}.cur-{2}";
		private const string ALL_MANUFACTURERS_HOME_KEY = "Nop.Allmanufacturershome.lgg-{0}";
		private const string ALL_MANUFACTURERS = "Nop.Allmanufacturers.lgg-{0}";

		private const string PRODUCT_LIST_MODEL = "Nop.ProductListModel.{0}.{1}.{2}.{3}.{4}.{5}.{6}.{7}.{8}.{9}.{10}.cr-{11}";

        private const string RECENTLY_ADDED_PRODUCTS_HOME_PAGE = "Nop.RecentlyAddedProducts280HomePage.lgg-{0}.cur-{1}.{2}.{3}";
        private const string PREPARE_PRODUCT_OVERVIEW_MODELS = "Nop.PrepareProductOverviewModels280.lgg-{0}.cur-{1}.{2}.{3}";
		#endregion

		#region Fields

		private readonly ICategoryService _categoryService;
		private readonly IManufacturerService _manufacturerService;
		private readonly IProductService _productService;
		private readonly IProductAttributeService _productAttributeService;
		private readonly IProductAttributeParser _productAttributeParser;
		private readonly IWorkContext _workContext;
		private readonly ITaxService _taxService;
		private readonly ICurrencyService _currencyService;
		private readonly IPictureService _pictureService;
		private readonly ILocalizationService _localizationService;
		private readonly IPriceCalculationService _priceCalculationService;
		private readonly IPriceFormatter _priceFormatter;
		private readonly IWebHelper _webHelper;
		private readonly ISpecificationAttributeService _specificationAttributeService;
		private readonly ICustomerContentService _customerContentService;
		private readonly IDateTimeHelper _dateTimeHelper;
		private readonly IShoppingCartService _shoppingCartService;
		private readonly IRecentlyViewedProductsService _recentlyViewedProductsService;
		private readonly ICompareProductsService _compareProductsService;
		private readonly IWorkflowMessageService _workflowMessageService;
		private readonly IProductTagService _productTagService;
		private readonly IOrderReportService _orderReportService;
		private readonly ICustomerService _customerService;
		private readonly IPermissionService _permissionService;
		private readonly ISettingService _settingService;


		private readonly MediaSettings _mediaSetting;
		private readonly CatalogSettings _catalogSettings;
		private readonly ShoppingCartSettings _shoppingCartSettings;
		private readonly StoreInformationSettings _storeInformationSettings;
		private readonly LocalizationSettings _localizationSettings;
		private readonly CustomerSettings _customerSettings;
		private readonly IContentService _contentService;
		private readonly ICacheManager _cacheManager;
		private readonly ITopicService _topicService;

		private readonly INewsService _newsService;
		private readonly ICustomerActivityService _customerActivityService;
        private readonly ILogger _logger;

		private int currencyId;

		#endregion

		#region Constructors

		public CatalogController(ICategoryService categoryService,
			IManufacturerService manufacturerService, IProductService productService,
			IProductAttributeService productAttributeService, IProductAttributeParser productAttributeParser,
			IWorkContext workContext, ITaxService taxService, ICurrencyService currencyService,
			IPictureService pictureService, ILocalizationService localizationService,
			IPriceCalculationService priceCalculationService, IPriceFormatter priceFormatter,
			IWebHelper webHelper, ISpecificationAttributeService specificationAttributeService,
			ICustomerContentService customerContentService, IDateTimeHelper dateTimeHelper,
			IShoppingCartService shoppingCartService,
			IRecentlyViewedProductsService recentlyViewedProductsService, ICompareProductsService compareProductsService,
			IWorkflowMessageService workflowMessageService, IProductTagService productTagService,
			IOrderReportService orderReportService, ICustomerService customerService,
			MediaSettings mediaSetting, CatalogSettings catalogSettings,
			ShoppingCartSettings shoppingCartSettings, StoreInformationSettings storeInformationSettings,
			LocalizationSettings localizationSettings, CustomerSettings customerSettings,
			IPermissionService permissionService,
			IContentService contentService, ITopicService topicService, ICacheManager cacheManager, INewsService newsService,
			ICustomerActivityService customerActivityService, ISettingService settingService
            , ILogger logger)
		{
			this._categoryService = categoryService;
			this._manufacturerService = manufacturerService;
			this._productService = productService;
			this._productAttributeService = productAttributeService;
			this._productAttributeParser = productAttributeParser;
			this._workContext = workContext;
			this._taxService = taxService;
			this._currencyService = currencyService;
			this._pictureService = pictureService;
			this._localizationService = localizationService;
			this._priceCalculationService = priceCalculationService;
			this._priceFormatter = priceFormatter;
			this._webHelper = webHelper;
			this._specificationAttributeService = specificationAttributeService;
			this._customerContentService = customerContentService;
			this._dateTimeHelper = dateTimeHelper;
			this._shoppingCartService = shoppingCartService;
			this._recentlyViewedProductsService = recentlyViewedProductsService;
			this._compareProductsService = compareProductsService;
			this._workflowMessageService = workflowMessageService;
			this._productTagService = productTagService;
			this._orderReportService = orderReportService;
			this._customerService = customerService;
			this._permissionService = permissionService;

			this._mediaSetting = mediaSetting;
			this._catalogSettings = catalogSettings;
			this._shoppingCartSettings = shoppingCartSettings;
			this._storeInformationSettings = storeInformationSettings;
			this._localizationSettings = localizationSettings;
			this._customerSettings = customerSettings;
			this._contentService = contentService;
			this._cacheManager = EngineContext.Current.ContainerManager.Resolve<ICacheManager>("nop_cache_static");
			this._topicService = topicService;
			this._newsService = newsService;
			this._customerActivityService = customerActivityService;
			this._settingService = settingService;
			currencyId = _workContext.WorkingCurrency.Id;
            this._logger = logger;
		}

		#endregion

		#region Utilities
		//TODO: 280 cache
		[NonAction]
		protected List<int> GetChildCategoryIds(int parentCategoryId, bool showHidden = false)
		{
			//var customerRolesIds = _workContext.CurrentCustomer.CustomerRoles
			//    .Where(cr => cr.Active).Select(cr => cr.Id).ToList();
			//string cacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_CHILD_IDENTIFIERS_MODEL_KEY, parentCategoryId, showHidden, string.Join(",", customerRolesIds));
			//return _cacheManager.Get(cacheKey, () =>
			//{
			//    var categoriesIds = new List<int>();
			//    var categories = _categoryService.GetAllCategoriesByParentCategoryId(parentCategoryId, showHidden);
			//    foreach (var category in categories)
			//    {
			//        categoriesIds.Add(category.Id);
			//        categoriesIds.AddRange(GetChildCategoryIds(category.Id, showHidden));
			//    }
			//    return categoriesIds;
			//});

			var categoriesIds = new List<int>();
			var categories = _categoryService.GetAllCategoriesByParentCategoryId(parentCategoryId, showHidden);
			foreach (var category in categories)
			{
				categoriesIds.Add(category.Id);
				categoriesIds.AddRange(GetChildCategoryIds(category.Id, showHidden));
			}
			return categoriesIds;
		}

		[NonAction]
		private void SetCallForPrice(IList<ProductModel> model)
		{
			//Customer Quute
			var hasQouteCustomer = _customerService.GetProductVariantQuoteByCustomerId(_workContext.CurrentCustomer.Id);
			if (hasQouteCustomer.Count > 0)
			{
				foreach (var product in model)
				{
					foreach (ProductModel.ProductVariantModel item in product.ProductVariantModels)
					{
						var quote = hasQouteCustomer.Where(x => x.ProductVariantId == item.Id && x.RequestDate != null && x.ActivateDate != null).FirstOrDefault();
						if (quote != null && quote.ActivateDate != null && quote.RequestDate != null)
						{
							var pv = _productService.GetProductVariantById(item.Id);
							ProductModel.ProductVariantModel pivotmodel = new ProductModel.ProductVariantModel();
							pivotmodel = PrepareCategoryProductVariantModel(item, pv, product);

						}
					}
				}
			}
		}

		private void SetProductVariantPriceModel(IList<ProductModel> productModels)
		{
			//Customer Quute

			foreach (var product in productModels)
			{
				foreach (ProductModel.ProductVariantModel model in product.ProductVariantModels)
				{
					var productVariant = _productService.GetProductVariantById(model.Id);
					//model.Entity = productVariant;
					model.Stock = productVariant.StockQuantity;
					model.ProductVariantPrice.ProductVariantId = productVariant.Id;
					model.ProductVariantPrice.DynamicPriceUpdate = _catalogSettings.EnableDynamicPriceUpdate;
					if (_permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
					{
						model.ProductVariantPrice.HidePrices = false;
						if (productVariant.CustomerEntersPrice)
						{
							model.ProductVariantPrice.CustomerEntersPrice = true;
						}
						else
						{

							//model.ProductVariantPrice.CallForPrice = productVariant.CallForPrice;
							if (productVariant.CallForPrice)
							{
								model.ProductVariantPrice.CallForPrice = !productVariant.CallforPriceRequested(_workContext.CurrentCustomer);
								model.HidePriceIfCallforPrice = model.ProductVariantPrice.CallForPrice && !productVariant.DisplayPriceIfCallforPrice;
							}
							decimal taxRate = decimal.Zero;
							decimal oldPriceBase = _taxService.GetProductPrice(productVariant, productVariant.OldPrice, out taxRate);
							decimal finalPriceWithoutDiscountBase = _taxService.GetProductPrice(productVariant, _priceCalculationService.GetFinalPrice(productVariant, false), out taxRate);
							decimal finalPriceWithDiscountBase = _taxService.GetProductPrice(productVariant, _priceCalculationService.GetFinalPrice(productVariant, true, false), out taxRate);
							decimal oldPrice = _currencyService.ConvertFromPrimaryStoreCurrency(oldPriceBase, _workContext.WorkingCurrency);
							decimal finalPriceWithoutDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithoutDiscountBase, _workContext.WorkingCurrency);
							decimal finalPriceWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithDiscountBase, _workContext.WorkingCurrency);
							if (finalPriceWithoutDiscountBase != oldPriceBase && oldPriceBase > decimal.Zero)
								model.ProductVariantPrice.OldPrice = _priceFormatter.FormatPrice(oldPrice);
							model.ProductVariantPrice.Price = _priceFormatter.FormatPrice(finalPriceWithoutDiscount);
							if (finalPriceWithoutDiscountBase != finalPriceWithDiscountBase)
								model.ProductVariantPrice.PriceWithDiscount = _priceFormatter.FormatPrice(finalPriceWithDiscount);
							else
								model.ProductVariantPrice.PriceWithDiscount = null;
							model.ProductVariantPrice.PriceValue = finalPriceWithoutDiscount;
							model.ProductVariantPrice.PriceWithDiscountValue = finalPriceWithDiscount;
							model.ProductVariantPrice.Currency = CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol;

						}
					}
					else
					{
						model.ProductVariantPrice.HidePrices = true;
						model.ProductVariantPrice.OldPrice = null;
						model.ProductVariantPrice.Price = null;
					}



				}
			}

		}

		[NonAction]
		private ProductVariant GetMinimalPriceProductVariant(IList<ProductVariant> variants)
		{
			if (variants == null)
				throw new ArgumentNullException("variants");

			if (variants.Count == 0)
				return null;

			var tmp1 = variants.ToList();
			tmp1.Sort(new GenericComparer<ProductVariant>("Price", GenericComparer<ProductVariant>.SortOrder.Ascending));
			return tmp1[0];
		}

		[NonAction]
		private IList<Category> GetCategoryBreadCrumb(Category category)
		{
			if (category == null)
				throw new ArgumentNullException("category");

			var breadCrumb = new List<Category>();

			while (category != null && //category is not null
				!category.Deleted && //category is not deleted
				category.Published) //category is published
			{
				breadCrumb.Add(category);
				category = _categoryService.GetCategoryById(category.ParentCategoryId);
			}
			breadCrumb.Reverse();
			return breadCrumb;
		}

		[NonAction]
		private ProductModel.ProductPriceModel PrepareProductPriceModel(Product product)
		{
			if (product == null)
				throw new ArgumentNullException("product");

			var model = new ProductModel.ProductPriceModel();
			var productVariants = _productService.GetProductVariantsByProductId(product.Id);

			switch (productVariants.Count)
			{
				case 0:
					{
						//no variants
						model.OldPrice = null;
						model.Price = null;
					}
					break;
				case 1:
					{
						//only one variant
						var productVariant = productVariants[0];

						if (_permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
						{
							if (!productVariant.CustomerEntersPrice)
							{
								if (productVariant.CallForPrice && productVariant.Price == 0)
								{
									model.OldPrice = null;
									model.Price = _localizationService.GetResource("Products.Price.CallForPrice");
								}
								else
								{
									decimal taxRate = decimal.Zero;
									decimal oldPriceBase = _taxService.GetProductPrice(productVariant, productVariant.OldPrice, out taxRate);
									decimal finalPriceBase = _taxService.GetProductPrice(productVariant, _priceCalculationService.GetFinalPrice(productVariant, true, false), out taxRate);

									decimal oldPrice = _currencyService.ConvertFromPrimaryStoreCurrency(oldPriceBase, _workContext.WorkingCurrency);
									decimal finalPrice = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceBase, _workContext.WorkingCurrency);

									model.PriceValue = finalPrice;

									if (finalPriceBase != oldPriceBase && oldPriceBase != decimal.Zero)
									{
										model.OldPrice = _priceFormatter.FormatPrice(oldPrice);
										model.Price = _priceFormatter.FormatPrice(finalPrice);
									}
									else
									{
										model.OldPrice = null;
										model.Price = _priceFormatter.FormatPrice(finalPrice);
									}
								}
							}
						}
						else
						{
							model.OldPrice = null;
							model.Price = null;
						}
					}
					break;
				default:
					{
						//multiple variants
						var productVariant = GetMinimalPriceProductVariant(productVariants);
						if (productVariant != null)
						{
							if (_permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
							{
								if (!productVariant.CustomerEntersPrice)
								{
									if (productVariant.CallForPrice && productVariant.Price == 0)
									{
										model.OldPrice = null;
										model.Price = _localizationService.GetResource("Products.Price.CallForPrice");
									}
									else
									{
										decimal taxRate = decimal.Zero;
										decimal fromPriceBase = _taxService.GetProductPrice(productVariant, _priceCalculationService.GetFinalPrice(productVariant, false), out taxRate);
										decimal fromPrice = _currencyService.ConvertFromPrimaryStoreCurrency(fromPriceBase, _workContext.WorkingCurrency);

										model.OldPrice = null;
										model.Price = String.Format(_localizationService.GetResource("Products.PriceRangeFrom"), _priceFormatter.FormatPrice(fromPrice));
									}
								}
							}
							else
							{
								model.OldPrice = null;
								model.Price = null;

							}
						}
					}
					break;
			}

			//'add to cart' button
			switch (productVariants.Count)
			{
				case 0:
					{
						// no variants
						model.DisableBuyButton = true;
					}
					break;
				case 1:
					{

						//only one variant
						var productVariant = productVariants[0];
						model.CallForPrice = productVariant.CallForPrice;
						model.DisableBuyButton = productVariant.DisableBuyButton;
						if (!_permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
						{
							model.DisableBuyButton = true;
						}
					}
					break;
				default:
					{
						//multiple variants
						model.DisableBuyButton = true;
					}
					break;
			}

			return model;
		}

		[NonAction]
		private ProductModel PrepareProductOverviewModel(Product product, bool preparePriceModel = true, bool preparePictureModel = true, int pictureSize = 0)
		{
			if (product == null)
				throw new ArgumentNullException("product");

			var model = product.ToModel();
			//price
			if (preparePriceModel)
			{
				model.ProductPrice = PrepareProductPriceModel(product);
			}

			var pm = product.ProductManufacturers.FirstOrDefault();
			if (pm != null)
			{
				model.Manufacturer = pm.Manufacturer.GetLocalized(x => x.Name);
			}

			//picture
			if (preparePictureModel)
			{				
				if (pictureSize == 0)
					pictureSize = _mediaSetting.CartThumbPictureSize;

				var picture = product.GetProductPicture(_pictureService);
				if (picture != null)
					model.DefaultPictureModel.ImageUrl = _pictureService.GetPictureUrl(picture, pictureSize, true);
				else
					model.DefaultPictureModel.ImageUrl = _pictureService.GetDefaultPictureUrl(pictureSize);
				model.DefaultPictureModel.Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat"), model.Name);
				model.DefaultPictureModel.AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat"), model.Name);
			}
			return model;
		}

		[NonAction]
		private ProductModel PrepareProductDetailsPageModel(Product product, int variantId = 0)
		{
			if (product == null)
				throw new ArgumentNullException("product");

			var model = product.ToModel();

			var manufacturer = product.GetDefaultManufacturer();
			model.Manufacturer = manufacturer != null ? manufacturer.GetLocalized(x => x.Name) : null;
			model.ManufacturerId = manufacturer != null ? manufacturer.Id : 0;
			model.ManufacturerSeName = manufacturer != null ? manufacturer.GetSeName() : null;
			model.SeName = manufacturer != null ? manufacturer.GetLocalized(x => x.SeName) : null;
			var category = product.GetPublishDefaultProductCategory();
			if (category != null)
			{
				model.CategoryName = category.GetLocalized(x => x.Name);
				model.CategoryId = category.Id;
			}

			#region pictures
			model.DefaultPictureZoomEnabled = _mediaSetting.DefaultPictureZoomEnabled;
			var pictures = _pictureService.GetPicturesByProductId(product.Id);
			if (pictures.Count > 0)
			{
				foreach (var picture in pictures)
				{
					model.PictureModels.Add(new PictureModel()
					{
						ImageUrl = _pictureService.GetPictureUrl(picture, _mediaSetting.ProductDetailsPictureSize),
						FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
						Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat"), model.Name),
						AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat"), model.Name),

					});
				}
				model.DefaultPictureModel = model.PictureModels.FirstOrDefault();
			}
			else
			{
				//no images. set the default one
				model.DefaultPictureModel = new PictureModel()
				{
					ImageUrl = _pictureService.GetDefaultPictureUrl(_mediaSetting.ProductDetailsPictureSize),
					FullSizeImageUrl = _pictureService.GetDefaultPictureUrl(),
					Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat"), model.Name),
					AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat"), model.Name),
				};
			}
			#endregion pictures

			#region Specifications
			foreach (var psa in _specificationAttributeService.GetProductSpecificationAttributesByProductId(product.Id, null, null))
			{
				var attr = model.SpecificationAttributeModels.FirstOrDefault(x => x.SpecificationAttributeId == psa.SpecificationAttributeOption.SpecificationAttributeId);
				if (attr == null)
				{
					model.SpecificationAttributeModels.Add(new ProductSpecificationModel()
					{
						SpecificationAttributeId = psa.SpecificationAttributeOption.SpecificationAttributeId,
						SpecificationAttributeName = psa.SpecificationAttributeOption.SpecificationAttribute.GetLocalized(x => x.Name),
						SpecificationAttributeOption = psa.SpecificationAttributeOption.GetLocalized(x => x.Name),
						Visible = psa.ShowOnProductPage,
						Position = psa.SpecificationAttributeOption.DisplayOrder
					});
				}
				else
				{
					if (psa.ShowOnProductPage)
						if (attr.Visible)
							attr.SpecificationAttributeOption += ", " + psa.SpecificationAttributeOption.GetLocalized(x => x.Name);
						else
							attr.SpecificationAttributeOption = psa.SpecificationAttributeOption.GetLocalized(x => x.Name);
					attr.Visible |= psa.ShowOnProductPage;
				}
			}
			//////foreach (var psa in _specificationAttributeService.GetProductSpecificationAttributesByProductId(product.Id, null, null))
			//////{
			//////	var attr = model.SpecificationAttributeModels.FirstOrDefault(x => x.SpecificationAttributeId == psa.SpecificationAttributeOption.SpecificationAttributeId);
			//////	if (attr == null)
			//////	{
			//////		Specification sp = new Specification();
			//////		psa.SpecificationAttributeOption = sp.GetSpecificationAttributeOptionById(psa.SpecificationAttributeOption.Id);
			//////		//psa.SpecificationAttributeOption = _specificationAttributeService.GetSpecificationAttributeOptionById(psa.SpecificationAttributeOption.Id);
			//////		//psa.SpecificationAttributeOption.SpecificationAttribute = _specificationAttributeService.GetSpecificationAttributeById(psa.SpecificationAttributeOption.SpecificationAttribute.Id);
			//////		model.SpecificationAttributeModels.Add(new ProductSpecificationModel()
			//////		{
			//////			SpecificationAttributeId = psa.SpecificationAttributeOption.SpecificationAttributeId,
			//////			SpecificationAttributeName = sp.GetSpecificationAttributeById(psa.SpecificationAttributeOption.SpecificationAttributeId).GetLocalized(x => x.Name),
			//////			SpecificationAttributeOption = psa.SpecificationAttributeOption.GetLocalized(x => x.Name),
			//////			Visible = psa.ShowOnProductPage,
			//////			Position = psa.SpecificationAttributeOption.DisplayOrder
			//////		});
			//////	}
			//////	else
			//////	{
			//////		if (psa.ShowOnProductPage)
			//////			if (attr.Visible)
			//////				attr.SpecificationAttributeOption += ", " + psa.SpecificationAttributeOption.GetLocalized(x => x.Name);
			//////			else
			//////				attr.SpecificationAttributeOption = psa.SpecificationAttributeOption.GetLocalized(x => x.Name);
			//////		attr.Visible |= psa.ShowOnProductPage;
			//////	}
			//////}
			//model.SpecificationAttributeModels = _specificationAttributeService.GetProductSpecificationAttributesByProductId(product.Id, null, null)
			//   .Select(psa =>
			//   {
			//       return new ProductSpecificationModel()
			//       {
			//           SpecificationAttributeId = psa.SpecificationAttributeOption.SpecificationAttributeId,
			//           SpecificationAttributeName = psa.SpecificationAttributeOption.SpecificationAttribute.GetLocalized(x => x.Name),
			//           SpecificationAttributeOption = psa.SpecificationAttributeOption.GetLocalized(x => x.Name),
			//           Visible= psa.ShowOnProductPage
			//       };
			//   })
			//   .ToList();
			#endregion Specifications

			#region product variants
			foreach (var variant in _productService.GetProductVariantsByProductId(product.Id))
				model.ProductVariantModels.Add(PrepareProductVariantModel(new ProductModel.ProductVariantModel(), variant, model));
			if (model.ProductVariantModels.Count == 0)
				//throw new Exception("0 product variant");
				return null;

			model.DefaultVariantModel = model.ProductVariantModels.FirstOrDefault();


			if (variantId != 0)
			{
				var selectedVariantModel = model.ProductVariantModels.FirstOrDefault(x => x.Id == variantId);
				if (selectedVariantModel != null)
					model.DefaultVariantModel = selectedVariantModel;
			}
			#endregion product variants
			LoadProductAttributeSelectionModel(model);

			return model;
		}

		[NonAction]
		private void LoadProductAttributeSelectionModel(ProductModel model)
		{

			var majorAttribute = model.DefaultVariantModel.ProductVariantAttributes.FirstOrDefault(x => x.Values.Count == 1);
			int majorAttributeId = majorAttribute == null ? 0 : majorAttribute.ProductAttributeId;
			//There are variants but there is no major attribute; do nothing about attributes
			if (majorAttributeId == 0 && model.ProductVariantModels.Count > 1) return;

			List<AttributeModel> attributeModels = new List<AttributeModel>();

			List<int> attributeValueCombination = null;
			foreach (var variantModel in model.ProductVariantModels)
			{
				ProductVariant variant = _productService.GetProductVariantById(variantModel.Id);
				majorAttribute = variantModel.ProductVariantAttributes.FirstOrDefault(x => x.ProductAttributeId == majorAttributeId);
				//if variant-identifier attribute is not the first element, we take it to the top of the list
				if (majorAttribute != null && variantModel.ProductVariantAttributes.IndexOf(majorAttribute) != 0)
				{
					variantModel.ProductVariantAttributes.Remove(majorAttribute);
					variantModel.ProductVariantAttributes.Insert(0, majorAttribute);
				}
				AttributeModel attributeModel = null;
				AttributeValueModel attributeValueModel = null;
				IList<IList<int>> attributeValueIdCombinations = new List<IList<int>>();
				IList<int> attributeValueIds = null;
				int parentValueId = 0;


				#region stock management via attribute combinations
				if (variant.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes)
				{

					foreach (var combination in variant.ProductVariantAttributeCombinations)
					{
						if (variant.BackorderMode == BackorderMode.NoBackorders && combination.StockQuantity < 1) continue;
						IList<int> attributeIds = _productAttributeParser.ParseProductVariantAttributeIds(combination.AttributesXml);
						attributeValueCombination = new List<int>();
						attributeValueModel = null;
						foreach (int attributeId in attributeIds)
						{
							parentValueId = attributeValueModel == null ? 0 : attributeValueModel.ProductVariantAttributeValueId;
							if (parentValueId == 0)
							{
								attributeModel = attributeModels.FirstOrDefault(x => x.ParentProductVariantAttributeValueId == 0);
							}
							else
							{
								attributeModel = attributeModels.FirstOrDefault(x => x.ParentProductVariantAttributeValueId == attributeValueModel.ProductVariantAttributeValueId);
							}

							var productVariantAttributeModel = variantModel.ProductVariantAttributes.FirstOrDefault(x => x.Id == attributeId);
							if (attributeModel == null && productVariantAttributeModel != null)
							{
								attributeModel = new AttributeModel(productVariantAttributeModel, parentValueId);
								attributeModels.Add(attributeModel);
							}

							int valueId = int.Parse(_productAttributeParser.ParseValues(combination.AttributesXml, attributeId)[0]);

							attributeValueModel = attributeModel.ValueModels.FirstOrDefault(x => x.ProductVariantAttributeValueId == valueId);
							if (attributeValueModel == null)
							{
								var productvariantAttributeValueModel = productVariantAttributeModel.Values.FirstOrDefault(x => x.Id == valueId);
								if (productvariantAttributeValueModel != null)
								{
									attributeValueModel = new AttributeValueModel(productvariantAttributeValueModel, variant.Id, combination.StockQuantity);
									attributeModel.ValueModels.Add(attributeValueModel);
								}
							}
							attributeValueCombination.Add(attributeValueModel.ProductAttributeOptionId);

						}
						attributeValueIdCombinations.Add(attributeValueCombination);

					}
					variantModel.AttributeValueIdCombinations = attributeValueIdCombinations;
				}

				#endregion stock management via attribute combinations

				#region stock management via variant
				else if (variant.ManageInventoryMethod == ManageInventoryMethod.ManageStock || variant.ManageInventoryMethod == ManageInventoryMethod.DontManageStock)
				{
					attributeValueIds = new List<int>();
					foreach (var productVariantAttributeModel in variantModel.ProductVariantAttributes)
					{
						variant = _productService.GetProductVariantById(variantModel.Id);
						if (parentValueId == 0)
						{
							attributeModel = attributeModels.FirstOrDefault(x => x.AttributeId == productVariantAttributeModel.ProductAttributeId);
						}
						else
						{
							attributeModel = attributeModels.FirstOrDefault(x => x.ParentProductVariantAttributeValueId == parentValueId);
						}
						if (attributeModel == null)
						{
							attributeModel = new AttributeModel(productVariantAttributeModel, parentValueId);
							attributeModels.Add(attributeModel);
						}
						foreach (var productVariantAttributevalueModel in productVariantAttributeModel.Values)
						{
							attributeValueModel = attributeModel.ValueModels.FirstOrDefault(x => x.ProductAttributeOptionId == productVariantAttributevalueModel.ProductAttributeOptionId);
							if (attributeValueModel == null)
							{
								attributeValueModel = new AttributeValueModel(productVariantAttributevalueModel, variant.Id);
								attributeModel.ValueModels.Add(attributeValueModel);

								if (attributeModel.AttributeId == majorAttributeId)
									parentValueId = attributeValueModel.ProductVariantAttributeValueId;
							}
							attributeValueIds.Add(attributeValueModel.ProductAttributeOptionId);
						}

					}
					variantModel.AttributeValueIds = attributeValueIds;

				}

				#endregion stock management via variant


			}
			//if attributemodel does not have any value remove it from the list.
			attributeModels.RemoveAll(x => x.ValueModels.Count == 0);

			model.AttributeSelectionModel = attributeModels;

			model.IsGuest = _workContext.CurrentCustomer.IsGuest();

		}


		[NonAction]
		private ProductModel PrepareProductOverviewPageModel(Product product)
		{
			if (product == null)
				throw new ArgumentNullException("product");
			var model = product.ToModel(true);

			#region picture
			var picture = product.GetProductPicture(_pictureService);
			if (picture != null)
			{
				model.DefaultPictureModel.ImageUrl = _pictureService.GetPictureUrl(picture, _mediaSetting.ProductThumbPictureSize, true);
				model.DefaultPictureModel.LargeSizeImageUrl = _pictureService.GetPictureUrl(picture, _mediaSetting.CategoryThumbPictureSize, true);

			}
			else
			{
				model.DefaultPictureModel.ImageUrl = _pictureService.GetDefaultPictureUrl(_mediaSetting.ProductThumbPictureSize);
				model.DefaultPictureModel.ImageUrl = _pictureService.GetDefaultPictureUrl(_mediaSetting.CategoryThumbPictureSize);
			}
			model.DefaultPictureModel.Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat"), model.Name);
			model.DefaultPictureModel.AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat"), model.Name);


			#endregion picture
			#region Specifications

			model.SpecificationAttributeModels = _specificationAttributeService.GetProductSpecificationAttributesByProductId(product.Id, true, null)
			   .Select(psa =>
			   {
				   return new ProductSpecificationModel()
				   {
					   SpecificationAttributeId = psa.SpecificationAttributeOption.SpecificationAttributeId,
					   SpecificationAttributeName = psa.SpecificationAttributeOption.SpecificationAttribute.GetLocalized(x => x.Name),
					   SpecificationAttributeOption = psa.SpecificationAttributeOption.GetLocalized(x => x.Name),
					   SpecificationAttributeOptionId = psa.SpecificationAttributeOptionId,
					   Position = psa.SpecificationAttributeOption.DisplayOrder
				   };
			   })
			   .ToList();
			#endregion Specifications
			#region product variants
			foreach (var variant in _productService.GetProductVariantsByProductId(product.Id))
				model.ProductVariantModels.Add(PrepareCategoryProductVariantModel(new ProductModel.ProductVariantModel(), variant, model));
			model.DefaultVariantModel = model.ProductVariantModels.FirstOrDefault();
			#endregion product variants

			LoadProductAttributeSelectionModel(model);


			return model;
		}

		/*
		[NonAction]
		private ProductModel PrepareNewsItemproductsOverviewPageModel(Product product)
		{
			if (product == null)
				throw new ArgumentNullException("product");
			var model = product.ToModel(true);
			model.Manufacturer = product.ProductManufacturers.FirstOrDefault() != null ? product.ProductManufacturers.FirstOrDefault().Manufacturer.GetLocalized(x => x.Name) : null;


			#region picture
			var picture = product.GetProductPicture(_pictureService);
			if (picture != null)
			{
				model.DefaultPictureModel.ImageUrl = _pictureService.GetPictureUrl(picture, _mediaSetting.ProductThumbPictureSize, true);
				model.DefaultPictureModel.LargeSizeImageUrl = _pictureService.GetPictureUrl(picture, _mediaSetting.CategoryThumbPictureSize, true);

			}
			else
			{
				model.DefaultPictureModel.ImageUrl = _pictureService.GetDefaultPictureUrl(_mediaSetting.ProductThumbPictureSize);
				model.DefaultPictureModel.ImageUrl = _pictureService.GetDefaultPictureUrl(_mediaSetting.CategoryThumbPictureSize);
			}
			model.DefaultPictureModel.Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat"), model.Name);
			model.DefaultPictureModel.AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat"), model.Name);


			#endregion picture
			#region Specifications
			model.SpecificationAttributeModels = _specificationAttributeService.GetProductSpecificationAttributesByProductId(product.Id, true, null)
			   .Select(psa =>
			   {
				   return new ProductSpecificationModel()
				   {
					   SpecificationAttributeId = psa.SpecificationAttributeOption.SpecificationAttributeId,
					   SpecificationAttributeName = psa.SpecificationAttributeOption.SpecificationAttribute.GetLocalized(x => x.Name),
					   SpecificationAttributeOption = psa.SpecificationAttributeOption.GetLocalized(x => x.Name),
					   SpecificationAttributeOptionId = psa.SpecificationAttributeOptionId,
					   Position = psa.SpecificationAttributeOption.DisplayOrder
				   };
			   })
			   .ToList();
			#endregion Specifications
			#region product variants
			foreach (var variant in _productService.GetProductVariantsByProductId(product.Id))
				model.ProductVariantModels.Add(PrepareCategoryProductVariantModel(new ProductModel.ProductVariantModel(), variant, model));
			model.DefaultVariantModel = model.ProductVariantModels.FirstOrDefault();
			#endregion product variants

			LoadProductAttributeSelectionModel(model);


			return model;
		}
		//[NonAction]
		//private ProductModel PrepareProductOverveiwPageModel(Product product, int languageId)
		//{
		//    if (product == null)
		//        throw new ArgumentNullException("product");
		//    var model = product.ToModel(languageId);
		//    var manufacturer = product.GetDefaultManufacturer();
		//    model.Manufacturer = manufacturer != null ? manufacturer.GetLocalized(x => x.Name, languageId):"";

		//    #region picture
		//    var picture = product.GetProductPicture(_pictureService);
		//    if (picture != null)
		//    {
		//        model.DefaultPictureModel.ImageUrl = _pictureService.GetPictureUrl(picture, _mediaSetting.ProductThumbPictureSize, true);
		//        model.DefaultPictureModel.LargeSizeImageUrl = _pictureService.GetPictureUrl(picture, _mediaSetting.CategoryThumbPictureSize, true);

		//    }
		//    else
		//    {
		//        model.DefaultPictureModel.ImageUrl = _pictureService.GetDefaultPictureUrl(_mediaSetting.ProductThumbPictureSize);
		//        model.DefaultPictureModel.ImageUrl = _pictureService.GetDefaultPictureUrl(_mediaSetting.CategoryThumbPictureSize);
		//    }
		//    model.DefaultPictureModel.Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat", languageId), model.Name);
		//    model.DefaultPictureModel.AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat",languageId), model.Name);


		//    #endregion picture
		//    #region Specifications
		//    model.SpecificationAttributeModels = _specificationAttributeService.GetProductSpecificationAttributesByProductId(product.Id, true, null)
		//       .Select(psa =>
		//       {
		//           return new ProductSpecificationModel()
		//           {
		//               SpecificationAttributeId = psa.SpecificationAttributeOption.SpecificationAttributeId,
		//               SpecificationAttributeName = psa.SpecificationAttributeOption.SpecificationAttribute.GetLocalized(x => x.Name, languageId),
		//               SpecificationAttributeOption = psa.SpecificationAttributeOption.GetLocalized(x => x.Name, languageId),
		//               SpecificationAttributeOptionId = psa.SpecificationAttributeOptionId
		//           };
		//       })
		//       .ToList();
		//    #endregion Specifications
		//    #region product variants
		//    foreach (var variant in _productService.GetProductVariantsByProductId(product.Id))
		//        model.ProductVariantModels.Add(PrepareCategoryProductVariantModel(new ProductModel.ProductVariantModel(), variant, model, languageId));
		//    model.DefaultVariantModel = model.ProductVariantModels.FirstOrDefault();
		//    #endregion product variants

		//    LoadProductAttributeSelectionModel(model);


		//    return model;
		//}

		*/

		[NonAction]
		private void PrepareProductReviewsModel(ProductReviewsModel model, Product product)
		{
			if (product == null)
				throw new ArgumentNullException("product");

			if (model == null)
				throw new ArgumentNullException("model");

			model.ProductId = product.Id;
			model.ProductName = product.GetLocalized(x => x.Name);
			model.ProductSeName = product.GetSeName();

			var productReviews = product.ProductReviews.Where(pr => pr.IsApproved).OrderBy(pr => pr.CreatedOnUtc);
			foreach (var pr in productReviews)
			{
				model.Items.Add(new ProductReviewModel()
				{
					Id = pr.Id,
					CustomerId = pr.CustomerId,
					CustomerName = pr.Customer.FormatUserName(),
					AllowViewingProfiles = _customerSettings.AllowViewingProfiles && pr.Customer != null && !pr.Customer.IsGuest(),
					Title = pr.Title,
					ReviewText = pr.ReviewText,
					Rating = pr.Rating,
					Helpfulness = new ProductReviewHelpfulnessModel()
					{
						ProductReviewId = pr.Id,
						HelpfulYesTotal = pr.HelpfulYesTotal,
						HelpfulNoTotal = pr.HelpfulNoTotal,
					},
					WrittenOnStr = _dateTimeHelper.ConvertToUserTime(pr.CreatedOnUtc, DateTimeKind.Utc).ToString("g"),
				});
			}
		}

		//AF
		[NonAction]
		private ProductModel.ProductVariantModel PrepareProductVariantModel(ProductModel.ProductVariantModel model, ProductVariant productVariant, ProductModel productModel)
		{
			if (productVariant == null)
				throw new ArgumentNullException("productVariant");

			if (model == null)
				throw new ArgumentNullException("model");

			#region Properties
			//model.Entity = productVariant;
			model.Id = productVariant.Id;
			model.Name = productVariant.GetLocalized(x => x.Name);
			if (string.IsNullOrWhiteSpace(model.Name)) model.Name = productVariant.Product.GetLocalized(x => x.Name);
			model.ShowSku = _catalogSettings.ShowProductSku;
			model.Sku = productVariant.Sku;
			model.Description = productVariant.GetLocalized(x => x.Description);
			if (string.IsNullOrWhiteSpace(model.Description))
			{
				model.Description = productModel.ShortDescription;
			}
			model.ShowManufacturerPartNumber = _catalogSettings.ShowManufacturerPartNumber;
			model.ManufacturerPartNumber = productVariant.ManufacturerPartNumber;
			model.StockAvailablity = productVariant.FormatStockMessage(_localizationService);
			model.Stock = productVariant.StockQuantity;
			model.OrderMaximumQuantity = productVariant.OrderMaximumQuantity;
			model.HideDiscount = productVariant.HideDiscount;
			model.ManageInventoryMethodId = (int)productVariant.ManageInventoryMethod;
			if (productVariant.IsDownload && productVariant.HasSampleDownload)
			{
				model.DownloadSampleUrl = Url.Action("Sample", "Download", new { productVariantId = productVariant.Id });
			}
			#endregion

			#region Product variant price
			model.ProductVariantPrice.ProductVariantId = productVariant.Id;
			model.ProductVariantPrice.DynamicPriceUpdate = _catalogSettings.EnableDynamicPriceUpdate;
			if (_permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
			{
				model.ProductVariantPrice.HidePrices = false;
				if (productVariant.CustomerEntersPrice)
				{
					model.ProductVariantPrice.CustomerEntersPrice = true;
				}
				else
				{
					if (productVariant.CallForPrice)
					{
						model.ProductVariantPrice.CallForPrice = !productVariant.CallforPriceRequested(_workContext.CurrentCustomer);
						model.HidePriceIfCallforPrice = model.ProductVariantPrice.CallForPrice && !productVariant.DisplayPriceIfCallforPrice;
					}

					decimal taxRate = decimal.Zero;
					decimal oldPriceBase = _taxService.GetProductPrice(productVariant, productVariant.OldPrice, out taxRate);
					decimal finalPriceWithoutDiscountBase = _taxService.GetProductPrice(productVariant, _priceCalculationService.GetFinalPrice(productVariant, false), out taxRate);
					decimal finalPriceWithDiscountBase = _taxService.GetProductPrice(productVariant, _priceCalculationService.GetFinalPrice(productVariant, true, false), out taxRate);

					decimal oldPrice = _currencyService.ConvertFromPrimaryStoreCurrency(oldPriceBase, _workContext.WorkingCurrency);
					decimal finalPriceWithoutDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithoutDiscountBase, _workContext.WorkingCurrency);
					decimal finalPriceWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithDiscountBase, _workContext.WorkingCurrency);


					if (finalPriceWithoutDiscountBase != oldPriceBase && oldPriceBase > decimal.Zero)
						model.ProductVariantPrice.OldPrice = _priceFormatter.FormatPrice(oldPrice, false, false);

					model.ProductVariantPrice.Price = _priceFormatter.FormatPrice(finalPriceWithoutDiscount, false, false);

					if (finalPriceWithoutDiscountBase != finalPriceWithDiscountBase)
					{
						IList<Discount> discounts = null;
						model.ProductVariantPrice.PriceWithDiscount = _priceFormatter.FormatPrice(finalPriceWithDiscount, false, false);
						decimal discountValueBase = _priceCalculationService.GetDiscountAmount(productVariant, _workContext.CurrentCustomer, 0, out discounts, false);
						model.ProductVariantPrice.DiscountValue = _currencyService.ConvertFromPrimaryStoreCurrency(discountValueBase, _workContext.WorkingCurrency);
						model.ProductVariantPrice.DiscountPrice = _priceFormatter.FormatPrice(model.ProductVariantPrice.DiscountValue, false, false);

						decimal finalpercentage = 0;
						var discount = discounts.First();
						if (discounts.Count > 1 || !discount.UsePercentage)
						{
							finalpercentage = ((finalPriceWithoutDiscount - finalPriceWithDiscount) / finalPriceWithoutDiscount) * 100;
						}
						else
						{
							finalpercentage = discount.DiscountPercentage;
						}
						if (string.Equals(_workContext.WorkingLanguage.LanguageCulture, "en-US", StringComparison.OrdinalIgnoreCase))
							model.ProductVariantPrice.DiscountPercentage = String.Format("({0}%)", ((int)finalpercentage).ToString());
						else
							model.ProductVariantPrice.DiscountPercentage = String.Format("(%{0})", ((int)finalpercentage).ToString());

					}
					model.ProductVariantPrice.PriceValue = finalPriceWithoutDiscount;
					model.ProductVariantPrice.PriceWithDiscountValue = finalPriceWithDiscount;

					model.ProductVariantPrice.Currency = CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol;
				}
			}
			else
			{
				model.ProductVariantPrice.HidePrices = true;
				model.ProductVariantPrice.OldPrice = null;
				model.ProductVariantPrice.Price = null;
			}
			#endregion

			#region 'Add to cart' model

			model.AddToCart.ProductVariantId = productVariant.Id;

			//quantity
			model.AddToCart.EnteredQuantity = productVariant.OrderMinimumQuantity;

			//'add to cart', 'add to wishlist' buttons
			if (productVariant.DisableBuyButton)
			{
				model.AddToCart.DisableBuyButton = true;
				model.AddToCart.DisableWishlistButton = true;
			}
			if (!_permissionService.Authorize(StandardPermissionProvider.EnableWishlist))
			{
				model.AddToCart.DisableWishlistButton = true;
			}
			if (!_permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
			{
				model.AddToCart.DisableBuyButton = true;
				model.AddToCart.DisableWishlistButton = true;
			}
			model.AddToCart.CustomerEntersPrice = productVariant.CustomerEntersPrice;
			if (model.AddToCart.CustomerEntersPrice)
			{
				decimal minimumCustomerEnteredPrice = _currencyService.ConvertFromPrimaryStoreCurrency(productVariant.MinimumCustomerEnteredPrice, _workContext.WorkingCurrency);
				decimal maximumCustomerEnteredPrice = _currencyService.ConvertFromPrimaryStoreCurrency(productVariant.MaximumCustomerEnteredPrice, _workContext.WorkingCurrency);

				model.AddToCart.CustomerEnteredPrice = minimumCustomerEnteredPrice;
				model.AddToCart.CustomerEnteredPriceRange = string.Format(_localizationService.GetResource("Products.EnterProductPrice.Range"),
					_priceFormatter.FormatPrice(minimumCustomerEnteredPrice, false, false),
					_priceFormatter.FormatPrice(maximumCustomerEnteredPrice, false, false));
			}

			#endregion

			#region Gift card

			model.GiftCard.IsGiftCard = productVariant.IsGiftCard;
			if (model.GiftCard.IsGiftCard)
			{
				model.GiftCard.GiftCardType = productVariant.GiftCardType;
				model.GiftCard.SenderName = _workContext.CurrentCustomer.GetFullName();
				model.GiftCard.SenderEmail = _workContext.CurrentCustomer.Email;
			}

			#endregion

			#region Product attributes

			var productVariantAttributes = _productAttributeService.GetProductVariantAttributesByProductVariantId(productVariant.Id);
			foreach (var attribute in productVariantAttributes)
			{
				var pvaModel = new ProductModel.ProductVariantModel.ProductVariantAttributeModel()
				{
					Id = attribute.Id,
					ProductVariantId = productVariant.Id,
					ProductAttributeId = attribute.ProductAttributeId,
					Name = attribute.ProductAttribute.GetLocalized(x => x.Name),
					Description = attribute.ProductAttribute.GetLocalized(x => x.Description),
					TextPrompt = attribute.ProductAttribute.GetLocalized(x => x.Name),
					IsRequired = attribute.IsRequired,
					AttributeControlType = attribute.AttributeControlType,
				};

				if (attribute.ShouldHaveValues())
				{
					//values
					var pvaValues = _productAttributeService.GetProductVariantAttributeValues(attribute.Id);
					foreach (var pvaValue in pvaValues)
					{
						var pvaValueModel = new ProductModel.ProductVariantModel.ProductVariantAttributeValueModel()
						{
							Id = pvaValue.Id,
							Name = pvaValue.GetLocalized(x => x.Name),
							ProductAttributeOptionId = pvaValue.ProductAttributeOptionId,
							IsPreSelected = pvaValue.IsPreSelected,
						};
						pvaModel.Values.Add(pvaValueModel);

						//display price if allowed
						if (_permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
						{
							decimal taxRate = decimal.Zero;
							decimal priceAdjustmentBase = _taxService.GetProductPrice(productVariant, pvaValue.PriceAdjustment, out taxRate);
							decimal priceAdjustment = _currencyService.ConvertFromPrimaryStoreCurrency(priceAdjustmentBase, _workContext.WorkingCurrency);
							if (priceAdjustmentBase > decimal.Zero)
								pvaValueModel.PriceAdjustment = "+" + _priceFormatter.FormatPrice(priceAdjustment, false, false);
							else if (priceAdjustmentBase < decimal.Zero)
								pvaValueModel.PriceAdjustment = "-" + _priceFormatter.FormatPrice(-priceAdjustment, false, false);

							pvaValueModel.PriceAdjustmentValue = priceAdjustment;
						}
					}
				}

				model.ProductVariantAttributes.Add(pvaModel);
			}



			#endregion

			#region Pictures

			var pictures = _pictureService.GetPicturesByProductVariantId(productVariant.Id);
			if (pictures.Count > 0)
			{
				//default picture

				//model.DefaultPictureModel = new PictureModel()
				//{
				//    ImageUrl = _pictureService.GetPictureUrl(pictures.FirstOrDefault(), _mediaSetting.ProductDetailsPictureSize),
				//    FullSizeImageUrl = _pictureService.GetPictureUrl(pictures.FirstOrDefault()),
				//    Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat"), model.Name),
				//    AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat"), model.Name),
				//};

				//all pictures
				foreach (var picture in pictures)
				{
					model.PictureModels.Add(new PictureModel()
					{
						ImageUrl = _pictureService.GetPictureUrl(picture, _mediaSetting.ProductDetailsPictureSize),
						FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
						Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat"), model.Name),
						AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat"), model.Name),

					});
				}
				model.DefaultPictureModel = model.PictureModels.FirstOrDefault();
			}
			else if (productModel.PictureModels.Count > 0)
			{
				model.DefaultPictureModel = productModel.DefaultPictureModel;
				model.PictureModels = productModel.PictureModels;
			}
			else
			{
				//no images. set the default one
				model.DefaultPictureModel = new PictureModel()
				{
					ImageUrl = _pictureService.GetDefaultPictureUrl(_mediaSetting.ProductDetailsPictureSize),
					FullSizeImageUrl = _pictureService.GetDefaultPictureUrl(),
					Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat"), model.Name),
					AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat"), model.Name),
				};
			}


			#endregion Pictures

			return model;
		}

		private ProductModel.ProductVariantModel PrepareCategoryProductVariantModel(ProductModel.ProductVariantModel model, ProductVariant productVariant, ProductModel productModel)
		{
			if (productVariant == null)
				throw new ArgumentNullException("productVariant");

			if (model == null)
				throw new ArgumentNullException("model");



			#region Properties
			//model.Entity = productVariant;
			model.Name = productVariant.GetLocalized(x => x.Name);
			model.Id = productVariant.Id;
			if (string.IsNullOrWhiteSpace(model.Name)) model.Name = productModel.Name;
			model.ShowSku = _catalogSettings.ShowProductSku;
			model.Sku = productVariant.Sku;
			model.Description = productVariant.GetLocalized(x => x.Description);
			if (string.IsNullOrWhiteSpace(model.Description))
			{
				model.Description = productModel.ShortDescription;
			}
			model.ShowManufacturerPartNumber = _catalogSettings.ShowManufacturerPartNumber;
			model.ManufacturerPartNumber = productVariant.ManufacturerPartNumber;
			model.StockAvailablity = productVariant.FormatStockMessage(_localizationService);
			model.Stock = productVariant.StockQuantity;
			model.OrderMaximumQuantity = productVariant.OrderMaximumQuantity;
			model.HideDiscount = productVariant.HideDiscount;
			model.ManageInventoryMethodId = (int)productVariant.ManageInventoryMethod;
			if (productVariant.IsDownload && productVariant.HasSampleDownload)
			{
				model.DownloadSampleUrl = Url.Action("Sample", "Download", new { productVariantId = productVariant.Id });
			}
			#endregion

			#region Product variant price
			model.ProductVariantPrice.ProductVariantId = productVariant.Id;
			model.ProductVariantPrice.DynamicPriceUpdate = _catalogSettings.EnableDynamicPriceUpdate;
			if (_permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
			{
				model.ProductVariantPrice.HidePrices = false;
				if (productVariant.CustomerEntersPrice)
				{
					model.ProductVariantPrice.CustomerEntersPrice = true;
				}
				else
				{

					//model.ProductVariantPrice.CallForPrice = productVariant.CallForPrice;
					if (productVariant.CallForPrice)
					{
						model.ProductVariantPrice.CallForPrice = !productVariant.CallforPriceRequested(_workContext.CurrentCustomer);
						model.HidePriceIfCallforPrice = model.ProductVariantPrice.CallForPrice && !productVariant.DisplayPriceIfCallforPrice;
					}
					decimal taxRate = decimal.Zero;
					decimal oldPriceBase = _taxService.GetProductPrice(productVariant, productVariant.OldPrice, out taxRate);
					decimal finalPriceWithoutDiscountBase = _taxService.GetProductPrice(productVariant, _priceCalculationService.GetFinalPrice(productVariant, false), out taxRate);
					decimal finalPriceWithDiscountBase = _taxService.GetProductPrice(productVariant, _priceCalculationService.GetFinalPrice(productVariant, true, false), out taxRate);
					decimal oldPrice = _currencyService.ConvertFromPrimaryStoreCurrency(oldPriceBase, _workContext.WorkingCurrency);
					decimal finalPriceWithoutDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithoutDiscountBase, _workContext.WorkingCurrency);
					decimal finalPriceWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithDiscountBase, _workContext.WorkingCurrency);
					if (finalPriceWithoutDiscountBase != oldPriceBase && oldPriceBase > decimal.Zero)
						model.ProductVariantPrice.OldPrice = _priceFormatter.FormatPrice(oldPrice);


					model.ProductVariantPrice.Price = _priceFormatter.FormatPrice(finalPriceWithoutDiscount);
					if (finalPriceWithoutDiscountBase != finalPriceWithDiscountBase)
						model.ProductVariantPrice.PriceWithDiscount = _priceFormatter.FormatPrice(finalPriceWithDiscount);
					else
						model.ProductVariantPrice.PriceWithDiscount = null;
					model.ProductVariantPrice.PriceValue = finalPriceWithoutDiscount;
					model.ProductVariantPrice.PriceWithDiscountValue = finalPriceWithDiscount;
					model.ProductVariantPrice.Currency = CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol;

				}
			}
			else
			{
				model.ProductVariantPrice.HidePrices = true;
				model.ProductVariantPrice.OldPrice = null;
				model.ProductVariantPrice.Price = null;
			}
			#endregion

			#region 'Add to cart' model

			model.AddToCart.ProductVariantId = productVariant.Id;

			//quantity
			model.AddToCart.EnteredQuantity = productVariant.OrderMinimumQuantity;

			//'add to cart', 'add to wishlist' buttons
			if (productVariant.DisableBuyButton)
			{
				model.AddToCart.DisableBuyButton = true;
				model.AddToCart.DisableWishlistButton = true;
			}
			if (!_permissionService.Authorize(StandardPermissionProvider.EnableWishlist))
			{
				model.AddToCart.DisableWishlistButton = true;
			}
			if (!_permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
			{
				model.AddToCart.DisableBuyButton = true;
				model.AddToCart.DisableWishlistButton = true;
			}
			model.AddToCart.CustomerEntersPrice = productVariant.CustomerEntersPrice;
			if (model.AddToCart.CustomerEntersPrice)
			{
				decimal minimumCustomerEnteredPrice = _currencyService.ConvertFromPrimaryStoreCurrency(productVariant.MinimumCustomerEnteredPrice, _workContext.WorkingCurrency);
				decimal maximumCustomerEnteredPrice = _currencyService.ConvertFromPrimaryStoreCurrency(productVariant.MaximumCustomerEnteredPrice, _workContext.WorkingCurrency);

				model.AddToCart.CustomerEnteredPrice = minimumCustomerEnteredPrice;
				model.AddToCart.CustomerEnteredPriceRange = string.Format(_localizationService.GetResource("Products.EnterProductPrice.Range"),
					_priceFormatter.FormatPrice(minimumCustomerEnteredPrice, false, false),
					_priceFormatter.FormatPrice(maximumCustomerEnteredPrice, false, false));
			}

			#endregion

			#region Gift card

			model.GiftCard.IsGiftCard = productVariant.IsGiftCard;
			if (model.GiftCard.IsGiftCard)
			{
				model.GiftCard.GiftCardType = productVariant.GiftCardType;
				model.GiftCard.SenderName = _workContext.CurrentCustomer.GetFullName();
				model.GiftCard.SenderEmail = _workContext.CurrentCustomer.Email;
			}

			#endregion

			#region Product attributes

			var productVariantAttributes = _productAttributeService.GetProductVariantAttributesByProductVariantId(productVariant.Id);
			foreach (var attribute in productVariantAttributes)
			{
				var pvaModel = new ProductModel.ProductVariantModel.ProductVariantAttributeModel()
				{
					Id = attribute.Id,
					ProductVariantId = productVariant.Id,
					ProductAttributeId = attribute.ProductAttributeId,
					Name = attribute.ProductAttribute.GetLocalized(x => x.Name),
					Description = attribute.ProductAttribute.GetLocalized(x => x.Description),
					TextPrompt = attribute.TextPrompt ?? attribute.ProductAttribute.Name,
					IsRequired = attribute.IsRequired,
					AttributeControlType = attribute.AttributeControlType,
				};

				if (attribute.ShouldHaveValues())
				{
					//values
					var pvaValues = _productAttributeService.GetProductVariantAttributeValues(attribute.Id);
					foreach (var pvaValue in pvaValues)
					{
						var pvaValueModel = new ProductModel.ProductVariantModel.ProductVariantAttributeValueModel()
						{
							Id = pvaValue.Id,
							Name = pvaValue.GetLocalized(x => x.Name),
							ProductAttributeOptionId = pvaValue.ProductAttributeOptionId,
							IsPreSelected = pvaValue.IsPreSelected,
						};
						pvaModel.Values.Add(pvaValueModel);

						//display price if allowed
						if (_permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
						{
							decimal taxRate = decimal.Zero;
							decimal priceAdjustmentBase = _taxService.GetProductPrice(productVariant, pvaValue.PriceAdjustment, out taxRate);
							decimal priceAdjustment = _currencyService.ConvertFromPrimaryStoreCurrency(priceAdjustmentBase, _workContext.WorkingCurrency);
							if (priceAdjustmentBase > decimal.Zero)
								pvaValueModel.PriceAdjustment = "+" + _priceFormatter.FormatPrice(priceAdjustment, false, false);
							else if (priceAdjustmentBase < decimal.Zero)
								pvaValueModel.PriceAdjustment = "-" + _priceFormatter.FormatPrice(-priceAdjustment, false, false);

							pvaValueModel.PriceAdjustmentValue = priceAdjustment;
						}
					}
				}

				model.ProductVariantAttributes.Add(pvaModel);
			}



			#endregion

			#region Pictures

			var pictures = _pictureService.GetPicturesByProductVariantId(productVariant.Id);
			if (pictures.Count > 0)
			{
				var picture = pictures.FirstOrDefault();
				model.DefaultPictureModel = new PictureModel()
				{

					ImageUrl = _pictureService.GetPictureUrl(picture, _mediaSetting.ProductThumbPictureSize),
					LargeSizeImageUrl = _pictureService.GetPictureUrl(picture, _mediaSetting.CategoryThumbPictureSize),
					FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
					Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat"), model.Name),
					AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat"), model.Name),

				};
			}
			else if (productModel.DefaultPictureModel != null)
			{
				model.DefaultPictureModel = productModel.DefaultPictureModel;
			}
			else
			{
				//no images. set the default one
				model.DefaultPictureModel = new PictureModel()
				{
					ImageUrl = _pictureService.GetDefaultPictureUrl(_mediaSetting.ProductDetailsPictureSize),
					FullSizeImageUrl = _pictureService.GetDefaultPictureUrl(),
					Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat"), model.Name),
					AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat"), model.Name),
				};
			}

			#endregion Pictures

			return model;
		}

		private AttributeModel PrepareProductAttributeViewModel(ProductModel.ProductVariantModel.ProductVariantAttributeValueModel productVariantAttributeValueModel)
		{

			return new AttributeModel();
		}

		#endregion

		#region Categories
		/*New Search*************************************************/
		[NonAction]
		protected IEnumerable<ProductOverviewModel280> PrepareProductOverviewModels280(IEnumerable<Product> products,
			bool preparePriceModel = true, bool preparePictureModel = true,
			int? productThumbPictureSize = null, bool prepareSpecificationAttributes = false, bool prepareProductAttributes = false,
			bool forceRedirectionAfterAddingToCart = false)
		{
			if (products == null)
				throw new ArgumentNullException("products");

			//performance optimization. let's load all variants at one go
			var allVariants = _productService.GetProductVariantsByProductIds(products.Select(x => x.Id).ToArray());

			var models = new List<ProductOverviewModel280>();
			foreach (var product in products)
			{
				//we use already loaded variants
				var productVariant = allVariants.Where(x => x.ProductId == product.Id).FirstOrDefault();
				if (productVariant == null) continue;
				var model = new ProductOverviewModel280();
				model.Id = product.Id;
				model.VariantId = productVariant.Id;
				model.Name = productVariant.GetLocalized(x => x.Name);
				if (string.IsNullOrWhiteSpace(model.Name)) model.Name = product.GetLocalized(x => x.Name);
				model.ShortDescription = productVariant.GetLocalized(x => x.Description);
				if (string.IsNullOrWhiteSpace(model.ShortDescription)) model.ShortDescription = product.GetLocalized(x => x.ShortDescription);
				model.SeName = product.GetSeName();

				//model.Stock = 1; //productVariant.ManageInventoryMethod == ManageInventoryMethod.ManageStock ? productVariant.StockQuantity : productVariant.ProductVariantAttributeCombinations.Sum(x => x.StockQuantity);
				model.Stock = productVariant.ManageInventoryMethod == ManageInventoryMethod.ManageStock ? productVariant.StockQuantity : productVariant.ProductVariantAttributeCombinations.Sum(x => x.StockQuantity);
				model.HideDiscount = productVariant.HideDiscount;
				var pm = product.ProductManufacturers.FirstOrDefault();
				if (pm != null)
				{
					model.Manufacturer = pm.Manufacturer.GetLocalized(x => x.Name);
				}

				//price
				if (preparePriceModel)
				{
					#region Prepare product price
					var priceModel = new ProductOverviewModel280.ProductPriceModel280();
					if (productVariant.CallForPrice)
					{
						priceModel.CallForPrice = !productVariant.CallforPriceRequested(_workContext.CurrentCustomer);
						priceModel.HidePriceIfCallforPrice = priceModel.CallForPrice && !productVariant.DisplayPriceIfCallforPrice;
					}
					decimal taxRate = decimal.Zero;
					decimal finalPriceWithoutDiscountBase = _taxService.GetProductPrice(productVariant, _priceCalculationService.GetFinalPrice(productVariant, false), out taxRate);
					decimal finalPriceWithDiscountBase = _taxService.GetProductPrice(productVariant, _priceCalculationService.GetFinalPrice(productVariant, true, false), out taxRate);
					decimal finalPriceWithoutDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithoutDiscountBase, _workContext.WorkingCurrency);
					decimal finalPriceWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithDiscountBase, _workContext.WorkingCurrency);

					priceModel.Price = _priceFormatter.FormatPrice(finalPriceWithoutDiscount);
					if (finalPriceWithoutDiscountBase != finalPriceWithDiscountBase)
						priceModel.PriceWithDiscount = _priceFormatter.FormatPrice(finalPriceWithDiscount);
					else
						priceModel.PriceWithDiscount = null;

					#endregion Prepare product price

					model.ProductPrice = priceModel;
				}

				//picture
				if (preparePictureModel)
				{
					#region Prepare product picture
					//If a size has been set in the view, we use it in priority
					int pictureSize = _mediaSetting.CategoryThumbPictureSize;
					if (productThumbPictureSize != null)
						pictureSize = productThumbPictureSize.Value;
					
					//prepare picture model

					//var picture = productVariant.GetDefaultProductVariantPicture(_pictureService);
					PictureRepository pr = new PictureRepository();
					var picture = pr.GetByProductVariantId(productVariant.Id);

					var pictureModel = new PictureModel()
					{
						ImageUrl = _pictureService.GetPictureUrl(picture, pictureSize),
						FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
						Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat"), model.Name),
						AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat"), model.Name),
					};

					model.DefaultPictureModel = pictureModel;
					#endregion
				}
				//specs
				if (prepareSpecificationAttributes)
				{
					//
				}
				// attributes
				if (prepareProductAttributes)
				{
					//model. SpecificationAttributeModels = PrepareProductSpecificationModel(product);
				}

				models.Add(model);
			}
			return models;
		}

		private void GetCatalogPagingFilteringModel280FromQueryString(ref CatalogPagingFilteringModel280 command)
		{
			if (Request.Params["ViewMode"] != null)
				command.ViewMode = Request.Params["ViewMode"];

			if (Request.Params["OrderBy"] != null && !Request.Params["OrderBy"].Equals("undefined"))
				command.OrderBy = int.Parse(Request.Params["OrderBy"]);

			if (Request.Params["PriceRangeFilter.Min"] != null)
				command.PriceRangeFilter.Min = decimal.Parse(Request.Params["PriceRangeFilter.Min"]);

			if (Request.Params["PriceRangeFilter.Max"] != null && !Request.Params["PriceRangeFilter.Max"].Equals(""))
				command.PriceRangeFilter.Max = decimal.Parse(Request.Params["PriceRangeFilter.Max"]);

			if (Request.Params["PriceRangeFilter.From"] != null)
				command.PriceRangeFilter.From = decimal.Parse(Request.Params["PriceRangeFilter.From"]);

			if (Request.Params["PriceRangeFilter.To"] != null && !Request.Params["PriceRangeFilter.To"].Equals("undefined"))
				command.PriceRangeFilter.To = decimal.Parse(Request.Params["PriceRangeFilter.To"]);

			if (Request.Params["SpecificationFilter.FilteredOptionIds"] != null)
				command.SpecificationFilter.FilteredOptionIds = Array.ConvertAll(Request.Params["SpecificationFilter.FilteredOptionIds"].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries), int.Parse).ToList();

			if (Request.Params["ManufacturerFilter.FilteredManufacturerIds"] != null)
				command.ManufacturerFilter.FilteredManufacturerIds = Array.ConvertAll(Request.Params["ManufacturerFilter.FilteredManufacturerIds"].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries), int.Parse).ToList();

			if (Request.Params["CategoryFilter.FilteredCategoryIds"] != null)
				command.CategoryFilter.FilteredCategoryIds = Array.ConvertAll(Request.Params["CategoryFilter.FilteredCategoryIds"].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries), int.Parse).ToList();

			if (Request.Params["PageSize"] != null)
				command.PageSize = int.Parse(Request.Params["PageSize"]);

			if (Request.Params["PageNumber"] != null)
				command.PageNumber = int.Parse(Request.Params["PageNumber"]);

			if (Request.Params["FilterTrigging"] != null)
				command.FilterTrigging = Request.Params["FilterTrigging"];

			if (Request.Params["Q"] != null)
				command.Q = Request.Params["Q"];
		}

		[HttpGet]
		public ActionResult CategoryProducts280(int categoryId, CatalogPagingFilteringModel280 command)
		{
			GetCatalogPagingFilteringModel280FromQueryString(ref command);

			var category = _categoryService.GetCategoryById(categoryId);
            if (category == null || category.Deleted || !category.Published)
                return InvokeHttp404();
			//'Continue shopping' URL
			_customerService.SaveCustomerAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.LastContinueShoppingPage, _webHelper.GetThisPageUrl(false));
			//for breadcrumb prev,next products
			StatefulStorage.PerSession.Add("navigation", () => "cat-" + categoryId);

			if (command.PageNumber <= 0) command.PageNumber = 1;
			var model = category.ToModel280();

			//page size
			command.PageSize = category.PageSize;

			//price ranges
			decimal? minPriceConverted = null;
			decimal? maxPriceConverted = null;
			if (command.PriceRangeFilter.From.HasValue)
			{
				minPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(command.PriceRangeFilter.From.Value, _workContext.WorkingCurrency);
			}
			if (command.PriceRangeFilter.To.HasValue)
			{
				maxPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(command.PriceRangeFilter.To.Value, _workContext.WorkingCurrency);
			}

			#region category breadcrumb
			model.DisplayCategoryBreadcrumb = _catalogSettings.CategoryBreadcrumbEnabled;
			if (model.DisplayCategoryBreadcrumb)
			{
				foreach (var catBr in GetCategoryBreadCrumb(category))
				{
					model.CategoryBreadcrumb.Add(new CategoryModel()
					{
						Id = catBr.Id,
						Name = catBr.GetLocalized(x => x.Name),
						SeName = catBr.GetSeName()
					});
				}
			}
			#endregion category breadcrumb

			//products
			IList<int> alreadyFilteredSpecOptionIds = command.SpecificationFilter.GetFilteredOptionIds();
			IList<int> alreadyFilteredProductVariantAttributeIds = command.AttributeFilter.GetProductVariantAttributeIds();
			IList<int> alreadyFilteredManufacturerIds = command.ManufacturerFilter.GetFilteredManufacturerIds();
			IList<int> alreadyFilteredCategoryIds = command.CategoryFilter.GetFilteredCategoryIds();
			var categoryIds = alreadyFilteredCategoryIds;
			if (categoryIds.Count == 0)
				categoryIds.Add(category.Id);

			string productListModelCacheKey = string.Format(PRODUCT_LIST_MODEL, string.Join(",", categoryIds), minPriceConverted, maxPriceConverted, _workContext.WorkingLanguage.Id
				, string.Join(",", alreadyFilteredSpecOptionIds), string.Join(",", alreadyFilteredProductVariantAttributeIds)
				, string.Join(",", alreadyFilteredManufacturerIds), command.OrderBy, command.PageNumber, command.PageSize, null, currencyId);
			ProductListwithOverviewModel pm = new ProductListwithOverviewModel();

            if (Request.Params["cc"] != null && Request.Params["cc"].ToString().ToLowerInvariant().Equals("true"))
            {
                _cacheManager.Remove(productListModelCacheKey);
            }

			pm = _cacheManager.Get(productListModelCacheKey, int.MaxValue, () =>
			{
				IList<int> filterableSpecificationAttributeOptionIds = null;
				IList<int> filterableProductVariantAttributeIds = null;
				IList<int> filterableManufacturerIds = null;
				IList<int> filterableCategoryIds = null;
				decimal? maxPrice = null;

				pm.Products = _productService.SearchProductsFilterable(
					categoryIds, 0, null, minPriceConverted, maxPriceConverted,
					0, string.Empty, false, false, _workContext.WorkingLanguage.Id,
					alreadyFilteredSpecOptionIds, alreadyFilteredProductVariantAttributeIds,
					alreadyFilteredManufacturerIds, (ProductSortingEnum)command.OrderBy, command.PageNumber - 1, command.PageSize, true,
					out filterableSpecificationAttributeOptionIds,
					out filterableProductVariantAttributeIds,
					out filterableManufacturerIds,
					out filterableCategoryIds,
					out maxPrice);

				pm.FilterableSpecificationAttributeOptionIds = filterableSpecificationAttributeOptionIds;
				pm.FilterableProductVariantAttributeIds = filterableProductVariantAttributeIds;
				pm.FilterableManufacturerIds = filterableManufacturerIds;
				pm.FilterableCategoryIds = filterableCategoryIds;
				pm.MaxPrice = maxPrice;

				pm.ProductsOverview = PrepareProductOverviewModels280(pm.Products).ToList();
				return pm;
			});

			model.Products = pm.ProductsOverview;

			model.PagingFilteringContext.LoadPagedList(pm.Products);
			model.PagingFilteringContext.ViewMode = command.ViewMode;

			//specs
			model.PagingFilteringContext.SpecificationFilter.PrepareSpecsFilters(alreadyFilteredSpecOptionIds,
				pm.FilterableSpecificationAttributeOptionIds,
				_specificationAttributeService, _webHelper, _workContext);

			//attributes
			//model.PagingFilteringContext.AttributeFilter.PrepareAttributeFilters(alreadyFilteredProductVariantAttributeIds,
			//  filterableProductVariantAttributeIds,
			//  _productAttributeService, _webHelper, _workContext);

			//maufacturers
			model.PagingFilteringContext.ManufacturerFilter.PrepareManufacturerFilters(alreadyFilteredManufacturerIds, pm.FilterableManufacturerIds, _manufacturerService, _webHelper, _workContext);

			//categories
			categoryIds = new List<int>();
			model.PagingFilteringContext.CategoryFilter.PrepareCategoryFilters(categoryId, categoryIds, pm.FilterableCategoryIds, _categoryService, _webHelper, _workContext);

			//pricerange
			model.PagingFilteringContext.PriceRangeFilter.PreparePriceRangeFilters(0, pm.MaxPrice, command.PriceRangeFilter.From, command.PriceRangeFilter.To, _webHelper, _workContext);


			//if (command.FilterTrigging == "C" || command.FilterTrigging == "M" || command.FilterTrigging == "S" || command.FilterTrigging == "A")
			//    model.PagingFilteringContext.ShowFilteringPanel = true;
			//else
			//    model.PagingFilteringContext.ShowFilteringPanel = false;


			//news content
			//TODO:280 cache here
			model.NewsItemStartPoint = _catalogSettings.ProductListBannerStartpoint;
			model.NewsItemModel = GetCategoryNewsItemModel(categoryId);

			model.IsGuest = _workContext.CurrentCustomer.IsGuest();

			return View("CategoryProducts280", model);
		}

		public JsonResult CategoryProductsJS280(int categoryId, CatalogPagingFilteringModel280 command)
		{
			var category = _categoryService.GetCategoryById(categoryId);
			if (category == null || category.Deleted || !category.Published)
				return Json(new { success = false, Reaction = "Redirect", Url = Url.Action("Index", "Home"), });

			if (command.PageNumber <= 0) command.PageNumber = 1;
			var model = category.ToModel280();

			//page size
			command.PageSize = category.PageSize;

			//price ranges
			decimal? minPriceConverted = null;
			decimal? maxPriceConverted = null;
			if (command.PriceRangeFilter.From.HasValue)
			{
				minPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(command.PriceRangeFilter.From.Value, _workContext.WorkingCurrency);
			}
			if (command.PriceRangeFilter.To.HasValue)
			{
				maxPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(command.PriceRangeFilter.To.Value, _workContext.WorkingCurrency);
			}
			var categoryIds = new List<int>();
			categoryIds.Add(category.Id);
			//products
			IList<int> alreadyFilteredSpecOptionIds = command.SpecificationFilter.GetFilteredOptionIds();
			IList<int> alreadyFilteredProductVariantAttributeIds = command.AttributeFilter.GetProductVariantAttributeIds();
			IList<int> alreadyFilteredManufacturerIds = command.ManufacturerFilter.GetFilteredManufacturerIds();
			IList<int> alreadyFilteredCategoryIds = command.CategoryFilter.GetFilteredCategoryIds();


			string productListModelCacheKey = string.Format(PRODUCT_LIST_MODEL, string.Join(",", categoryIds), minPriceConverted, maxPriceConverted, _workContext.WorkingLanguage.Id
				, string.Join(",", alreadyFilteredSpecOptionIds), string.Join(",", alreadyFilteredProductVariantAttributeIds)
				, string.Join(",", alreadyFilteredManufacturerIds), command.OrderBy, command.PageNumber, command.PageSize, null, currencyId);
			ProductListwithOverviewModel pm = new ProductListwithOverviewModel();
			pm = _cacheManager.Get(productListModelCacheKey, () =>
			{
				IList<int> filterableSpecificationAttributeOptionIds = null;
				IList<int> filterableProductVariantAttributeIds = null;
				IList<int> filterableManufacturerIds = null;
				IList<int> filterableCategoryIds = null;
				decimal? maxPrice = null;

				pm.Products = _productService.SearchProductsFilterable(
					categoryIds, 0, null, minPriceConverted, maxPriceConverted,
					0, string.Empty, false, false, _workContext.WorkingLanguage.Id,
					alreadyFilteredSpecOptionIds, alreadyFilteredProductVariantAttributeIds,
					alreadyFilteredManufacturerIds, (ProductSortingEnum)command.OrderBy, command.PageNumber - 1, command.PageSize, true,
					out filterableSpecificationAttributeOptionIds,
					out filterableProductVariantAttributeIds,
					out filterableManufacturerIds,
					out filterableCategoryIds,
					out maxPrice);

				pm.FilterableSpecificationAttributeOptionIds = filterableSpecificationAttributeOptionIds;
				pm.FilterableProductVariantAttributeIds = filterableProductVariantAttributeIds;
				pm.FilterableManufacturerIds = filterableManufacturerIds;
				pm.FilterableCategoryIds = filterableCategoryIds;
				pm.MaxPrice = maxPrice;

				pm.ProductsOverview = PrepareProductOverviewModels280(pm.Products).ToList();
				return pm;
			});

			model.Products = pm.ProductsOverview;





			model.PagingFilteringContext.LoadPagedList(pm.Products);
			//model.PagingFilteringContext.ViewMode = viewMode;

			//specs
			model.PagingFilteringContext.SpecificationFilter.PrepareSpecsFilters(alreadyFilteredSpecOptionIds,
				pm.FilterableSpecificationAttributeOptionIds,
				_specificationAttributeService, _webHelper, _workContext);

			//attributes
			//model.PagingFilteringContext.AttributeFilter.PrepareAttributeFilters(alreadyFilteredProductVariantAttributeIds,
			//  filterableProductVariantAttributeIds,
			//  _productAttributeService, _webHelper, _workContext);

			//maufacturers
			model.PagingFilteringContext.ManufacturerFilter.PrepareManufacturerFilters(alreadyFilteredManufacturerIds, pm.FilterableManufacturerIds, _manufacturerService, _webHelper, _workContext);

			//categories
			model.PagingFilteringContext.CategoryFilter.PrepareCategoryFilters(categoryId, alreadyFilteredCategoryIds, pm.FilterableCategoryIds, _categoryService, _webHelper, _workContext);

			//pricerange
			model.PagingFilteringContext.PriceRangeFilter.PreparePriceRangeFilters(0, pm.MaxPrice, command.PriceRangeFilter.From, command.PriceRangeFilter.To, _webHelper, _workContext);

			if (command.FilterTrigging == "C" || command.FilterTrigging == "M" || command.FilterTrigging == "S" || command.FilterTrigging == "A")
				model.PagingFilteringContext.ShowFilteringPanel = true;
			else
				model.PagingFilteringContext.ShowFilteringPanel = false;

			string view = null;
			if (command.ViewMode == "grid1")
				view = @"~/Views/Catalog/ProductsList1280.cshtml";
			else if (command.ViewMode == "grid2")
				view = @"~/Views/Catalog/ProductsList2280.cshtml";
			else
				view = @"~/Views/Catalog/ProductsList3280.cshtml";

			return Json(new
			{
				Html = Utilities.RenderPartialViewToString(this, view, model),
				SelectionHtml = Utilities.RenderPartialViewToString(this, "~/Views/Catalog/_yourSelection280.cshtml", model.PagingFilteringContext),
				PriceMin = model.PagingFilteringContext.PriceRangeFilter.Min,
				PriceMax = model.PagingFilteringContext.PriceRangeFilter.Max,
				PriceValue = model.PagingFilteringContext.PriceRangeFilter.To,
				PriceStep = model.PagingFilteringContext.PriceRangeFilter.StepSize,
				HasMore = model.PagingFilteringContext.HasNextPage,
				PageNumber = model.PagingFilteringContext.PageNumber,
				PageSize = model.PagingFilteringContext.PageSize,
				ViewMode = command.ViewMode
			});
		}

		/**************************************************/

		//AF
		public ActionResult CategoryProducts(int categoryId, CatalogExtendedPagingFilteringModel command)
		{
			var category = _categoryService.GetCategoryById(categoryId);
			if (category == null || category.Deleted || !category.Published)
				return RedirectToAction("Index", "Home");
			_customerService.SaveCustomerAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.LastContinueShoppingPage, _webHelper.GetThisPageUrl(false));

			var model = category.ToModel();
			#region category breadcrumb
			model.DisplayCategoryBreadcrumb = _catalogSettings.CategoryBreadcrumbEnabled;
			if (model.DisplayCategoryBreadcrumb)
			{
				foreach (var catBr in GetCategoryBreadCrumb(category))
				{
					model.CategoryBreadcrumb.Add(new CategoryModel()
					{
						Id = catBr.Id,
						Name = catBr.GetLocalized(x => x.Name),
						SeName = catBr.GetSeName()
					});
				}
			}
			#endregion category breadcrumb
			var baseProducts = _cacheManager.Get(string.Format(CATEGORY_MODEL_BY_ID_KEY, category.Id, _workContext.WorkingLanguage.Id, _workContext.WorkingCurrency.Id), int.MaxValue, () => GetProductsModel(command));

			if (command.PageSize <= 0) command.PageSize = category.PageSize;
			if (command.PageNumber <= 0) command.PageNumber = 1;

			model.NewsItemStartPoint = _catalogSettings.ProductListBannerStartpoint;
			//newsItem
			model.NewsItemModel = GetCategoryNewsItemModel(categoryId);

			var productsSet = GetFilteredProducts(command, baseProducts);
			model.Products = productsSet["all"];
			LoadFilteringContext(model, command, productsSet);
			SelectPagedProducts(model, command);
			SetProductVariantPriceModel(model.Products);
			model.IsGuest = _workContext.CurrentCustomer.IsGuest();
			return View(model);
		}

		// [AjaxCallErrorHandler(Reaction = AjaxErrorReaction.Redirect, Url = UrlHelper.GenerateUrl(null, "Index", "Controller", null, null, null, false))]
		public JsonResult CategoryProductsJS(int categoryId, CatalogExtendedPagingFilteringModel command)
		{
			var category = _categoryService.GetCategoryById(categoryId);
			if (category == null || category.Deleted || !category.Published)
				return Json(new { success = false, Reaction = "Redirect", Url = Url.Action("Index", "Home"), });

			var model = category.ToModel();
			var baseProducts = _cacheManager.Get(string.Format(CATEGORY_MODEL_BY_ID_KEY, category.Id, _workContext.WorkingLanguage.Id, _workContext.WorkingCurrency.Id), int.MaxValue, () => GetProductsModel(command));

			if (command.PageSize <= 0) command.PageSize = category.PageSize;
			if (command.PageNumber <= 0) command.PageNumber = 1;

			var productsSet = GetFilteredProducts(command, baseProducts);
			model.Products = productsSet["all"];
			LoadFilteringContext(model, command, productsSet);
			SelectPagedProducts(model, command);
			string view = null;
			if (command.ViewMode == "grid1")
				view = @"~/Views/Catalog/ProductsList1.cshtml";
			else if (command.ViewMode == "grid2")
				view = @"~/Views/Catalog/ProductsList2.cshtml";
			else
				view = @"~/Views/Catalog/ProductsList3.cshtml";

			//Customer Quute 
			SetProductVariantPriceModel(model.Products);
			return Json(new
			{
				Html = Utilities.RenderPartialViewToString(this, view, model),
				SelectionHtml = Utilities.RenderPartialViewToString(this, "~/Views/Catalog/_yourSelection.cshtml", model.PagingFilteringContext),
				PriceMin = model.PagingFilteringContext.PriceRangeSliderFilter.Item.Min,
				PriceMax = model.PagingFilteringContext.PriceRangeSliderFilter.Item.Max,
				PriceValue = model.PagingFilteringContext.PriceRangeSliderFilter.Item.Value,
				PriceStep = model.PagingFilteringContext.PriceRangeSliderFilter.Item.StepSize,
				HasMore = model.PagingFilteringContext.HasNextPage,
				PageNumber = model.PagingFilteringContext.PageNumber,
				PageSize = model.PagingFilteringContext.PageSize,
				ViewMode = command.ViewMode
			});
		}


		private void LoadSpecificationFilterModel(CatalogExtendedPagingFilteringModel model, Dictionary<string, List<ProductModel>> productsSet)
		{
			IEnumerable<ProductSpecificationModel> options = new List<ProductSpecificationModel>();
			//foreach (var pm in products)
			//    options = options.Union(pm.SpecificationAttributeModels, new ProductSpecificationModelComparer()).ToList();
			////Notfiltered stores all specification attributes*options related to the product set
			//model.SpecificationFilter.NotFilteredItems = options.Select(x =>
			//{
			//    var item = new Nop.Web.Models.Catalog.CatalogPagingFilteringModel.SpecificationFilterItem();
			//    item.SpecificationAttributeName = x.SpecificationAttributeName;
			//    item.SpecificationAttributeOptionName = x.SpecificationAttributeOption;
			//    item.AttributeId = x.SpecificationAttributeId;
			//    item.OptionId = x.SpecificationAttributeOptionId;
			//    return item;
			//}).ToList();

			foreach (var kvp in productsSet)
			{
				if (kvp.Key.StartsWith("spec"))
				{
					int id = int.Parse(kvp.Key.Substring(4));
					foreach (var pm in kvp.Value)
						options = options.Union(pm.SpecificationAttributeModels.Where(x => x.SpecificationAttributeId == id), new ProductSpecificationModelComparer());
					//Notfiltered stores all specification attributes*options related to the product set
				}
			}
			// if (options.Count == 0)
			//  {
			foreach (var pm in productsSet["all"])
				options = options.Union(pm.SpecificationAttributeModels, new ProductSpecificationModelComparer());
			options = options.OrderBy(x => x.SpecificationAttributeId).ThenBy(x => x.Position).ToList();

			//  }

			model.SpecificationFilter.NotFilteredItems = options.Select(x =>
			{
				var item = new Nop.Web.Models.Catalog.CatalogPagingFilteringModel.SpecificationFilterItem();
				item.SpecificationAttributeName = x.SpecificationAttributeName;
				item.SpecificationAttributeOptionName = x.SpecificationAttributeOption;
				item.AttributeId = x.SpecificationAttributeId;
				item.OptionId = x.SpecificationAttributeOptionId;
				return item;
			}).ToList();

		}

		private IList<AttributeModel> LoadProductAttributesModel_(IList<ProductModel> products, out decimal max)
		{
			List<AttributeModel> productAttributeModels = new List<AttributeModel>();
			max = 0;
			foreach (var product in products)
			{
				product.SpecificationAttributeModels = _specificationAttributeService.GetProductSpecificationAttributesByProductId(product.Id, null, true)
			   .Select(psa =>
			   {
				   return new ProductSpecificationModel()
				   {
					   SpecificationAttributeId = psa.SpecificationAttributeOption.SpecificationAttributeId,
					   SpecificationAttributeOptionId = psa.SpecificationAttributeOptionId,
					   SpecificationAttributeName = psa.SpecificationAttributeOption.SpecificationAttribute.GetLocalized(x => x.Name),
					   SpecificationAttributeOption = psa.SpecificationAttributeOption.GetLocalized(x => x.Name),
					   Position = psa.SpecificationAttributeOption.DisplayOrder
				   };
			   })
			   .ToList();
				foreach (var variantModel in product.ProductVariantModels)
				{
					max = Math.Max(max, variantModel.ProductVariantPrice.PriceValue);
					foreach (var attribute in variantModel.ProductVariantAttributes)
					{
						var attr = productAttributeModels.Find(x => x.AttributeId == attribute.ProductAttributeId);
						if (attr == null)
						{
							attr = new AttributeModel() { AttributeId = attribute.ProductAttributeId, Name = attribute.TextPrompt };
							productAttributeModels.Add(attr);
						}
						foreach (var value in attribute.Values)
						{
							var val = attr.ValueModels.FirstOrDefault(x => x.ProductAttributeOptionId == value.ProductAttributeOptionId);
							if (val == null)
							{
								attr.ValueModels.Add(new AttributeValueModel(value));
							}
						}
					}
				}
			}
			return productAttributeModels;
		}

		private IList<AttributeModel> LoadProductAttributesModel(Dictionary<string, List<ProductModel>> productsSet)
		{
			List<AttributeModel> productAttributeModels = new List<AttributeModel>();

			foreach (var kvp in productsSet)
			{
				if (kvp.Key.StartsWith("attr"))
				{
					int id = int.Parse(kvp.Key.Substring(4));
					foreach (var product in kvp.Value)
					{
						foreach (var variantModel in product.ProductVariantModels)
						{
							foreach (var attribute in variantModel.ProductVariantAttributes)
							{
								var attr = productAttributeModels.Find(x => x.AttributeId == attribute.ProductAttributeId);
								if (attr == null)
								{
									attr = new AttributeModel() { AttributeId = attribute.ProductAttributeId, Name = attribute.Name };
									productAttributeModels.Add(attr);
								}
								foreach (var value in attribute.Values)
								{
									var val = attr.ValueModels.FirstOrDefault(x => x.ProductAttributeOptionId == value.ProductAttributeOptionId);
									if (val == null)
									{
										attr.ValueModels.Add(new AttributeValueModel(value));
									}
								}
							}
						}
					}
				}
			}
			if (productAttributeModels.Count == 0)
			{
				foreach (var product in productsSet["all"])
				{
					foreach (var variantModel in product.ProductVariantModels)
					{
						foreach (var attribute in variantModel.ProductVariantAttributes)
						{
							var attr = productAttributeModels.Find(x => x.AttributeId == attribute.ProductAttributeId);
							if (attr == null)
							{
								attr = new AttributeModel() { AttributeId = attribute.ProductAttributeId, Name = attribute.TextPrompt };
								productAttributeModels.Add(attr);
							}
							foreach (var value in attribute.Values)
							{
								var val = attr.ValueModels.FirstOrDefault(x => x.ProductAttributeOptionId == value.ProductAttributeOptionId);
								if (val == null)
								{
									attr.ValueModels.Add(new AttributeValueModel(value));
								}
							}
						}
					}
				}
			}
			return productAttributeModels;
		}

		CatalogExtendedPagingFilteringModel.AttributeFilterModel GetAttributeFilterModel(IList<AttributeModel> list)
		{
			var model = new CatalogExtendedPagingFilteringModel.AttributeFilterModel();
			foreach (var attr in list)
			{
				foreach (var option in attr.ValueModels)
				{
					model.Items.Add(new CatalogExtendedPagingFilteringModel.AttributeFilterItem()
					{
						AttributeId = attr.AttributeId,
						AttributeName = attr.Name,
						AttributeOptionName = option.Name,
						OptionId = option.ProductAttributeOptionId
					});
				}
			}
			return model;
		}

		private bool ProductHasPrice(ProductModel productModel, decimal? maxPrice)
		{
			if (!maxPrice.HasValue) return true;
			return productModel.DefaultVariantModel.ProductVariantPrice.PriceValue <= maxPrice;
		}

		private bool ProductHasManufacturer(ProductModel productModel, IList<CatalogExtendedPagingFilteringModel.ManufacturerFilterItem> manufacturers)
		{
			if (manufacturers.Count == 0) return true;
			if (productModel.ProductManufacturers.Count == 0) return false;

			foreach (var filterItem in manufacturers)
			{
				if (productModel.ProductManufacturers.FirstOrDefault(x => x.Id == filterItem.Id) != null)
					return true;
			}
			return false;

		}

		private bool ProductHasCategory(ProductModel productModel, IList<CatalogExtendedPagingFilteringModel.CategoryFilterItem> categories)
		{
			if (categories.Count == 0) return true;
			if (productModel.ProductCategories.Count == 0) return false;

			foreach (var filterItem in categories)
			{
				if (productModel.ProductCategories.FirstOrDefault(x => x.Id == filterItem.Id) != null)
					return true;
			}
			return false;
		}

		private bool ProductHasSpecificationOptions(ProductModel productModel, IList<CatalogPagingFilteringModel.SpecificationFilterItem> filterItems)
		{
			var hasSpecOption = true;
			ProductSpecificationModel specificationModel = null;
			foreach (var filterItemGroup in filterItems.GroupBy(x => x.AttributeId))
			{
				if (!hasSpecOption) break;
				hasSpecOption = false;
				specificationModel = productModel.SpecificationAttributeModels.FirstOrDefault(x => x.SpecificationAttributeId == filterItemGroup.FirstOrDefault().AttributeId);
				if (specificationModel != null)
				{
					foreach (var option in filterItemGroup)
					{
						if (option.OptionId == specificationModel.SpecificationAttributeOptionId)
						{
							hasSpecOption = true;
							break;
						}
					}
				}

			}
			return hasSpecOption;
		}
		//Specification based
		private bool ProductHasSpecification(ProductModel productModel, IList<CatalogPagingFilteringModel.SpecificationFilterItem> filterItems)
		{
			var hasSpecOption = false;
			var specificationModels = productModel.SpecificationAttributeModels.Where(x => x.SpecificationAttributeId == filterItems.FirstOrDefault().AttributeId);

			foreach (var specificationModel in specificationModels)
			{
				foreach (var option in filterItems)
				{
					if (option.OptionId == specificationModel.SpecificationAttributeOptionId)
					{
						return true;
					}
				}

			}
			return hasSpecOption;
		}

		private List<List<int>> CreateAttributeCombinations(IList<CatalogExtendedPagingFilteringModel.AttributeFilterItem> filterItems)
		{
			List<List<int>> combinations;
			List<List<int>> newcombinations = new List<List<int>>();
			foreach (var attributeGroup in filterItems.GroupBy(x => x.AttributeId))
			{
				combinations = newcombinations;
				newcombinations = new List<List<int>>();
				foreach (var filterItem in attributeGroup)
				{
					foreach (var combination in combinations)
					{
						var comb = new List<int>(combination);
						comb.Add(filterItem.OptionId);
						newcombinations.Add(comb);
					}
					if (combinations.Count == 0)
						newcombinations.Add(new List<int>() { filterItem.OptionId });
				}
			}
			return newcombinations;
		}

		private bool VariantHasAttributeValues(ProductModel.ProductVariantModel model, List<List<int>> attributeCombinations)
		{
			if (attributeCombinations.Count == 0) return true;
			if (model.ManageInventoryMethodId == (int)ManageInventoryMethod.ManageStockByAttributes)
			{
				foreach (var filterAttributeCombination in attributeCombinations)
				{
					foreach (var attributeValueIdCombination in model.AttributeValueIdCombinations)
					{
						if (attributeValueIdCombination == null) continue;
						if (filterAttributeCombination.Except(attributeValueIdCombination).Count() == 0) return true;
					}
				}
			}
			else
			{
				foreach (var filterAttributeCombination in attributeCombinations)
				{
					if (filterAttributeCombination.Except(model.AttributeValueIds).Count() == 0) return true;
				}
			}
			return false;
		}

		private bool ProductHasAttributeOptions(ProductModel productModel, List<List<int>> attributeCombinations)
		{
			bool exists = false;
			List<int> variantIds = new List<int>();
			foreach (var variantModel in productModel.ProductVariantModels)
			{
				variantModel.Active = VariantHasAttributeValues(variantModel, attributeCombinations);
				exists = exists || variantModel.Active;

			}
			return exists;
		}

		private bool ProductVariantHasAttributeOptions(ProductModel.ProductVariantModel variantModel, IList<CatalogExtendedPagingFilteringModel.AttributeFilterItem> filterItems)
		{
			var hasAttributeOption = true;
			ProductModel.ProductVariantModel.ProductVariantAttributeModel attributeModel = null;
			if (variantModel.ManageInventoryMethodId == (int)ManageInventoryMethod.ManageStock || variantModel.ManageInventoryMethodId == (int)ManageInventoryMethod.DontManageStock)
			{
				foreach (var attributeGroup in filterItems.GroupBy(x => x.AttributeId))
				{
					if (!hasAttributeOption) break;
					hasAttributeOption = false;
					attributeModel = variantModel.ProductVariantAttributes.FirstOrDefault(x => x.Id == attributeGroup.FirstOrDefault().AttributeId);
					if (attributeModel != null)
					{
						foreach (var attributeFilterItem in attributeGroup)
						{
							if (attributeModel.Values.FirstOrDefault(x => x.ProductAttributeOptionId == attributeFilterItem.OptionId) != null)
							{
								hasAttributeOption = true;
								break;
							}
						}
					}
				}
			}
			//attribute combinations
			else
			{
				hasAttributeOption = true;


				//foreach(var combination in variantModel.Entity.ProductVariantAttributeCombinations)
				//{

				//    variantModel.ProductVariantAttributes[0].Values[0].


				//    foreach (var attributeGroup in filterItems.GroupBy(x => x.AttributeId))
				//    {
				//        var valueids = _productAttributeParser.ParseValues(combination.AttributesXml,);

				//        if (!hasAttributeOption) break;
				//        hasAttributeOption = false;
				//        ids.Contains(


				//    }

				//}
			}
			return hasAttributeOption;
		}


		[ChildActionOnly]
		public ActionResult HomepageCategories()
		{
			var listModel = _categoryService.GetAllCategoriesDisplayedOnHomePage()
				.Select(x =>
				{
					var catModel = x.ToModel();
					catModel.PictureModel.ImageUrl = _pictureService.GetPictureUrl(x.PictureId, _mediaSetting.CategoryThumbPictureSize, true);
					catModel.PictureModel.Title = string.Format(_localizationService.GetResource("Media.Category.ImageLinkTitleFormat"), catModel.Name);
					catModel.PictureModel.AlternateText = string.Format(_localizationService.GetResource("Media.Category.ImageAlternateTextFormat"), catModel.Name);
					return catModel;
				})
				.ToList();

			return PartialView(listModel);
		}

		[ChildActionOnly]
		public ActionResult CategoryProductsToolBox(int categoryId)
		{
			var model = new CatalogExtendedPagingFilteringModel();
			var category = _categoryService.GetCategoryById(categoryId);

			var manufacturers = GetCategoryManufacturers(categoryId);
			var manufacturersModel = manufacturers.Select(x => x.ToModel()).ToList();

			model.SpecificationFilter.LoadSpecsFilters(category, _specificationAttributeService, _webHelper, _workContext);
			model.ManufacturerFilter.Items = manufacturersModel.Select(x => new CatalogExtendedPagingFilteringModel.ManufacturerFilterItem() { Id = x.Id, Name = x.Name, Selected = false }).ToList();

			return View(model);
		}
		//AF
		[OutputCache(Duration = int.MaxValue, VaryByParam = "categoryId", VaryByCustom = "lgg", Location = OutputCacheLocation.Server)]
		public ActionResult CategoryMain(int categoryId)
		{
            var category = _categoryService.GetCategoryById(categoryId);
            if (category == null || category.Deleted)
                return InvokeHttp404();

			var categoryModel = _cacheManager.Get(string.Format(CATEGORY_MAIN_MODEL_BY_ID_KEY, category.Id, _workContext.WorkingLanguage.Id, _workContext.WorkingCurrency.Id), int.MaxValue, () =>
			{
				var model = new CategoryMenuModel();
				model.SeName = category.GetSeName();
				var subCategories = _categoryService.GetAllCategoriesByParentCategoryId(category.Id);
				var subCategoryModels = subCategories.Select(x => x.ToModel());
				foreach (var subCategoryModel in subCategoryModels)
				{
					var subHeaderItem = new SubHeaderItem() { Name = subCategoryModel.Name, Title = subCategoryModel.Name, Url = Url.RouteUrl("Category", new { categoryId = subCategoryModel.Id, SeName = subCategoryModel.SeName }) };
					var childCategories = _categoryService.GetAllCategoriesByParentCategoryId(subCategoryModel.Id);
					var childCategoryModels = childCategories.Select(x => x.ToModel());
					subHeaderItem.Items = childCategoryModels.Select(x => new MenuItem() { Name = x.Name, Title = x.Name, Url = Url.RouteUrl("Category", new { categoryId = x.Id, SeName = x.SeName }) }).ToList<MenuItem>();
                    if (subHeaderItem.Items.Count > 0)
                    {
                        //subHeaderItem.Url = "#";
                        model.SubHeaders.Add(subHeaderItem);
                    }
                    else
                    {
                        var products = GetCategoryProducts(subCategoryModel.Id, true);
                        if (products.Count > 0)
                            model.SubHeaders.Add(subHeaderItem);
                    }

				}
				//var manufacturersAll = GetProductsManufacturers(GetCategoryProducts(category.Id, true));
				var manufacturersAll = _manufacturerService.GetManufacturerByCategoryIds(GetChildCategoryIds(categoryId));
				var manufacturers = manufacturersAll.Skip(0).Take(3);
				var manufacturerModels = manufacturers.Select(x => x.ToModel());
				var brandItems = manufacturerModels.Select(x => new MenuItem() { Name = x.Name, Title = x.Name, Url = Url.RouteUrl("ManufacturerCategorySe", new { SeName = x.SeName, categoryId = categoryId }) }).ToList<MenuItem>();
				if (manufacturersAll.Count > 3)
				{
					brandItems.Add(new MenuItem()
					{
						Name = _localizationService.GetResource("Category.AllDesigners"),
						Title = _localizationService.GetResource("Category.AllDesigners"),
						Url = Url.RouteUrl("CategoryManufacturerList", new { categoryId = categoryId, SeName = model.SeName })
					});
				}
				if (brandItems.Count > 0)
				{
                    var subHeaderItem = new SubHeaderItem() { Name = _localizationService.GetResource("Manufacturer.Manufacturers"), Url = Url.RouteUrl("CategoryManufacturerList", new { categoryId = categoryId, SeName = model.SeName }) };
					subHeaderItem.Items = brandItems;
					subHeaderItem.HasMore = manufacturersAll.Count > 3;
					model.SubHeaders.Add(subHeaderItem);
				}
				model.ContentItem = GetCategoryContent(category).FirstOrDefault();
				var tempModel = category.ToModel();
				model.Name = tempModel.Name;
				model.MetaDescription = tempModel.MetaDescription;
				model.MetaKeywords = tempModel.MetaKeywords;
				model.MetaTitle = tempModel.MetaTitle;
				model.IsGuest = _workContext.CurrentCustomer.IsGuest();

				return model;

			});
			return View(categoryModel);
		}
		//AF
		[NonAction]
		private IEnumerable<ContentItemModel> GetCategoryContent(Category category)
		{
			var contentProductCategories = _contentService.GetContent(ContentType.CategoryHomeContent.ToContentString(), category.Id, int.MaxValue);
			var contentProducts = contentProductCategories.Select(x => x.Product);
			var contentItems = contentProducts.ToContentItems(_pictureService);
			return contentItems;
		}
		//AF
		[NonAction]
		private List<Product> GetCategoryProducts(int categoryId, bool includeChildCategories = true)
		{
			IEnumerable<Product> products = new List<Product>();
			products = products.Union(_productService.SearchProducts(categoryId,
								0, null, null, null, 0, string.Empty, false, 0, null,
								ProductSortingEnum.Position, 0, int.MaxValue));
			if (includeChildCategories)
				foreach (var subCategory in _categoryService.GetAllCategoriesByParentCategoryId(categoryId))
				{
					products = products.Union(GetCategoryProducts(subCategory.Id, true));
				}
			return products.ToList();
		}
		//AF
		[NonAction]
		private List<Manufacturer> GetProductsManufacturers(IList<Product> products)
		{
			List<Manufacturer> manufacturers = new List<Manufacturer>();
			foreach (var product in products)
			{
				foreach (var productManufacturer in product.ProductManufacturers)
				{
					if (productManufacturer.Manufacturer.Published)
						if (!manufacturers.Contains(productManufacturer.Manufacturer))
						{
							manufacturers.Add(productManufacturer.Manufacturer);
						}
				}
			}
			return manufacturers.OrderBy(x => x.DisplayOrder).ToList();
		}
		private List<ProductModel.ProductManufacturerModel> GetProductsManufacturers(IList<ProductModel> products)
		{
			List<ProductModel.ProductManufacturerModel> manufacturers = new List<ProductModel.ProductManufacturerModel>();
			ProductModel.ProductManufacturerModel productManufacturerModel;
			foreach (var product in products)
			{
				//TODO: comment out for temporary solution of "gift" categories not to be ssen at "your selection"
				//foreach (var productCategory in product.Entity.ProductCategories)
				//{
				productManufacturerModel = product.ProductManufacturers.FirstOrDefault();
				if (productManufacturerModel != null)
				{
					if (manufacturers.FirstOrDefault(x => x.Id == productManufacturerModel.Id) == null)
					{
						manufacturers.Add(productManufacturerModel);
					}
				}
				//}
			}
			return manufacturers;
		}
		//AF
		[NonAction]
		private List<Manufacturer> GetCategoryManufacturers(int categoryId, bool includeChildCategories = true)
		{
			var products = GetCategoryProducts(categoryId, includeChildCategories);
			return GetProductsManufacturers(products);
		}
		//AF
		[NonAction]
		private List<Category> GetManufacturerCategories(int manufacturerId)
		{
			var products = _productService.SearchProducts(0,
								manufacturerId, null, null, null, 0, string.Empty, false, 0, null,
								ProductSortingEnum.Position, 0, int.MaxValue);

			return GetProductsCategories(products);
		}
		//AF
		[NonAction]
		private List<Category> GetProductsCategories(IList<Product> products)
		{
			List<Category> categories = new List<Category>();
			foreach (var product in products)
			{
				foreach (var productCategory in product.ProductCategories)
				{
					if (!categories.Contains(productCategory.Category))
					{
						categories.Add(productCategory.Category);
					}
				}
			}
			return categories;
		}
		private List<ProductModel.ProductCategoryModel> GetProductsCategories(IList<ProductModel> products)
		{
			List<ProductModel.ProductCategoryModel> categories = new List<ProductModel.ProductCategoryModel>();
			ProductModel.ProductCategoryModel productCategoryModel;
			foreach (var product in products)
			{
				//TODO: comment out for temporary solution of "gift" categories not to be ssen at "your selection"
				//foreach (var productCategory in product.Entity.ProductCategories)
				//{
				productCategoryModel = product.ProductCategories.FirstOrDefault();
				if (productCategoryModel != null)
				{
					if (categories.FirstOrDefault(x => x.Id == productCategoryModel.Id) == null)
					{
						categories.Add(productCategoryModel);
					}
				}
				//}
			}
			return categories;
		}
		#endregion

		#region NewsItems
		[NonAction]
		private NewsItemModel GetManufacturerNewsItemModel(int manufacturerId)
		{
			var newsItem = _manufacturerService.GetManufacturerNewsItemByManufacturerId(manufacturerId, _workContext.WorkingLanguage.Id);
			if (newsItem == null)
				return null;
			var model = new NewsItemModel();
			PrepareNewsItemDetailModel(model, newsItem, false, false);
			return model;
		}
		[NonAction]
		private NewsItemModel GetCategoryNewsItemModel(int categoryId)
		{
			var newsItem = _categoryService.GetCategoryNewsItemByCategoryId(categoryId, _workContext.WorkingLanguage.Id);
			if (newsItem == null)
				return null;
			var model = new NewsItemModel();
			PrepareNewsItemDetailModel(model, newsItem, false, false);
			return model;
		}
		private void PrepareNewsItemDetailModel(NewsItemModel model, NewsItem newsItem, bool prepareComments, bool prepareProductModel = true)
		{
			if (newsItem == null)
				throw new ArgumentNullException("newsItem");



			model.Id = newsItem.Id;
			model.SeName = newsItem.GetSeName();
			model.Title = newsItem.Title;
			model.Short = newsItem.Short;
			model.Full = newsItem.Full;
			model.MetaDescription = newsItem.MetaDescription;
			model.MetaTitle = newsItem.MetaTitle;
			model.MetaKeywords = newsItem.MetaKeywords;
			model.AllowComments = newsItem.AllowComments;
			model.CreatedOn = _dateTimeHelper.ConvertToUserTime(newsItem.CreatedOnUtc, DateTimeKind.Utc);
			model.NumberOfComments = newsItem.NewsComments.Count;

			#region pictures

			var pictures = _newsService.GetNewsItemPicturesByNewsItemId(newsItem.Id, NewsItemPictureType.Standard);
			if (pictures.Count > 0)
			{
				foreach (var newsItemPicture in pictures)
				{
					model.PictureModels.Add(new PictureModel()
					{
						ImageUrl =
							_pictureService.GetPictureUrl(newsItemPicture.PictureId,
														  _mediaSetting.NewsItemDetailPictureSize),
						FullSizeImageUrl = _pictureService.GetPictureUrl(newsItemPicture.PictureId),
						Title =
							string.Format(
								_localizationService.GetResource(
									"Media.Product.ImageLinkTitleFormat"), model.Title),
						AlternateText =
							string.Format(
								_localizationService.GetResource(
									"Media.Product.ImageAlternateTextFormat"), model.Title),

					});
				}
				model.DefaultPictureModel = model.PictureModels.FirstOrDefault();
			}
			else
			{
				//no images. set the default one
				model.DefaultPictureModel = new PictureModel()
				{
					ImageUrl =
						_pictureService.GetDefaultPictureUrl(
							_mediaSetting.NewsItemDetailPictureSize),
					FullSizeImageUrl = _pictureService.GetDefaultPictureUrl(),
					Title =
						string.Format(
							_localizationService.GetResource(
								"Media.Product.ImageLinkTitleFormat"), model.Title),
					AlternateText =
						string.Format(
							_localizationService.GetResource(
								"Media.Product.ImageAlternateTextFormat"), model.Title),
				};
			}

			#endregion pictures

			#region extra contents

			var extraContents = newsItem.ExtraContents.OrderBy(x => x.DisplayOrder).ToList();
			if (extraContents.Count > 0)
			{
				foreach (var newItemExtraContent in extraContents)
				{
					model.ExtraContentModels.Add(new ExtraContentModel()
					{
						FullDescription = newItemExtraContent.FullDescription,
						Id = newItemExtraContent.Id,
						DisplayOrder = newItemExtraContent.DisplayOrder,
						Title = newItemExtraContent.Title
					});
				}
			}



			#endregion extra contents

			#region comments

			if (prepareComments)
			{
				var newsComments = newsItem.NewsComments.Where(pr => pr.IsApproved).OrderBy(pr => pr.CreatedOnUtc);
				foreach (var nc in newsComments)
				{
					var commentModel = new NewsCommentModel()
					{
						Id = nc.Id,
						CustomerId = nc.CustomerId,
						CustomerName = nc.Customer.FormatUserName(),
						CommentTitle = nc.CommentTitle,
						CommentText = nc.CommentText,
						CreatedOn =
							_dateTimeHelper.ConvertToUserTime(nc.CreatedOnUtc, DateTimeKind.Utc),
						AllowViewingProfiles =
							_customerSettings.AllowViewingProfiles && nc.Customer != null &&
							!nc.Customer.IsGuest(),
					};
					if (_customerSettings.AllowCustomersToUploadAvatars)
					{
						var customer = nc.Customer;
						string avatarUrl =
							_pictureService.GetPictureUrl(
								customer.GetAttribute<int>(SystemCustomerAttributeNames.AvatarPictureId),
								_mediaSetting.AvatarPictureSize, false);
						if (String.IsNullOrEmpty(avatarUrl) && _customerSettings.DefaultAvatarEnabled)
							avatarUrl = _pictureService.GetDefaultPictureUrl(_mediaSetting.AvatarPictureSize,
																			 PictureType.Avatar);
						commentModel.CustomerAvatarUrl = avatarUrl;
					}
					model.Comments.Add(commentModel);
				}
			}

			#endregion comments

			#region products

			if (prepareProductModel)
			{
				var products = _newsService.GetNewsItemProductsByNewsItemId(newsItem.Id);
				model.ProductModels = products.Select(x => PrepareProductOverviewModel(x.Product)).ToList();
			}

			#endregion products



		}

		#endregion

		#region Manufacturers
		//Directly redirects to ManufacturerSe action
		public ActionResult Manufacturer(int manufacturerId, int? categoryId, CatalogExtendedPagingFilteringModel command)
		{

			var manufacturer = _manufacturerService.GetManufacturerById(manufacturerId);
            if (manufacturer == null || manufacturer.Deleted || !manufacturer.Published)
                return InvokeHttp404();

			return RedirectToAction("ManufacturerSe", new { SeName = manufacturer.GetLocalized(m => m.SeName) });
			_customerService.SaveCustomerAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.LastContinueShoppingPage, _webHelper.GetThisPageUrl(false));

			if (categoryId.HasValue)
			{


				command.CategoryFilter.AlreadyFilteredItems.Add(new CatalogExtendedPagingFilteringModel.CategoryFilterItem() { Id = categoryId.Value });
				var categories = _categoryService.GetAllCategoriesByParentCategoryId(categoryId.Value);
				foreach (var category in categories)
				{
					var subCategories = _categoryService.GetAllCategoriesByParentCategoryId(category.Id);
					if (subCategories.Count > 0)
					{
						foreach (var subCat in subCategories)
						{
							command.CategoryFilter.AlreadyFilteredItems.Add(new CatalogExtendedPagingFilteringModel.CategoryFilterItem() { Id = subCat.Id });
						}

					}
					else
					{
						command.CategoryFilter.AlreadyFilteredItems.Add(new CatalogExtendedPagingFilteringModel.CategoryFilterItem() { Id = category.Id });
					}
				}
				command.CategoryId = 0;
			}
			var model = manufacturer.ToModel();

			#region category breadcrumb
			model.DisplayCategoryBreadcrumb = _catalogSettings.CategoryBreadcrumbEnabled;
			if (model.DisplayCategoryBreadcrumb && categoryId.HasValue)
			{
				var category = _categoryService.GetCategoryById(categoryId.Value);
				foreach (var catBr in GetCategoryBreadCrumb(category))
				{
					model.CategoryBreadcrumb.Add(new CategoryModel()
					{
						Id = catBr.Id,
						Name = catBr.GetLocalized(x => x.Name),
						SeName = catBr.GetSeName()
					});
				}
				model.CategoryBreadcrumb.Add(model.CategoryBreadcrumb.Last());
			}
			#endregion category breadcrumb

			var baseProducts = _cacheManager.Get(string.Format(MANUFACTURER_MODEL_BY_ID_KEY, manufacturer.Id, _workContext.WorkingLanguage.Id, _workContext.WorkingCurrency.Id), int.MaxValue, () => GetProductsModel(command));

			if (command.PageSize <= 0) command.PageSize = manufacturer.PageSize;
			if (command.PageNumber <= 0) command.PageNumber = 1;
			var productsSet = GetFilteredProducts(command, baseProducts);
			model.Products = productsSet["all"];
			LoadFilteringContext(model, command, productsSet);
			model.PagingFilteringContext.CategoryFilter.AlreadyFilteredItems = new List<CatalogExtendedPagingFilteringModel.CategoryFilterItem>();
			SelectPagedProducts(model, command);

			//model.PagingFilteringContext.ViewMode = "grid2";

			//Customer Quute 
			SetProductVariantPriceModel(model.Products);
			model.IsGuest = _workContext.CurrentCustomer.IsGuest();
			return View(model);

		}

		public ActionResult ManufacturerSe280(string SeName, int? categoryId, CatalogPagingFilteringModel280 command)
		{
			GetCatalogPagingFilteringModel280FromQueryString(ref command);

			var manufacturer = _manufacturerService.GetManufacturerBySeName(SeName);
            if (manufacturer == null || manufacturer.Deleted || !manufacturer.Published)
                return InvokeHttp404();
			//'Continue shopping' URL
			_customerService.SaveCustomerAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.LastContinueShoppingPage, _webHelper.GetThisPageUrl(false));
			//for breadcrumb prev,next products
			StatefulStorage.PerSession.Add("navigation", () => "man-" + manufacturer.Id);

			if (command.PageNumber <= 0) command.PageNumber = 1;
			var model = manufacturer.ToModel280();
			//page size
			command.PageSize = manufacturer.PageSize;

			//price ranges
			decimal? minPriceConverted = null;
			decimal? maxPriceConverted = null;
			if (command.PriceRangeFilter.From.HasValue)
			{
				minPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(command.PriceRangeFilter.From.Value, _workContext.WorkingCurrency);
			}
			if (command.PriceRangeFilter.To.HasValue)
			{
				maxPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(command.PriceRangeFilter.To.Value, _workContext.WorkingCurrency);
			}

			#region category breadcrumb
			model.DisplayCategoryBreadcrumb = _catalogSettings.CategoryBreadcrumbEnabled;
			if (model.DisplayCategoryBreadcrumb && categoryId.HasValue)
			{
				var category = _categoryService.GetCategoryById(categoryId.Value);
				if (category == null || category.Deleted || !category.Published)
				{
                    return InvokeHttp404();
				}

				foreach (var catBr in GetCategoryBreadCrumb(category))
				{
					model.CategoryBreadcrumb.Add(new CategoryModel()
					{
						Id = catBr.Id,
						Name = catBr.GetLocalized(x => x.Name),
						SeName = catBr.GetSeName()
					});
				}
				model.CategoryBreadcrumb.Add(model.CategoryBreadcrumb.Last());
			}
			#endregion category breadcrumb


			//products
			IList<int> alreadyFilteredSpecOptionIds = command.SpecificationFilter.GetFilteredOptionIds();
			IList<int> alreadyFilteredProductVariantAttributeIds = command.AttributeFilter.GetProductVariantAttributeIds();
			IList<int> alreadyFilteredManufacturerIds = command.ManufacturerFilter.GetFilteredManufacturerIds();
			IList<int> alreadyFilteredCategoryIds = command.CategoryFilter.GetFilteredCategoryIds();
			var categoryIds = alreadyFilteredCategoryIds.ToList();
			if (categoryIds.Count == 0 && categoryId.HasValue)
			{
				categoryIds.Add(categoryId.Value);
				categoryIds.AddRange(GetChildCategoryIds(categoryId.Value));
			}

			string productListModelCacheKey = string.Format(PRODUCT_LIST_MODEL, string.Join(",", categoryIds), minPriceConverted, maxPriceConverted, _workContext.WorkingLanguage.Id
				, string.Join(",", alreadyFilteredSpecOptionIds), string.Join(",", alreadyFilteredProductVariantAttributeIds)
				, string.Join(",", alreadyFilteredManufacturerIds), command.OrderBy, command.PageNumber, command.PageSize, manufacturer.Id, currencyId);
			ProductListwithOverviewModel pm = new ProductListwithOverviewModel();

			if (Request.Params["cc"] != null && Request.Params["cc"].ToString().ToLowerInvariant().Equals("true"))
				_cacheManager.Remove(productListModelCacheKey);

			pm = _cacheManager.Get(productListModelCacheKey, int.MaxValue, () =>
			{
				IList<int> filterableSpecificationAttributeOptionIds = null;
				IList<int> filterableProductVariantAttributeIds = null;
				IList<int> filterableManufacturerIds = null;
				IList<int> filterableCategoryIds = null;
				decimal? maxPrice = null;

				pm.Products = _productService.SearchProductsFilterable(
					categoryIds, manufacturer.Id, null, minPriceConverted, maxPriceConverted,
					0, string.Empty, false, false, _workContext.WorkingLanguage.Id,
					alreadyFilteredSpecOptionIds, alreadyFilteredProductVariantAttributeIds,
					alreadyFilteredManufacturerIds, (ProductSortingEnum)command.OrderBy, command.PageNumber - 1, command.PageSize, true,
					out filterableSpecificationAttributeOptionIds,
					out filterableProductVariantAttributeIds,
					out filterableManufacturerIds,
					out filterableCategoryIds,
					out maxPrice);

				pm.FilterableSpecificationAttributeOptionIds = filterableSpecificationAttributeOptionIds;
				pm.FilterableProductVariantAttributeIds = filterableProductVariantAttributeIds;
				pm.FilterableManufacturerIds = filterableManufacturerIds;
				pm.FilterableCategoryIds = filterableCategoryIds;
				pm.MaxPrice = maxPrice;

				pm.ProductsOverview = PrepareProductOverviewModels280(pm.Products).ToList();
				return pm;
			});

			model.Products = pm.ProductsOverview;

			model.PagingFilteringContext.LoadPagedList(pm.Products);
			model.PagingFilteringContext.ViewMode = command.ViewMode;

			//specs
			model.PagingFilteringContext.SpecificationFilter.PrepareSpecsFilters(alreadyFilteredSpecOptionIds,
				pm.FilterableSpecificationAttributeOptionIds,
				_specificationAttributeService, _webHelper, _workContext);

			//attributes
			//model.PagingFilteringContext.AttributeFilter.PrepareAttributeFilters(alreadyFilteredProductVariantAttributeIds,
			//  filterableProductVariantAttributeIds,
			//  _productAttributeService, _webHelper, _workContext);

			//maufacturers
			model.PagingFilteringContext.ManufacturerFilter.PrepareManufacturerFilters(manufacturer.Id, alreadyFilteredManufacturerIds, pm.FilterableManufacturerIds, _manufacturerService, _webHelper, _workContext);

			//categories
			//var alreadyFilteredCategoryIds = command.CategoryFilter.FilteredCategoryIds;
			model.PagingFilteringContext.CategoryFilter.PrepareCategoryFilters(alreadyFilteredCategoryIds, pm.FilterableCategoryIds, _categoryService, _webHelper, _workContext);

			//pricerange
			model.PagingFilteringContext.PriceRangeFilter.PreparePriceRangeFilters(0, pm.MaxPrice, command.PriceRangeFilter.From, command.PriceRangeFilter.To, _webHelper, _workContext);


			//if (command.FilterTrigging == "C" || command.FilterTrigging == "M" || command.FilterTrigging == "S" || command.FilterTrigging == "A")
			//    model.PagingFilteringContext.ShowFilteringPanel = true;
			//else
			//    model.PagingFilteringContext.ShowFilteringPanel = false;


			//news content
			//TODO:280 cache here
			model.NewsItemStartPoint = _catalogSettings.ProductListBannerStartpoint;
			model.NewsItemModel = GetManufacturerNewsItemModel(manufacturer.Id);

			model.IsGuest = _workContext.CurrentCustomer.IsGuest();

			return View("Manufacturer280", model);
		}
		public JsonResult ManufacturerJS280(int manufacturerId, CatalogPagingFilteringModel280 command)
		{
			var manufacturer = _manufacturerService.GetManufacturerById(manufacturerId);
			if (manufacturer == null || manufacturer.Deleted || !manufacturer.Published)
                return Json(new { success = false, Reaction = "Redirect", Url = Url.Action("PageNotFound", "Common"), });

			if (command.PageNumber <= 0) command.PageNumber = 1;
			var model = manufacturer.ToModel280();

			//page size
			command.PageSize = manufacturer.PageSize;

			//price ranges
			decimal? minPriceConverted = null;
			decimal? maxPriceConverted = null;
			if (command.PriceRangeFilter.From.HasValue)
			{
				minPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(command.PriceRangeFilter.From.Value, _workContext.WorkingCurrency);
			}
			if (command.PriceRangeFilter.To.HasValue)
			{
				maxPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(command.PriceRangeFilter.To.Value, _workContext.WorkingCurrency);
			}
			var categoryIds = command.CategoryFilter.FilteredCategoryIds;

			//products
			IList<int> alreadyFilteredSpecOptionIds = command.SpecificationFilter.GetFilteredOptionIds();
			IList<int> alreadyFilteredProductVariantAttributeIds = command.AttributeFilter.GetProductVariantAttributeIds();
			IList<int> alreadyFilteredManufacturerIds = command.ManufacturerFilter.GetFilteredManufacturerIds();


			string productListModelCacheKey = string.Format(PRODUCT_LIST_MODEL, string.Join(",", categoryIds), minPriceConverted, maxPriceConverted, _workContext.WorkingLanguage.Id
				, string.Join(",", alreadyFilteredSpecOptionIds), string.Join(",", alreadyFilteredProductVariantAttributeIds)
				, string.Join(",", alreadyFilteredManufacturerIds), command.OrderBy, command.PageNumber, command.PageSize, manufacturer.Id, currencyId);
			ProductListwithOverviewModel pm = new ProductListwithOverviewModel();
			pm = _cacheManager.Get(productListModelCacheKey, () =>
			{
				IList<int> filterableSpecificationAttributeOptionIds = null;
				IList<int> filterableProductVariantAttributeIds = null;
				IList<int> filterableManufacturerIds = null;
				IList<int> filterableCategoryIds = null;
				decimal? maxPrice = null;

				pm.Products = _productService.SearchProductsFilterable(
					categoryIds, manufacturer.Id, null, minPriceConverted, maxPriceConverted,
					0, string.Empty, false, false, _workContext.WorkingLanguage.Id,
					alreadyFilteredSpecOptionIds, alreadyFilteredProductVariantAttributeIds,
					alreadyFilteredManufacturerIds, (ProductSortingEnum)command.OrderBy, command.PageNumber - 1, command.PageSize, true,
					out filterableSpecificationAttributeOptionIds,
					out filterableProductVariantAttributeIds,
					out filterableManufacturerIds,
					out filterableCategoryIds,
					out maxPrice);

				pm.FilterableSpecificationAttributeOptionIds = filterableSpecificationAttributeOptionIds;
				pm.FilterableProductVariantAttributeIds = filterableProductVariantAttributeIds;
				pm.FilterableManufacturerIds = filterableManufacturerIds;
				pm.FilterableCategoryIds = filterableCategoryIds;
				pm.MaxPrice = maxPrice;

				pm.ProductsOverview = PrepareProductOverviewModels280(pm.Products).ToList();
				return pm;
			});

			model.Products = pm.ProductsOverview;


			model.PagingFilteringContext.LoadPagedList(pm.Products);
			//model.PagingFilteringContext.ViewMode = viewMode;

			//specs
			model.PagingFilteringContext.SpecificationFilter.PrepareSpecsFilters(alreadyFilteredSpecOptionIds,
				pm.FilterableSpecificationAttributeOptionIds,
				_specificationAttributeService, _webHelper, _workContext);

			//attributes
			//model.PagingFilteringContext.AttributeFilter.PrepareAttributeFilters(alreadyFilteredProductVariantAttributeIds,
			//  filterableProductVariantAttributeIds,
			//  _productAttributeService, _webHelper, _workContext);

			//maufacturers
			model.PagingFilteringContext.ManufacturerFilter.PrepareManufacturerFilters(manufacturer.Id, alreadyFilteredManufacturerIds, pm.FilterableManufacturerIds, _manufacturerService, _webHelper, _workContext);

			//categories
			var alreadyFilteredCategoryIds = command.CategoryFilter.FilteredCategoryIds;
			model.PagingFilteringContext.CategoryFilter.PrepareCategoryFilters(alreadyFilteredCategoryIds, pm.FilterableCategoryIds, _categoryService, _webHelper, _workContext);

			//pricerange
			model.PagingFilteringContext.PriceRangeFilter.PreparePriceRangeFilters(0, pm.MaxPrice, command.PriceRangeFilter.From, command.PriceRangeFilter.To, _webHelper, _workContext);

			if (command.FilterTrigging == "C" || command.FilterTrigging == "M" || command.FilterTrigging == "S" || command.FilterTrigging == "A")
				model.PagingFilteringContext.ShowFilteringPanel = true;
			else
				model.PagingFilteringContext.ShowFilteringPanel = false;

			string view = null;
			if (command.ViewMode == "grid1")
				view = @"~/Views/Catalog/ProductsList1280.cshtml";
			else if (command.ViewMode == "grid2")
				view = @"~/Views/Catalog/ProductsList2280.cshtml";
			else
				view = @"~/Views/Catalog/ProductsList3280.cshtml";

			return Json(new
			{
				Html = Utilities.RenderPartialViewToString(this, view, model),
				SelectionHtml = Utilities.RenderPartialViewToString(this, "~/Views/Catalog/_yourSelection280.cshtml", model.PagingFilteringContext),
				PriceMin = model.PagingFilteringContext.PriceRangeFilter.Min,
				PriceMax = model.PagingFilteringContext.PriceRangeFilter.Max,
				PriceValue = model.PagingFilteringContext.PriceRangeFilter.To,
				PriceStep = model.PagingFilteringContext.PriceRangeFilter.StepSize,
				HasMore = model.PagingFilteringContext.HasNextPage,
				PageNumber = model.PagingFilteringContext.PageNumber,
				PageSize = model.PagingFilteringContext.PageSize,
				ViewMode = command.ViewMode
			});
		}

		public ActionResult ManufacturerSe(string SeName, int? categoryId, CatalogExtendedPagingFilteringModel command)
		{
			var manufacturer = _manufacturerService.GetManufacturerBySeName(SeName);
            if (manufacturer == null || manufacturer.Deleted || !manufacturer.Published)
                return InvokeHttp404();
			_customerService.SaveCustomerAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.LastContinueShoppingPage, _webHelper.GetThisPageUrl(false));
			command.ManufacturerId = manufacturer.Id;

			if (categoryId.HasValue)
			{
				var cat = _categoryService.GetCategoryById(categoryId.Value);
                if (cat == null || cat.Deleted || !cat.Published)
                    return InvokeHttp404();
				command.CategoryFilter.AlreadyFilteredItems.Add(new CatalogExtendedPagingFilteringModel.CategoryFilterItem() { Id = categoryId.Value });
				var categories = _categoryService.GetAllCategoriesByParentCategoryId(categoryId.Value);
				foreach (var category in categories)
				{
					var subCategories = _categoryService.GetAllCategoriesByParentCategoryId(category.Id);
					if (subCategories.Count > 0)
					{
						foreach (var subCat in subCategories)
						{
							command.CategoryFilter.AlreadyFilteredItems.Add(new CatalogExtendedPagingFilteringModel.CategoryFilterItem() { Id = subCat.Id });
						}

					}
					else
					{
						command.CategoryFilter.AlreadyFilteredItems.Add(new CatalogExtendedPagingFilteringModel.CategoryFilterItem() { Id = category.Id });
					}
				}
				command.CategoryId = 0;
			}
			var model = manufacturer.ToModel();

			#region category breadcrumb
			model.DisplayCategoryBreadcrumb = _catalogSettings.CategoryBreadcrumbEnabled;
			if (model.DisplayCategoryBreadcrumb && categoryId.HasValue)
			{
				var category = _categoryService.GetCategoryById(categoryId.Value);
				foreach (var catBr in GetCategoryBreadCrumb(category))
				{
					model.CategoryBreadcrumb.Add(new CategoryModel()
					{
						Id = catBr.Id,
						Name = catBr.GetLocalized(x => x.Name),
						SeName = catBr.GetSeName()
					});
				}
				model.CategoryBreadcrumb.Add(model.CategoryBreadcrumb.Last());
			}
			#endregion category breadcrumb

			var baseProducts = _cacheManager.Get(string.Format(MANUFACTURER_MODEL_BY_ID_KEY, manufacturer.Id, _workContext.WorkingLanguage.Id, _workContext.WorkingCurrency.Id), int.MaxValue, () => GetProductsModel(command));

			if (command.PageSize <= 0) command.PageSize = manufacturer.PageSize;
			if (command.PageNumber <= 0) command.PageNumber = 1;
			var productsSet = GetFilteredProducts(command, baseProducts);
			model.Products = productsSet["all"];
			LoadFilteringContext(model, command, productsSet);
			model.PagingFilteringContext.CategoryFilter.AlreadyFilteredItems = new List<CatalogExtendedPagingFilteringModel.CategoryFilterItem>();
			SelectPagedProducts(model, command);
			//model.PagingFilteringContext.ViewMode = "grid2";
			model.NewsItemStartPoint = _catalogSettings.ProductListBannerStartpoint;
			//newsItem
			model.NewsItemModel = GetManufacturerNewsItemModel(manufacturer.Id);

			//Customer Quute 
			SetProductVariantPriceModel(model.Products);
			model.IsGuest = _workContext.CurrentCustomer.IsGuest();
			return View("Manufacturer", model);

		}
		[AjaxOnly]
		public JsonResult ManufacturerJS(CatalogExtendedPagingFilteringModel command)
		{
			var manufacturer = _manufacturerService.GetManufacturerById(command.ManufacturerId);
			if (manufacturer == null || manufacturer.Deleted || !manufacturer.Published)
                return Json(new { success = false, Reaction = "Redirect", Url = Url.Action("PageNotFound", "Common"), });

			var model = manufacturer.ToModel();
			var baseProducts = _cacheManager.Get(string.Format(MANUFACTURER_MODEL_BY_ID_KEY, manufacturer.Id, _workContext.WorkingLanguage.Id, _workContext.WorkingCurrency.Id), int.MaxValue, () => GetProductsModel(command));

			if (command.PageSize <= 0) command.PageSize = manufacturer.PageSize;
			if (command.PageNumber <= 0) command.PageNumber = 1;

			var productsSet = GetFilteredProducts(command, baseProducts);
			model.Products = productsSet["all"];
			LoadFilteringContext(model, command, productsSet);
			SelectPagedProducts(model, command);
			string view = null;
			if (command.ViewMode == "grid1")
				view = @"~/Views/Catalog/ProductsList1.cshtml";
			else if (command.ViewMode == "grid2")
				view = @"~/Views/Catalog/ProductsList2.cshtml";
			else
				view = @"~/Views/Catalog/ProductsList3.cshtml";

			//Customer Quate 
			SetProductVariantPriceModel(model.Products);
			return Json(new
			{
				Html = Utilities.RenderPartialViewToString(this, view, model),
				SelectionHtml = Utilities.RenderPartialViewToString(this, "~/Views/Catalog/_yourSelection.cshtml", model.PagingFilteringContext),
				PriceMin = model.PagingFilteringContext.PriceRangeSliderFilter.Item.Min,
				PriceMax = model.PagingFilteringContext.PriceRangeSliderFilter.Item.Max,
				PriceValue = model.PagingFilteringContext.PriceRangeSliderFilter.Item.Value,
				PriceStep = model.PagingFilteringContext.PriceRangeSliderFilter.Item.StepSize,
				HasMore = model.PagingFilteringContext.HasNextPage,
				PageNumber = model.PagingFilteringContext.PageNumber,
				PageSize = model.PagingFilteringContext.PageSize,
				ViewMode = command.ViewMode

			});

		}

		//AF
		[OutputCache(Duration = int.MaxValue, VaryByCustom = "lgg")]
		public ActionResult ManufacturerAll()
		{
			var model = _cacheManager.Get(string.Format(ALL_MANUFACTURERS_HOME_KEY, _workContext.WorkingLanguage.Id), int.MaxValue, () =>
			{
				var categories = _categoryService.GetAllCategoriesDisplayedOnHomePage();
				var categoryModels = categories.Select(x => x.ToModel()).ToList();
				foreach (var categoryModel in categoryModels)
				{
					var items = GetProductsManufacturers(GetCategoryProducts(categoryModel.Id, true)).Select(x => x.ToModel()).ToList();
					categoryModel.ManufacturerModels = new List<ManufacturerModel>(items);
				}
				return categoryModels.ToList();
			});
			ViewBag.IsGuest = _workContext.CurrentCustomer.IsGuest();
			return View(model);
		}

		[OutputCache(Duration = int.MaxValue, VaryByCustom = "lgg")]
		public ActionResult FilteredManufacturerAll(string filter)
		{
			if (string.IsNullOrWhiteSpace(filter))
				return RedirectToAction("ManufacturerAll");

			var model = _cacheManager.Get(string.Format(ALL_MANUFACTURERS, _workContext.WorkingLanguage.Id), int.MaxValue, () =>
			{
				var manufacturers = _manufacturerService.GetAllManufacturers();
				var manufacturerModels = new List<ManufacturerModel>();
				foreach (var manufacturer in manufacturers)
				{
					var manufacturerModel = manufacturer.ToModel();
					manufacturerModel.PictureModel.ImageUrl = _pictureService.GetPictureUrl(manufacturer.PictureId, _mediaSetting.ProductThumbPictureSize, true);
					manufacturerModel.PictureModel.Title = string.Format(_localizationService.GetResource("Media.Manufacturer.ImageLinkTitleFormat"), manufacturer.Name);
					manufacturerModel.PictureModel.AlternateText = string.Format(_localizationService.GetResource("Media.Manufacturer.ImageAlternateTextFormat"), manufacturer.Name);
					manufacturerModels.Add(manufacturerModel);
				}
				return manufacturerModels;
			});

			return View(model.Where(x => x.Name.StartsWith(filter, StringComparison.InvariantCultureIgnoreCase)).ToList());
		}


		[ChildActionOnly]
		public ActionResult ManufacturerNavigation(int currentManufacturerId)
		{
			var currentManufacturer = _manufacturerService.GetManufacturerById(currentManufacturerId);

			var model = new List<ManufacturerNavigationModel>();
			foreach (var manufacturer in _manufacturerService.GetAllManufacturers())
			{
				var modelMan = new ManufacturerNavigationModel()
				{
					Id = manufacturer.Id,
					Name = manufacturer.GetLocalized(x => x.Name),
					SeName = manufacturer.GetSeName(),
					IsActive = currentManufacturer != null && currentManufacturer.Id == manufacturer.Id,
				};
				model.Add(modelMan);
			}

			return PartialView(model);
		}

		//AF
		[ChildActionOnly]
		[OutputCache(Duration = int.MaxValue, VaryByParam = "categoryId", VaryByCustom = "lgg")]
		public ActionResult ManufacturerCategories(int categoryId)
		{
			var categories = _categoryService.GetAllCategoriesDisplayedOnHomePage();
			var categoryModels = categories.Select(x => x.ToModel()).ToList();
			categoryModels.ForEach(x => { if (x.Id == categoryId)x.Selected = true; });
			//ViewBag.IsGuest = _workContext.CurrentCustomer.IsGuest();
			return View(categoryModels);
		}
		//AF
		[OutputCache(Duration = int.MaxValue, VaryByParam = "categoryId", VaryByCustom = "lgg")]
		public ActionResult CategoryManufacturers(int categoryId)
		{
			var category = _categoryService.GetCategoryById(categoryId);
			if (category == null) return RedirectToAction("ManufacturerAll");
			var model = new CategoryManufacturerModel();
			model.Category = category.ToModel();
			ManufacturerModel manufacturerModel = null;
			//foreach (var manufacturer in GetProductsManufacturers(GetCategoryProducts(categoryId, true)))

			PictureRepository pr = new PictureRepository();
			var pics = pr.GetAllmanufacturerPictures();

			foreach (var manufacturer in _manufacturerService.GetManufacturerByCategoryIds(GetChildCategoryIds(categoryId)).OrderBy(a => a.DisplayOrder))
			{
				manufacturerModel = manufacturer.ToModel();
				manufacturerModel.PictureModel.ImageUrl = _pictureService.GetPictureUrl(pics.FirstOrDefault(x => x.Id == manufacturer.PictureId), _mediaSetting.ProductThumbPictureSize, true);
				manufacturerModel.PictureModel.Title = string.Format(_localizationService.GetResource("Media.Manufacturer.ImageLinkTitleFormat"), manufacturer.Name);
				manufacturerModel.PictureModel.AlternateText = string.Format(_localizationService.GetResource("Media.Manufacturer.ImageAlternateTextFormat"), manufacturer.Name);
				model.Manufacturers.Add(manufacturerModel);
			}
			model.IsGuest = _workContext.CurrentCustomer.IsGuest();

			return View(model);
		}

		[OutputCache(Duration = int.MaxValue, VaryByParam = "mLetter", VaryByCustom = "lgg")]
		public ActionResult ManufacturersByStartingLetter(string mLetter)
		{
			if (mLetter == null || mLetter.Equals(string.Empty)) return RedirectToAction("ManufacturerAll");

			var model = new CategoryManufacturerModel();
			ManufacturerModel manufacturerModel = null;

			PictureRepository pr = new PictureRepository();
			var pics = pr.GetAllmanufacturerPictures();

			foreach (var manufacturer in new CatalogRepository().ManufacturersByStartingLetter(mLetter).OrderBy(a => a.DisplayOrder))
			{
				manufacturerModel = manufacturer.ToModel();
				manufacturerModel.PictureModel.ImageUrl = _pictureService.GetPictureUrl(pics.FirstOrDefault(x => x.Id == manufacturer.PictureId), _mediaSetting.ProductThumbPictureSize, true);
				manufacturerModel.PictureModel.Title = string.Format(_localizationService.GetResource("Media.Manufacturer.ImageLinkTitleFormat"), manufacturer.Name);
				manufacturerModel.PictureModel.AlternateText = string.Format(_localizationService.GetResource("Media.Manufacturer.ImageAlternateTextFormat"), manufacturer.Name);
				model.Manufacturers.Add(manufacturerModel);
			}
			model.IsGuest = _workContext.CurrentCustomer.IsGuest();

			return View(model);
		}

		//TODO: cache
        [OutputCache(Duration = 86400, VaryByParam = "none", VaryByCustom = "lgg")]
		public ActionResult CategoryManufacturersAll()
		{
            if (Request.Params["cc"] != null && Request.Params["cc"].ToString().ToLowerInvariant().Equals("true"))
            {
                _cacheManager.Remove(ModelCacheEventConsumer.AF_MANUFACTURERS_PAGE_MANPICTURES);
                _cacheManager.Remove(string.Format(ModelCacheEventConsumer.AF_MANUFACTURERS_PAGE, _workContext.WorkingLanguage.Id));
            }

			var model = new CategoryManufacturerModel();
			ManufacturerModel manufacturerModel = null;

            PictureRepository pr = new PictureRepository();
            //var pics = _cacheManager.Get(ModelCacheEventConsumer.AF_MANUFACTURERS_PAGE_MANPICTURES, 3600, () =>
            //{
            //    return pr.GetAllmanufacturerPictures();
            //});
            var pics = pr.GetAllmanufacturerPictures();

            //var manufacturersAll = _cacheManager.Get(string.Format(ModelCacheEventConsumer.AF_MANUFACTURERS_PAGE, _workContext.WorkingLanguage.Id), 3600, () =>
            //{
            //    return _manufacturerService.GetAllManufacturers();
            //});
            var manufacturersAll = _manufacturerService.GetAllManufacturers();

			foreach (var manufacturer in manufacturersAll)
			{
				manufacturerModel = manufacturer.ToModel();
				//manufacturerModel.PictureModel.ImageUrl = _pictureService.GetPictureUrl(manufacturer.PictureId, _mediaSetting.ProductThumbPictureSize, true);
				manufacturerModel.PictureModel.ImageUrl = _pictureService.GetPictureUrl(pics.FirstOrDefault(x => x.Id == manufacturer.PictureId), _mediaSetting.ProductThumbPictureSize, true);
				manufacturerModel.PictureModel.Title = string.Format(_localizationService.GetResource("Media.Manufacturer.ImageLinkTitleFormat"), manufacturer.Name);
				manufacturerModel.PictureModel.AlternateText = string.Format(_localizationService.GetResource("Media.Manufacturer.ImageAlternateTextFormat"), manufacturer.Name);
				model.Manufacturers.Add(manufacturerModel);
			}
			model.Category = new CategoryModel() { Id = 0 };
			model.IsGuest = _workContext.CurrentCustomer.IsGuest();

			return View("CategoryManufacturers", model);
		}

		#endregion

		#region Products

		//AF
		//product details page
		[OutputCache(Location = OutputCacheLocation.Server, VaryByParam = "productId;variantId", Duration = int.MaxValue)]
		public ActionResult Product(int productId, int? variantId = 0)
		{
			var product = _productService.GetProductById(productId);
            if (product == null || product.Deleted || !product.Published)
                return InvokeHttp404();

			//can be used from cache!
			//var model = _cacheManager.Get(string.Format(PRODUCT_MODEL_BY_ID_KEY, product.Id, _workContext.WorkingLanguage.Id, _workContext.WorkingCurrency.Id), int.MaxValue,
			//                            () => PrepareProductDetailsPageModel(product));


			var model = PrepareProductDetailsPageModel(product, variantId.Value);
			//
            if (model == null)
                return InvokeHttp404();
			//save as recently viewed
			//_recentlyViewedProductsService.AddProductToRecentlyViewedList(product.Id);

			//activity log
			_customerActivityService.InsertActivity("PublicStore.ViewProduct", _localizationService.GetResource("ActivityLog.PublicStore.ViewProduct"), product.Id + ": " + product.Name);

			return View(model);
		}


		[ChildActionOnly]
		public ActionResult SizeChart(string systemName)
		{

			var topic = _topicService.GetTopicBySystemName(systemName);
			if (topic == null)
				return Content("");

			var model = topic.ToModel();
			if (model.IsPasswordProtected)
			{
				model.Title = string.Empty;
				model.Body = string.Empty;
			}

			return PartialView(model);
		}

		[ChildActionOnly]
		public ActionResult ProductBreadcrumb(int productId)
		{
			var product = _productService.GetProductById(productId);
			if (product == null)
				throw new ArgumentException("No product found with the specified id");

			var model = new ProductModel.ProductBreadcrumbModel()
			{
				DisplayBreadcrumb = _catalogSettings.CategoryBreadcrumbEnabled,
				ProductId = product.Id,
				ProductName = product.GetLocalized(x => x.Name),
				ProductSeName = product.GetSeName(),
				PreviousListUrl = HttpContext.Request.UrlReferrer == null ? "" :
				_workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.LastContinueShoppingPage)
			};
			if (model.DisplayBreadcrumb)
			{
				var productCategories = _categoryService.GetProductCategoriesByProductId(product.Id);
				if (productCategories.Count > 0)
				{
					var category = productCategories[0].Category;
					if (category != null)
					{
						foreach (var catBr in GetCategoryBreadCrumb(category))
						{
							model.CategoryBreadcrumb.Add(new CategoryModel()
							{
								Id = catBr.Id,
								Name = catBr.GetLocalized(x => x.Name),
								SeName = catBr.GetSeName()
							});
						}
					}
					int[] baseProductIds = null;
					if (Session["productList"] == null)
					{
						if (category != null)
							baseProductIds = _cacheManager.Get(string.Format(CATEGORY_MODEL_BY_ID_KEY, category.Id, _workContext.WorkingLanguage.Id, _workContext.WorkingCurrency.Id), int.MaxValue, () => GetProductsModel(new CatalogExtendedPagingFilteringModel() { CategoryId = category.Id })).Select(x => x.Id).ToArray();
					}
					else
						baseProductIds = Session["productList"] as int[];
					var currentProductId = baseProductIds.FirstOrDefault(x => x == product.Id);
					var index = Array.IndexOf(baseProductIds, currentProductId);
					int prevProductId;
					int nextProductId;
					if (index > 0)
					{
						prevProductId = baseProductIds.ElementAt(index - 1);
						var prevProduct = _productService.GetProductById(prevProductId);
						model.PreviousProductId = prevProduct.Id;
						model.PreviousProductSeName = prevProduct.GetSeName();
					}
					if (index < baseProductIds.Length - 1)
					{
						nextProductId = baseProductIds.ElementAt(index + 1);
						var nextProduct = _productService.GetProductById(nextProductId);
						model.NextProductId = nextProduct.Id;
						model.NextProductSeName = nextProduct.GetSeName();
					}

				}
			}
			return PartialView(model);
		}

		[ChildActionOnly]
		public ActionResult ProductBreadcrumb280(int productId)
		{
			var product = _productService.GetProductById(productId);
			if (product == null)
				throw new ArgumentException("No product found with the specified id");

			var model = new ProductModel.ProductBreadcrumbModel()
			{
				DisplayBreadcrumb = true,
				ProductId = product.Id,
				ProductName = product.GetLocalized(x => x.Name),
				ProductSeName = product.GetSeName(),
				PreviousListUrl = HttpContext.Request.UrlReferrer == null ? "" :
				_workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.LastContinueShoppingPage)
			};
			if (model.DisplayBreadcrumb)
			{
				var productCategories = _categoryService.GetProductCategoriesByProductId(product.Id);
				if (productCategories.Count > 0)
				{
					var category = productCategories[0].Category;
					if (category != null)
					{
						foreach (var catBr in GetCategoryBreadCrumb(category))
						{
							model.CategoryBreadcrumb.Add(new CategoryModel()
							{
								Id = catBr.Id,
								Name = catBr.GetLocalized(x => x.Name),
								SeName = catBr.GetSeName()
							});
						}
					}
					var navigation = StatefulStorage.PerSession.Get<string>("navigation");

					int? manufacturerId = null;
					int? categoryId = null;
					if (!string.IsNullOrEmpty(navigation))
					{
						var segments = navigation.Split(new char[] { '-' });
						if (segments[0] == "man")
						{
							manufacturerId = int.Parse(segments[1]);
							if (product.ProductManufacturers.FirstOrDefault(x => x.ManufacturerId == manufacturerId) == null)
								manufacturerId = null;
						}
						else if (segments[0] == "cat")
						{
							categoryId = int.Parse(segments[1]);
							if (product.ProductCategories.FirstOrDefault(x => x.CategoryId == categoryId) == null)
								categoryId = null;
						}
					}

					int index = 0;
					Product prevProduct = null;
					Product nextProduct = null;
					if (manufacturerId.HasValue)
					{
						var cpm = _manufacturerService.GetProductManufacturersByManufacturerId(manufacturerId.Value);
						index = cpm.IndexOf(cpm.FirstOrDefault(x => x.ProductId == productId));
						if (index > 0)
						{
							prevProduct = cpm.ElementAt(index - 1).Product;
							model.PreviousProductId = prevProduct.Id;
							model.PreviousProductSeName = prevProduct.GetSeName();
						}
						if (index < cpm.Count - 1)
						{
							nextProduct = cpm.ElementAt(index + 1).Product;
							model.NextProductId = nextProduct.Id;
							model.NextProductSeName = nextProduct.GetSeName();
						}
					}
					else
					{
						int catId = 0;
						if (categoryId.HasValue)
						{
							catId = categoryId.Value;
						}
						else
						{
							var cp = product.ProductCategories.FirstOrDefault();
							catId = cp != null ? cp.CategoryId : 0;
						}
						if (catId != 0)
						{

							var cpm = _categoryService.GetProductCategoriesByCategoryId(catId);
							index = cpm.IndexOf(cpm.FirstOrDefault(x => x.ProductId == productId));
							if (index > 0)
							{
								prevProduct = cpm.ElementAt(index - 1).Product;
								model.PreviousProductId = prevProduct.Id;
								model.PreviousProductSeName = prevProduct.GetSeName();
							}
							if (index < cpm.Count - 1)
							{
								nextProduct = cpm.ElementAt(index + 1).Product;
								model.NextProductId = nextProduct.Id;
								model.NextProductSeName = nextProduct.GetSeName();
							}
						}
					}
				}
			}
			return PartialView(model);
		}

		[ChildActionOnly]
		public ActionResult ProductManufacturers(int productId)
		{
			var model = _manufacturerService.GetProductManufacturersByProductId(productId)
				.Select(x =>
				{
					var m = x.Manufacturer.ToModel();
					m.PictureModel.ImageUrl = _pictureService.GetPictureUrl(x.Manufacturer.PictureId, _mediaSetting.CategoryThumbPictureSize, true);
					m.PictureModel.Title = string.Format(_localizationService.GetResource("Media.Manufacturer.ImageLinkTitleFormat"), m.Name);
					m.PictureModel.AlternateText = string.Format(_localizationService.GetResource("Media.Manufacturer.ImageAlternateTextFormat"), m.Name);
					return m;
				})
				.ToList();

			return PartialView(model);
		}

		[ChildActionOnly]
		public ActionResult ProductReviewOverview(int productId)
		{
			var product = _productService.GetProductById(productId);
			if (product == null)
				throw new ArgumentException("No product found with the specified id");

			var model = new ProductReviewOverviewModel()
			{
				ProductId = product.Id,
				RatingSum = product.ApprovedRatingSum,
				TotalReviews = product.ApprovedTotalReviews,
				AllowCustomerReviews = product.AllowCustomerReviews
			};
			return PartialView(model);
		}

		[ChildActionOnly]
		public ActionResult ProductSpecifications(int productId)
		{
			var product = _productService.GetProductById(productId);
			if (product == null)
				throw new ArgumentException("No product found with the specified id");

			var model = _specificationAttributeService.GetProductSpecificationAttributesByProductId(product.Id, null, true)
				.Select(psa =>
				{
					return new ProductSpecificationModel()
					{
						SpecificationAttributeId = psa.SpecificationAttributeOption.SpecificationAttributeId,
						SpecificationAttributeName = psa.SpecificationAttributeOption.SpecificationAttribute.GetLocalized(x => x.Name),
						SpecificationAttributeOption = psa.SpecificationAttributeOption.GetLocalized(x => x.Name),
						Position = psa.SpecificationAttributeOption.DisplayOrder
					};
				})
				.ToList();

			return PartialView(model);
		}

		[ChildActionOnly]
		public ActionResult ProductTierPrices(int productVariantId)
		{
			if (_permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
			{
				var variant = _productService.GetProductVariantById(productVariantId);
				if (variant == null)
					throw new ArgumentException("No product variant found with the specified id");

				var model = _productService.GetTierPricesByProductVariantId(productVariantId)
					.FilterForCustomer(_workContext.CurrentCustomer)
					.Select(tierPrice =>
					{
						var m = new ProductModel.ProductVariantModel.TierPriceModel()
						{
							Quantity = tierPrice.Quantity,
						};
						decimal taxRate = decimal.Zero;
						decimal priceBase = _taxService.GetProductPrice(variant, tierPrice.Price, out taxRate);
						decimal price = _currencyService.ConvertFromPrimaryStoreCurrency(priceBase, _workContext.WorkingCurrency);
						m.Price = _priceFormatter.FormatPrice(price, false, false);

						return m;
					})
					.ToList();

				return PartialView(model);
			}
			else
			{
				//hide prices
				return Content("");
			}
		}

		//AF
		//TODO: mustafa get count from settings.
		[ChildActionOnly]
		public ActionResult RelatedProducts(int categoryId, int productId)
		{
			IList<Product> products = new List<Product>();

			var list = _productService.GetRandomRelatedProductByCategoryId(categoryId, _catalogSettings.RandomRelatedProductCount);

			if (list.Count < _catalogSettings.RandomRelatedProductCount)
			{
				var categoryProduct = _categoryService.GetProductCategoriesByProductId(productId).FirstOrDefault();//ürünün bulundugu kategorilerin ilkini döndürür

				if (categoryProduct != null)
					products = _categoryService.GetRandomProductsByCategoryId(categoryProduct.CategoryId, _catalogSettings.RandomRelatedProductCount, productId);
			}


			//else if (list.Count >= 0 && list.Count <= 6)
			//{
			//    var categoryProduct = _categoryService.GetProductCategoriesByProductId(productId).FirstOrDefault();

			//    if (categoryProduct != null)
			//    {
			//        var category = _categoryService.GetCategoryById(categoryProduct.CategoryId);
			//        if (category != null)
			//        {
			//            products = _categoryService.GetRandomProductsByCategoryId(category.ParentCategoryId, _catalogSettings.RandomRelatedProductCount);
			//        }
			//    }
			//}



			//else
			//{
			//    foreach (var rp in list)
			//    {
			//        var product = _productService.GetProductById(rp.ProductId2);
			//        if (product == null)
			//            continue;

			//        //ensure that a related product has at least one available variant
			//        var variants = _productService.GetProductVariantsByProductId(product.Id);
			//        if (variants.Count > 0)
			//            products.Add(product);
			//    }
			//}


			var model = list.Select(x => PrepareProductOverviewModel(x, false)).ToList();

			return PartialView(model);
		}

		//AF
		[ChildActionOnly]
		public ActionResult SuggestedProducts(int categoryId, int productId)
		{
			IList<Product> products = new List<Product>();

			var allCategories = _categoryService.GetAllCategoriesIncludeChildNode();
			var category = _categoryService.GetCategoryById(categoryId);

			List<int> categoryIdList = new List<int>();

			categoryIdList = allCategories.Where(x => x.ParentCategoryId == category.ParentCategoryId).Where(x => x.Id != category.Id).Select(x => x.Id).ToList();

			products = _categoryService.GetRandomCrossSellProductsByCategoryId(categoryIdList, _catalogSettings.RandomCrossSellProductCount, productId);

			var model = products.Select(x => PrepareProductOverviewModel(x, false)).ToList();

			return PartialView(model);

		}

		[ChildActionOnly]
		public ActionResult ProductsAlsoPurchased(int productId)
		{
			if (!_catalogSettings.ProductsAlsoPurchasedEnabled)
				return Content("");

			var product = _productService.GetProductById(productId);
			if (product == null)
				throw new ArgumentException("No product found with the specified id");

			var products = _orderReportService.GetProductsAlsoPurchasedById(productId,
				_catalogSettings.ProductsAlsoPurchasedNumber);

			var model = products.Select(x => PrepareProductOverviewModel(x)).ToList();

			return PartialView(model);
		}

		[ChildActionOnly]
		public ActionResult ShareButton()
		{
			var shareCode = _catalogSettings.PageShareCode;
			if (_webHelper.IsCurrentConnectionSecured())
			{
				//need to change the addthis link to be https linked when the page is, so that the page doesnt ask about mixed mode when viewed in https...
				shareCode = shareCode.Replace("http://", "https://");
			}

			return PartialView("ShareButton", shareCode);
		}

		[ChildActionOnly]
		public ActionResult CrossSellProducts()
		{
			var cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();
			var products = _productService.GetCrosssellProductsByShoppingCart(cart, int.MaxValue);
			var productModels = products.Select(x => PrepareProductOverviewModel(x, false));
			var model = productModels.OrderBy(x => x.ProductPrice.PriceValue).Take(_shoppingCartSettings.CrossSellsNumber).ToList();
			return PartialView(model);
		}

		//recently viewed products
		public ActionResult RecentlyViewedProducts()
		{
			var model = new List<ProductModel>();
			if (_catalogSettings.RecentlyViewedProductsEnabled)
			{
				var products = _recentlyViewedProductsService.GetRecentlyViewedProducts(_catalogSettings.RecentlyViewedProductsNumber);
				foreach (var product in products)
					model.Add(PrepareProductOverviewModel(product));
			}
			return View(model);
		}

		[ChildActionOnly]
		public ActionResult RecentlyViewedProductsBlock()
		{
			var model = new List<ProductModel>();
			if (_catalogSettings.RecentlyViewedProductsEnabled)
			{
				var products = _recentlyViewedProductsService.GetRecentlyViewedProducts(_catalogSettings.RecentlyViewedProductsNumber);
				foreach (var product in products)
					model.Add(PrepareProductOverviewModel(product, false, false));
			}
			return PartialView(model);
		}

		//recently added products

		public ActionResult RecentlyAddedProducts(CatalogExtendedPagingFilteringModel command)
		{
			_customerService.SaveCustomerAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.LastContinueShoppingPage, _webHelper.GetThisPageUrl(false));
			var model = new RecentlyAddedProductsModel();
			//command.CreatedFrom = DateTime.UtcNow.Subtract(new TimeSpan(30, 0, 0, 0));
			command.OrderBy = (int)ProductSortingEnumAF.CreatedOn;
			command.PageSize = _catalogSettings.RecentlyAddedProductsNumber;
			var baseProducts = _cacheManager.Get(string.Format(RECENTLY_ADDED_MODEL_KEY, _workContext.WorkingLanguage.Id, _workContext.WorkingCurrency.Id), int.MaxValue, () => GetProductsModel(command));

			if (command.PageSize <= 0) command.PageSize = _catalogSettings.RecentlyAddedProductsNumber;
			if (command.PageNumber <= 0) command.PageNumber = 1;

			var productsSet = GetFilteredProducts(command, baseProducts);
			model.Products = productsSet["all"];
			LoadFilteringContext(model, command, productsSet);
			SelectPagedProducts(model, command);

			//model.PagingFilteringContext.ViewMode = "grid2";
			SetProductVariantPriceModel(model.Products);
			model.IsGuest = _workContext.CurrentCustomer.IsGuest();
			return View(model);
		}

        //[OutputCache(Duration = 3600, VaryByCustom = "lgg")]
		public ActionResult RecentlyAddedProducts280(CatalogPagingFilteringModel280 command)
		{
            //GetCatalogPagingFilteringModel280FromQueryString(ref command);

            //_customerService.SaveCustomerAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.LastContinueShoppingPage, _webHelper.GetThisPageUrl(false));
            //var model = new RecentlyAddedProductsModel280();
            ////command.CreatedFrom = DateTime.UtcNow.Subtract(new TimeSpan(30, 0, 0, 0));
            //if (command.PageSize <= 0) command.PageSize = _catalogSettings.RecentlyAddedProductsNumber;
            //if (command.PageNumber <= 0) command.PageNumber = 1;

            //decimal? minPriceConverted = null;
            //decimal? maxPriceConverted = null;
            //if (command.PriceRangeFilter.From.HasValue)
            //{
            //    minPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(command.PriceRangeFilter.From.Value, _workContext.WorkingCurrency);
            //}
            //if (command.PriceRangeFilter.To.HasValue)
            //{
            //    maxPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(command.PriceRangeFilter.To.Value, _workContext.WorkingCurrency);
            //}

            //////#region filtreleme
            //////if (_catalogSettings.RecentlyAddedProductsAllowFiltering)
            //////{
            ////    var categoryIds = new List<int>();
            ////    //products
            ////    IList<int> alreadyFilteredSpecOptionIds = command.SpecificationFilter.GetFilteredOptionIds();
            ////    IList<int> filterableSpecificationAttributeOptionIds = null;
            ////    IList<int> alreadyFilteredProductVariantAttributeIds = command.AttributeFilter.GetProductVariantAttributeIds();
            ////    IList<int> filterableProductVariantAttributeIds = null;
            ////    IList<int> alreadyFilteredManufacturerIds = command.ManufacturerFilter.GetFilteredManufacturerIds();
            ////    IList<int> alreadyFilteredCategoryIds = command.CategoryFilter.GetFilteredCategoryIds();
            ////    IList<int> filterableManufacturerIds = null;
            ////    IList<int> filterableCategoryIds = null;
            ////    decimal? maxPrice = null;

            ////    var products = _productService.SearchProductsFilterable(
            ////        categoryIds, 0, null, minPriceConverted, maxPriceConverted,
            ////        0, null, false, false, _workContext.WorkingLanguage.Id,
            ////        alreadyFilteredSpecOptionIds, alreadyFilteredProductVariantAttributeIds,
            ////        alreadyFilteredManufacturerIds, ProductSortingEnum.CreatedOn, command.PageNumber - 1, command.PageSize, true,
            ////        out filterableSpecificationAttributeOptionIds,
            ////        out filterableProductVariantAttributeIds,
            ////        out filterableManufacturerIds,
            ////        out filterableCategoryIds,
            ////        out maxPrice);

            ////    model.Products = PrepareProductOverviewModels280(products).ToList();
            ////    //model.PagingFilteringContext.FilteringDisabled = true;
            ////    //model.PagingFilteringContext.PriceRangeFilter.Disabled = true;
            ////    //model.PagingFilteringContext.AllowProductSorting = false;
            ////    model.PagingFilteringContext.LoadPagedList(products);
            ////    //model.PagingFilteringContext.ViewMode = viewMode;

            ////    //specs
            ////    model.PagingFilteringContext.SpecificationFilter.PrepareSpecsFilters(alreadyFilteredSpecOptionIds,
            ////    filterableSpecificationAttributeOptionIds,
            ////     _specificationAttributeService, _webHelper, _workContext);

            ////    //attributes
            ////    model.PagingFilteringContext.AttributeFilter.PrepareAttributeFilters(alreadyFilteredProductVariantAttributeIds,
            ////    filterableProductVariantAttributeIds,
            ////    _productAttributeService, _webHelper, _workContext);

            ////    //maufacturers
            ////    model.PagingFilteringContext.ManufacturerFilter.PrepareManufacturerFilters(alreadyFilteredManufacturerIds, filterableManufacturerIds, _manufacturerService, _webHelper, _workContext);

            ////    //categories
            ////    model.PagingFilteringContext.CategoryFilter.PrepareCategoryFilters(alreadyFilteredCategoryIds, filterableCategoryIds, _categoryService, _webHelper, _workContext);

            ////    //pricerange
            ////    model.PagingFilteringContext.PriceRangeFilter.PreparePriceRangeFilters(0, maxPrice, command.PriceRangeFilter.From, command.PriceRangeFilter.To, _webHelper, _workContext);
            
            //////}
            //////#endregion




            ////FILTRELEMEYI AKTIF ETMEK ICIN ALTTAKI KODU COMMENTLE
            //model.PagingFilteringContext.FilteringDisabled = true;
            //model.PagingFilteringContext.PriceRangeFilter.Disabled = true;
            //model.PagingFilteringContext.AllowProductSorting = false;
            //model.PagingFilteringContext.ViewMode = command.ViewMode;













            //    //products
            //    IList<int> alreadyFilteredSpecOptionIds = command.SpecificationFilter.GetFilteredOptionIds();
            //    IList<int> alreadyFilteredProductVariantAttributeIds = command.AttributeFilter.GetProductVariantAttributeIds();
            //    IList<int> alreadyFilteredManufacturerIds = command.ManufacturerFilter.GetFilteredManufacturerIds();
            //    IList<int> alreadyFilteredCategoryIds = command.CategoryFilter.GetFilteredCategoryIds();
            //    var categoryIds = alreadyFilteredCategoryIds;
            //    IList<int> filterableSpecificationAttributeOptionIds = null;
            //    IList<int> filterableProductVariantAttributeIds = null;
            //    IList<int> filterableManufacturerIds = null;
            //    IList<int> filterableCategoryIds = null;
            //    decimal? maxPrice = null;
                
            //    string productListModelCacheKey = string.Format(PRODUCT_LIST_MODEL, string.Join(",", categoryIds), minPriceConverted, maxPriceConverted, _workContext.WorkingLanguage.Id
            //        , string.Join(",", alreadyFilteredSpecOptionIds), string.Join(",", alreadyFilteredProductVariantAttributeIds)
            //        , string.Join(",", alreadyFilteredManufacturerIds), command.OrderBy, command.PageNumber, command.PageSize, null, currencyId);
            //    ProductListwithOverviewModel pm = new ProductListwithOverviewModel();

            //    if (Request.Params["cc"] != null && Request.Params["cc"].ToString().ToLowerInvariant().Equals("true"))
            //        _cacheManager.Remove(productListModelCacheKey);

            //    pm = _cacheManager.Get(productListModelCacheKey, int.MaxValue, () =>
            //    {
            //        pm.Products = _productService.SearchProductsFilterable(
            //            categoryIds, 0, null, minPriceConverted, maxPriceConverted,
            //            0, string.Empty, false, false, _workContext.WorkingLanguage.Id,
            //            alreadyFilteredSpecOptionIds, alreadyFilteredProductVariantAttributeIds,
            //            alreadyFilteredManufacturerIds, ProductSortingEnum.CreatedOn, command.PageNumber - 1, command.PageSize, true,
            //            out filterableSpecificationAttributeOptionIds,
            //            out filterableProductVariantAttributeIds,
            //            out filterableManufacturerIds,
            //            out filterableCategoryIds,
            //            out maxPrice);

            //        pm.FilterableSpecificationAttributeOptionIds = filterableSpecificationAttributeOptionIds;
            //        pm.FilterableProductVariantAttributeIds = filterableProductVariantAttributeIds;
            //        pm.FilterableManufacturerIds = filterableManufacturerIds;
            //        pm.FilterableCategoryIds = filterableCategoryIds;
            //        pm.MaxPrice = maxPrice;

            //        pm.ProductsOverview = PrepareProductOverviewModels280(pm.Products).ToList();
            //        return pm;
            //    });

            //    pm.Products.TotalCount = _catalogSettings.RecentlyAddedTotalProducts;

            //    model.Products = pm.ProductsOverview;

            //    //paging
            //    model.PagingFilteringContext.LoadPagedList(pm.Products);

            //    //specs
            //    model.PagingFilteringContext.SpecificationFilter.PrepareSpecsFilters(alreadyFilteredSpecOptionIds,
            //    pm.FilterableSpecificationAttributeOptionIds,
            //     _specificationAttributeService, _webHelper, _workContext);

            //    //attributes
            //    model.PagingFilteringContext.AttributeFilter.PrepareAttributeFilters(alreadyFilteredProductVariantAttributeIds,
            //    pm.FilterableProductVariantAttributeIds,
            //    _productAttributeService, _webHelper, _workContext);

            //    //maufacturers
            //    model.PagingFilteringContext.ManufacturerFilter.PrepareManufacturerFilters(alreadyFilteredManufacturerIds, pm.FilterableManufacturerIds, _manufacturerService, _webHelper, _workContext);

            //    //categories
            //    model.PagingFilteringContext.CategoryFilter.PrepareCategoryFilters(alreadyFilteredCategoryIds, pm.FilterableCategoryIds, _categoryService, _webHelper, _workContext);

            //    //pricerange
            //    model.PagingFilteringContext.PriceRangeFilter.PreparePriceRangeFilters(0, maxPrice, command.PriceRangeFilter.From, command.PriceRangeFilter.To, _webHelper, _workContext);
            





            ////#region filtreleme burda
            ////if (_catalogSettings.RecentlyAddedProductsAllowFiltering)
            ////{
            ////    }
            ////#endregion

            //model.IsGuest = _workContext.CurrentCustomer.IsGuest();
            //model.RecentlyAddedTotalProduct = _catalogSettings.RecentlyAddedTotalProducts;
            //return View(model);



            // AŞAĞIDAKİ KISIM CANLI ORTAM NOP.WEB.DLL DECOMPILE EDILEREK ALINDI

            this.GetCatalogPagingFilteringModel280FromQueryString(ref command);
            this._customerService.SaveCustomerAttribute<string>(this._workContext.CurrentCustomer, SystemCustomerAttributeNames.LastContinueShoppingPage, this._webHelper.GetThisPageUrl(false));
            RecentlyAddedProductsModel280 recentlyAddedProductsModel280 = new RecentlyAddedProductsModel280();
            if (command.PageSize <= 0)
            {
                command.PageSize = this._catalogSettings.RecentlyAddedProductsNumber;
            }
            if (command.PageNumber <= 0)
            {
                command.PageNumber = 1;
            }
            decimal? nullable = null;
            decimal? nullable1 = null;
            if (command.PriceRangeFilter.From.HasValue)
            {
                ICurrencyService currencyService = this._currencyService;
                decimal? from = command.PriceRangeFilter.From;
                nullable = new decimal?(currencyService.ConvertToPrimaryStoreCurrency(from.Value, this._workContext.WorkingCurrency));
            }
            if (command.PriceRangeFilter.To.HasValue)
            {
                ICurrencyService currencyService1 = this._currencyService;
                decimal? to = command.PriceRangeFilter.To;
                nullable1 = new decimal?(currencyService1.ConvertToPrimaryStoreCurrency(to.Value, this._workContext.WorkingCurrency));
            }
            object[] ıd = new object[] { nullable, nullable1, this._workContext.WorkingLanguage.Id, command.OrderBy, command.PageNumber, command.PageSize, ProductSortingEnum.CreatedOn, null, null, null, null, this.currencyId };
            string str = string.Format("Nop.ProductListModel.{0}.{1}.{2}.{3}.{4}.{5}.{6}.{7}.{8}.{9}.{10}.cr-{11}", ıd);
            ProductListwithOverviewModel productListwithOverviewModel = new ProductListwithOverviewModel();
            if (Request.Params["cc"] != null && Request.Params["cc"].ToString().ToLowerInvariant().Equals("true"))
            {
                this._cacheManager.Remove(str);
            }
            productListwithOverviewModel = this._cacheManager.Get<ProductListwithOverviewModel>(str, 2147483647, () =>
            {
                productListwithOverviewModel.Products = this._productService.SearchProductsFilterable(nullable, nullable1, 0, ProductSortingEnum.CreatedOn, command.PageNumber - 1, command.PageSize, false, false, this._workContext.WorkingLanguage.Id, true);
                productListwithOverviewModel.Products.TotalCount = this._catalogSettings.RecentlyAddedTotalProducts;
                productListwithOverviewModel.ProductsOverview = this.PrepareProductOverviewModels280(productListwithOverviewModel.Products, true, true, null, false, false, false).ToList<ProductOverviewModel280>();
                return productListwithOverviewModel;
            });
            recentlyAddedProductsModel280.Products = productListwithOverviewModel.ProductsOverview;
            recentlyAddedProductsModel280.PagingFilteringContext.LoadPagedList<Product>(productListwithOverviewModel.Products);
            recentlyAddedProductsModel280.PagingFilteringContext.FilteringDisabled = true;
            recentlyAddedProductsModel280.PagingFilteringContext.PriceRangeFilter.Disabled = true;
            recentlyAddedProductsModel280.PagingFilteringContext.AllowProductSorting = false;
            recentlyAddedProductsModel280.PagingFilteringContext.ViewMode = command.ViewMode;
            recentlyAddedProductsModel280.IsGuest = this._workContext.CurrentCustomer.IsGuest(true);
            recentlyAddedProductsModel280.RecentlyAddedTotalProduct = this._catalogSettings.RecentlyAddedTotalProducts;
            return base.View(recentlyAddedProductsModel280);
		}

        //[OutputCache(Duration = 3600, VaryByCustom = "lgg")]
		[ChildActionOnly]
		public ActionResult RecentlyAddedProducts280HomePage(CatalogPagingFilteringModel280 command)
		{
			var model = new RecentlyAddedProductsModel280();
			//if (command.PageSize <= 0) command.PageSize = _catalogSettings.RecentlyAddedProductsNumber;
			command.PageSize = 30;
			if (command.PageNumber <= 0) command.PageNumber = 1;
			decimal? minPriceConverted = null;
			decimal? maxPriceConverted = null;

			ProductListwithOverviewModel pm = new ProductListwithOverviewModel();
			pm.Products = _cacheManager.Get(string.Format(RECENTLY_ADDED_PRODUCTS_HOME_PAGE, _workContext.WorkingLanguage.Id, _workContext.WorkingCurrency.Id, command.PageNumber, command.PageSize), 3600, ()=>
                {
                    return _productService.SearchProductsFilterable(minPriceConverted, maxPriceConverted, 0, ProductSortingEnum.CreatedOn, command.PageNumber - 1, command.PageSize,
				        false, false, _workContext.WorkingLanguage.Id);
                });

            pm.ProductsOverview = _cacheManager.Get(string.Format(PREPARE_PRODUCT_OVERVIEW_MODELS, _workContext.WorkingLanguage.Id, _workContext.WorkingCurrency.Id, command.PageNumber, command.PageSize), 3600, () =>
                {
                    return PrepareProductOverviewModels280(pm.Products, true, true, 237).ToList();
                });

			model.Products = pm.ProductsOverview;
			return View(model);
		}
		
		[AjaxOnly]
		public ActionResult RecentlyAddedProductsJS280(CatalogPagingFilteringModel280 command)
		{
			var model = new RecentlyAddedProductsModel280();

			if (command.PageSize <= 0) command.PageSize = _catalogSettings.RecentlyAddedProductsNumber;
			if (command.PageNumber <= 0) command.PageNumber = 1;

			//price ranges
			decimal? minPriceConverted = null;
			decimal? maxPriceConverted = null;
			if (command.PriceRangeFilter.From.HasValue)
			{
				minPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(command.PriceRangeFilter.From.Value, _workContext.WorkingCurrency);
			}
			if (command.PriceRangeFilter.To.HasValue)
			{
				maxPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(command.PriceRangeFilter.To.Value, _workContext.WorkingCurrency);
			}

			////products
			//IList<int> alreadyFilteredSpecOptionIds = command.SpecificationFilter.GetFilteredOptionIds();
			////IList<int> filterableSpecificationAttributeOptionIds = null;
			//IList<int> alreadyFilteredProductVariantAttributeIds = command.AttributeFilter.GetProductVariantAttributeIds();
			////IList<int> filterableProductVariantAttributeIds = null;
			//IList<int> alreadyFilteredManufacturerIds = command.ManufacturerFilter.GetFilteredManufacturerIds();
			////IList<int> filterableManufacturerIds = null;
			////IList<int> filterableCategoryIds = null;
			//IList<int> alreadyFilteredCategoryIds = command.CategoryFilter.GetFilteredCategoryIds();
			////decimal? maxPrice = null;
			//var categoryIds = alreadyFilteredCategoryIds;

			//var products = _productService.SearchProductsFilterable(
			//    categoryIds, 0, null, minPriceConverted, maxPriceConverted,
			//    0, null, false, false, _workContext.WorkingLanguage.Id,
			//    alreadyFilteredSpecOptionIds, alreadyFilteredProductVariantAttributeIds,
			//    alreadyFilteredManufacturerIds, ProductSortingEnum.CreatedOn, command.PageNumber - 1, command.PageSize, true,
			//    out filterableSpecificationAttributeOptionIds,
			//    out filterableProductVariantAttributeIds,
			//    out filterableManufacturerIds,
			//    out filterableCategoryIds,
			//    out maxPrice);


			string productListModelCacheKey = string.Format(PRODUCT_LIST_MODEL, minPriceConverted, maxPriceConverted, _workContext.WorkingLanguage.Id
				, command.OrderBy, command.PageNumber, command.PageSize, ProductSortingEnum.CreatedOn, null, null, null, null, currencyId);
			ProductListwithOverviewModel pm = new ProductListwithOverviewModel();
			pm = _cacheManager.Get(productListModelCacheKey, () =>
			{
				pm.Products = _productService.SearchProductsFilterable(minPriceConverted, maxPriceConverted, 0, ProductSortingEnum.CreatedOn, command.PageNumber - 1, command.PageSize,
				   false, false, _workContext.WorkingLanguage.Id);

				pm.ProductsOverview = PrepareProductOverviewModels280(pm.Products).ToList();
				return pm;
			});

			model.Products = pm.ProductsOverview;
			model.PagingFilteringContext.LoadPagedList(pm.Products);
			model.PagingFilteringContext.FilteringDisabled = true;
			model.PagingFilteringContext.PriceRangeFilter.Disabled = true;
			model.PagingFilteringContext.AllowProductSorting = false;

			//model.PagingFilteringContext.ViewMode = viewMode;

			//specs
			//model.PagingFilteringContext.SpecificationFilter.PrepareSpecsFilters(alreadyFilteredSpecOptionIds,
			//    filterableSpecificationAttributeOptionIds,
			//    _specificationAttributeService, _webHelper, _workContext);

			//attributes
			//model.PagingFilteringContext.AttributeFilter.PrepareAttributeFilters(alreadyFilteredProductVariantAttributeIds,
			//  filterableProductVariantAttributeIds,
			//  _productAttributeService, _webHelper, _workContext);

			//maufacturers
			//model.PagingFilteringContext.ManufacturerFilter.PrepareManufacturerFilters(alreadyFilteredManufacturerIds, filterableManufacturerIds, _manufacturerService, _webHelper, _workContext);

			//categories
			//model.PagingFilteringContext.CategoryFilter.PrepareCategoryFilters(alreadyFilteredCategoryIds, filterableCategoryIds, _categoryService, _webHelper, _workContext);

			//pricerange
			//model.PagingFilteringContext.PriceRangeFilter.PreparePriceRangeFilters(0, maxPrice, command.PriceRangeFilter.From, command.PriceRangeFilter.To, _webHelper, _workContext);

			//if (command.FilterTrigging == "C" || command.FilterTrigging == "M" || command.FilterTrigging == "S" || command.FilterTrigging == "A")
			//    model.PagingFilteringContext.ShowFilteringPanel = true;
			//else
			//    model.PagingFilteringContext.ShowFilteringPanel = false;

			string view = null;
			if (command.ViewMode == "grid1")
				view = @"~/Views/Catalog/ProductsList1280.cshtml";
			else if (command.ViewMode == "grid2")
				view = @"~/Views/Catalog/ProductsList2280.cshtml";
			else
				view = @"~/Views/Catalog/ProductsList3280.cshtml";

			return Json(new
			{
				Html = Utilities.RenderPartialViewToString(this, view, model),
				SelectionHtml = Utilities.RenderPartialViewToString(this, "~/Views/Catalog/_yourSelection280.cshtml", model.PagingFilteringContext),
				PriceMin = model.PagingFilteringContext.PriceRangeFilter.Min,
				PriceMax = model.PagingFilteringContext.PriceRangeFilter.Max,
				PriceValue = model.PagingFilteringContext.PriceRangeFilter.To,
				PriceStep = model.PagingFilteringContext.PriceRangeFilter.StepSize,
				HasMore = model.PagingFilteringContext.HasNextPage,
				PageNumber = model.PagingFilteringContext.PageNumber,
				PageSize = model.PagingFilteringContext.PageSize,
				ViewMode = command.ViewMode
			});


		}


		[AjaxOnly]
		public ActionResult RecentlyAddedProductsJS(CatalogExtendedPagingFilteringModel command)
		{
			var model = new RecentlyAddedProductsModel();
			//command.CreatedFrom = DateTime.UtcNow.Subtract(new TimeSpan(30, 0, 0));
			//command.OrderBy = (int)ProductSortingEnumAF.CreatedOn;
			command.PageSize = _catalogSettings.RecentlyAddedProductsNumber;
			var baseProducts = _cacheManager.Get(string.Format(RECENTLY_ADDED_MODEL_KEY, _workContext.WorkingLanguage.Id, _workContext.WorkingCurrency.Id), int.MaxValue, () => GetProductsModel(command));

			if (command.PageSize <= 0) command.PageSize = _catalogSettings.RecentlyAddedProductsNumber;
			if (command.PageNumber <= 0) command.PageNumber = 1;

			var productsSet = GetFilteredProducts(command, baseProducts);
			model.Products = productsSet["all"];
			LoadFilteringContext(model, command, productsSet);
			SelectPagedProducts(model, command);
			SetProductVariantPriceModel(model.Products);
			string view = null;
			if (command.ViewMode == "grid1")
				view = @"~/Views/Catalog/ProductsList1.cshtml";
			else if (command.ViewMode == "grid2")
				view = @"~/Views/Catalog/ProductsList2.cshtml";
			else
				view = @"~/Views/Catalog/ProductsList3.cshtml";
			return Json(new
			{
				Html = Utilities.RenderPartialViewToString(this, view, model),
				SelectionHtml = Utilities.RenderPartialViewToString(this, "~/Views/Catalog/_yourSelection.cshtml", model.PagingFilteringContext),
				PriceMin = model.PagingFilteringContext.PriceRangeSliderFilter.Item.Min,
				PriceMax = model.PagingFilteringContext.PriceRangeSliderFilter.Item.Max,
				PriceValue = model.PagingFilteringContext.PriceRangeSliderFilter.Item.Value,
				PriceStep = model.PagingFilteringContext.PriceRangeSliderFilter.Item.StepSize,
				HasMore = model.PagingFilteringContext.HasNextPage,
				PageNumber = model.PagingFilteringContext.PageNumber,
				PageSize = model.PagingFilteringContext.PageSize,
				ViewMode = command.ViewMode
			});
		}



		[ChildActionOnly]
		public ActionResult HomepageBestSellers()
		{
			if (!_catalogSettings.ShowBestsellersOnHomepage || _catalogSettings.NumberOfBestsellersOnHomepage == 0)
				return Content("");

			var products = new List<Product>();
			var report = _orderReportService.BestSellersReport(null, null, null, null, null,
				_catalogSettings.NumberOfBestsellersOnHomepage);
			foreach (var line in report)
			{
				var productVariant = _productService.GetProductVariantById(line.ProductVariantId);
				if (productVariant != null)
				{
					var product = productVariant.Product;
					if (product != null)
					{
						bool contains = false;
						foreach (var p in products)
						{
							if (p.Id == product.Id)
							{
								contains = true;
								break;
							}
						}
						if (!contains)
							products.Add(product);
					}
				}
			}

			var model = products
				.Select(x => PrepareProductOverviewModel(x))
				.ToList();

			return PartialView(model);
		}

        //[OutputCache(Duration = 3600, VaryByCustom = "lgg")]
		[ChildActionOnly]
		public ActionResult HomepageProducts()
		{
			int[] imageSizes = new int[] { 241, 402, 434, 208, 208, 241, 402 };
			int imageCounter = 0;

            var model = _cacheManager.Get(string.Format(ModelCacheEventConsumer.HOME_PAGE_PRODUCTS, _workContext.WorkingLanguage.Id, _workContext.WorkingCurrency.Id), 3600, () =>
            {
                return _productService.GetAllProductsDisplayedOnHomePage().Take(7)
                .Select(x => PrepareProductOverviewModel(x, true, true, imageSizes[imageCounter++]))
                .ToList();
            });

			return PartialView(model);
		}

		#endregion

		#region Product tags


		//Product tags

		[ChildActionOnly]
		public ActionResult ProductTags(int productId)
		{
			var product = _productService.GetProductById(productId);
			if (product == null)
				throw new ArgumentException("No product found with the specified id");

			var model = product.ProductTags
				.OrderByDescending(x => x.ProductCount)
				.Select(x =>
				{
					var ptModel = new ProductTagModel()
					{
						Id = x.Id,
						Name = x.Name,
						ProductCount = x.ProductCount
					};
					return ptModel;
				})
				.ToList();

			return PartialView(model);
		}

		[ChildActionOnly]
		public ActionResult PopularProductTags()
		{
			var model = new PopularProductTagsModel();

			//get all tags
			var tags = _productTagService.GetAllProductTags()
				.OrderByDescending(x => x.ProductCount)
				.Take(_catalogSettings.NumberOfProductTags)
				.ToList();
			//sorting
			tags = tags.OrderBy(x => x.Name).ToList();

			foreach (var tag in tags)
				model.Tags.Add(new ProductTagModel()
				{
					Id = tag.Id,
					Name = tag.Name,
					ProductCount = tag.ProductCount
				});

			return PartialView(model);
		}

		public ActionResult ProductsByTag(int productTagId, CatalogPagingFilteringModel command)
		{
			var productTag = _productTagService.GetProductById(productTagId);
            if (productTag == null)
                return InvokeHttp404();

			if (command.PageSize <= 0) command.PageSize = 4;
			if (command.PageNumber <= 0) command.PageNumber = 1;

			var model = new ProductsByTagModel()
			{
				TagName = productTag.Name
			};


			//sorting
			model.AllowProductFiltering = _catalogSettings.AllowProductSorting;
			if (model.AllowProductFiltering)
			{
				foreach (ProductSortingEnum enumValue in Enum.GetValues(typeof(ProductSortingEnum)))
				{
					var currentPageUrl = _webHelper.GetThisPageUrl(true);
					var sortUrl = _webHelper.ModifyQueryString(currentPageUrl, "orderby=" + ((int)enumValue).ToString(), null);

					var sortValue = enumValue.GetLocalizedEnum(_localizationService, _workContext);
					model.AvailableSortOptions.Add(new SelectListItem()
					{
						Text = sortValue,
						Value = sortUrl,
						Selected = enumValue == (ProductSortingEnum)command.OrderBy
					});
				}
			}


			//view mode
			model.AllowProductViewModeChanging = _catalogSettings.AllowProductViewModeChanging;
			if (model.AllowProductViewModeChanging)
			{
				var currentPageUrl = _webHelper.GetThisPageUrl(true);
				//grid
				model.AvailableViewModes.Add(new SelectListItem()
				{
					Text = _localizationService.GetResource("Categories.ViewMode.Grid"),
					Value = _webHelper.ModifyQueryString(currentPageUrl, "viewmode=grid", null),
					Selected = command.ViewMode == "grid"
				});
				//list
				model.AvailableViewModes.Add(new SelectListItem()
				{
					Text = _localizationService.GetResource("Categories.ViewMode.List"),
					Value = _webHelper.ModifyQueryString(currentPageUrl, "viewmode=list", null),
					Selected = command.ViewMode == "list"
				});
			}


			//products
			var products = _productService.SearchProducts(0, 0, false, null, null,
				productTag.Id, string.Empty, false, _workContext.WorkingLanguage.Id, null,
				(ProductSortingEnum)command.OrderBy, command.PageNumber - 1, command.PageSize);
			model.Products = products.Select(x => PrepareProductOverviewModel(x)).ToList();

			model.PagingFilteringContext.LoadPagedList(products);
			model.PagingFilteringContext.ViewMode = command.ViewMode;
			return View(model);
		}


		#endregion

		#region Product reviews

		//products reviews
		public ActionResult ProductReviews(int productId)
		{
			var product = _productService.GetProductById(productId);
            if (product == null || product.Deleted || !product.Published || !product.AllowCustomerReviews)
                return InvokeHttp404();

			var model = new ProductReviewsModel();
			PrepareProductReviewsModel(model, product);
			//default value
			model.AddProductReview.Rating = 4;
			return View(model);
		}

		[HttpPost, ActionName("ProductReviews")]
		[FormValueRequired("add-review")]
		public ActionResult ProductReviewsAdd(int productId, ProductReviewsModel model)
		{
			var product = _productService.GetProductById(productId);
            if (product == null || product.Deleted || !product.Published || !product.AllowCustomerReviews)
                return InvokeHttp404();

			if (ModelState.IsValid)
			{
				if (_workContext.CurrentCustomer.IsGuest() && !_catalogSettings.AllowAnonymousUsersToReviewProduct)
				{
					ModelState.AddModelError("", _localizationService.GetResource("Reviews.OnlyRegisteredUsersCanWriteReviews"));
				}
				else
				{
					//save review
					int rating = model.AddProductReview.Rating;
					if (rating < 1 || rating > 5)
						rating = 4;
					bool isApproved = !_catalogSettings.ProductReviewsMustBeApproved;

					var productReview = new ProductReview()
					{
						ProductId = product.Id,
						CustomerId = _workContext.CurrentCustomer.Id,
						IpAddress = _webHelper.GetCurrentIpAddress(),
						Title = model.AddProductReview.Title,
						ReviewText = model.AddProductReview.ReviewText,
						Rating = rating,
						HelpfulYesTotal = 0,
						HelpfulNoTotal = 0,
						IsApproved = isApproved,
						CreatedOnUtc = DateTime.UtcNow,
						UpdatedOnUtc = DateTime.UtcNow,
					};
					_customerContentService.InsertCustomerContent(productReview);

					//update product totals
					_productService.UpdateProductReviewTotals(product);

					//notify store owner
					if (_catalogSettings.NotifyStoreOwnerAboutNewProductReviews)
						_workflowMessageService.SendProductReviewNotificationMessage(productReview, _localizationSettings.DefaultAdminLanguageId);


					PrepareProductReviewsModel(model, product);
					model.AddProductReview.Title = null;
					model.AddProductReview.ReviewText = null;

					model.AddProductReview.SuccessfullyAdded = true;
					if (!isApproved)
						model.AddProductReview.Result = _localizationService.GetResource("Reviews.SeeAfterApproving");
					else
						model.AddProductReview.Result = _localizationService.GetResource("Reviews.SuccessfullyAdded");

					return View(model);
				}
			}

			//If we got this far, something failed, redisplay form
			PrepareProductReviewsModel(model, product);
			return View(model);
		}

		[HttpPost]
		public ActionResult SetProductReviewHelpfulness(int productReviewId, bool washelpful)
		{
			var productReview = _customerContentService.GetCustomerContentById(productReviewId) as ProductReview;
			if (productReview == null)
				throw new ArgumentException("No product review found with the specified id");

			if (_workContext.CurrentCustomer.IsGuest() && !_catalogSettings.AllowAnonymousUsersToReviewProduct)
			{
				return Json(new
				{
					Result = _localizationService.GetResource("Reviews.Helpfulness.OnlyRegistered"),
					TotalYes = productReview.HelpfulYesTotal,
					TotalNo = productReview.HelpfulNoTotal
				});
			}

			//delete previous helpfulness
			var oldPrh = (from prh in productReview.ProductReviewHelpfulnessEntries
						  where prh.CustomerId == _workContext.CurrentCustomer.Id
						  select prh).FirstOrDefault();
			if (oldPrh != null)
				_customerContentService.DeleteCustomerContent(oldPrh);

			//insert new helpfulness
			var newPrh = new ProductReviewHelpfulness()
			{
				ProductReviewId = productReview.Id,
				CustomerId = _workContext.CurrentCustomer.Id,
				IpAddress = _webHelper.GetCurrentIpAddress(),
				WasHelpful = washelpful,
				IsApproved = true, //always approved
				CreatedOnUtc = DateTime.UtcNow,
				UpdatedOnUtc = DateTime.UtcNow,
			};
			_customerContentService.InsertCustomerContent(newPrh);

			//new totals
			int helpfulYesTotal = (from prh in productReview.ProductReviewHelpfulnessEntries
								   where prh.WasHelpful
								   select prh).Count();
			int helpfulNoTotal = (from prh in productReview.ProductReviewHelpfulnessEntries
								  where !prh.WasHelpful
								  select prh).Count();

			productReview.HelpfulYesTotal = helpfulYesTotal;
			productReview.HelpfulNoTotal = helpfulNoTotal;
			_customerContentService.UpdateCustomerContent(productReview);

			return Json(new
			{
				Result = _localizationService.GetResource("Reviews.Helpfulness.SuccessfullyVoted"),
				TotalYes = productReview.HelpfulYesTotal,
				TotalNo = productReview.HelpfulNoTotal
			});
		}

		#endregion

		#region Email a friend

		//products email a friend
		//[ChildActionOnly]
		//public ActionResult ProductEmailAFriendButton(int productId)
		//{
		//    if (!_catalogSettings.EmailAFriendEnabled)
		//        return Content("");
		//    var model = new ProductEmailAFriendModel()
		//    {
		//        VariantId = productId
		//    };

		//    return PartialView("ProductEmailAFriendButton", model);
		//}

		//public ActionResult ProductEmailAFriend(int variantId)
		//{
		//    var product = _productService.GetProductVariantById(variantId);
		//    if (product == null || product.Deleted || !product.Published || !_catalogSettings.EmailAFriendEnabled)
		//        return RedirectToAction("Index", "Home");

		//    var model = new ProductEmailAFriendModel();
		//    model.ProductId = product.Id;
		//    model.ProductName = product.GetLocalized(x => x.Name);
		//    model.ProductSeName = product.Product.GetSeName();
		//    model.YourEmailAddress = _workContext.CurrentCustomer.Email;
		//    return View(model);
		//}

		[ChildActionOnly]
		[BotGetControl]
		public ActionResult ProductEmailAFriend()
		{
			var model = new ProductEmailAFriendModel();
			model.YourEmailAddress = _workContext.CurrentCustomer.Email;
			return View(model);
		}

		[HttpPost]
		[BotPostControl(RedirectUrl = "/", RedirectAjaxUrl = "/Catalog/ProductEmailAFriendSuccess", TrapFormElementName = "Surname", MinimumRequestPeriod = 2)]
		public ActionResult ProductEmailAFriendSend(ProductEmailAFriendModel model)
		{
			var productVariant = _productService.GetProductVariantById(model.VariantId);
			if (productVariant == null || productVariant.Deleted || !productVariant.Published || !_catalogSettings.EmailAFriendEnabled)
				return Json(new { Success = false, Message = _localizationService.GetResource("ProductEmailFriend.UnSucsess") });
			if (ModelState.IsValid)
			{
				if (_workContext.CurrentCustomer.IsGuest() && !_catalogSettings.AllowAnonymousUsersToEmailAFriend)
				{
					return Json(new { Success = false, Message = _localizationService.GetResource("ProductEmailFriend.UnSucsess") });
				}
				else
				{
					//email
					_workflowMessageService.SendProductEmailAFriendMessage(model.YourName, model.FriendName, _workContext.WorkingLanguage.Id, productVariant,
							model.YourEmailAddress, model.FriendEmail, Core.Html.HtmlHelper.FormatText(model.PersonalMessage, false, true, false, false, false, false));
					//Core.Html.HtmlHelper.FormatText(model.PersonalMessage, false, true, false, false, false, false)
					return Json(new { Success = true, Message = _localizationService.GetResource("ProductEmailFriend.Sucsess") });
				}
			}
			return Json(new { Success = false, Message = _localizationService.GetResource("ProductEmailFriend.UnSucsess") });
		}

		public ActionResult ProductEmailAFriendSuccess()
		{
			return Json(new { Success = true, header = _localizationService.GetResource("ProductEmailFriend.Sucsess"), buttonText = _localizationService.GetResource("NewsLetterSubscription.Newsletter.TextOfButton"), linkText = _localizationService.GetResource("NewsLetterSubscription.Newsletter.TextOfLink") }, JsonRequestBehavior.AllowGet);
		}
		#endregion

		#region Comparing products

		//compare products
		public ActionResult AddProductToCompareList(int productId)
		{
			var product = _productService.GetProductById(productId);
            if (product == null || product.Deleted || !product.Published)
                return InvokeHttp404();

			if (!_catalogSettings.CompareProductsEnabled)
				return RedirectToAction("Index", "Home");

			_compareProductsService.AddProductToCompareList(productId);

			return RedirectToRoute("CompareProducts");
		}

		public ActionResult RemoveProductFromCompareList(int productId)
		{
			var product = _productService.GetProductById(productId);
			if (product == null)
				return RedirectToAction("Index", "Home");

			if (!_catalogSettings.CompareProductsEnabled)
				return RedirectToAction("Index", "Home");

			_compareProductsService.RemoveProductFromCompareList(productId);

			return RedirectToRoute("CompareProducts");
		}

		public ActionResult CompareProducts()
		{
			if (!_catalogSettings.CompareProductsEnabled)
				return RedirectToAction("Index", "Home");

			var model = new List<ProductModel>();
			foreach (var product in _compareProductsService.GetComparedProducts())
			{
				var productModel = PrepareProductOverviewModel(product);
				//specs for comparing
				productModel.SpecificationAttributeModels = _specificationAttributeService.GetProductSpecificationAttributesByProductId(product.Id, null, true)
					.Select(psa =>
					{
						return new ProductSpecificationModel()
						{
							SpecificationAttributeId = psa.SpecificationAttributeOption.SpecificationAttributeId,
							SpecificationAttributeName = psa.SpecificationAttributeOption.SpecificationAttribute.GetLocalized(x => x.Name),
							SpecificationAttributeOption = psa.SpecificationAttributeOption.GetLocalized(x => x.Name),
							Position = psa.SpecificationAttributeOption.DisplayOrder
						};
					})
					.ToList();
				model.Add(productModel);
			}
			return View(model);
		}

		public ActionResult ClearCompareList()
		{
			if (!_catalogSettings.CompareProductsEnabled)
				return RedirectToAction("Index", "Home");

			_compareProductsService.ClearCompareProducts();

			return RedirectToRoute("CompareProducts");
		}

		[ChildActionOnly]
		public ActionResult CompareProductsButton(int productId)
		{
			if (!_catalogSettings.CompareProductsEnabled)
				return Content("");

			var model = new AddToCompareListModel()
			{
				ProductId = productId
			};

			return PartialView("CompareProductsButton", model);
		}

		#endregion

		#region Searching

		private IList<ProductModel> GetProductsModel(CatalogExtendedPagingFilteringModel command)
		{
			if (command.Q != null)
				command.Q = command.Q.Trim();

			var products = _productService.SearchProducts(command.CategoryId, command.ManufacturerId, null,
				command.PriceRangeSliderFilter.Item.Min, command.PriceRangeSliderFilter.Item.Max,
				0, command.Q, true, _workContext.WorkingLanguage.Id, null,
				(ProductSortingEnumAF)command.OrderBy, 0, command.PageSize);
			return products.Select(x => PrepareProductOverviewPageModel(x)).ToList();
		}

		//public IList<ProductModel> GetFilteredProducts(CatalogExtendedPagingFilteringModel command, IList<ProductModel> baseProducts)
		//{
		//    var products = new List<ProductModel>();
		//    decimal maxPrice = 0;
		//    foreach (var productModel in baseProducts)
		//    {
		//        if (!ProductHasCategory(productModel, command.CategoryFilter.AlreadyFilteredItems)) continue;
		//        if (!ProductHasManufacturer(productModel, command.ManufacturerFilter.AlreadyFilteredItems)) continue;
		//        if (!ProductHasSpecificationOptions(productModel, command.SpecificationFilter.AlreadyFilteredItems)) continue;
		//        if (!ProductHasAttributeOptions(productModel, CreateAttributeCombinations(command.ProductAttributeFilter.AlreadyFilteredItems))) continue;

		//        maxPrice = Math.Max(maxPrice, productModel.DefaultVariantModel.ProductVariantPrice.PriceValue);
		//        if (!(ProductHasPrice(productModel, command.PriceRangeSliderFilter.Item.Value))) continue;
		//        products.Add(productModel);
		//    }
		//    command.PriceRangeSliderFilter.Item.Max = Convert.ToInt32(Math.Ceiling(maxPrice));
		//    if (!command.PriceRangeSliderFilter.Item.Value.HasValue)
		//    {
		//        command.PriceRangeSliderFilter.Item.Value = command.PriceRangeSliderFilter.Item.Max;
		//    }

		//    return products;
		//}

		public Dictionary<string, List<ProductModel>> GetFilteredProducts(CatalogExtendedPagingFilteringModel command, IList<ProductModel> baseProducts)
		{
			Dictionary<string, bool> ProductExists = new Dictionary<string, bool>();
			Dictionary<string, List<ProductModel>> FilteredProductsLists = new Dictionary<string, List<ProductModel>>();
			FilteredProductsLists["all"] = new List<ProductModel>();
			FilteredProductsLists["cat"] = new List<ProductModel>();
			FilteredProductsLists["man"] = new List<ProductModel>();

			foreach (var productModel in baseProducts)
			{
				//reset flag
				foreach (var vm in productModel.ProductVariantModels)
				{
					vm.Active = true;
				}

				ProductExists["cat"] = ProductHasCategory(productModel, command.CategoryFilter.AlreadyFilteredItems);
				ProductExists["man"] = ProductHasManufacturer(productModel, command.ManufacturerFilter.AlreadyFilteredItems);
				foreach (var filterItemGroup in command.SpecificationFilter.AlreadyFilteredItems.GroupBy(x => x.AttributeId))
				{
					ProductExists["spec" + filterItemGroup.Key] = ProductHasSpecification(productModel, filterItemGroup.ToList());
				}
				var combinations = CreateAttributeCombinations(command.ProductAttributeFilter.AlreadyFilteredItems);
				foreach (var filterItemGroup in command.ProductAttributeFilter.AlreadyFilteredItems.GroupBy(x => x.AttributeId))
				{
					var comb = combinations.Select(x => { return x.Remove(filterItemGroup.Key); });
					ProductExists["attr" + filterItemGroup.Key] = ProductHasAttributeOptions(productModel, combinations);
				}
				ProductExists["price"] = ProductHasPrice(productModel, command.PriceRangeSliderFilter.Item.Value);
				ProductExists["all"] = true;

				foreach (var kvp in ProductExists)
				{
					bool add = true;
					foreach (var item in ProductExists)
					{
						if (item.Key == kvp.Key) continue;
						add = add && item.Value;
					}
					if (add)
					{
						if (!FilteredProductsLists.ContainsKey(kvp.Key))
							FilteredProductsLists[kvp.Key] = new List<ProductModel>();
						FilteredProductsLists[kvp.Key].Add(productModel);
					}
				}
			}

			//if (FilteredProductsLists.ContainsKey("price"))
			//{
			//    var orderedPrices= FilteredProductsLists["price"].Select(x => { return x.DefaultVariantModel.ProductVariantPrice.PriceValue; }).OrderByDescending(x => x);
			//    maxPrice = orderedPrices.FirstOrDefault();
			//    minPrice = orderedPrices.LastOrDefault();
			//}
			//    command.PriceRangeSliderFilter.Item.Max = Convert.ToInt32(Math.Ceiling(maxPrice));
			//    command.PriceRangeSliderFilter.Item.Min = Convert.ToInt32(Math.Ceiling(minPrice));
			//    command.PriceRangeSliderFilter.Item.StepSize = 100;
			//if (!command.PriceRangeSliderFilter.Item.Value.HasValue)
			//{
			//    command.PriceRangeSliderFilter.Item.Value = command.PriceRangeSliderFilter.Item.Max;
			//}

			return FilteredProductsLists;
		}

		private void LoadFilteringContext(ProductListModel model, CatalogExtendedPagingFilteringModel command, Dictionary<string, List<ProductModel>> productsSet, bool extended = true)
		{
			var filteringCategories = this.GetProductsCategories(productsSet["cat"]);
			//var categoriesModel = filteringCategories.Select(x => x.ToModel()).ToList();
			//model.PagingFilteringContext.CategoryFilter.Items = categoriesModel.Select(x => new CatalogExtendedPagingFilteringModel.CategoryFilterItem() { Id = x.Id, Name = x.Name, Selected = false }).ToList();
			model.PagingFilteringContext.CategoryFilter.Items = filteringCategories.Select(x => new CatalogExtendedPagingFilteringModel.CategoryFilterItem() { Id = x.Id, Name = x.Name, Selected = false }).ToList();
			model.PagingFilteringContext.CategoryFilter.AlreadyFilteredItems = command.CategoryFilter.AlreadyFilteredItems;

			var filteringManufacturers = this.GetProductsManufacturers(productsSet["man"]);
			//var manufacturersModel = filteringManufacturers.Select(x => x.ToModel()).ToList();
			model.PagingFilteringContext.ManufacturerFilter.Items = filteringManufacturers.Select(x => new CatalogExtendedPagingFilteringModel.ManufacturerFilterItem() { Id = x.Id, Name = x.Name, Selected = false }).ToList();
			model.PagingFilteringContext.ManufacturerFilter.AlreadyFilteredItems = command.ManufacturerFilter.AlreadyFilteredItems;

			if (extended || model.PagingFilteringContext.ManufacturerFilter.AlreadyFilteredItems.Count > 0 || model.PagingFilteringContext.CategoryFilter.AlreadyFilteredItems.Count > 0)
			{
				LoadSpecificationFilterModel(model.PagingFilteringContext, productsSet);
				model.PagingFilteringContext.SpecificationFilter.AlreadyFilteredItems = command.SpecificationFilter.AlreadyFilteredItems;

				model.ProductAttributeModels = LoadProductAttributesModel(productsSet);
				model.PagingFilteringContext.ProductAttributeFilter = GetAttributeFilterModel(model.ProductAttributeModels);
				model.PagingFilteringContext.ProductAttributeFilter.AlreadyFilteredItems = command.ProductAttributeFilter.AlreadyFilteredItems;

			}

			decimal maxPrice = 0;
			decimal minPrice = 0;

			if (productsSet.ContainsKey("price"))
			{
				var orderedPrices = productsSet["price"].Select(x => { return x.DefaultVariantModel.ProductVariantPrice.PriceValue; }).OrderByDescending(x => x);
				maxPrice = orderedPrices.FirstOrDefault();
				minPrice = orderedPrices.LastOrDefault();
			}
			var max = (Math.Ceiling(maxPrice));
			var min = (Math.Floor(minPrice));
			decimal stepSize = (decimal)Math.Pow(10, Math.Floor(Math.Log10((double)(max - min))) - 1);
			if (stepSize == 0) stepSize = max;
			if (stepSize != 0)
			{
				max = Math.Ceiling(max / stepSize) * stepSize;
				min = Math.Ceiling(min / stepSize) * stepSize;
			}


			if (command.PriceRangeSliderFilter.Item.Value.HasValue)
			{
				model.PagingFilteringContext.PriceRangeSliderFilter.Item.Value = command.PriceRangeSliderFilter.Item.Value;
			}
			else
			{
				model.PagingFilteringContext.PriceRangeSliderFilter.Item.Value = max;
			}
			if (model.PagingFilteringContext.PriceRangeSliderFilter.Item.Value > max) model.PagingFilteringContext.PriceRangeSliderFilter.Item.Value = max;
			if (model.PagingFilteringContext.PriceRangeSliderFilter.Item.Value < min) model.PagingFilteringContext.PriceRangeSliderFilter.Item.Value = min;
			model.PagingFilteringContext.PriceRangeSliderFilter.Item.Max = max;
			model.PagingFilteringContext.PriceRangeSliderFilter.Item.Min = min;
			model.PagingFilteringContext.PriceRangeSliderFilter.Item.StepSize = stepSize;


			if (command.FilterTrigging == "C" || command.FilterTrigging == "M" || command.FilterTrigging == "S" || command.FilterTrigging == "A")
				model.PagingFilteringContext.ShowFilteringPanel = true;
			else
				model.PagingFilteringContext.ShowFilteringPanel = false;

			model.PagingFilteringContext.TogglePriceEnabled = command.ViewMode != "grid3";
		}

		private void SelectPagedProducts(ProductListModel model, CatalogExtendedPagingFilteringModel command)
		{
			int skipCount = command.PageIndex * command.PageSize;

			//command.OuterSorting = command.OrderBy == (int)ProductSortingEnumAF.Position ? ProductOuterSortingEnumAF.Category : ProductOuterSortingEnumAF.None;

			IOrderedEnumerable<ProductModel> products = null;
			if (command.OuterSorting != ProductOuterSortingEnumAF.None)
			{
				switch ((ProductOuterSortingEnumAF)command.OuterSorting)
				{
					case ProductOuterSortingEnumAF.Category:
						products = model.Products.OrderBy(x => x.CategoryId);
						break;
					case ProductOuterSortingEnumAF.Manufacturer:
						products = model.Products.OrderBy(x => x.ManufacturerId);
						break;
					default:
						break;
				}


				switch ((ProductSortingEnumAF)command.OrderBy)
				{
					case ProductSortingEnumAF.PriceAscending:
						products = products.ThenBy(x => x.DefaultVariantModel.ProductVariantPrice.PriceWithDiscountValue);
						break;
					case ProductSortingEnumAF.CreatedOn:
						products = products.ThenByDescending(x => x.CreatedOnUtc);
						break;
					case ProductSortingEnumAF.PriceDescending:
						products = products.ThenByDescending(x => x.DefaultVariantModel.ProductVariantPrice.PriceWithDiscountValue);
						break;

					case ProductSortingEnumAF.Position:
						if (command.CategoryId != 0)
							products = products.ThenBy(x => x.ProductCategories.Where(pc => pc.Id == command.CategoryId).FirstOrDefault().DisplayOrder);
						else if (command.ManufacturerId != 0)
							products = products.ThenBy(x => x.ProductManufacturers.Where(pm => pm.Id == command.ManufacturerId).FirstOrDefault().DisplayOrder);
						else
							products = products.ThenByDescending(x => x.CreatedOnUtc);
						break;
					default:
						products = products.ThenByDescending(x => x.CreatedOnUtc);
						break;
				}
			}
			else
			{
				switch ((ProductSortingEnumAF)command.OrderBy)
				{
					case ProductSortingEnumAF.PriceAscending:
						products = model.Products.OrderBy(x => x.DefaultVariantModel.ProductVariantPrice.PriceWithDiscountValue);
						break;
					case ProductSortingEnumAF.CreatedOn:
						products = model.Products.OrderByDescending(x => x.CreatedOnUtc);
						break;
					case ProductSortingEnumAF.PriceDescending:
						products = model.Products.OrderByDescending(x => x.DefaultVariantModel.ProductVariantPrice.PriceWithDiscountValue);
						break;
					case ProductSortingEnumAF.Position:
						if (command.CategoryId != 0)
							products = model.Products.OrderBy(x => x.ProductCategories.Where(pc => pc.Id == command.CategoryId).FirstOrDefault().DisplayOrder);
						else if (command.ManufacturerId != 0)
							products = model.Products.OrderBy(x => x.ProductManufacturers.Where(pm => pm.Id == command.ManufacturerId).FirstOrDefault().DisplayOrder);
						else
							products = model.Products.OrderByDescending(x => x.CreatedOnUtc);
						break;
					default:
						products = model.Products.OrderByDescending(x => x.CreatedOnUtc);
						break;
				}

			}
			//TODO:do it well remove session variable !!
			var list = products.ToList();
			Session["productList"] = list.Select(x => x.Id).ToArray();
			model.PagingFilteringContext.TotalItems = list.Count;
			model.PagingFilteringContext.PageSize = command.PageSize;
			model.PagingFilteringContext.PageNumber = command.PageNumber;
			model.PagingFilteringContext.HasNextPage = products.Count() > command.PageSize * command.PageNumber;
			model.Products = products.Skip(skipCount).Take(command.PageSize).ToList();
		}

		public ActionResult Search(CatalogExtendedPagingFilteringModel command)
		{

			var messageModel = new MessageModel();
			var QLength = _catalogSettings.ProductSearchTermMinimumLength;

			if (command.Q == null || command.Q.Length < QLength)
			{
				messageModel.MessageList.Add(_localizationService.GetResource("SearchResult.TermMinimumLength"));
				messageModel.ActionText = _localizationService.GetResource("SearchResult.Empty.Continue");
				messageModel.ActionUrl = ControllerContext.HttpContext.Request.ApplicationPath;
				ViewBag.MessageModel = messageModel;
				return View(
					new SearchModel
					{
						SearchReturnTitle = _localizationService.GetResource("SearchResult.Text"),
						Q = command.Q
					});
			}
			_customerService.SaveCustomerAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.LastContinueShoppingPage, Url.RouteUrl("ProductSearch", new { Q = command.Q }));
			var model = new SearchModel();
			StatefulStorage.PerSession.Remove<IList<ProductModel>>("searchproducts");
			var baseProducts = StatefulStorage.PerSession.GetOrAdd("searchproducts", () => GetProductsModel(command));
			if (command.PageSize <= 0) command.PageSize = _catalogSettings.SearchPageProductsPerPage;
			if (command.PageNumber <= 0) command.PageNumber = 1;
			var productsSet = GetFilteredProducts(command, baseProducts);
			model.Products = productsSet["all"];
			model.Q = command.Q;
			model.SearchReturnTitle = (_localizationService.GetResource("SearchResult.Text"));
			LoadFilteringContext(model, command, productsSet, extended: false);
			command.OuterSorting = ProductOuterSortingEnumAF.Category;
			SelectPagedProducts(model, command);
			if (model.Products.Count > 0)
				return View(model);

			messageModel.MessageList.Add(_localizationService.GetResource("SearchResult.EmptyResult"));
			messageModel.ActionText = _localizationService.GetResource("SearchResult.Empty.Continue");
			messageModel.ActionUrl = ControllerContext.HttpContext.Request.ApplicationPath;
			ViewBag.MessageModel = messageModel;

			//Customer Quate 
			SetProductVariantPriceModel(model.Products);

			// model.PagingFilteringContext.ViewMode = "grid2";
			return View(model);
		}

		public ActionResult Search280(CatalogPagingFilteringModel280 command)
		{
			GetCatalogPagingFilteringModel280FromQueryString(ref command);

			var messageModel = new MessageModel();
			var QLength = _catalogSettings.ProductSearchTermMinimumLength;

			if (command.Q == null || command.Q.Length < QLength)
			{
				messageModel.MessageList.Add(_localizationService.GetResource("SearchResult.TermMinimumLength"));
				messageModel.ActionText = _localizationService.GetResource("SearchResult.Empty.Continue");
				messageModel.ActionUrl = ControllerContext.HttpContext.Request.ApplicationPath;
				ViewBag.MessageModel = messageModel;
				return View(
					new SearchProductsModel280
					{
						SearchReturnTitle = _localizationService.GetResource("SearchResult.Text"),
						Q = command.Q
					});
			}
			_customerService.SaveCustomerAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.LastContinueShoppingPage, Url.RouteUrl("ProductSearch", new { Q = command.Q }));

			var model = new SearchProductsModel280();
			model.Q = command.Q;
			model.SearchReturnTitle = (_localizationService.GetResource("SearchResult.Text"));
			if (command.PageSize <= 0) command.PageSize = _catalogSettings.SearchPageProductsPerPage;
			if (command.PageNumber <= 0) command.PageNumber = 1;
			//price ranges
			decimal? minPriceConverted = null;
			decimal? maxPriceConverted = null;
			if (command.PriceRangeFilter.From.HasValue)
			{
				minPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(command.PriceRangeFilter.From.Value, _workContext.WorkingCurrency);
			}
			if (command.PriceRangeFilter.To.HasValue)
			{
				maxPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(command.PriceRangeFilter.To.Value, _workContext.WorkingCurrency);
			}
			var categoryIds = new List<int>();
			//products
			IList<int> alreadyFilteredSpecOptionIds = command.SpecificationFilter.GetFilteredOptionIds();
			IList<int> alreadyFilteredProductVariantAttributeIds = command.AttributeFilter.GetProductVariantAttributeIds();
			IList<int> alreadyFilteredManufacturerIds = command.ManufacturerFilter.GetFilteredManufacturerIds();
			IList<int> alreadyFilteredCategoryIds = command.CategoryFilter.GetFilteredCategoryIds();



			string productListModelCacheKey = string.Format(PRODUCT_LIST_MODEL, string.Join(",", categoryIds), minPriceConverted, maxPriceConverted, _workContext.WorkingLanguage.Id
				, string.Join(",", alreadyFilteredSpecOptionIds), string.Join(",", alreadyFilteredProductVariantAttributeIds)
				, string.Join(",", alreadyFilteredManufacturerIds), command.OrderBy, command.PageNumber, command.PageSize, command.Q, currencyId);
			ProductListwithOverviewModel pm = new ProductListwithOverviewModel();

			if (Request.Params["cc"] != null && Request.Params["cc"].ToString().ToLowerInvariant().Equals("true"))
				_cacheManager.Remove(productListModelCacheKey);

			pm = _cacheManager.Get(productListModelCacheKey, int.MaxValue, () =>
			{
				IList<int> filterableSpecificationAttributeOptionIds = null;
				IList<int> filterableProductVariantAttributeIds = null;
				IList<int> filterableManufacturerIds = null;
				IList<int> filterableCategoryIds = null;
				decimal? maxPrice = null;

				pm.Products = _productService.SearchProductsFilterable(
					categoryIds, 0, null, minPriceConverted, maxPriceConverted,
					0, command.Q, true, false, _workContext.WorkingLanguage.Id,
					alreadyFilteredSpecOptionIds, alreadyFilteredProductVariantAttributeIds,
					alreadyFilteredManufacturerIds, (ProductSortingEnum)command.OrderBy, command.PageNumber - 1, command.PageSize, true,
					out filterableSpecificationAttributeOptionIds,
					out filterableProductVariantAttributeIds,
					out filterableManufacturerIds,
					out filterableCategoryIds,
					out maxPrice);

				pm.FilterableSpecificationAttributeOptionIds = filterableSpecificationAttributeOptionIds;
				pm.FilterableProductVariantAttributeIds = filterableProductVariantAttributeIds;
				pm.FilterableManufacturerIds = filterableManufacturerIds;
				pm.FilterableCategoryIds = filterableCategoryIds;
				pm.MaxPrice = maxPrice;

				pm.ProductsOverview = PrepareProductOverviewModels280(pm.Products).ToList();
				return pm;
			});

			model.Products = pm.ProductsOverview;



			model.PagingFilteringContext.LoadPagedList(pm.Products);
			model.PagingFilteringContext.ViewMode = command.ViewMode;

			//specs
			model.PagingFilteringContext.SpecificationFilter.PrepareSpecsFilters(alreadyFilteredSpecOptionIds,
				pm.FilterableSpecificationAttributeOptionIds,
				_specificationAttributeService, _webHelper, _workContext);

			//attributes
			//model.PagingFilteringContext.AttributeFilter.PrepareAttributeFilters(alreadyFilteredProductVariantAttributeIds,
			//  filterableProductVariantAttributeIds,
			//  _productAttributeService, _webHelper, _workContext);

			//maufacturers
			model.PagingFilteringContext.ManufacturerFilter.PrepareManufacturerFilters(alreadyFilteredManufacturerIds, pm.FilterableManufacturerIds, _manufacturerService, _webHelper, _workContext);

			//categories
			model.PagingFilteringContext.CategoryFilter.PrepareCategoryFilters(alreadyFilteredCategoryIds, pm.FilterableCategoryIds, _categoryService, _webHelper, _workContext);

			//pricerange
			model.PagingFilteringContext.PriceRangeFilter.PreparePriceRangeFilters(0, pm.MaxPrice, command.PriceRangeFilter.From, command.PriceRangeFilter.To, _webHelper, _workContext);

			model.SearchReturnTitle = (_localizationService.GetResource("SearchResult.Text"));
			if (model.Products.Count > 0)
				return View(model);

			messageModel.MessageList.Add(_localizationService.GetResource("SearchResult.EmptyResult"));
			messageModel.ActionText = _localizationService.GetResource("SearchResult.Empty.Continue");
			messageModel.ActionUrl = ControllerContext.HttpContext.Request.ApplicationPath;
			ViewBag.MessageModel = messageModel;

			return View(model);
		}

		[AjaxOnly]
		public ActionResult SearchJS280(CatalogPagingFilteringModel280 command)
		{
			var model = new SearchProductsModel280();

			//page size
			if (command.PageSize <= 0) command.PageSize = _catalogSettings.SearchPageProductsPerPage;
			if (command.PageNumber <= 0) command.PageNumber = 1;

			//price ranges
			decimal? minPriceConverted = null;
			decimal? maxPriceConverted = null;
			if (command.PriceRangeFilter.From.HasValue)
			{
				minPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(command.PriceRangeFilter.From.Value, _workContext.WorkingCurrency);
			}
			if (command.PriceRangeFilter.To.HasValue)
			{
				maxPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(command.PriceRangeFilter.To.Value, _workContext.WorkingCurrency);
			}

			//products
			IList<int> alreadyFilteredSpecOptionIds = command.SpecificationFilter.GetFilteredOptionIds();
			IList<int> alreadyFilteredProductVariantAttributeIds = command.AttributeFilter.GetProductVariantAttributeIds();
			IList<int> alreadyFilteredManufacturerIds = command.ManufacturerFilter.GetFilteredManufacturerIds();
			IList<int> alreadyFilteredCategoryIds = command.CategoryFilter.GetFilteredCategoryIds();
			var categoryIds = alreadyFilteredCategoryIds;



			string productListModelCacheKey = string.Format(PRODUCT_LIST_MODEL, string.Join(",", categoryIds), minPriceConverted, maxPriceConverted, _workContext.WorkingLanguage.Id
				, string.Join(",", alreadyFilteredSpecOptionIds), string.Join(",", alreadyFilteredProductVariantAttributeIds)
				, string.Join(",", alreadyFilteredManufacturerIds), command.OrderBy, command.PageNumber, command.PageSize, command.Q, currencyId);
			ProductListwithOverviewModel pm = new ProductListwithOverviewModel();

			pm = _cacheManager.Get(productListModelCacheKey, () =>
			{
				IList<int> filterableSpecificationAttributeOptionIds = null;
				IList<int> filterableProductVariantAttributeIds = null;
				IList<int> filterableManufacturerIds = null;
				IList<int> filterableCategoryIds = null;
				decimal? maxPrice = null;

				pm.Products = _productService.SearchProductsFilterable(
					categoryIds, 0, null, minPriceConverted, maxPriceConverted,
					0, command.Q, true, false, _workContext.WorkingLanguage.Id,
					alreadyFilteredSpecOptionIds, alreadyFilteredProductVariantAttributeIds,
					alreadyFilteredManufacturerIds, (ProductSortingEnum)command.OrderBy, command.PageNumber - 1, command.PageSize, true,
					out filterableSpecificationAttributeOptionIds,
					out filterableProductVariantAttributeIds,
					out filterableManufacturerIds,
					out filterableCategoryIds,
					out maxPrice);

				pm.FilterableSpecificationAttributeOptionIds = filterableSpecificationAttributeOptionIds;
				pm.FilterableProductVariantAttributeIds = filterableProductVariantAttributeIds;
				pm.FilterableManufacturerIds = filterableManufacturerIds;
				pm.FilterableCategoryIds = filterableCategoryIds;
				pm.MaxPrice = maxPrice;

				pm.ProductsOverview = PrepareProductOverviewModels280(pm.Products).ToList();
				return pm;
			});

			model.Products = pm.ProductsOverview;



			model.PagingFilteringContext.LoadPagedList(pm.Products);
			//model.PagingFilteringContext.ViewMode = viewMode;

			//specs
			model.PagingFilteringContext.SpecificationFilter.PrepareSpecsFilters(alreadyFilteredSpecOptionIds,
				pm.FilterableSpecificationAttributeOptionIds,
				_specificationAttributeService, _webHelper, _workContext);

			//attributes
			//model.PagingFilteringContext.AttributeFilter.PrepareAttributeFilters(alreadyFilteredProductVariantAttributeIds,
			//  filterableProductVariantAttributeIds,
			//  _productAttributeService, _webHelper, _workContext);

			//maufacturers
			model.PagingFilteringContext.ManufacturerFilter.PrepareManufacturerFilters(alreadyFilteredManufacturerIds, pm.FilterableManufacturerIds, _manufacturerService, _webHelper, _workContext);

			//categories
			model.PagingFilteringContext.CategoryFilter.PrepareCategoryFilters(alreadyFilteredCategoryIds, pm.FilterableCategoryIds, _categoryService, _webHelper, _workContext);

			//pricerange
			model.PagingFilteringContext.PriceRangeFilter.PreparePriceRangeFilters(0, pm.MaxPrice, command.PriceRangeFilter.From, command.PriceRangeFilter.To, _webHelper, _workContext);

			if (command.FilterTrigging == "C" || command.FilterTrigging == "M" || command.FilterTrigging == "S" || command.FilterTrigging == "A")
				model.PagingFilteringContext.ShowFilteringPanel = true;
			else
				model.PagingFilteringContext.ShowFilteringPanel = false;

			string view = null;
			if (command.ViewMode == "grid1")
				view = @"~/Views/Catalog/ProductsList1280.cshtml";
			else if (command.ViewMode == "grid2")
				view = @"~/Views/Catalog/ProductsList2280.cshtml";
			else
				view = @"~/Views/Catalog/ProductsList3280.cshtml";

			return Json(new
			{
				Html = Utilities.RenderPartialViewToString(this, view, model),
				SelectionHtml = Utilities.RenderPartialViewToString(this, "~/Views/Catalog/_yourSelection280.cshtml", model.PagingFilteringContext),
				PriceMin = model.PagingFilteringContext.PriceRangeFilter.Min,
				PriceMax = model.PagingFilteringContext.PriceRangeFilter.Max,
				PriceValue = model.PagingFilteringContext.PriceRangeFilter.To,
				PriceStep = model.PagingFilteringContext.PriceRangeFilter.StepSize,
				HasMore = model.PagingFilteringContext.HasNextPage,
				PageNumber = model.PagingFilteringContext.PageNumber,
				PageSize = model.PagingFilteringContext.PageSize,
				ViewMode = command.ViewMode
			});
		}


		[AjaxOnly]
		public ActionResult SearchJS(CatalogExtendedPagingFilteringModel command)
		{
			var model = new SearchModel();
			var baseProducts = StatefulStorage.PerSession.GetOrAdd("searchproducts", () => GetProductsModel(command));
			if (command.PageSize <= 0) command.PageSize = _catalogSettings.SearchPageProductsPerPage;
			if (command.PageNumber <= 0) command.PageNumber = 1;
			var productsSet = GetFilteredProducts(command, baseProducts);
			model.Products = productsSet["all"];
			LoadFilteringContext(model, command, productsSet, extended: false);
			command.OuterSorting = ProductOuterSortingEnumAF.Category;
			SelectPagedProducts(model, command);
			string view = null;
			if (command.ViewMode == "grid1")
				view = @"~/Views/Catalog/ProductsList1.cshtml";
			else if (command.ViewMode == "grid2")
				view = @"~/Views/Catalog/ProductsList2.cshtml";
			else
				view = @"~/Views/Catalog/ProductsList3.cshtml";

			//Customer Quate 
			SetProductVariantPriceModel(model.Products);
			return Json(new
			{
				Html = Utilities.RenderPartialViewToString(this, view, model),
				SelectionHtml = Utilities.RenderPartialViewToString(this, "~/Views/Catalog/_yourSelection.cshtml", model.PagingFilteringContext),
				PriceMin = model.PagingFilteringContext.PriceRangeSliderFilter.Item.Min,
				PriceMax = model.PagingFilteringContext.PriceRangeSliderFilter.Item.Max,
				PriceValue = model.PagingFilteringContext.PriceRangeSliderFilter.Item.Value,
				PriceStep = model.PagingFilteringContext.PriceRangeSliderFilter.Item.StepSize,
				HasMore = model.PagingFilteringContext.HasNextPage,
				PageNumber = model.PagingFilteringContext.PageNumber,
				PageSize = model.PagingFilteringContext.PageSize,
				ViewMode = command.ViewMode
			});
		}

		public ActionResult SearchExt(CatalogExtendedPagingFilteringModel command)
		{
			var messageModel = new MessageModel();
			var QLength = _catalogSettings.ProductSearchTermMinimumLength;
			if (command.Q.Length < QLength)
			{
				messageModel.MessageList.Add(_localizationService.GetResource("SearchResult.TermMinimumLength"));
				messageModel.ActionText = _localizationService.GetResource("SearchResult.Empty.Continue");
				messageModel.ActionUrl = ControllerContext.HttpContext.Request.ApplicationPath;
				ViewBag.MessageModel = messageModel;
				return View(
					new SearchModel
					{
						SearchReturnTitle = _localizationService.GetResource("SearchResult.Text"),
						Q = command.Q
					});
			}
			_customerService.SaveCustomerAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.LastContinueShoppingPage, Url.RouteUrl("ProductSearch", new { Q = command.Q }));
			var model = new SearchModel();
			if (command.PageSize <= 0) command.PageSize = _catalogSettings.SearchPageProductsPerPage;
			if (command.PageNumber <= 0) command.PageNumber = 1;
			// StatefulStorage.PerSession.Remove<IList<ProductModel>>("searchproducts");
			// var baseProducts = StatefulStorage.PerSession.GetOrAdd("searchproducts", () => GetProductsModel(command));

			IList<int> filterableSpecificationAttributeOptionIds = null;
			decimal? minPriceConverted = null;
			decimal? maxPriceConverted = null;
			bool searchInDescriptions = false;
			IPagedList<Product> products = new PagedList<Product>(new List<Product>(), 0, 1);
			products = _productService.SearchProductsExt(null, null, null,
				minPriceConverted, maxPriceConverted, 0,
				model.Q, searchInDescriptions, _workContext.WorkingLanguage.Id, null,
				ProductSortingEnum.Position, command.PageNumber - 1, command.PageSize,
				false, out filterableSpecificationAttributeOptionIds);
			model.Products = products.Select(x => PrepareProductOverviewPageModel(x)).ToList();

			if (model.Products.Count > 0)
				return View("Search", model);

			messageModel.MessageList.Add(_localizationService.GetResource("SearchResult.EmptyResult"));
			messageModel.ActionText = _localizationService.GetResource("SearchResult.Empty.Continue");
			messageModel.ActionUrl = ControllerContext.HttpContext.Request.ApplicationPath;
			ViewBag.MessageModel = messageModel;

			//Customer Quate 
			SetProductVariantPriceModel(model.Products);

			// model.PagingFilteringContext.ViewMode = "grid2";
			return View("Search", model);
		}

		#endregion Searching

		#region NewsItemProducts
		public ActionResult NewsItemProducts(int newsItemId, CatalogExtendedPagingFilteringModel command)
		{
			_customerService.SaveCustomerAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.LastContinueShoppingPage, _webHelper.GetThisPageUrl(false));
			var model = new NewsItemProductsModel();

			List<NewsItemProduct> publishVariantList = new List<NewsItemProduct>();       //Variantı publish olan ve silinmemiş ürünler için list oluşturuldu
			var allProductList = _newsService.GetNewsItemProductsByNewsItemId(newsItemId);

			foreach (var item in allProductList.ToList())
			{
				foreach (var i in item.Product.ProductVariants.Where(x => x.Published))
				{
					if (i.Published == true && i.Deleted == false)
					{
						publishVariantList.Add(item);
					}
				}
			}

			var baseProducts = publishVariantList.Select(x => PrepareProductOverviewPageModel(x.Product)).ToList();  // baseproduct ta variantı publish olan ürünler yer alır.
			if (command.PageSize <= 0) command.PageSize = _catalogSettings.SearchPageProductsPerPage;
			if (command.PageNumber <= 0) command.PageNumber = 1;
			var productsSet = GetFilteredProducts(command, baseProducts);
			model.Products = productsSet["all"];
			model.Id = newsItemId;
			var newsItem = _newsService.GetNewsById(newsItemId);
			model.Title = newsItem.Title;
			model.SeName = newsItem.GetSeName();
			LoadFilteringContext(model, command, productsSet);
			SelectPagedProducts(model, command);
			if (model.Products.Count > 0)
				return View(model);


			//Customer Quate 
			SetProductVariantPriceModel(model.Products);

			// model.PagingFilteringContext.ViewMode = "grid2";
			return View(model);
		}

		//AF
		// [AjaxCallErrorHandler(Reaction = AjaxErrorReaction.Redirect, Url = UrlHelper.GenerateUrl(null, "Index", "Controller", null, null, null, false))]
		public JsonResult NewsItemProductsJS(int newsItemId, CatalogExtendedPagingFilteringModel command)
		{
			var model = new NewsItemProductsModel();
			var baseProducts = _newsService.GetNewsItemProductsByNewsItemId(newsItemId).Select(x => PrepareProductOverviewPageModel(x.Product)).ToList();
			if (command.PageSize <= 0) command.PageSize = _catalogSettings.SearchPageProductsPerPage;
			if (command.PageNumber <= 0) command.PageNumber = 1;
			var productsSet = GetFilteredProducts(command, baseProducts);
			model.Products = productsSet["all"];
			LoadFilteringContext(model, command, productsSet);
			SelectPagedProducts(model, command);
			string view = null;
			if (command.ViewMode == "grid1")
				view = @"~/Views/Catalog/ProductsList1.cshtml";
			else if (command.ViewMode == "grid2")
				view = @"~/Views/Catalog/ProductsList2.cshtml";
			else
				view = @"~/Views/Catalog/ProductsList3.cshtml";

			//Customer Quute 
			SetProductVariantPriceModel(model.Products);
			return Json(new
			{
				Html = Utilities.RenderPartialViewToString(this, view, model),
				SelectionHtml = Utilities.RenderPartialViewToString(this, "~/Views/Catalog/_yourSelection.cshtml", model.PagingFilteringContext),
				PriceMin = model.PagingFilteringContext.PriceRangeSliderFilter.Item.Min,
				PriceMax = model.PagingFilteringContext.PriceRangeSliderFilter.Item.Max,
				PriceValue = model.PagingFilteringContext.PriceRangeSliderFilter.Item.Value,
				PriceStep = model.PagingFilteringContext.PriceRangeSliderFilter.Item.StepSize,
				HasMore = model.PagingFilteringContext.HasNextPage,
				PageNumber = model.PagingFilteringContext.PageNumber,
				PageSize = model.PagingFilteringContext.PageSize,
				ViewMode = command.ViewMode
			});

		}

		#endregion




		public ActionResult ProductsWithImages(CatalogPagingFilteringModel280 command)
		{
			GetCatalogPagingFilteringModel280FromQueryString(ref command);

			var model = new RecentlyAddedProductsModel280();
			if (command.PageSize <= 0) command.PageSize = 500;
			if (command.PageNumber <= 0) command.PageNumber = 1;

			ProductListwithOverviewModel pm = new ProductListwithOverviewModel();
			pm.Products = _productService.SearchProductsFilterable(0, int.MaxValue, 0, ProductSortingEnum.CreatedOn, command.PageNumber - 1, command.PageSize,
			   false, false, _workContext.WorkingLanguage.Id);
			//pm.Products.TotalCount = pm.Products.Count;
			pm.ProductsOverview = PrepareProductOverviewModels280(pm.Products, false).ToList();

			model.Products = pm.ProductsOverview;
			model.PagingFilteringContext.LoadPagedList(pm.Products);
			model.PagingFilteringContext.FilteringDisabled = true;
			model.PagingFilteringContext.PriceRangeFilter.Disabled = true;
			model.PagingFilteringContext.AllowProductSorting = false;
			model.PagingFilteringContext.ViewMode = command.ViewMode;
			model.IsGuest = _workContext.CurrentCustomer.IsGuest();
			//model.RecentlyAddedTotalProduct = pm.Products.TotalCount;
			return View("RecentlyAddedProducts280", model);
		}

		public ActionResult CreateProductImages(int pictureSize)
		{
			try
			{
				PictureRepository pr = new PictureRepository();
				List<Picture> pictures = pr.GetAllByProductAndVariant();
				foreach (Picture picture in pictures)
					_pictureService.GetPictureUrl(picture, pictureSize);
			}
			catch (Exception ex)
			{
				return Content(ex.Message + " - " + ex.StackTrace);
			}
			return Content("OK");
		}

        public ActionResult RecentlyAddedProductsRss()
        {
            var feed = new SyndicationFeed(
                                    string.Format("{0}: Recently added products", "Alwaysfashion"),
                                    "Information about products",
                                    new Uri(_webHelper.GetStoreLocation(false)),
                                    "RecentlyAddedProductsRSS",
                                    DateTime.UtcNow);

            if (!_catalogSettings.RecentlyAddedProductsEnabled)
                return new RssActionResult() { Feed = feed };

            var items = new List<SyndicationItem>();

            //var products = _productService.SearchProducts(0, 0, null, null, null, 0, string.Empty, false,
            //        _workContext.WorkingLanguage.Id, new List<int>(),
            //        ProductSortingEnum.CreatedOn, 0, int.MaxValue, true).Where(x => x.Published == true).Where(x => x.Deleted == false).ToList();

            var products = _productService.SearchProductsFilterable(null, null, 0, ProductSortingEnum.CreatedOn, 0, 500,
                        false, false);

            foreach (var product in products)
            {
                //string productUrl = Url.RouteUrl("Product", new { SeName = product.GetSeName() }, "http");
                var url = string.Format("{0}p/{1}/{2}{3}", _webHelper.GetStoreLocation(false), product.Id, product.GetSeName(), _settingService.GetSettingByKey<string>("Affiliate.Parameter"));

                items.Add(new SyndicationItem(product.GetLocalized(x => x.Name), product.GetLocalized(x => x.ShortDescription), new Uri(url), String.Format("Product:{0}", product.Id), product.CreatedOnUtc));
            }
            feed.Items = items;
            return new RssActionResult() { Feed = feed };
        }

        public ActionResult Rss20DataFeed()
        {
            string fileName = string.Format("Rss20DataFeed-{0}.xml", _workContext.WorkingLanguage.UniqueSeoCode);
            string filePath = System.IO.Path.Combine(HttpRuntime.AppDomainAppPath, "content\\files\\exportimport", fileName);
            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                GenerateFeed(fs);
            }

            return null;

            //var feed = new SyndicationFeed(
            //                        string.Format("{0}: Recently added products", "Hatemoglu"),
            //                        "Information about products",
            //                        new Uri(_webHelper.GetStoreLocation(false)),
            //                        "RecentlyAddedProductsRSS",
            //                        DateTime.UtcNow);

            //if (!_catalogSettings.RecentlyAddedProductsEnabled)
            //    return new RssActionResult() { Feed = feed };

            //var items = new List<SyndicationItem>();

            //var products = _productService.SearchProducts(0, 0, null, null, null, 0, string.Empty, false,
            //        _workContext.WorkingLanguage.Id, new List<int>(),
            //        ProductSortingEnum.CreatedOn, 0, int.MaxValue, true).Where(x => x.Published == true).Where(x => x.Deleted == false).ToList();

            //foreach (var product in products)
            //{
            //    //string productUrl = Url.RouteUrl("Product", new { SeName = product.GetSeName() }, "http");
            //    var url = string.Format("{0}p/{1}/{2}{3}", _webHelper.GetStoreLocation(false), product.Id, product.GetSeName(), _settingService.GetSettingByKey<string>("Affiliate.Parameter"));

            //    items.Add(new SyndicationItem(product.GetLocalized(x => x.Name), product.GetLocalized(x => x.ShortDescription), new Uri(url), String.Format("Product:{0}", product.Id), product.CreatedOnUtc));
            //}
            //feed.Items = items;
            //return new RssActionResult() { Feed = feed };
        }

        public void GenerateFeed(Stream stream)
        {
            TextInfo textInfo = new CultureInfo("tr-TR", false).TextInfo;
            
            const string googleBaseNamespace = "http://base.google.com/ns/1.0";

            var settings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8
            };
            using (var writer = XmlWriter.Create(stream, settings))
            {
                //Generate feed according to the following specs: http://www.google.com/support/merchants/bin/answer.py?answer=188494&expand=GB
                writer.WriteStartDocument();
                writer.WriteStartElement("rss");
                writer.WriteAttributeString("version", "2.0");
                writer.WriteAttributeString("xmlns", "g", null, googleBaseNamespace);
                writer.WriteStartElement("channel");
                writer.WriteElementString("title", string.Format("{0} Google Base", "www.alwaysfashion.com"));
                writer.WriteElementString("link", "http://base.google.com/base/");
                writer.WriteElementString("description", "Information about products");


                var products = _productService.GetAllProducts(false);
                foreach (var product in products)
                {
                    var productVariants = _productService.GetProductVariantsByProductId(product.Id, false);

                    foreach (var productVariant in productVariants)
                    {
                        try
                        {
                            writer.WriteStartElement("item");

                            #region Basic Product Information

                            //id [id]- An identifier of the item
                            writer.WriteElementString("g", "id", googleBaseNamespace, product.Id.ToString());

                            //title [title] - Title of the item
                            writer.WriteStartElement("title");
                            string title;
                            title = product.ShortDescription == null ? "" : product.GetLocalized(x => x.ShortDescription);
                            //title should be not longer than 70 characters
                            if (title != null && title.Length > 70)
                                title = title.Substring(0, 70);
                            writer.WriteCData(title);
                            writer.WriteEndElement(); // title

                            //description [description] - Description of the item
                            writer.WriteStartElement("description");
                            string description = product.GetLocalized(x => x.FullDescription);
                            if (String.IsNullOrEmpty(description))
                                description = productVariant.GetLocalized(x => x.Description);
                            if (String.IsNullOrEmpty(description))
                                description = product.GetLocalized(x => x.ShortDescription);
                            if (String.IsNullOrEmpty(description))
                                description = product.GetLocalized(x => x.Name);
                            if (String.IsNullOrEmpty(description))
                                description = productVariant.GetLocalized(x => x.FullProductName); //description is required
                            writer.WriteCData(description);
                            writer.WriteEndElement(); // description



                            ////google product category [google_product_category] - Google's category of the item
                            ////the category of the product according to Google’s product taxonomy. http://www.google.com/support/merchants/bin/answer.py?answer=160081
                            //string googleProductCategory = "";
                            ////var googleProduct = _googleService.GetByProductVariantId(productVariant.Id);
                            ////if (googleProduct != null)
                            ////    googleProductCategory = googleProduct.Taxonomy;
                            ////if (String.IsNullOrEmpty(googleProductCategory))
                            ////    googleProductCategory = _froogleSettings.DefaultGoogleCategory;
                            ////if (String.IsNullOrEmpty(googleProductCategory))
                            ////    throw new NopException("Default Google category is not set");

                            //googleProductCategory = "Apparel & Accessories"; // VARSAYILAN KATEGORI OLARAK ATADIK..
                            //writer.WriteElementString("g", "google_product_category", googleBaseNamespace,
                            //                          HttpUtility.HtmlEncode(googleProductCategory));


                            //product type [product_type] - Your category of the item
                            var defaultProductCategory = _categoryService.GetProductCategoriesByProductId(product.Id).FirstOrDefault();
                            if (defaultProductCategory != null)
                            {
                                var categoryBreadCrumb = GetCategoryBreadCrumb(defaultProductCategory.Category);
                                string yourProductCategory = "";
                                for (int i = 0; i < categoryBreadCrumb.Count; i++)
                                {
                                    var cat = categoryBreadCrumb[i];
                                    yourProductCategory = yourProductCategory + textInfo.ToTitleCase(cat.GetLocalized(x => x.Name));
                                    if (i != categoryBreadCrumb.Count - 1)
                                        yourProductCategory = yourProductCategory + " > ";
                                }
                                if (!String.IsNullOrEmpty((yourProductCategory)))
                                    writer.WriteElementString("g", "product_type", googleBaseNamespace,
                                                              HttpUtility.HtmlEncode(yourProductCategory));
                            }

                            //link [link] - URL directly linking to your item's page on your website
                            var productUrl = string.Format("{0}{1}/p/{2}/{3}", _webHelper.GetStoreLocation(false), _workContext.WorkingLanguage.UniqueSeoCode, product.Id,
                                                           product.GetSeName()); //+ "?variantId=" + productVariant.Id.ToString());
                            //productUrl = productUrl.AddLanguageSeoCodeToRawUrl("", _workContext.WorkingLanguage);
                            writer.WriteElementString("link", productUrl);

                            //image link [image_link] - URL of an image of the item
                            string imageUrl;
                            var picture = product.GetDefaultProductPicture(_pictureService); //.GetDefaultProductVariantPicture(_pictureService); //_pictureService.GetPictureById(productVariant.PictureId);
                            //if (picture == null)
                            //    picture = product.GetDefaultProductPicture(_pictureService); //_pictureService.GetPicturesByProductId(product.Id, 1).FirstOrDefault();

                            //always use HTTP when getting image URL
                            if (picture != null)
                                imageUrl = _pictureService.GetPictureUrl(picture, 612);
                            else
                                imageUrl = _pictureService.GetDefaultPictureUrl(612);

                            writer.WriteElementString("g", "image_link", googleBaseNamespace, imageUrl);

                            //condition [condition] - Condition or state of the item
                            writer.WriteElementString("g", "condition", googleBaseNamespace, "new");

                            #endregion

                            #region Availability & Price

                            //availability [availability] - Availability status of the item
                            string availability = "in stock"; //in stock by default
                            if (productVariant.ManageInventoryMethod == ManageInventoryMethod.ManageStock
                                && productVariant.StockQuantity <= 0)
                            {
                                switch (productVariant.BackorderMode)
                                {
                                    case BackorderMode.NoBackorders:
                                        {
                                            availability = "out of stock";
                                        }
                                        break;
                                    case BackorderMode.AllowQtyBelow0:
                                    case BackorderMode.AllowQtyBelow0AndNotifyCustomer:
                                        {
                                            availability = "available for order";
                                            //availability = "preorder";
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                            writer.WriteElementString("g", "availability", googleBaseNamespace, availability);

                            //price [price] - Price of the item
                            var currency = _workContext.WorkingCurrency; //GetUsedCurrency();
                        
                            decimal taxRate = decimal.Zero;
                        
                            decimal finalPriceWithOutDiscountBase = _taxService.GetProductPrice(productVariant, _priceCalculationService.GetFinalPrice(productVariant, false, false), out taxRate);
                            decimal finalPriceWithOutDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithOutDiscountBase, _workContext.WorkingCurrency);

                            if (finalPriceWithOutDiscount.ToString() != null)
                            {
                                //xmlWriter.WriteElementString("DiscountPrice", null, finalPriceWithDiscount.ToString());
                                writer.WriteElementString("g", "price", googleBaseNamespace,
                                                        finalPriceWithOutDiscount.ToString(new CultureInfo("en-US", false).NumberFormat) + " " +
                                                        currency.CurrencyCode);
                            }

                            taxRate = decimal.Zero;
                            decimal finalPriceWithDiscountBase = _taxService.GetProductPrice(productVariant, _priceCalculationService.GetFinalPrice(productVariant, true, false), out taxRate);
                            decimal finalPriceWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithDiscountBase, _workContext.WorkingCurrency);

                            if (finalPriceWithDiscount.ToString() != null)
                            {
                                //xmlWriter.WriteElementString("DiscountPrice", null, finalPriceWithDiscount.ToString());
                                writer.WriteElementString("g", "sale_price", googleBaseNamespace,
                                                        finalPriceWithDiscount.ToString(new CultureInfo("en-US", false).NumberFormat) + " " +
                                                        currency.CurrencyCode);
                            }

                            //decimal price = _currencyService.ConvertFromPrimaryStoreCurrency(productVariant.Price, currency);
                            //writer.WriteElementString("g", "price", googleBaseNamespace,
                            //                          price.ToString(new CultureInfo("en-US", false).NumberFormat) + " " +
                            //                          currency.CurrencyCode);
                        
                        

                            #endregion

                            #region Unique Product Identifiers

                            /* Unique product identifiers such as UPC, EAN, JAN or ISBN allow us to show your listing on the appropriate product page. If you don't provide the required unique product identifiers, your store may not appear on product pages, and all your items may be removed from Product Search.
                             * We require unique product identifiers for all products - except for custom made goods. For apparel, you must submit the 'brand' attribute. For media (such as books, movies, music and video games), you must submit the 'gtin' attribute. In all cases, we recommend you submit all three attributes.
                             * You need to submit at least two attributes of 'brand', 'gtin' and 'mpn', but we recommend that you submit all three if available. For media (such as books, movies, music and video games), you must submit the 'gtin' attribute, but we recommend that you include 'brand' and 'mpn' if available.
                            */

                            //GTIN [gtin] - GTIN
                            var gtin = productVariant.Gtin;
                            if (!String.IsNullOrEmpty(gtin))
                            {
                                writer.WriteStartElement("g", "gtin", googleBaseNamespace);
                                writer.WriteCData(gtin);
                                writer.WriteFullEndElement(); // g:gtin
                            }

                            //brand [brand] - Brand of the item
                            var defaultManufacturer =
                                _manufacturerService.GetProductManufacturersByProductId((product.Id)).FirstOrDefault();
                            if (defaultManufacturer != null)
                            {
                                writer.WriteStartElement("g", "brand", googleBaseNamespace);
                                writer.WriteCData(defaultManufacturer.Manufacturer.GetLocalized(x => x.Name));
                                writer.WriteFullEndElement(); // g:brand
                            }


                            //mpn [mpn] - Manufacturer Part Number (MPN) of the item
                            var mpn = productVariant.Sku;
                            if (!String.IsNullOrEmpty(mpn))
                            {
                                writer.WriteStartElement("g", "mpn", googleBaseNamespace);
                                writer.WriteCData(mpn);
                                writer.WriteFullEndElement(); // g:mpn
                            }

                            #endregion

                            #region Tax & Shipping

                            //tax [tax]
                            //The tax attribute is an item-level override for merchant-level tax settings as defined in your Google Merchant Center account. This attribute is only accepted in the US, if your feed targets a country outside of the US, please do not use this attribute.
                            //IMPORTANT NOTE: Set tax in your Google Merchant Center account settings

                            //IMPORTANT NOTE: Set shipping in your Google Merchant Center account settings

                            ////shipping weight [shipping_weight] - Weight of the item for shipping
                            ////We accept only the following units of weight: lb, oz, g, kg.
                            //if (_froogleSettings.PassShippingInfo)
                            //{
                            //    var weightName = "kg";
                            //    var shippingWeight = productVariant.Weight;
                            //    switch (_measureService.GetMeasureWeightById(_measureSettings.BaseWeightId).SystemKeyword)
                            //    {
                            //        case "ounce":
                            //            weightName = "oz";
                            //            break;
                            //        case "lb":
                            //            weightName = "lb";
                            //            break;
                            //        case "grams":
                            //            weightName = "g";
                            //            break;
                            //        case "kg":
                            //            weightName = "kg";
                            //            break;
                            //        default:
                            //            //unknown weight 
                            //            weightName = "kg";
                            //            break;
                            //    }
                            //    writer.WriteElementString("g", "shipping_weight", googleBaseNamespace, string.Format(CultureInfo.InvariantCulture, "{0} {1}", shippingWeight.ToString(new CultureInfo("en-US", false).NumberFormat), weightName));
                            //}

                            #endregion

                            writer.WriteElementString("g", "expiration_date", googleBaseNamespace, DateTime.Now.AddDays(28).ToString("yyyy-MM-dd"));


                            writer.WriteEndElement(); // item
                        }
                        catch (Exception ex)
                        {
                            _logger.Error("Google XML product feed oluşturulurken hata! Hatanın oluştuğu ürün Id: " + product.Id.ToString(), ex);
                            throw ex;
                        }
                        
                    }
                }

                writer.WriteEndElement(); // channel
                writer.WriteEndElement(); // rss
                writer.WriteEndDocument();
            }

















            #region FromNop310
            //if (stream == null)
            //    throw new ArgumentNullException("stream");

            //const string googleBaseNamespace = "http://base.google.com/ns/1.0";

            //var settings = new XmlWriterSettings
            //{
            //    Encoding = Encoding.UTF8
            //};
            //using (var writer = XmlWriter.Create(stream, settings))
            //{
            //    //Generate feed according to the following specs: http://www.google.com/support/merchants/bin/answer.py?answer=188494&expand=GB
            //    writer.WriteStartDocument();
            //    writer.WriteStartElement("rss");
            //    writer.WriteAttributeString("version", "2.0");
            //    writer.WriteAttributeString("xmlns", "g", null, googleBaseNamespace);
            //    writer.WriteStartElement("channel");
            //    writer.WriteElementString("title", "Google Base feed");
            //    writer.WriteElementString("link", "http://base.google.com/base/");
            //    writer.WriteElementString("description", "Information about products");

            //    var products1 = _productService.GetAllProducts();
            //    foreach (var product1 in products1)
            //    {
            //        var productsToProcess = new List<Product>();
            //        switch (product1.ProductType)
            //        {
            //            case ProductType.SimpleProduct:
            //                {
            //                    //simple product doesn't have child products
            //                    productsToProcess.Add(product1);
            //                }
            //                break;
            //            case ProductType.GroupedProduct:
            //                {
            //                    //grouped products could have several child products
            //                    var associatedProducts = _productService.SearchProducts(
            //                        storeId: store.Id,
            //                        visibleIndividuallyOnly: false,
            //                        parentGroupedProductId: product1.Id
            //                        );
            //                    productsToProcess.AddRange(associatedProducts);
            //                }
            //                break;
            //            default:
            //                continue;
            //        }
            //        foreach (var product in productsToProcess)
            //        {
            //            writer.WriteStartElement("item");

            //            #region Basic Product Information

            //            //id [id]- An identifier of the item
            //            writer.WriteElementString("g", "id", googleBaseNamespace, product.Id.ToString());

            //            //title [title] - Title of the item
            //            writer.WriteStartElement("title");
            //            var title = product.Name;
            //            //title should be not longer than 70 characters
            //            if (title.Length > 70)
            //                title = title.Substring(0, 70);
            //            writer.WriteCData(title);
            //            writer.WriteEndElement(); // title

            //            //description [description] - Description of the item
            //            writer.WriteStartElement("description");
            //            string description = product.FullDescription;
            //            if (String.IsNullOrEmpty(description))
            //                description = product.ShortDescription;
            //            if (String.IsNullOrEmpty(description))
            //                description = product.Name;
            //            if (String.IsNullOrEmpty(description))
            //                description = product.Name; //description is required
            //            //resolving character encoding issues in your data feed
            //            description = StripInvalidChars(description, true);
            //            writer.WriteCData(description);
            //            writer.WriteEndElement(); // description



            //            //google product category [google_product_category] - Google's category of the item
            //            //the category of the product according to Google’s product taxonomy. http://www.google.com/support/merchants/bin/answer.py?answer=160081
            //            string googleProductCategory = "";
            //            var googleProduct = _googleService.GetByProductId(product.Id);
            //            if (googleProduct != null)
            //                googleProductCategory = googleProduct.Taxonomy;
            //            if (String.IsNullOrEmpty(googleProductCategory))
            //                googleProductCategory = _froogleSettings.DefaultGoogleCategory;
            //            if (String.IsNullOrEmpty(googleProductCategory))
            //                throw new NopException("Default Google category is not set");
            //            writer.WriteStartElement("g", "google_product_category", googleBaseNamespace);
            //            writer.WriteCData(googleProductCategory);
            //            writer.WriteFullEndElement(); // g:google_product_category

            //            //product type [product_type] - Your category of the item
            //            var defaultProductCategory = _categoryService.GetProductCategoriesByProductId(product.Id).FirstOrDefault();
            //            if (defaultProductCategory != null)
            //            {
            //                var category = defaultProductCategory.Category.GetFormattedBreadCrumb(_categoryService, separator: ">");
            //                if (!String.IsNullOrEmpty((category)))
            //                {
            //                    writer.WriteStartElement("g", "product_type", googleBaseNamespace);
            //                    writer.WriteCData(category);
            //                    writer.WriteFullEndElement(); // g:product_type
            //                }
            //            }

            //            //link [link] - URL directly linking to your item's page on your website
            //            var productUrl = string.Format("{0}{1}", store.Url, product.GetSeName(_workContext.WorkingLanguage.Id));
            //            writer.WriteElementString("link", productUrl);

            //            //image link [image_link] - URL of an image of the item
            //            string imageUrl;
            //            var picture = _pictureService.GetPicturesByProductId(product.Id, 1).FirstOrDefault();

            //            if (picture != null)
            //                imageUrl = _pictureService.GetPictureUrl(picture, _froogleSettings.ProductPictureSize, storeLocation: store.Url);
            //            else
            //                imageUrl = _pictureService.GetDefaultPictureUrl(_froogleSettings.ProductPictureSize, storeLocation: store.Url);

            //            writer.WriteElementString("g", "image_link", googleBaseNamespace, imageUrl);

            //            //condition [condition] - Condition or state of the item
            //            writer.WriteElementString("g", "condition", googleBaseNamespace, "new");

            //            writer.WriteElementString("g", "expiration_date", googleBaseNamespace, DateTime.Now.AddDays(28).ToString("yyyy-MM-dd"));

            //            #endregion

            //            #region Availability & Price

            //            //availability [availability] - Availability status of the item
            //            string availability = "in stock"; //in stock by default
            //            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock
            //                && product.StockQuantity <= 0)
            //            {
            //                switch (product.BackorderMode)
            //                {
            //                    case BackorderMode.NoBackorders:
            //                        {
            //                            availability = "out of stock";
            //                        }
            //                        break;
            //                    case BackorderMode.AllowQtyBelow0:
            //                    case BackorderMode.AllowQtyBelow0AndNotifyCustomer:
            //                        {
            //                            availability = "available for order";
            //                            //availability = "preorder";
            //                        }
            //                        break;
            //                    default:
            //                        break;
            //                }
            //            }
            //            writer.WriteElementString("g", "availability", googleBaseNamespace, availability);

            //            //price [price] - Price of the item
            //            var currency = GetUsedCurrency();
            //            decimal price = _currencyService.ConvertFromPrimaryStoreCurrency(product.Price, currency);
            //            writer.WriteElementString("g", "price", googleBaseNamespace,
            //                                      price.ToString(new CultureInfo("en-US", false).NumberFormat) + " " +
            //                                      currency.CurrencyCode);

            //            #endregion

            //            #region Unique Product Identifiers

            //            /* Unique product identifiers such as UPC, EAN, JAN or ISBN allow us to show your listing on the appropriate product page. If you don't provide the required unique product identifiers, your store may not appear on product pages, and all your items may be removed from Product Search.
            //             * We require unique product identifiers for all products - except for custom made goods. For apparel, you must submit the 'brand' attribute. For media (such as books, movies, music and video games), you must submit the 'gtin' attribute. In all cases, we recommend you submit all three attributes.
            //             * You need to submit at least two attributes of 'brand', 'gtin' and 'mpn', but we recommend that you submit all three if available. For media (such as books, movies, music and video games), you must submit the 'gtin' attribute, but we recommend that you include 'brand' and 'mpn' if available.
            //            */

            //            //GTIN [gtin] - GTIN
            //            var gtin = product.Gtin;
            //            if (!String.IsNullOrEmpty(gtin))
            //            {
            //                writer.WriteStartElement("g", "gtin", googleBaseNamespace);
            //                writer.WriteCData(gtin);
            //                writer.WriteFullEndElement(); // g:gtin
            //            }

            //            //brand [brand] - Brand of the item
            //            var defaultManufacturer =
            //                _manufacturerService.GetProductManufacturersByProductId((product.Id)).FirstOrDefault();
            //            if (defaultManufacturer != null)
            //            {
            //                writer.WriteStartElement("g", "brand", googleBaseNamespace);
            //                writer.WriteCData(defaultManufacturer.Manufacturer.Name);
            //                writer.WriteFullEndElement(); // g:brand
            //            }


            //            //mpn [mpn] - Manufacturer Part Number (MPN) of the item
            //            var mpn = product.ManufacturerPartNumber;
            //            if (!String.IsNullOrEmpty(mpn))
            //            {
            //                writer.WriteStartElement("g", "mpn", googleBaseNamespace);
            //                writer.WriteCData(mpn);
            //                writer.WriteFullEndElement(); // g:mpn
            //            }

            //            #endregion

            //            #region Apparel Products

            //            /* Apparel includes all products that fall under 'Apparel & Accessories' (including all sub-categories)
            //             * in Google’s product taxonomy.
            //            */

            //            //gender [gender] - Gender of the item
            //            if (googleProduct != null && !String.IsNullOrEmpty(googleProduct.Gender))
            //            {
            //                writer.WriteStartElement("g", "gender", googleBaseNamespace);
            //                writer.WriteCData(googleProduct.Gender);
            //                writer.WriteFullEndElement(); // g:gender
            //            }

            //            //age group [age_group] - Target age group of the item
            //            if (googleProduct != null && !String.IsNullOrEmpty(googleProduct.AgeGroup))
            //            {
            //                writer.WriteStartElement("g", "age_group", googleBaseNamespace);
            //                writer.WriteCData(googleProduct.AgeGroup);
            //                writer.WriteFullEndElement(); // g:age_group
            //            }

            //            //color [color] - Color of the item
            //            if (googleProduct != null && !String.IsNullOrEmpty(googleProduct.Color))
            //            {
            //                writer.WriteStartElement("g", "color", googleBaseNamespace);
            //                writer.WriteCData(googleProduct.Color);
            //                writer.WriteFullEndElement(); // g:color
            //            }

            //            //size [size] - Size of the item
            //            if (googleProduct != null && !String.IsNullOrEmpty(googleProduct.Size))
            //            {
            //                writer.WriteStartElement("g", "size", googleBaseNamespace);
            //                writer.WriteCData(googleProduct.Size);
            //                writer.WriteFullEndElement(); // g:size
            //            }

            //            #endregion

            //            #region Tax & Shipping

            //            //tax [tax]
            //            //The tax attribute is an item-level override for merchant-level tax settings as defined in your Google Merchant Center account. This attribute is only accepted in the US, if your feed targets a country outside of the US, please do not use this attribute.
            //            //IMPORTANT NOTE: Set tax in your Google Merchant Center account settings

            //            //IMPORTANT NOTE: Set shipping in your Google Merchant Center account settings

            //            //shipping weight [shipping_weight] - Weight of the item for shipping
            //            //We accept only the following units of weight: lb, oz, g, kg.
            //            if (_froogleSettings.PassShippingInfo)
            //            {
            //                var weightName = "kg";
            //                var shippingWeight = product.Weight;
            //                switch (_measureService.GetMeasureWeightById(_measureSettings.BaseWeightId).SystemKeyword)
            //                {
            //                    case "ounce":
            //                        weightName = "oz";
            //                        break;
            //                    case "lb":
            //                        weightName = "lb";
            //                        break;
            //                    case "grams":
            //                        weightName = "g";
            //                        break;
            //                    case "kg":
            //                        weightName = "kg";
            //                        break;
            //                    default:
            //                        //unknown weight 
            //                        weightName = "kg";
            //                        break;
            //                }
            //                writer.WriteElementString("g", "shipping_weight", googleBaseNamespace, string.Format(CultureInfo.InvariantCulture, "{0} {1}", shippingWeight.ToString(new CultureInfo("en-US", false).NumberFormat), weightName));
            //            }

            //            #endregion

            //            writer.WriteEndElement(); // item
            //        }
            //    }

            //    writer.WriteEndElement(); // channel
            //    writer.WriteEndElement(); // rss
            //    writer.WriteEndDocument();
            //}
            #endregion



        }

	}

	class ProductListwithOverviewModel
	{
		public IList<ProductOverviewModel280> ProductsOverview;
		public IPagedList<Product> Products;
		public IList<int> FilterableSpecificationAttributeOptionIds { get; set; }
		public IList<int> FilterableProductVariantAttributeIds { get; set; }
		public IList<int> FilterableManufacturerIds { get; set; }
		public IList<int> FilterableCategoryIds { get; set; }
		public decimal? MaxPrice { get; set; }
	}
}