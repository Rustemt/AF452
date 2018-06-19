using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Media;
using Nop.Core.Infrastructure;

namespace AlternativeDataAccess
{
	public class PictureRepository
	{
		private readonly ICacheManager _cacheManager;
		private static readonly object s_lock = new object();
		private IDbConnection _db = new SqlConnection(new DataSettingsManager().LoadSettings().DataConnectionString);

		public const string NOP_CACHE_PRODUCTPICTUREVARIANTS = "nop_cache_ProductPictureVariants";
		public const string NOP_CACHE_PRODUCTPICTURES = "nop_cache_ProductPictures";

		public PictureRepository()
		{
			this._cacheManager = EngineContext.Current.ContainerManager.Resolve<ICacheManager>("nop_cache_static");
		}
	
		public List<Picture> GetAllmanufacturerPictures()
		{
			var pl = this._db.Query<PictureLocal>("SELECT picture.Id, picture.MimeType, picture.IsNew, picture.SeoFilename FROM Manufacturer inner join picture on picture.Id = PictureId").ToList();
			return pl.Select(p => new Picture { Id = p.Id, IsNew = p.IsNew, MimeType = p.MimeType, SeoFilename = p.SeoFilename }).ToList();
		}

		private List<PictureLocal> _ProductPicturesByVariant()
		{
			string sql = @"select Id, MimeType, IsNew, SeoFilename, ProductVariantId, DisplayOrder from Picture,
								(
								SELECT PictureId, DisplayOrder, ProductVariantId
								FROM ProductVariant_Picture_Mapping
								union all
								SELECT ppm.PictureId, ppm.DisplayOrder, pv.Id as ProductVariantId
								FROM Product_Picture_Mapping ppm, ProductVariant pv
								where ppm.ProductId = pv.ProductId  
								and pv.Id not in (SELECT ProductVariantId FROM ProductVariant_Picture_Mapping)
									) X
							where Picture.Id = PictureId";

			return _db.Query<PictureLocal>(sql).ToList();
		}

		public List<Picture> GetByProductVariantId(int productVariantId, int recordsToReturn)
		{
			List<PictureLocal> pictureLocals = null;
			if (!_cacheManager.IsSet(NOP_CACHE_PRODUCTPICTUREVARIANTS))
			{
				lock (s_lock)
				{
					pictureLocals = _cacheManager.Get(NOP_CACHE_PRODUCTPICTUREVARIANTS, () =>
					{
						return _ProductPicturesByVariant();
					});
				};
			}
			if (pictureLocals == null)
			{
				pictureLocals = _cacheManager.Get(NOP_CACHE_PRODUCTPICTUREVARIANTS, () =>
				{
					return _ProductPicturesByVariant();
				});
			}
			var ps = pictureLocals.Where(x => x.ProductVariantId == productVariantId).OrderBy(x => x.DisplayOrder).ToList();
			if (recordsToReturn > 0)
				ps = ps.Take(recordsToReturn).ToList();

			return ps.Select(p => new Picture { Id = p.Id, IsNew = p.IsNew, MimeType = p.MimeType, SeoFilename = p.SeoFilename }).ToList();
		}

		public List<Picture> GetAllByProductAndVariant()
		{
			List<PictureLocal> pictureLocals = _cacheManager.Get(NOP_CACHE_PRODUCTPICTUREVARIANTS, () =>
			{
				return _ProductPicturesByVariant();
			});

			return pictureLocals.Select(p => new Picture { Id = p.Id, IsNew = p.IsNew, MimeType = p.MimeType, SeoFilename = p.SeoFilename }).ToList();
		}

		private List<PictureLocal> _ProductPictures()
		{
			string sql = @"select Picture.Id, MimeType, IsNew, SeoFilename, ProductId, DisplayOrder from Picture, Product_Picture_Mapping ppm
							where Picture.Id = PictureId";

			return _db.Query<PictureLocal>(sql).ToList();
		}

		public List<Picture> GetByProductId(int productId, int recordsToReturn)
		{
			List<PictureLocal> pictureLocals = null;
			if (!_cacheManager.IsSet(NOP_CACHE_PRODUCTPICTURES))
			{
				lock (s_lock)
				{
					pictureLocals = _cacheManager.Get(NOP_CACHE_PRODUCTPICTURES, () =>
					{
						return _ProductPictures();
					});
				};
			}
			if (pictureLocals == null)
			{
				pictureLocals = _cacheManager.Get(NOP_CACHE_PRODUCTPICTURES, () =>
				{
					return _ProductPictures();
				});
			}
			var ps = pictureLocals.Where(x => x.ProductId == productId).OrderBy(x => x.DisplayOrder).ToList();
			if (recordsToReturn > 0)
				ps = ps.Take(recordsToReturn).ToList();

			return ps.Select(p => new Picture { Id = p.Id, IsNew = p.IsNew, MimeType = p.MimeType, SeoFilename = p.SeoFilename }).ToList();
		}

		public byte[] PictureBinary(int pictureId)
		{
			string sql = @"select PictureBinary from Picture where Id = @pictureId";
			return _db.Query<byte[]>(sql, new { pictureId = pictureId }).SingleOrDefault();
		}

		public Picture GetByProductVariantId(int productVariantId)
		{
			return GetByProductVariantId(productVariantId, 1).FirstOrDefault();
		}

		public Picture Find(int pictureId)
		{
			return this._db.Query<Picture>("select * from Picture where Id = @pictureId", new { pictureId = pictureId}).SingleOrDefault();
		}

		public Picture Add(Picture user)
		{
			var sqlQuery = "INSERT INTO Users (FirstName, LastName, Email) VALUES(@FirstName, @LastName, @Email); ” + “SELECT CAST(SCOPE_IDENTITY() as int)";
			var userId = this._db.Query<int>(sqlQuery, user).Single();
			user.Id = userId;
			return user;
		}

		public Picture Update(Picture user)
		{
			var sqlQuery =
			"UPDATE Users " +
			"SET FirstName = @FirstName, " +
			" LastName = @LastName, " +
			" Email = @Email " +
			"WHERE UserID = @UserID";
			this._db.Execute(sqlQuery, user);
			return user;
		}

		public void Remove(int id)
		{
			throw new NotImplementedException();
		}

		public Picture GetUserInformatiom(int id)
		{
			using (var multipleResults = this._db.QueryMultiple("GetUserByID", new { Id = id }, commandType: CommandType.StoredProcedure))
			{
				var user = multipleResults.Read<Picture>().SingleOrDefault();

				//var addresses = multipleResults.Read<Address>().ToList();
				//if (user != null && addresses != null)
				//{
				//user.Address.AddRange(addresses);
				//}

				return user;
			}
		}

		private class PictureLocal
		{
			public int Id { get; set; }
			public string MimeType { get; set; }
			public string SeoFilename { get; set; }
			public bool IsNew { get; set; }
			public int ProductVariantId { get; set; }
			public int ProductId { get; set; }
			public int DisplayOrder { get; set; }		
		}
	}
}