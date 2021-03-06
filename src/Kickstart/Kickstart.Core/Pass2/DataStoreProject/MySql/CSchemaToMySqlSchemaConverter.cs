using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Utility;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace Kickstart.Pass2.SqlServer
{
    public class CSchemaToMySqlSchemaConverter
    {
        #region Methods

        public string Convert(CSchema schema)
        {
            return $@"CREATE SCHEMA IF NOT EXISTS {schema.SchemaName.WrapReservedAndSnakeCase(DataStoreTypes.MySql, schema.ConvertToSnakeCase)};";
        }

        #endregion Methods

        #region Fields

        #endregion Fields

        #region Properties

        #endregion Properties

        #region Constructors

        #endregion Constructors
    }
}