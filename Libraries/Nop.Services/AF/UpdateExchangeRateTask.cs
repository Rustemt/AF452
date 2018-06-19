using System;
using System.Xml;
using Nop.Core.Configuration;
using Nop.Core.Domain.Directory;
using Nop.Core.Infrastructure;
using Nop.Services.Tasks;
using Nop.Services.Configuration;
using Nop.Services.Catalog;
using Nop.Core.Caching;
using Nop.Services.Logging;
using Nop.Core.Domain.Catalog;

namespace Nop.Services.Directory
{
    /// <summary>
    /// Represents a task for updating exchange rates
    /// </summary>
    public partial class UpdateExchangeRateTask : ITask
    {
        public void ExecuteForce()
        {
            var currencySettings = EngineContext.Current.Resolve<IConfigurationProvider<CurrencySettings>>().Settings;
            var logger = EngineContext.Current.Resolve<ILogger>();
            try
            {
                logger.Information("Currency updating started");
                var currencyService = EngineContext.Current.Resolve<ICurrencyService>();
                var exchangeRates = currencyService.GetCurrencyLiveRates(currencyService.GetCurrencyById(currencySettings.PrimaryExchangeRateCurrencyId).CurrencyCode);
                foreach (var exchageRate in exchangeRates)
                {
                    var currency = currencyService.GetCurrencyByCode(exchageRate.CurrencyCode);
                    if (currency != null)
                    {
                        currency.Rate = exchageRate.Rate;
                        currency.UpdatedOnUtc = DateTime.UtcNow;
                        currencyService.UpdateCurrency(currency);
                    }
                }

                var updateProductsDisabled = EngineContext.Current.Resolve<Nop.Services.Configuration.ISettingService>().GetSettingByKey<bool?>("currencysettings.updateproductsafternewratesdisabled");
                if (updateProductsDisabled.HasValue && updateProductsDisabled.Value) return;
                var productService = EngineContext.Current.Resolve<IProductService>();
                logger.Information("Kur guncelleme islemi. Tum urunler hafizaya aliniyor...");
                var products = productService.GetAllProducts(showHidden: true);
                logger.Information("Kur guncelleme islemi. Tum urunler hafizaya alindi.");
                logger.Information("Start updating products due to exchange-rate changes. Prodcuts count: "+products.Count);
                long startTs = DateTime.Now.Ticks;
                int counter = 0;
                #region Updating Products
                foreach (var product in products)
                {
                    foreach (var variant in product.ProductVariants)
                    {
                        counter++;
                        if (variant.CurrencyId.HasValue && variant.CurrencyId != currencySettings.PrimaryStoreCurrencyId)
                        {
                            try
                            {
                                if (variant.CurrencyPrice.HasValue)
                                    variant.Price = Decimal.Round(currencyService.ConvertToPrimaryStoreCurrency(variant.CurrencyPrice.Value, variant.Currency));
                                if (variant.CurrencyOldPrice.HasValue)
                                    variant.OldPrice = Decimal.Round(currencyService.ConvertToPrimaryStoreCurrency(variant.CurrencyOldPrice.Value, variant.Currency));
                                if (variant.CurrencyProductCost.HasValue)
                                    variant.ProductCost = Decimal.Round(currencyService.ConvertToPrimaryStoreCurrency(variant.CurrencyProductCost.Value, variant.Currency));
                            }
                            catch (Exception ex)
                            {
                                logger.Information("Kur guncelleme esnasinda Price, OldPrice yada ProductCost atamasi hata turetti. ProductVariantId ->" + variant.Id.ToString(), ex);
                                continue;
                            }

                            try
                            {
                                //productService.UpdateProductVariant(variant);
                            }
                            catch (Exception ex)
                            {
                                logger.Information("Kur guncelleme esnasinda UpdateProductVariant() metodu hata turetti. ProductVariantId ->" + variant.Id.ToString(), ex);
                                continue;
                            }
                        }
                        if (counter % 1000 == 0)
                            logger.Information(string.Format("Update Exchange: number of products updated till now: {0}", counter));

                    }
                }
                #endregion

                var cacheManager = EngineContext.Current.Resolve<ICacheManager>();
                productService.UpdateProductVariant(new ProductVariant());// update every thing and clear cache

                long endTs = DateTime.Now.Ticks;
                TimeSpan ts = new TimeSpan(endTs - startTs);
                logger.Information(string.Format("Updating products' prices took: {0} sec",ts));
                //save new update time value
                currencySettings.LastUpdateTime = DateTime.UtcNow.ToBinary();
                var settingService = EngineContext.Current.Resolve<ISettingService>();
                settingService.SaveSetting(currencySettings);
                logger.Information("Currency updating ended");

                //clear cache
                var clearCacheAfterProductsCurrencyPricesUpdated = EngineContext.Current.Resolve<Nop.Services.Configuration.ISettingService>().GetSettingByKey<bool>("currencysettings.clearcacheafterproductscurrencypricesupdated", false);
                if (clearCacheAfterProductsCurrencyPricesUpdated)
                {
                    logger.Information("Cache is being cleared after currency rate update.");
                    //var cacheManager = new MemoryCacheManager();
                    cacheManager.Clear();
                    logger.Information("Cache is cleared after currency rate update.");

                    var requestBulkCatalogForCaching = EngineContext.Current.Resolve<Nop.Services.Configuration.ISettingService>().GetSettingByKey<bool>("currencysettings.requestbulkcatalogforcaching", false);
                    if (requestBulkCatalogForCaching)
                    {
                        logger.Information("Bulk requests for caching started-sync.");
                        EngineContext.Current.Resolve<Nop.Services.Catalog.IProductService>().RequestBulkCatalog();
                        logger.Information("Bulk requests for caching ended-sync.");
                    }
                }

            }
            catch (Exception e)
            {
                logger.Error("Update Currency", e);
            }
        }
        /// <summary>
        /// Executes a task
        /// </summary>
        public void Execute()
        {
            
            var currencySettings = EngineContext.Current.Resolve<IConfigurationProvider<CurrencySettings>>().Settings;
            if (!currencySettings.AutoUpdateEnabled)
                return;
            if (DateTime.Now.TimeOfDay > new TimeSpan(5, 30, 0))
                return;

            long lastUpdateTimeTicks = currencySettings.LastUpdateTime;
            DateTime lastUpdateTime = DateTime.FromBinary(lastUpdateTimeTicks);
            lastUpdateTime = DateTime.SpecifyKind(lastUpdateTime, DateTimeKind.Utc);
            if (lastUpdateTime.AddHours(1) < DateTime.UtcNow)
            {

                ExecuteForce();
            }
        }
    }
}
