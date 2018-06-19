using System;
using Nop.Web.Framework.Mvc;
using Telerik.Web.Mvc;

namespace AF.Nop.Plugins.XmlUpdate.Models
{
    public class ImportXmlModel : BaseNopEntityModel
    {
        public ImportXmlModel()
        {
            Products = new GridModel<ProcessRecord>();
        }

        public ProviderModel Provider { get; set; }

        public GridModel<ProcessRecord> Products { get; set; }
    }
}
