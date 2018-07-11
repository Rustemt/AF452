using Nop.Core.Infrastructure;
using Nop.Services.Localization;
using Nop.Web.Framework.Localization;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.WebPages;
using Telerik.Web.Mvc.UI;

namespace AF.Nop.Plugins.RssFeed.Extensions
{
    public static class Extensions
    {
        public static HelperResult CustomLocalizedEditor<T, TLocalizedModelLocal>(this HtmlHelper<T> helper, string name,  Func<int, HelperResult> localizedTemplate)
            where T : ILocalizedModel<TLocalizedModelLocal> where TLocalizedModelLocal : ILocalizedModelLocal
        {
            return new HelperResult(writer =>
            {
                if (helper.ViewData.Model.Locales.Count > 1)
                {
                    var tabStrip = helper.Telerik().TabStrip().Name(name).Items(x =>
                    {
                        for (int i = 0; i < helper.ViewData.Model.Locales.Count; i++)
                        {
                            var locale = helper.ViewData.Model.Locales[i];
                            var language = EngineContext.Current.Resolve<ILanguageService>().GetLanguageById(locale.LanguageId);
                            x.Add().Text(language.Name)
                                .Content(localizedTemplate(i).ToHtmlString())
                                .Selected(i==0)
                                .ImageUrl("~/Content/images/flags/" + language.FlagImageFileName);
                        }
                    }).ToHtmlString();
                    writer.Write(tabStrip);
                }
            });
        }
    }
}
