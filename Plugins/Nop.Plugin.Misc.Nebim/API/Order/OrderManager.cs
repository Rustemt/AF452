using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NebimV3.Library;
using NebimV3.v3Entities;
using NebimV3.ItemTransactions;
using NebimV3.DataConnector;
using Nop.Services.ExportImport;
using Nop.Services.Logging;

namespace Nop.Plugin.Misc.NebimIntegration.API
{
    public class OrderManager
    {
        ILogger _logger;
        NebimIntegrationSettings _nebimIntegrationSettings;
        public OrderManager(NebimIntegrationSettings nebimIntegrationSettings, ILogger logger)
        {
            _nebimIntegrationSettings = nebimIntegrationSettings;
            ConnectionManager.ConnectDB(nebimIntegrationSettings);
            _logger = logger;
        }
        public string CreateOrder(string description, DateTime? orderDate, string customerCode, Guid? shippingPostalAddressID, Guid? BillingPostalAddressID,
            IList<Tuple<string, string, string, string, int, decimal?, decimal?, Tuple<string>>> items,
            decimal? discountAmount, double? discountRate, string currencyCode, decimal shippingCost, decimal giftBoxCost, string discountNames)
        {
            try
            {
                using (NebimV3.DataConnector.JoinedSqlTransactionScope sqlTrans = new JoinedSqlTransactionScope())
                {

                    NebimV3.Orders.RetailSale order = new NebimV3.Orders.RetailSale();

                    // Required fields
                    order.OrderDate = orderDate.HasValue ? orderDate.Value : NebimV3.ApplicationCommon.V3Application.Context.Today; // Or --> order.OrderDate = new DateTime(2011, 12, 05)
                    order.OrderTime = orderDate.HasValue ? orderDate.Value.TimeOfDay : NebimV3.ApplicationCommon.V3Application.Context.Time; // (Optional)
                    order.OfficeCode = _nebimIntegrationSettings.API_OfficeCode;// "O-2": Daimamoda
                    order.WarehouseCode = _nebimIntegrationSettings.API_WarehouseCode;// "1-2-1-1": Daimamoda Merkez
                    order.DocCurrencyCode = currencyCode;
                    order.DocumentNumber = description;
                    order.CustomerCode = customerCode;
                    if (!order.Customer.ExistsInDB())
                        throw new Exception("Order customer does not exists. CustomerCode:" + customerCode);
                    
                    order.Customer.Load();
                    order.Customer.CurrAccDefault.Load();
                    order.Customer.AllPostalAddresses.Load();

                    #region addresses
                    //first set default addresses.
                    order.ShippingPostalAddressID = order.Customer.CurrAccDefault.PostalAddressID;
                    order.BillingPostalAddressID = order.Customer.CurrAccDefault.PostalAddressID;

                    if (shippingPostalAddressID.HasValue)
                    {
                        var address = order.Customer.AllPostalAddresses.FirstOrDefault(x => x.PostalAddressID == shippingPostalAddressID.Value);
                        if (address != null)
                        {
                            order.ShippingPostalAddressID = address.PostalAddressID;
                        }
                    }

                    if (BillingPostalAddressID.HasValue)
                    {
                        var address = order.Customer.AllPostalAddresses.FirstOrDefault(x => x.PostalAddressID == BillingPostalAddressID.Value);
                        if (address != null)
                        {
                            order.BillingPostalAddressID = address.PostalAddressID;
                        }
                    }

                    #endregion addresses

                    // Optional
                    //order.ShipmentMethodCode = "1"; // ("1" means Immediate Delivery for ex.)
                    order.Description = description;//keep B2C order id

                    foreach (var item in items)
                    {
                        if (item.Rest != null && !string.IsNullOrWhiteSpace(item.Rest.Item1))
                        {
                            // via barcode
                            CreateRetailSaleOrderLineWithBarcode(order, item.Rest.Item1, item.Item5, item.Item6, item.Item7, null, currencyCode, discountNames);
                        }
                        else
                        {
                            //via combinations,variant
                            CreateRetailSaleOrderLineWithItemVariant(order, item.Item1, item.Item2, item.Item3, item.Item4, item.Item5, item.Item6, item.Item7, null, currencyCode, discountNames);
                        }
                    }

                    //shipment cost
                    if (shippingCost > 0)
                        CreateRetailSaleOrderExpense(order, _nebimIntegrationSettings.API_ShipmentExpenseProductCode, null, shippingCost, currencyCode);
                    //giftbox cost
                    if (giftBoxCost > 0)
                    {
                        CreateRetailSaleOrderExpense(order, _nebimIntegrationSettings.API_GiftboxExpenseProductCode, null, giftBoxCost, currencyCode);
                    }


                    //discounts to order total!
                    if (discountRate.HasValue)
                        ApplyDiscountRate(order, discountRate.Value); // % 30 iskonto
                    if (discountAmount.HasValue)
                        ApplyDiscountAmount(order, discountAmount.Value); // 20 TL iskonto



                    //TODO: ?
                    order.SaveAsCompleted();

                    //do not ship order it will be shipped after by another event (manual or scheduled automatically)
                    //this.CreateShipment(order);

                    sqlTrans.Commit();
                    return order.OrderNumber;
                }
            }
            catch (Exception ex)
            {
                NebimV3.Library.V3Exception v3Ex = ex as NebimV3.Library.V3Exception;
                if (v3Ex != null)
                    throw new Exception(NebimV3.ApplicationCommon.ExceptionHandlerBase.Default.GetExceptionMessage(v3Ex), ex);
                throw;

            }
        }

