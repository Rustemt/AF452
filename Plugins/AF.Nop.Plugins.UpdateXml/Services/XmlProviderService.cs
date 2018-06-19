using System;
using System.Linq;
using Nop.Core.Data;
using AF.Nop.Plugins.XmlUpdate.Domain;
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using System.Net;
using System.Xml;
using System.IO;
using Nop.Services.Catalog;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using System.Web;
using AF.Nop.Plugins.XmlUpdate.Models;
using Nop.Services.Logging;

namespace AF.Nop.Plugins.XmlUpdate.Services
{
    public class XmlProviderService : IXmlProviderService
    {
        public const int AUTH_BASIC = 1;

        #region Constructor / Fields
        private readonly IProductService _productService;
        private readonly IRepository<XmlProvider> _xmlProviderRepository;
        private readonly IRepository<XmlProperty> _xmlPropertyRepository;
        private readonly ILogger _logger;

        private readonly IRepository<ProductVariant> _productVariantRepository;
        private readonly IRepository<ProductManufacturer> _productManufacturerRepository;

        public XmlProviderService(
            IProductService productService,
            IRepository<XmlProvider> xmlProviderRepository,
            IRepository<XmlProperty> xmlPropertyRepository,
            IRepository<ProductVariant> productVariantRepository,
            IRepository<ProductManufacturer> productManufacturerRepository,
            ILogger logger
            )
        {
            _productService = productService;
            _xmlProviderRepository = xmlProviderRepository;
            _xmlPropertyRepository = xmlPropertyRepository;
            _productVariantRepository = productVariantRepository;
            _productManufacturerRepository = productManufacturerRepository;
            _logger = logger;
        }

        #endregion

        public string[] SupportedProperties
        {
            get
            {
                return new string[]
                {
                    "Sku", "Stock", "Manufacturer"
                };
            }
        }

        public XmlProvider GetProviderById(int id)
        {
            return _xmlProviderRepository.GetById(id);
        }

        public void UpdateProvider(XmlProvider entity)
        {
            _xmlProviderRepository.Update(entity);
        }

        public void DeleteProvider(XmlProvider entity)
        {
            _xmlProviderRepository.Delete(entity);
        }

        public IList<XmlProvider> GetAllProviders()
        {
            var q = from p in _xmlProviderRepository.Table
                    orderby p.Name
                    select p;
            return q.ToList();
        }

        public XmlProvider AddNewProvider(string name, string url, string xmlRoot, string xmlItem, bool enabled, int authType, string username, string password, bool autoResetStock, bool autoUnpublish, bool unpublishZeroStock)
        {
            XmlProvider record = new XmlProvider()
            {
                Enabled = enabled,
                Name = name,
                Url = url,
                XmlRootNode = xmlRoot,
                XmlItemNode = xmlItem,
                AuthType = authType,
                Username = username,
                Password = password,
                AutoResetStock = autoResetStock,
                AutoUnpublish = autoUnpublish,
                UnpublishZeroStock = unpublishZeroStock
            };
            _xmlProviderRepository.Insert(record);

            return record;
        }

        public IList<XmlProperty> GetProviderProperties(int providerId)
        {
            var query = _xmlPropertyRepository.Table.Where(x => x.ProviderId == providerId);
            return query.ToList();
        }

        public void SaveXmlProperties(int providerId, IList<XmlProperty> properties)
        {
            var query = _xmlPropertyRepository.Table.Where(x => x.ProviderId == providerId);
            foreach (var o in query.ToList())
                _xmlPropertyRepository.Delete(o);

            foreach (var prop in properties)
            {
                prop.ProviderId = providerId;
                _xmlPropertyRepository.Insert(prop);
            }
        }
        
