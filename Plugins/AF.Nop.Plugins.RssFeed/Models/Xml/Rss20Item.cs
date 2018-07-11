using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace AF.Nop.Plugins.RssFeed.Models.Xml
{
    public class Rss20Item
    {
        [XmlElement("id", Namespace= RssFeedHelper.GoogleNamespce)]
        public int ProductId { get; set; }

        [XmlElement("title")]
        public string Title { get; set; }

        [XmlElement("link")]
        public string Link { get; set; }

        [XmlElement("description")]
        public string Description { get; set; }

        [XmlElement("price", Namespace= RssFeedHelper.GoogleNamespce)]
        public string Price { get; set; }

        [XmlElement("sale_price", Namespace= RssFeedHelper.GoogleNamespce)]
        public string SalePrice { get; set; }

        [XmlElement("product_type", Namespace = RssFeedHelper.GoogleNamespce)]
        public List<string> ProductType { get; set; } = new List<string>();

        [XmlElement("brand", Namespace= RssFeedHelper.GoogleNamespce)]
        public string Brand { get; set; }

        [XmlElement("mpn", Namespace = RssFeedHelper.GoogleNamespce)]
        public string MPN { get; set; }

        [XmlElement("gtin", Namespace = RssFeedHelper.GoogleNamespce)]
        public string Gtin { get; set; }

        [XmlElement("image_link", Namespace = RssFeedHelper.GoogleNamespce)]
        public string Image { get; set; }

        [XmlElement("condition", Namespace = RssFeedHelper.GoogleNamespce)]
        public string Condition { get; set; }

        [XmlElement("availability", Namespace = RssFeedHelper.GoogleNamespce)]
        public string Availability { get; set; }

        [XmlElement("expiration_date", Namespace = RssFeedHelper.GoogleNamespce)]
        public string ExpirationDate { get; set; }

        [XmlIgnore]
        public bool CallForPrice { get; set; }
    }
}
