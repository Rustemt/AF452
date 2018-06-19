using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Xml;
using Nop.Core;
using Nop.Core.Plugins;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Core.Infrastructure;

namespace Nop.Plugin.ExchangeRate.TCMBExchange
{
    public class TCMBExchangeRateProvider : BasePlugin, IExchangeRateProvider
    {
        private readonly ILocalizationService _localizationService;

        public TCMBExchangeRateProvider(ILocalizationService localizationService)
        {
            this._localizationService = localizationService;
        }

        public IList<Core.Domain.Directory.ExchangeRate> GetCurrencyLiveRates(string exchangeRateCurrencyCode)
        {

            if (String.IsNullOrEmpty(exchangeRateCurrencyCode) ||
                exchangeRateCurrencyCode.ToLower() != "try")
                throw new NopException(_localizationService.GetResource("Plugins.ExchangeRate.TCMBExchange.SetCurrencyToTRY"));

            var exchangeRates = new List<Core.Domain.Directory.ExchangeRate>();
            var request = WebRequest.Create("http://www.tcmb.gov.tr/kurlar/today.xml") as HttpWebRequest;
            using (var response = request.GetResponse())
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(response.GetResponseStream());
                var provider = new NumberFormatInfo();
                provider.NumberDecimalSeparator = ".";
                provider.NumberGroupSeparator = "";
                DateTime date = DateTime.Now;

                try
                {
                    foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
                    {
                        var rate = new Core.Domain.Directory.ExchangeRate();
                        rate.UpdatedOn = date;
                        decimal unit = 1;
                        decimal rawRate = 0;
                        rate.CurrencyCode = node.Attributes["CurrencyCode"].Value;
                        foreach (XmlNode detailNode in node.ChildNodes)
                        {
                            switch (detailNode.Name)
                            {
                                case "Unit":
                                    decimal.TryParse(detailNode.InnerText, out unit);
                                    break;

                                case "ForexSelling":
                                    if (decimal.TryParse(detailNode.InnerText, NumberStyles.Currency, provider, out rawRate))
                                        rate.Rate = 1 / (rawRate / unit);
                                    break;

                                default:
                                    break;
                            }

                        }

                        // Update the Rate in the collection if its already in there
                        if (rate.CurrencyCode != null)
                        {
                            exchangeRates.Add(rate);
                        }
                    }
                }
                catch (Exception ex)
                {

                    throw;
                }
            }

            //var currencyService = EngineContext.Current.Resolve<ICurrencyService>();
          
            //foreach (var exchageRate in exchangeRates)
            //{
            //    var currency = currencyService.GetCurrencyByCode(exchageRate.CurrencyCode);
            //    if (currency != null)
            //    {
            //        currency.Rate = exchageRate.Rate;
            //        currency.UpdatedOnUtc = DateTime.UtcNow;
            //        currencyService.UpdateCurrency(currency);
            //    }
            //}
            return exchangeRates;
        }
    }
}
