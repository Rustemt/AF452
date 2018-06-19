using System;
using System.Collections.Generic;
using Nop.Core.Configuration;
using System.Linq;
namespace Nop.Services.ExportImport
{
    public class NebimIntegrationSettings : ISettings
    {
        public string UserGroup { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ServerNameIP { get; set; }
        public string Database { get; set; }
        public bool ProductsSyncEnabled { get; set; }
        public int ProductsSyncStartTimeMinutes { get; set; }
        public long LastProductsSyncTime { get; set; }
        public long LastOrdersSyncTime { get; set; }
        public string API_OfficeCode { get; set; }//"O-2" : Daimamoda
        public string API_WarehouseCode { get; set; }//"1-2-1-1" : Daimamoda Merkez
        public string API_StoreCode { get; set; }//1-5-1-1
        public string API_CommunicationTypeEmail { get; set; }//3
        public string API_CommunicationTypePhone { get; set; }//1
        public string API_BankAccountCode { get; set; }//1-6-1-1 :Garanti
        public string API_CashAccountCode { get; set; }//not yet supported
        public byte API_AttributeTypeCodeForManufacturer { get; set; }
        public string API_ShipmentExpenseProductCode { get; set; }//"KRG"
        public string API_GiftboxExpenseProductCode { get; set; }//"GFT"
        public string API_CustomerCodePrefix { get; set; }//"KRG"
        public string API_StandardColorCode { get; set; }//"STD"
        public string API_StandardSizeCode { get; set; }//"x"
        public int ColorAttributeId { get; set; }// renk dinamik olduðu zaman
        public int ColorSpecificationAttributeId { get; set; }// renk specification attribute olduðu zaman
        public int Dim1AttributeId { get; set; }
        public int Dim2AttributeId { get; set; }
        public string SpecificationAttribute_API_AttributeTypeCodes { get; set; }//specifications matching to be stored at nebim : 52-2,57-3
        public string SpecificationOptionId_API_AttributeCodes { get; set; }
        public string Category_API_HierarchyIds { get; set; }//category matching to be stored at nebim : 20-1,22-2,34-5
        public string PaymentMethodSystemName_API_CreditCardTypeCode { get; set; }//PaymentMethodSystemName-CreditCardTypeCode matching:  Payments.CC.KuveytTurk--1-6-1-4,Payments.CC.Garanti--1-6-1-1,Payments.CC.YapiKredi--1-6-1-2,Payments.CC.Akbank--1-6-1-3
        public string StateProvinceId_API_CityCodes { get; set; }//160--TR.01,161--TR.02
        public string ColorNames_API_ColorCodes { get; set; }
        public string TaxCategory_API_ItemAccountGrCodes { get; set; } //mapping for Tax categories: tax category id * API_ItemAccountGrCodes
        public string AddressFormat { get; set; }
        public bool DiscountNameSavingEnabled { get; set; }
        public string ProdImpLanguageCode { get; set; }
        public string ProdImpPriceGroup1Code { get; set; }
        public string ProdImpPriceGroup2Code { get; set; }
        public string ProdImpWarehouse1Code { get; set; }
        public string ProdImpWarehouse2Code { get; set; }
        public string ProdImpBarcodeTypeCode1 { get; set; }
        public int ProdImpLastDay { get; set; }

        private Dictionary<string, List<string>> _colorMapping;
        /// <summary>
        /// Nop color name * nebim color code
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        public Dictionary<string, List<string>> ColorMapping
        {
            get
            {
                if (_colorMapping == null)
                {
                    //"B2C Renk Adý"--"RenkKodu1,RenkKodu2,RenkKodu3,";
                    //Beyaz--BYZ,BYZ1;Sarý--SR;Kýrmýzý--KMZ,KMZ1,KMZ2
                    var setting = this.ColorNames_API_ColorCodes;
                    Dictionary<string, List<string>> mapping = new Dictionary<string, List<string>>();
                    if (string.IsNullOrWhiteSpace(setting)) return mapping;
                    foreach (var match in setting.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var codes = match.Split(new string[] { "--" }, StringSplitOptions.RemoveEmptyEntries);
                        var list = new List<string>(codes[1].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries));
                        mapping.Add(codes[0], list);
                    }
                    _colorMapping = mapping;
                }
                return _colorMapping;
            }
        }

        private Dictionary<int, int> _attributeMapping;
        /// <summary>
        /// (nop specification attribute id)*(nebim attributetypecode)
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        public Dictionary<int, int> AttributeMapping
        {
            get
            {
                if (_attributeMapping == null)
                {
                    string setting = this.SpecificationAttribute_API_AttributeTypeCodes;
                    Dictionary<int, int> mapping = new Dictionary<int, int>();
                    if (string.IsNullOrWhiteSpace(setting)) return mapping;
                    foreach (var match in setting.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var codes = match.Split(new string[] { "--" }, StringSplitOptions.RemoveEmptyEntries);
                        mapping.Add(int.Parse(codes[0]), int.Parse(codes[1]));
                    }
                    _attributeMapping = mapping;
                }
                return _attributeMapping;
            }
        }

