using AF.Nop.Plugins.XmlUpdate.Domain;
using AF.Nop.Plugins.XmlUpdate.Models;
using Nop.Core.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AF.Nop.Plugins.XmlUpdate.Extensions
{
    public static class Extensions
    {
        #region ProviderModel / XmlProvider
        public static ProductModel ToViewModel(this ProductVariant entity)
        {
            return new ProductModel()
            {
                Id = entity.Id,
                Name = entity.Name,
                Price = entity.Price,
                Sku = entity.Sku,
                StockQuantity = entity.StockQuantity
            };
        }
        #endregion



        #region ProviderModel / XmlProvider
        public static ProviderModel ToViewModel(this XmlProvider entity)
        {
            return new ProviderModel()
            {
                Id = entity.Id,
                Name = entity.Name,
                XmlRootNode = entity.XmlRootNode,
                XmlItemNode = entity.XmlItemNode,
                Url = entity.Url,
                AuthType = entity.AuthType,
                Username = entity.Username,
                Password = entity.Password,
                Enabled = entity.Enabled,
                AutoResetStock =entity.AutoResetStock,
                UnpublishZeroStock = entity.UnpublishZeroStock,
                AutoUnpublish=entity.AutoUnpublish
            };
        }

        public static XmlProvider ToEntity(this ProviderModel viewModel)
        {
            var entity = new XmlProvider()
            {
                Id = viewModel.Id,
                Name = viewModel.Name,
                Url = viewModel.Url,
                XmlRootNode = viewModel.XmlRootNode,
                XmlItemNode = viewModel.XmlItemNode,
                AuthType = viewModel.AuthType,
                Username = viewModel.Username,
                Password = viewModel.Password,
                Enabled = viewModel.Enabled,
                UnpublishZeroStock = viewModel.UnpublishZeroStock,
                AutoResetStock = viewModel.AutoResetStock,
                AutoUnpublish = viewModel.AutoUnpublish
            };

            return entity;
        }
        #endregion


        #region PropertyModel / XmlProperty
        public static PropertyModel ToViewModel(this XmlProperty entity)
        {
            return new PropertyModel()
            {
                Id = entity.Id,
                Name = entity.Name,
                Enabled = entity.Enabled,
                ProductProperty = entity.ProductProperty,
            };
        }

        public static XmlProperty ToEntity(this PropertyModel viewModel)
        {
            var entity = new XmlProperty()
            {
                Name = viewModel.Name,
                ProductProperty = viewModel.ProductProperty,
                Enabled = viewModel.Enabled
            };
            if (viewModel.Id.HasValue)
                entity.Id = viewModel.Id.Value;
            return entity;
        }
        #endregion
        
    }
}
