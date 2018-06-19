using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Nop.Plugin.Payments.IsBank.Models;
using Nop.Plugin.Payments.IsBank.Validators;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Payments.IsBank.Controllers
{
    public class PaymentIsBankController : BaseNopPaymentController
    {
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly IsBankPaymentSettings _IsBankPaymentSettings;

        public PaymentIsBankController(ISettingService settingService, 
            ILocalizationService localizationService, IsBankPaymentSettings IsBankPaymentSettings)
        {
            this._settingService = settingService;
            this._localizationService = localizationService;
            this._IsBankPaymentSettings = IsBankPaymentSettings;
        }
        
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            var model = new ConfigurationModel();
            model.ClientId = _IsBankPaymentSettings.ClientId;
            model.TestClientId = _IsBankPaymentSettings.TestClientId;
            model.Name = _IsBankPaymentSettings.Name;
            model.TestName = _IsBankPaymentSettings.TestName;
            model.Password = _IsBankPaymentSettings.Password;
            model.TestPassword = _IsBankPaymentSettings.TestPassword;
            model.ServiceUrl = _IsBankPaymentSettings.ServiceUrl;
            model.TestServiceUrl = _IsBankPaymentSettings.TestServiceUrl;
            model.TestOrder = _IsBankPaymentSettings.TestOrder;
            model.UseTestServer = _IsBankPaymentSettings.UseTestServer;

            model.StoreKey      = _IsBankPaymentSettings.StoreKey; 
            model.SuccessURL    = _IsBankPaymentSettings.SuccessURL;
            model.ErrorURL      = _IsBankPaymentSettings.ErrorURL;
            model.HostAddress3D = _IsBankPaymentSettings.HostAddress3D;
            model.StoreType     = _IsBankPaymentSettings.StoreType;
            model.StoreName     = _IsBankPaymentSettings.StoreName;








            return View("Nop.Plugin.Payments.IsBank.Views.PaymentIsBank.Configure", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();
            
            //save settings
            _IsBankPaymentSettings.ClientId = model.ClientId;
            _IsBankPaymentSettings.TestClientId = model.TestClientId;
            _IsBankPaymentSettings.Name = model.Name;
            _IsBankPaymentSettings.TestName = model.TestName;
            _IsBankPaymentSettings.Password = model.Password;
            _IsBankPaymentSettings.TestPassword = model.TestPassword;
            _IsBankPaymentSettings.ServiceUrl = model.ServiceUrl;
            _IsBankPaymentSettings.TestServiceUrl = model.TestServiceUrl;
            _IsBankPaymentSettings.TestOrder = model.TestOrder;
            _IsBankPaymentSettings.UseTestServer= model.UseTestServer;
            _IsBankPaymentSettings.StoreKey = model.StoreKey;
            _IsBankPaymentSettings.SuccessURL=model.SuccessURL;
            _IsBankPaymentSettings.ErrorURL=model.ErrorURL;
            _IsBankPaymentSettings.HostAddress3D=model.HostAddress3D;
            _IsBankPaymentSettings.StoreType=model.StoreType;
            _IsBankPaymentSettings.StoreName=model.StoreName;
          
            _settingService.SaveSetting(_IsBankPaymentSettings);
            return View("Nop.Plugin.Payments.IsBank.Views.PaymentIsBank.Configure", model);
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

            return View("Nop.Plugin.Payments.IsBank.Views.PaymentIsBank.PaymentInfo", model);
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