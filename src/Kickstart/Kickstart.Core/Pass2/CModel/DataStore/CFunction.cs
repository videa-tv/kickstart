using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kickstart.Interface;
using Kickstart.Utility;

namespace Kickstart.Pass2.CModel.DataStore
{
    public class CFunction : CPart
    {
        public CSchema Schema { get; set; }


        public bool ConvertToSnakeCase { get; set; }
        public string FunctionName { get; set; }

        public string FunctionText { get; set; }
        public string FunctionBody { get; set; }

        public List<CStoredProcedureParameter> Parameter { get; set; } = new List<CStoredProcedureParameter>();

        public List<CColumn> ResultSet { get; set; } = new List<CColumn>();

        public DataStoreTypes DatabaseType { get; set; }
        public bool HasResultSet { get; set; }
        public string ResultSetName { get; set; }


        public CFunction(DataStoreTypes databaseType)
        {
            DatabaseType = databaseType;
        }
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
