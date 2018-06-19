//using AF.Nop.Plugins.XmlUpdate.Models;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.Linq;
//using System.Text;

//namespace AF.Nop.Plugins.XmlUpdate.Validators
//{
//    public class PropertyListValidator : ValidationAttribute
//    {
//        public PropertyListValidator():base()
//        {

//        }

//        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
//        {
//            IList<PropertyModel> list = value as IList<PropertyModel>;
//            if (list == null)
//            {
//                return new ValidationResult("{0} does not contain any valid properties");
//            }
//            foreach(var prop in list)
//            {
//                if(prop.Enabled && String.IsNullOrEmpty(prop.Name))
//                    return new ValidationResult(String.Format("The property {0} is enabled but it does not have a value", prop.ProductProperty));
//            }

//            return ValidationResult.Success;
//        }
//    }
//}
