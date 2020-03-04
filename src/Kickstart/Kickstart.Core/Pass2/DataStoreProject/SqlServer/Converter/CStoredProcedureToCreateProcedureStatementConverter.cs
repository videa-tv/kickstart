using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Utility;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickstart.Pass2.SqlServer
{
    class CStoredProcedureToCreateProcedureStatementConverter
    {
        public CreateProcedureStatement Convert(CStoredProcedure storedProcedure  )
        {
            //build body

            string[] parts = { storedProcedure.Schema.SchemaName, storedProcedure.StoredProcedureName };

            var createStoredProcedure = new CreateProcedureStatement();

            ///set schema and table name
            ///
            var schemaObjectName = new SchemaObjectName();
            schemaObjectName.Identifiers.Add(new Identifier { Value = storedProcedure.Schema.SchemaName });
            schemaObjectName.Identifiers.Add(new Identifier { Value = storedProcedure.StoredProcedureName });

            createStoredProcedure.ProcedureReference = new ProcedureReference();
            createStoredProcedure.ProcedureReference.Name = schemaObjectName;

            //add parameters


            foreach (var param in storedProcedure.Parameter)
                if (param.ParameterTypeIsUserDefined)
                {
                    var dataType = new UserDataTypeReference();
                    dataType.Name = new SchemaObjectName();
                    dataType.Name.Identifiers.Add(new Identifier { Value = param.ParameterTypeRaw });
                    if (param.ParameterLength > 0)
                        dataType.Parameters.Add(new IntegerLiteral { Value = param.ParameterLength.ToString() });

                    createStoredProcedure.Parameters.Add(new ProcedureParameter
                    {
                        VariableName = new Identifier { Value = $"@{param.ParameterName}" },
                        DataType = dataType,
                        Value = param.DefaultToNull ? new NullLiteral() : null,
                        Modifier = param.ParameterTypeRaw.ToLower() == "sysname" ? ParameterModifier.None: ParameterModifier.ReadOnly //todo
                    });
                }
                else
                {
                    var dataType = new SqlDataTypeReference();
                    var parameterName = param.ParameterName;
                    if (param.IsCollection)
                    {
                        parameterName += "_Collection";// temp solution for comma separate collection parameters
                    }
                    if (param.ParameterTypeRaw == "enum")
                    {
                        dataType.SqlDataTypeOption = SqlDataTypeOption.Int; //todo: review this
                    }
                    else
                    {
                        dataType.SqlDataTypeOption = SqlMapper.SqlTypeToSqlDataTypeOption(param.ParameterTypeRaw);
                    }

                    if (param.ParameterLength > 0)
                        dataType.Parameters.Add(new IntegerLiteral { Value = param.ParameterLength.ToString() });

                    createStoredProcedure.Parameters.Add(new ProcedureParameter
                    {
                        VariableName = new Identifier { Value = $"@{parameterName}" },
                        DataType = dataType,
                        Value = param.DefaultToNull ? new NullLiteral() : null
                    });
                }

            var parser = new TSql120Parser(false);

            createStoredProcedure.StatementList = new StatementList();
            if (!string.IsNullOrEmpty(storedProcedure.StoredProcedureBody))
            {
                IList<ParseError> errors;
                var script2 =
                    parser.Parse(new StringReader(storedProcedure.StoredProcedureBody), out errors) as TSqlScript;
                if (errors.Count > 0)
                    RaiseSqlErrorsException(errors, storedProcedure.StoredProcedureBody);
                foreach (var batch2 in script2.Batches)
                    foreach (var statement in batch2.Statements)
                        createStoredProcedure.StatementList.Statements.Add(statement);
            }

            return createStoredProcedure;
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