        /// <summary>
        /// Import XML from provider and update products
        /// </summary>
        public XmlProcessResult UpdateProductsFromXML(XmlProvider provider, Stream writer)
        {
            Exception exception = null;

            var result = ImportAndProcessXml(provider);

            using (var xlPackage = new ExcelPackage(writer))
            {
                ExcelWorksheet worksheet = xlPackage.Workbook.Worksheets.Add("Products");
                try
                {
                    int col = -1, row = 0;
                    var properties = this.GetProviderProperties(provider.Id);
                    WorkSheet_AppendHeaderCell(worksheet, ++col, "Id");
                    WorkSheet_AppendHeaderCell(worksheet, ++col, "Manufacturer");
                    WorkSheet_AppendHeaderCell(worksheet, ++col, "Sku");
                    WorkSheet_AppendHeaderCell(worksheet, ++col, "Name");
                    WorkSheet_AppendHeaderCell(worksheet, ++col, "New");
                    WorkSheet_AppendHeaderCell(worksheet, ++col, "Published");
                    WorkSheet_AppendHeaderCell(worksheet, ++col, "Processed"); // The products that are not included in the xml file will take the value false;
                    foreach (var prop in properties)
                    {
                        if (prop.ProductProperty == "Sku") continue;
                        WorkSheet_AppendHeaderCell(worksheet, ++col, prop.ProductProperty);
                    }
                    
                    foreach (var rec in result.Records)
                    {
                        //rec.UpdateProduct();
                        row++;
                        col = -1;
                        WorkSheet_AppendDataCell(worksheet, row, ++col, rec.IsNewProduct ?"" : rec.ProductVariant.Id.ToString());
                        WorkSheet_AppendDataCell(worksheet, row, ++col, rec.ManufacturerName);
                        WorkSheet_AppendDataCell(worksheet, row, ++col, rec.Sku);
                        WorkSheet_AppendDataCell(worksheet, row, ++col, rec.ProductName);
                        WorkSheet_AppendDataCell(worksheet, row, ++col, rec.IsNewProduct ? "Y" : "n", rec.IsNewProduct ? Color.Red : Color.DarkGray);
                        WorkSheet_AppendDataCell(worksheet, row, ++col, rec.IsPublished ? "Y" : "n", rec.IsPublished ? /*Color.DodgerBlue*/Color.Red : Color.DarkGray);
                        WorkSheet_AppendDataCell(worksheet, row, ++col, rec.IsProcessed ? "Y" : "n", rec.IsProcessed ? Color.DarkGray:Color.Red);

                        foreach (var prop in properties)
                        {

                            if (prop.ProductProperty == "Sku" || !prop.Enabled)
                                continue;

                            WorkSheet_AppendDataCell(worksheet, row, ++col, rec.GetProperty(prop.ProductProperty, ""), Color.Purple);
                        }
                    }
                }
                catch (Exception ex)
                {
                    exception = ex;
                }

                worksheet.View.FreezePanes(2, 4);
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                worksheet.Cells[worksheet.Dimension.Address].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                for (var ii = 1; ii <= 4; ii++)
                    worksheet.Column(ii).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                xlPackage.Save();

                if (exception != null)
                    throw exception;
            }
            return result;
        }

