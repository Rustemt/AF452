using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;

namespace AlternativeDataAccess
{
	public class CatalogRepository
	{
		private IDbConnection _db = new SqlConnection(new DataSettingsManager().LoadSettings().DataConnectionString);

		public List<Product> ProductsByNewsItem(int newsItemId)
		{
			string sql = @"SELECT Product.*
							FROM  News_Product_Mapping
							INNER JOIN Product on News_Product_Mapping.ProductId = Product.Id
							WHERE NewsItemId = @newsItemId AND Product.Deleted <> cast(1 as bit) AND Product.Published = 1
							ORDER BY News_Product_Mapping.DisplayOrder ASC

							SELECT ProductVariant.*
							FROM  News_Product_Mapping
							INNER JOIN Product on News_Product_Mapping.ProductId = Product.Id
							INNER JOIN ProductVariant on ProductVariant.ProductId = Product.Id
							WHERE NewsItemId = @newsItemId AND Product.Deleted <> cast(1 as bit) AND Product.Published = 1";

			var mapped = _db.QueryMultiple(sql, new { newsItemId = newsItemId })
				.Map<Product, ProductVariant, int>(
				p => p.Id,
				pv => pv.ProductId,
				(p, pv) => { p.ProductVariants = pv; }
			);
			return mapped.ToList();		
		}


		public List<dynamic> ProductsSummaryByNewsItem(int newsItemId)
		{
			string sql = @"SELECT     Product.Id ProductId, min(Product.seName) ProductSeName, min(Picture.Id) PictureId, min(Picture.SeoFilename) PictureSeoFilename, '' PictureUrl
								FROM         News_Product_Mapping INNER JOIN
													  Product ON News_Product_Mapping.ProductId = Product.Id INNER JOIN
													  Product_Picture_Mapping ON Product.Id = Product_Picture_Mapping.ProductId INNER JOIN
													  Picture ON Product_Picture_Mapping.PictureId = Picture.Id
								WHERE     (News_Product_Mapping.NewsItemId = @newsItemId AND Product.Published = 1)
								group by Product.Id";

			var mapped = _db.Query(sql, new { newsItemId = newsItemId });
			return mapped.ToList();
		}

		public List<Category> Categories()
		{
			string sql = @"select Id, Name, SeName, ParentCategoryId, ShowOnHomePage, DisplayOrder, PictureId
							from Category
							where Published = 1 and Deleted <> 1";

			var mapped = _db.Query<Category>(sql);
			return mapped.ToList();
		}

		public List<Manufacturer> ManufacturerByParentCategory(int parentCategoryId)
		{
			string sql = @"select m.Id, m.name, m.DisplayOrder, m.SeName, min(m.PictureId) PictureId, min(m.MenuPictureId) MenuPictureId, min(m.MenuShowcasePictureId) MenuShowcasePictureId
								FROM Manufacturer m
								INNER JOIN Product_Manufacturer_Mapping pmm ON m.Id = pmm.ManufacturerId
								INNER JOIN Product_Category_Mapping pcm ON pmm.ProductId = pcm.ProductId
								INNER JOIN Category c ON pcm.CategoryId = c.Id
								INNER JOIN Product p ON pcm.ProductId = p.Id
								INNER JOIN CategoryChildren(@parentCategoryId) cc ON cc.Id = pcm.CategoryId
								WHERE m.Deleted <> 1 AND m.Published = 1
								and c.Deleted <> 1 AND c.Published = 1
								and p.Deleted <> 1 AND p.Published = 1
								group by m.Id, m.name, m.DisplayOrder, m.SeName";

			var mapped = _db.Query<Manufacturer>(sql, new { parentCategoryId = parentCategoryId });
			return mapped.ToList();
		}

		public List<Manufacturer> Manufacturers()
		{
			string sql = @"select * FROM Manufacturer m WHERE m.Deleted <> 1 AND m.Published = 1";

			var mapped = _db.Query<Manufacturer>(sql);
			return mapped.ToList();
		}

		public List<Manufacturer> ManufacturersByStartingLetter(string mLetter)
		{
			string sql = @"select * FROM Manufacturer m WHERE m.Deleted <> 1 AND m.Published = 1 and SUBSTRING(name, 1, 1) = @mLetter" ;

			var mapped = _db.Query<Manufacturer>(sql, new { mLetter = mLetter });
			return mapped.ToList();
		}
	}
}