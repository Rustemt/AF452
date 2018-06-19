using Nop.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using Nop.Core;
using AF.Nop.Plugins.XmlUpdate.Data;
using System.Data.Entity.Infrastructure;

namespace AF.Nop.Plugins.XmlUpdate.Domain
{
    public class XmlUpdateObjectContext : DbContext, IDbContext
    {
        public XmlUpdateObjectContext(string nameOrConnectionString) : base(nameOrConnectionString) { }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new XmlProviderMap());
            modelBuilder.Configurations.Add(new XmlPropertyMap());
            modelBuilder.Conventions.Remove<IncludeMetadataConvention>();

            base.OnModelCreating(modelBuilder);
        }

        public string CreateDatabaseScript()
        {
            return ((IObjectContextAdapter)this).ObjectContext.CreateDatabaseScript();
        }

        public new IDbSet<TEntity> Set<TEntity>() where TEntity : BaseEntity
        {
            return base.Set<TEntity>();
        }

        /// <summary>
        /// Install
        /// </summary>
        public void Install()
        {
            //create the table
            Database.SetInitializer<XmlUpdateObjectContext>(null);

            var dbScript = CreateDatabaseScript();
            Database.ExecuteSqlCommand(dbScript);
            SaveChanges();
        }

        /// <summary>
        /// Uninstall
        /// </summary>
        public void Uninstall()
        {
            //drop the table
            try
            {
                Database.ExecuteSqlCommand("DROP TABLE " + XmlPropertyMap.TABLE_NAME);
                Database.ExecuteSqlCommand("DROP TABLE " + XmlProviderMap.TABLE_NAME);
                SaveChanges();
            }
            catch
            {
            }
        }


        public IList<TEntity> ExecuteStoredProcedureList<TEntity>(string commandText, params object[] parameters) where TEntity : BaseEntity, new()
        {
            throw new NotImplementedException();
        }
    }
}
