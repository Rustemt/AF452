using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation.Attributes;
using Nop.Admin.Validators.Catalog;
using Nop.Web.Framework;
using Nop.Web.Framework.Localization;
using Nop.Web.Framework.Mvc;

namespace Nop.Admin.Models.Catalog
{
    [Validator(typeof(ProductAttributeOptionValidator))]
    public class ProductAttributeOptionModel : BaseNopEntityModel, ILocalizedModel<ProductAttributeOptionLocalizedModel>
    {
        public ProductAttributeOptionModel()
        {
            Locales = new List<ProductAttributeOptionLocalizedModel>();
        }

        public int ProductAttributeId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Options.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Options.Fields.DisplayOrder")]
        public int DisplayOrder {get;set;}

        public IList<ProductAttributeOptionLocalizedModel> Locales { get; set; }

    }

    public class ProductAttributeOptionLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Options.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }
    }
}