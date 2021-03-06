using System;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Utility;

namespace Kickstart.Pass2.DataLayerProject.Table
{
    public class CTableTypeToCClassConverter
    {

        public CClass Convert(CTableType tableType)
        {
            if (string.IsNullOrEmpty(tableType.TableName))
                throw new Exception("cannot create CClass without a class name");

            var @class = new CClass(tableType.TableName);
            @class.Namespace = new CNamespace {NamespaceName = tableType.Schema.SchemaName};

            @class.NamespaceRef.Add(new CNamespaceRef("System"));

            if (!string.IsNullOrEmpty(tableType.TableName))
                foreach (var column in tableType.Column)
                {
                    var prop = new CProperty();
                    prop.PropertyName = column.ColumnName;
                    prop.Type = column.ColumnType.ToClrTypeName();
                    prop.MaxLength = column.ColumnLength;

                    @class.Property.Add(prop);
                }

            return @class;
        }
    }
}