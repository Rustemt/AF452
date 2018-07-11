using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using AF.Nop.Plugins.RssFeed.Models.Xml;
using System.IO;
using System.Xml;

namespace AF.Nop.Plugins.RssFeed.Services
{
    public interface IRssFeedService
    {
        void GeneratRssFeedFiles(XmlWriter stream, IList<ProductVariant> products, int languageId);

        void GeneratRssFeedFiles(IList<ProductVariant> products);

        void GeneratRssFeedFiles();

        void SaveUpdateTime(DateTime time);

    }
}
