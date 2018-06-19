using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nop.Core.Domain.Common
{
    public class Store : BaseEntity
    {
        public virtual string Name { get; set; }
        public virtual string Address { get; set; }
        public virtual string Phone { get; set; }
        public virtual string City { get; set; }
        public virtual string Type { get; set; }
        public virtual string Country { get; set; }
        public virtual string Location { get; set; }
        public virtual string Latitude { get; set; }
        public virtual string Longitude { get; set; }
        public virtual int? DisplayOrder { get; set; }
    }
}
