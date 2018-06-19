using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.UI.Paging;
using Nop.Core.Domain.AFEntities;
using System.Web.Mvc;

namespace Nop.Web.Models.Catalog
{
    //nop 2.80
    public partial class CatalogPagingFilteringModel280 : BasePageableModel
    {
        #region Constructors

        public CatalogPagingFilteringModel280()
        {
            this.AvailableSortOptions = new List<SelectListItem>();
            this.AvailableViewModes = new List<SelectListItem>();
            this.PageSizeOptions = new List<SelectListItem>();

            this.PriceRangeFilter = new PriceRangeFilterModel();
            this.SpecificationFilter = new SpecificationFilterModel();
            this.AttributeFilter = new AttributeFilterModel();
            this.ManufacturerFilter = new ManufacturerFilterModel();
            this.CategoryFilter = new CategoryFilterModel();
            this.AllowProductSorting = true;
            this.OrderBy = (int)ProductSortingEnum.Position;
        }

        #endregion

        #region Properties
        public bool FilteringDisabled { get; set; }
        public PriceRangeFilterModel PriceRangeFilter { get; set; }
        public SpecificationFilterModel SpecificationFilter { get; set; }
        public AttributeFilterModel AttributeFilter { get; set; }
        public ManufacturerFilterModel ManufacturerFilter { get; set; }
        public CategoryFilterModel CategoryFilter { get; set; }
        public string Q { get; set; }

        public bool AllowProductSorting { get; set; }
        public IList<SelectListItem> AvailableSortOptions { get; set; }

        public bool AllowProductViewModeChanging { get; set; }
        public IList<SelectListItem> AvailableViewModes { get; set; }

        public bool AllowCustomersToSelectPageSize { get; set; }
        public IList<SelectListItem> PageSizeOptions { get; set; }

        /// <summary>
        /// Order by
        /// </summary>
        [NopResourceDisplayName("Categories.OrderBy")]
        public int OrderBy { get; set; }

        /// <summary>
        /// Product sorting
        /// </summary>
        [NopResourceDisplayName("Categories.ViewMode")]
        public string ViewMode { get; set; }


        public bool ShowFilteringPanel { get; set; }

        /// <summary>
        /// S:specificitaion
        /// M:manufacturer
        /// A:attribute
        /// P:price range
        /// </summary>
        public string FilterTrigging { get; set; }


        #endregion

        #region Nested classes

        public partial class PriceRangeFilterModel
        {
            public bool Disabled { get; set; }
            public string CurrencySymbol { get; set; }
            public decimal Max { get; set; }
            public decimal Min { get; set; }
            public decimal StepSize { get; set; }
            //public PriceRange SelectedPriceRange { get; set; }
            public virtual decimal? From { get; set; }
            public virtual decimal? To { get; set; }
            public bool CurrencySymbolIsAfter { get; set; }

            public void PreparePriceRangeFilters(decimal? min, decimal? max, decimal? from, decimal? to, IWebHelper _webHelper, IWorkContext workContext)
            {
                if (!max.HasValue)
                {
                    this.Disabled = true;
                    return;
                }
                if (!min.HasValue) min = 0;
                this.StepSize = (decimal)Math.Pow(10, Math.Floor(Math.Log10((double)(max.Value - min.Value))) - 1);
                if (StepSize == 0) StepSize = max.Value;
                if (StepSize != 0)
            {
                max = Math.Ceiling(max.Value / StepSize) * StepSize;
                min = Math.Ceiling(min.Value / StepSize) * StepSize;
            }

            this.Min = min.Value;
            this.Max = max.Value;
            this.From = from.HasValue ? from.Value : 0;
            this.To = to.HasValue ? to.Value : this.Max;
            this.CurrencySymbol = workContext.WorkingCurrency.GetCurrencySymbol();
            this.CurrencySymbolIsAfter = workContext.WorkingCurrency.CurrencyCode == "TRY";

            }
             
        }

        public partial class PriceRange
        {
            public virtual decimal? From { get; set; }
            public virtual decimal? To { get; set; }
        }

        public partial class SpecificationFilterModel
        {
            public IList<SpecificationFilterGroup> Groups { get; set; }

            public IList<int> FilteredOptionIds { get; set; }

            public SpecificationFilterModel()
            {
                this.Groups = new List<SpecificationFilterGroup>();
                this.FilteredOptionIds = new List<int>();
            }
            public IList<int> GetFilteredOptionIds()
            {
                //IList<int> ids = new List<int>();
                //foreach (var group in this.Groups)
                //{
                //    foreach (var option in group.Items.Where(o => o.State == FilterItemState.Checked))
                //    {
                //        ids.Add(option.Id);
                //    }
                //}

                //return ids;
                return this.FilteredOptionIds;
            }

