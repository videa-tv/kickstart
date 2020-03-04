using System;
using System.Collections.Generic;
using System.Data;
using Kickstart.Pass2.CModel;
using Kickstart.Pass2.CModel.AWS.DMS;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using MySql.Data.MySqlClient;
using NpgsqlTypes;
using Int32 = System.Int32;

namespace Kickstart.Utility
{
    public static class SqlMapper
    {
        private static readonly Dictionary<DbType, Type> DbTypeMapToNullableType = new Dictionary<DbType, Type>
        {
            {DbType.Byte, typeof(byte?)},
            {DbType.SByte, typeof(sbyte?)},
            {DbType.Int16, typeof(short?)},
            {DbType.UInt16, typeof(ushort?)},
            {DbType.Int32, typeof(int?)},
            {DbType.UInt32, typeof(uint?)},
            {DbType.Int64, typeof(long?)},
            {DbType.UInt64, typeof(ulong?)},
            {DbType.Single, typeof(float?)},
            {DbType.Double, typeof(double?)},
            {DbType.Decimal, typeof(decimal?)},
            {DbType.Boolean, typeof(bool?)},
            {DbType.StringFixedLength, typeof(char?)},
            {DbType.Guid, typeof(Guid?)},
            {DbType.DateTime, typeof(DateTime?)},
            {DbType.DateTimeOffset, typeof(DateTimeOffset?)},
            {DbType.Binary, typeof(byte[])}
        };

        private static readonly Dictionary<DbType, Type> DbTypeMapToType = new Dictionary<DbType, Type>
        {
            {DbType.Byte, typeof(byte)},
            {DbType.AnsiString, typeof(string)},
            {DbType.AnsiStringFixedLength, typeof(char[])},
            {DbType.SByte, typeof(sbyte)},
            {DbType.Int16, typeof(short)},
            {DbType.UInt16, typeof(ushort)},
            {DbType.Int32, typeof(int)},
            {DbType.UInt32, typeof(uint)},
            {DbType.Int64, typeof(long)},
            {DbType.UInt64, typeof(ulong)},
            {DbType.Single, typeof(float)},
            {DbType.Double, typeof(double)},
            {DbType.Decimal, typeof(decimal)},
            {DbType.Currency, typeof(decimal)},
            {DbType.Boolean, typeof(bool)},
            {DbType.String, typeof(string)},
            {DbType.StringFixedLength, typeof(char[])},
            {DbType.Guid, typeof(Guid)},
            {DbType.DateTime, typeof(DateTime)},
            {DbType.DateTime2, typeof(DateTime)},
            {DbType.Date, typeof(DateTime)},
            {DbType.Time, typeof(DateTime)},
            {DbType.Xml, typeof(string)},
            {DbType.DateTimeOffset, typeof(DateTimeOffset)},
            {DbType.Object, typeof(Object) },
            {DbType.Binary, typeof(byte[])}
        };

        private static readonly Dictionary<SqlDbType, Type> SqlDbTypeToNullableType = new Dictionary<SqlDbType, Type>
        {
            {SqlDbType.BigInt, typeof(long?)},
            {SqlDbType.Binary, typeof(byte[])},
            {SqlDbType.Image, typeof(byte[])},
            {SqlDbType.Timestamp, typeof(byte[])},
            {SqlDbType.VarBinary, typeof(byte[])},
            {SqlDbType.Bit, typeof(bool?)},
            {SqlDbType.Char, typeof(string)},
            {SqlDbType.NChar, typeof(string)},
            {SqlDbType.NText, typeof(string)},
            {SqlDbType.NVarChar, typeof(string)},
            {SqlDbType.Text, typeof(string)},
            {SqlDbType.VarChar, typeof(string)},
            {SqlDbType.Xml, typeof(string)},
            {SqlDbType.DateTime, typeof(DateTime?)},
            {SqlDbType.SmallDateTime, typeof(DateTime?)},
            {SqlDbType.Date, typeof(DateTime?)},
            {SqlDbType.Time, typeof(DateTime?)},
            {SqlDbType.DateTime2, typeof(DateTime?)},
            {SqlDbType.Decimal, typeof(decimal?)},
            {SqlDbType.Money, typeof(decimal?)},
            {SqlDbType.SmallMoney, typeof(decimal?)},
            {SqlDbType.Float, typeof(double?)},
            {SqlDbType.Int, typeof(int?)},
            {SqlDbType.Real, typeof(float?)},
            {SqlDbType.UniqueIdentifier, typeof(Guid?)},
            {SqlDbType.SmallInt, typeof(short?)},
            {SqlDbType.TinyInt, typeof(byte?)},
            {SqlDbType.Variant, typeof(object)},
            {SqlDbType.Udt, typeof(object)},
            {SqlDbType.Structured, typeof(DataTable)},
            {SqlDbType.DateTimeOffset, typeof(DateTimeOffset)}
        };

        private static readonly Dictionary<SqlDbType, Type> SqlDbTypeToType = new Dictionary<SqlDbType, Type>
        {
            {SqlDbType.BigInt, typeof(long)},
            {SqlDbType.Binary, typeof(byte[])},
            {SqlDbType.Image, typeof(byte[])},
            {SqlDbType.Timestamp, typeof(byte[])},
            {SqlDbType.VarBinary, typeof(byte[])},
            {SqlDbType.Bit, typeof(bool)},
            {SqlDbType.Char, typeof(string)},
            {SqlDbType.NChar, typeof(string)},
            {SqlDbType.NText, typeof(string)},
            {SqlDbType.NVarChar, typeof(string)},
            {SqlDbType.Text, typeof(string)},
            {SqlDbType.VarChar, typeof(string)},
            {SqlDbType.Xml, typeof(string)},
            {SqlDbType.DateTime, typeof(DateTime)},
            {SqlDbType.SmallDateTime, typeof(DateTime)},
            {SqlDbType.Date, typeof(DateTime)},
            {SqlDbType.Time, typeof(DateTime)},
            {SqlDbType.DateTime2, typeof(DateTime)},
            {SqlDbType.Decimal, typeof(decimal)},
            {SqlDbType.Money, typeof(decimal)},
            {SqlDbType.SmallMoney, typeof(decimal)},
            {SqlDbType.Float, typeof(double)},
            {SqlDbType.Int, typeof(int)},
            {SqlDbType.Real, typeof(float)},
            {SqlDbType.UniqueIdentifier, typeof(Guid)},
            {SqlDbType.SmallInt, typeof(short)},
            {SqlDbType.TinyInt, typeof(byte)},
            {SqlDbType.Variant, typeof(object)},
            {SqlDbType.Udt, typeof(object)},
            {SqlDbType.Structured, typeof(DataTable)},
            {SqlDbType.DateTimeOffset, typeof(DateTimeOffset)}
        };

