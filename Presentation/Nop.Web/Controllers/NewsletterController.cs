using System;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Messages;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Models.Newsletter;
using Nop.Services.Directory;
using System.Collections.Generic;
using Nop.Web.Models.Common;
using Nop.Web.Framework;
using Nop.Services.Logging;
using Nop.Core.Infrastructure;

namespace Nop.Web.Controllers
{
    public class NewsletterController : BaseNopController
    {
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IWorkflowMessageService _workflowMessageService;

        private readonly CustomerSettings _customerSettings;
        private readonly ICountryService _countryService;

        public NewsletterController(ILocalizationService localizationService,
            IWorkContext workContext, INewsLetterSubscriptionService newsLetterSubscriptionService,
            IWorkflowMessageService workflowMessageService, CustomerSettings customerSettings,
            ICountryService countryService)
        {
            this._localizationService = localizationService;
            this._workContext = workContext;
            this._newsLetterSubscriptionService = newsLetterSubscriptionService;
            this._workflowMessageService = workflowMessageService;

            this._customerSettings = customerSettings;
            this._countryService = countryService;
        }

        [ChildActionOnly]
        [BotGetControl]
        public ActionResult NewsletterBox()
        {
            if (_customerSettings.HideNewsletterBlock)
                return Content("");
            var model = new NewsletterBoxModel();
            //model.Genders.Add(new SelectListItem() { Text = _localizationService.GetResource("Common.Dropdown.SelectValue"), Value = "" });
            model.Genders.Add(new SelectListItem() { Text = _localizationService.GetResource("Account.Fields.Gender.Female"), Value = "F" });
            model.Genders.Add(new SelectListItem() { Text = _localizationService.GetResource("Account.Fields.Gender.Male"), Value = "M" });

            return PartialView(model);
        }

        [HttpPost]
        [BotPostControl(RedirectUrl = "/", RedirectAjaxUrl = "/Catalog/ProductEmailAFriendSuccess", TrapFormElementName = "Surname", MinimumRequestPeriod = 5)]
        [ValidateInput(false)]
        public JsonResult SubscribeNewsletter(NewsletterBoxModel model)
        {
            string result;
            bool success = false;
            var email = model.Email;
            var subscribe = model.Subscribe;
            if (!CommonHelper.IsValidEmail(email))
                result = _localizationService.GetResource("Newsletter.Email.Wrong");
            else
            {
                //subscribe/unsubscribe
                email = email.Trim();

                var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmail(email);
                if (subscription != null)
                {
                    if (subscribe)
                    {
                        if (!subscription.Active)
                        {
                            _workflowMessageService.SendNewsLetterSubscriptionActivationMessage(subscription, _workContext.WorkingCurrency.Id);
                        }
                        result = _localizationService.GetResource("Newsletter.SubscribeEmailSent");
                    }
                    else
                    {
                        if (subscription.Active)
                        {
                            _workflowMessageService.SendNewsLetterSubscriptionDeactivationMessage(subscription, _workContext.WorkingLanguage.Id);
                        }
                        result = _localizationService.GetResource("Newsletter.UnsubscribeEmailSent");
                    }
                }
                else if (subscribe)
                {
                    subscription = new NewsLetterSubscription()
                    {
                        NewsLetterSubscriptionGuid = Guid.NewGuid(),
                        Email = email,
                        FirstName= model.FirstName,
                        LastName = model.LastName,
                        Gender = model.Gender,
                        Active = true,
                        CreatedOnUtc = DateTime.UtcNow,
                        LanguageId = _workContext.WorkingLanguage.Id 
                    };
                    _newsLetterSubscriptionService.InsertNewsLetterSubscription(subscription);
                    //_workflowMessageService.SendNewsLetterSubscriptionActivationMessage(subscription, _workContext.WorkingLanguage.Id);

                    //result = _localizationService.GetResource("Newsletter.SubscribeEmailSent");
                }
                else
                {
                    //result = _localizationService.GetResource("Newsletter.UnsubscribeEmailSent");
                }
                success = true;
            }

            return Json(new
            {
                Success = success
            });
        }

        public ActionResult SubscriptionActivation(Guid token, bool active)
        {
            var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByGuid(token);
            if (subscription == null)
                return RedirectToAction("Index", "Home");
            var model = new SubscriptionActivationModel();
            if (active)
            {
                subscription.Active = active;
                _newsLetterSubscriptionService.UpdateNewsLetterSubscription(subscription);

                if (subscription.RegistrationType == "promoted5")
                {
                    StatefulStorage.PerSession.Add<Guid>("newslettersubscriptiontoken", () => token );
                   return RedirectToAction("NewsletterPromotedDynasty");
                }
            }
            else
                _newsLetterSubscriptionService.DeleteNewsLetterSubscription(subscription);

            if (active)
                model.Result = _localizationService.GetResource("Newsletter.ResultActivated");
            else
                model.Result = _localizationService.GetResource("Newsletter.ResultDeactivated");

            return View(model);
        }


        public ActionResult NewsletterPromoted()
        {
            if (false)
                return Content("");
            var model = new NewsletterBoxModel();
            //model.Genders.Add(new SelectListItem() { Text = _localizationService.GetResource("Common.Dropdown.SelectValue"), Value = "" });
            model.Genders.Add(new SelectListItem() { Text = _localizationService.GetResource("Account.Fields.Gender.Female"), Value = "F" });
            model.Genders.Add(new SelectListItem() { Text = _localizationService.GetResource("Account.Fields.Gender.Male"), Value = "M" });

            model.AvailableCountries = new List<SelectListItem>();
            foreach (var c in _countryService.GetAllCountries())
                model.AvailableCountries.Add(new SelectListItem() { Text = c.Name, Value = c.Id.ToString() });

            return PartialView(model);
        }

