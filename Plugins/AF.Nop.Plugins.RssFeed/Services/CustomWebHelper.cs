using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace AF.Nop.Plugins.RssFeed.Services
{
    public class CustomWebHelper : WebHelper
    {
        protected HttpContextBase _httpContext;

        public CustomWebHelper(HttpContextBase httpContext) : base(httpContext)
        {
            _httpContext = httpContext;
        }

        public void SetHttpContext(HttpContextBase c)
        {
            _httpContext = c;
        }

        public override string GetCurrentIpAddress()
        {
            try { return base.GetCurrentIpAddress(); }
            catch  { return string.Empty; }
        }

        public override string GetUrlReferrer()
        {
            try { return base.GetUrlReferrer(); }
            catch  { return string.Empty; }
        }

        public override string GetThisPageUrl(bool includeQueryString, bool useSsl)
        {
            try { return base.GetThisPageUrl(includeQueryString, useSsl); }
            catch { return string.Empty; }
        }

        public override bool IsCurrentConnectionSecured()
        {
            try { return base.IsCurrentConnectionSecured(); }
            catch { return false; }
        }

        public override string ServerVariables(string name)
        {
            try { return base.ServerVariables(name); }
            catch { return string.Empty; }
        }

        public override string GetStoreHost(bool useSsl)
        {
            try { return base.GetStoreHost(useSsl); }
            catch { return string.Empty; }
        }

        public override string GetStoreLocation(bool useSsl)
        {
            try { return base.GetStoreLocation(useSsl); }
            catch { return string.Empty; }
        }
    }
}
