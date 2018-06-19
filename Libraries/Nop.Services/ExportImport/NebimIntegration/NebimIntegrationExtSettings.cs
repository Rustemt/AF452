using Nop.Core.Configuration;

namespace Nop.Services.ExportImport
{
    public class NebimIntegrationExtSettings : NebimIntegrationSettings
    {
        //public string UserGroup { get; set; }
        //public string UserName { get; set; }      
        //public string Password { get; set; }      
        //public string ServerNameIP { get; set; } 
        //public string Database { get; set; }
        //public bool ProductsSyncEnabled { get; set; }
        //public int ProductsSyncStartTimeMinutes { get; set; }
        //public long LastProductsSyncTime { get; set; }
        //public long LastOrdersSyncTime { get; set; }

        //public string API_OfficeCode { get; set; }//"O-2" : Daimamoda
        //public string API_WarehouseCode { get; set; }//"1-2-1-1" : Daimamoda Merkez
        //public string API_StoreCode { get; set; }//1-5-1-1
        //public string API_CommunicationTypeEmail { get; set; }//3
        //public string API_CommunicationTypePhone { get; set; }//1
        //public string API_BankAccountCode { get; set; }//1-6-1-1 :Garanti
        //public string API_CashAccountCode { get; set; }//not yet supported
        //public byte API_AttributeTypeCodeForManufacturer { get; set; }
        //public string API_ShipmentExpenseProductCode { get; set; }//"KRG"
       
        //public int ColorAttributeId { get; set; }
        //public int Dim1AttributeId { get; set; }
        //public int Dim2AttributeId { get; set; }
        //public string SpecificationAttribute_API_AttributeTypeCodes { get; set; }//specifications matching to be stored at nebim : 52-2,57-3
        //public string Category_API_HierarchyLevelCodes { get; set; }//category matching to be stored at nebim : 20-1,22-2,34-5
        //public string PaymentMethodSystemName_API_CreditCardTypeCode { get; set; }//PaymentMethodSystemName-CreditCardTypeCode matching:  Payments.CC.KuveytTurk--1-6-1-4,Payments.CC.Garanti--1-6-1-1,Payments.CC.YapiKredi--1-6-1-2,Payments.CC.Akbank--1-6-1-3
        //public string StateProvinceId_API_CityCodes { get; set; }//160--TR.01,161--TR.02


        // sql script to add settings

//        INSERT INTO Setting(Name,Value)
//select REPLACE([Name],'settings.','extsettings.')
//      ,[Value]
//  FROM [AF2].[dbo].[Setting]
//  where Name like 'Nebim%'


    }
}
