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
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.News;

namespace AlternativeDataAccess
{
	public class LocalizedPropertyRepository
	{
		private IDbConnection _db = new SqlConnection(new DataSettingsManager().LoadSettings().DataConnectionString);

		public List<LocalizedProperty> All()
		{
			string sql = @"SELECT Id, EntityId, LanguageId, LocaleKeyGroup, LocaleKey, LocaleValue FROM LocalizedProperty";

			var mapped = _db.Query<LocalizedProperty>(sql);
			return mapped.ToList();
		}
	}
}