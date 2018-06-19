using Nop.Core.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AF.Nop.Plugins.XmlUpdate.Models
{
    public class ProcessRecord
    {
        protected Manufacturer _manufacturer;
        public bool IsNewProduct { get { return ProductVariant == null; } }
        public bool IsPublished { get { return ProductVariant != null && ProductVariant.Product.Published; } }
        public bool IsProcessed { get; set; }
        public string Sku { get; set; }
        public decimal? Price { get; set; }
        public int? Quantity { get; set; }
        public Manufacturer Manufacturer { get { return GetManufacturer();  } }
        public string ManufacturerName { get { return Manufacturer != null ? Manufacturer.Name : ""; } }
        public string ProductName { get { return IsNewProduct ? "" : Product.Name; } }
        public ProductVariant ProductVariant { get; set; }
        public Product Product { get { return IsNewProduct ? null : ProductVariant.Product; } }

        public void SetProperty(string name, object value)
        {
            if (value == null) return;

            try
            {
                switch (name.ToLower())
                {
                    case "sku":
                        this.Sku = value.ToString();
                        break;
                    case "price":
                        this.Price = Convert.ToDecimal(value, System.Globalization.CultureInfo.InvariantCulture);
                        break;
                    case "stockquantity":
                        this.Quantity = Convert.ToInt32(value);
                        break;
                    default:
                        throw new Exception("Unrecognized proprty '" + name + "'");
                }
            }
            catch (Exception e)
            {
                var msg = "Invalid value for '{0}' property: {1}";
                msg = string.Format(msg, name, value == null ? "(null)" : value.ToString());
                
                throw new Exception(msg, e);
            }
        }
        public object GetProperty(string name, object defaultValue)
        {
            object value = null;
            switch (name.ToLower())
            {
                case "sku":
                    value = this.Sku;
                    break;
                case "price":
                    value = this.Price;
                    break;
                case "stockquantity":
                    value = this.Quantity;
                    break;
            }
            return value == null ? defaultValue : value;
        }

        public void UpdateProduct(bool unpublishZeroStock, bool autoResetStock, bool autoUnpublish)
        {
            if (IsNewProduct) return;
            if (IsProcessed/*Product is included in the XML*/)
            {
                if (Price.HasValue) ProductVariant.Price = Price.Value;
                if (Quantity.HasValue)
                {
                    ProductVariant.StockQuantity = Quantity.Value;
                    if (Quantity > 0)
                        ProductVariant.Published = Product.Published = true;
                    if (unpublishZeroStock)
                        ProductVariant.Published = Product.Published = (Quantity != 0);
                }
            }
            else /*Product is not included in the XML*/
            {
                if (autoResetStock) Quantity = ProductVariant.StockQuantity = 0;
                if (autoUnpublish) ProductVariant.Published = Product.Published = false;
            }
        }

        protected Manufacturer GetManufacturer()
        {
            if (_manufacturer != null)
                return _manufacturer;
            if (IsNewProduct)
                return null;
            if (Product.ProductManufacturers.Count == 0)
                return null;

            _manufacturer = Product.ProductManufacturers.ElementAt(0).Manufacturer;

            return _manufacturer;
        }

    }
}
