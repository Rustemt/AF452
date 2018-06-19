using System;
using System.Web.Routing;
using Nop.Core.Domain.Shipping;
using Nop.Core.Plugins;
using Nop.Plugin.Shipping.ByWeight.Data;
using Nop.Plugin.Shipping.ByWeight.Services;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Shipping;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Core.Domain.Orders;
using System.Collections.Generic;
using Nop.Core.Domain.Customers;

namespace Nop.Plugin.Shipping.ByWeight
{
    public class ByWeightShippingComputationMethod : BasePlugin, IShippingRateComputationMethod
    {
        #region Fields

        private readonly IShippingService _shippingService;
        private readonly IShippingByWeightService _shippingByWeightService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ShippingByWeightSettings _shippingByWeightSettings;
        private readonly ShippingByWeightObjectContext _objectContext;
        private readonly ShippingSettings _shippingSettings;
        private readonly IAddressService _addressService;
        private readonly ISettingService _settingService;
        #endregion

        #region Ctor
        public ByWeightShippingComputationMethod(IShippingService shippingService,
            IShippingByWeightService shippingByWeightService,
            IPriceCalculationService priceCalculationService, 
            ShippingByWeightSettings shippingByWeightSettings,
            ShippingByWeightObjectContext objectContext,
            ShippingSettings shippingSettings,
            IAddressService addressService,
            ISettingService settingService
            )
        {
            this._shippingService = shippingService;
            this._shippingByWeightService = shippingByWeightService;
            this._priceCalculationService = priceCalculationService;
            this._shippingByWeightSettings = shippingByWeightSettings;
            this._objectContext = objectContext;
            this._shippingSettings = shippingSettings;
            this._addressService = addressService;
            this._settingService = settingService;
        }
        #endregion

        #region Utilities
        
        private decimal? GetRate(decimal subTotal, decimal weight, int shippingMethodId, int countryId)
        {
            decimal? shippingTotal = null;

            var shippingByWeightRecord = _shippingByWeightService.FindRecord(shippingMethodId, countryId, weight);
            if (shippingByWeightRecord == null)
            {
                if (_shippingByWeightSettings.LimitMethodsToCreated)
                    return null;
                else
                    return decimal.Zero;
            }
            if (shippingByWeightRecord.UsePercentage && shippingByWeightRecord.ShippingChargePercentage <= decimal.Zero)
                return decimal.Zero;
            if (!shippingByWeightRecord.UsePercentage && shippingByWeightRecord.ShippingChargeAmount <= decimal.Zero)
                return decimal.Zero;
            if (shippingByWeightRecord.UsePercentage)
                shippingTotal = Math.Round((decimal)((((float)subTotal) * ((float)shippingByWeightRecord.ShippingChargePercentage)) / 100f), 2);
            else
            {
                if (_shippingByWeightSettings.CalculatePerWeightUnit)
                    shippingTotal = shippingByWeightRecord.ShippingChargeAmount * weight;
                else
                    shippingTotal = shippingByWeightRecord.ShippingChargeAmount;
            }
            if (shippingTotal < decimal.Zero)
                shippingTotal = decimal.Zero;
            return shippingTotal;
        }

        /// <summary>
        /// Gets shopping cart weight
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <returns>Shopping cart weight</returns>
        private decimal GetShoppingCartTotalDeci(IList<ShoppingCartItem> cart)
        {
            decimal totalDeci = decimal.Zero;
            decimal weight = 0;
            decimal deci = 0;
            decimal height=0;
            decimal width =0;
            decimal length = 0;
            //shopping cart items
            foreach (var shoppingCartItem in cart)
            {
                var productVariant = shoppingCartItem.ProductVariant;
                weight = _shippingService.GetShoppingCartItemTotalWeight(shoppingCartItem);
                deci = 0;
                height = productVariant.Height * shoppingCartItem.Quantity;
                width = productVariant.Width * shoppingCartItem.Quantity;
                length = productVariant.Length * shoppingCartItem.Quantity;

                if (height > 0 && width > 0 && length > 0)
                {
                    deci = height * width * length / 5;
                }
                deci = Math.Max(deci, weight);
                totalDeci += deci;
            }

            //checkout attributes
            totalDeci += _shippingService.GetShoppingCartCheckoutAttibutesWeight(cart);
            return totalDeci;
        }

       



        #endregion

        #region Methods