        private static readonly Dictionary<Type, DbType> TypeToDbType = new Dictionary<Type, DbType>
        {
            {typeof(byte), DbType.Byte},
            {typeof(sbyte), DbType.SByte},
            {typeof(short), DbType.Int16},
            {typeof(ushort), DbType.UInt16},
            {typeof(int), DbType.Int32},
            {typeof(uint), DbType.UInt32},
            {typeof(long), DbType.Int64},
            {typeof(ulong), DbType.UInt64},
            {typeof(float), DbType.Single},
            {typeof(double), DbType.Double},
            {typeof(decimal), DbType.Decimal},
            {typeof(bool), DbType.Boolean},
            {typeof(string), DbType.String},
            {typeof(char), DbType.StringFixedLength},
            {typeof(char[]), DbType.StringFixedLength},
            {typeof(Guid), DbType.Guid},
            {typeof(DateTime), DbType.DateTime},
            {
                typeof(DateTimeOffset),
                DbType.DateTimeOffset
            },
            {typeof(byte[]), DbType.Binary},
            {typeof(byte?), DbType.Byte},
            {typeof(sbyte?), DbType.SByte},
            {typeof(short?), DbType.Int16},
            {typeof(ushort?), DbType.UInt16},
            {typeof(int?), DbType.Int32},
            {typeof(uint?), DbType.UInt32},
            {typeof(long?), DbType.Int64},
            {typeof(ulong?), DbType.UInt64},
            {typeof(float?), DbType.Single},
            {typeof(double?), DbType.Double},
            {typeof(decimal?), DbType.Decimal},
            {typeof(bool?), DbType.Boolean},
            {typeof(char?), DbType.StringFixedLength},
            {typeof(Guid?), DbType.Guid},
            {typeof(DateTime?), DbType.DateTime},
            {
                typeof(DateTimeOffset?),
                DbType.DateTimeOffset
            },
            {typeof(Object), DbType.Object }
        };

        private static readonly Dictionary<Type, string> Aliases = new Dictionary<Type, string>
        {
            {typeof(char[]), "char[]"},
            {typeof(byte[]), "byte[]"},
            {typeof(byte), "byte"},
            {typeof(sbyte), "sbyte"},
            {typeof(short), "short"},
            {typeof(ushort), "ushort"},
            {typeof(int), "int"},
            {typeof(uint), "uint"},
            {typeof(long), "long"},
            {typeof(ulong), "ulong"},
            {typeof(float), "float"},
            {typeof(double), "double"},
            {typeof(decimal), "decimal"},
            {typeof(object), "object"},
            {typeof(bool), "bool"},
            {typeof(char), "char"},
            {typeof(string), "string"},
            {typeof(void), "void"},
            {typeof(DateTime), "DateTime"},
            {typeof(byte?), "byte?"},
            {typeof(sbyte?), "sbyte?"},
            {typeof(short?), "short?"},
            {typeof(ushort?), "ushort?"},
            {typeof(int?), "int?"},
            {typeof(uint?), "uint?"},
            {typeof(long?), "long?"},
            {typeof(ulong?), "ulong?"},
            {typeof(float?), "float?"},
            {typeof(double?), "double?"},
            {typeof(decimal?), "decimal?"},
            {typeof(bool?), "bool?"},
            {typeof(char?), "char?"}
        };

        public static Type ToClrType(object columnType)
        {
            throw new NotImplementedException();
        }

        public static Type ToClrType(this DbType dbType)
        {
            Type type;
            if (DbTypeMapToType.TryGetValue(dbType, out type))
                return type;

            throw new ArgumentOutOfRangeException("dbType", dbType, "Cannot map the DbType to Type");
        }

        //return alias for scaler types
        public static string ToClrTypeName(this SqlDbType sqlDbType)
        {
            var type = ToClrType(sqlDbType);
            if (Aliases.ContainsKey(type))
                return Aliases[type];
            return type.Name;
        }

        public static string ToClrTypeName(this DbType dbType)
        {
            var type = ToClrType(dbType);
            if (type == typeof(Char[]))
            {
                return "string";
            }
            if (Aliases.ContainsKey(type))
                return Aliases[type];
            return type.Name;
        }

        public static string ToClrTypeName(this Type type)
        {
            if (Aliases.ContainsKey(type))
                return Aliases[type];
            return type.Name;
        }

        public static Type ToClrType(this SqlDbType sqlDbType)
        {
            Type type;
            if (SqlDbTypeToType.TryGetValue(sqlDbType, out type))
                return type;

            throw new ArgumentOutOfRangeException("sqlDbType", sqlDbType, "Cannot map the SqlDbType to Type");
        }

        public static DbType ToDbType(this Type type)
        {
            DbType dbType;
            if (TypeToDbType.TryGetValue(type, out dbType))
                return dbType;

            throw new ArgumentOutOfRangeException("type", type, "Cannot map the Type to DbType");
        }

        public static Type ToNullableClrType(this DbType dbType)
        {
            Type type;
            if (DbTypeMapToNullableType.TryGetValue(dbType, out type))
                return type;

            throw new ArgumentOutOfRangeException("dbType", dbType, "Cannot map the DbType to Nullable Type");
        }

        public static Type ToNullableClrType(this SqlDbType sqlDbType)
        {
            Type type;
            if (SqlDbTypeToNullableType.TryGetValue(sqlDbType, out type))
                return type;

            throw new ArgumentOutOfRangeException("sqlDbType", sqlDbType, "Cannot map the SqlDbType to Nullable Type");
        }

