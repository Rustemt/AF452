using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Nop.Admin.Models.Customers;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.ExportImport;
using Nop.Services.Helpers;
using Nop.Services.Security;
using Nop.Services.Tax;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Telerik.Web.Mvc;

namespace Nop.Admin.Controllers
{
    [AdminAuthorize]
    public class CustomerProductQuoteController : BaseNopController
    {
        #region Fields
        private readonly IPermissionService _permissionService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly AdminAreaSettings _adminAreaSettings;
        private readonly ITaxService _taxService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IWorkContext _workContext;
        private readonly CurrencySettings _currencySettings;
        #endregion
        
        #region Constructors
        public CustomerProductQuoteController(ICustomerService customerService, IDateTimeHelper dateTimeHelper, IProductService productService,
            AdminAreaSettings adminAreaSettings,IWorkContext workContext,IPriceCalculationService priceCalculationService, ITaxService taxService, ICurrencyService currencyService, IPriceFormatter priceFormatter
            ,CurrencySettings currencySettings )
        {
            this._customerService = customerService;
            this._productService = productService;
            this._dateTimeHelper = dateTimeHelper;
            this._adminAreaSettings = adminAreaSettings;
            this._taxService = taxService;
            this._currencyService = currencyService;
            this._priceFormatter = priceFormatter;
            this._priceCalculationService = priceCalculationService;
            this._workContext = workContext;
            this._currencySettings = currencySettings;

        }
        #endregion 

        #region Utitilies

        //public ActionResult ExportExcel()
        //{

        //    try
        //    {
        //        var pvqs = _customerService.GetAllProductVariantQuotes(null, null, null,
        //        null, null, 0, _adminAreaSettings.GridPageSize);
        //        var listModel = new ProductVariantQuoteListModel();
        //        listModel.ProductVariantQuotes = new GridModel<ProductVariantQuoteModel>
        //        {
        //            Data = pvqs.Select(x => PrepareProductVariantQuoteModelForList(x)),
        //            Total = pvqs.TotalCount
        //        };
        //        string fileName = string.Format("quotes{0}_{1}.xlsx", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), CommonHelper.GenerateRandomDigitCode(4));
        //        string filePath = string.Format("{0}content\\files\\ExportImport\\{1}", Request.PhysicalApplicationPath, fileName);

        //        _exportManager.ExportQuotesToXlsx(filePath, listModel);

        //        var bytes = System.IO.File.ReadAllBytes(filePath);
        //        return File(bytes, "text/xls", fileName);
        //    }
        //    catch (Exception exc)
        //    {
        //        ErrorNotification(exc);
        //        return RedirectToAction("List");
        //    }
        //}

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

       
     
        public ActionResult List()
        {
           
            var listModel = new ProductVariantQuoteListModel();

            var pvqs = _customerService.GetAllProductVariantQuotes(null, null, null,
                null,null,0,_adminAreaSettings.GridPageSize);
            //pvq list
            listModel.ProductVariantQuotes = new GridModel<ProductVariantQuoteModel>
            {
                Data = pvqs.Select(x => PrepareProductVariantQuoteModelForList(x)),
                Total = pvqs.TotalCount
            };
            return View(listModel);
        }
       
        [GridAction(EnableCustomBinding = true)]
        public ActionResult QuoteList(GridCommand command,ProductVariantQuoteListModel model)
        {


            var pvqs = _customerService.GetAllProductVariantQuotes(model.SearchRequestDateFrom, model.SearchRequestDateTo, model.SearchEmail,
                model.SearchDescription,model.SearchSku, command.Page - 1, _adminAreaSettings.GridPageSize);
            var gridModel = new GridModel<ProductVariantQuoteModel>
            {
                Data = pvqs.Select(x => PrepareProductVariantQuoteModelForList(x)),
                Total = pvqs.TotalCount
            };
            return new JsonResult
            {
                Data = gridModel
            };
        }

        


        [NonAction]
        private ProductVariantQuoteModel PrepareProductVariantQuoteModelForList(CustomerProductVariantQuote customerProductVariantQuote)
        {
             #region Product variant price
                var productVariant = _productService.GetProductVariantById(customerProductVariantQuote.ProductVariantId);
                if (productVariant == null || productVariant.Id == 0)
                    return null;
                string productName = productVariant.FullProductName;
                Currency currency = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId);
                if (productVariant != null && productVariant.CurrencyId.HasValue && (productVariant.CurrencyId.Value != _currencySettings.PrimaryStoreCurrencyId))
                {
                    currency = _currencyService.GetCurrencyById(productVariant.CurrencyId.Value);
                }

                StatefulStorage.PerSession.Add<bool?>("SkipQuoteDiscountActivationCheck", () => (bool?)true);
                decimal taxRate = decimal.Zero; 
                var customer = _customerService.GetCustomerById(customerProductVariantQuote.CustomerId);
                decimal oldPriceBase = _taxService.GetProductPrice(productVariant, productVariant.OldPrice, out taxRate);
                decimal finalPriceWithoutDiscountBase = _taxService.GetProductPrice(productVariant, _priceCalculationService.GetFinalPrice(productVariant, customer, false, false), out taxRate);
                decimal finalPriceWithDiscountBase = _taxService.GetProductPrice(productVariant, _priceCalculationService.GetFinalPrice(productVariant, customer, true, false), out taxRate);
                
