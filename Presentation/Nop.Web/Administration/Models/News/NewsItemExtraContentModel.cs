using Nop.Web.Framework.Mvc;


namespace Nop.Admin.Models.News
{
    public class NewsItemExtraContentModel : BaseNopModel
    {
        public int NewsItemId { get; set; }

        public  ExtraContentModel ExtraContent { get; set; }
    }
}