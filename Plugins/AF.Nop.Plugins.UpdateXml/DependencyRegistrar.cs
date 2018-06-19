using AF.Nop.Plugins.XmlUpdate.Domain;
using AF.Nop.Plugins.XmlUpdate.Services;
using Autofac;
using Autofac.Core;
using Autofac.Integration.Mvc;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;

namespace AF.Nop.Plugins.XmlUpdate
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        private const string CONTEXT_NAME = "af_object_context_xml_update";

        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder)
        {
            builder.RegisterType<XmlProviderService>().As<IXmlProviderService>()
                //.InstancePerHttpRequest()
                .InstancePerLifetimeScope()
            ;
            //this.RegisterPluginDataContext<XmlProviderObjectContext>(builder, CONTEXT_NAME);

            var dataSettingsManager = new DataSettingsManager();
            var dataProviderSettings = dataSettingsManager.LoadSettings();
            if (dataProviderSettings != null && dataProviderSettings.IsValid())
            {
                builder.Register<IDbContext>(c => new XmlUpdateObjectContext(dataProviderSettings.DataConnectionString))
                    .Named<IDbContext>(CONTEXT_NAME)
                    //.InstancePerHttpRequest()
                    .InstancePerLifetimeScope()
                ;

                builder.Register<XmlUpdateObjectContext>(c => new XmlUpdateObjectContext(dataProviderSettings.DataConnectionString))
                    //.InstancePerHttpRequest()
                    .InstancePerLifetimeScope()
                ;
            }
            else
            {
                builder.Register<IDbContext>(c => new XmlUpdateObjectContext(c.Resolve<DataSettings>().DataConnectionString))
                    .Named<IDbContext>(CONTEXT_NAME)
                    //.InstancePerHttpRequest()
                    .InstancePerLifetimeScope()
                ;

                builder.Register<XmlUpdateObjectContext>(c => new XmlUpdateObjectContext(c.Resolve<DataSettings>().DataConnectionString))
                    //.InstancePerHttpRequest()
                    .InstancePerLifetimeScope()
                ;
            }

            builder.RegisterType<EfRepository<XmlProvider>>()
                .As<IRepository<XmlProvider>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(CONTEXT_NAME))
                .InstancePerLifetimeScope()
            ;

            builder.RegisterType<EfRepository<XmlProperty>>()
                .As<IRepository<XmlProperty>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(CONTEXT_NAME))
                .InstancePerLifetimeScope()
            ;
        }

        public int Order
        {
            get { return 1; }
        }
    }
}
