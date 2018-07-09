using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Nop.Core.Domain.Tasks;
using Nop.Plugin.Misc.UpdateFromExcel.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Tasks;
using Nop.Web.Framework.Controllers;
using Nop.Core;
using Nop.Services.Catalog;
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using System.IO;
using Nop.Web.Framework.UI;
using OfficeOpenXml;
using Nop.Services.Directory;
using Nop.Core.Domain.Directory;

namespace Nop.Plugin.Misc.UpdateFromExcel.Controllers
{
    [AdminAuthorize]
    public class UpdateFromExcelController : Controller
    {
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IProductService _productService;
        private readonly IManufacturerService _manufacturerService;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;

        public UpdateFromExcelController(ISettingService settingService, IScheduleTaskService scheduleTaskService,              
            ILocalizationService localizationService,
            IWorkContext workContext
            , IProductService productService
            , IManufacturerService manufacturerService
            , ICurrencyService currencyService
            , CurrencySettings currencySettings)
        {
            this._settingService = settingService;
            this._scheduleTaskService = scheduleTaskService;
            this._localizationService = localizationService;
            this._workContext = workContext;
            this._productService = productService;
            this._manufacturerService = manufacturerService;
            this._currencyService = currencyService;
            this._currencySettings = currencySettings;
        }


        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            return View("Nop.Plugin.Misc.UpdateFromExcel.Views.UpdateFromExcel.Configure");
        }

        [AdminAuthorize]
        public ActionResult UpdateFromExcelPopup()
        {
            var model = new UpdateFromExcelModel();


            SelectListItem resource = new SelectListItem();
            resource.Text = this._localizationService.GetResource("Admin.Common.Select");
            resource.Value = "0";
            model.AvailableManufacturers.Add(resource);
            foreach (Manufacturer allManufacturer in this._manufacturerService.GetAllManufacturers(true))
            {
                //IList<SelectListItem> availableManufacturers1 = model.AvailableManufacturers;
                SelectListItem name = new SelectListItem();
                name.Text = allManufacturer.Name;
                name.Value = allManufacturer.Id.ToString();
                model.AvailableManufacturers.Add(name);
            }

            return View("Nop.Plugin.Misc.UpdateFromExcel.Views.UpdateFromExcel.UpdateFromExcelPopup", model);
        }

