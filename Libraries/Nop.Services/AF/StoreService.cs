using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nop.Core.Domain.Common;
using Nop.Core.Data;
using Nop.Core.Events;
using Nop.Core;

namespace Nop.Services.Common
{
    public class StoreService : IStoreService
    {
        private readonly IRepository<Store> _storeRepository;

        private readonly IEventPublisher _eventPublisher;

        public StoreService(IRepository<Store> storeRepository, IEventPublisher eventPublisher)
        {
            this._storeRepository = storeRepository;
            this._eventPublisher = eventPublisher;
        }

        public virtual void DeleteStore(Store store)
        {
            if (store == null)
                throw new ArgumentNullException("extraContent");

            _storeRepository.Delete(store);

            //event notification
            _eventPublisher.EntityDeleted(store);
        }

        public void InsertStore(Store store)
        {
            if (store == null)
                throw new ArgumentNullException("store");

            _storeRepository.Insert(store);

            //event notification
            _eventPublisher.EntityInserted(store);
            //return store.Id;

        }

        public Store GetStoreById(int storeId)
        {
            var store = _storeRepository.GetById(storeId);
            return store;
        }

        public void UpdateStore(Store store)
        {
            if (store == null)
                throw new ArgumentNullException("store");

            _storeRepository.Update(store);

        }
        public virtual IList<Store> GetStores()
        {
            var query = (from m in _storeRepository.Table
                         select m).Distinct().OrderBy(n => n.DisplayOrder).ThenBy(n => n.Name);
                var stores = query.ToList();
                    return stores;
        }
        public virtual IList<Store> GetStoresByCity(string city)
        {
            var query = (from m in _storeRepository.Table
                        where m.City == city
                        orderby m.DisplayOrder
                         select m).ThenBy(n => n.Name);
            var stores = query.ToList();
            return stores;
        }
        public virtual IList<string> GetCities()
        {
            var query = (from m in _storeRepository.Table
                         select m);
            query = query.OrderBy(m => m.DisplayOrder).ThenBy(m => m.City);
            var cities = query.ToList().GroupBy(x => x.City).Select(y => y.First().City).ToList();
            return cities;
        }
        public virtual IList<string> GetCountries()
        {
            var query = (from m in _storeRepository.Table
                         select m.Country).Distinct();
            var countries = query.ToList();
            return countries;
        }
        public virtual IList<Store> GetStoresByCityAndType(string city,string type)
        {
            var query = from m in _storeRepository.Table
                        where m.City == city & m.Type == type
                        orderby m.DisplayOrder
                        select m;
            var stores = query.ToList();
            return stores;
        }
    }
}
