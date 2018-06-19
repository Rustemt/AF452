using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Nop.Core.Infrastructure;
using Nop.Web.Framework.UI;
using System.Net;
using System;
using System.Diagnostics;
using System.Web;
using System.Web.Compilation;

namespace Nop.Web.Framework
{
    public static class HtmlExtensionsAF
    {
        public static MvcHtmlString AFDropDown(this HtmlHelper html, string id, IList<SelectListItem> list, string attributes = "")
        {
            if (list == null) return new MvcHtmlString("");
            if (list.Count() == 0) return new MvcHtmlString("");
            var selectedItem = list.FirstOrDefault(x => x.Selected);
            if (selectedItem == null) selectedItem = list.First();
            StringBuilder sb = new StringBuilder();

            sb.Append(string.Format("<div class=\"selectBox\" id=\"{0}\" {1}>", id, attributes));
            sb.Append(string.Format("<span>{0}</span>", selectedItem.Text));
            sb.Append("<ul>");
            foreach (var item in list)
            {
                sb.Append(string.Format("<li {0} data-value=\"{1}\">{2}</li>", selectedItem.Value == item.Value ? "class=\"on\"" : "", item.Value, item.Text));
            }
            sb.Append("</ul>");
            sb.Append(string.Format("<input type=\"hidden\" id=\"{0}\" name=\"{0}\" value=\"{1}\" />", id, selectedItem.Value));
            sb.Append("</div>");

            return MvcHtmlString.Create(sb.ToString());

        }
        public static MvcHtmlString AFDropDown(this HtmlHelper html, string id, IList<SelectListItem> list, SelectListItem selectedListItem, string attributes = "")
        {
            if (list == null) return new MvcHtmlString("");
            if (list.Count() == 0) return new MvcHtmlString("");
            var selectedItem = selectedListItem;
            if (selectedItem == null)
                selectedItem = list.FirstOrDefault(x => x.Selected);
            if (selectedItem == null)
                selectedItem = list.First();
            StringBuilder sb = new StringBuilder();

            sb.Append(string.Format("<div class=\"selectBox\" id=\"{0}\" {1}>", id, attributes));
            sb.Append(string.Format("<span>{0}</span>", selectedItem.Text));
            sb.Append("<ul>");
            foreach (var item in list)
            {
                sb.Append(string.Format("<li {0} data-value=\"{1}\">{2}</li>", selectedItem.Value == item.Value ? "class=\"on\"" : "", item.Value, item.Text));
            }
            sb.Append("</ul>");
            sb.Append(string.Format("<input type=\"hidden\" name=\"{0}\" value=\"{1}\" />", id, selectedItem.Value));
            sb.Append("</div>");

            return MvcHtmlString.Create(sb.ToString());

        }
        public static MvcHtmlString AFRadioButton(this HtmlHelper html, string value, string content, string group = "", bool selected = false)
        {
            string str = string.Format("<a class=\"checkbox{3}\" href=\"javascript:;\" group=\"{1}\" mode=\"single\" rdoValue=\"{2}\">{0}</a>", content, group, value, selected ? " checked" : "");
            return MvcHtmlString.Create(str);
        }
        public static MvcHtmlString AFRadioButton(this HtmlHelper html, string id, string value, string content, string group = "", bool selected = false)
        {
            string str = string.Format("<a class=\"checkbox{3}\" href=\"javascript:;\" group=\"{1}\" mode=\"single\" rdoValue=\"{2}\">{0}</a>", content, group, value, selected ? " checked" : "");
            str += string.Format("<input type=\"hidden\" name=\"{0}\" />", id);
            return MvcHtmlString.Create(str);
        }
        public static MvcHtmlString AFCheckbox(this HtmlHelper html, string id, string content, bool selected = false)
        {
            string str = string.Format("<a class=\"checkbox{2}\" href=\"javascript:;\"  group=\"{1}\" >{0}</a>", content, id, selected ? " checked" : "");
            str += string.Format("<input type=\"hidden\" name=\"{0}\" value=\"{1}\" />", id, selected ? true : false);
            return MvcHtmlString.Create(str);
        }
        
        public static MvcHtmlString NopTitle(this HtmlHelper html, params string[] parts)
        {
            var pageTitleBuilder = EngineContext.Current.Resolve<IPageTitleBuilder>();
            html.AppendTitleParts(parts);
            return MvcHtmlString.Create(pageTitleBuilder.GenerateTitle(true));
        }

        public static string ContentV(this UrlHelper helper, string path)
        {
            if (HttpContext.Current.Cache["VersionTicks"] == null)
            {
                HttpContext.Current.Cache["VersionTicks"] = System.IO.File.GetCreationTime(BuildManager.GetGlobalAsaxType().BaseType.Assembly.Location).Ticks;
            }
            string ticks = HttpContext.Current.Cache["VersionTicks"].ToString();
            return helper.Content(string.Format("{0}?v={1}", path, ticks));
        }
    }


    public class AFWebClient : WebClient
    {
        public int TimeOut { get; set; }
        public AFWebClient()
        {

        }

        protected override WebRequest GetWebRequest(Uri uri)
        {
            WebRequest w = base.GetWebRequest(uri);
            if (TimeOut != 0)
                w.Timeout = TimeOut;
            return w;
        }
    }
}
