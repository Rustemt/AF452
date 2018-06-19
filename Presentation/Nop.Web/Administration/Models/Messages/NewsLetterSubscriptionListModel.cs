using System.Collections.Generic;
using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using Telerik.Web.Mvc;

namespace Nop.Admin.Models.Messages
{
    public class NewsLetterSubscriptionListModel : BaseNopModel
    {
        public NewsLetterSubscriptionListModel()
            {
                AvailableLanguageNames = new List<SelectListItem>();
            }

        public GridModel<NewsLetterSubscriptionModel> NewsLetterSubscriptions { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.List.SearchEmail")]
        public string SearchEmail { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.List.LanguageName")]
        public int SearchLanguageId { get; set; }


        public IList<SelectListItem> AvailableLanguageNames { get; set; }
    }
}