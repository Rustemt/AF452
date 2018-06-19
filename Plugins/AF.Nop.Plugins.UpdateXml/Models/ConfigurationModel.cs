using System;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using AF.Nop.Plugins.XmlUpdate.Domain;
using System.Web.Mvc;
using Telerik.Web.Mvc;

namespace AF.Nop.Plugins.XmlUpdate.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public ConfigurationModel()
        {
            Providers = new GridModel<ProviderModel>()
            {
            };
            AuthTypes = new List<SelectListItem>()
            {
                new SelectListItem() { Value="0", Text="" }
            };
        }

        public System.IO.FileInfo[] ReportFiles { get; set; }

        [Required]
        [NopResourceDisplayName("AF.XmlProvider.Text")]
        public string Text { get; set; }

        [NopResourceDisplayName("AF.XmlProvider.Enabled")]
        public bool Enabled { get; set; }

        public XmlProvider NewProviderModel { get; set; }

        [NopResourceDisplayName("AF.XmlProvider.Providers")]
        public GridModel<ProviderModel> Providers { get; }

        public IList<SelectListItem> AuthTypes { get; }
    }
}
