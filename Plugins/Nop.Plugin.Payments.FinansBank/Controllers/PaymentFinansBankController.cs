using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Nop.Plugin.Payments.FinansBank.Models;
using Nop.Plugin.Payments.FinansBank.Validators;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Payments.FinansBank.Controllers
{
    public class PaymentFinansBankController : BaseNopPaymentController
    {
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly FinansBankPaymentSettings _FinansBankPaymentSettings;

        public PaymentFinansBankController(ISettingService settingService, 
            ILocalizationService localizationService, FinansBankPaymentSettings FinansBankPaymentSettings)
        {
            this._settingService = settingService;
            this._localizationService = localizationService;
            this._FinansBankPaymentSettings = FinansBankPaymentSettings;
        }
        
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            var model = new ConfigurationModel();
            model.ClientId = _FinansBankPaymentSettings.ClientId;
            model.TestClientId = _FinansBankPaymentSettings.TestClientId;
            model.Name = _FinansBankPaymentSettings.Name;
            model.TestName = _FinansBankPaymentSettings.TestName;
            model.Password = _FinansBankPaymentSettings.Password;
            model.TestPassword = _FinansBankPaymentSettings.TestPassword;
            model.ServiceUrl = _FinansBankPaymentSettings.ServiceUrl;
            model.TestServiceUrl = _FinansBankPaymentSettings.TestServiceUrl;
            model.TestOrder = _FinansBankPaymentSettings.TestOrder;
            model.UseTestServer = _FinansBankPaymentSettings.UseTestServer;

            model.StoreKey      = _FinansBankPaymentSettings.StoreKey; 
            model.SuccessURL    = _FinansBankPaymentSettings.SuccessURL;
            model.ErrorURL      = _FinansBankPaymentSettings.ErrorURL;
            model.HostAddress3D = _FinansBankPaymentSettings.HostAddress3D;
            model.StoreType     = _FinansBankPaymentSettings.StoreType;
            model.StoreName     = _FinansBankPaymentSettings.StoreName;








            return View("Nop.Plugin.Payments.FinansBank.Views.PaymentFinansBank.Configure", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();
            
            //save settings
            _FinansBankPaymentSettings.ClientId = model.ClientId;
            _FinansBankPaymentSettings.TestClientId = model.TestClientId;
            _FinansBankPaymentSettings.Name = model.Name;
            _FinansBankPaymentSettings.TestName = model.TestName;
            _FinansBankPaymentSettings.Password = model.Password;
            _FinansBankPaymentSettings.TestPassword = model.TestPassword;
            _FinansBankPaymentSettings.ServiceUrl = model.ServiceUrl;
            _FinansBankPaymentSettings.TestServiceUrl = model.TestServiceUrl;
            _FinansBankPaymentSettings.TestOrder = model.TestOrder;
            _FinansBankPaymentSettings.UseTestServer= model.UseTestServer;
            _FinansBankPaymentSettings.StoreKey = model.StoreKey;
            _FinansBankPaymentSettings.SuccessURL=model.SuccessURL;
            _FinansBankPaymentSettings.ErrorURL=model.ErrorURL;
            _FinansBankPaymentSettings.HostAddress3D=model.HostAddress3D;
            _FinansBankPaymentSettings.StoreType=model.StoreType;
            _FinansBankPaymentSettings.StoreName=model.StoreName;
          
            _settingService.SaveSetting(_FinansBankPaymentSettings);
            return View("Nop.Plugin.Payments.FinansBank.Views.PaymentFinansBank.Configure", model);
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
            var model = new PaymentInfoModel();
            
            //CC types
            model.CreditCardTypes.Add(new SelectListItem()
                {
                    Text = "Visa",
                    Value = "Visa",
                });
            model.CreditCardTypes.Add(new SelectListItem()
            {
                Text = "Master card",
                Value = "MasterCard",
            });
            model.CreditCardTypes.Add(new SelectListItem()
            {
                Text = "Discover",
                Value = "Discover",
            });
            model.CreditCardTypes.Add(new SelectListItem()
            {
                Text = "Amex",
                Value = "Amex",
            });
            
            //years
            for (int i = 0; i < 15; i++)
            {
                string year = Convert.ToString(DateTime.Now.Year + i);
                model.ExpireYears.Add(new SelectListItem()
                {
                    Text = year,
                    Value = year,
                });
            }

            //months
            for (int i = 1; i <= 12; i++)
            {
                string text = (i < 10) ? "0" + i.ToString() : i.ToString();
                model.ExpireMonths.Add(new SelectListItem()
                {
                    Text = text,
                    Value = i.ToString(),
                });
            }

            //set postback values
            var form = this.Request.Form;
            model.CardholderName = form["CardholderName"];
            model.CardNumber = form["CardNumber"];
            model.CardCode = form["CardCode"];
            var selectedCcType = model.CreditCardTypes.Where(x => x.Value.Equals(form["CreditCardType"], StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (selectedCcType != null)
                selectedCcType.Selected = true;
            var selectedMonth = model.ExpireMonths.Where(x => x.Value.Equals(form["ExpireMonth"], StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (selectedMonth != null)
                selectedMonth.Selected = true;
            var selectedYear = model.ExpireYears.Where(x => x.Value.Equals(form["ExpireYear"], StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (selectedYear != null)
                selectedYear.Selected = true;

            return View("Nop.Plugin.Payments.FinansBank.Views.PaymentFinansBank.PaymentInfo", model);
        }

        [NonAction]
        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            var warnings = new List<string>();

            //validate
            var validator = new PaymentInfoValidator(_localizationService);
            var model = new PaymentInfoModel()
            {
                CardholderName = form["CardholderName"],
                CardNumber = form["CardNumber"],
                CardCode = form["CardCode"],
            };
            var validationResult = validator.Validate(model);
            if (!validationResult.IsValid)
                foreach (var error in validationResult.Errors)
                    warnings.Add(error.ErrorMessage);
            return warnings;
        }

        [NonAction]
        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest();
            paymentInfo.CreditCardType = form["ccTypes"];
            paymentInfo.CreditCardName = form["CardholderName"];
            paymentInfo.CreditCardNumber = form["CardNumber"];
            paymentInfo.CreditCardExpireMonth = int.Parse(form["ccMonth"]);
            paymentInfo.CreditCardExpireYear = int.Parse(form["ccYear"]);
            paymentInfo.CreditCardCvv2 = form["CardCode"];
            return paymentInfo;
        }
    }
}