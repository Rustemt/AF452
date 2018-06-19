using System;
using Nop.Core;
using Nop.Core.Domain.Messages;

namespace Nop.Services.Messages
{
    public partial interface INewsLetterSubscriptionService
    {
        IPagedList<NewsLetterSubscription> GetAllNewsLetterSubscriptions(string email,int languageId,
            int pageIndex, int pageSize, bool showHidden = false);
    }
}