        public static DbType SqlDbTypeToDbType(SqlDbType sqlDbType)
        {
            switch (sqlDbType)
            {
                case SqlDbType.BigInt:
                    return DbType.Int64;
                case SqlDbType.Binary:
                    return DbType.Binary;
                case SqlDbType.Bit:
                    return DbType.Boolean;
                case SqlDbType.Char:
                    return DbType.AnsiStringFixedLength;
                case SqlDbType.Date:
                    return DbType.Date;
                case SqlDbType.DateTime:
                    return DbType.DateTime;
                case SqlDbType.DateTime2:
                    return DbType.DateTime2;
                case SqlDbType.DateTimeOffset:
                    return DbType.DateTimeOffset;
                case SqlDbType.Decimal:
                    return DbType.Decimal;
                case SqlDbType.Float:
                    return DbType.Single;
                case SqlDbType.Image:
                    return DbType.Binary;
                case SqlDbType.Int:
                    return DbType.Int32;
                case SqlDbType.Money:
                    return DbType.Currency;
                case SqlDbType.NChar:
                    return DbType.StringFixedLength;
                case SqlDbType.NText:
                    return DbType.String; //???
                case SqlDbType.NVarChar:
                    return DbType.String;
                case SqlDbType.Real:
                    return DbType.Double;
                case SqlDbType.SmallDateTime:
                    return DbType.DateTime;
                case SqlDbType.SmallInt:
                    return DbType.Int16;
                case SqlDbType.SmallMoney:
                    return DbType.Currency;
                case SqlDbType.Text:
                    return DbType.String;
                case SqlDbType.Time:
                    return DbType.Time;
                case SqlDbType.Timestamp:
                    return DbType.Binary; //argh! no direct mapping
                case SqlDbType.TinyInt:
                    return DbType.Byte;
                case SqlDbType.UniqueIdentifier:
                    return DbType.Guid;
                case SqlDbType.VarChar:
                    return DbType.AnsiString;
                case SqlDbType.Xml:
                    return DbType.Xml;
                case SqlDbType.VarBinary:
                    return DbType.Binary;
                case SqlDbType.Structured:
                    return DbType.Object;
                default:
                    throw new NotImplementedException();
            }
        }

        public static SqlDbType DbTypeToSqlDbType(DbType dbType)
        {
            switch (dbType)
            {
                case DbType.Int64:
                    return SqlDbType.BigInt;
                case DbType.Binary:
                    return SqlDbType.Binary;
                case DbType.Boolean:
                    return SqlDbType.Bit;
                case DbType.AnsiStringFixedLength:
                    return SqlDbType.Char;
                case DbType.Date:
                    return SqlDbType.Date;
                case DbType.DateTime:
                    return SqlDbType.DateTime;
                case DbType.DateTime2:
                    return SqlDbType.DateTime2;
                case DbType.DateTimeOffset:
                    return SqlDbType.DateTimeOffset;
                case DbType.Decimal:
                    return SqlDbType.Decimal;
                case DbType.Single:
                    return SqlDbType.Float;
                //case DbType.Binary:
                //    return SqlDbType.Image;
                case DbType.Int32:
                    return SqlDbType.Int;
                case DbType.Currency:
                    return SqlDbType.Money;
                case DbType.StringFixedLength:
                    return SqlDbType.NChar;
                //case DbType.String:
                //   return SqlDbType.NText; //???
                case DbType.String:
                    return SqlDbType.NVarChar;
                case DbType.Double:
                    return SqlDbType.Real;
                //case DbType.DateTime:
                //    return SqlDbType.SmallDateTime;
                case DbType.Int16:
                    return SqlDbType.SmallInt;
                //case DbType.Currency:
                //    return SqlDbType.SmallMoney;
                //case DbType.AnsiString:
                //  return SqlDbType.Text;
                case DbType.Time:
                    return SqlDbType.Time;
                //case DbType.DateTime2:
                //  return SqlDbType.Timestamp;
                case DbType.Byte:
                    return SqlDbType.TinyInt;
                case DbType.Guid:
                    return SqlDbType.UniqueIdentifier;
                case DbType.AnsiString:
                    return SqlDbType.VarChar;

                default:
                    throw new NotImplementedException();
            }
        }


        public static NpgsqlDbType DbTypeToNpgsqlDbType(DbType dbType)
        {
            switch (dbType)
            {
                case DbType.Int64:
                    return NpgsqlDbType.Bigint;
                case DbType.Binary:
                    return NpgsqlDbType.Bytea;
                case DbType.Boolean:
                    return NpgsqlDbType.Boolean;
                case DbType.AnsiStringFixedLength:
                    return NpgsqlDbType.Char;
                case DbType.Date:
                    return NpgsqlDbType.Date;
                case DbType.DateTime:
                    return NpgsqlDbType.Timestamp;
                case DbType.DateTime2:
                    return NpgsqlDbType.Timestamp;
                case DbType.DateTimeOffset:
                    return NpgsqlDbType.TimestampTZ;
                case DbType.Decimal:
                    return NpgsqlDbType.Numeric;
                case DbType.Single:
                    return NpgsqlDbType.Real;
                //case DbType.Binary:
                //    return NpgsqlDbType.Image;
                case DbType.Int32:
                    return NpgsqlDbType.Integer;
                case DbType.Currency:
                    return NpgsqlDbType.Money;
                case DbType.StringFixedLength:
                    return NpgsqlDbType.Text;
                //case DbType.String:
                //   return NpgsqlDbType.NText; //???
                case DbType.String:
                    return NpgsqlDbType.Text;
                case DbType.Double:
                    return NpgsqlDbType.Real;
                //case DbType.DateTime:
                //    return NpgsqlDbType.SmallDateTime;
                case DbType.Int16:
                    return NpgsqlDbType.Smallint;
                //case DbType.Currency:
                //    return NpgsqlDbType.SmallMoney;
                //case DbType.AnsiString:
                //  return NpgsqlDbType.Text;
                case DbType.Time:
                    return NpgsqlDbType.Time;
                //case DbType.DateTime2:
                //  return NpgsqlDbType.Timestamp;
                case DbType.Byte:
                    return NpgsqlDbType.Smallint;
                case DbType.Guid:
                    return NpgsqlDbType.Uuid;
                case DbType.AnsiString:
                    return NpgsqlDbType.Varchar;
                case DbType.Xml:
                    return NpgsqlDbType.Xml;
                default:
                    throw new NotImplementedException();
            }
        }

