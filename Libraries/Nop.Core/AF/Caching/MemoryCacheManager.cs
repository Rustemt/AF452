using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Caching;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;

namespace Nop.Core.Caching
{
    /// <summary>
    /// Represents a MemoryCacheCache
    /// </summary>
    public partial class MemoryCacheManager : ICacheManager
    {

        /// <summary>
        /// Adds the specified key and object to the cache.
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="data">Data</param>
        /// <param name="cacheTime">Cache time</param>
        //public void Set(string key, object data, int cacheTime)
        //{
        //    if (data == null)
        //        return;

        //    var policy = new CacheItemPolicy();
        //    policy.SlidingExpiration = TimeSpan.FromDays(cacheTime);
        //    Cache.Add(new CacheItem(key, data), policy);
        //}

		public List<string> GetCacheList()
		{
			List<string> keys = new List<string>();
			foreach (var item in Cache)
			{
				//long size = 0;
				//using (Stream s = new MemoryStream())
				//{
				//	BinaryFormatter formatter = new BinaryFormatter();
				//	formatter.Serialize(s, item);
				//	size = s.Length;
				//}
				keys.Add(item.Key);
			}

			return keys;
		}     
    }
}