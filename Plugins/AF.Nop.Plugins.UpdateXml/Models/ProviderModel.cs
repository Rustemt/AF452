using System;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using AF.Nop.Plugins.XmlUpdate.Domain;
using System.Web.Mvc;
using AF.Nop.Plugins.XmlUpdate.Extensions;

namespace AF.Nop.Plugins.XmlUpdate.Models
{
    public class ProviderModel : BaseNopEntityModel
    {
        #region MyRegion
        public ProviderModel()
        {
            ProductProperties = new string[]
            {
                "Sku", "Price", "StockQuantity"
            };

            Properties = new List<PropertyModel>();
            foreach (var p in ProductProperties)
                Properties.Add(new PropertyModel
                {
                    ProductProperty = p
                });

            AuthTypes = new List<SelectListItem>()
            {
                new SelectListItem() { Value="0", Text="None" },
                new SelectListItem() { Value="1", Text="Basic" },
            };
        } 
        #endregion

        [Required]
        [NopResourceDisplayName("AF.XmlUpdate.Provider")]
        public string Name { get; set; }

        [Required]
        [NopResourceDisplayName("AF.XmlUpdate.SourceUrl")]
        public string Url { get; set; }

        [Required]
        [NopResourceDisplayName("AF.XmlUpdate.XmlRootNode")]
        public string XmlRootNode { get; set; }

        [Required]
        [NopResourceDisplayName("AF.XmlUpdate.XmlItemNode")]
        public string XmlItemNode { get; set; }

        [Required]
        [NopResourceDisplayName("AF.XmlUpdate.AuthType")]
        public int AuthType { get; set; }

        [NopResourceDisplayName("AF.XmlUpdate.Enabled")]
        public bool Enabled { get; set; }

        [NopResourceDisplayName("AF.XmlUpdate.Username")]
        public string Username { get; set; }

        [NopResourceDisplayName("AF.XmlUpdate.Password")]
        public string Password { get; set; }

        [NopResourceDisplayName("AF.XmlUpdate.AutoUnpublish")]
        public bool AutoUnpublish { get; set; }

        [NopResourceDisplayName("AF.XmlUpdate.AutoResetStock")]
        public bool AutoResetStock { get; set; }

        [NopResourceDisplayName("AF.XmlUpdate.UnpublishZeroStock")]
        public bool UnpublishZeroStock { get; set; }

        //[PropertyListValidator]
        [NopResourceDisplayName("AF.XmlUpdate.Properties")]
        public IList<PropertyModel> Properties { get; protected set; }
        public string[] ProductProperties { get; }

        public IList<SelectListItem> AuthTypes { get; }

        public void SetProperties(IList<XmlProperty> list)
        {
            Properties = new List<PropertyModel>();
            foreach(var name in ProductProperties)
            {
                bool found = false;
                foreach(var prop in list)
                {
                    if(name == prop.ProductProperty)
                    {
                        found = true;
                        Properties.Add(prop.ToViewModel());
                        break;
                    }
                }
                if (!found)
                    Properties.Add(new PropertyModel() { ProductProperty = name });
            }
        }
    }
}
