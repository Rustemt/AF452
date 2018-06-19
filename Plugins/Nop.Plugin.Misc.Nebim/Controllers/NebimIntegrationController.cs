using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Services.ExportImport;

namespace Nop.Plugin.Misc.NebimIntegration
{
    public class NebimIntegrationController : Controller
    {
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly NebimIntegrationSettings _NebimIntegrationSettings;

        public NebimIntegrationController(ISettingService settingService,
            ILocalizationService localizationService, NebimIntegrationSettings NebimIntegrationSettings)
        {
            this._settingService = settingService;
            this._localizationService = localizationService;
            this._NebimIntegrationSettings = NebimIntegrationSettings;
        }

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            var model = new ConfigurationModel();
            model.Database = _NebimIntegrationSettings.Database;
            model.Password= _NebimIntegrationSettings.Password;
            model.ServerNameIP = _NebimIntegrationSettings.ServerNameIP;
            model.UserGroup = _NebimIntegrationSettings.UserGroup;
            model.UserName = _NebimIntegrationSettings.UserName;
            model.ProductsSyncEnabled = _NebimIntegrationSettings.ProductsSyncEnabled;
            model.ProductsSyncStartTimeMinutes = _NebimIntegrationSettings.ProductsSyncStartTimeMinutes;
            model.API_CommunicationTypeEmail = _NebimIntegrationSettings.API_CommunicationTypeEmail;
            model.API_CommunicationTypePhone = _NebimIntegrationSettings.API_CommunicationTypePhone;
            model.API_OfficeCode = _NebimIntegrationSettings.API_OfficeCode;
            model.API_WarehouseCode = _NebimIntegrationSettings.API_WarehouseCode;
            model.ColorAttributeId = _NebimIntegrationSettings.ColorAttributeId;
            model.Dim1AttributeId = _NebimIntegrationSettings.Dim1AttributeId;
            model.Dim2AttributeId= _NebimIntegrationSettings.Dim2AttributeId;

            model.SpecificationAttribute_API_AttributeTypeCodes = _NebimIntegrationSettings.SpecificationAttribute_API_AttributeTypeCodes;//specifications matching to be stored at nebim : 52-2,57-3
            model.API_AttributeTypeCodeForManufacturer = _NebimIntegrationSettings.API_AttributeTypeCodeForManufacturer;
            model.Category_API_HierarchyLevelCodes = _NebimIntegrationSettings.Category_API_HierarchyIds;//category matching to be stored at nebim : 20-1,22-2,34-5


            return View("Nop.Plugin.Misc.Nebim.Views.NebimIntegration.Configure", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            //save settings
            _NebimIntegrationSettings.Database = model.Database;
            _NebimIntegrationSettings.Password = model.Password;
            _NebimIntegrationSettings.ServerNameIP = model.ServerNameIP;
            _NebimIntegrationSettings.UserGroup = model.UserGroup;
            _NebimIntegrationSettings.UserName = model.UserName;
            _NebimIntegrationSettings.ProductsSyncEnabled = model.ProductsSyncEnabled;
            _NebimIntegrationSettings.ProductsSyncStartTimeMinutes = model.ProductsSyncStartTimeMinutes;
            _NebimIntegrationSettings.API_CommunicationTypeEmail = model.API_CommunicationTypeEmail;
            _NebimIntegrationSettings.API_CommunicationTypePhone = model.API_CommunicationTypePhone;
            _NebimIntegrationSettings.API_OfficeCode = model.API_OfficeCode;
            _NebimIntegrationSettings.API_WarehouseCode = model.API_WarehouseCode;
            _NebimIntegrationSettings.ColorAttributeId = model.ColorAttributeId;
            _NebimIntegrationSettings.Dim1AttributeId = model.Dim1AttributeId;
            _NebimIntegrationSettings.Dim2AttributeId = model.Dim2AttributeId;
            _NebimIntegrationSettings.SpecificationAttribute_API_AttributeTypeCodes=model.SpecificationAttribute_API_AttributeTypeCodes;//specifications matching to be stored at nebim : 52-2,57-3
            _NebimIntegrationSettings.API_AttributeTypeCodeForManufacturer = model.API_AttributeTypeCodeForManufacturer ;
            _NebimIntegrationSettings.Category_API_HierarchyIds = _NebimIntegrationSettings.Category_API_HierarchyIds;//category matching to be stored at nebim : 20-1,22-2,34-5

            _settingService.SaveSetting(_NebimIntegrationSettings);
            return View("Nop.Plugin.Misc.Nebim.Views.NebimIntegration.Configure", model);
        }


    }
}