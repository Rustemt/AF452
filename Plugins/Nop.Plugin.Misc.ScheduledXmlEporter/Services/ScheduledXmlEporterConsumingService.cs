using System;
using System.Linq;
using Nop.Core.Domain.Tasks;
using Nop.Core.Plugins;
//using Nop.Plugin.Misc.ScheduledXmlEporter.Data;
using Nop.Services.Localization;
using Nop.Services.Tasks;
using Nop.Services.Catalog;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using System.Collections.Generic;
using Nop.Services.ExportImport;
using Nop.Web.Framework.Mvc;
using System.Xml;
using System.Web.Hosting;
using Nop.Core.Infrastructure;
using Nop.Services.Configuration;

namespace Nop.Plugin.Misc.ScheduledXmlEporter.Services
{
    public class ScheduledXmlEporterConsumingService : IScheduledXmlEporterConsumingService
    {
        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;
        private readonly ICategoryService _categoryService;
        private readonly IExportManager _exportManager;
        private readonly ScheduledXmlEporterSettings _scheduledXmlEporterSettings;
        private readonly ISettingService _settingService;

        public ScheduledXmlEporterConsumingService(IProductService productService, IWorkContext workContext, 
            ICategoryService categoryService, IExportManager exportManager, ScheduledXmlEporterSettings scheduledXmlEporterSettings, ISettingService settingService)
        {
            _productService = productService;
            _workContext = workContext;
            _categoryService = categoryService;
            _exportManager = exportManager;
            _scheduledXmlEporterSettings = scheduledXmlEporterSettings;
            _settingService = settingService;
        }

        /// <summary>
        /// Installs the sync task.
        /// </summary>
        public virtual void XmlExportForN11(int CategoryId)
        {            
            int catId = 0;

            catId = CategoryId;

            try
            {
                var products = _productService.SearchProducts(catId, 0, null, null, null, 0, string.Empty, false,
                    _workContext.WorkingLanguage.Id, new List<int>(),
                    ProductSortingEnum.Position, 0, int.MaxValue, false);

                if (products.Count == 0)
                {
                    var subCategory = _categoryService.GetAllCategoriesByParentCategoryId(catId);
                    foreach (var item in subCategory)
                    {
                        var subProduct = _productService.SearchProducts(item.Id, 0, null, null, null, 0, string.Empty, false, _workContext.WorkingLanguage.Id, new List<int>(), ProductSortingEnum.Position, 0, int.MaxValue, false);

                        if (subProduct.Count > 0)
                        {
                            foreach (var p in subProduct)
                            {
                                products.Add(p);
                            }
                        }
                    }
                }

                //var fileName = string.Format("products_{0}.xml", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));
                //var xml = _exportManager.ExportProductsToXmlForN11(products);
                //return new XmlDownloadResult(xml, fileName);

                string filePath = HostingEnvironment.MapPath("~/Content/files/ExportImport/");
                var fileName = "xmlProductsForN11.xml";
                var xml = _exportManager.ExportProductsToXmlForN11(products);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xml);
                xmlDoc.Save(filePath + fileName);
            }                                
            catch (Exception exc)
            {
                throw exc;
            }
        }


        public virtual void XmlExportForGG(int CategoryId)
        {
            int catId = 0;

            catId = CategoryId;

            try
            {
                var products = _productService.SearchProducts(catId, 0, null, null, null, 0, string.Empty, false,
                    _workContext.WorkingLanguage.Id, new List<int>(),
                    ProductSortingEnum.Position, 0, int.MaxValue, false);

                if (products.Count == 0)
                {
                    var subCategory = _categoryService.GetAllCategoriesByParentCategoryId(catId);
                    foreach (var item in subCategory)
                    {
                        var subProduct = _productService.SearchProducts(item.Id, 0, null, null, null, 0, string.Empty, false, _workContext.WorkingLanguage.Id, new List<int>(), ProductSortingEnum.Position, 0, int.MaxValue, false);

                        if (subProduct.Count > 0)
                        {
                            foreach (var p in subProduct)
                            {
                                products.Add(p);
                            }
                        }

                    }
                }

                //var fileName = string.Format("products_{0}.xml", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));
                //var xml = _exportManager.ExportProductsToXmlForGG(products);
                //return new XmlDownloadResult(xml, fileName);

                string filePath = HostingEnvironment.MapPath("~/Content/files/ExportImport/");
                var fileName = "xmlProductsForGG.xml";
                var xml = _exportManager.ExportProductsToXmlForN11(products);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xml);
                xmlDoc.Save(filePath + fileName);
            }
            catch (Exception exc)
            {
                throw exc;
            }
        }

        

    }
}