        #region Helper Methods
        protected XmlProcessResult ImportAndProcessXml(XmlProvider provider)
        {
            var result = new XmlProcessResult();
            var allVariants = _productService.SearchProductVariants(0, int.MaxValue, true);
            if (provider == null)
                return result;

            int cnt = 0;
            using (WebClient client = new WebClient())
            {
                try
                {
                    var properties = this.GetProviderProperties(provider.Id);
                    if (provider.AuthType == AUTH_BASIC)
                        client.Credentials = new NetworkCredential(provider.Username, provider.Password);

                    XmlDocument xml = new XmlDocument();
                    string content = client.DownloadString(provider.Url);
                    xml.LoadXml(content);
                    //xml.Load(HttpContext.Current.Server.MapPath(@"~/Plugins/AF.Nop.Plugins.XmlUpdate/Test/" + provider.Name.ToLower()+".xml"));

                    XmlNodeList nodes = xml.GetElementsByTagName(provider.XmlItemNode);
                    foreach (XmlNode n in nodes)
                    {
                        ++cnt;//if (++cnt % 19 == 0) break;

                        string sku = null;
                        var currRec = new ProcessRecord();
                        foreach (var prop in properties)
                        {
                            try
                            {
                                XmlElement elem = n[prop.Name];
                                if (elem == null)
                                    throw new Exception("There is no node with the name '" + prop.Name + "'");

                                if (prop.ProductProperty != "Sku" && !prop.Enabled)
                                    continue;
                                else
                                    currRec.SetProperty(prop.ProductProperty, elem.FirstChild.Value);
                            }
                            catch (Exception ex)
                            {
                                string msg = string.Format("XmlUpdate: Error in processing proprrty '{0}' on Node #{1}", prop.Name, cnt);
                                _logger.Error(msg, ex);
                            }
                        }

                        sku = currRec.Sku;
                        if (string.IsNullOrEmpty(sku))
                            continue;

                        //currRec.ProductVariant = _productService.GetProductVariantBySku(sku);
                        currRec.ProductVariant = allVariants.Where(x=>(x.Sku == sku)).FirstOrDefault();
                        result.AddRecord(currRec);
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }


            #region Add Un Processed Variants

            var variants = result.VariantsIds;
            var manufacturers = result.ManufacturersIds;
            var focusProductsIds = _productManufacturerRepository.Table
                .Where(pm => manufacturers.Contains(pm.ManufacturerId))
                .Select(pm => pm.ProductId)
                .ToList()
            ;
            var unProcessedVariants = allVariants.Where(x => !variants.Contains(x.Id) && focusProductsIds.Contains(x.ProductId)).ToList();

            var prods = string.Join(",", unProcessedVariants.Select(x => x.ProductId));
            foreach (var v in unProcessedVariants)
                result.AddMissingProduct(v);

            #endregion

            // Save All Changes
            foreach (var r in result.Records)
                r.UpdateProduct(provider.UnpublishZeroStock, provider.AutoResetStock, provider.AutoUnpublish);

            _productVariantRepository.Update(new ProductVariant());

            return result;
        }

        protected IList<ProductVariant> GetUnprocessedVariants(IList<int> manufacturers, IList<int> processedVariants)
        {
            var query = _productVariantRepository.Table;

            return query.ToList();
        }

        protected void SetProductAttribute(ProductVariant product, string attr, string value, out object newValue, out object oldValue)
        {
            oldValue = newValue = null;

            if (String.IsNullOrEmpty(value))
                return;
            var propInfo = product.GetType().GetProperty(attr);
            switch (attr)
            {
                case "Price":
                    newValue = Convert.ToDecimal(value);
                    break;
                case "StockQuantity":
                    newValue = Convert.ToInt32(value);
                    break;
                default:
                    newValue = value;
                    break;
            }
            oldValue = propInfo.GetValue(product, null);
            if (propInfo == null)
                throw new Exception(String.Format("Sku #{0}. Unknown property '{1}'.", product.Sku, attr));
            propInfo.SetValue(product, newValue, null);
        }

        #endregion

        #region Add Cells to Excel

        protected void WorkSheet_AppendHeaderCell(ExcelWorksheet worksheet, int index, string value)
        {
            worksheet.Cells[1, index + 1].Value = value;
            worksheet.Cells[1, index + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[1, index + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(184, 204, 228));
            worksheet.Cells[1, index + 1].Style.Font.Bold = true;
        }

        protected void WorkSheet_AppendDataCell(ExcelWorksheet worksheet, int row, int col, object value)
        {
            worksheet.Cells[row + 1, col + 1].Value = value;
            if (row % 2 == 0)
            {
                worksheet.Cells[row + 1, col + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row + 1, col + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(242, 246, 255));
            }
        }
        protected void WorkSheet_AppendDataCell(ExcelWorksheet worksheet, int row, int col, object value, Color color)
        {
            WorkSheet_AppendDataCell(worksheet, row, col, value);
            worksheet.Cells[row + 1, col + 1].Style.Font.Color.SetColor(color);
        }

        #endregion
    }
}

