using FluentValidation.Attributes;
using Nop.Web.Framework.Mvc;
using Nop.Web.Validators.Newsletter;
using Nop.Web.Framework;
using System.Collections.Generic;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
//AF
namespace Nop.Web.Models.Newsletter
{
    [Validator(typeof(NewsletterBoxValidator))]
    public class NewsletterBoxModel : BaseNopModel
    {

        public NewsletterBoxModel()
        {
            Genders = new List<SelectListItem>();
            GenderEnabled = true;

        
        }

        
        [NopResourceDisplayName("Account.Fields.Email")]
        public string Email { get; set; }

        [Remote("CampaignEmailCheck", "Home")]
        [NopResourceDisplayName("Account.Fields.Email")]
        public string CampaignMail { get; set; }

        [Remote("EmailCheck1", "Home")]
        [NopResourceDisplayName("Campaign.Account.Fields.Email")]
        public string Email1 { get; set; }
 
        [Remote("EmailCheck2", "Home", AdditionalFields = "Email1")]
        [NopResourceDisplayName("Campaign.Account.Fields.Email")]
        public string Email2 { get; set; }

        [Remote("EmailCheck3", "Home", AdditionalFields = "Email1,Email2")]
        [NopResourceDisplayName("Campaign.Account.Fields.Email")]
        public string Email3 { get; set; }

        [Remote("EmailCheck4", "Home", AdditionalFields = "Email1,Email2,Email3")]
        [NopResourceDisplayName("Campaign.Account.Fields.Email")]
        public string Email4 { get; set; }

        [Remote("EmailCheck5", "Home", AdditionalFields = "Email1,Email2,Email3,Email4")]
        [NopResourceDisplayName("Campaign.Account.Fields.Email")]
        public string Email5 { get; set; }
        

        [NopResourceDisplayName("Account.Fields.Gender")]
        public string Gender { get; set; }

        [NopResourceDisplayName("Account.Fields.FirstName")]
        public string FirstName { get; set; }
        
        [NopResourceDisplayName("Account.Fields.LastName")]
        public string LastName { get; set; }  
        
        public bool GenderEnabled { get; set; }
        public IList<SelectListItem> Genders { get; set; }

        public bool Subscribe { get; set; }

        [NopResourceDisplayName("Address.Fields.Country")]
        public int? CountryId { get; set; }
        
        [NopResourceDisplayName("Address.Fields.Country")]
        [AllowHtml]
        public string CountryName { get; set; }

        [NopResourceDisplayName("Address.Fields.Country")]
        public IList<SelectListItem> AvailableCountries { get; set; }

       
        [NopResourceDisplayName("Address.Fields.TermOfUse")]
        public bool TermOfUse  { get; set; }


    }

    
}