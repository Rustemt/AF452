using System;
using System.Linq;
using System.Web.Mvc;
using Nop.Services.Localization;
using Nop.Web.Framework.Controllers;
using AF.Nop.Plugins.Criteo.Models;
using Nop.Services.Common;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Core.Domain.Customers;
using System.Collections.Generic;
using Nop.Services.Configuration;
using Nop.Web.Models.Catalog;
using Nop.Services.Logging;
using Nop.Core.Domain.Orders;
using Nop.Web.Models.Checkout;
using Nop.Services.Orders;

namespace AF.Nop.Plugins.Criteo.Controllers
{
    public class CriteoController : Controller
    {
        public enum NotifyType
        {
            Success,
            Error
        }

        #region Fields & Contructor

        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderService _orderService;
        private readonly ILogger _logger;

        private readonly CriteoSettings _criteoSettings;

        public CriteoController(
            CriteoSettings criteoSettings,
            IWorkContext workContext,
            IStoreService storeService,
            ISettingService settingService,
            ILocalizationService localizationService,
            IOrderService orderService,
            ILogger logger)
        {
            _criteoSettings = criteoSettings;
            _workContext = workContext;
            _storeService = storeService;
            _settingService = settingService;
            _localizationService = localizationService;
            _orderService = orderService;
            _logger = logger;
        }

        #endregion

        #region Configuration

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            ConfigurationModel model = new ConfigurationModel();

            model.AccountId = _criteoSettings.AccountId;
            model.HashEmail = _criteoSettings.HashEmail;

            return View("~/Plugins/AF.Criteo/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            //save settings
            _criteoSettings.AccountId = model.AccountId;
            _criteoSettings.HashEmail = model.HashEmail;

            _settingService.SaveSetting(_criteoSettings);

            SuccessNotification("AF.Criteo.DataSaved");

            return View("~/Plugins/AF.Criteo/Views/Configure.cshtml", model);
        }

        #endregion

        [ChildActionOnly]
        public ActionResult ProcessWidget(int widgetId)
        {
            var s = RouteData.Values;
            
            var context = ControllerContext.ParentActionViewContext;
            if(context.IsChildAction)
            {
                context = context.RouteData.DataTokens["ParentActionViewContext"] as ViewContext;
            }
            var action = context.RouteData.GetRequiredString("action");
            var controller = context.RouteData.GetRequiredString("controller");
            var route = string.Format(@"{0}/{1}", controller, action).ToLowerInvariant();

            string script = "";

            var criteo = new CriteoJsCode()
            {
                AccountId = _criteoSettings.AccountId,
                CustomerEmail = _workContext.CurrentCustomer.Email,
                HashEmail = _criteoSettings.HashEmail,
                ClientDevice = Request.Browser.IsMobileDevice ? "m" : "d"
            };

            try
            {
                // catalog/categorymain
                if (route.Equals("home/index0310"))
                {
                    script = criteo.GenerateHomePageScript();
                }
                else if (route.Equals("shoppingcart/cart"))
                {
                    var cart = _workContext.CurrentCustomer.ShoppingCartItems
                            .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                        ;
                    script = criteo.GenerateCartScript(cart);
                }
                else if (route.Equals("catalog/categoryproducts280"))
                {
                    //var category = Convert.ToInt32(context.RouteData.Values["categoryId"]);
                    var items = (context.ViewData.Model as CategoryProductsModel280).Products.Select(x => x.Id);
                    script = criteo.GenerateProductListScript(items);
                }
                else if (route.Equals("catalog/product"))
                {
                    var product = Convert.ToInt32(context.RouteData.Values["productId"]);
                    //var product = Convert.ToInt32(context.RouteData.Values["variantId"]);
                    script = criteo.GenerateProductScript(product);
                }
                else if (route.Equals("checkout/completed"))
                {
                    var orderId = (context.ViewData.Model as CheckoutCompletedModel).OrderId;
                    var order = _orderService.GetOrderById(orderId);
                    if (order != null)
                        script = criteo.GenerateOrderScript(order);
                }
                else if (route.Equals("catalog/manufacturerse280"))
                {
                    var items = (context.ViewData.Model as ManufacturerProductsModel280).Products.Select(x => x.Id);
                    script = criteo.GenerateProductListScript(items);
                }
            }
            catch (Exception e)
            {
                _logger.Error("Criteo: Error in processing route '"+route+"'", e);
            }

            //_logger.Information("Criteo Route:" + route, new Exception(script));

            return Content(script);
        }

        #region Notification Method

        protected virtual void SuccessNotification(string message, bool persistForTheNextRequest = true)
        {
            AddNotification(NotifyType.Success, message, persistForTheNextRequest);
        }

        protected virtual void ErrorNotification(Exception exception, bool persistForTheNextRequest = true)
        {
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
        #endregion
    }
}
