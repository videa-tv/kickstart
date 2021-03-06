using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Utility;

namespace Kickstart.Pass2.DataLayerProject.Table
{
    public interface ICTableToCClassConverter
    {
        CClass Convert(CTable table, IEnumerable<CTable> allTables, bool convertForeignKeysToObjects);
    }
    //todo: this needs unit tests
    public class CTableToCClassConverter : ICTableToCClassConverter
    {
        

        public CClass Convert(CTable table, IEnumerable<CTable> allTables, bool convertForeignKeysToObjects)
        {
            var @class = new CClass(table.TableName);
            @class.DerivedFrom = table;
            @class.Namespace = new CNamespace {NamespaceName = table.Schema.SchemaName};
           
            @class.NamespaceRef.Add(new CNamespaceRef(){ ReferenceTo =  new CNamespace(){ NamespaceName = "System"}});
            @class.NamespaceRef.Add(new CNamespaceRef() { ReferenceTo = new CNamespace() { NamespaceName = "System.Collections.Generic" } });

            foreach (var column in table.Column)
            {
                if (!convertForeignKeysToObjects || !column.ForeignKeyColumn.Any())
                {
                    var prop = new CProperty();
                    prop.PropertyName = column.ColumnName;
                    prop.Type = column.ColumnType.ToClrTypeName();
                    @class.Property.Add(prop);
                }
                else
                {
                    //we add foreign keys later, as objects
                }
            }

            if (convertForeignKeysToObjects && allTables != null)
            {
                //add foreign keys as objects
                //todo: put this core logic into extension method
                foreach (var allTable in allTables)
                {
                    foreach (var allTableColumn in allTable.Column)
                    {
                        foreach (var allTableColumnFK in allTableColumn.ForeignKeyColumn)
                        {
                            if (allTableColumnFK.Table.Schema.SchemaName == table.Schema.SchemaName &&
                                allTableColumnFK.Table.TableName == table.TableName) //todo: use a fancy compare method
                            {
                                var prop = new CProperty();
                                var s = new Inflector.Inflector(CultureInfo.CurrentCulture);

                                prop.PropertyName = s.Pluralize(allTable.TableName);
                                prop.Type = $"IEnumerable<{allTable.TableName}>"; //todo: how can we determine if is a collection or not? may be able to use DerivedFrom
                                @class.Property.Add(prop);
                            }
                        }
                    }
                }
            }



            return @class;
        }
 
    }
}