        public static string NpgsqlDbTypeToPostgres(NpgsqlDbType dbType)
        {
            switch (dbType)
            {
                case NpgsqlDbType.Bigint:
                    return "bigint";
                //case NpgsqlDbType.Binary:
                case NpgsqlDbType.Boolean:
                    return "boolean";
                case NpgsqlDbType.Bit:
                    return "bit";
                case NpgsqlDbType.Char:
                    return "character";
                case NpgsqlDbType.Date:
                    return "date";
                case NpgsqlDbType.Timestamp:
                    return "timestamp";
                case NpgsqlDbType.TimestampTZ:
                    return "timestamp with time zone";
                case NpgsqlDbType.Numeric:
                    return "numeric";
                case NpgsqlDbType.Real:
                    return "real";
                //case DbType.Binary:
                //    case NpgsqlDbType.Image:
                case NpgsqlDbType.Integer:
                    return "integer";
                case NpgsqlDbType.Money:
                    return "money";
                case NpgsqlDbType.Text:
                    return "varchar"; //varchar can work as varchar, varchar(n) or text http://www.postgresqltutorial.com/postgresql-char-varchar-text/
                //   case NpgsqlDbType.NText: //???
                //case NpgsqlDbType.SmallDateTime:
                case NpgsqlDbType.Smallint:
                    return "smallint";
                //    case NpgsqlDbType.SmallMoney:
                //  case NpgsqlDbType.Text:
                case NpgsqlDbType.Time:
                    return "time";
                case NpgsqlDbType.TimeTZ:
                    return "time with time zone";
                //  case NpgsqlDbType.Timestamp:
                case NpgsqlDbType.Uuid:
                    return "uuid";
                case NpgsqlDbType.Varchar:
                    return "character varying";
                case NpgsqlDbType.Double:
                    return "double precision";
                case NpgsqlDbType.Bytea:
                    return "bytea";
                case NpgsqlDbType.Xml:
                    return "xml";
                default:
                    throw new NotImplementedException();
            }
        }

        public static MySqlDbType DbTypeToMySqlDbType(DbType dbType)
        {
            switch (dbType)
            {
                case DbType.Int64:
                    return MySqlDbType.Int64;
                case DbType.Binary:
                    throw new NotImplementedException();//return MySqlDbType.Binary;
                case DbType.Boolean:
                    return MySqlDbType.Bit;
                case DbType.AnsiStringFixedLength:
                    return MySqlDbType.String;
                case DbType.Date:
                    return MySqlDbType.Date;
                case DbType.DateTime:
                    return MySqlDbType.DateTime;
                case DbType.DateTime2:
                    return MySqlDbType.DateTime;
                //case DbType.DateTimeOffset:
                //    return MySqlDbType.TimestampTZ;
                case DbType.Decimal:
                    return MySqlDbType.Decimal;
                case DbType.Single:
                    return MySqlDbType.Float;
                //case DbType.Binary:
                //    return MySqlDbType.Image;
                case DbType.Int32:
                    return MySqlDbType.Int32;
                case DbType.Currency:
                    return MySqlDbType.Decimal;
                case DbType.StringFixedLength:
                    return MySqlDbType.Text;
                //case DbType.String:
                //   return MySqlDbType.NText; //???
                case DbType.String:
                    return MySqlDbType.Text;
                case DbType.Double:
                    return MySqlDbType.Double;
                //case DbType.DateTime:
                //    return MySqlDbType.SmallDateTime;
                case DbType.Int16:
                    return MySqlDbType.Int16;
                //case DbType.Currency:
                //    return MySqlDbType.SmallMoney;
                //case DbType.AnsiString:
                //  return MySqlDbType.Text;
                case DbType.Time:
                    return MySqlDbType.Time;
                //case DbType.DateTime2:
                //  return MySqlDbType.Timestamp;
                case DbType.Byte:
                    throw new NotImplementedException();
                case DbType.Guid:
                    return MySqlDbType.Guid;
                case DbType.AnsiString:
                    return MySqlDbType.VarChar;

                default:
                    throw new NotImplementedException();
            }
        }

        public static string MySqlDbTypeToMySql(MySqlDbType dbType)
        {
            switch (dbType)
            {
                case MySqlDbType.Int64:
                    return "BIGINT";
                //case MySqlDbType.Binary:
                case MySqlDbType.Bit:
                    return "BIT";
                case MySqlDbType.String:
                    return "CHAR";
                case MySqlDbType.Date:
                    return "DATE";
                case MySqlDbType.DateTime:
                    return "DATETIME";
                case MySqlDbType.Timestamp:
                    return "TIMESTAMP";
                //case MySqlDbType.TimestampTZ:
                //    return "timetz";
                case MySqlDbType.Double:
                    return "DOUBLE";
                case MySqlDbType.Float:
                    return "FLOAT";
                //case DbType.Binary:
                //    case MySqlDbType.Image:
                case MySqlDbType.Int32:
                    return "INT";
                //case MySqlDbType.Money:
                //    return "money";
                case MySqlDbType.Text:
                    return "TEXT";
                //   case MySqlDbType.NText: //???
                //case MySqlDbType.SmallDateTime:
                case MySqlDbType.Int16:
                    return "SMALLINT";
                //    case MySqlDbType.SmallMoney:
                //  case MySqlDbType.Text:
                case MySqlDbType.Time:
                    return "TIME";
                //  case MySqlDbType.Timestamp:
                case MySqlDbType.Guid:
                    return "GUID";
                case MySqlDbType.VarChar:
                    return "VARCHAR";

                default:
                    throw new NotImplementedException();
            }
        }

