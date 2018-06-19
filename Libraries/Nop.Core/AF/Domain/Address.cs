using System;
using Nop.Core.Domain.Directory;

namespace Nop.Core.Domain.Common
{
    public partial class Address : BaseEntity, ICloneable
    {
        public virtual string Name { get; set; }
        public virtual string CivilNo { get; set; }
        public virtual string TaxOffice { get; set; }
        public virtual string TaxNo { get; set; }
        public virtual string Title { get; set; }
        public virtual bool DefaultShippingAddress { get; set; }
        public virtual bool DefaultBillingAddress { get; set; }
        public virtual bool IsEnterprise { get; set; }

        public object Clone()
        {
            var addr = new Address()
            {
                FirstName = this.FirstName,
                LastName = this.LastName,
                Email = this.Email,
                Company = this.Company,
                Country = this.Country,
                CountryId = this.CountryId,
                StateProvince = this.StateProvince,
                StateProvinceId = this.StateProvinceId,
                City = this.City,
                Address1 = this.Address1,
                Address2 = this.Address2,
                ZipPostalCode = this.ZipPostalCode,
                PhoneNumber = this.PhoneNumber,
                FaxNumber = this.FaxNumber,
                CreatedOnUtc = this.CreatedOnUtc,
                Name =this.Name,
                CivilNo =this.CivilNo,
                TaxOffice=this.TaxOffice,
                TaxNo =this.TaxNo,
                Title =this.Title,
                DefaultBillingAddress = this.DefaultBillingAddress,
                DefaultShippingAddress=this.DefaultShippingAddress,
                IsEnterprise = this.IsEnterprise

            };
            return addr;
        }
    }
}
