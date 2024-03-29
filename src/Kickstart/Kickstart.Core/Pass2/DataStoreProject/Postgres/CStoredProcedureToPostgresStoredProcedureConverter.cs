using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Kickstart.Interface;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Pass2.DataStoreProject.Postgres;
using Kickstart.Pass2.SampleData;
using Kickstart.Utility;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace Kickstart.Pass2.SqlServer
{
    public class CStoredProcedureToPostgresStoredProcedureConverter: ICStoredProcedureToStoredProcedureConverter
    {
        #region Fields

        #endregion Fields

        #region Properties

        #endregion Properties

        #region Constructors

        #endregion Constructors

        #region Methods

        public string Convert(CStoredProcedure storedProcedure)
        {
            var converter = new CStoredProcedureToCreateProcedureStatementConverter();
            var createStoredProcedure = converter.Convert(storedProcedure);
            
            var snakeCaseVisitor = new SnakeCaseVisitor();
            createStoredProcedure.Accept(snakeCaseVisitor);


            var codeWriter = new CodeWriter();
            codeWriter.WriteLine($@"CREATE OR REPLACE FUNCTION {storedProcedure.Schema.SchemaName.WrapReservedAndSnakeCase(storedProcedure.DatabaseType, storedProcedure.ConvertToSnakeCase)}.{storedProcedure.StoredProcedureName.WrapReservedAndSnakeCase(storedProcedure.DatabaseType, storedProcedure.ConvertToSnakeCase)} ");
            codeWriter.WriteLine($@"(");
            bool first = true;
            foreach (var parameter in storedProcedure.Parameter)
            {
                if (!first)
                    codeWriter.Write(", ");
                first = false;

                var parameterType = string.Empty;
                if (parameter.ParameterTypeIsUserDefined)
                {
                    if (parameter.ParameterTypeRaw == "sysname")
                    {
                        //fixup. Todo: implement a better way
                        parameterType = SqlMapper.NpgsqlDbTypeToPostgres(NpgsqlTypes.NpgsqlDbType.Varchar);
                        parameter.ParameterLength = 128;
                    }
                    else
                    {
                        //todo: need schema included
                        parameterType = $"{parameter.ParameterTypeRawSchema.WrapReservedAndSnakeCase(storedProcedure.DatabaseType, storedProcedure.ConvertToSnakeCase)}.{parameter.ParameterTypeRaw.WrapReservedAndSnakeCase(storedProcedure.DatabaseType, storedProcedure.ConvertToSnakeCase)} []";
                    }
                }
                else
                {
                    parameterType = SqlMapper.NpgsqlDbTypeToPostgres(SqlMapper.DbTypeToNpgsqlDbType(parameter.ParameterType));
                }
                //todo: remove p_ prefix. Too much UI change, better to manually fix the stored procs
                var parameterName = "p_" + parameter.ParameterName;

                codeWriter.Write($@"{parameterName.WrapReservedAndSnakeCase(storedProcedure.DatabaseType, storedProcedure.ConvertToSnakeCase)} {parameterType} ");
                if (parameter.DoesNeedLength())
                {
                    codeWriter.Write($"({parameter.ParameterLength})");
                }

            }
            codeWriter.WriteLine();
            codeWriter.WriteLine($@")");
            if (storedProcedure.ResultSet.Count > 0)
            {
                codeWriter.Write($@"RETURNS TABLE (");

                {

                    var first2 = true;
                    foreach (var resultCol in storedProcedure.ResultSet)
                    {
                        if (!first2)
                            codeWriter.WriteLine(",");
                        first2 = false;

                        codeWriter.Write($"{resultCol.ColumnName.WrapReservedAndSnakeCase(storedProcedure.DatabaseType, storedProcedure.ConvertToSnakeCase)}");

                        codeWriter.Write($" {SqlMapper.NpgsqlDbTypeToPostgres(SqlMapper.DbTypeToNpgsqlDbType(resultCol.ColumnType))}");
                        if (resultCol.DoesNeedLength())
                        {
                            codeWriter.Write($"({resultCol.ColumnLength})");
                        }

                        if (!resultCol.IsNullable)
                        {
                            codeWriter.Write(" NOT NULL");
                        }
                    }
                }
                codeWriter.Write($@") ");

            }
            else
            {
                codeWriter.Write("RETURNS void ");
            }
            codeWriter.WriteLine($@"AS $func$");

            codeWriter.WriteLine($@"BEGIN");
            codeWriter.Indent();
            if (storedProcedure.ResultSet.Count > 0)
            {
                codeWriter.WriteLine($@"RETURN QUERY");
            }

            
            codeWriter.WriteLine("--TODO: Manually convert the sql below to Postgresql");

            if (storedProcedure.ResultSet.Count > 0)
            {
                codeWriter.Write("SELECT ");
                var first3 = true;
                var sampleDataService = new SamplePostgresDataService();
                foreach (var resultCol in storedProcedure.ResultSet)
                {
                    if (!first3)
                        codeWriter.Write(",");
                    first3 = false;

                    var npgsqlType = SqlMapper.DbTypeToNpgsqlDbType(resultCol.ColumnType);
                    var postgresType = SqlMapper.NpgsqlDbTypeToPostgres(npgsqlType);
                    codeWriter.Write($"CAST ({sampleDataService.GetSampleData(npgsqlType, resultCol.ColumnLength)} AS {postgresType} ) AS {resultCol.ColumnName.WrapReservedAndSnakeCase(DataStoreTypes.Postgres, storedProcedure.ConvertToSnakeCase)}");
                }


                codeWriter.WriteLine(";");
            }
            codeWriter.WriteLine("/*");
            //codeWriter.WriteLine(storedProcedure.StoredProcedureBody);

            
            var scriptGen = new Sql120ScriptGenerator();


            foreach (var statement2 in createStoredProcedure.StatementList.Statements)
            {
                string scriptOut;
                scriptGen.GenerateScript(statement2, out scriptOut);

                codeWriter.WriteLine(scriptOut);
            }
            
            codeWriter.WriteLine("*/");
            codeWriter.Unindent();
            codeWriter.WriteLine($@"END");
            codeWriter.WriteLine($@"$func$ LANGUAGE plpgsql;");
            return codeWriter.ToString();
        }

     

        #endregion Methods
    }
}