        public static DmsDbType DbTypeToDmsDbType(DbType dbType)
        {
            switch (dbType)
            {
                case DbType.Int64:
                    return DmsDbType.INT8;
                case DbType.Binary:
                    return DmsDbType.BYTES;
                case DbType.Boolean:
                    return DmsDbType.BOOL;
                case DbType.AnsiStringFixedLength:
                    return DmsDbType.STRING; //losing fixed
                case DbType.Date:
                    return DmsDbType.DATE;
                case DbType.DateTime:
                    return DmsDbType.DATETIME;
                case DbType.DateTime2:
                    return DmsDbType.DATETIME;
                case DbType.DateTimeOffset:
                    return DmsDbType.WSTRING;
                case DbType.Decimal:
                    return DmsDbType.NUMERIC;
                case DbType.Single:
                    return DmsDbType.REAL8;
                case DbType.Double:
                    return DmsDbType.REAL4;
                case DbType.Int32:
                    return DmsDbType.INT4;
                case DbType.Currency:
                    return DmsDbType.NUMERIC;
                case DbType.StringFixedLength:
                    return DmsDbType.WSTRING; //losing fixed
                case DbType.String:
                    return DmsDbType.WSTRING;
                case DbType.Int16:
                    return DmsDbType.INT2;
                case DbType.Byte:
                    return DmsDbType.INT1;
                case DbType.Time:
                    return DmsDbType.TIME;
                case DbType.Guid:
                    return DmsDbType.UNIQUEIDENTIFIER;
                case DbType.AnsiString:
                    return DmsDbType.STRING;
                default:
                    throw new NotImplementedException();
            }
        }

        public static string DmsDbTypeToPostgres(DmsDbType dmsDbType)
        {
            switch (dmsDbType)
            {
                case DmsDbType.INT8:
                    return "bigint";
                case DmsDbType.INT4:
                    return "integer";
                case DmsDbType.INT2:
                    return "smallint";
                case DmsDbType.INT1:
                    return "smallint";
                case DmsDbType.BYTES:
                    return "bytea";
                case DmsDbType.BOOL:
                    return "bool";
                case DmsDbType.STRING:
                    return "character varying";
                case DmsDbType.DATE:
                    return "date";
                case DmsDbType.DATETIME:
                    return "unmapp";
                case DmsDbType.WSTRING:
                    return "character varying";
                case DmsDbType.NUMERIC:
                    return "decimal";
                case DmsDbType.REAL8:
                    return "float8";
                case DmsDbType.REAL4:
                    return "float4";
               
                case DmsDbType.TIME:
                    return "time";
                case DmsDbType.UNIQUEIDENTIFIER:
                    return "unmapped";
                default:
                    throw new NotImplementedException();
            }
        }

        public static SqlDataTypeOption DbTypeToSqlDataTypeOption(DbType dbType)
        {
            switch (dbType)
            {
                case DbType.Int64:
                    return SqlDataTypeOption.BigInt;
                case DbType.Binary:
                    return SqlDataTypeOption.Binary;
                case DbType.Boolean:
                    return SqlDataTypeOption.Bit;
                case DbType.AnsiStringFixedLength:
                    return SqlDataTypeOption.Char;
                case DbType.Date:
                    return SqlDataTypeOption.Date;
                case DbType.DateTime:
                    return SqlDataTypeOption.DateTime;
                case DbType.DateTime2:
                    return SqlDataTypeOption.DateTime2;
                case DbType.DateTimeOffset:
                    return SqlDataTypeOption.DateTimeOffset;
                case DbType.Decimal:
                    return SqlDataTypeOption.Decimal;
                case DbType.Single:
                    return SqlDataTypeOption.Float;
                //case DbType.Binary:
                //    return SqlDataTypeOption.Image;
                case DbType.Int32:
                    return SqlDataTypeOption.Int;
                case DbType.Currency:
                    return SqlDataTypeOption.Money;
                case DbType.StringFixedLength:
                    return SqlDataTypeOption.NChar;
                //case DbType.String:
                //   return SqlDataTypeOption.NText; //???
                case DbType.String:
                    return SqlDataTypeOption.NVarChar;
                case DbType.Double:
                    return SqlDataTypeOption.Real;
                //case DbType.DateTime:
                //    return SqlDataTypeOption.SmallDateTime;
                case DbType.Int16:
                    return SqlDataTypeOption.SmallInt;
                //case DbType.Currency:
                //    return SqlDataTypeOption.SmallMoney;
                //case DbType.AnsiString:
                //  return SqlDataTypeOption.Text;
                case DbType.Time:
                    return SqlDataTypeOption.Time;
                //case DbType.DateTime2:
                //  return SqlDataTypeOption.Timestamp;
                case DbType.Byte:
                    return SqlDataTypeOption.TinyInt;
                case DbType.Guid:
                    return SqlDataTypeOption.UniqueIdentifier;
                case DbType.AnsiString:
                    return SqlDataTypeOption.VarChar;
                default:
                    throw new NotImplementedException();
            }
        }

        public static SqlDataTypeOption SqlTypeToSqlDataTypeOption(string sqlType)
        {
            switch (sqlType.ToLower())
            {
                case "bigint":
                    return SqlDataTypeOption.BigInt;
                case "binary":
                    return SqlDataTypeOption.Binary;
                case "bit":
                    return SqlDataTypeOption.Bit;
                case "char":
                    return SqlDataTypeOption.Char;
                case "date":
                    return SqlDataTypeOption.Date;
                case "datetime":
                    return SqlDataTypeOption.DateTime;
                case "datetime2":
                    return SqlDataTypeOption.DateTime2;
                case "datetimeoffset":
                    return SqlDataTypeOption.DateTimeOffset;
                case "decimal":
                    return SqlDataTypeOption.Decimal;
                case "float":
                    return SqlDataTypeOption.Float;
                case "image":
                    return SqlDataTypeOption.Image;
                case "int":
                    return SqlDataTypeOption.Int;
                case "money":
                    return SqlDataTypeOption.Money;
                case "nchar":
                    return SqlDataTypeOption.NChar;
                case "ntext":
                    return SqlDataTypeOption.NText;
                case "nvarchar":
                    return SqlDataTypeOption.NVarChar;
                case "real":
                    return SqlDataTypeOption.Real;
                case "smalldatetime":
                    return SqlDataTypeOption.SmallDateTime;
                case "smallint":
                    return SqlDataTypeOption.SmallInt;
                case "smallmoney":
                    return SqlDataTypeOption.SmallMoney;
                case "text":
                    return SqlDataTypeOption.Text;
                case "time":
                    return SqlDataTypeOption.Time;
                case "timestamp":
                    return SqlDataTypeOption.Timestamp;
                case "tinyint":
                    return SqlDataTypeOption.TinyInt;
                case "uniqueidentifier":
                    return SqlDataTypeOption.UniqueIdentifier;
                case "varchar":
                    return SqlDataTypeOption.VarChar;
                case "xml":
                    return SqlDataTypeOption.Text; //todo: verify
                case "varbinary":
                    return SqlDataTypeOption.VarBinary;
                case "tabletype":
                    return SqlDataTypeOption.Table;
                case "structured":
                    return SqlDataTypeOption.Table;
                default:
                    throw new NotImplementedException();
            }
        }

