using Nop.Plugin.Misc.XmlUpdateProducts.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.XmlUpdateProducts.Services
{
    public  interface IExcelService
    {
        void BuildExcelFile(Stream stream, IList<ReportLine> reportLines);
    }
}
