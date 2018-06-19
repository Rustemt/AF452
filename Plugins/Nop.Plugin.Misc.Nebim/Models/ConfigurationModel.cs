using System.ComponentModel;
using System.Web.Mvc;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Misc.NebimIntegration
{
    public class ConfigurationModel : BaseNopModel
    {

        [DisplayName("User Group")]
        public string UserGroup { get; set; }
        [DisplayName("User Name")]
        public string UserName { get; set; }
        [DisplayName("Password")]
        public string Password { get; set; }
        [DisplayName("Server IP")]
        public string ServerNameIP { get; set; }
        [DisplayName("Database")]
        public string Database { get; set; }
        [DisplayName("Products Sync Enabled")]
        public bool ProductsSyncEnabled { get; set; }
        [DisplayName("StartTimeMinutes")]
        public int ProductsSyncStartTimeMinutes { get; set; }
        [DisplayName("API_OfficeCode")]
        public string API_OfficeCode { get; set; }//"O-2" : Daimamoda
        [DisplayName("API_WarehouseCode")]
        public string API_WarehouseCode { get; set; }//"1-2-1-1" : Daimamoda Merkez
        [DisplayName("API_CommunicationTypeEmail")]
        public string API_CommunicationTypeEmail { get; set; }//3
        [DisplayName("API_CommunicationTypePhone")]
        public string API_CommunicationTypePhone { get; set; }//1
        [DisplayName("ColorAttributeId")]
        public int ColorAttributeId { get; set; }
        [DisplayName("Dim1AttributeId")]
        public int Dim1AttributeId { get; set; }
        [DisplayName("Dim2AttributeId")]
        public int Dim2AttributeId { get; set; }

        [DisplayName("SpecificationAttribute_API_AttributeTypeCodes")]
        public string SpecificationAttribute_API_AttributeTypeCodes { get; set; }//specifications matching to be stored at nebim : 52-2,57-3
        [DisplayName("API_AttributeTypeCodeForManufacturer")]
        public byte API_AttributeTypeCodeForManufacturer { get; set; }
        [DisplayName("Category_API_HierarchyLevelCodes")]
        public string Category_API_HierarchyLevelCodes { get; set; }//category matching to be stored at nebim : 20-1,22-2,34-5

    }
}