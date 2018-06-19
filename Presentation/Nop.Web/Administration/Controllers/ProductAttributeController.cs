using System.Linq;
using System.Web.Mvc;
using Nop.Admin.Models.Catalog;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Web.Framework.Controllers;
using Telerik.Web.Mvc;
using System;

namespace Nop.Admin.Controllers
{
    [AdminAuthorize]
    public class ProductAttributeController : BaseNopController
    {
        #region Fields

        private readonly IProductAttributeService _productAttributeService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IPermissionService _permissionService;

        #endregion Fields

        #region Constructors

        public ProductAttributeController(IProductAttributeService productAttributeService,
            ILanguageService languageService, ILocalizedEntityService localizedEntityService,
            ILocalizationService localizationService, ICustomerActivityService customerActivityService,
            IPermissionService permissionService)
        {
            this._productAttributeService = productAttributeService;
            this._languageService = languageService;
            this._localizedEntityService = localizedEntityService;
            this._localizationService = localizationService;
            this._customerActivityService = customerActivityService;
            this._permissionService = permissionService;
        }

        #endregion
        
        #region Utilities

        [NonAction]
        public void UpdateLocales(ProductAttribute productAttribute, ProductAttributeModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(productAttribute,
                                                               x => x.Name,
                                                               localized.Name,
                                                               localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(productAttribute,
                                                           x => x.Description,
                                                           localized.Description,
                                                           localized.LanguageId);
            }
        }

        [NonAction]
        public void UpdateOptionLocales(ProductAttributeOption productAttributeOption, ProductAttributeOptionModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(productAttributeOption,
                                                               x => x.Name,
                                                               localized.Name,
                                                               localized.LanguageId);
            }
        }

        #endregion
        
        #region Methods

        //list
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var productAttributes = _productAttributeService.GetAllProductAttributes();
            var gridModel = new GridModel<ProductAttributeModel>
            {
                Data = productAttributes.Select(x => x.ToModel()),
                Total = productAttributes.Count()
            };
            return View(gridModel);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult List(GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var productAttributes = _productAttributeService.GetAllProductAttributes();
            var gridModel = new GridModel<ProductAttributeModel>
            {
                Data = productAttributes.Select(x => x.ToModel()),
                Total = productAttributes.Count()
            };
            return new JsonResult
            {
                Data = gridModel
            };
        }
        
        //create
        public ActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var model = new ProductAttributeModel();
            //locales
            AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [HttpPost, FormValueExists("save", "save-continue", "continueEditing")]
        public ActionResult Create(ProductAttributeModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var productAttribute = model.ToEntity();
                _productAttributeService.InsertProductAttribute(productAttribute);
                UpdateLocales(productAttribute, model);

                //activity log
                _customerActivityService.InsertActivity("AddNewProductAttribute", _localizationService.GetResource("ActivityLog.AddNewProductAttribute"), productAttribute.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Attributes.ProductAttributes.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = productAttribute.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        //edit
        public ActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var productAttribute = _productAttributeService.GetProductAttributeById(id);
            if (productAttribute == null)
                //No product attribute found with the specified id
                return RedirectToAction("List");

            var model = productAttribute.ToModel();
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = productAttribute.GetLocalized(x => x.Name, languageId, false, false);
                locale.Description = productAttribute.GetLocalized(x => x.Description, languageId, false, false);
            });

