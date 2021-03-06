using Kickstart.Pass2.CModel.DataStore;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace Kickstart.Pass2.SqlServer
{
    public class CSchemaToSqlServerSchemaConverter : ICSchemaToSchemaConverter
    {
        #region Methods

        public string Convert(CSchema schema)
        {
            string[] parts = {schema.SchemaName};

            var createSchemaStatement = new CreateSchemaStatement();
            createSchemaStatement.Name = new Identifier {Value = schema.SchemaName};

            //generate DDL
            var script = new TSqlScript();
            var batch = new TSqlBatch();
            script.Batches.Add(batch);
            batch.Statements.Add(createSchemaStatement);
            var dacpacModel = new TSqlModel(SqlServerVersion.Sql120, new TSqlModelOptions());
            var existing = dacpacModel.GetObject(Schema.TypeClass, new ObjectIdentifier(parts), DacQueryScopes.All);
            if (existing != null)
                return existing.GetScript();
            dacpacModel.AddObjects(script);
            existing = dacpacModel.GetObject(Schema.TypeClass, new ObjectIdentifier(parts), DacQueryScopes.All);
            return existing.GetScript();
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