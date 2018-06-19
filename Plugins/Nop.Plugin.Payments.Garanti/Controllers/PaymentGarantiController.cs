using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Nop.Plugin.Payments.Garanti.Models;
using Nop.Plugin.Payments.Garanti.Validators;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Payments.Garanti.Controllers
{
    public class PaymentGarantiController : BaseNopPaymentController
    {
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly GarantiPaymentSettings _GarantiPaymentSettings;

        public PaymentGarantiController(ISettingService settingService, 
            ILocalizationService localizationService, GarantiPaymentSettings GarantiPaymentSettings)
        {
            this._settingService = settingService;
            this._localizationService = localizationService;
            this._GarantiPaymentSettings = GarantiPaymentSettings;
        }
        
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            var model = new ConfigurationModel();
            model.HostAddress = _GarantiPaymentSettings.HostAddress;
            model.MerchantId = _GarantiPaymentSettings.MerchantId;
            model.Mode = _GarantiPaymentSettings.Mode;
            model.ProvisionPassword = _GarantiPaymentSettings.ProvisionPassword;
            model.ProvisionUserId = _GarantiPaymentSettings.ProvisionUserId;
            model.TerminalId = _GarantiPaymentSettings.TerminalId;
            model.MotoInd = _GarantiPaymentSettings.MotoInd;
            model.CardholderPresentCode = _GarantiPaymentSettings.CardholderPresentCode;
            model.StoreKey = _GarantiPaymentSettings.StoreKey;
            model.SuccessURL = _GarantiPaymentSettings.SuccessURL;
            model.ErrorURL = _GarantiPaymentSettings.ErrorURL;
            model.HostAddress3D = _GarantiPaymentSettings.HostAddress3D;
            model.SecurityLevel3D = _GarantiPaymentSettings.SecurityLevel3D;
            model.Type = _GarantiPaymentSettings.Type;
            model.UserId = _GarantiPaymentSettings.UserId;
            model.Version = _GarantiPaymentSettings.Version;;
 
            model.TestOrder = _GarantiPaymentSettings.TestOrder;
            
            model.TestHostAddress = _GarantiPaymentSettings.TestHostAddress;
            model.TestMerchantId = _GarantiPaymentSettings.TestMerchantId;
            model.TestMode = _GarantiPaymentSettings.TestMode; 
            model.TestProvisionPassword = _GarantiPaymentSettings.TestProvisionPassword;
            model.TestProvisionUserId = _GarantiPaymentSettings.TestProvisionUserId;
            model.TestTerminalId = _GarantiPaymentSettings.TestTerminalId;
            model.TestMotoInd = _GarantiPaymentSettings.TestMotoInd;
            model.TestCardholderPresentCode = _GarantiPaymentSettings.TestCardholderPresentCode;
            model.TestStoreKey = _GarantiPaymentSettings.TestStoreKey;
            model.TestSuccessURL = _GarantiPaymentSettings.TestSuccessURL;
            model.TestErrorURL = _GarantiPaymentSettings.TestErrorURL;
            model.TestHostAddress3D = _GarantiPaymentSettings.TestHostAddress3D;
            model.TestSecurityLevel3D = _GarantiPaymentSettings.TestSecurityLevel3D;
            model.TestType = _GarantiPaymentSettings.TestType;
            model.TestUserId = _GarantiPaymentSettings.TestUserId;
            model.TestVersion = _GarantiPaymentSettings.TestVersion;
           

            return View("Nop.Plugin.Payments.Garanti.Views.PaymentGaranti.Configure", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();
            //save settings 
            _GarantiPaymentSettings.Mode = model.Mode;
            _GarantiPaymentSettings.Version = model.Version;
            _GarantiPaymentSettings.TerminalId = model.TerminalId;
            _GarantiPaymentSettings.ProvisionUserId = model.ProvisionUserId;
            _GarantiPaymentSettings.ProvisionPassword = model.ProvisionPassword;
            _GarantiPaymentSettings.UserId = model.UserId;
            _GarantiPaymentSettings.MerchantId = model.MerchantId; 
            _GarantiPaymentSettings.Type = model.Type;
            _GarantiPaymentSettings.HostAddress = model.HostAddress;
            _GarantiPaymentSettings.MotoInd = model.MotoInd;
            _GarantiPaymentSettings.CardholderPresentCode = model.CardholderPresentCode;
            _GarantiPaymentSettings.StoreKey = model.StoreKey;
            _GarantiPaymentSettings.SuccessURL = model.SuccessURL;
            _GarantiPaymentSettings.ErrorURL = model.ErrorURL;
            _GarantiPaymentSettings.HostAddress3D = model.HostAddress3D;
            _GarantiPaymentSettings.SecurityLevel3D = model.SecurityLevel3D;

            _GarantiPaymentSettings.TestOrder = model.TestOrder;

            _GarantiPaymentSettings.TestMode = model.TestMode;
            _GarantiPaymentSettings.TestVersion = model.TestVersion;
            _GarantiPaymentSettings.TestTerminalId = model.TestTerminalId;
            _GarantiPaymentSettings.TestProvisionUserId = model.TestProvisionUserId;
            _GarantiPaymentSettings.TestProvisionPassword = model.TestProvisionPassword;
            _GarantiPaymentSettings.TestUserId = model.TestUserId;
            _GarantiPaymentSettings.TestMerchantId = model.TestMerchantId;
            _GarantiPaymentSettings.TestType = model.TestType;
            _GarantiPaymentSettings.TestHostAddress = model.TestHostAddress;
            _GarantiPaymentSettings.TestMotoInd = model.MotoInd;
            _GarantiPaymentSettings.TestCardholderPresentCode = model.CardholderPresentCode;
            _GarantiPaymentSettings.TestStoreKey = model.TestStoreKey;
            _GarantiPaymentSettings.TestSuccessURL = model.TestSuccessURL;
            _GarantiPaymentSettings.TestErrorURL = model.TestErrorURL;
            _GarantiPaymentSettings.TestHostAddress3D = model.TestHostAddress3D;
            _GarantiPaymentSettings.TestSecurityLevel3D = model.TestSecurityLevel3D;


            _settingService.SaveSetting(_GarantiPaymentSettings);
            return View("Nop.Plugin.Payments.Garanti.Views.PaymentGaranti.Configure", model);
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

            return View("Nop.Plugin.Payments.Garanti.Views.PaymentGaranti.PaymentInfo", model);
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