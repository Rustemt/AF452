using Autofac;
using Autofac.Core;
using Autofac.Integration.Mvc;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Plugin.Misc.XmlUpdateFromRotap.Services;
using Nop.Plugin.Misc.XmlUpdateProducts.Services;

namespace Nop.Plugin.Misc.XmlUpdateFromRotap
{
    public class DependencyRegistrar : IDependencyRegistrar
    {

        /// <summary>
        /// Registers the specified builder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="typeFinder">The type finder.</param>
        public void Register(ContainerBuilder builder, ITypeFinder typeFinder)
        {
            builder.RegisterType<ExcelService>().As<IExcelService>().InstancePerHttpRequest();
            builder.RegisterType<XmlUpdateFromRotapConsumingService>().As<IXmlUpdateFromRotapConsumingService>().InstancePerHttpRequest();            
        }

        /// <summary>
        /// Gets the order.
        /// </summary>
        public int Order
        {
            get { return 0; }
        }
    }
}