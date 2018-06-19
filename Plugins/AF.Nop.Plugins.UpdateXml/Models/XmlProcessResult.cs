using Nop.Core.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AF.Nop.Plugins.XmlUpdate.Models
{
    public class XmlProcessResult
    {
        public int XmlItemsCount { get {return Records.Count(x => x.IsProcessed); } }
        public IList<int> VariantsIds { get; }
        public IList<int> ManufacturersIds { get; }

        public XmlProcessResult()
        {
            Records = new List<ProcessRecord>();
            VariantsIds = new List<int>();
            ManufacturersIds = new List<int>();
        }
        public IList<ProcessRecord> Records { get; }
        public IList<ProcessRecord> XmlRecords { get { return Records.Where(x => x.IsProcessed).ToList(); } }

        public void AddRecord(ProcessRecord record)
        {
            var product = record.Product;
            bool found = false;
            record.IsProcessed = true;
            if(product!=null)
            {

                VariantsIds.Add(record.ProductVariant.Id);
                var manufacturers = product.ProductManufacturers;
                foreach(var m in manufacturers)
                {
                    found = false;
                    foreach(var m2 in ManufacturersIds)
                    {
                        if (m2 == m.ManufacturerId)
                        {
                            found = true;
                            break;
                        }
                    }
                    if(!found)
                        ManufacturersIds.Add(m.ManufacturerId);
                }
            }

            Records.Add(record);
        }

        public void AddMissingProduct(ProductVariant product)
        {
            if (product == null) return;

            var record = new ProcessRecord()
            {
                IsProcessed = false,
                ProductVariant = product,
                Sku = product.Sku,
                Price = product.Price,
                Quantity = product.StockQuantity
            };

            Records.Add(record);
        }
    }
}
