using Nop.Web.Framework.Mvc;
using System;
using System.Collections.Generic;

namespace Nop.Web.Models.Catalog
{
    public class ProductSpecificationModel : BaseNopModel
    {
        public int SpecificationAttributeId { get; set; }

        public int SpecificationAttributeOptionId { get; set; }

        public string SpecificationAttributeName { get; set; }

        public string SpecificationAttributeOption { get; set; }

        public int Position { get; set; }

        public bool Visible { get; set; }

       
    }

    public class ProductSpecificationModelComparer : IEqualityComparer<ProductSpecificationModel>
    {
        public bool Equals(ProductSpecificationModel x, ProductSpecificationModel y)
        {
            if (Object.ReferenceEquals(x, y)) return true;
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;
            return x.SpecificationAttributeOptionId == y.SpecificationAttributeOptionId;
        }

        public int GetHashCode(ProductSpecificationModel obj)
        {
            return obj.SpecificationAttributeOptionId;
        }
    }


    public partial class ProductSpecificationModel280 : BaseNopModel
    {
        public int SpecificationAttributeId { get; set; }

        public int SpecificationAttributeOptionId { get; set; }

        public string SpecificationAttributeName { get; set; }

        public string SpecificationAttributeOption { get; set; }

        public int Position { get; set; }

        public bool Visible { get; set; }
    }

}