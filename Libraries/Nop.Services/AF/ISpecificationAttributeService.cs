using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.Catalog;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Specification attribute service interface
    /// </summary>
    public partial interface ISpecificationAttributeService
    {
        IList<SpecificationAttributeOption> GetSpecificationAttributeOptionsByIds(IList<int> specificationAttributeOptionIds);
    }
}
