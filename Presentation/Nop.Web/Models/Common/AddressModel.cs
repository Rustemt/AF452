using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation.Attributes;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using Nop.Web.Validators.Common;

namespace Nop.Web.Models.Common
{
    [Validator(typeof(AddressValidator))]
    public class AddressModel : BaseNopEntityModel
    {
        public AddressModel()
        {
            AvailableCountries = new List<SelectListItem>();
            AvailableStates = new List<SelectListItem>();
            EnterpriseOptions = new List<SelectListItem>();
            Genders = new List<SelectListItem>();

        }
        [NopResourceDisplayName("Address.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }
        [NopResourceDisplayName("Address.Fields.FirstName")]
        [AllowHtml]
        public string FirstName { get; set; }
        [NopResourceDisplayName("Address.Fields.LastName")]
        [AllowHtml]
        public string LastName { get; set; }
        [NopResourceDisplayName("Address.Fields.Email")]
        [AllowHtml]
        public string Email { get; set; }
        [NopResourceDisplayName("Address.Fields.Company")]
        [AllowHtml]
        public string Company { get; set; }
        [NopResourceDisplayName("Address.Fields.Country")]
        public int? CountryId { get; set; }
        [NopResourceDisplayName("Address.Fields.Country")]
        [AllowHtml]
        public string CountryName { get; set; }
        [NopResourceDisplayName("Address.Fields.StateProvince")]
        public int? StateProvinceId { get; set; }
        [NopResourceDisplayName("Address.Fields.StateProvince")]
        [AllowHtml]
        public string StateProvinceName { get; set; }
        [NopResourceDisplayName("Address.Fields.City")]
        [AllowHtml]
        public string City { get; set; }
        [NopResourceDisplayName("Address.Fields.Address1")]
        [AllowHtml]
        public string Address1 { get; set; }
        [NopResourceDisplayName("Address.Fields.Address2")]
        [AllowHtml]
        public string Address2 { get; set; }
        [NopResourceDisplayName("Address.Fields.ZipPostalCode")]
        [AllowHtml]
        public string ZipPostalCode { get; set; }
        [NopResourceDisplayName("Address.Fields.PhoneNumber")]
        [AllowHtml]
        public string PhoneNumber { get; set; }
        [NopResourceDisplayName("Address.Fields.FaxNumber")]
        [AllowHtml]
        public string FaxNumber { get; set; }

        [NopResourceDisplayName("Address.Fields.CivilNo")]
        [AllowHtml]
        public string CivilNo { get; set; }

        [NopResourceDisplayName("Address.Fields.TaxOffice")]
        [AllowHtml]
        public string TaxOffice { get; set; }

        [NopResourceDisplayName("Address.Fields.TaxNo")]
        [AllowHtml]
        public string TaxNo { get; set; }

        [NopResourceDisplayName("Address.Fields.Title")]
        [AllowHtml]
        public string Title { get; set; }
        // [NopResourceDisplayName("Address.Fields.DefaultShipping")]
        [AllowHtml]
        public bool DefaultShippingAddress { get; set; }
        // [NopResourceDisplayName("Address.Fields.DefaultBilling")]
        [AllowHtml]
        public bool DefaultBillingAddress { get; set; }
        // [NopResourceDisplayName("Address.Fields.IsEnterprise")]
        [AllowHtml]
        public bool IsEnterprise { get; set; }

        public IList<SelectListItem> EnterpriseOptions { get; set; }
        public IList<SelectListItem> Genders { get; set; }
        public IList<SelectListItem> AvailableCountries { get; set; }
        public IList<SelectListItem> AvailableStates { get; set; }

        public bool FirstNameDisabled { get; set; }
        public bool LastNameDisabled { get; set; }
        public bool EmailDisabled { get; set; }
        public bool CompanyDisabled { get; set; }
        public bool CountryDisabled { get; set; }
        public bool StateProvinceDisabled { get; set; }
        public bool CityDisabled { get; set; }
        public bool Address1Disabled { get; set; }
        public bool Address2Disabled { get; set; }
        public bool ZipPostalCodeDisabled { get; set; }
        public bool PhoneNumberDisabled { get; set; }
        public bool FaxNumberDisabled { get; set; }
        public bool CivilNoDisabled { get; set; }
        public bool TaxOfficeDisabled { get; set; }
        public bool TaxNoDisabled { get; set; }

    }
}