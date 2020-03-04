using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickstart.Utility
{
    public static class DDLHelperExtension
    {
        public static string WrapReservedAndSnakeCase(this string str, DataStoreTypes databaseType, bool snakeCase)
        {
            if (str == null)
                return str;
            if (snakeCase)
            {
                str = str.ToSnakeCase();
            }
            if (databaseType == DataStoreTypes.Postgres)
            {
                if (str.ToLower() == "order")
                {
                    return $@"""{str}""";
                }
                if (str.ToLower() == "user")
                {
                    return $@"""{str}""";
                }

                if (str.ToLower() == "end")
                {
                    return $@"""{str}""";
                }
                if (str.ToLower() == "primary")
                {
                    return $@"""{str}""";
                }

                if (str.ToLower() == "default")
                {
                    return $@"""{str}""";
                }

                if (str.ToLower() == "distinct")
                {
                    return $@"""{str}""";
                }
                if (str.ToLower() == "position")
                {
                    return $@"""{str}""";
                }
            }
            else if (databaseType == DataStoreTypes.SqlServer)
            {
                //todo: read data from somewhere like 
               // https://docs.microsoft.com/en-us/sql/t-sql/language-elements/reserved-keywords-transact-sql?view=sql-server-2017
                if (str.ToLower() == "order")
                {
                    return $@"[{str}]";
                }
                if (str.ToLower() == "user")
                {
                    return $@"[{str}]";
                }
                if (str.ToLower() == "end")
                {
                    return $@"[{str}]";
                }
                if (str.ToLower() == "primary")
                {
                    return $@"[{str}]";
                }
                if (str.ToLower() == "default")
                {
                    return $@"[{str}]";
                }
                if (str.ToLower() == "distinct")
                {
                    return $@"[{str}]";
                }
                if (str.ToLower() == "position")
                {
                    return $@"[{str}]";
                }
                if (str.ToLower() == "group")
                {
                    return $@"[{str}]";
                }
            }
            else
            {
                throw new NotImplementedException();
            }

            return str;
        }
        public static string UnwrapReserved(this string str, DataStoreTypes databaseType)
        {
            if (databaseType == DataStoreTypes.SqlServer)
            {
                if (str.StartsWith("[") && str.EndsWith("]"))
                {
                    str = str.Substring(1, str.Length - 2);
                }
            }
            else
            {
                throw new NotImplementedException();
            }

            return str;
        }
    }
}
