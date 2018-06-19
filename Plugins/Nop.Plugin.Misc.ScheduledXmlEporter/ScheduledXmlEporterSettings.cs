using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.ScheduledXmlEporter
{
    public class ScheduledXmlEporterSettings : ISettings
    {
        /// <summary>
        /// Gets or sets the start hour.
        /// </summary>
        /// <value>
        /// The API key.
        /// </value>
        public virtual string ScheduleTime { get; set; }

        /// <summary>
        /// Gets or sets the last start day of date.
        /// </summary>
        /// <value>
        /// The API key.
        /// </value>
        public virtual string LastStartDate { get; set; }

        ///// <summary>
        ///// Gets or sets the API key.
        ///// </summary>
        ///// <value>
        ///// The API key.
        ///// </value>
        //public virtual string ApiKey { get; set; }

        ///// <summary>
        ///// Gets or sets the web hook key.
        ///// </summary>
        ///// <value>
        ///// The web hook key.
        ///// </value>
        //public virtual string WebHookKey { get; set; }

        ///// <summary>
        ///// Gets or sets the default list id.
        ///// </summary>
        ///// <value>
        ///// The default list id.
        ///// </value>
        //public virtual string DefaultListId { get; set; }
    }
}