        [HttpPost]
        //[ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult NewsletterPromoted(NewsletterBoxModel model)
        {
            string result;
            bool success = false;
            var email = model.Email;
            var subscribe = model.Subscribe;
            if (!CommonHelper.IsValidEmail(email))
            {
                ModelState.AddModelError("", _localizationService.GetResource("Newsletter.Email.Wrong"));
            //model.Genders.Add(new SelectListItem() { Text = _localizationService.GetResource("Common.Dropdown.SelectValue"), Value = "" });
                model.Genders.Add(new SelectListItem() { Text = _localizationService.GetResource("Account.Fields.Gender.Female"), Value = "F" });
                model.Genders.Add(new SelectListItem() { Text = _localizationService.GetResource("Account.Fields.Gender.Male"), Value = "M" });

                model.AvailableCountries = new List<SelectListItem>();
            foreach (var c in _countryService.GetAllCountries())
                model.AvailableCountries.Add(new SelectListItem() { Text = c.Name, Value = c.Id.ToString() });

            return View(model);

            }
            else
            {
                //subscribe/unsubscribe
                email = email.Trim();
                NewsLetterSubscription subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmail(email);
                if (subscription == null)
                {
                    subscription = new NewsLetterSubscription();
                    subscription.CreatedOnUtc = DateTime.UtcNow;
                    subscription.Email = email;
                }

                subscription.NewsLetterSubscriptionGuid = Guid.NewGuid();
                subscription.FirstName = model.FirstName;
                subscription.LastName = model.LastName;
                subscription.Gender = model.Gender;
                subscription.Active = false;
                subscription.LanguageId = _workContext.WorkingLanguage.Id;
                subscription.CountryId = model.CountryId.HasValue ? model.CountryId.Value : 0;
                subscription.RegistrationType = "promoted5";

                _newsLetterSubscriptionService.InsertNewsLetterSubscription(subscription);
                _workflowMessageService.SendNewsLetterSubscriptionActivationMessage(subscription, subscription.RegistrationType, _workContext.WorkingLanguage.Id);
                ViewBag.Success = true;
                return View();
            }
        }

        public ActionResult NewsletterPromotedDynasty()
        {
            var token = StatefulStorage.PerSession.Get<Guid>("newslettersubscriptiontoken");
            var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByGuid(token);
            if (subscription == null)
                return RedirectToAction("Index", "Home");
            if (!subscription.Active)
                return RedirectToAction("Index", "Home");

            var model = new  NewsletterDynastyModel();
            model.RootEmail= subscription.Email;
            return View(model);

 
        }
        
        [HttpPost]
        public ActionResult NewsletterPromotedDynasty(FormCollection form)
        {
            if(!ModelState.IsValid)
            {
                return RedirectToAction("Index", "Home");
            }
            var emailList = new List<string>();
            var rootEmail = form["RootEmail"];

            foreach (var key in form.AllKeys)
            {
                if (!key.StartsWith("Email", StringComparison.InvariantCultureIgnoreCase)) continue;

                if(!string.IsNullOrEmpty(form[key]))
                {
                    if (emailList.Contains(form[key].ToLower().Trim())) continue;
                    emailList.Add(form[key].ToLower().Trim());
                }
            }

            foreach (var email in emailList)
            {
                this.SubscribeFriend(rootEmail, email);
            }
            return RedirectToAction("NewsletterPromotedDynastySuccess");


 
        }

        private void SubscribeFriend(string rootEmail, string email)
        {
            email = email.Trim();
            if (rootEmail == email) return;
            try
            {
                NewsLetterSubscription subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmail(email);
                if (subscription == null)
                {
                    subscription = new NewsLetterSubscription();
                    subscription.CreatedOnUtc = DateTime.UtcNow;
                    subscription.Email = email;
                    subscription.RefererEmail = rootEmail;
                    subscription.NewsLetterSubscriptionGuid = Guid.NewGuid();
                    // subscription.FirstName = model.FirstName;
                    // subscription.LastName = model.LastName;
                    // subscription.Gender = model.Gender;
                    subscription.Active = false;
                    subscription.LanguageId = _workContext.WorkingLanguage.Id;
                    subscription.CountryId = 0;
                    subscription.RegistrationType = "promoted5";
                    _newsLetterSubscriptionService.InsertNewsLetterSubscription(subscription);

                }
                else
                {
                    subscription.RefererEmail = rootEmail;
                    subscription.NewsLetterSubscriptionGuid = Guid.NewGuid();
                    // subscription.FirstName = model.FirstName;
                    // subscription.LastName = model.LastName;
                    // subscription.Gender = model.Gender;
                    subscription.Active = false;
                    subscription.LanguageId = _workContext.WorkingLanguage.Id;
                    subscription.CountryId = 0;
                    subscription.RegistrationType = "promoted5";
                    _newsLetterSubscriptionService.UpdateNewsLetterSubscription(subscription);
 
                }
               _workflowMessageService.SendNewsLetterSubscriptionActivationMessage(subscription, subscription.RegistrationType, _workContext.WorkingLanguage.Id);

            }
            catch (Exception ex)
            {
                EngineContext.Current.Resolve<ILogger>().Error("Newsletter friend subscription failed: root email:" + rootEmail + "- email:" + email, ex);
            }
        }

        public ActionResult NewsletterPromotedDynastySuccess()
        {
            return View();

        }
    }
}