        [AdminAuthorize]
        [HttpPost]
        public ActionResult UpdateFromExcelPopup(UpdateFromExcelModel model)        
        {
            if (!ModelState.IsValid)
            {
                return View("Nop.Plugin.Misc.UpdateFromExcel.Views.UpdateFromExcel.UpdateFromExcelPopup", model);
            }

            model.Message = ""; 

            if (model.ManufacturerId == 0)
            {
                SelectListItem resource = new SelectListItem();
                resource.Text = this._localizationService.GetResource("Admin.Common.Select");
                resource.Value = "0";
                model.AvailableManufacturers.Add(resource);
                foreach (Manufacturer allManufacturer in this._manufacturerService.GetAllManufacturers(true))
                {
                    //IList<SelectListItem> availableManufacturers1 = model.AvailableManufacturers;
                    SelectListItem name = new SelectListItem();
                    name.Text = allManufacturer.Name;
                    name.Value = allManufacturer.Id.ToString();
                    model.AvailableManufacturers.Add(name);
                }
                model.Message = "Manufacturer seçilmedi!";
                return View("Nop.Plugin.Misc.UpdateFromExcel.Views.UpdateFromExcel.UpdateFromExcelPopup", model);
            }

            try
            {
                var file = Request.Files["importexcelfile"];
                if (file != null && file.ContentLength > 0)
                {
                    var mStream = new MemoryStream();

                    mStream = ImportProductsFromXlsx(file.InputStream, model.ManufacturerId);

                    try
                    {
                        byte[] bytes = null;
                        bytes = mStream.ToArray();

                        //_localizationService.GetResource("AF.Plugin.Misc.UpdateFromExcel.Updated");
                        return File(bytes, "text/xls", "updatedProducts.xlsx");
                    }
                    catch (Exception exc)
                    {
                        SelectListItem resource = new SelectListItem();
                        resource.Text = this._localizationService.GetResource("Admin.Common.Select");
                        resource.Value = "0";
                        model.AvailableManufacturers.Add(resource);
                        foreach (Manufacturer allManufacturer in this._manufacturerService.GetAllManufacturers(true))
                        {
                            //IList<SelectListItem> availableManufacturers1 = model.AvailableManufacturers;
                            SelectListItem name = new SelectListItem();
                            name.Text = allManufacturer.Name;
                            name.Value = allManufacturer.Id.ToString();
                            model.AvailableManufacturers.Add(name);
                        }
                        model.Message = exc.Message;
                        return View("Nop.Plugin.Misc.UpdateFromExcel.Views.UpdateFromExcel.UpdateFromExcelPopup", model);
                    }

                }
                else
                {
                    SelectListItem resource = new SelectListItem();
                    resource.Text = this._localizationService.GetResource("Admin.Common.Select");
                    resource.Value = "0";
                    model.AvailableManufacturers.Add(resource);
                    foreach (Manufacturer allManufacturer in this._manufacturerService.GetAllManufacturers(true))
                    {
                        //IList<SelectListItem> availableManufacturers1 = model.AvailableManufacturers;
                        SelectListItem name = new SelectListItem();
                        name.Text = allManufacturer.Name;
                        name.Value = allManufacturer.Id.ToString();
                        model.AvailableManufacturers.Add(name);
                    }
                    model.Message = _localizationService.GetResource("AF.Plugin.Misc.UpdateFromExcel.NotUpdated");
                    return View("Nop.Plugin.Misc.UpdateFromExcel.Views.UpdateFromExcel.UpdateFromExcelPopup", model);
                }
            }
            catch (Exception exc)
            {
                SelectListItem resource = new SelectListItem();
                resource.Text = this._localizationService.GetResource("Admin.Common.Select");
                resource.Value = "0";
                model.AvailableManufacturers.Add(resource);
                foreach (Manufacturer allManufacturer in this._manufacturerService.GetAllManufacturers(true))
                {
                    //IList<SelectListItem> availableManufacturers1 = model.AvailableManufacturers;
                    SelectListItem name = new SelectListItem();
                    name.Text = allManufacturer.Name;
                    name.Value = allManufacturer.Id.ToString();
                    model.AvailableManufacturers.Add(name);
                }
                model.Message = exc.Message;
                return View("Nop.Plugin.Misc.UpdateFromExcel.Views.UpdateFromExcel.UpdateFromExcelPopup", model);
            }

        }

#region tmp
        //[HttpPost, ActionName("Configure")]
        //[FormValueRequired("generate")]
        //public ActionResult GenerateFeed(UpdateFromExcelModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return Configure();
        //    }


        //    try
        //    {
        //        string fileName = string.Format("become_{0}_{1}.csv", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), CommonHelper.GenerateRandomDigitCode(4));
        //        string filePath = string.Format("{0}content\\files\\exportimport\\{1}", Request.PhysicalApplicationPath, fileName);
        //        using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
        //        {
        //            var feed = _promotionFeedService.LoadPromotionFeedBySystemName("PromotionFeed.Become");
        //            feed.GenerateFeed(fs);
        //        }

        //        string clickhereStr = string.Format("<a href=\"{0}content/files/exportimport/{1}\" target=\"_blank\">{2}</a>", _webHelper.GetStoreLocation(false), fileName, _localizationService.GetResource("Plugins.Feed.Become.ClickHere"));
        //        string result = string.Format(_localizationService.GetResource("Plugins.Feed.Become.SuccessResult"), clickhereStr);
        //        model.GenerateFeedResult = result;
        //    }
        //    catch (Exception exc)
        //    {
        //        model.GenerateFeedResult = exc.Message;
        //        _logger.Error(exc.Message, exc);
        //    }


        //    foreach (var c in _currencyService.GetAllCurrencies(false))
        //    {
        //        model.AvailableCurrencies.Add(new SelectListItem()
        //        {
        //            Text = c.Name,
        //            Value = c.Id.ToString()
        //        });
        //    }
        //    return View("Nop.Plugin.Feed.Become.Views.FeedBecome.Configure", model);
        //}
#endregion

