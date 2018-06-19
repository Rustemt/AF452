using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;

namespace Nop.Core
{
    /// <summary>
    /// Represents a common helper
    /// </summary>
    public partial class WebHelper : IWebHelper 
    {
        //public string GetAbsolutePath(string relativePath)
        //{
        //    string absolutePath = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority + HttpContext.Current.Request.ApplicationPath + relativePath;
        //    return absolutePath;
        //}

        public string GetAbsolutePath(string relativePath)
        {
            string absolutePath = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority + relativePath;
            return absolutePath;
        }

        public static string GetFullPath(string relativePath)
        {
             return HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority + HttpContext.Current.Request.ApplicationPath + relativePath;
            
        }
    }
}
