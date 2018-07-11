using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace AF.Nop.Plugins.RssFeed.Models.Xml
{
    [XmlRoot("rss"/*, Namespace = RssFeedHelper.GoogleNamespce*/)]
    public class Rss20
    {
        public Rss20()
        {
            Channel = new Rss20Channel();
        }
        public Rss20(string title, string link, string description)
        {
            Channel = new Rss20Channel()
            {
                Link = link,
                Title = title,
                Description = description,
                Items = new List<Rss20Item>()
            };
        }

        [XmlAttribute("version")]
        public string Version { get; set; } = "2.0";

        [XmlElement("channel")]
        public Rss20Channel Channel { get; set; }

        public void AddItem(Rss20Item item)
        {
            Channel.Items.Add(item);
        }
    }
}
