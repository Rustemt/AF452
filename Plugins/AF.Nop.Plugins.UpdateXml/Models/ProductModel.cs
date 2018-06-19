using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using System.Web.Mvc;

namespace AF.Nop.Plugins.XmlUpdate.Models
{
    public class ProductModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.ID")]
        public override int Id { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.Sku")]
        [AllowHtml]
        public string Sku { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.StockQuantity")]
        public int StockQuantity { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.Price")]
        public decimal Price { get; set; }
    }
}