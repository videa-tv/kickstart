using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickstart.Pass2.SampleData
{
    public class SamplePostgresDataService
    {
        public string GetSampleData (NpgsqlDbType npgsqlType, int length)
        {

            switch (npgsqlType)
            {
                case NpgsqlDbType.Char:
                    return $"'{new String('0',length)}'";
                case NpgsqlDbType.Date:
                    return "'2018-01-14'";
                case NpgsqlDbType.Integer:
                    return "1";
                case NpgsqlDbType.Smallint:
                    return "0";
                case NpgsqlDbType.Bigint:
                    return "2";
                case NpgsqlDbType.Boolean:
                    return "'true'";
                case NpgsqlDbType.Timestamp:
                    return "'1900-01-01 17:30:00'";
                case NpgsqlDbType.Varchar:
                case NpgsqlDbType.Text:
                    return "'Sample Data'";
                case NpgsqlDbType.Numeric:
                    return "1.2";
                default:
                    throw new NotImplementedException();

            }
        }

    }
}