        // This method demonstrates how to use "variant info" and other options.
        private void CreateRetailSaleOrderLineWithItemVariant(NebimV3.Orders.RetailSale ST,
            string itemCode,
            string colorCode,
            string itemDim1Code,
            string itemDim2Code,
            int quantity,
            decimal? discountIncludingTax,
            decimal? priceIncludingTax,
            string description,
            string currencyCode,
            string discountNames
            )
        {
            // Create a new line.
            NebimV3.Orders.RetailSaleLine line = (NebimV3.Orders.RetailSaleLine)(ST.TransactionFactory.CreateLine(ST));

            // Required
            line.ItemTypeCode = (byte)(NebimV3.ApplicationCommon.ItemTypes.Product);
            line.ItemCode = itemCode;
            line.ColorCode = colorCode;
            line.ItemDim1Code = itemDim1Code;
            line.ItemDim2Code = itemDim2Code; 
            line.ItemDim3Code = "";
            line.Qty1 = quantity;
            //line.CurrencyCode = currencyCode;
            line.ActualPriceCurrencyCode = currencyCode;


            if (priceIncludingTax.HasValue)
            {
                line.PriceVI = priceIncludingTax.Value;// kdv dahil 
            }
           

            line.LineDescription = description;

            line.Save(); // Saving the first line of the transaction, also saves the transaction header.

              //save discount names(campaign names)
            if (_nebimIntegrationSettings.DiscountNameSavingEnabled && !string.IsNullOrWhiteSpace(discountNames))
            {
                foreach (var name in discountNames.Split(new string[] { "--" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    SaveOrderDiscountOffer(Guid.Empty, ST.HeaderID, line.LineID, 0, 0, "1", name);
                }
            }
        }

        private void CreateRetailSaleOrderLineWithBarcode(NebimV3.Orders.RetailSale TW, string barcode, int quantity, decimal? discountIncludingTax,
            decimal? priceIncludingTax, string description, string currencyCode, string discountNames)
        {
            // Create a new line.
            NebimV3.Orders.RetailSaleLine line = (NebimV3.Orders.RetailSaleLine)(TW.TransactionFactory.CreateLine(TW));

            // If you do not have any item variant info(ItemCode, ColorCode, Dim1Code, etc...) 
            // But have a Barcode Number
            line.UsedBarcode = barcode;
            line.Qty1 = quantity;
            line.ActualPriceCurrencyCode = currencyCode;

            if (priceIncludingTax.HasValue)
            {
                line.PriceVI = priceIncludingTax.Value;// kdv ddahil 
            }
            line.LineDescription = description;
            // Optional properties(LineDescription, DeliveryDate etc...) can be assigned also as described in previous method.
            line.Save();

            //save discount names(campaign names)
            if (_nebimIntegrationSettings.DiscountNameSavingEnabled && !string.IsNullOrWhiteSpace(discountNames))
            {
                foreach (var name in discountNames.Split(new string[] { "--" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    SaveOrderDiscountOffer(Guid.Empty, TW.HeaderID, line.LineID, 0, 0, "1", name);
                }
            }

        }

        private void CreateRetailSaleOrderExpense(NebimV3.Orders.RetailSale ST,
         string itemCode,
         decimal? discountIncludingTax,
         decimal? priceIncludingTax,
         string currencyCode
         )
        {
            // Create a new line.
            NebimV3.Orders.RetailSaleLine line = (NebimV3.Orders.RetailSaleLine)(ST.TransactionFactory.CreateLine(ST));

            // Required
            //line.ItemTypeCode = (byte)(NebimV3.ApplicationCommon.ItemTypes.Expense);
            line.ItemTypeCode = (byte)(NebimV3.ApplicationCommon.ItemTypes.Product);
            line.ItemCode = itemCode;
            line.ColorCode = "";
            line.ItemDim1Code = "";
            line.ItemDim2Code = "";
            line.ItemDim3Code = "";
            line.Qty1 = 1;
            //line.CurrencyCode = currencyCode;
            line.ActualPriceCurrencyCode = currencyCode;

            if (priceIncludingTax.HasValue)
            {
                line.PriceVI = priceIncludingTax.Value;// kdv ddahil 
            }
            line.Save(); // Saving the first line of the transaction, also saves the transaction header.
        }


        private void ApplyDiscountAmount(NebimV3.Orders.RetailSale order, decimal discountAmount)
        {

            // amount discount is not supported so, do it by rate discount
            //    order.Summary.Calculate();
            //    order.Summary.SetTDiscountVI(1, discountAmount); // 1 sabit
            //    order.ApplySummary();
        }

        private void ApplyDiscountRate(NebimV3.Orders.RetailSale order, double rate)
        {
            order.Summary.Calculate();
            order.Summary.SetTDisRate(2, rate); // 2 sabit
            order.ApplySummary();
        }


        public void CreateShipment(string orderDescription)
        {
            try
            {
                using (JoinedSqlTransactionScope sqlTrans = new JoinedSqlTransactionScope())
                {
                    var OrderHeaderID = FindOrderHeaderIDByDescription(orderDescription);
                    if (!OrderHeaderID.HasValue)
                    {
                        throw new Exception("CreateShipment=> B2C order id:" + orderDescription + "can not find order from B2C id");
                    }
                    NebimV3.Orders.RetailSale order = new NebimV3.Orders.RetailSale(OrderHeaderID.Value);
                    //order.get
                    NebimV3.Shipments.RetailSale newShipment = CreateRetailSaleShipment(order, "", null);
                    CreateRetailSaleShipmentLinesFromOrderLines(order, newShipment);
                    newShipment.SaveAsCompleted();
                    sqlTrans.Commit();
                }
            }
            catch (Exception ex)
            {
                NebimV3.Library.V3Exception v3Ex = ex as NebimV3.Library.V3Exception;
                if (v3Ex != null)
                    throw new Exception(NebimV3.ApplicationCommon.ExceptionHandlerBase.Default.GetExceptionMessage(v3Ex), ex);
                throw;
            }
        }

        //creates the shipment event => stock inventory update
        private void CreateShipment(NebimV3.Orders.RetailSale order)
        {
            try
            {
                using (JoinedSqlTransactionScope sqlTrans = new JoinedSqlTransactionScope())
                {
                    NebimV3.Shipments.RetailSale newShipment = CreateRetailSaleShipment(order, "", null);
                    CreateRetailSaleShipmentLinesFromOrderLines(order, newShipment);
                    newShipment.SaveAsCompleted();
                    sqlTrans.Commit();
                }
            }
            catch (Exception ex)
            {
                NebimV3.Library.V3Exception v3Ex = ex as NebimV3.Library.V3Exception;
                if (v3Ex != null)
                    throw new Exception(NebimV3.ApplicationCommon.ExceptionHandlerBase.Default.GetExceptionMessage(v3Ex), ex);
                throw;
            }
        }

        private NebimV3.Shipments.RetailSale CreateRetailSaleShipment(NebimV3.Orders.RetailSale order, string description, DateTime? shipmentDate)
        {
            if (order == null)
            {
                throw new Exception("order can not be null for shipment");
            }
            if (!order.ExistsInDB())
            {
                throw new Exception("order does not exists for the shipment. order description:" + order.Description);
            }

            order.Load();
            NebimV3.Shipments.RetailSale shipment = new NebimV3.Shipments.RetailSale();

            // Required fields
            shipment.IsOrderBase = true;
            shipment.ShippingDate = shipmentDate.HasValue ? shipmentDate.Value : NebimV3.ApplicationCommon.V3Application.Context.Today; // Or --> shipment.ShippingDate = new DateTime(2011, 12, 05)
            shipment.ShippingTime = shipmentDate.HasValue ? shipmentDate.Value.TimeOfDay : NebimV3.ApplicationCommon.V3Application.Context.Time; // (Optional)
            shipment.OfficeCode = _nebimIntegrationSettings.API_OfficeCode;
            shipment.WarehouseCode = _nebimIntegrationSettings.API_WarehouseCode;

            shipment.CustomerCode = order.CustomerCode;
            //shipment.Customer.CurrAccDefault.Load();
            shipment.ShippingPostalAddressID = order.ShippingPostalAddressID;
            shipment.BillingPostalAddressID = order.BillingPostalAddressID;

            // Optional
            shipment.Description = description;

            return shipment;
        }
        private void CreateRetailSaleShipmentLinesFromOrderLines(NebimV3.Orders.RetailSale order, NebimV3.Shipments.RetailSale ST)
        {
            foreach (NebimV3.Orders.RetailSaleLine orderLine in order.Lines)
            {
                NebimV3.Shipments.RetailSaleLine line = (NebimV3.Shipments.RetailSaleLine)(ST.TransactionFactory.CreateLine(ST));

                // Required
                line.OrderLineID = orderLine.LineID;
                line.Qty1 = orderLine.Qty1; // Or may be less.
                line.ItemCode = orderLine.ItemCode;
                // Optional
                line.LineDescription = orderLine.LineDescription;

                line.Save();
            }
        }

        private Guid? FindOrderHeaderIDByDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description)) return null;
            try
            {
                NebimV3.DataConnector.SqlSelectStatement query = new NebimV3.DataConnector.SqlSelectStatement();
                query.TableNames.Add("trOrderHeader", false);

                query.Parameters.Add(new NebimV3.DataConnector.PropertyCondition("trOrderHeader", "Description"));
                query.Parameters.Add(new NebimV3.DataConnector.PropertyCondition("trOrderHeader", "OrderHeaderID"));
                query.Filter = new NebimV3.DataConnector.GroupCondition();

                if (description != null)
                {
                    query.Filter.AddCondition(
                      new NebimV3.DataConnector.BinaryCondition(
                          new NebimV3.DataConnector.PropertyCondition("trOrderHeader", "Description"),
                          new NebimV3.DataConnector.ValueCondition(description)
                          ));
                }


                HashSet<Guid> results = new HashSet<Guid>();

                using (System.Data.IDataReader reader = NebimV3.DataConnector.SqlStatmentExecuter.ExecuteSelect(query))
                {
                    while (reader.Read())
                    {
                        results.Add((Guid)(reader["OrderHeaderID"]));
                    }
                }

                // if (results.Count > 1)  // Örnek olarak yaptik. Exception atmak yerine ne yapilmasi gerektigi uygulamaya göre degisebilir.
                //     throw new Exception("More than one record with the same B2C customer Id");

                if (results.Count == 0)
                    return null;

                return results.LastOrDefault();
            }
            catch (Exception ex)
            {
                NebimV3.Library.V3Exception v3Ex = ex as NebimV3.Library.V3Exception;
                if (v3Ex != null)
                    throw new Exception(NebimV3.ApplicationCommon.ExceptionHandlerBase.Default.GetExceptionMessage(v3Ex), ex);
                throw;

            }
        }


        //satır bazlı indirim
        //Kampanya için.
        //Satıra LDisRate3 alanına indirim uygulanmalı.
        //Bu durumda LdiscountVI3 alanı otomatik olarak hesaplanacaktır.
        //Daha sonra satıdaki LdiscountVI3 tutarı dikkate alınarak aşağıdaki foksiyon çağırılmalı.
        //Yani eğer bir satıra 2 ayrı iskonto kodu atanacaksa aşağıdaki fonksiyonu iki kere çağırmalı ve discount parametresine geçtiğiniz değerlerin toplamı, LdiscountVI3 e yazdığınız değere eşit olmalı.
        //NOT: Eğer bunlar sipariş için kullanılacaksa, Invoice geçen yerleri Order olarak replace etmeniz yetecektir.
        //NOT 2: Eğer discountOfferCode ve discountPointTypeCode nedir? Buraya hangi değerler gelebilir? Şeklinde sorularınız olursa bu konuda Fatih Bey yardımcı olacaktır.

        private void SaveInvoiceDiscountOffer(Guid customerDiscountPointID, Guid InvoiceHeaderID, Guid LineID, decimal discount, double rate, string discountPointTypeCode, string discountOfferCode)
        {
            NebimV3.Invoices.InvoiceDiscountOffer InvoiceDiscountOffer = new NebimV3.Invoices.InvoiceDiscountOffer();
            InvoiceDiscountOffer.InvoiceHeaderID = InvoiceHeaderID;
            InvoiceDiscountOffer.InvoiceLineID = LineID;
            InvoiceDiscountOffer.DiscountOfferCode = discountOfferCode.Substring(0, 10);
            InvoiceDiscountOffer.DiscountPointTypeCode = discountPointTypeCode;
            InvoiceDiscountOffer.DiscountRate = rate;
            InvoiceDiscountOffer.UsedAmount = discount;
            InvoiceDiscountOffer.Save();
        }


        private void SaveOrderDiscountOffer(Guid customerDiscountPointID, Guid OrderHeaderID, Guid LineID, decimal discount, double rate, string discountPointTypeCode, string discountOfferCode)
        {
            NebimV3.Orders.OrderDiscountOffer OrderDiscountOffer = new NebimV3.Orders.OrderDiscountOffer();
            OrderDiscountOffer.OrderHeaderID = OrderHeaderID;
            OrderDiscountOffer.OrderLineID = LineID;
            OrderDiscountOffer.DiscountOfferCode = discountOfferCode.Substring(0, Math.Min(10, discountOfferCode.Length));// maximum 10 characters allowed.
            OrderDiscountOffer.DiscountPointTypeCode = discountPointTypeCode;
            OrderDiscountOffer.DiscountRate = rate;
            OrderDiscountOffer.UsedAmount = discount;
            OrderDiscountOffer.Save();
        }    

    }
}