        public static string SqlDataTypeOptionToSqlType(SqlDataTypeOption sqlDataTypeOption)
        {
            switch (sqlDataTypeOption)
            {
                case SqlDataTypeOption.BigInt:
                    return "BIGINT";

                case SqlDataTypeOption.Binary:
                    return "BINARY";
                case SqlDataTypeOption.VarBinary:
                    return "VARBINARY";
                case SqlDataTypeOption.Bit:
                    return "BIT";
                case SqlDataTypeOption.Char:
                    return "CHAR";
                case SqlDataTypeOption.Date:
                    return "DATE";
                case SqlDataTypeOption.DateTime:
                    return "DATETIME";
                case SqlDataTypeOption.DateTime2:
                    return "DATETIME2";
                case SqlDataTypeOption.DateTimeOffset:
                    return "DATETIMEOFFSET";
                case SqlDataTypeOption.Decimal:
                    return "DECIMAL";
                case SqlDataTypeOption.Float:
                    return "FLOAT";
                case SqlDataTypeOption.Image:
                    return "IMAGE";
                case SqlDataTypeOption.Int:
                    return "INT";
                case SqlDataTypeOption.Money:
                    return "MONEY";
                case SqlDataTypeOption.NChar:
                    return "NCHAR";
                case SqlDataTypeOption.NText:
                    return "NTEXT";
                case SqlDataTypeOption.NVarChar:
                    return "NVARCHAR";
                case SqlDataTypeOption.Numeric:
                    return "NUMERIC";
                case SqlDataTypeOption.Real:
                    return "REAL";
                case SqlDataTypeOption.SmallDateTime:
                    return "SMALLDATETIME";
                case SqlDataTypeOption.SmallInt:
                    return "SMALLINT";
                case SqlDataTypeOption.SmallMoney:
                    return "SMALLMONEY";
                case SqlDataTypeOption.Text:
                    return "TEXT";
                case SqlDataTypeOption.Time:
                    return "TIME";
                case SqlDataTypeOption.Timestamp:
                    return "TIMESTAMP";
                case SqlDataTypeOption.Rowversion:
                    return "ROWVERSION";
                case SqlDataTypeOption.TinyInt:
                    return "TINYINT";
                case SqlDataTypeOption.UniqueIdentifier:
                    return "UNIQUEIDENTIFIER";
                case SqlDataTypeOption.VarChar:
                    return "VARCHAR";
                //  case SqlDataTypeOption.Xml:
                //    return "xml";
                case SqlDataTypeOption.None:
                    return "NONE";
                default:
                    throw new NotImplementedException();
            }
        }


        public static SqlDbType SqlDataTypeOptionToSqlDbType(SqlDataTypeOption sqlDataTypeOption)
        {
            switch (sqlDataTypeOption)
            {

                case SqlDataTypeOption.BigInt:
                    return SqlDbType.BigInt;
                case SqlDataTypeOption.Binary:
                    return SqlDbType.Binary;
                case SqlDataTypeOption.Bit:
                    return SqlDbType.Bit;
                case SqlDataTypeOption.Char:
                    return SqlDbType.Char;
                case SqlDataTypeOption.Date:
                    return SqlDbType.Date;
                case SqlDataTypeOption.DateTime:
                    return SqlDbType.DateTime;
                case SqlDataTypeOption.DateTime2:
                    return SqlDbType.DateTime2;
                case SqlDataTypeOption.DateTimeOffset:
                    return SqlDbType.DateTimeOffset;
                case SqlDataTypeOption.Decimal:
                    return SqlDbType.Decimal;
                case SqlDataTypeOption.Float:
                    return SqlDbType.Float;
                case SqlDataTypeOption.Image:
                    return SqlDbType.Image;
                case SqlDataTypeOption.Int:
                    return SqlDbType.Int;
                case SqlDataTypeOption.Money:
                    return SqlDbType.Money;
                case SqlDataTypeOption.NChar:
                    return SqlDbType.NChar;
                case SqlDataTypeOption.NText:
                    return SqlDbType.NText;
                case SqlDataTypeOption.NVarChar:
                    return SqlDbType.NVarChar;
                case SqlDataTypeOption.Real:
                    return SqlDbType.Real;
                case SqlDataTypeOption.SmallDateTime:
                    return SqlDbType.SmallDateTime;
                case SqlDataTypeOption.SmallInt:
                    return SqlDbType.SmallInt;
                case SqlDataTypeOption.SmallMoney:
                    return SqlDbType.SmallMoney;
                case SqlDataTypeOption.Text:
                    return SqlDbType.Text;
                case SqlDataTypeOption.Time:
                    return SqlDbType.Time;
                case SqlDataTypeOption.Timestamp:
                    return SqlDbType.Timestamp;
                case SqlDataTypeOption.TinyInt:
                    return SqlDbType.TinyInt;
                case SqlDataTypeOption.UniqueIdentifier:
                    return SqlDbType.UniqueIdentifier;
                case SqlDataTypeOption.VarChar:
                    return SqlDbType.VarChar;
                case SqlDataTypeOption.Numeric:
                    return SqlDbType.Decimal;
                case SqlDataTypeOption.Sql_Variant:
                    return SqlDbType.Variant;
                default:
                    throw new NotImplementedException();
            }
        }

