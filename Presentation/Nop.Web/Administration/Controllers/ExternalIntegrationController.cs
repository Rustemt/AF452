using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Nop.Admin.Models.Catalog;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.ExportImport;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Tax;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Telerik.Web.Mvc;
using Nop.Services.Directory;
using Nop.Core.Domain.Directory;
using Nop.Core.Infrastructure;
using Nop.Services.Orders;

namespace Nop.Admin.Controllers
{
    [AdminAuthorize]
    public class ExternalIntegrationController : BaseNopController
    {
		#region Fields

        private readonly IProductService _productService;
        private readonly IProductTemplateService _productTemplateService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IWorkContext _workContext;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IPictureService _pictureService;
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly IProductTagService _productTagService;
        private readonly ICopyProductService _copyProductService;
        private readonly IPdfService _pdfService;
        private readonly IExportManager _exportManager;
        private readonly IImportManager _importManager;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IPermissionService _permissionService;
        private readonly AdminAreaSettings _adminAreaSettings;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly INebimIntegrationService _nebimIntegrationService;
        private readonly INebimIntegrationImportService _nebimIntegrationImportService;
        private readonly IOrderService _orderService;


        #endregion

		#region Constructors

        public ExternalIntegrationController(IProductService productService, 
            IProductTemplateService productTemplateService,
            ICategoryService categoryService, IManufacturerService manufacturerService,
            IWorkContext workContext, ILanguageService languageService, 
            ILocalizationService localizationService, ILocalizedEntityService localizedEntityService,
            ISpecificationAttributeService specificationAttributeService, IPictureService pictureService,
            ITaxCategoryService taxCategoryService, IProductTagService productTagService,
            ICopyProductService copyProductService, IPdfService pdfService,
            IExportManager exportManager, IImportManager importManager,
            ICustomerActivityService customerActivityService,
            IPermissionService permissionService, AdminAreaSettings adminAreaSettings, ICurrencyService currencyService,
            CurrencySettings currencySettings, IMessageTemplateService messageTemplateService,
            INebimIntegrationService nebimIntegrationService,INebimIntegrationImportService nebimIntegrationImportService, IOrderService orderService)
        {
            this._productService = productService;
            this._productTemplateService = productTemplateService;
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._workContext = workContext;
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._localizedEntityService = localizedEntityService;
            this._specificationAttributeService = specificationAttributeService;
            this._pictureService = pictureService;
            this._taxCategoryService = taxCategoryService;
            this._productTagService = productTagService;
            this._copyProductService = copyProductService;
            this._pdfService = pdfService;
            this._exportManager = exportManager;
            this._importManager = importManager;
            this._customerActivityService = customerActivityService;
            this._permissionService = permissionService;
            this._adminAreaSettings = adminAreaSettings;
            this._currencyService = currencyService;
            this._currencySettings = currencySettings;
            this._messageTemplateService = messageTemplateService;
            this._nebimIntegrationService = nebimIntegrationService;
            this._nebimIntegrationImportService = nebimIntegrationImportService;
            this._orderService = orderService;
       
        }

        #endregion 

        #region Methods

        #region Export / Import

 
         public ActionResult SendProductsToNebim(string selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMaintenance))
                return AccessDeniedView();

            var products = new List<Product>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToArray();
                // products.AddRange(_productService.GetProductsByIds(ids));
                foreach (var id in ids)
                {
                    products.Add(_productService.GetProductById(id));
                }
            }


            _nebimIntegrationService.AddUpdateProductsToNebim(products);

            SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.NebimUpdated"));
            return RedirectToAction("List", "Product");
         
        }

         public ActionResult SendProductToNebim(int productId)
         {
             if (!_permissionService.Authorize(StandardPermissionProvider.ManageMaintenance))
                 return AccessDeniedView();
             var product = _productService.GetProductById(productId);
             _nebimIntegrationService.AddUpdateProductToNebim(product);
             SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.NebimUpdated"));
             return RedirectToAction("Edit", "Product", new { id = product.Id });

         }
         public ActionResult SendProductVariantToNebim(int productVariantId)
         {
             if (!_permissionService.Authorize(StandardPermissionProvider.ManageMaintenance))
                 return AccessDeniedView();
             var productVariant = _productService.GetProductVariantById(productVariantId);
             _nebimIntegrationService.AddUpdateProductVariantToNebim(productVariant);
             SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.NebimUpdated"));
             return RedirectToAction("Edit", "Product", new { id = productVariant.ProductId });

         }

         public ActionResult SendOrderToNebim(int orderId)
         {
             if (!_permissionService.Authorize(StandardPermissionProvider.ManageMaintenance))
                 return AccessDeniedView();
             try
             {
             var order = _orderService.GetOrderById(orderId);
             if (order == null)
             {
                 ErrorNotification("order does not exists");
                 return RedirectToAction("Edit", "Order", new { id = order.Id });
             }
             _nebimIntegrationService.SyncOrderToNebim(order);
               }
            catch (Exception ex)
            {
                ErrorNotification(ex, true);
                return RedirectToAction("Edit", "Order", new { id = orderId });
                //_logger.Error("SyncOrderToNebim=>order id:" + orderId + " message:" + ex.Message, ex);
                //if (ex.InnerException != null)
                //{
                //    _logger.Error("SyncOrderToNebim=>order id:" + orderId + "inner ex message:" + ex.InnerException.Message, ex.InnerException);
                //}
            }

             SuccessNotification(_localizationService.GetResource("Admin.Catalog.Order.NebimUpdated"));
             return RedirectToAction("Edit", "Order", new { id = orderId });

         }

         public ActionResult ShipOrderToNebim(int orderId)
         {
             if (!_permissionService.Authorize(StandardPermissionProvider.ManageMaintenance))
                 return AccessDeniedView();
             try
             {
                 var order = _orderService.GetOrderById(orderId);
                 if (order == null)
                 {
                     ErrorNotification("order does not exists");
                     return RedirectToAction("Edit", "Order", new { id = order.Id });
                 }
                 _nebimIntegrationService.CreateShipment(order);
             }
             catch (Exception ex)
             {
                 ErrorNotification(ex, true);
                 return RedirectToAction("Edit", "Order", new { id = orderId });
                 //_logger.Error("SyncOrderToNebim=>order id:" + orderId + " message:" + ex.Message, ex);
                 //if (ex.InnerException != null)
                 //{
                 //    _logger.Error("SyncOrderToNebim=>order id:" + orderId + "inner ex message:" + ex.InnerException.Message, ex.InnerException);
                 //}
             }

             SuccessNotification(_localizationService.GetResource("Admin.Catalog.Order.NebimUpdated"));
             return RedirectToAction("Edit", "Order", new { id = orderId });

         }

         public ActionResult ImportProductsFromNebim()
         {
             if (!_permissionService.Authorize(StandardPermissionProvider.ManageMaintenance))
                 return AccessDeniedView();

             _nebimIntegrationImportService.ImportAllProducts();
             SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.NebimUpdated"));
             return RedirectToAction("List", "Product");

         }
         
        #endregion

        #endregion
    }
}



