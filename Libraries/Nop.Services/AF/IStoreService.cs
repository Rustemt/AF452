using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nop.Core.Domain.Common;

namespace Nop.Services.Common
{
    public interface IStoreService
    {
         IList<Store> GetStoresByCity(string city);
         IList<Store> GetStores();
         IList<string> GetCountries();
         IList<string> GetCities();
         IList<Store> GetStoresByCityAndType(string city, string type);
         Store GetStoreById(int storeId);
         void UpdateStore(Store store);
         //Store GetStoreById(int storeId);
         void InsertStore(Store store);
         void DeleteStore(Store store);
    }
}
