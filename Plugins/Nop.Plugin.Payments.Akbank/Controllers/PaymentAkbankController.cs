using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Nop.Plugin.Payments.Akbank.Models;
using Nop.Plugin.Payments.Akbank.Validators;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Payments.Akbank.Controllers
{
    public class PaymentAkbankController : BaseNopPaymentController
    {
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly AkbankPaymentSettings _AkbankPaymentSettings;

        public PaymentAkbankController(ISettingService settingService, 
            ILocalizationService localizationService, AkbankPaymentSettings AkbankPaymentSettings)
        {
            this._settingService = settingService;
            this._localizationService = localizationService;
            this._AkbankPaymentSettings = AkbankPaymentSettings;
        }
        
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            var model = new ConfigurationModel();
            model.ClientId = _AkbankPaymentSettings.ClientId;
            model.TestClientId = _AkbankPaymentSettings.TestClientId;
            model.Name = _AkbankPaymentSettings.Name;
            model.TestName = _AkbankPaymentSettings.TestName;
            model.Password = _AkbankPaymentSettings.Password;
            model.TestPassword = _AkbankPaymentSettings.TestPassword;
            model.ServiceUrl = _AkbankPaymentSettings.ServiceUrl;
            model.TestServiceUrl = _AkbankPaymentSettings.TestServiceUrl;
            model.TestOrder = _AkbankPaymentSettings.TestOrder;
            model.UseTestServer = _AkbankPaymentSettings.UseTestServer;

            model._3dclientid = _AkbankPaymentSettings._3dclientid;
            model._3dfailUrl = _AkbankPaymentSettings._3dfailUrl;
            model._3dislemtipi = _AkbankPaymentSettings._3dislemtipi;
            model._3dokUrl = _AkbankPaymentSettings._3dokUrl;
            model._3dStoreKey = _AkbankPaymentSettings._3dStoreKey;
            model._3dstoretype = _AkbankPaymentSettings._3dstoretype;
            model._3durl = _AkbankPaymentSettings._3durl;

            return View("Nop.Plugin.Payments.Akbank.Views.PaymentAkbank.Configure", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();
            
            //save settings
            _AkbankPaymentSettings.ClientId = model.ClientId;
            _AkbankPaymentSettings.TestClientId = model.TestClientId;
            _AkbankPaymentSettings.Name = model.Name;
            _AkbankPaymentSettings.TestName = model.TestName;
            _AkbankPaymentSettings.Password = model.Password;
            _AkbankPaymentSettings.TestPassword = model.TestPassword;
            _AkbankPaymentSettings.ServiceUrl = model.ServiceUrl;
            _AkbankPaymentSettings.TestServiceUrl = model.TestServiceUrl;
            _AkbankPaymentSettings.TestOrder = model.TestOrder;
            _AkbankPaymentSettings.UseTestServer= model.UseTestServer;

            _AkbankPaymentSettings._3dclientid = model._3dclientid;
            _AkbankPaymentSettings._3dfailUrl = model._3dfailUrl;
            _AkbankPaymentSettings._3dislemtipi = model._3dislemtipi;
            _AkbankPaymentSettings._3dokUrl = model._3dokUrl;
            _AkbankPaymentSettings._3dStoreKey  = model._3dStoreKey;
            _AkbankPaymentSettings._3dstoretype = model._3dstoretype;
            _AkbankPaymentSettings._3durl = model._3durl;
            
            _settingService.SaveSetting(_AkbankPaymentSettings);


            

            return View("Nop.Plugin.Payments.Akbank.Views.PaymentAkbank.Configure", model);
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

            return View("Nop.Plugin.Payments.Akbank.Views.PaymentAkbank.PaymentInfo", model);
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
            int installmentCount = 0;
            int.TryParse(form["Installment"], out installmentCount);
            paymentInfo.Installment = installmentCount;
            return paymentInfo;
        }
    }
}