            return View(model);
        }

        [HttpPost, FormValueExists("save", "save-continue", "continueEditing")]
        public ActionResult Edit(ProductAttributeModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var productAttribute = _productAttributeService.GetProductAttributeById(model.Id);
            if (productAttribute == null)
                //No product attribute found with the specified id
                return RedirectToAction("List");
            
            if (ModelState.IsValid)
            {
                productAttribute = model.ToEntity(productAttribute);
                _productAttributeService.UpdateProductAttribute(productAttribute);

                UpdateLocales(productAttribute, model);

                //activity log
                _customerActivityService.InsertActivity("EditProductAttribute", _localizationService.GetResource("ActivityLog.EditProductAttribute"), productAttribute.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Attributes.ProductAttributes.Updated"));
                return continueEditing ? RedirectToAction("Edit", productAttribute.Id) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        //delete
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var productAttribute = _productAttributeService.GetProductAttributeById(id);
            if (productAttribute == null)
                //No product attribute found with the specified id
                return RedirectToAction("List");

            _productAttributeService.DeleteProductAttribute(productAttribute);

            //activity log
            _customerActivityService.InsertActivity("DeleteProductAttribute", _localizationService.GetResource("ActivityLog.DeleteProductAttribute"), productAttribute.Name);

            SuccessNotification(_localizationService.GetResource("Admin.Catalog.Attributes.ProductAttributes.Deleted"));
            return RedirectToAction("List");
        }

        #region product attribute options

        //list
        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult OptionList(int productAttributeId, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var options = _productAttributeService.GetProductAttributeOptionsByProductAttribute(productAttributeId);
            var gridModel = new GridModel<ProductAttributeOptionModel>
            {
                Data = options.Select(x =>
                {
                    var model = x.ToModel();
                    //locales
                    //AddLocales(_languageService, model.Locales, (locale, languageId) =>
                    //{
                    //    locale.Name = x.GetLocalized(y => y.Name, languageId, false, false);
                    //});
                    return model;
                }),
                Total = options.Count()
            };
            return new JsonResult
            {
                Data = gridModel
            };
        }

        //create
        public ActionResult OptionCreatePopup(int productAttributeId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var model = new ProductAttributeOptionModel();
            model.ProductAttributeId = productAttributeId;
            //locales
            AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [HttpPost]
        public ActionResult OptionCreatePopup(string btnId, string formId, ProductAttributeOptionModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var productAttribute = _productAttributeService.GetProductAttributeById(model.ProductAttributeId);
            if (productAttribute == null)
                throw new ArgumentException("No product attribute found with the specified id");

            if (ModelState.IsValid)
            {
                var sao = model.ToEntity();

                _productAttributeService.InsertProductAttributeOption(sao);
                UpdateOptionLocales(sao, model);

                ViewBag.RefreshPage = true;
                ViewBag.btnId = btnId;
                ViewBag.formId = formId;
                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        //edit
        public ActionResult OptionEditPopup(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var sao = _productAttributeService.GetProductAttributeOptionById(id);
            if (sao == null)
                throw new ArgumentException("No product attribute option found with the specified id", "id");
            var model = sao.ToModel();
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = sao.GetLocalized(x => x.Name, languageId, false, false);
            });

            return View(model);
        }

        [HttpPost]
        public ActionResult OptionEditPopup(string btnId, string formId, ProductAttributeOptionModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var sao = _productAttributeService.GetProductAttributeOptionById(model.Id);
            if (sao == null)
                throw new ArgumentException("No product attribute option found with the specified id");
            if (ModelState.IsValid)
            {
                sao = model.ToEntity(sao);
                _productAttributeService.UpdateProductAttributeOption(sao);

                UpdateOptionLocales(sao, model);

                ViewBag.RefreshPage = true;
                ViewBag.btnId = btnId;
                ViewBag.formId = formId;
                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        //delete
        [GridAction(EnableCustomBinding = true)]
        public ActionResult OptionDelete(int optionId, int productAttributeId, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var sao = _productAttributeService.GetProductAttributeOptionById(optionId);
            _productAttributeService.DeleteProductAttributeOption(sao);

            return OptionList(productAttributeId, command);
        }


        //ajax
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetOptionsByAttributeId(string attributeId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            // This action method gets called via an ajax request
            if (String.IsNullOrEmpty(attributeId))
                throw new ArgumentNullException("attributeId");

            var options = _productAttributeService.GetProductAttributeOptionsByProductAttribute(Convert.ToInt32(attributeId));
            var result = (from o in options
                          select new { id = o.Id, name = o.Name }).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion product attribute options
        
        #endregion
    }
}
