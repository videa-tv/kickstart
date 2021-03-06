using System;
using System.Data;
using Kickstart.Interface;
using Kickstart.Utility;

namespace Kickstart.Pass2.CModel.DataStore
{
    public class CStoredProcedureParameter : CPart
    {
        #region Methods

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }

        #endregion Methods

        #region Fields

        #endregion Fields

        #region Properties

        public DbType ParameterType { get; set; }

        public int ParameterLength { get; set; }

        public string ParameterName { get; set; }

        public string ParameterNameCamelCase => char.ToLowerInvariant(ParameterName[0]) + ParameterName.Substring(1);

        public bool DefaultToNull { get; set; }

        public CColumn SourceColumn { get; set; }

        public string ParameterTypeRaw { get; set; }
        public bool ParameterTypeIsUserDefined { get; set; }
        public string ParameterTypeRawSchema { get; internal set; }
        public bool IsCollection { get; internal set; }

        //public string ParameterTypeRawSchemaSnakeCase { get { return ParameterTypeRawSchema.ToSnakeCase(); } }
        //public string ParameterNameSnakeCase { get { return ParameterName.ToSnakeCase(); } }
        //public string ParameterTypeRawSnakeCase { get { return ParameterTypeRaw.ToSnakeCase(); } }

        internal bool DoesNeedLength()
        {

            //https://docs.microsoft.com/en-us/sql/t-sql/statements/create-type-transact-sql
            if (this.ParameterType == DbType.AnsiStringFixedLength)
                return ParameterLength > 0;
            if (ParameterType == DbType.Binary)
                return ParameterLength > 0;
            if (ParameterType == DbType.StringFixedLength)
                return ParameterLength > 0;

            if (ParameterType == DbType.String)
                return ParameterLength > 0;
            if (ParameterType == DbType.AnsiString)
                return ParameterLength > 0;

            return false;
        }

        #endregion Properties

        #region Constructors
        public CStoredProcedureParameter()
        {

        }
        #endregion Constructors
    }
}