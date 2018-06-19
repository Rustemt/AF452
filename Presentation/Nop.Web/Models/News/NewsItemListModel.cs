using System.Collections.Generic;
using Nop.Web.Framework.Mvc;
using System.Linq;

namespace Nop.Web.Models.News
{
    public class NewsItemListModel : BaseNopModel
    {
        public NewsItemListModel()
        {
            PagingFilteringContext = new NewsPagingFilteringModel();
            MainNewsItems = new List<NewsItemModel>();
            MainNewsCount = 4;
        }

        public int WorkingLanguageId { get; set; }
        public NewsPagingFilteringModel PagingFilteringContext { get; set; }
        public IList<NewsItemModel> MainNewsItems { get; set; }
        public IList<IGrouping <string,NewsItemModel>> MonthlyNewsItems { get; set; }
        public int MainNewsCount { get; set; }
        public bool IsGuest { get; set; }
    }
}