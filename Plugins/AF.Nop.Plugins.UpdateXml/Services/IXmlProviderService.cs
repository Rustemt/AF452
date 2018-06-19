using AF.Nop.Plugins.XmlUpdate.Domain;
using AF.Nop.Plugins.XmlUpdate.Models;
using Nop.Core.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AF.Nop.Plugins.XmlUpdate.Services
{
    public interface IXmlProviderService
    {
        XmlProvider GetProviderById(int id);

        void DeleteProvider(XmlProvider entity);

        void UpdateProvider(XmlProvider entity);

        string[] SupportedProperties { get; }

        IList<XmlProvider> GetAllProviders();

        IList<XmlProperty> GetProviderProperties(int providerId);

        void SaveXmlProperties(int providerId, IList<XmlProperty> properties);

        XmlProvider AddNewProvider(string name, string url, string xmlRoot, string xmlItem, bool enabled, int authType, string username, string password, bool autoResetStock, bool autoUnpublish, bool unpublishZeroStock);

        XmlProcessResult UpdateProductsFromXML(XmlProvider provider, Stream writer);
    }
}