        public static SqlDbType ParseValueAsSqlDbType(string value, string pstrDateFormat = null)
        {
            switch (value.ToLower())
            {
                case "bigint":
                case "int64":
                case "long":
                    return SqlDbType.BigInt;
                case "bit":
                case "boolean":
                case "bool":
                    return SqlDbType.Bit;
                case "ntext":
                    return SqlDbType.NText;
                case "nvarchar":
                    return SqlDbType.NVarChar;
                case "varchar":
                case "string":
                    return SqlDbType.VarChar;
                case "nchar":
                    return SqlDbType.NChar;
                case "text":
                    return SqlDbType.Text;
                case "char":
                    return SqlDbType.Char; //return SqlDbType.Char;
                case "char[]":
                    return SqlDbType.VarChar; //return SqlDbType.Char;

                case "smalldatetime":
                    return SqlDbType.SmallDateTime;
                case "datetime":
                    return SqlDbType.DateTime;
                case "datetime2":
                    return SqlDbType.DateTime2;
                case "date":
                    return SqlDbType.Date;
                case "datetimeoffset":
                    return SqlDbType.DateTimeOffset;
                case "time":
                    return SqlDbType.Time;
                case "money":
                    return SqlDbType.Money;
                case "smallmoney":
                    return SqlDbType.SmallMoney;
                case "decimal":
                    return SqlDbType.Decimal;
                case "float":
                    return SqlDbType.Float;
                case "binary":
                    return SqlDbType.Binary;
                case "varbinary":
                    return SqlDbType.VarBinary;
                case "timestamp":
                    return SqlDbType.Timestamp;
                case "image":
                    return SqlDbType.Image;
                case "int":
                case "int32":

                    return SqlDbType.Int;
                case "real":
                    return SqlDbType.Real;
                case "smallint":
                    return SqlDbType.SmallInt;
                case "tinyint":
                    return SqlDbType.TinyInt;
                case "byte":
                    return SqlDbType.TinyInt;
                case "byte[]":
                    return SqlDbType.Timestamp;
                case "xml":
                    return SqlDbType.Xml;
                case "uniqueidentifier":
                    return SqlDbType.UniqueIdentifier;
                case "enum":
                    return SqlDbType.Int;
                case "tabletype":
                    return SqlDbType.Structured;
                default:
                    throw new ArgumentOutOfRangeException("value", value.ToLower(),
                        $"Cannot map the type {value.ToLower()} to SqlDbType");
            }
        }

        public static DbType GetDbType(string dataType)
        {
            var sqlDbType = ParseValueAsSqlDbType(dataType);
            return SqlDbTypeToDbType(sqlDbType);
        }

        public static GrpcType SqlDbTypeToGrpcType(SqlDbType sqlDbType)
        {
            switch (sqlDbType)
            {
                case SqlDbType.BigInt:
                    return GrpcType.__int64;
                case SqlDbType.Binary:
                    return GrpcType.__bytes;
                case SqlDbType.Bit:
                    return GrpcType.__bool;
                case SqlDbType.Char:
                    return GrpcType.__string;
                case SqlDbType.Date:
                    return GrpcType.__google_protobuf_Timestamp;
                case SqlDbType.DateTime:
                    return GrpcType.__google_protobuf_Timestamp;
                case SqlDbType.DateTime2:
                    return GrpcType.__google_protobuf_Timestamp;
                case SqlDbType.DateTimeOffset:
                    return GrpcType.__google_protobuf_Timestamp;
                case SqlDbType.Decimal:
                    return GrpcType.__company_Decimal64Value;
                case SqlDbType.Float:
                    return GrpcType.__float;
                case SqlDbType.Image:
                    return GrpcType.__bytes;
                case SqlDbType.Int:
                    return GrpcType.__int32;
                case SqlDbType.Money:
                    return GrpcType.__string;
                case SqlDbType.NChar:
                    return GrpcType.__string;
                case SqlDbType.NText:
                    return GrpcType.__string; //???
                case SqlDbType.NVarChar:
                    return GrpcType.__string;
                case SqlDbType.Real:
                    return GrpcType.__double;
                case SqlDbType.SmallDateTime:
                    return GrpcType.__google_protobuf_Timestamp;
                case SqlDbType.SmallInt:
                    throw new NotImplementedException();
                case SqlDbType.SmallMoney:
                    throw new NotImplementedException();
                case SqlDbType.Text:
                    return GrpcType.__string;
                case SqlDbType.Time:
                    return GrpcType.__google_protobuf_Timestamp;
                case SqlDbType.Timestamp:
                    return GrpcType.__bytes;
                case SqlDbType.TinyInt:
                    return GrpcType.__int32;
                case SqlDbType.UniqueIdentifier:
                    throw new NotImplementedException();
                case SqlDbType.VarChar:
                    return GrpcType.__string;
                case SqlDbType.Xml:
                    return GrpcType.__string;
                default:
                    throw new NotImplementedException();
            }
        }

        public static SqlDbType GrpcTypeToSqlDbType(GrpcType grpcType)
        {
            switch (grpcType)
            {
                case GrpcType.__int64:
                    return SqlDbType.BigInt;
                case GrpcType.__bytes:
                    return SqlDbType.Binary;
                case GrpcType.__bool:
                    return SqlDbType.Bit;

                case GrpcType.__google_protobuf_Timestamp:
                    return SqlDbType.DateTime;
                case GrpcType.__company_Decimal64Value:
                    return SqlDbType.Decimal;
                case GrpcType.__float:
                    return SqlDbType.Float;
                case GrpcType.__google_protobuf_Int32Value:
                case GrpcType.__int32:
                    return SqlDbType.Int;
                case GrpcType.__google_protobuf_StringValue:
                case GrpcType.__string:
                    return SqlDbType.VarChar;
                case GrpcType.__double:
                    return SqlDbType.Real;
                case GrpcType.__enum:
                    return SqlDbType.Int;
                case GrpcType.__message:
                    return SqlDbType.Structured;
                default:
                    throw new NotImplementedException();
            }
        }

