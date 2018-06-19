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
	public class Specification
	{
		private IDbConnection _db = new SqlConnection(new DataSettingsManager().LoadSettings().DataConnectionString);

		public SpecificationAttribute GetSpecificationAttributeById(int id)
		{
			return this._db.Query<SpecificationAttribute>("SELECT Id, Name, DisplayOrder FROM SpecificationAttribute WHERE Id = @id", new { id }).SingleOrDefault();
		}

		public SpecificationAttributeOption GetSpecificationAttributeOptionById(int id)
		{
			return this._db.Query<SpecificationAttributeOption>("SELECT Id, SpecificationAttributeId, Name, DisplayOrder FROM SpecificationAttributeOption WHERE Id = @id", new { id }).SingleOrDefault();
		}
	}
}