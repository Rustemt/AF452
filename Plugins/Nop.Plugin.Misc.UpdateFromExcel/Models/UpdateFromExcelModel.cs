using System.Collections.Generic;
using System.Web.Mvc;
using Nop.Web.Framework;

namespace Nop.Plugin.Misc.UpdateFromExcel.Models
{
    public class UpdateFromExcelModel
    {
        IList<SelectListItem> _availableManufacturers;

        /// <summary>
        /// Gets or sets the list options.
        /// </summary>
        /// <value>
        /// The list options.
        /// </value>
        public virtual IList<SelectListItem> AvailableManufacturers
        {
            get { return _availableManufacturers ?? (_availableManufacturers = new List<SelectListItem>()); }
            set { _availableManufacturers = value; }
        }

        [NopResourceDisplayName("Plugin.Misc.UpdateFromExcel.Manufacturer")]
        public virtual int ManufacturerId { get; set; }

        [NopResourceDisplayName("Plugin.Misc.UpdateFromExcel.Message")]
        public string Message { get; set; }
    }
}