        public static Type GrpcTypeToClrType(GrpcType grpcType)
        {
            switch (grpcType)
            {
                case GrpcType.__int64:
                    return typeof(long);
                case GrpcType.__bytes:
                    return typeof(byte[]);
                case GrpcType.__bool:
                    return typeof(bool);

                case GrpcType.__google_protobuf_Timestamp:
                    return typeof(DateTime);
                case GrpcType.__google_protobuf_StringValue:
                    return typeof(string);
                case GrpcType.__google_protobuf_Int32Value:
                    return typeof(int);
                case GrpcType.__float:
                    return typeof(float);

                case GrpcType.__int32:
                    return typeof(int);
                case GrpcType.__string:
                    return typeof(string);
                case GrpcType.__double:
                    return typeof(double);
                case GrpcType.__company_Decimal64Value:
                    return typeof(decimal);

                default:
                    throw new NotImplementedException();
            }
        }

        public static string GrpcTypeToSqlRaw(GrpcType grpcType)
        {
            switch (grpcType)
            {
                case GrpcType.__int64:
                    return "bigint";
                case GrpcType.__bytes:
                    return "char";
                case GrpcType.__bool:
                    return "bit";

                case GrpcType.__google_protobuf_Timestamp:
                    return "datetime";
                case GrpcType.__company_Decimal64Value:
                    return "decimal";
                case GrpcType.__float:
                    return "float";

                case GrpcType.__google_protobuf_Int32Value:
                case GrpcType.__int32:
                    return "int";
                case GrpcType.__google_protobuf_StringValue:
                case GrpcType.__string:
                    return "varchar";
                case GrpcType.__double:
                    return "real";
                case GrpcType.__enum:
                    return "enum";
                case GrpcType.__message:
                    return "tabletype";
                default:
                    throw new NotImplementedException();
            }
        }

        public static GrpcType ClrTypeToGrpcType(Type clrType)
        {
            if (clrType == typeof(long))
                return GrpcType.__int64;
            if (clrType == typeof(byte[]))
                return GrpcType.__bytes;
            if (clrType == typeof(bool))
                return GrpcType.__bool;

            if (clrType == typeof(DateTime))
                return GrpcType.__google_protobuf_Timestamp;
            if (clrType == typeof(float))
                return GrpcType.__float;
            if (clrType == typeof(int))
                return GrpcType.__int32;
            if (clrType == typeof(string))
                return GrpcType.__string;
            if (clrType == typeof(double))
                return GrpcType.__double;
            if (clrType == typeof(decimal))
                return GrpcType.__company_Decimal64Value;

            throw new NotImplementedException($"No conversion implemented for clr type {clrType} to Grpc type");
        }

        public static Type ClrTypeAliasToClrType(string aliasString)
        {
            foreach (var alias in Aliases)
                if (alias.Value.ToLower() == aliasString.ToLower())
                    return alias.Key;
            throw new NotImplementedException();
        }

        public static string PrintMappingFromSqlServer()
        {
            const string unmapped = "<unmapped>";
            var codeWriter = new CodeWriter();
            codeWriter.WriteLine($"{"Sql Server",-25}\t{"Postgresql",-25}\t{"MySql",-25}\t{"AWS DMS",-25}\t{"DMS to Postgrs",-25}\t{"SqlDataTypeOption",-25}\t{"SqlDbType",-25}\t{"DbType",-25}\t{"NpgsqlType",-25}\t{"MySqlDbType",-25}");
            foreach (var sqlDataTypeOption in Enum.GetValues(typeof(SqlDataTypeOption)))
            {
                if ((SqlDataTypeOption)sqlDataTypeOption == SqlDataTypeOption.None)
                    continue;
                var sqlType = "<unmapped>";


                try
                {
                    sqlType = SqlMapper.SqlDataTypeOptionToSqlType((SqlDataTypeOption)sqlDataTypeOption);
                }
                catch (NotImplementedException)
                {
                }

                try
                {
                    var sqlDbType = SqlMapper.SqlDataTypeOptionToSqlDbType((SqlDataTypeOption)sqlDataTypeOption);
                    var dbTypeString = unmapped;
                    var nPGSqlTypeString = unmapped;
                    var postGresTypeString = unmapped;

                    var mySqlDbTypeString = unmapped;
                    var mySqlType = unmapped;
                    var dmsDbTypeString = unmapped;
                    var dmsDbTypeToPostgresString = unmapped;


                    try
                    {
                        var dbType = SqlMapper.SqlDbTypeToDbType(sqlDbType);
                        var dmsDbType = SqlMapper.DbTypeToDmsDbType(dbType);
                        dmsDbTypeString = Enum.GetName(typeof(DmsDbType), dmsDbType);
                        dbTypeString = dbType.ToString();
                        var nPGSqlType = SqlMapper.DbTypeToNpgsqlDbType(dbType);
                        nPGSqlTypeString = nPGSqlType.ToString();
                        var postGresType = SqlMapper.NpgsqlDbTypeToPostgres(nPGSqlType);
                        postGresTypeString = postGresType.ToString();
                        try
                        {
                            dmsDbTypeToPostgresString = SqlMapper.DmsDbTypeToPostgres(dmsDbType);
                        }
                        catch (NotImplementedException)
                        {

                        }
                        try
                        {
                            var mySqlDbType = SqlMapper.DbTypeToMySqlDbType(dbType);
                            mySqlDbTypeString = mySqlDbType.ToString();

                            mySqlType = SqlMapper.MySqlDbTypeToMySql(mySqlDbType).ToString();
                        }
                        catch (NotImplementedException)
                        {
                        }
                    }
                    catch (NotImplementedException)
                    {
                    }
                    codeWriter.WriteLine($"{sqlType,-25}\t{postGresTypeString,-25}\t{mySqlType,-25}\t{dmsDbTypeString,-25}\t{dmsDbTypeToPostgresString,-25}\t{sqlDataTypeOption,-25}\t{sqlDbType,-25}\t{dbTypeString,-25}\t{nPGSqlTypeString,-25}\t{mySqlDbTypeString,-25}");
                }
                catch (NotImplementedException)
                {
                    codeWriter.WriteLine($"{sqlType,-25}\t{unmapped,-25}\t{unmapped,-25}\t{unmapped,-25}\t{unmapped,-25}\t{sqlDataTypeOption,-25}\t{unmapped,-25}\t{unmapped,-25}\t{unmapped,-25}\t{unmapped,-25}");
                }
            }
            return codeWriter.ToString();
        }
    }
}