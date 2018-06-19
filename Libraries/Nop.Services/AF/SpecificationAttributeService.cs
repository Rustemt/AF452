using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Events;
using Nop.Services.Localization;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Specification attribute service
    /// </summary>
    public partial class SpecificationAttributeService 
    {
        public virtual IList<SpecificationAttributeOption> GetSpecificationAttributeOptionsByIds(IList<int> specificationAttributeOptionIds)
        {
           if (specificationAttributeOptionIds == null || specificationAttributeOptionIds.Count == 0)
                return new List<SpecificationAttributeOption>();

            var query = _specificationAttributeOptionRepository.Table;

            query = query.Where(sao => specificationAttributeOptionIds.Contains(sao.Id));
            query = query.OrderBy(sao => sao.SpecificationAttribute.DisplayOrder).ThenBy(sao=>sao.DisplayOrder);

            var specificationAttributeOptions = query.ToList();
            return specificationAttributeOptions;
        }
    }
}