        public virtual MemoryStream ImportProductsFromXlsx(Stream stream, int manufacturerId)
        {
            //var pList = _productService.SearchProducts(0,
            //                    manufacturerId, null, null, null, 0, string.Empty, false, 0, null,
            //                    ProductSortingEnum.Position, 0, int.MaxValue);

            var productManufacturers = _manufacturerService.GetProductManufacturersByManufacturerId(manufacturerId, true);
            var manProds = productManufacturers
                .Select(x =>
                {
                    return x.Product;

                    //var p = _productService.GetProductById(x.pro
                    //return new ManufacturerModel.ManufacturerProductModel()
                    //{
                    //    Id = x.Id,
                    //    ManufacturerId = x.ManufacturerId,
                    //    ProductId = x.ProductId,
                    //    ProductName = _productService.GetProductById(x.ProductId).Name,
                    //    IsFeaturedProduct = x.IsFeaturedProduct,
                    //    DisplayOrder1 = x.DisplayOrder
                    //};
                })
                .ToList();


            IList<int> islenenUrunler = new List<int>();
            IList<ProductVariant> islenmeyenUrunler = new List<ProductVariant>();
            //IList<String> kombinasyonStoguSifirlananUrunler = new List<string>();
            MemoryStream mStream = new MemoryStream();
            ExcelPackage excelPackage = new ExcelPackage(stream);
            try
            {
                ExcelWorksheet excelWorksheet = excelPackage.Workbook.Worksheets.FirstOrDefault<ExcelWorksheet>();
                if (excelWorksheet == null)
                {
                    throw new NopException("No worksheet found");
                }
                string[] columns = new string[] { "Ürün Adı", "Sku", "Stok", "Fiyat", "Para Birimi" };
                string[] strArrays1 = columns;
                int satir = 2;
                while (true)
                {
                    bool flag = true;
                    int sutun = 1;
                    while (sutun <= (int)strArrays1.Length)
                    {
                        if ((excelWorksheet.Cells[satir, sutun].Value == null ? true : string.IsNullOrEmpty(excelWorksheet.Cells[satir, sutun].Value.ToString())))
                        {
                            sutun++;
                        }
                        else
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag)
                    {
                        break;
                    }

                    string pUrunAd;
                    string pSku = "";
                    //string _pStok;
                    int pStok = -1;
                    //string _pFiyat;
                    decimal pFiyat;
                    string pParaBirimi;

                    try
                    {
                        pUrunAd = excelWorksheet.Cells[satir, this.GetColumnIndex(strArrays1, "Ürün Adı")].Value as string;
                        var _pSku = excelWorksheet.Cells[satir, this.GetColumnIndex(strArrays1, "Sku")].Value;
                        pSku = _pSku == null ? "" : Convert.ToString(_pSku);
                        var _pStok = excelWorksheet.Cells[satir, this.GetColumnIndex(strArrays1, "Stok")].Value;
                        pStok = _pStok == null ? -1 : Convert.ToInt32(_pStok);
                        var _pFiyat = excelWorksheet.Cells[satir, this.GetColumnIndex(strArrays1, "Fiyat")].Value;
                        pFiyat = _pFiyat == null ? -1 : Convert.ToDecimal(_pFiyat);
                        pParaBirimi = excelWorksheet.Cells[satir, this.GetColumnIndex(strArrays1, "Para Birimi")].Value as string;
                    }
                    catch {
                        excelWorksheet.Cells[satir, columns.Length + 1].Value = "HATALI DEGER VAR!";
                        satir++;
                        continue;
                    }                    

                    //var productBySku = pList.Select(x => x.ProductVariants.Where(y => y.Gtin == pSku));
                    var productBySku = new List<ProductVariant>();

                    manProds.ForEach(x =>
                    {
                        var _tmp = x.ProductVariants.Where(y => y.Sku == pSku);
                        if(_tmp != null && _tmp.Count() > 0)
                        {
                            productBySku.AddRange(_tmp);
                        }
                    });

                    bool guncellendi = false;
                    if (productBySku == null)
                    {
                        //TODO: Buraya olmayan ürünler için gerekli implemantasyon yapılmalı.
                        excelWorksheet.Cells[satir, columns.Length + 1].Value = "URETICIYE AIT SKU YOK";
                        satir++;
                        continue;
                    }
                    else if(productBySku.Count() == 0)
                    {
                        //TODO: Buraya olmayan ürünler için gerekli implemantasyon yapılmalı.
                        excelWorksheet.Cells[satir, columns.Length + 1].Value = "URETICIYE AIT SKU YOK";
                        satir++;
                        continue;
                    }
                    else if (productBySku.Count() > 1)
                    {
                        //TODO: Buraya olmayan ürünler için gerekli implemantasyon yapılmalı.
                        excelWorksheet.Cells[satir, columns.Length + 1].Value = "URETICIYE AIT BIRDEN FAZLA SKU VAR";
                        satir++;
                        continue;
                    }
                    else
                    {
                        // will be continue process
                    }

                    var pv = productBySku.FirstOrDefault<ProductVariant>();

                    islenenUrunler.Add(pv.Id);

                    if (pFiyat > 0)
                    {
                        Currency newCur = _currencyService.GetCurrencyByCode(pParaBirimi);

                        if (newCur == null)
                        {
                            excelWorksheet.Cells[satir, columns.Length + 2].Value = "PARA BIRIMI BULUNAMADI! (HICBIR GUNCELLEME YAPILMADI)";
                            satir++;
                            continue;
                        }
                        if (pv.CurrencyId.HasValue && (pv.CurrencyId.Value != newCur.Id))
                        {
                            // GELEN PARA BİRİMİ ÜRÜNÜN PARA BİRİMİ İLE AYNI DEĞİL İSE HİÇBİR GÜNCELLEME YAPMA UYARI VER
                            //TODO: Buraya olmayan ürünler için gerekli implemantasyon yapılmalı.
                            excelWorksheet.Cells[satir, columns.Length + 2].Value = "PARA BIRIMI EŞIT DEGIL! (HICBIR GUNCELLEME YAPILMADI)";
                            satir++;
                            continue;
                        }
                        else if (pv.CurrencyId.HasValue && (pv.CurrencyId.Value != _currencySettings.PrimaryStoreCurrencyId))
                        {
                            // GELEN PARA BİRİMİ MAĞAZANIN PARA BİRİMİ İLE AYNI DEĞİL İSE
                            if (pv.CurrencyPrice != pFiyat)
                            {
                                var cur = _currencyService.GetCurrencyById(pv.CurrencyId.Value);
                                pv.CurrencyPrice = pFiyat;
                                pv.Price = _currencyService.ConvertToPrimaryStoreCurrency(pv.Price, cur);
                                //productBySku.CurrencyOldPrice = productBySku.OldPrice;
                                //productBySku.OldPrice = _currencyService.ConvertToPrimaryStoreCurrency(productBySku.OldPrice, cur);
                                //productBySku.CurrencyProductCost = productBySku.ProductCost;
                                //productBySku.ProductCost = _currencyService.ConvertToPrimaryStoreCurrency(productBySku.ProductCost, cur);
                                excelWorksheet.Cells[satir, columns.Length + 3].Value = "FIYAT GUNCELLENDI";
                                pv.UpdatedOnUtc = DateTime.UtcNow;
                                guncellendi = true;
                            }
                        }
                        else
                        {
                            // GELEN PARA BİRİMİ MAĞAZANIN PARA BİRİMİ İLE AYNI İSE
                            if (pv.Price != pFiyat)
                            {
                                pv.Price = pFiyat;
                                excelWorksheet.Cells[satir, columns.Length + 3].Value = "FIYAT GUNCELLENDI";
                                pv.UpdatedOnUtc = DateTime.UtcNow;
                                guncellendi = true;
                            }
                        }
                    }
                    else 
                    {
                        excelWorksheet.Cells[satir, columns.Length + 3].Value = "FIYAT PASS GECILDI";
                    }

                    if (pStok != -1)
                    {
                        if (pv.StockQuantity != pStok)
                        {
                            pv.StockQuantity = pStok;
                            excelWorksheet.Cells[satir, columns.Length + 4].Value = "STOK GUNCELLENDI";
                            pv.UpdatedOnUtc = DateTime.UtcNow;
                            guncellendi = true;
                        }
                    }
                    else
                    {
                        excelWorksheet.Cells[satir, columns.Length + 4].Value = "STOK PASS GECILDI";
                    }


                    if (guncellendi)
                    {
                        this._productService.UpdateProductVariant(pv);
                    }
                    else
                    {
                        excelWorksheet.Cells[satir, columns.Length + 5].Value = "HICBIR DEIGISIKLIK YAPILMADI";
                    }

                    if (pv.Published)
                    {
                        if (pv.Product.Published)
                        {
                            excelWorksheet.Cells[satir, columns.Length + 6].Value = "VARYANT VE PRODUCT YAYINDA";
                        }
                        else {
                            excelWorksheet.Cells[satir, columns.Length + 6].Value = "VARYANT YAYINDA, PRODUCT YAYINDA DEĞİL";
                        }
                    }
                    else {
                        if (pv.Product.Published)
                        {
                            excelWorksheet.Cells[satir, columns.Length + 6].Value = "VARYANT YAYINDA DEĞİL, PRODUCT YAYINDA";
                        }
                        else
                        {
                            excelWorksheet.Cells[satir, columns.Length + 6].Value = "VARYANT VE PRODUCT YAYINDA DEĞİL";
                        }
                    }

                    satir++;
                }

                // URETICININ TUM URUNLERINI KONTROL EDEREK EXCEL DE OLMAYAN URUNLERIN LISTESINI EXCEL SONUNA EKLIYORUZ.

                //var manProds = _productService.SearchProducts(0, manufacturerId, null, null, null, 0, "", false, _workContext.WorkingLanguage.Id, null, Core.Domain.AFEntities.ProductSortingEnumAF.CreatedOn, 0, int.MaxValue, false);
                //IList<Product> manProds = new List<Product>();

                

                foreach (Product p in manProds)
                {
                    foreach (ProductVariant pv in p.ProductVariants)
                    { 
                        if(islenenUrunler.IndexOf(pv.Id) < 0)
                            islenmeyenUrunler.Add(pv);
                        
                        //if(islenenUrunler.Select(x => x == pv.Id).Count() == 0)
                        //{
                            
                        //}
                    }
                }

                satir = satir + 2;
                
                excelWorksheet.Cells[satir, 1].Value = "-SITEDE OLUP EXCEL LISTESINDE OLMAYAN URUNLERIN SKU LISTESI-";
                satir++;

                foreach (ProductVariant _pv in islenmeyenUrunler)
                {
                    excelWorksheet.Cells[satir, 1].Value = _pv.Sku;
                    if (_pv.Published)
                    {
                        if (_pv.Product.Published)
                        {
                            excelWorksheet.Cells[satir, 2].Value = "VARYANT VE PRODUCT YAYINDA";
                        }
                        else
                        {
                            excelWorksheet.Cells[satir, 2].Value = "VARYANT YAYINDA, PRODUCT YAYINDA DEĞİL";
                        }
                    }
                    else
                    {
                        if (_pv.Product.Published)
                        {
                            excelWorksheet.Cells[satir, 2].Value = "VARYANT YAYINDA DEĞİL, PRODUCT YAYINDA";
                        }
                        else
                        {
                            excelWorksheet.Cells[satir, 2].Value = "VARYANT VE PRODUCT YAYINDA DEĞİL";
                        }
                    }
                    satir++;
                }

                excelPackage.Save();
                excelPackage.Stream.Position = 0;
                excelPackage.Stream.CopyTo(mStream);
            }
            finally
            {
                if (excelPackage != null)
                {
                    //((IDisposable)excelPackage).Dispose();
                }
            }
            return mStream;
        }

