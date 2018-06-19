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
using Nop.Core.Domain.News;

namespace AlternativeDataAccess
{
	public class NewsRepository
	{
		private IDbConnection _db = new SqlConnection(new DataSettingsManager().LoadSettings().DataConnectionString);

		public List<dynamic> NewItemsBySystemType(int languageId, NewsType systemTypeId, int top, NewsItemPictureType newsItemPictureType = NewsItemPictureType.Standard)
		{
			string sql = @"select top(@top) News.Id NewsItemId, News.Url NewsItemUrl, News.MetaDescription NewsItemMetaDescription, Title, Short, SeName, Picture.Id PictureId, Picture.SeoFilename PictureSeoFilename
							from News, News_Picture_Mapping, Picture
							where LanguageId = @languageId and Published = 1 and SystemTypeId = @systemTypeId
							and News.Id = NewsItemId
							and News_Picture_Mapping.PictureId = Picture.Id
							and News_Picture_Mapping.DisplayOrder = 0
							and NewsItemPictureTypeId = @newsItemPictureType
							order by CreatedOnUtc desc";

			var mapped = _db.Query(sql, new { languageId = languageId, systemTypeId = systemTypeId, top = top, newsItemPictureType = newsItemPictureType });
			return mapped.ToList();
		}
	}
}