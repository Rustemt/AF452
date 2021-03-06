﻿using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation.Attributes;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using Nop.Web.Validators.Customer;
using Nop.Services.Messages;

namespace Nop.Web.Models.Customer
{
    //AF
    [Validator(typeof(CustomerInfoValidator))]
    public class CustomerInfoModel : BaseNopModel
    {
     
        public CustomerInfoModel()
        {
            this.AvailableTimeZones = new List<SelectListItem>();
            this.AvailableLocations = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Account.Fields.PhoneNumber")]
        [AllowHtml]
        public string PhoneNumber { get; set; }

        [NopResourceDisplayName("Account.Fields.Nation")]
        [AllowHtml]
        public string Nation { get; set; }

        [NopResourceDisplayName("Account.Fields.Email")]
        [AllowHtml]
        public string Email { get; set; }

        [NopResourceDisplayName("Account.Fields.NewEmail")]
        [AllowHtml]
        public string NewEmail { get; set; }


        [NopResourceDisplayName("Account.Fields.ConfirmNewEmail")]
        [AllowHtml]
        public string ConfirmNewEmail { get; set; }

        public bool AllowUsersToChangeUsernames { get; set; }
        public bool UsernamesEnabled { get; set; }
        [NopResourceDisplayName("Account.Fields.Username")]
        [AllowHtml]
        public string Username { get; set; }

        //form fields & properties
        public bool GenderEnabled { get; set; }
        [NopResourceDisplayName("Account.Fields.Gender")]
        public string Gender { get; set; }

        [NopResourceDisplayName("Account.Fields.FirstName")]
        [AllowHtml]
        public string FirstName { get; set; }
        [NopResourceDisplayName("Account.Fields.LastName")]
        [AllowHtml]
        public string LastName { get; set; }

        [NopResourceDisplayName("Account.Fields.Password")]
        [AllowHtml]
        public string Password { get; set; }

        [NopResourceDisplayName("Account.Fields.NewPassword")]
        [AllowHtml]
        public string NewPassword { get; set; }

        [NopResourceDisplayName("Account.Fields.ConfirmNewPassword")]
        [AllowHtml]
        public string ConfirmNewPassword { get; set; }

        public bool DateOfBirthEnabled { get; set; }
        [NopResourceDisplayName("Account.Fields.DateOfBirth")]
        public int? DateOfBirthDay { get; set; }
        [NopResourceDisplayName("Account.Fields.DateOfBirth")]
        public int? DateOfBirthMonth { get; set; }
        [NopResourceDisplayName("Account.Fields.DateOfBirth")]
        public int? DateOfBirthYear { get; set; }

        public bool CompanyEnabled { get; set; }
        [NopResourceDisplayName("Account.Fields.Company")]
        [AllowHtml]
        public string Company { get; set; }

        public bool NewsletterEnabled { get; set; }
        [NopResourceDisplayName("Account.Fields.Newsletter")]
        public bool Newsletter { get; set; }

        //preferences
        public bool SignatureEnabled { get; set; }
        [NopResourceDisplayName("Account.Fields.Signature")]
        [AllowHtml]
        public string Signature { get; set; }

        public bool LocationEnabled { get; set; }
        [NopResourceDisplayName("Account.Fields.Location")]
        public int LocationCountryId { get; set; }
        public IList<SelectListItem> AvailableLocations { get; set; }

        //time zone
        [NopResourceDisplayName("Account.Fields.TimeZone")]
        public string TimeZoneId { get; set; }
        public bool AllowCustomersToSetTimeZone { get; set; }
        public IList<SelectListItem> AvailableTimeZones { get; set; }

        //EU VAT
        [NopResourceDisplayName("Account.Fields.VatNumber")]
        [AllowHtml]
        public string VatNumber { get; set; }
        public string VatNumberStatusNote { get; set; }
        public bool DisplayVatNumber { get; set; }

        public CustomerNavigationModel NavigationModel { get; set; }

        public IList<SelectListItem> Genders { get; set; }
    }
}