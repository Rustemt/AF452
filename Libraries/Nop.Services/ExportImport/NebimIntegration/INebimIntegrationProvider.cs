using System.Collections.Generic;
using Nop.Core.Domain.Directory;
using Nop.Core.Plugins;
using Nop.Services.ExportImport;
using Nop.Services.Common;
using System;

namespace Nop.Services.Directory
{
    /// <summary>
    /// Exchange rate provider interface
    /// </summary>
    public partial interface INebimIntegrationProvider : IMiscPlugin
    {
        void ResetConnection();
        void Connect(NebimIntegrationSettings settings);

        string AddUpdateCustomerToNebim(string description, string languageCode, string firstName, string lastName,
           List<string> emails, List<string> phones, string identityNum, string gender, string customerSegment, DateTime? registerDate);

        Guid AddUpdateCustomerAddressToNebimByCustomerDescription(string customerDescription, string type,
           string firstName, string lastName,
           string addressLine, string district, string city,
           string zipCode, string countryCode,
           string taxNumber, string taxOffice);
        Guid AddUpdateCustomerAddressToNebimByCustomerCode(string customerCode, string type,
         string firstName, string lastName,
         string addressLine, string district, string city,
         string zipCode, string countryCode,
         string taxNumber, string taxOffice);

        string AddUpdateOrderToNebim(string description, DateTime? orderDate, string customerCode,
            Guid? shippingPostalAddressID, Guid? BillingPostalAddressID,
            IList<Tuple<string, string, string, string, int, decimal?, decimal?, Tuple<string>>> items,
            decimal? discountAmount, double? discountRate, string currencyCode, decimal shippingCost, decimal giftBoxCost, string discountNames);

        void CreateShipment(string orderDescription);


        void AddUpdateProductToNebim(
            string Barcode,
                    string ProductCode,/*sku-manufacturer part number*/
                    string ProductNameTR,
                    string ProductNameEN,
                    int ItemDimTypeCode,
                    IList<Tuple<string, string, string, string, string, string, string>> combinations,//renk kodu,renk adý, renk adý, boyut kodu,boyut adý,boyut adý
                    string UnitOfMeasureCode1,
                    string CountryCode,
                    string PurcVatRate,//tax rate
                    string SellVatRate,//tax rate
                    string ItemAccountGrCode,//tax rate
                    int[] ProductHierarchyLevelCodes,
                    IList<Tuple<byte, string, string, string>> attributes,//attr type code (1-20),attr code, attr name,attr name
                  decimal price1,
                  decimal price2,
                  decimal price3,
                    string currencyCode);

        void AddUpdateProductToNebim(
                  string Barcode,
                  string ProductCode,/*sku*/
                  string ProductNameTR,
                  string ProductNameEN,
                  int ItemDimTypeCode,
                  IList<Tuple<string, string, string, string, string, string, string>> combinations,//barcode, renk kodu,renk adý, renk adý, boyut kodu,boyut adý,boyut adý
                  string UnitOfMeasureCode1,
                  string CountryCode,
                  string PurcVatRate,//tax rate
                  string SellVatRate,//tax rate
                  string ItemAccountGrCode,//tax rate
                  Dictionary<int, string> ProductHierarchyLevelNames,// level*ProductHierarchyLevelName
                  IList<Tuple<byte, string, string, string>> attributes,//attr type code (1-20),attr code, attr name,attr name
                  decimal price1,
                  decimal price2,
                  decimal price3,
                  string currencyCode);

        void AddUpdateOrderPaymentToNebim(string orderNumber, int paymentType, string creditCardTypeCode, byte installment, string maskedCCNo, string provisionNo);

        System.Data.DataTable GetAllProducts();


    }
}