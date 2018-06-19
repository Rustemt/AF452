using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nop.Web.Models.Catalog
{
    public class AttributeModel
    {
        public int ProductVariantAttributeId { get; set; }
        public int AttributeId { get; set; }
        public int ParentProductVariantAttributeValueId { get; set; }
        public string Name { get; set; }
        public IList<AttributeValueModel> ValueModels { get; set; }
        public AttributeModel()
        {
            ValueModels = new List<AttributeValueModel>();
        }
        public AttributeModel(ProductModel.ProductVariantModel.ProductVariantAttributeModel model)
            :this()
        {
            AttributeId = model.ProductAttributeId;
            Name = model.Name;
            ParentProductVariantAttributeValueId = 0;
            ProductVariantAttributeId = model.Id;
        }
        public AttributeModel(ProductModel.ProductVariantModel.ProductVariantAttributeModel model, int parentValueId)
            : this(model)
        {
            ParentProductVariantAttributeValueId = parentValueId;
        }
    }

    //public class ProductAttributeValueViewModel
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //    public int ProductAttributeOptionId { get; set; }
    //    public IList<ValueViewModel> RelatedValueModels { get; set; }
    //}

    public class AttributeValueModel
    {
        public int ProductVariantAttributeValueId { get; set; }
        public int ProductAttributeOptionId { get; set; }
        public string Name { get; set; }
        public AttributeModel RelatedAttributeViewModel { get; set; }
        //This value determines the variant of the product in combo selection
        public int VariantId { get; set; }
        public int Quantity { get; set; }

        public AttributeValueModel(ProductModel.ProductVariantModel.ProductVariantAttributeValueModel model)
        {
            ProductVariantAttributeValueId = model.Id;
            Name = model.Name;
            ProductAttributeOptionId = model.ProductAttributeOptionId;
            VariantId = 0;
        }
        public AttributeValueModel(ProductModel.ProductVariantModel.ProductVariantAttributeValueModel model, int variantId)
            : this(model)
        {
            VariantId = variantId;
        }
        public AttributeValueModel(ProductModel.ProductVariantModel.ProductVariantAttributeValueModel model, int variantId, int quantity)
            : this(model,variantId)
        {
            Quantity = quantity;
        }
    }
}