        private Dictionary<int, string> _specificationOptionMapping;
        public Dictionary<int, string> SpecificationOptionMapping
        {
            get
            {
                if (_specificationOptionMapping == null)
                {
                    string setting = this.SpecificationOptionId_API_AttributeCodes;
                    Dictionary<int, string> mapping = new Dictionary<int, string>();
                    if (string.IsNullOrWhiteSpace(setting)) return mapping;
                    foreach (var match in setting.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var codes = match.Split(new string[] { "--" }, StringSplitOptions.RemoveEmptyEntries);
                        mapping.Add(int.Parse(codes[0]), codes[1]);
                    }
                    _specificationOptionMapping = mapping;
                }
                return _specificationOptionMapping;
            }
        }
        public int GetB2CSpecificationOptionId(string APIAttributeCode)
        {
            try
            {
                var match = SpecificationOptionMapping.FirstOrDefault(x => x.Value.ToLower().Trim() == APIAttributeCode.ToLower().Trim());
                var specificationOptionId = match.Key;
                return specificationOptionId;
            }
            catch (Exception ex)
            {
                throw new Exception("Nebim Integration->Specification Attribute mapping error.APIAttributeCode:" + APIAttributeCode, ex);
            }
        }

        public string GetB2CColorSpecName(string APIColorCode)
        {
            try
            {
                var colorMatch = ColorMapping.FirstOrDefault(x => x.Value.Any(y => y == APIColorCode));
                return colorMatch.Key;
            }
            catch (Exception ex)
            {
                throw new Exception("Nebim Integration->Color mapping error.APIColorCode:" + APIColorCode, ex);
            }
        }

        private Dictionary<int, string> _taxCategoryMapping;
        public Dictionary<int, string> TaxCategoryMapping
        {
            get
            {
                if (_taxCategoryMapping == null)
                {
                    string setting = this.TaxCategory_API_ItemAccountGrCodes;
                    Dictionary<int, string> mapping = new Dictionary<int, string>();
                    if (string.IsNullOrWhiteSpace(setting)) return mapping;
                    foreach (var match in setting.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var codes = match.Split(new string[] { "--" }, StringSplitOptions.RemoveEmptyEntries);
                        mapping.Add(int.Parse(codes[0]), codes[1]);
                    }
                    _taxCategoryMapping = mapping;
                }
                return _taxCategoryMapping;
            }
        }
        public int GetB2CTaxCategoryId(string APIItemAccountGrCode)
        {
            try
            {
                var match = this.TaxCategoryMapping.FirstOrDefault(x => x.Value.ToLower().Trim() == APIItemAccountGrCode.ToLower().Trim());
                return match.Key;
            }
            catch (Exception ex)
            {
                throw new Exception("Nebim Integration->Tax category mapping error.APIItemAccountGrCode:" + APIItemAccountGrCode, ex);
            }
        }

        private Dictionary<int, int> _categoryMapping;
        public Dictionary<int, int> CategoryMapping
        {
            get
            {
                if (_categoryMapping == null)
                {
                    string setting = this.Category_API_HierarchyIds;
                    Dictionary<int, int> mapping = new Dictionary<int, int>();
                    if (string.IsNullOrWhiteSpace(setting)) return mapping;
                    foreach (var match in setting.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var codes = match.Split(new string[] { "--" }, StringSplitOptions.RemoveEmptyEntries);
                        mapping.Add(int.Parse(codes[0]), int.Parse(codes[1]));
                    }
                    _categoryMapping = mapping;
                }
                return _categoryMapping;
            }
        }
        public int GetB2CCategoryId(int APIHierarcyId)
        {
            try
            {
                var match = CategoryMapping.FirstOrDefault(x => x.Value == APIHierarcyId);
                return match.Key;
            }
            catch (Exception ex)
            {
                throw new Exception("Nebim Integration->Category mapping error.HierarchyId:" + APIHierarcyId, ex);
            }
        }

        private Dictionary<int, string> _provinceMapping;
        public Dictionary<int, string> ProvinceMapping
        {
            get
            {
                if (_provinceMapping == null)
                {
                    string setting = this.StateProvinceId_API_CityCodes;
                    Dictionary<int, string> mapping = new Dictionary<int, string>();
                    if (string.IsNullOrWhiteSpace(setting)) return mapping;
                    foreach (var match in setting.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var codes = match.Split(new string[] { "--" }, StringSplitOptions.RemoveEmptyEntries);
                        mapping.Add(int.Parse(codes[0]), codes[1]);
                    }
                    _provinceMapping = mapping;
                }
                return _provinceMapping;
            }
        }
        public string GetAPICityCode(int B2CProvinceId)
        {
            try
            {
                return ProvinceMapping[B2CProvinceId];
            }
            catch (Exception ex)
            {
                throw new Exception("Nebim Integration->Province mapping error.B2CProvinceId:" + B2CProvinceId, ex);
            }
        }

    }
}
