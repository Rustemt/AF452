
using Nop.Core.Domain.Orders;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Tax;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;

namespace AF.Nop.Plugins.Criteo
{
    public class CriteoJsCode
    {
        public const string SERVICE_URL = "//static.criteo.net/js/ld/ld.js";

        public int AccountId { get; set; }

        public bool HashEmail { get; set; }

        public string CustomerEmail { get; set; }

        public string ClientDevice { get; set; }

        public CriteoJsCode() { }

        public string GenerateHomePageScript()
        {
            var packet = InitPacket();
            packet.Add(new { @event = "viewHome" });
            var script = GenerateScript(packet);
            return script;
        }

        public string GenerateProductScript(int id)
        {
            var packet = InitPacket();
            packet.Add(new { @event = "viewItem", item = id.ToString() });
            var script = GenerateScript(packet);
            return script;
        }

        public string GenerateCartScript(IEnumerable<ShoppingCartItem> cart)
        {
            var packet = InitPacket();
            List<object> items = new List<object>();
            var _taxService = EngineContext.Current.Resolve<ITaxService>();
            var _priceCalculationService = EngineContext.Current.Resolve<IPriceCalculationService>();

            if (_taxService == null)
                throw new Exception("Cannot Resolve an object of ITaxService");
            if (_priceCalculationService == null)
                throw new Exception("Cannot Resolve an object of IPriceCalculationService");

            decimal taxRate = decimal.Zero, unitPrice, finalUnitPrice;
            foreach (var sci in cart)
            {
                unitPrice = _priceCalculationService.GetUnitPrice(sci, true);
                finalUnitPrice = _taxService.GetProductPrice(sci.ProductVariant, unitPrice, out taxRate);

                items.Add(new {
                    id = sci.ProductVariant.ProductId.ToString(),
                    price = finalUnitPrice.ToString("0.00", CultureInfo.InvariantCulture),
                    quantity = sci.Quantity.ToString()
                });
            }
            packet.Add(new { @event = "viewBasket", item = items });
            var script = GenerateScript(packet);
            return script;
        }

        public string GenerateOrderScript(Order order)
        {
            var packet = InitPacket();
            List<object> items = new List<object>();
            decimal unitPrice;

            foreach (var oi in order.OrderProductVariants)
            {
                unitPrice = oi.UnitPriceInclTax;

                items.Add(new
                {
                    id = oi.ProductVariant.ProductId,
                    price = unitPrice.ToString("0.00", CultureInfo.InvariantCulture),
                    quantity = oi.Quantity.ToString()
                });
            }
            packet.Add(new { @event = "trackTransaction", id=order.Id.ToString(), item = items });
            var script = GenerateScript(packet);
            return script;
        }

        public string GenerateProductListScript(IEnumerable<int> products)
        {
            var packet = InitPacket();
            List<object> items = new List<object>();
            packet.Add(new { @event = "viewList", item = products.Select(x=>x.ToString()).ToArray() });
            var script = GenerateScript(packet);
            return script;
        }

        protected List<object> InitPacket()
        {
            List<object> packet = new List<object>()
            {
                new {@event="setAccount", account=AccountId},
                new {@event="setSiteType", type=ClientDevice}
            };

            #region Add Customer Email

            if (string.IsNullOrEmpty(CustomerEmail))
                packet.Add(new { @event = "setHashedEmail", email = "" });
            else
            {
                var customerEmail = CustomerEmail.Trim().ToLowerInvariant();
                if (HashEmail)
                    packet.Add(new { @event = "setHashedEmail", email = GetHashEmail(customerEmail) });
                else
                    packet.Add(new { @event = "setEmail", email = customerEmail });
            }

            #endregion

            return packet;
        }

        protected string GenerateScript(List<object> packet)
        {
            var serialier = new JavaScriptSerializer();
            List <string> json = new List<string>();
            foreach (var item in packet)
            {
                try
                {
                    json.Add (serialier.Serialize(item));
                }
                catch (Exception x)
                {
                    throw;
                }
            }

            var script = new StringBuilder();
            script.AppendLine(string.Format(@"<script type=""text/javascript"" src=""{0}"" async=""true""></script>", SERVICE_URL));
            script.AppendLine(@"<script type=""text/javascript"">");
            script.AppendLine(@"window.criteo_q = window.criteo_q || [];");
            //script.AppendLine(string.Format("window.criteo_q=window.criteo_q.concat({0})", json));
            script.AppendLine(@"window.criteo_q.push(");
            script.Append("    ");
            script.Append(string.Join(",\n    ", json));
            script.AppendLine();
            script.AppendLine(@");");
            script.AppendLine(@"</script>");

            return script.ToString();
        }

        protected string GetHashEmail(string email)
        {
            if (string.IsNullOrEmpty(email)) return null;

            using (MD5 md5 = MD5.Create())
            {
                byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(email));
                var hash = BitConverter.ToString(data).Replace("-", "");
                return hash.ToLowerInvariant();
            }
        }
    }
}
