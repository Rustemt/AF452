#region Assembly System.Web.Mvc.dll, v4.0.30319
// c:\Program Files (x86)\Microsoft ASP.NET\ASP.NET MVC 3\Assemblies\System.Web.Mvc.dll
#endregion

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Web.Mvc;
using System.Text;

namespace System.Web.Mvc.Html
{
    // Summary:
    //     Provides support for validating the input from an HTML form.
    public static partial class ValidationExtensions
    {
        
        // Returns:
        //     If the property or object is valid, an empty string; otherwise, a span element
        //     that contains an error message.
        public static MvcHtmlString AFValidationMessageFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)
        {
           StringBuilder strBuild = new StringBuilder();

           strBuild.Append("<div class=\"numberformError parentFormformID formError\">");
           strBuild.Append("<div class=\"formErrorContent\">");
           strBuild.Append("aaaaa aaaaaaaaaaaaaaaaaa aaaaaaaaaaaaaaaaaaaaaaa aaaaaaaaaaaaa fvfvf fv fv fvf vfvfvfvfvfv");
           strBuild.Append(" <br /></div><div class=\"formErrorArrow\"><div class=\"line10\"><!-- --></div><div class=\"line9\"><!-- --></div><div class=\"line8\"><!-- --></div><div class=\"line7\"><!-- --></div><div class=\"line6\"><!-- --> </div><div class=\"line5\"> <!-- --></div><div class=\"line4\"><!-- --></div><div class=\"line3\"><!-- --></div><div class=\"line2\"><!-- --></div><div class=\"line1\"><!-- --></div></div></div>");
           string str = strBuild.ToString();
           return new MvcHtmlString(strBuild.ToString());
        }
    }
}

