using System;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Customers;
using Nop.Core.Infrastructure;
using Nop.Services.Affiliates;
using Nop.Services.Customers;
using Nop.Services.Logging;
using Nop.Web.Framework.Security;
using Nop.Web.Framework.UI;
using System.Collections.Generic;
using Nop.Web.Framework;
using System.Web.Routing;

namespace Nop.Web.Controllers
{
    [LanguageSeoCode]
    [NopHttpsRequirement(Framework.Security.SslRequirement.No)]
    public class BaseNopController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            //StoreLastVisitedPage(filterContext);
            CheckAffiliate();
        }

        protected virtual ActionResult InvokeHttp404()
        {
            // Call target Controller and pass the routeData.
            IController errorController = EngineContext.Current.Resolve<Nop.Web.Controllers.CommonController>();

            var routeData = new RouteData();
            routeData.Values.Add("controller", "Common");
            routeData.Values.Add("action", "PageNotFound");

            errorController.Execute(new RequestContext(this.HttpContext, routeData));

            return new EmptyResult();
        }

        protected virtual void StoreLastVisitedPage(ActionExecutingContext filterContext)
        {
            if (!DataSettingsHelper.DatabaseIsInstalled())
                return;

            if (filterContext == null || filterContext.HttpContext == null || filterContext.HttpContext.Request == null)
                return;
            
            //only GET requests
            if (!String.Equals(filterContext.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
                return;

            {
                var webHelper = EngineContext.Current.Resolve<IWebHelper>();
                var pageUrl = webHelper.GetThisPageUrl(true);
                if (!String.IsNullOrEmpty(pageUrl))
                {
                    var workContext = EngineContext.Current.Resolve<IWorkContext>();
                    var customerService = EngineContext.Current.Resolve<ICustomerService>();

                    var previousPageUrl = workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.LastVisitedPage);
                    if (!pageUrl.Equals(previousPageUrl))
                    {
                        customerService.SaveCustomerAttribute(workContext.CurrentCustomer,
                            SystemCustomerAttributeNames.LastVisitedPage, pageUrl);
                    }
                }
            }
        }

        protected virtual void CheckAffiliate()
        {
            if (Request != null &&
                Request.QueryString != null && Request.QueryString["AffiliateId"] != null)
            {
                var affiliateId = Convert.ToInt32(Request.QueryString["AffiliateId"]);

                if (affiliateId > 0)
                {
                    var affiliateService = EngineContext.Current.Resolve<IAffiliateService>();
                    var affiliate = affiliateService.GetAffiliateById(affiliateId);
                    if (affiliate != null && !affiliate.Deleted && affiliate.Active)
                    {
                        var workContext = EngineContext.Current.Resolve<IWorkContext>();
                        if (workContext.CurrentCustomer != null &&
                            workContext.CurrentCustomer.AffiliateId != affiliate.Id)
                        {
                            workContext.CurrentCustomer.AffiliateId = affiliate.Id;
                            var customerService = EngineContext.Current.Resolve<ICustomerService>();
                            customerService.UpdateCustomer(workContext.CurrentCustomer);
                        }
                    }
                }
            }
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            if (filterContext == null)
                return;
          
            var isAjax=filterContext.HttpContext.Request.IsAjaxRequest();
            if (isAjax)
            {
                filterContext.ExceptionHandled = false;
                //filterContext.Result = new JsonResult()
                //{
                //    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                //    Data = new
                //    {
                //        header="header",
                //        messages="Messages"
                //    }
                //};
            }
            else
            {  
                if (filterContext.Exception != null)
                LogException(filterContext.Exception);
                filterContext.ExceptionHandled = true;
                filterContext.Result = View("Error");
            }
          
           
           
        }

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

    }
}
