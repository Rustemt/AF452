﻿using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using Nop.Web.Validators.Customer;
using FluentValidation.Attributes;

namespace Nop.Web.Models.Customer
{
    [Validator(typeof(LoginValidator))]
    public class LoginModel : BaseNopModel
    {
        public bool CheckoutAsGuest { get; set; }

        [NopResourceDisplayName("Account.Login.Fields.Email")]
        public string Email { get; set; }

        public bool UsernamesEnabled { get; set; }
        [NopResourceDisplayName("Account.Login.Fields.UserName")]
        public string UserName { get; set; }

        [DataType(DataType.Password)]
        [NopResourceDisplayName("Account.Login.Fields.Password")]
        public string Password { get; set; }

        [NopResourceDisplayName("Account.Login.Fields.RememberMe")]
        public bool RememberMe { get; set; }
    }
}