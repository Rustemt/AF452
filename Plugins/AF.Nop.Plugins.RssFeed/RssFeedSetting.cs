using Newtonsoft.Json;
using Nop.Core.Configuration;
using Nop.Core.Domain.Localization;
using Nop.Core.Infrastructure;
using Nop.Services.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AF.Nop.Plugins.RssFeed
{
    public class RssFeedSetting : ISettings
    {
        public RssFeedSetting()
        {
        }

        protected IList<RssFeedSettingLocale> _locales;
        
        public bool EnabledSchedule { get; set; }

        public int TaskId { get; set; }

        public string TaskName { get; set; }

        public DateTime TaskRunTime { get; set; }

        public int TaskCheckTime { get; set; } = 3600;

        public DateTime LastRunTime { get; set; }

        public int ImageSize { get; set; } = 0;

        public string Json { get; set; }

        public IList<RssFeedSettingLocale> Locales
        {
            get
            {
                if (_locales == null)
                    RebuildList();
                return _locales;
            }
        }

        public void RebuildJson()
        {
            try { this.Json = JsonConvert.SerializeObject(Locales.ToArray()); }
            catch (Exception ex) { }
        }
        public void RebuildList()
        {
            try { _locales = JsonConvert.DeserializeObject<IList<RssFeedSettingLocale>>(this.Json); }
            finally
            {
                if (_locales == null)
                    _locales = new List<RssFeedSettingLocale>();
            }
            
        }
    }

    public class RssFeedSettingLocale
    {
        public int LanguageId { get; set; }
        
        public string Title { get; set; }

        public string Link { get; set; }
        public string FileName { get; set; }

        public string Description { get; set; }
    }
}
