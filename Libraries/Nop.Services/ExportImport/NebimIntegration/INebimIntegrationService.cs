using System;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using System.Collections.Generic;
namespace Nop.Services.ExportImport
{
    public interface INebimIntegrationService
    {
        //string AddUpdateCustomerToNebim(Nop.Core.Domain.Customers.Customer customer);
        string AddUpdateOrderCustomerToNebim(Nop.Core.Domain.Orders.Order order);
        
       // void AddUpdateOrderToNebim(Nop.Core.Domain.Orders.Order order);
        //void AddUpdateOrdersToNebim(IList<Nop.Core.Domain.Orders.Order> order);
        //void AddUpdateOrdersToNebim();
        void CreateShipment(Core.Domain.Orders.Order order);

        void AddUpdateProductVariantToNebim(Nop.Core.Domain.Catalog.ProductVariant productVariant);
        void AddUpdateProductVariantsToNebim(IList<Nop.Core.Domain.Catalog.ProductVariant> productVariant);
        void AddUpdateProductToNebim(Nop.Core.Domain.Catalog.Product product);
        void AddUpdateProductsToNebim(IList<Nop.Core.Domain.Catalog.Product> product);
        //void AddUpdateAllProducts();
        void SyncOrderToNebim(Core.Domain.Orders.Order order);

        //void GetAllProducts(string langCode, int lastNDay);


       // long GetLastOrdersSyncTime();
       // long GetLastProductsSyncTime();

    }
}
