using System;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework;
using System.ComponentModel.DataAnnotations;

namespace AF.Nop.Plugins.Criteo.Models
{
    public class ConfigurationModel : BaseNopModel
    {

        [Required]
        [NopResourceDisplayName("AF.Criteo.AccountId")]
        public int AccountId { get; set; }

        [NopResourceDisplayName("AF.Criteo.HashEmail")]
        public bool HashEmail { get; set; }
    }
}
