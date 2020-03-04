using Kickstart.Pass2.CModel.DataStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kickstart.Utility;

namespace Kickstart.Pass2.DataStoreProject
{
    public class FlywayFileNameService : IFlywayFileNameService
    {
        public int SprintNumber { get; private set; } = 100 + ((DateTime.Now - new DateTime(2018, 3, 28)).Days / 14);//todo: Calculate this?
        int _itemCount = 0;

        DateTime _nowStart = DateTime.Now;
        public string FlywayVersionNumber
        {
            get
            {
                var now = _nowStart + new TimeSpan(0, 0, _itemCount);
                _itemCount++;
                return $"V{ SprintNumber}_{ now.ToString("yyyyMMddHHmmss")}";
            }
        }

        public string GetFlywayFileName(CTableType tableType)
        {
            var fileName = $"{FlywayVersionNumber}__Create_Type_{tableType.Schema.SchemaName}_{tableType.TableName}.sql";
            return fileName;
        }

        public string GetFlywayFileName(CTable table)
        {
            return $"{FlywayVersionNumber}__Create_Table_{table.Schema.SchemaName}_{table.TableName}.sql";
        }

        public string GetFlywayFileName(CSchema schema)
        {
            return $"{FlywayVersionNumber}__Create_Schema_{schema.SchemaName}.sql";
        }

        public string GetFlywayFileName(CView view)
        {
            return $"{FlywayVersionNumber}__Create_View_{view.Schema.SchemaName}_{view.ViewName}.sql";
        }

        public string GetFlywayFileName(CFunction function)
        {
            return $"{FlywayVersionNumber}__Create_Function_{function.Schema.SchemaName}_{function.FunctionName}.sql";
        }

        public string GetFlywayFileName(CStoredProcedure storedProcedure)
        {
            if (storedProcedure.DatabaseType == DataStoreTypes.SqlServer)
            {
                return
                    $"{FlywayVersionNumber}__Create_Procedure_{storedProcedure.Schema.SchemaName}_{storedProcedure.StoredProcedureName}.sql";
            }
            else
            {
                return
                    $"{FlywayVersionNumber}__Create_Function_{storedProcedure.Schema.SchemaName}_{storedProcedure.StoredProcedureName}.sql";
            }
        }
    }
}
