using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Kickstart.Interface;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Utility;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using Kickstart.Pass2.DataStoreProject.Postgres;

namespace Kickstart.Pass2.SqlServer
{
    public class CStoredProcedureToSSqlServerStoredProcedureConverter : ICStoredProcedureToStoredProcedureConverter
    {
 
        public string Convert(CStoredProcedure storedProcedure)
        {
            var converter = new CStoredProcedureToCreateProcedureStatementConverter();
            var createStoredProcedure = converter.Convert(storedProcedure);

            //todo: this should be done when creating the CStoredProcedure.StoredProcBody
            //var snakeCaseVisitor = new SqlServerSnakeCaseVisitor();
            //createStoredProcedure.Accept(snakeCaseVisitor);

            string scriptOut;
            var scriptGen = new Sql120ScriptGenerator();

            scriptGen.GenerateScript(createStoredProcedure, out scriptOut);
            return scriptOut;
        }

        private void RaiseSqlErrorsException(IList<ParseError> sqlParseErrors, string sql)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Error(s) parsing script:");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(sql);
            stringBuilder.AppendLine();

            foreach (var error in sqlParseErrors)
                stringBuilder.AppendLine($"Line ({error.Line}) {error.Message}");
            throw new ApplicationException(stringBuilder.ToString());
        }

       
    }
}