                decimal oldPrice = _currencyService.ConvertFromPrimaryStoreCurrency(oldPriceBase, currency);
                decimal finalPriceWithoutDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithoutDiscountBase, currency);
                decimal finalPriceWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithDiscountBase, currency);

                string productVariantPriceOldPrice = String.Empty;
                if (finalPriceWithoutDiscountBase != oldPriceBase && oldPriceBase > decimal.Zero)
                    productVariantPriceOldPrice = _priceFormatter.FormatPrice(oldPrice, false, false);
                string productVariantPrice = _priceFormatter.FormatPrice(finalPriceWithoutDiscount, false, currency);
                string productVariantPricePriceWithDiscount = "", productVariantPriceDiscountPrice = "", productVariantDiscountPercentage = "", productVariantPriceCurrency = "";
                decimal productVariantPricediscountValueBase = 0, productVariantPriceDiscountValue = 0, productVariantPriceValue = 0, productVariantPricePriceWithDiscountValue = 0;
                
                if (finalPriceWithoutDiscountBase != finalPriceWithDiscountBase)
                {
                    IList<Discount> discounts = null;
                    productVariantPricePriceWithDiscount = _priceFormatter.FormatPrice(finalPriceWithDiscount, false, currency);
                    productVariantPricediscountValueBase = _priceCalculationService.GetDiscountAmount(productVariant, customer, 0, out discounts, false);
                    productVariantPriceDiscountValue = _currencyService.ConvertFromPrimaryStoreCurrency(productVariantPricediscountValueBase, _workContext.WorkingCurrency);
                    productVariantPriceDiscountPrice = _priceFormatter.FormatPrice(productVariantPricediscountValueBase, true, false);

                    if (_workContext.WorkingLanguage.DisplayOrder == 2)
                        productVariantDiscountPercentage = String.Format("({0}%)", ((int)discounts.First().DiscountPercentage).ToString());
                    else
                        productVariantDiscountPercentage = String.Format("(%{0})", ((int)discounts.First().DiscountPercentage).ToString());
                }
                productVariantPriceValue = finalPriceWithoutDiscount;
                productVariantPricePriceWithDiscountValue = finalPriceWithDiscount;
                productVariantPriceCurrency = CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol;
            //////

                StatefulStorage.PerSession.Remove<bool?>("SkipQuoteDiscountActivationCheck");

                var pvpModel = new ProductVariantQuoteModel.ProductVariantPriceModel()
                {
                    Price = productVariantPrice,
                    PriceWithDiscount = productVariantPricePriceWithDiscount,
                    DiscountValue = productVariantPriceDiscountValue,
                    DiscountPrice = productVariantPriceDiscountPrice,
                    DiscountPercentage = productVariantDiscountPercentage,
                    PriceValue = productVariantPriceValue,
                    PriceWithDiscountValue = productVariantPricePriceWithDiscountValue,
                    Currency = productVariantPriceCurrency,
                    OldPrice = productVariantPriceOldPrice,
                };
                        
                   
                    #endregion

                string name = customerProductVariantQuote.Name;
                string phoneNumber = customerProductVariantQuote.PhoneNumber;
                try
                {
                    Customer cust = _customerService.GetCustomerById(customerProductVariantQuote.CustomerId);
                if(cust != null)
                 {       if (!cust.IsGuest())
                    {
                        name = cust.GetFullName();
                            if(cust.Addresses.Count() > 0)
                        phoneNumber = cust.Addresses.FirstOrDefault().PhoneNumber;
                    }
                 }
                }
                catch (Exception)
                {
                }
            return new ProductVariantQuoteModel()
            {
                Id = customerProductVariantQuote.Id,
                ProductVariantId = customerProductVariantQuote.ProductVariantId,
                Email = customerProductVariantQuote.Email,
                PhoneNumber = phoneNumber,
                Enquiry = customerProductVariantQuote.Enquiry,
                Name = name,
                ManufacturerName = _productService.GetProductById(customerProductVariantQuote.ProductVariant.ProductId).GetDefaultManufacturer().Name,
                ProductName = productName,
                Sku = productVariant.Sku,
                ProductVariantPrice = pvpModel,
                Description = customerProductVariantQuote.Description,
                RequestDate =  _dateTimeHelper.ConvertToUserTime(customerProductVariantQuote.RequestDate, DateTimeKind.Utc),
                PriceWithDiscount=customerProductVariantQuote.PriceWithDiscount,
                PriceWithoutDiscount = customerProductVariantQuote.PriceWithoutDiscount,
                DiscountPercentage = customerProductVariantQuote.DiscountPercentage,
                ActivateDate = customerProductVariantQuote.ActivateDate,
            };
        }


        [GridAction(EnableCustomBinding = true)]
        public ActionResult QuoteUpdate(ProductVariantQuoteModel model, GridCommand command)
        {
            var quote = _customerService.GetCustomerProductVariantQuoteById(model.Id);
            if (quote == null)
                return Json(new { Result = false }, JsonRequestBehavior.AllowGet);

            quote.Description = model.Description;
            _customerService.UpdateCustomerProductVariantQuote(quote);
            return QuoteList(command, new ProductVariantQuoteListModel());
        }

        #endregion
    }
}
