using System.Collections.Generic;
using System.Web.Mvc;
using Nop.Web.Framework;

namespace Nop.Plugin.Misc.ScheduledXmlEporter.Models
{
    public class ScheduledXmlEporterSettingsModel
    {
        /// <summary>
        /// Gets or sets a value indicating whether [auto sync].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [auto sync]; otherwise, <c>false</c>.
        /// </value>
        [NopResourceDisplayName("Plugin.Misc.ScheduledXmlEporter.AutoSync")]
        public virtual bool AutoSync { get; set; }

        [NopResourceDisplayName("Plugin.Misc.ScheduledXmlEporter.AutoSyncEachMinutes")]
        public virtual int AutoSyncEachMinutes { get; set; }

        [NopResourceDisplayName("Plugin.Misc.ScheduledXmlEporter.ScheduleTime")]
        public virtual string ScheduleTime { get; set; }

        [NopResourceDisplayName("Plugin.Misc.ScheduledXmlEporter.LastStartDate")]
        public virtual string LastStartDate { get; set; }
    }
}