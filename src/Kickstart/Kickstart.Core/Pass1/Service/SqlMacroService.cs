using System.Collections.Generic;
using System.Data;
using System.Linq;
using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel.DataStore;

namespace Kickstart.Pass1.Service
{
    public class SqlMacroService
    {
        public static void AddModifiedByAppUserColumn(IEnumerable<KView> views, string viewName)
        {
            var view = views.First(v => v.GeneratedView.ViewName.ToLower() == viewName.ToLower()).GeneratedView;
            if (view.Column.Exists(c => c.ColumnName.ToLower() == "modifiedbyappuser"))
                return;
            view.Column.Add(new CColumn(view)
            {
                ColumnName = "ModifiedByAppUser",
                ColumnType = DbType.AnsiString,
                ColumnTypeRaw = "varchar",
                ColumnLength = 255,
                IsNullable = true
            });
        }

        internal static void SetSampleDataFile(IEnumerable<KView> views, string viewName, string excelFilePath)
        {
            views.First(v => v.ViewName.ToLower() == viewName.ToLower()).SampleDataExcelFile = excelFilePath;
        }

        public static void MakeIdentity(IEnumerable<KTable> tables, string tableName, string columnName)
        {
            tables.First(v => v.GeneratedTable.TableName.ToLower() == tableName.ToLower()).GeneratedTable.Column
                .First(c => c.ColumnName == columnName).IsIdentity = true;
        }

        public static void MakeIdentity(IEnumerable<KView> views, string tableName, string columnName)
        {
            views.First(v => v.GeneratedView.ViewName.ToLower() == tableName.ToLower()).GeneratedView.Column
                .First(c => c.ColumnName == columnName).IsIdentity = true;
        }

        internal static void SetDefaultValue(IEnumerable<KView> views, string tableName, string columnName,
            string defaultValue)
        {
            views.First(v => v.GeneratedView.ViewName.ToLower() == tableName.ToLower()).GeneratedView.Column
                .First(c => c.ColumnName == columnName).DefaultValue = defaultValue;
        }

        public static void AddFk(IEnumerable<KView> views, string fkTableName, string fkColumnName, string pkTableName,
            string pkColumnName)
        {
            var colFKTarget = views.First(v => v.GeneratedView.ViewName.ToLower() == pkTableName.ToLower())
                .GeneratedView.Column.First(c => c.ColumnName.ToLower() == pkColumnName.ToLower());

            var colFKSource = views.First(v => v.GeneratedView.ViewName.ToLower() == fkTableName.ToLower())
                .GeneratedView.Column.First(c => c.ColumnName.ToLower() == fkColumnName.ToLower());
            if (colFKSource.ForeignKeyColumn == null)
                colFKSource.ForeignKeyColumn = new List<CColumn>();
            colFKSource.ForeignKeyColumn.Add(colFKTarget);
            colFKSource.IsNullable = false; //fks shouldn't be nullable
        }

        public static void MakeNotNullable(IEnumerable<KView> views)
        {
            foreach (var view in views)
            foreach (var column in view.GeneratedView.Column)
            {
                if (column.ColumnName == "ModifiedDateUtc") //todo: resolve this hack
                    continue;
                column.IsNullable = false;
            }
        }

        internal static void MakeNotNullable(IEnumerable<KView> views, string columnName)
        {
            foreach (var view in views)
            foreach (var column in view.GeneratedView.Column)
                if (column.ColumnName.ToLower() == columnName.ToLower()) //todo: resolve this hack
                    column.IsNullable = false;
        }

        public static void MakeNotNullable(IEnumerable<KView> views, string viewName, string columnName)
        {
            views.First(v => v.GeneratedView.ViewName.ToLower() == viewName.ToLower()).GeneratedView.Column
                .First(c => c.ColumnName == columnName).IsNullable = false;
        }

        public static void MakeNullable(IEnumerable<KView> views, string viewName, string columnName,
            bool nullable = true)
        {
            views.First(v => v.GeneratedView.ViewName.ToLower() == viewName.ToLower()).GeneratedView.Column
                .First(c => c.ColumnName == columnName).IsNullable = nullable;
        }

        public static void MakeUnique(IEnumerable<KView> views, string viewName, string columnName, bool unique = true)
        {
            views.First(v => v.GeneratedView.ViewName.ToLower() == viewName.ToLower()).GeneratedView.Column
                .First(c => c.ColumnName == columnName).IsUnique = unique;
        }

        public static void AddIndex(IEnumerable<KView> views, string viewName, string columnName)
        {
            views.First(v => v.GeneratedView.ViewName.ToLower() == viewName.ToLower()).GeneratedView.Column
                .First(c => c.ColumnName == columnName).IsIndexed = true;
        }

        public static void MakeFirstColumnPrimaryKey(IEnumerable<KView> views)
        {
            foreach (var view in views)
                view.GeneratedView.Column.First().IsPrimaryKey = true;
        }

        public static void MakeFirstColumnPrimaryKey(IEnumerable<KTable> tables)
        {
            foreach (var table in tables)
                table.GeneratedTable.Column.First().IsPrimaryKey = true;
        }

        public static void MakePrimaryKey(IEnumerable<KView> views, string viewName, string columnName)
        {
            views.First(v => v.ViewName.ToLower() == viewName.ToLower()).GeneratedView.Column
                .First(c => c.ColumnName == columnName).IsPrimaryKey = true;
        }

        internal static void SetResultSetName(List<KStoredProcedure> storedProcedures, string storedProcedureName,
            string resultSetName)
        {
            storedProcedures.First(v => v.StoredProcedureName.ToLower() == storedProcedureName.ToLower())
                .ResultSetName = resultSetName;
        }

        internal static void SetReturnsMultipleRows(List<KStoredProcedure> storedProcedures, string storedProcedureName,
            string resultSetName)
        {
            storedProcedures.First(v => v.StoredProcedureName.ToLower() == storedProcedureName.ToLower())
                .ReturnsMultipleRows = true;
        }

        internal static void SetKickstartApi(List<KStoredProcedure> storedProcedures, string storedProcedureName)
        {
            storedProcedures.First(v => v.StoredProcedureName.ToLower() == storedProcedureName.ToLower())
                .GeneratedStoredProcedure.KickstartApi = true;
        }
    }
}