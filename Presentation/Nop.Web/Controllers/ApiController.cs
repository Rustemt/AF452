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
using Nop.Web.Framework.AF;
using Nop.Services.ExportImport;
using Nop.Web.Framework.UI;
using System.IO;


namespace Nop.Web.Controllers
{ 
    [FilterIP(ConfigurationKeyAllowedSingleIPs = "AllowedAPISingleIPs")]
    public class ApiController : BaseNopController
    {
        #region Fields
     
        private readonly IWorkContext _workContext;
        private readonly IWebHelper _webHelper;
        private readonly IExportManager _exportManager;
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;

        #endregion

        #region Constructors

        public ApiController(
            IWorkContext workContext,
            IWebHelper webHelper, 
            IExportManager exportManager,
            ICustomerService customerService,
            IOrderService orderService,
            IProductService productService
            )

        {
            this._workContext = workContext;
            this._webHelper = webHelper;
            this._exportManager = exportManager;
            this._customerService = customerService;
            this._orderService = orderService;
            this._productService = productService;
        }

        #endregion

        #region utilities
        private void LogException(Exception exc)
        {
            var workContext = EngineContext.Current.Resolve<IWorkContext>();
            var logger = EngineContext.Current.Resolve<ILogger>();

            var customer = workContext.CurrentCustomer;
            logger.Error(exc.Message, exc, customer);
        }
        protected virtual void ErrorNotification(Exception exception, bool persistForTheNextRequest = true)
        {
            LogException(exception);
            AddNotification(NotifyType.Error, exception.Message, persistForTheNextRequest);
        }
        protected virtual void AddNotification(NotifyType type, string message, bool persistForTheNextRequest)
        {
            string dataKey = string.Format("nop.notifications.{0}", type);
            if (persistForTheNextRequest)
            {
                if (TempData[dataKey] == null)
                    TempData[dataKey] = new List<string>();
                ((List<string>)TempData[dataKey]).Add(message);
            }
            else
            {
                if (ViewData[dataKey] == null)
                    ViewData[dataKey] = new List<string>();
                ((List<string>)ViewData[dataKey]).Add(message);
            }
        }
        #endregion utilities

        public ActionResult ExportCustomersExcelNebim()
        {
            try
            {
                var registeredCustomers = _customerService.GetAllCustomers(null, null, new int[] { 1, 2, 3 }, null,
                    null, null, null, 0, 0, false, null, 0, int.MaxValue);

                var allOrders = _orderService.LoadAllOrders();
                var unregisteredOrders = allOrders.Where(x => (x.Customer.IsGuest() && !x.Customer.IsRegistered()));
                var unregisterdCustomers = unregisteredOrders.Select(x => x.Customer).ToList();

                var customers = registeredCustomers.Union(unregisterdCustomers).ToList();

                string fileName = string.Format("customers_{0}_{1}.xlsx", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), CommonHelper.GenerateRandomDigitCode(4));
                string filePath = string.Format("{0}content\\files\\ExportImport\\{1}", Request.PhysicalApplicationPath, fileName);

                _exportManager.ExportCustomersToXlsxForNebim(filePath, customers);

                var bytes = System.IO.File.ReadAllBytes(filePath);
                return File(bytes, "text/xls", fileName);
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        public ActionResult ExportOrdersExcelNebim()
        {
            try
            {
                var orders = _orderService.SearchOrders(null, null, null,
                    null, null, null, null, 0, int.MaxValue);
                string fileName = string.Format("orders_{0}_{1}.xlsx", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), CommonHelper.GenerateRandomDigitCode(4));
                string filePath = string.Format("{0}content\\files\\ExportImport\\{1}", Request.PhysicalApplicationPath, fileName);
                _exportManager.ExportOrdersToXlsx(filePath, orders);

                var bytes = System.IO.File.ReadAllBytes(filePath);
                return File(bytes, "text/xls", fileName);
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        public ActionResult ExportProductsExcelNebim()
        {
            try
            {
                var products = _productService.SearchProducts(0, 0, null, null, null, 0, string.Empty, false,
                    _workContext.WorkingLanguage.Id, new List<int>(),
                    ProductSortingEnum.Position, 0, int.MaxValue, true);

                string fileName = string.Format("products_{0}_{1}.xlsx", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), CommonHelper.GenerateRandomDigitCode(4));
                string filePath = string.Format("{0}content\\files\\ExportImport\\{1}", Request.PhysicalApplicationPath, fileName);

                _exportManager.ExportProductsToXlsxForNebim(filePath, products);

                var bytes = System.IO.File.ReadAllBytes(filePath);
                return File(bytes, "text/xls", fileName);
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }


    }
}
