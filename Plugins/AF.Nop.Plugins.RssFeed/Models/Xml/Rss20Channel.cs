using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace AF.Nop.Plugins.RssFeed.Models.Xml
{
    public class Rss20Channel
    {
        [XmlElement("title", Order =0)]
        public string Title { get; set; }

        [XmlElement("link", Order = 1)]
        public string Link { get; set; }

        [XmlElement("description", Order = 2)]
        public string Description { get; set; }

        [XmlElement("item", Order = 10)]
        public List<Rss20Item> Items;
    }
}
