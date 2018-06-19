using System;
using Nop.Core.Domain.Localization;

namespace Nop.Core.Domain.Messages
{
    /// <summary>
    /// Represents NewsLetterSubscription entity
    /// </summary>
    public partial class NewsLetterSubscription : BaseEntity
    {
        public virtual string LastName { get; set; }
        public virtual string FirstName { get; set; }
        public virtual string Gender { get; set; }
        public virtual int LanguageId { get; set; }
        public virtual Language Language { get; set; }
        public virtual int? CountryId { get; set; }
        public virtual string RefererEmail { get; set; }
        public virtual string RegistrationType { get; set; }
    }
}
