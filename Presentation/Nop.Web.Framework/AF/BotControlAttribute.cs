using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web;
using Nop.Core.Infrastructure;
using Nop.Core;
using System.Web.Routing;
using Nop.Services.Logging;

namespace Nop.Web.Framework
{
    public class BotPostControlAttribute : ActionFilterAttribute
    {
        public string RedirectUrl { get; set; }
        public string RedirectAjaxUrl { get; set; }
        public string TrapFormElementName { get; set; }
        public int MinimumRequestPeriod { get; set; }

        private void SetResult(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAjaxRequest())
                filterContext.Result = new RedirectResult(RedirectAjaxUrl);
            else
                filterContext.Result = new RedirectResult(RedirectUrl);
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext == null || filterContext.HttpContext == null)
                return;

            HttpRequestBase request = filterContext.HttpContext.Request;
            if (request == null)
                return;

           
            int count = 1;
            //Dictionary<string, string> bots = request.RequestContext.HttpContext.Cache["bots"] as Dictionary<string, string>;
            //if (bots == null)
            //    bots = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(request.Form[TrapFormElementName]))
            { 
                var logger = EngineContext.Current.Resolve<ILogger>();
                SetResult(filterContext);
               // bots[request.UserHostAddress] = string.Format("{0}_{1}_{2}", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongTimeString(), count);
                logger.Information("Bot Detected", new Exception(string.Format("{0}_{1}_{2}", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongTimeString(), count)));
            }   

            //if (bots.ContainsKey(request.UserHostAddress))
            //{
            //    string[] infos = bots[request.UserHostAddress].Split('_');
            //    DateTime getRequestTime = DateTime.Parse(infos.FirstOrDefault());
            //    count = int.Parse(infos.LastOrDefault());
            //    count++;

            //    //bot infos: [get request time]_[last post request time]_[count]
            //    bots[request.UserHostAddress] = string.Format("{0}_{1}_{2}", getRequestTime.ToLongTimeString(), DateTime.Now.ToLongTimeString(), count);
            //    SetResult(filterContext);
            //}
            //else
            //{
            //    if (request.RequestContext.HttpContext.Cache[string.Format("get-{0}", request.UserHostAddress)] != null)
            //    {
            //        DateTime getRequestTime = (DateTime)request.RequestContext.HttpContext.Cache[string.Format("get-{0}", request.UserHostAddress)];
            //        TimeSpan getRequestPeriod = DateTime.Now - getRequestTime;

            //        if (getRequestPeriod.TotalSeconds <= MinimumRequestPeriod)
            //        {
            //            bots.Add(request.UserHostAddress, string.Format("{0}_{1}_{2}", getRequestTime.ToLongTimeString(), DateTime.Now.ToLongTimeString(), count));
            //            SetResult(filterContext);
            //        }
            //    }
            //    else
            //    {
            //        if (request.UrlReferrer == null)
            //        {
            //            bots.Add(request.UserHostAddress, string.Format("{0}_{1}_{2}", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongTimeString(), count));
            //            SetResult(filterContext);    
            //        }
            //    }
            //}
            //request.RequestContext.HttpContext.Cache["bots"] = bots;
        }
    }

    public class BotGetControlAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext == null || filterContext.HttpContext == null)
                return;

            HttpRequestBase request = filterContext.HttpContext.Request;
            if (request == null)
                return;

            if (request.RequestType.ToLowerInvariant() == "get")
                request.RequestContext.HttpContext.Cache[string.Format("get-{0}", request.UserHostAddress)] = DateTime.Now;
        }
    }
}