            public virtual void PrepareSpecsFilters(IList<int> alreadyFilteredSpecOptionIds,
             IList<int> filterableSpecificationAttributeOptionIds,
             ISpecificationAttributeService specificationAttributeService,
             IWebHelper webHelper,
             IWorkContext workContext)
            {
                var specificationAttributeOptions = specificationAttributeService.GetSpecificationAttributeOptionsByIds(filterableSpecificationAttributeOptionIds);
                SpecificationFilterGroup specificationFilterGroup = null;
                foreach (var specificationGroup in specificationAttributeOptions.GroupBy(sao => sao.SpecificationAttributeId))
                {
                    var specification = specificationGroup.FirstOrDefault().SpecificationAttribute;
                    specificationFilterGroup = new SpecificationFilterGroup() { Name = specification.GetLocalized(x => x.Name), Id = specification.Id };
                 
                    foreach (var item in specificationGroup)
                    {
                        specificationFilterGroup.Items.Add(new SpecificationFilterItem() { Id = item.Id, Name = item.GetLocalized(x => x.Name), State = alreadyFilteredSpecOptionIds.Contains(item.Id)?FilterItemState.Checked:FilterItemState.Unchecked});
                    }
                    this.Groups.Add(specificationFilterGroup);
                }

            }

        }

        public partial class SpecificationFilterGroup 
        {   
            public int Id { get; set; }
            public string Name { get; set; }
            public bool Enabled { get; set; }

            public IList<SpecificationFilterItem> Items { get; set; }
        
            public SpecificationFilterGroup()
            {
                this.Items = new List<SpecificationFilterItem>();
            }
         } 

        public partial class SpecificationFilterItem 
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public FilterItemState State { get; set; }
        }

        public partial class AttributeFilterModel
        {
            public IList<AttributeFilterGroup> Groups { get; set; }

            public AttributeFilterModel()
            {
                this.Groups = new List<AttributeFilterGroup>();
            }
            public IList<int> GetProductVariantAttributeIds()
            {
                //IList<int> ids = new List<int>();
                //foreach (var group in this.Groups)
                //{
                //    foreach (var option in group.Items.Where(o => o.State == FilterItemState.Checked))
                //    {
                //        ids.Concat(option.ProductVariantAttributeIds);
                //    }
                //}

                //return ids;

                return new List<int>();
            }

            public virtual void PrepareAttributeFilters(IList<int> alreadyFilteredProductVariantAttributeIds,
            IList<int> filterableProductVariantAttributeIds,
            IProductAttributeService productAttributeService,
            IWebHelper webHelper,
            IWorkContext workContext)
            {
                //var productVariantAttributes = productAttributeService.GetProductVariantAttributesByIds(filterableProductVariantAttributeIds);
                //AttributeFilterGroup attributeFilterGroup = null;
                //foreach (var attributeGroup in productVariantAttributes.GroupBy(pva => pva.ProductAttributeId))
                //{
                //    var attribute = attributeGroup.FirstOrDefault().ProductAttribute;
                //    attributeFilterGroup = new AttributeFilterGroup() { Name = attribute.GetLocalized(x => x.Name), Id = attribute.Id };
                //    foreach (var attributeGrouped in attributeGroup)
                //    {
                //        foreach (var valueGroup in attributeGrouped.ProductVariantAttributeValues.GroupBy(x => x.ProductAttributeOptionId))
                //        {
                //            var value = valueGroup.First();

                //            attributeFilterGroup.Items.Add(new AttributeFilterItem()
                //            {
                //                Id = value.ProductAttributeOptionId,
                //                Name = value.GetLocalized(x => x.Name),
                //                State = alreadyFilteredProductVariantAttributeIds.Contains(item.Id) ? FilterItemState.Checked : FilterItemState.Unchecked
                //            });
                //        }
                //    }
                //    this.Groups.Add(attributeFilterGroup);
                //}

            }
        }

        public partial class AttributeFilterGroup
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public bool Enabled { get; set; }

            public IList<AttributeFilterItem> Items { get; set; }

