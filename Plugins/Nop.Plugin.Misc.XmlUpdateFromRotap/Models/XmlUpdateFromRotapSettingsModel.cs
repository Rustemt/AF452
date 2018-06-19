using System.Collections.Generic;
using System.Web.Mvc;
using Nop.Web.Framework;

namespace Nop.Plugin.Misc.XmlUpdateFromRotap.Models
{
    public class XmlUpdateFromRotapSettingsModel
    {
        /// <summary>
        /// Gets or sets a value indicating whether [auto sync].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [auto sync]; otherwise, <c>false</c>.
        /// </value>
        [NopResourceDisplayName("Plugin.Misc.XmlUpdateFromRotap.AutoSync")]
        public bool AutoSync { get; set; }

        [NopResourceDisplayName("Plugin.Misc.XmlUpdateFromRotap.AutoSyncEachMinutes")]
        public int AutoSyncEachMinutes { get; set; }

        [NopResourceDisplayName("Plugin.Misc.XmlUpdateFromRotap.ScheduleTime")]
        public string ScheduleTime { get; set; }

        [NopResourceDisplayName("Plugin.Misc.XmlUpdateFromRotap.LastStartDate")]
        public string LastStartDate { get; set; }


        [NopResourceDisplayName("Plugin.Misc.XmlUpdateFromRotap.EmailForReporting")]
        public string EmailForReporting { get; set; }

        [NopResourceDisplayName("Plugin.Misc.XmlUpdateFromRotap.EmailForReportingCC")]
        public string EmailForReportingCC { get; set; }

        [NopResourceDisplayName("Plugin.Misc.XmlUpdateFromRotap.NameForReporting")]
        public string NameForReporting { get; set; }

        [NopResourceDisplayName("Plugin.Misc.XmlUpdateFromRotap.EnablePriceUpdate")]
        public bool EnablePriceUpdate { get; set; }

    }
}