        /// <summary>
        ///  Gets available shipping options
        /// </summary>
        /// <param name="getShippingOptionRequest">A request for getting shipping options</param>
        /// <returns>Represents a response of getting shipping rate options</returns>
        public GetShippingOptionResponse GetShippingOptions(GetShippingOptionRequest getShippingOptionRequest)
        {
            if (getShippingOptionRequest == null)
                throw new ArgumentNullException("getShippingOptionRequest");

            var response = new GetShippingOptionResponse();

            if (getShippingOptionRequest.Items == null || getShippingOptionRequest.Items.Count == 0)
            {
                response.AddError("No shipment items");
                return response;
            }
            if (getShippingOptionRequest.ShippingAddress == null)
            {
                var originAddress = _shippingSettings.ShippingOriginAddressId > 0
                                  ? _addressService.GetAddressById(_shippingSettings.ShippingOriginAddressId)
                                  : null;
                if (originAddress == null)
                {
                    response.AddError("Origin Address or Shipping address is not set");
                    return response;
                }
                getShippingOptionRequest.ShippingAddress = originAddress;
            }
            
            int countryId = getShippingOptionRequest.ShippingAddress.CountryId.HasValue ? getShippingOptionRequest.ShippingAddress.CountryId.Value : 0;
            
            decimal subTotal = decimal.Zero;
            foreach (var shoppingCartItem in getShippingOptionRequest.Items)
            {
                if (shoppingCartItem.IsFreeShipping || !shoppingCartItem.IsShipEnabled)
                    continue;
                subTotal += _priceCalculationService.GetSubTotal(shoppingCartItem, true);
            }

            decimal deci = GetShoppingCartTotalDeci(getShippingOptionRequest.Items);

            var shippingMethods = _shippingService.GetAllShippingMethods(countryId);
            decimal additionalRate = _settingService.GetSettingByKey<decimal>("ShippingRateComputationMethod.ByWeight.DHLRate");
            additionalRate = Math.Max(1, additionalRate);
            foreach (var shippingMethod in shippingMethods)
            {
                decimal? rate = GetRate(subTotal, deci, shippingMethod.Id, countryId);
                if (rate.HasValue)
                {
                    var shippingOption = new ShippingOption();
                    shippingOption.Name = shippingMethod.GetLocalized(x => x.Name);
                    shippingOption.Description = shippingMethod.GetLocalized(x => x.Description);
                    shippingOption.Rate = rate.Value * additionalRate;
                    response.ShippingOptions.Add(shippingOption);
                }
            }


            return response;
        }

        /// <summary>
        /// Gets fixed shipping rate (if shipping rate computation method allows it and the rate can be calculated before checkout).
        /// </summary>
        /// <param name="getShippingOptionRequest">A request for getting shipping options</param>
        /// <returns>Fixed shipping rate; or null in case there's no fixed shipping rate</returns>
        public decimal? GetFixedRate(GetShippingOptionRequest getShippingOptionRequest)
        {
            return 0;
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "ShippingByWeight";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Shipping.ByWeight.Controllers" }, { "area", null } };
        }
        
        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            //database objects
            _objectContext.Install();

            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeight.Fields.Country", "Country");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeight.Fields.Country.Hint", "If an asteriks is selected, then this tax rate will apply to all customers, regardless of the country.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeight.Fields.ShippingMethod", "Shipping method");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeight.Fields.ShippingMethod.Hint", "The shipping method.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeight.Fields.From", "Order weight from");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeight.Fields.From.Hint", "Order weight from.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeight.Fields.To", "Order weight to");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeight.Fields.To.Hint", "Order weight toy.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeight.Fields.UsePercentage", "Use percentage");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeight.Fields.UsePercentage.Hint", "Check to use 'charge percentage' value.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeight.Fields.ShippingChargePercentage", "Charge percentage (of subtotal)");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeight.Fields.ShippingChargePercentage.Hint", "Charge percentage (of subtotal).");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeight.Fields.ShippingChargeAmount", "Charge amount");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeight.Fields.ShippingChargeAmount.Hint", "Charge amount.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeight.Fields.LimitMethodsToCreated", "Limit shipping methods to configured ones");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeight.Fields.LimitMethodsToCreated.Hint", "If you check this option, then your customers will be limited to shipping options configured here. Otherwise, they'll be able to choose any existing shipping options even they've not configured here (zero shipping fee in this case).");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeight.Fields.CalculatePerWeightUnit", "Calculate per weight unit");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeight.Fields.CalculatePerWeightUnit.Hint", "If you check this option, then rates are multiplied per weight unit (lb, kg, etc). This option is used for the fixed rates (without percents).");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeight.AddRecord", "Add record");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeight.AddRecord.Hint", "Adding a new record");
            
            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //database objects
            _objectContext.Uninstall();

            //locales
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeight.Fields.Country");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeight.Fields.Country.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeight.Fields.ShippingMethod");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeight.Fields.ShippingMethod.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeight.Fields.From");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeight.Fields.From.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeight.Fields.To");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeight.Fields.To.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeight.Fields.UsePercentage");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeight.Fields.UsePercentage.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeight.Fields.ShippingChargePercentage");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeight.Fields.ShippingChargePercentage.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeight.Fields.ShippingChargeAmount");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeight.Fields.ShippingChargeAmount.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeight.Fields.LimitMethodsToCreated");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeight.Fields.LimitMethodsToCreated.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeight.Fields.CalculatePerWeightUnit");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeight.Fields.CalculatePerWeightUnit.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeight.AddRecord");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeight.AddRecord.Hint");
            
            base.Uninstall();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a shipping rate computation method type
        /// </summary>
        public ShippingRateComputationMethodType ShippingRateComputationMethodType
        {
            get
            {
                return ShippingRateComputationMethodType.Offline;
            }
        }

        #endregion
    }
}
