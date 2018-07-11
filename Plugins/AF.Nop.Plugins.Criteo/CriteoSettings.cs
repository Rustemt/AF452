using System;
using Nop.Core.Configuration;

namespace AF.Nop.Plugins.Criteo
{
    public class CriteoSettings : ISettings
    {
        public int AccountId { get; set; }

        public bool HashEmail { get; set; }
    }
}
