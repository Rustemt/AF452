using System;
using Nop.Core;

namespace AF.Nop.Plugins.XmlUpdate.Domain
{
    public class XmlProperty : BaseEntity
    {
        public int ProviderId { get; set; }

        public string Name { get; set; }

        public string ProductProperty { get; set; }

        public bool Enabled { get; set; }

        public virtual XmlProvider Provider { get; set; }
    }
}
