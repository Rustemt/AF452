using System;
using System.Xml;
using Nop.Core.Configuration;
using Nop.Core.Domain.Directory;
using Nop.Core.Infrastructure;
using Nop.Services.Tasks;
using Nop.Services.Configuration;
using Nop.Services.Catalog;
using Nop.Core.Caching;
using Nop.Services.Logging;

namespace Nop.Services.Caching
{
    /// <summary>
    /// Represents BulkRequestTask for to be cached.
    /// </summary>
    public partial class BulkRequestTask : ITask
    {
        /// <summary>
        /// Executes a task
        /// </summary>
        public void Execute()
        {
            var productService = EngineContext.Current.Resolve<IProductService>();

            productService.RequestBulkCatalog();
           
        }
    }
}
