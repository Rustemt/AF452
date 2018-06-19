using System;
using Nop.Core;
using System.Collections.Generic;

namespace AF.Nop.Plugins.XmlUpdate.Domain
{
    public class XmlProvider : BaseEntity
    { 
        protected ICollection<XmlProperty>  _properties;

        public string Name { get; set; }
        
        public string Url { get; set; }

        public string XmlRootNode { get; set; }

        public string XmlItemNode { get; set; }

        public int AuthType { get; set; }

        public bool Enabled { get; set; }

        public string Username { get; set; }
        
        public string Password { get; set; }

        public bool AutoUnpublish { get; set; }

        public bool AutoResetStock { get; set; }

        public bool UnpublishZeroStock { get; set; }

        public virtual ICollection<XmlProperty> Properties
        {
            get { return _properties ?? (_properties = new List<XmlProperty>()); }
            protected set { _properties = value; }
        }
    }
}
