using System;
using Nop.Core;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework;

namespace AF.Nop.Plugins.XmlUpdate.Models
{
    public class PropertyModel : BaseNopModel
    {
        public int? Id { get; set; }
        
        [NopResourceDisplayName("AF.XmlUpdate.Property.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("AF.XmlUpdate.Property.ProductProperty")]
        public string ProductProperty { get; set; }

        [NopResourceDisplayName("AF.XmlUpdate.Property.Enabled")]
        public bool Enabled { get; set; }
    }
}
