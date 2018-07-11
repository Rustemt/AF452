using AF.Nop.Plugins.RssFeed.Services;
using Autofac;
using Autofac.Core;
using Autofac.Integration.Mvc;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;

namespace AF.Nop.Plugins.RssFeed
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder)
        {
            builder.RegisterType<CustomWebHelper>().As<IWebHelper>().InstancePerHttpRequest();
            builder.RegisterType<RssFeedService>().As<IRssFeedService>().InstancePerLifetimeScope() ;
        }

        public int Order
        {
            get { return 10; }
        }
    }
}
