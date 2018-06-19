using System;
using System.Linq;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Messages;
using Nop.Core.Events;
using Nop.Data;

namespace Nop.Services.Messages
{

    public partial class NewsLetterSubscriptionService : INewsLetterSubscriptionService
    {
       
        public virtual IPagedList<NewsLetterSubscription> GetAllNewsLetterSubscriptions(string email,int languageId,
            int pageIndex, int pageSize, bool showHidden = false)
        {
            var query = _subscriptionRepository.Table;
            if (languageId!=0) query = query.Where(nls => nls.LanguageId.Equals(languageId));
            if (!String.IsNullOrEmpty(email)) query = query.Where(nls => nls.Email.Contains(email));
            query = query.OrderBy(nls => nls.Email);
            var newsletterSubscriptions = new PagedList<NewsLetterSubscription>(query, pageIndex, pageSize);
            return newsletterSubscriptions;
        }

    }
}