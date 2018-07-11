using System;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Nop.Web.Framework.Localization;
using System.IO;

namespace AF.Nop.Plugins.RssFeed.Models
{
    public class ConfigurationModel : BaseNopModel, ILocalizedModel<RssFeedLocalizedModel>
    {
        public ConfigurationModel()
        {
            Locales = new List<RssFeedLocalizedModel>();
        }

        [NopResourceDisplayName("AF.RssFeed.EnabledSchedule")]
        public bool EnabledSchedule { get; set; }

        [NopResourceDisplayName("AF.RssFeed.TaskId")]
        public int TaskId { get; set; }

        [NopResourceDisplayName("AF.RssFeed.TaskName")]
        public string TaskName { get; set; }

        [DataType(DataType.Time)]
        [NopResourceDisplayName("AF.RssFeed.TaskRunTime")]
        public DateTime TaskRunTime { get; set; }

        [NopResourceDisplayName("AF.RssFeed.TaskCheckTime")]
        public int TaskCheckTime { get; set; }

        [NopResourceDisplayName("AF.RssFeed.LastRunTime")]
        public DateTime LastRunTime { get; set; }

        public IList<RssFeedLocalizedModel> Locales { get; set; }

        public FileInfo[] Files { get; set; }
    }

    public class RssFeedLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [Required]
        [NopResourceDisplayName("AF.RssFeed.Title")]
        public string Title { get; set; }

        [Required]
        [NopResourceDisplayName("AF.RssFeed.Link")]
        public string Link { get; set; }

        [Required]
        [NopResourceDisplayName("AF.RssFeed.FileName")]
        public string FileName { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        [NopResourceDisplayName("AF.RssFeed.Description")]
        public string Description { get; set; }
    }
}
