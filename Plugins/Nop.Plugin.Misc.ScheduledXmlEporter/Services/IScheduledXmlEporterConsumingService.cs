using System;
using System.Linq;
using Nop.Core.Domain.Tasks;
using Nop.Core.Plugins;
//using Nop.Plugin.Misc.ScheduledXmlEporter.Data;
using Nop.Services.Localization;
using Nop.Services.Tasks;
using Nop.Services.Catalog;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using System.Collections.Generic;
using Nop.Services.ExportImport;
using Nop.Web.Framework.Mvc;
using System.Xml;

namespace Nop.Plugin.Misc.ScheduledXmlEporter.Services
{
    public interface IScheduledXmlEporterConsumingService
    {
        void XmlExportForN11(int CategoryId);

        void XmlExportForGG(int CategoryId);        
    }
}