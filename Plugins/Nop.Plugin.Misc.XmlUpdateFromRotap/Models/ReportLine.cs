using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.XmlUpdateProducts.Models
{
    public class ReportLine
    {
        //SKU	Product	Stock	Publish
        public string SKU { get; set; }
        public string Product { get; set; }
        public string Stock { get; set; }
        public string PublishP { get; set; }
        public string PublishV { get; set; }
        public string Price { get; set; }
        public string StockQty { get; set; }
    }
}