        protected virtual int GetColumnIndex(string[] properties, string columnName)
        {
            int int32;
            if (properties == null)
            {
                throw new ArgumentNullException("properties");
            }
            if (columnName == null)
            {
                throw new ArgumentNullException("columnName");
            }
            int int321 = 0;
            while (true)
            {
                if (int321 >= (int)properties.Length)
                {
                    int32 = 0;
                    break;
                }
                else if (!properties[int321].Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                {
                    int321++;
                }
                else
                {
                    int32 = int321 + 1;
                    break;
                }
            }
            return int32;
        }


        /// <summary>
        /// Display success notification
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="persistForTheNextRequest">A value indicating whether a message should be persisted for the next request</param>
        protected virtual void SuccessNotification(string message, bool persistForTheNextRequest = true)
        {
            AddNotification(NotifyType.Success, message, persistForTheNextRequest);
        }
        /// <summary>
        /// Display error notification
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="persistForTheNextRequest">A value indicating whether a message should be persisted for the next request</param>
        protected virtual void ErrorNotification(string message, bool persistForTheNextRequest = true)
        {
            AddNotification(NotifyType.Error, message, persistForTheNextRequest);
        }
        /// <summary>
        /// Display error notification
        /// </summary>
        /// <param name="exception">Exception</param>
        /// <param name="persistForTheNextRequest">A value indicating whether a message should be persisted for the next request</param>
        /// <param name="logException">A value indicating whether exception should be logged</param>
        protected virtual void ErrorNotification(Exception exception, bool persistForTheNextRequest = true, bool logException = true)
        {
            if (logException)
                //LogException(exception);
                AddNotification(NotifyType.Error, exception.Message, persistForTheNextRequest);
        }
        /// <summary>
        /// Display notification
        /// </summary>
        /// <param name="type">Notification type</param>
        /// <param name="message">Message</param>
        /// <param name="persistForTheNextRequest">A value indicating whether a message should be persisted for the next request</param>
        protected virtual void AddNotification(NotifyType type, string message, bool persistForTheNextRequest)
        {
            string dataKey = string.Format("nop.notifications.{0}", type);
            if (persistForTheNextRequest)
            {
                if (TempData[dataKey] == null)
                    TempData[dataKey] = new List<string>();
                ((List<string>)TempData[dataKey]).Add(message);
            }
            else
            {
                if (ViewData[dataKey] == null)
                    ViewData[dataKey] = new List<string>();
                ((List<string>)ViewData[dataKey]).Add(message);
            }
        }
    
    }
}