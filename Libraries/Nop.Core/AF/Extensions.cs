using System;
using System.Runtime.Caching;
using Nop.Core.Infrastructure;

namespace Nop.Core.Caching
{
    /// <summary>
    /// Extensions
    /// </summary>
    public static class AFExtensions
    {
       

        //public static T Get<T>(this ICacheManager cacheManager, string key, Func<T> acquire, bool recover = false)
        //{
        //    return Get(cacheManager, key, 60, acquire);
        //}
      
        //public static T Get<T>(this ICacheManager cacheManager, string key, int cacheTime, Func<T> acquire, bool recover = false)
        //{
        //    if (cacheManager.IsSet(key))
        //    {
        //        return cacheManager.Get<T>(key);
        //    }
        //    else
        //    {
        //        var result = acquire();
        //        if (result != null)
        //            if (recover && cacheManager is MemoryCacheManager)
        //            {
        //                CacheEntryRemovedCallback rc = null;
        //                rc = new CacheEntryRemovedCallback(
        //                    (CacheEntryRemovedArguments x) =>
        //                    {
        //                        var workContext = EngineContext.Current.Resolve<IWorkContext>();
        //                        workContext.WorkingLanguage = new Domain.Localization.Language() { Id = 2 };
        //                        workContext.WorkingCurrency = new Domain.Directory.Currency() { Id = 12 };

        //                        result = acquire();
        //                        cacheManager.Set(key, result, cacheTime, rc);
        //                    }
        //                    );

        //                cacheManager.Set(key, result, cacheTime, rc);
        //            }
        //            else
        //            {
        //                cacheManager.Set(key, result, cacheTime);
        //            }
        //        return result;
        //    }
        //}


    }


}
