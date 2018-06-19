using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Nop.Plugin.Payments.KuveytTurk.Models;
using Nop.Plugin.Payments.KuveytTurk.Validators;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Payments.KuveytTurk.Controllers
{
    public class PaymentKuveytTurkController : BaseNopPaymentController
    {
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly KuveytTurkPaymentSettings _KuveytTurkPaymentSettings;

        public PaymentKuveytTurkController(ISettingService settingService, 
            ILocalizationService localizationService, KuveytTurkPaymentSettings KuveytTurkPaymentSettings)
        {
            this._settingService = settingService;
            this._localizationService = localizationService;
            this._KuveytTurkPaymentSettings = KuveytTurkPaymentSettings;
        }
        
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            var model = new ConfigurationModel();
            model.ClientId = _KuveytTurkPaymentSettings.ClientId;
            model.TestClientId = _KuveytTurkPaymentSettings.TestClientId;
            model.Name = _KuveytTurkPaymentSettings.Name;
            model.TestName = _KuveytTurkPaymentSettings.TestName;
            model.Password = _KuveytTurkPaymentSettings.Password;
            model.TestPassword = _KuveytTurkPaymentSettings.TestPassword;
            model.ServiceUrl = _KuveytTurkPaymentSettings.ServiceUrl;
            model.TestServiceUrl = _KuveytTurkPaymentSettings.TestServiceUrl;
            model.TestOrder = _KuveytTurkPaymentSettings.TestOrder;
            model.UseTestServer = _KuveytTurkPaymentSettings.UseTestServer;

            model.StoreKey      = _KuveytTurkPaymentSettings.StoreKey; 
            model.SuccessURL    = _KuveytTurkPaymentSettings.SuccessURL;
            model.ErrorURL      = _KuveytTurkPaymentSettings.ErrorURL;
            model.HostAddress3D = _KuveytTurkPaymentSettings.HostAddress3D;
            model.StoreType     = _KuveytTurkPaymentSettings.StoreType;
            model.StoreName     = _KuveytTurkPaymentSettings.StoreName;








            return View("Nop.Plugin.Payments.KuveytTurk.Views.PaymentKuveytTurk.Configure", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();
            
            //save settings
            _KuveytTurkPaymentSettings.ClientId = model.ClientId;
            _KuveytTurkPaymentSettings.TestClientId = model.TestClientId;
            _KuveytTurkPaymentSettings.Name = model.Name;
            _KuveytTurkPaymentSettings.TestName = model.TestName;
            _KuveytTurkPaymentSettings.Password = model.Password;
            _KuveytTurkPaymentSettings.TestPassword = model.TestPassword;
            _KuveytTurkPaymentSettings.ServiceUrl = model.ServiceUrl;
            _KuveytTurkPaymentSettings.TestServiceUrl = model.TestServiceUrl;
            _KuveytTurkPaymentSettings.TestOrder = model.TestOrder;
            _KuveytTurkPaymentSettings.UseTestServer= model.UseTestServer;
            _KuveytTurkPaymentSettings.StoreKey = model.StoreKey;
            _KuveytTurkPaymentSettings.SuccessURL=model.SuccessURL;
            _KuveytTurkPaymentSettings.ErrorURL=model.ErrorURL;
            _KuveytTurkPaymentSettings.HostAddress3D=model.HostAddress3D;
            _KuveytTurkPaymentSettings.StoreType=model.StoreType;
            _KuveytTurkPaymentSettings.StoreName=model.StoreName;
          
            _settingService.SaveSetting(_KuveytTurkPaymentSettings);
            return View("Nop.Plugin.Payments.KuveytTurk.Views.PaymentKuveytTurk.Configure", model);
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

            return View("Nop.Plugin.Payments.KuveytTurk.Views.PaymentKuveytTurk.PaymentInfo", model);
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