using System;
using System.Linq.Expressions;
using System.Reflection;
using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Core.Infrastructure;
using Nop.Core.Plugins;
using System.Globalization;

namespace Nop.Services.Localization
{ 
    
    public enum TextCase
        {
            None,
            Upper,
            Lower,
            Capitalize
        }

    public static class AFLocalizationExtentions
    {
         public static string GetLocalized<T>(this T entity,
           Expression<Func<T, string>> keySelector, TextCase decorationCase)
           where T : BaseEntity, ILocalizedEntity
        {
            var workContext = EngineContext.Current.Resolve<IWorkContext>();
            var localizedItem =entity.GetLocalized(keySelector, workContext.WorkingLanguage.Id);
            if (localizedItem == null) return null;
            switch (decorationCase)
            {
                case TextCase.None:
                    return localizedItem;
                case TextCase.Upper:
                   return new CultureInfo(workContext.WorkingLanguage.LanguageCulture, false).TextInfo.ToUpper(localizedItem);
                case TextCase.Lower:
                   return new CultureInfo(workContext.WorkingLanguage.LanguageCulture, false).TextInfo.ToLower(localizedItem);
                case TextCase.Capitalize:
                   return new CultureInfo(workContext.WorkingLanguage.LanguageCulture, false).TextInfo.ToTitleCase(localizedItem);
                default:
                    return localizedItem;
            }
        }
       
    }
}
