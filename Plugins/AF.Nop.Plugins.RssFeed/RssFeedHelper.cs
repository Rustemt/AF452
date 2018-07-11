using AF.Nop.Plugins.RssFeed.Models;
using Nop.Core.Domain;
using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;
using Nop.Services.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace AF.Nop.Plugins.RssFeed
{
    public static class RssFeedHelper
    {
        private static HttpContext _httpContext;

        public const string TASK_USERNAME = "google-merchant";
        public const string TASK_PASSWORD = "2IGHLKJHH";

        static public HttpContext GetHttpContext()
        {
            if (HttpContext.Current != null)
                return HttpContext.Current;
            var _storeContext = EngineContext.Current.Resolve<StoreInformationSettings>();
            HttpRequest request = new HttpRequest("/", _storeContext.StoreUrl, "");
            HttpResponse response = new HttpResponse(new StringWriter());
            var _httpContext = new HttpContext(request, response);
            return _httpContext;
        }

        public const string GoogleNamespce = "http://base.google.com/ns/1.0";
        public const string RssFolder = "~/Content/Rss";
        public const string ScheduleTaskName = "Generator Rss Feed XML files";

        public static string HtmlToPlainText(string html)
        {
            const string tagWhiteSpace = @"(>|$)(\W|\n|\r)+<";//matches one or more (white space or line breaks) between '>' and '<'
            const string stripFormatting = @"<[^>]*(>|$)";//match any character between '<' and '>', even when end tag is missing
            const string lineBreak = @"<(br|BR)\s{0,1}\/{0,1}>";//matches: <br>,<br/>,<br />,<BR>,<BR/>,<BR />
            var lineBreakRegex = new Regex(lineBreak, RegexOptions.Multiline);
            var stripFormattingRegex = new Regex(stripFormatting, RegexOptions.Multiline);
            var tagWhiteSpaceRegex = new Regex(tagWhiteSpace, RegexOptions.Multiline);

            var text = html;
            //Remove tag whitespace/line breaks
            text = tagWhiteSpaceRegex.Replace(text, "><");
            //Replace <br /> with line breaks
            text = lineBreakRegex.Replace(text, Environment.NewLine);
            //Strip formatting
            text = stripFormattingRegex.Replace(text, string.Empty);

            //Decode html specific characters
            text = System.Net.WebUtility.HtmlDecode(text);

            return text;

        }
        
        public static string GetFileUrl(string fileName)
        {
            var _urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);

            var urlBuilder = new UriBuilder(HttpContext.Current.Request.Url.AbsoluteUri)
            {
                Path = Path.Combine(RssFolder.Trim('~'),fileName),
                Query = null,
            };
            return urlBuilder.Uri.AbsoluteUri;
        }

        public static string GetRssFolder
        {
            get
            {
                return RssFolder;// MapPath(RssFolder);
            }
        }

        public static string GetFilePath(string fileName)
        {
            return MapPath(Path.Combine(GetRssFolder, fileName));
        }

        public static string MapPath(string path)
        {
            if (HostingEnvironment.IsHosted)
            {
                //hosted
                return HostingEnvironment.MapPath(path);
            }
            else
            {
                //not hosted. For example, run in unit tests
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                path = path.Replace("~/", "").TrimStart('/').Replace('/', '\\');
                return Path.Combine(baseDirectory, path);
            }
        }
    }
}
