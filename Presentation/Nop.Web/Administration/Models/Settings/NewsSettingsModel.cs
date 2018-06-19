using Nop.Web.Framework;

namespace Nop.Admin.Models.Settings
{
    public class NewsSettingsModel
    {
        [NopResourceDisplayName("Admin.Configuration.Settings.News.Enabled")]
        public bool Enabled { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.News.AllowNotRegisteredUsersToLeaveComments")]
        public bool AllowNotRegisteredUsersToLeaveComments { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.News.NotifyAboutNewNewsComments")]
        public bool NotifyAboutNewNewsComments { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.News.ShowNewsOnMainPage")]
        public bool ShowNewsOnMainPage { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.News.MainPageNewsCount")]
        public int MainPageNewsCount { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.News.NewsArchivePageSize")]
        public int NewsArchivePageSize { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.News.ShowHeaderRSSUrl")]
        public bool ShowHeaderRssUrl { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.News.NewsArchiveMonthSpan")]
        public int NewsArchiveMonthSpan { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.News.MainNewsCount")]
        public int MainNewsCount { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.News.MainPageContentsCount")]
        public int MainPageContentsCount { get; set; }
    
    }
}