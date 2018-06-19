using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nop.Core.Domain.News;
using Nop.Services.AFServices;
using Nop.Core.Data;
using Nop.Core.Events;

namespace Nop.Services.News
{
    public partial class ExtraContentService : IExtraContentService
    {
        #region Fields

        private readonly IRepository<ExtraContent> _extraContentRepository;

        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor

        public ExtraContentService(IRepository<ExtraContent> extraContentRepository, IEventPublisher eventPublisher)
        {
            this._extraContentRepository = extraContentRepository;
            this._eventPublisher = eventPublisher;
        }

        #endregion

        public virtual void DeleteExtraContent(ExtraContent extraContent)
        {
            if (extraContent == null)
                throw new ArgumentNullException("extraContent");

            _extraContentRepository.Delete(extraContent);

            //event notification
            _eventPublisher.EntityDeleted(extraContent);
        }

        public int InsertExtraContent(ExtraContent extraContent)
        {
            if (extraContent == null)
                throw new ArgumentNullException("extraContent");

            _extraContentRepository.Insert(extraContent);

            //event notification
            _eventPublisher.EntityInserted(extraContent);
            return extraContent.Id;

        }

        public ExtraContent GetExtraContentById(int extraContentId)
        {
            var extraContent = _extraContentRepository.GetById(extraContentId);
            return extraContent;
        }

        public void UpdateExtraContent(ExtraContent extraContent)
        {
            if (extraContent == null)
                throw new ArgumentNullException("extraContent");

            _extraContentRepository.Update(extraContent);

        }
}
}