            public AttributeFilterGroup()
            {
                this.Items = new List<AttributeFilterItem>();
            }
        }

        public partial class AttributeFilterItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public FilterItemState State { get; set; }
            public IList<int> ProductVariantAttributeIds { get; set; }
        }

        public class ManufacturerFilterModel 
        {
            public bool Enabled { get; set; }
            public int ManufacturerId { get; set; }
            public IList<ManufacturerFilterItem> Items { get; set; }
            public IList<int> FilteredManufacturerIds { get; set; }
            public ManufacturerFilterModel()
            {
                Items = new List<ManufacturerFilterItem>();
                this.FilteredManufacturerIds = new List<int>();
            }
            public IList<int> GetFilteredManufacturerIds()
            {
                //IList<int> ids = new List<int>();
                //foreach (var item in this.Items.Where(i=>i.State==FilterItemState.Checked))
                //{
                //    ids.Add(item.Id);
                //}

                //return ids;
                return this.FilteredManufacturerIds;
            }


           public virtual void PrepareManufacturerFilters(int manufacturerId, IList<int> alreadyFilteredManufacturerIds,
           IList<int> filterableManufacturerIds,
           IManufacturerService manufacturerService,
           IWebHelper webHelper,
           IWorkContext workContext)
            {
                this.ManufacturerId = manufacturerId;
                //TODO:280 cache here.
                if (manufacturerId != 0)
                {
                    filterableManufacturerIds = new List<int>() { manufacturerId };
                }
                var manufacrurers = manufacturerService.GetManufacturersByIds(filterableManufacturerIds);
                this.Items = manufacrurers.Select(m =>
                {
                    return new ManufacturerFilterItem()
                        {
                            Id = m.Id,
                            Name = m.GetLocalized(x => x.Name),
                            State = alreadyFilteredManufacturerIds.Contains(m.Id) ? FilterItemState.Checked : FilterItemState.Unchecked
                        };
                }).ToList();

            }

           public virtual void PrepareManufacturerFilters(IList<int> alreadyFilteredManufacturerIds,
      IList<int> filterableManufacturerIds,
      IManufacturerService manufacturerService,
      IWebHelper webHelper,
      IWorkContext workContext)
           {
               this.PrepareManufacturerFilters(0, alreadyFilteredManufacturerIds, filterableManufacturerIds, manufacturerService, webHelper, workContext);
           }

        }

        public class ManufacturerFilterItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public FilterItemState State { get; set; }
        }

        public class CategoryFilterModel
        {
            public bool Enabled { get; set; }
            public int CategoryId { get; set; }
            public IList<CategoryFilterItem> Items { get; set; }
            public IList<int> FilteredCategoryIds { get; set; }
            public CategoryFilterModel()
            {
                Items = new List<CategoryFilterItem>();
                this.FilteredCategoryIds = new List<int>();
            }
            public IList<int> GetFilteredCategoryIds()
            {
                //IList<int> ids = new List<int>();
                //foreach (var item in this.Items.Where(i => i.State == FilterItemState.Checked))
                //{
                //    ids.Add(item.Id);
                //}
                //return ids;
                return this.FilteredCategoryIds;
            }

            public virtual void PrepareCategoryFilters(int categoryId, IList<int> alreadyFilteredCategoryIds,
         IList<int> filterableCategoryIds,
         ICategoryService categoryService,
         IWebHelper webHelper,
         IWorkContext workContext)
            {
                //TODO:280 cache here. get all needed categories at once.
                this.CategoryId = categoryId;
                if (this.CategoryId != 0)
                {
                    filterableCategoryIds = new List<int>() { categoryId };
                }
                foreach (var id in filterableCategoryIds)
                {
                    var category = categoryService.GetCategoryById(id);
                    this.Items.Add(new CategoryFilterItem()
                    {
                        Id = category.Id,
                        Name = category.GetLocalized(x => x.Name),
                        State = alreadyFilteredCategoryIds.Contains(category.Id) ? FilterItemState.Checked : FilterItemState.Unchecked
                    });
                }
            }

            public virtual void PrepareCategoryFilters(IList<int> alreadyFilteredCategoryIds,
         IList<int> filterableCategoryIds,
         ICategoryService categoryService,
         IWebHelper webHelper,
         IWorkContext workContext)
            {
                this.PrepareCategoryFilters(0, alreadyFilteredCategoryIds, filterableCategoryIds, categoryService, webHelper, workContext);
            }
        
        }

        public class CategoryFilterItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public FilterItemState State { get; set; }
        }
        
        #endregion

        public enum FilterItemState
        {
            Unchecked = 0,
            Checked = 1,
            CheckedDisabled = 2,
            Disabled = 3,
        }
    }
}