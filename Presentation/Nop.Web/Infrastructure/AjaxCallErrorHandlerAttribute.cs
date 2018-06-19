using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Nop.Web.Infrastructure
{
    public class AjaxCallErrorHandlerAttribute : FilterAttribute, IExceptionFilter
    {
        //Redirect, Alert,
        public AjaxErrorReaction Reaction { get; set; }
        public string Message { get; set; }
        public string Url { get; set; }
        public void OnException(ExceptionContext filterContext)
        {
            filterContext.ExceptionHandled = true;
            filterContext.Result = new JsonResult
            {
                Data = new
                {
                    success = false,
                    error = filterContext.Exception.ToString(),
                    Reaction = Reaction,
                    Url = Url,
                    Message = Message
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
    }
    public enum AjaxErrorReaction
    {
        Nothing,
        Redirect,
        Alert
    }

}