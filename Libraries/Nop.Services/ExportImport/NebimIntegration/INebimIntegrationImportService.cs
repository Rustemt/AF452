using System;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using System.Collections.Generic;
namespace Nop.Services.ExportImport
{
    public interface INebimIntegrationImportService
    {
     

        void GetAllProducts(string langCode, int lastNDay);
        void ImportAllProducts();

       // long GetLastOrdersSyncTime();
       // long GetLastProductsSyncTime();

    }
}
