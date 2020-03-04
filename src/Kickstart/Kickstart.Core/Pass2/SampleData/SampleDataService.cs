using System;
using System.Collections.Generic;
using System.Data;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Pass2.CModel.Proto;
using Kickstart.Utility;

namespace Kickstart.Pass2.SampleData
{
    internal class SampleDataService
    {
        private static readonly Dictionary<string, string> _sampleData = new Dictionary<string, string>();

        public static string GetSampleData(CProtoMessageField childField, COperationIs operationIs)
        {
            var dataType = SqlMapper.SqlDbTypeToDbType(SqlMapper.GrpcTypeToSqlDbType(childField.FieldType));
            var isIdentity = false;
            var isForeignKey = false;

            if (childField.DerivedFrom is CColumn)
            {
                var column = childField.DerivedFrom as CColumn;
                isIdentity = column.IsIdentity;
                dataType = column.ColumnType;
            }
            else if (childField.DerivedFrom is CStoredProcedureParameter)
            {
                var parameter = childField.DerivedFrom as CStoredProcedureParameter;
                isIdentity = parameter.SourceColumn.IsIdentity;
                isForeignKey = parameter.SourceColumn.ForeignKeyColumn.Count > 0;
                dataType = parameter.ParameterType;
            }


            if (isForeignKey && dataType == DbType.Int64)
                return "_currentDbIdentityValue";
            if (isForeignKey && dataType == DbType.Int32)
                return "_currentDbIdentityValue";

            if (isIdentity)
                if (operationIs == COperationIs.Update)
                    return "_currentDbIdentityValue";
                else
                    return "0";

            if (_sampleData.ContainsKey(childField.FieldName))
                return LookupSample(childField);

            var sampleData = GenerateSampleData(dataType);
            _sampleData.Add(childField.FieldName, sampleData);
            return sampleData;
        }

        private static string LookupSample(CProtoMessageField childField)
        {
            return _sampleData[childField.FieldName];
        }

        private static string GenerateSampleData(DbType dataType)
        {
            switch (dataType)
            {
                case DbType.Xml:
                    return @"""<tag>test</tag>""";
                case DbType.AnsiStringFixedLength:
                case DbType.AnsiString:
                case DbType.String:
                case DbType.StringFixedLength:
                    return $@"""{Guid.NewGuid().ToString().Replace("-","").ToUpper()}""";
                    ;
                case DbType.Int32:
                {
                    return new Random().Next().ToString();
                }
                case DbType.Int64:
                {
                    return new Random().Next().ToString();
                }
                case DbType.Currency:
                case DbType.Decimal:

                    return $@"""1.0""";
                case DbType.Binary:
                    return "Google.Protobuf.ByteString.Empty";
                case DbType.Byte:
                    return new Random().Next().ToString();
                case DbType.DateTime2:
                case DbType.DateTime:
                case DbType.Date:
                case DbType.Time:
                case DbType.DateTimeOffset:
                    return $@"Timestamp.FromDateTime(DateTime.SpecifyKind(  DateTime.Parse(""{
                            DateTime.UtcNow.ToString("o")
                        }""), DateTimeKind.Utc))";
                case DbType.Boolean:
                    return $@"true";
            }
            return "null";
        }
    }
}