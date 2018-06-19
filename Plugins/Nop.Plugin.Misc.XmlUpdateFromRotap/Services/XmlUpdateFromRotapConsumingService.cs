using System;
using System.Linq;
using Nop.Core.Domain.Tasks;
using Nop.Core.Plugins;
//using Nop.Plugin.Misc.XmlUpdateFromRotap.Data;
using Nop.Services.Localization;
using Nop.Services.Tasks;
using Nop.Services.Catalog;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using System.Collections.Generic;
using Nop.Services.ExportImport;
using Nop.Web.Framework.Mvc;
using System.Xml;
using System.Web.Hosting;
using Nop.Core.Infrastructure;
using Nop.Services.Configuration;
using System.IO;
using Nop.Plugin.Misc.XmlUpdateProducts.Models;
using Nop.Plugin.Misc.XmlUpdateProducts.Services;
using Nop.Services.Messages;
using Nop.Core.Domain.Messages;

namespace Nop.Plugin.Misc.XmlUpdateFromRotap.Services
{
    public class XmlUpdateFromRotapConsumingService : IXmlUpdateFromRotapConsumingService
    {
        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;
        private readonly ICategoryService _categoryService;
        private readonly IExportManager _exportManager;
        private readonly XmlUpdateFromRotapSettings _xmlUpdateFromRotapSettings;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly IExcelService _excelService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly IQueuedEmailService _queuedEmailService;

        public XmlUpdateFromRotapConsumingService(IProductService productService
            , IWorkContext workContext
            , ICategoryService categoryService
            , IExportManager exportManager
            , XmlUpdateFromRotapSettings xmlUpdateFromRotapSettings
            , ISettingService settingService
            , IWebHelper webHelper
            , IExcelService excelService
            , IQueuedEmailService queuedEmailService
            , IEmailAccountService emailAccountService
            , EmailAccountSettings emailAccountSettings)
        {
            _productService = productService;
            _workContext = workContext;
            _categoryService = categoryService;
            _exportManager = exportManager;
            _xmlUpdateFromRotapSettings = xmlUpdateFromRotapSettings;
            _settingService = settingService;
            _webHelper = webHelper;
            _excelService = excelService;
            _queuedEmailService = queuedEmailService;
            _emailAccountService = emailAccountService;
            _emailAccountSettings = emailAccountSettings;

        }

