using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Nop.Plugin.Payments.YapiKredi.Models;
using Nop.Plugin.Payments.YapiKredi.Validators;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Payments.YapiKredi.Controllers
{
    public class PaymentYapiKrediController : BaseNopPaymentController
    {
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly YapiKrediPaymentSettings _YapiKrediPaymentSettings;

        public PaymentYapiKrediController(ISettingService settingService, 
            ILocalizationService localizationService, YapiKrediPaymentSettings YapiKrediPaymentSettings)
        {
            this._settingService = settingService;
            this._localizationService = localizationService;
            this._YapiKrediPaymentSettings = YapiKrediPaymentSettings;
        }
        
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            var model = new ConfigurationModel();
            model.MerchantId = _YapiKrediPaymentSettings.MerchantId;
            model.TestMerchantId = _YapiKrediPaymentSettings.TestMerchantId;
            model.TerminalId = _YapiKrediPaymentSettings.TerminalId;
            model.TestTerminalId = _YapiKrediPaymentSettings.TestTerminalId;
            model.Password = _YapiKrediPaymentSettings.Password;
            model.TestPassword = _YapiKrediPaymentSettings.TestPassword;
            model.ServiceUrl = _YapiKrediPaymentSettings.ServiceUrl;
            model.TestServiceUrl = _YapiKrediPaymentSettings.TestServiceUrl;
            model.TestOrder = _YapiKrediPaymentSettings.TestOrder;
            model.UseTestServer = _YapiKrediPaymentSettings.UseTestServer;
            model.HostAddress3D = _YapiKrediPaymentSettings.HostAddress3D;
            model.MerchantReturnURL = _YapiKrediPaymentSettings.MerchantReturnURL;
            model.VftCode = _YapiKrediPaymentSettings.VftCode;
            model.PosnetId = _YapiKrediPaymentSettings.PosnetId;
            model.EncKey = _YapiKrediPaymentSettings.EncKey;

            return View("Nop.Plugin.Payments.YapiKredi.Views.PaymentYapiKredi.Configure", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();
            
            //save settings
            _YapiKrediPaymentSettings.MerchantId = model.MerchantId;
            _YapiKrediPaymentSettings.TestMerchantId = model.TestMerchantId;
            _YapiKrediPaymentSettings.TerminalId = model.TerminalId;
            _YapiKrediPaymentSettings.TestTerminalId = model.TestTerminalId;
            _YapiKrediPaymentSettings.Password = model.Password;
            _YapiKrediPaymentSettings.TestPassword = model.TestPassword;
            _YapiKrediPaymentSettings.ServiceUrl = model.ServiceUrl;
            _YapiKrediPaymentSettings.TestServiceUrl = model.TestServiceUrl;
            _YapiKrediPaymentSettings.TestOrder = model.TestOrder;
            _YapiKrediPaymentSettings.UseTestServer = model.UseTestServer;

            _YapiKrediPaymentSettings.HostAddress3D = model.HostAddress3D;
            _YapiKrediPaymentSettings.MerchantReturnURL = model.MerchantReturnURL;
            _YapiKrediPaymentSettings.VftCode = model.VftCode;
            _YapiKrediPaymentSettings.PosnetId = model.PosnetId;
            _YapiKrediPaymentSettings.EncKey = model.EncKey;

            _settingService.SaveSetting(_YapiKrediPaymentSettings);
            return View("Nop.Plugin.Payments.YapiKredi.Views.PaymentYapiKredi.Configure", model);
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

            return View("Nop.Plugin.Payments.YapiKredi.Views.PaymentYapiKredi.PaymentInfo", model);
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
            paymentInfo.CCOption = form["CCOption"];
            int installmentCount = 0;
            int.TryParse(form["Installment"], out installmentCount);
            paymentInfo.Installment = installmentCount;

            return paymentInfo;
        }
    }
}