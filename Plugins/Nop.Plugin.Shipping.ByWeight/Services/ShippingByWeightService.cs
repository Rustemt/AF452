using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Plugin.Shipping.ByWeight.Domain;
using Nop.Services.Directory;

namespace Nop.Plugin.Shipping.ByWeight.Services
{
    public partial class ShippingByWeightService : IShippingByWeightService
    {
        #region Fields

        private readonly IRepository<ShippingByWeightRecord> _sbwRepository;
        private readonly ICurrencyService _currencyService;
        private readonly ICacheManager _cacheManager;

        #endregion

        #region Ctor

        public ShippingByWeightService(ICacheManager cacheManager,
            IRepository<ShippingByWeightRecord> sbwRepository,
             ICurrencyService currencyService)
        {
            this._cacheManager = cacheManager;
            this._sbwRepository = sbwRepository;
            this._currencyService = currencyService;
        }

        #endregion

        #region Methods

        
        public virtual void DeleteShippingByWeightRecord(ShippingByWeightRecord shippingByWeightRecord)
        {
            if (shippingByWeightRecord == null)
                throw new ArgumentNullException("shippingByWeightRecord");

            _sbwRepository.Delete(shippingByWeightRecord);
        }

        public virtual IList<ShippingByWeightRecord> GetAll()
        {
            var query = from sbw in _sbwRepository.Table
                        orderby sbw.CountryId, sbw.ShippingMethodId, sbw.From
                        select sbw;
            var records = query.ToList();
            return records;
        }

        public virtual ShippingByWeightRecord FindRecord(int shippingMethodId, int countryId, decimal weight)
        {
            ShippingByWeightRecord result = null;
            var query = from sbw in _sbwRepository.Table
                        where sbw.ShippingMethodId == shippingMethodId && weight >= sbw.From && weight <= sbw.To
                        orderby sbw.CountryId, sbw.ShippingMethodId, sbw.From
                        select sbw;

            var existingRecords = query.ToList();

            //filter by country
            foreach (var sbw in existingRecords)
                if (countryId == sbw.CountryId)
                    result= sbw;

            foreach (var sbw in existingRecords)
                if (sbw.CountryId == 0)
                    result= sbw;

            if (result != null)
            {
                if (result.CurrencyId.HasValue)
                {
                    var cur = _currencyService.GetCurrencyById(result.CurrencyId.Value);
                    result.ShippingChargeAmount = _currencyService.ConvertToPrimaryStoreCurrency(result.ShippingChargeAmount, cur);
                }
            }

            return result;
        }

        public virtual ShippingByWeightRecord GetById(int shippingByWeightRecordId)
        {
            if (shippingByWeightRecordId == 0)
                return null;

            var record = _sbwRepository.GetById(shippingByWeightRecordId);
            return record;
        }

        public virtual void InsertShippingByWeightRecord(ShippingByWeightRecord shippingByWeightRecord)
        {
            if (shippingByWeightRecord == null)
                throw new ArgumentNullException("shippingByWeightRecord");

            _sbwRepository.Insert(shippingByWeightRecord);
        }

        public virtual void UpdateShippingByWeightRecord(ShippingByWeightRecord shippingByWeightRecord)
        {
            if (shippingByWeightRecord == null)
                throw new ArgumentNullException("shippingByWeightRecord");

            _sbwRepository.Update(shippingByWeightRecord);
        }

        #endregion
    }
}