        /// <summary>
        /// Installs the sync task.
        /// </summary>
        public virtual void XmlUpdate()
        {
            //string xmlUrl = "C:\\Users\\fatih\\Downloads\\products.xml";
            string xmlUrl = _webHelper.MapPath("~/content/files/Rotap"); //_xmlUpdateProductsSettings.XmlFileUrl.Trim();
            string xmldeVarSitedeYok = "";
            string xmldeOlupSitedePublishDurumu = "";
            string sitedePublishXmldeOlmayan = "";
            string aciklamaNot = "";
            IList<ReportLine> reportLines = new List<ReportLine>();

            var directory = new DirectoryInfo(xmlUrl);
            var myFile = (from f in directory.GetFiles()
                          orderby f.LastWriteTime descending
                          select f).First();

            //// or...
            //var myFile = directory.GetFiles()
            //             .OrderByDescending(f => f.LastWriteTime)
            //             .First();

            XmlDocument doc = new XmlDocument();
            doc.Load(myFile.FullName);

            XmlNodeList xnList = doc.SelectNodes("/Products/Product");

            IList<myProduct> pList = new List<myProduct>();

            foreach (XmlNode xn in xnList)
            {
                myProduct p = new myProduct();
                p.Sku = xn["SKU"].InnerText;
                p.StockQuantity = Convert.ToInt32(xn["StockQuantity"].InnerText);
                p.Price = Convert.ToDecimal(xn["Price"].InnerText);

                pList.Add(p);
            }

            //var allProducts = _productService.GetAllProducts();
            //var allProducts = _productService.SearchProducts(productType: ProductType.SimpleProduct, showHidden: true);
            var allProducts = _productService.SearchProductVariants(0, int.MaxValue, true);

            bool updated = false;
            foreach (myProduct p in pList)
            {
                ReportLine rl = new ReportLine(); 
                updated = false;

                if (allProducts.Where(x => x.Gtin == p.Sku).Count() > 1)
                {
                    aciklamaNot = aciklamaNot + "AF e-ticaret veri tabanında tekrarlayan Gtin mevcut! -> " + p.Sku;
                    aciklamaNot = aciklamaNot + System.Environment.NewLine;
                    aciklamaNot = aciklamaNot + "<br />";
                }
                //var _p = _productService.GetProductBySku(p.Sku);
                var _p = allProducts.Where(x => x.Gtin == p.Sku).FirstOrDefault();

                if (_p == null)
                {
                    rl.SKU = p.Sku.ToString();
                    rl.Product = "Ürün Sitede Ekli Değil";

                    rl.StockQty = p.StockQuantity.ToString();
                    reportLines.Add(rl);
                    continue;
                }
                else
                {
                    rl.SKU = p.Sku.ToString();
                    rl.Product = "Ürün Sitede Ekli";
                }

                if (_p.Published || !_p.Deleted)
                {
                    rl.PublishV = "Varyant Yayında";
                }
                else
                {
                    rl.PublishV = _p.Deleted == true ? "Varyant Silinmiş" : "Varyant Yayında Değil";
                }

                if (_p.Product.Published || !_p.Product.Deleted)
                {
                    rl.PublishP = "Ürün Yayında";
                }
                else
                {
                    rl.PublishP = _p.Product.Deleted == true ? "Ürün Silinmiş" : "Ürün Yayında Değil";
                }

                if (_p.StockQuantity == p.StockQuantity)
                {
                    rl.Stock = "Değişiklik Yapılmadı";
                }
                else
                {
                    rl.Stock = "Stok Güncellendi";
                    _p.StockQuantity = p.StockQuantity;
                    updated = true;
                }
                rl.StockQty = p.StockQuantity.ToString();

                if (_xmlUpdateFromRotapSettings.EnablePriceUpdate)
                {
                    if (_p.Price == p.Price)
                    {
                        rl.Price = "Değişiklik Yapılmadı";
                    }
                    else
                    {
                        rl.Price = "Fiyat Güncellendi";
                        _p.CurrencyPrice = p.Price;
                        updated = true;
                    }
                }
                else
                {
                    rl.Price = "Fiyat Güncelleme Kapalı";
                }


                //_p.SpecialPrice_Original

                reportLines.Add(rl);
                //_productService.UpdateProduct(_p);
                if(updated)
                    _productService.UpdateProductVariant(_p);
            }

            //foreach (ProductVariant p in allProducts)
            //{
            //    if (p.Gtin == null)
            //    {
            //        //aciklamaNot = aciklamaNot + "Sitede Gtin bilgisi olmayan ürünId! -> " + p.Id;
            //        //aciklamaNot = aciklamaNot + System.Environment.NewLine;
            //        //aciklamaNot = aciklamaNot + "<br />";
            //        ReportLine rl = new ReportLine();
            //        rl.SKU = "MPN bilgisi olmayan ürünID";
            //        rl.Product = p.Id.ToString();
            //        reportLines.Add(rl);
            //        continue;
            //    }
            //    if (pList.Where(x => x.Sku == p.Gtin).Count() > 1)
            //    {
            //        //aciklamaNot = aciklamaNot + "Tedarikçi XML dosyası içerisinde tekrarlayan SKU mevcut! -> " + p.Gtin;
            //        //aciklamaNot = aciklamaNot + System.Environment.NewLine;
            //        //aciklamaNot = aciklamaNot + "<br />";  
            //        ReportLine rl = new ReportLine();
            //        rl.SKU = "Tedarikçi XML dosyası içerisinde tekrarlayan SKU";
            //        rl.Product = p.Gtin.ToString();
            //        reportLines.Add(rl);
            //    }
            //    var _p = pList.Where(x => x.Sku == p.Gtin).FirstOrDefault();
            //    if (_p == null)
            //    {
            //        ReportLine rl = new ReportLine();
            //        rl.SKU = p.Gtin.ToString();
            //        rl.Product = "Ürün Sitede Var XML de Yok";
            //        reportLines.Add(rl);
            //    }
            //}

            byte[] bytes = null;
            using (var stream = new MemoryStream())
            {
                _excelService.BuildExcelFile(stream, reportLines);
                bytes = stream.ToArray();
            }
            string fileName = string.Format("Rotap_Xml_Report_{0}.xlsx", DateTime.Now.ToString("ddMMyyyyHHmm"));
            string filePath = Path.Combine(_webHelper.MapPath("~/content/files/ExportImport"), fileName);
            File.WriteAllBytes(filePath, bytes);
            //return File(bytes, "text/xls", "products.xlsx");

            var emailAccount = _emailAccountService.GetEmailAccountById(_emailAccountSettings.DefaultEmailAccountId);
            if (emailAccount == null)
                emailAccount = _emailAccountService.GetAllEmailAccounts().FirstOrDefault();
            if (emailAccount == null)
                throw new Exception("No email account could be loaded");

            QueuedEmail qe = new QueuedEmail();

            //qe.AttachmentFileName = fileName;
            //qe.AttachmentFilePath = filePath;
            qe.EmailAccount = emailAccount;
            qe.EmailAccountId = emailAccount.Id;
            qe.Body = "AF ROTAP XML ÜRÜN GÜNCELLEME RAPORU" + System.Environment.NewLine;
            qe.Body += "<br />";
            string excelLink = "http://www.alwaysfashion.com/content/files/ExportImport/" + fileName.ToString();
            qe.Body += "Güncelleme raporu: <a href='" + excelLink.ToString() + "'>" + excelLink.ToString() + "</a>";
            qe.Body += "<br />";
            qe.Body += System.Environment.NewLine;
            qe.Body += "<br />";
            if (!string.IsNullOrEmpty(aciklamaNot) && !string.IsNullOrWhiteSpace(aciklamaNot))
            {
                qe.Body += "AÇIKLAMA :" + aciklamaNot + System.Environment.NewLine;
                qe.Body += "<br />";
            }
            //qe.Body += "Xml de Olan Sitede Olmayan Ürün Sku Listesi" + System.Environment.NewLine;
            //qe.Body += "<br />";
            //qe.Body += xmldeVarSitedeYok + System.Environment.NewLine;
            //qe.Body += System.Environment.NewLine;
            //qe.Body += "<br />";
            //qe.Body += "Xml de Olup Sitede Publish Durumu Listesi" + System.Environment.NewLine;
            //qe.Body += "<br />";
            //qe.Body += xmldeOlupSitedePublishDurumu + System.Environment.NewLine;
            //qe.Body += System.Environment.NewLine;
            //qe.Body += "<br />";
            //qe.Body += "Sitede Publish Olup Xml de Olmayanlar Listesi" + System.Environment.NewLine;
            //qe.Body += "<br />";
            //qe.Body += sitedePublishXmldeOlmayan + System.Environment.NewLine;
            //qe.Body += System.Environment.NewLine;
            //qe.Body += "<br />";

            qe.To = _xmlUpdateFromRotapSettings.EmailForReporting;
            qe.ToName = _xmlUpdateFromRotapSettings.NameForReporting;
            qe.CC = _xmlUpdateFromRotapSettings.EmailForReportingCC;
            qe.CreatedOnUtc = DateTime.UtcNow;
            qe.From = emailAccount.Email;
            qe.FromName = emailAccount.DisplayName;
            qe.Priority = 5;
            qe.Subject = string.Format("AF Rotap XML Stok Güncelleme Raporu - {0}", DateTime.Now.ToString("dd.MM.yyyy HH:mm")); //"ROTAP XML ÜRÜN GÜNCELLEME RAPORU";            

            _queuedEmailService.InsertQueuedEmail(qe);

            _xmlUpdateFromRotapSettings.LastStartDate = DateTime.Now.ToString();
            _settingService.SaveSetting<XmlUpdateFromRotapSettings>(_xmlUpdateFromRotapSettings);

            //XmlDocument xml = new XmlDocument();
            //xml.LoadXml(myXmlString); // suppose that myXmlString contains "<Names>...</Names>"

            //XmlNodeList xnList = xml.SelectNodes("/Names/Name");
            //foreach (XmlNode xn in xnList)
            //{
            //    string firstName = xn["FirstName"].InnerText;
            //    string lastName = xn["LastName"].InnerText;
            //    Console.WriteLine("Name: {0} {1}", firstName, lastName);
            //}





            //XmlNode node = doc.DocumentElement.SelectSingleNode("/book/title");

            //foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            //{
            //    string text = node.InnerText; //or loop through its children as well
            //}

            //string text = node.InnerText;

            //string attr = node.Attributes["theattributename"].InnerText
        }
    }
}