using System.Collections.Generic;
using System.IO;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Messages;
using System.Xml;

namespace Nop.Services.ExportImport
{
    /// <summary>
    /// Export manager interface
    /// </summary>
    public interface IExportManager
    {
        /// <summary>
        /// Export manufacturer list to xml
        /// </summary>
        /// <param name="manufacturers">Manufacturers</param>
        /// <returns>Result in XML format</returns>
        string ExportManufacturersToXml(IList<Manufacturer> manufacturers);

        /// <summary>
        /// Export category list to xml
        /// </summary>
        /// <returns>Result in XML format</returns>
        string ExportCategoriesToXml();

        /// <summary>
        /// Export product list to xml
        /// </summary>
        /// <param name="products">Products</param>
        /// <returns>Result in XML format</returns>
        string ExportProductsToXml(IList<Product> products);
        string ExportProductsToXmlForN11(IList<Product> products);
        string ExportProductsToXmlForGG(IList<Product> products);

        string RetargetingExportProductsToXml(IList<Product> products);

        /// <summary>
        /// Export products to XLSX
        /// </summary>
        /// <param name="filePath">File path to use</param>
        /// <param name="products">Products</param>
        void ExportProductsToXlsx(string filePath, IList<Product> products);
        void ExportProductsToXlsxWithDefaultLanguage(string filePath, IList<Product> products);
        void ExportProductsToXlsxForNebim(string filePath, IList<Product> products);

        /// <summary>
        /// Export order list to xml
        /// </summary>
        /// <param name="orders">Orders</param>
        /// <returns>Result in XML format</returns>
        string ExportOrdersToXml(IList<Order> orders);

        /// <summary>
        /// Export orders to XLSX
        /// </summary>
        /// <param name="filePath">File path to use</param>
        /// <param name="orders">Orders</param>
        void ExportOrdersToXlsx(string filePath, IList<Order> orders);
        
        /// <summary>
        /// Export customer list to XLSX
        /// </summary>
        /// <param name="filePath">File path to use</param>
        /// <param name="customers">Customers</param>
        void ExportCustomersToXlsx(string filePath, IList<Customer> customers);
        void ExportCustomersToXlsxForNebim(string filePath, IList<Customer> customers);

        

        void ExportAddressToXlsx(string filePath, IList<NewsLetterSubscription> customers);


        /// <summary>
        /// Export customer list to xml
        /// </summary>
        /// <param name="customers">Customers</param>
        /// <returns>Result in XML format</returns>
        string ExportCustomersToXml(IList<Customer> customers);
        
        /// <summary>
        /// Export language resources to xml
        /// </summary>
        /// <param name="language">Language</param>
        /// <returns>Result in XML format</returns>
        string ExportLanguageToXml(Language language);
    }
}
