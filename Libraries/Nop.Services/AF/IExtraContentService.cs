using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nop.Core.Domain.News;

namespace Nop.Services.News
{
    public partial interface IExtraContentService
    {
        int InsertExtraContent(ExtraContent extraContent);

        ExtraContent GetExtraContentById(int extraContentId);

        void UpdateExtraContent(ExtraContent extraContent);

        void DeleteExtraContent(ExtraContent extraContent);
    }
}
