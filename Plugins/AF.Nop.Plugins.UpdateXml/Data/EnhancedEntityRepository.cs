using Nop.Core;
using Nop.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace AF.Nop.Plugins.XmlUpdate.Data
{
    public class EnhancedEntityRepository<T> : EfRepository<T> where T : BaseEntity
    {
        protected readonly IDbContext _myContext;
        private IDbSet<T> _myEntities;

        public EnhancedEntityRepository(IDbContext context) : base(context)
        {
            this._myContext = context;
        }

        public IDbSet<T> DbSet
        {
            get
            {
                if (_myEntities == null)
                    _myEntities = _myContext.Set<T>();
                return _myEntities;
            }
        }

        public void DalateAll(IQueryable<T> query)
        {
            foreach (var o in query)
                this.Delete(o);
        }
    }
}
