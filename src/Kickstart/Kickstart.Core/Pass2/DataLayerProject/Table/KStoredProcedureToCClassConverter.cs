using System;
using System.Data;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Utility;

namespace Kickstart.Pass2.DataLayerProject.Table
{
    public class CStoredProcedureToCClassConverter
    {
        #region Fields

        #endregion Fields

        #region Properties

        #endregion Properties

        #region Constructors

        #endregion Constructors

        #region Methods

        public CClass ConvertByParameterSet(CStoredProcedure storedProcedure)
        {
            if (string.IsNullOrEmpty(storedProcedure.ParameterSetName))
                throw new Exception("cannot create CClass without a class name");

            var @class = new CClass(storedProcedure.ParameterSetName);
            @class.DerivedFrom = storedProcedure;
            @class.Namespace = new CNamespace {NamespaceName = storedProcedure.Schema.SchemaName};

            if (storedProcedure.Parameter.Exists(p => p.ParameterType == DbType.DateTime2))
            {
                @class.NamespaceRef.Add(new CNamespaceRef() { ReferenceTo = new CNamespace() { NamespaceName = "System"}});
            }

            if (!string.IsNullOrEmpty(storedProcedure.ParameterSetName))
                foreach (var parameter in storedProcedure.Parameter)
                {
                    var prop = new CProperty
                    {
                        PropertyName = parameter.ParameterName,
                        Type = parameter.ParameterType.ToClrTypeName(),
                        MaxLength = parameter.ParameterLength
                    };
                    /*
if (prop.Type == "Char" && prop.MaxLength > 0)
{
prop.Type = "Char []";
}*/
                    @class.Property.Add(prop);
                }

            return @class;

            /*
            var view = new SView
            {
                Schema = new SSchema {  SchemaName = storedProcedure.Schema.SchemaName},
                ViewName = storedProcedure.StoredProcedureName
            };
            view.Column = new List<SColumn>();

            foreach (var col in storedProcedure.ResultSet)
            {
                var column = new SColumn(view)
                {
                    ColumnName = col.ColumnName,
                    ColumnTypeRaw = col.ColumnTypeRaw,
                    ColumnSqlDbType = col.ColumnSqlDbType,
                    ColumnType = col.ColumnType,
                    ColumnLength = col.ColumnLength
                };
                view.Column.Add(column);
            }
            return view;
            */
        }

        public CClass ConvertByResultSet(CStoredProcedure storedProcedure)
        {
            if (string.IsNullOrEmpty(storedProcedure.ResultSetName))
                throw new Exception("cannot create CClass without a class name");

            var @class = new CClass(storedProcedure.ResultSetName)
            {
                DerivedFrom = storedProcedure,
                Namespace = new CNamespace {NamespaceName = storedProcedure.Schema.SchemaName}
            };

            if (storedProcedure.ResultSet.Exists(r =>r.ColumnType == DbType.DateTime2))
            {
                @class.NamespaceRef.Add(new CNamespaceRef() { ReferenceTo = new CNamespace() { NamespaceName = "System" } });
            }

            if (!string.IsNullOrEmpty(storedProcedure.ResultSetName))
                foreach (var column in storedProcedure.ResultSet)
                {
                    var prop = new CProperty
                    {
                        PropertyName = column.ColumnName,
                        Type = column.ColumnType.ToClrTypeName(),
                        MaxLength = column.ColumnLength
                    };
                    @class.Property.Add(prop);
                }


            return @class;

            /*
            var view = new SView
            {
                Schema = new SSchema {  SchemaName = storedProcedure.Schema.SchemaName},
                ViewName = storedProcedure.StoredProcedureName
            };
            view.Column = new List<SColumn>();

            foreach (var col in storedProcedure.ResultSet)
            {
                var column = new SColumn(view)
                {
                    ColumnName = col.ColumnName,
                    ColumnTypeRaw = col.ColumnTypeRaw,
                    ColumnSqlDbType = col.ColumnSqlDbType,
                    ColumnType = col.ColumnType,
                    ColumnLength = col.ColumnLength
                };
                view.Column.Add(column);
            }
            return view;
            */
        }

        #endregion Methods
    }
}