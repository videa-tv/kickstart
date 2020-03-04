using System.Collections.Generic;
using System.Data;
using System.Linq;
using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Pass2.DataLayerProject.Table;
using Kickstart.Pass2.SqlServer;
using Kickstart.Utility;

namespace Kickstart.Pass2.SqlProject
{

    public interface ISqlServerProjectService : IDataStoreProjectService
    {
        CProject BuildProject(KSolution mSolution, KDataStoreProject sqlKProject);
    }

    public class SqlServerProjectService : ISqlServerProjectService
    {
        private readonly ICTableToCStoredProcedureUpdateConverter _cTableToCStoredProcedureUpdateConverter;

        public SqlServerProjectService(ICTableToCStoredProcedureUpdateConverter cTableToCStoredProcedureUpdateConverter)
        {
            _cTableToCStoredProcedureUpdateConverter = cTableToCStoredProcedureUpdateConverter;
        }


        public string AuditSuffix { get; set; } = "Audit";


        public CProject BuildProject(KSolution mSolution, KDataStoreProject sqlKProject)
        {
            var dbProject = new CProject
            {
                ProjectName = sqlKProject.ProjectFullName, //$"{CompanyName}.{ProjectName}.{ProjectSuffix}",
                ProjectShortName = sqlKProject.ProjectName, // $@"{ProjectName}.{ProjectSuffix}",
                ProjectFolder = sqlKProject.ProjectFolder,
                ProjectType = CProjectType.SqlProj
            };

            dbProject.TemplateProjectPath =
                @"templates\SqlServer2014DbProject.sqlproj";
            dbProject.ProjectIs = CProjectIs.DataBase;

            var schemaConverter = new CSchemaToSqlServerSchemaConverter();
            //create schema
            AddSchemas(dbProject, mSolution, sqlKProject);

            var tables2 = sqlKProject.Table.Select(t => t.GeneratedTable).ToList();
            AddTables(dbProject, tables2);

            var tables = GetTables(sqlKProject.MockView, false);

            AddTables(dbProject, tables);

            if (sqlKProject.KickstartAuditTables)
            {
                var auditTables = GetTables(sqlKProject.MockView, true);
                //fixup the FK's
                FixupAuditFKs(auditTables);
                AddIsDeleteColumn(auditTables);
                AddTables(dbProject, auditTables);
            }

            AddTableTypes(dbProject, sqlKProject.TableType);

            AddViews(dbProject, sqlKProject.View);

            var allTables = new List<CTable>();
            allTables.AddRange(tables);
            allTables.AddRange(tables2);
            /*
            if (sqlKProject.KickstartCRUDStoredProcedures)
            {
                //AddInsertStoredProcedures(dataProject, tables);
                //AddUpdateStoredProcedures(dataProject, tables);

                AddInsertUpdateStoredProcedures(dbProject, allTables);
                //AddInsertUpdateStoredProcedures(dbProject, tables2);

            }*/
            if (true) //todo: add separate setting for CRUD vs GET
                AddGetStoredProcedures(dbProject, sqlKProject.StoredProcedure);
            /*
            if (sqlMProject.KickstartCRUDStoredProcedures)
            {
                //delete stored procedures
                AddDeleteStoredProcedures(dbProject, allTables);
                //AddDeleteStoredProcedures(dbProject, tables2);
            }
            */
            if (sqlKProject.KickstartTableSeedScripts)
                AddSeedScripts(dbProject, allTables);

            return dbProject;
        }


        private void AddViews(CProject dataProject, List<KView> views)
        {
            foreach (var view in views)
            {
                var fileName = $"{view.GeneratedView.ViewName}.sql";
                //var filePath = System.IO.Path.Combine(System.IO.Path.Combine(OutputRootPath, dataProject.ProjectFolder), fileName);

                dataProject.ProjectContent.Add(new CProjectContent
                {
                    Content = view.GeneratedView,
                    BuildAction = CBuildAction.Build,
                    File = new CFile {Folder = $"{view.Schema.SchemaName}\\Views", FileName = fileName}
                });
            }
        }

        private void AddIsDeleteColumn(List<CTable> auditTables)
        {
            foreach (var table in auditTables)
                table.Column.Add(new CColumn(table)
                {
                    ColumnName = "IsDelete",
                    ColumnType = DbType.Boolean,
                    ColumnTypeRaw = "bit",
                    IsNullable = false
                });
        }

        private void FixupAuditFKs(List<CTable> auditTables)
        {
            foreach (var table in auditTables)
            {
                table.TableName = $"{table.TableName}{AuditSuffix}";
                foreach (var column in table.Column)
                foreach (var fkColumn in column.ForeignKeyColumn)
                {
                    fkColumn.Table.Schema.SchemaName = $"{fkColumn.Table.Schema.SchemaName}{AuditSuffix}";
                    fkColumn.Table.TableName = $"{fkColumn.Table.TableName}{AuditSuffix}";
                }
            }
        }

        private void AddAuditTables(CProject dataProject, List<CTableType> tableTypes)
        {
            foreach (var tableType in tableTypes)
            {
                var fileName = $"{tableType.TableName}.sql";
                //var filePath = System.IO.Path.Combine(System.IO.Path.Combine(OutputRootPath, dataProject.ProjectFolder), fileName);

                dataProject.ProjectContent.Add(new CProjectContent
                {
                    Content = tableType,
                    BuildAction = CBuildAction.Build,
                    File = new CFile
                    {
                        Folder = $"{tableType.Schema.SchemaName}\\User Defined Types",
                        FileName = fileName
                    }
                });
            }
        }

        private void AddTableTypes(CProject dataProject, IEnumerable<KTableType> tableTypes)
        {
            foreach (var tableType in tableTypes)
            {
                var fileName = $"{tableType.GeneratedTableType.TableName}.sql";
                //var filePath = System.IO.Path.Combine(System.IO.Path.Combine(OutputRootPath, dataProject.ProjectFolder), fileName);

                dataProject.ProjectContent.Add(new CProjectContent
                {
                    Content = tableType.GeneratedTableType,
                    BuildAction = CBuildAction.Build,
                    File = new CFile
                    {
                        Folder = $"{tableType.GeneratedTableType.Schema.SchemaName}\\User Defined Types",
                        FileName = fileName
                    }
                });
            }
        }

        private void AddGetStoredProcedures(CProject dataProject, List<KStoredProcedure> storedProcedures)
        {
            foreach (var storedProcedure in storedProcedures)
            {
                if (storedProcedure.GeneratedStoredProcedure.GenerateAsEmbeddedQuery)
                    continue;

                var fileName = $"{storedProcedure.StoredProcedureName}.sql";
                //var filePath = System.IO.Path.Combine(System.IO.Path.Combine(OutputRootPath, dataProject.ProjectFolder), fileName);

                dataProject.ProjectContent.Add(new CProjectContent
                {
                    Content = storedProcedure.GeneratedStoredProcedure,
                    BuildAction = CBuildAction.Build,
                    File = new CFile
                    {
                        Folder = $"{storedProcedure.GeneratedStoredProcedure.Schema.SchemaName}\\Stored Procedures",
                        FileName = fileName
                    }
                });
            }
        }

        private void AddSchemas(CProject dataProject, KSolution mSolution, KDataStoreProject sqlKProject)
        {
            foreach (var mockView in sqlKProject.MockView)
            {
                AddSchema(dataProject, mockView.Schema.SchemaName);
                if (sqlKProject.KickstartAuditTables)
                    AddSchema(dataProject, $"{mockView.Schema.SchemaName}{AuditSuffix}");

                AddSchema(dataProject, $"{mockView.Schema.SchemaName}Api");
            }
            foreach (var table in sqlKProject.Table)
                AddSchema(dataProject, $"{table.GeneratedTable.Schema.SchemaName}");

            foreach (var view in sqlKProject.View)
                AddSchema(dataProject, $"{view.GeneratedView.Schema.SchemaName}");

            foreach (var storeProcedure in sqlKProject.StoredProcedure)
                AddSchema(dataProject, $"{storeProcedure.GeneratedStoredProcedure.Schema.SchemaName}");
        }

        private void AddSchema(CProject dataProject, string schemaName)
        {
            if (schemaName.ToLower() == "dbo")
                return;

            if (dataProject.ProjectContent.Exists(pc =>
                pc.Content is CSchema && (pc.Content as CSchema).SchemaName == schemaName))
                return;

            dataProject.ProjectContent.Add(new CProjectContent
            {
                Content = new CSchema {SchemaName = schemaName, DatabaseType = DataStoreTypes.SqlServer},
                BuildAction = CBuildAction.Build,
                File = new CFile {Folder = "Security", FileName = $"{schemaName}.sql"}
            });
        }

        private void AddSeedScripts(CProject dataProject, List<CTable> tables)
        {
            var executeScripts = new CodeWriter();
            foreach (var table in tables)
            {
                if (table.Row.Count == 0)
                    continue; //no data to seed

                var converter = new CTableToSSeedScriptConverter();
                var seedScript = converter.Convert(table);
                dataProject.ProjectContent.Add(new CProjectContent
                {
                    Content = seedScript,
                    BuildAction = CBuildAction.None,
                    File = new CFile
                    {
                        Folder = $@"Scripts\Post Deployment\Seed",
                        FileName = $"{seedScript.SeedScriptName}.sql"
                    }
                });
                executeScripts.WriteLine($@":r .\Seed\{seedScript.SeedScriptName}.sql");
            }
            //add script to execute them
            var executeSeedScript = new CSeedScript
            {
                SeedScriptBody = executeScripts.ToString(),
                SeedScriptName = "Script.PostDeployment"
            };
            dataProject.ProjectContent.Add(new CProjectContent
            {
                Content = executeSeedScript,
                BuildAction = CBuildAction.PostDeploy,
                File = new CFile
                {
                    Folder = $@"Scripts\Post Deployment\",
                    FileName = $"{executeSeedScript.SeedScriptName}.sql"
                }
            });
        }

        private void AddUpdateStoredProcedures(CProject dataProject, List<CTable> tables)
        {
            //update stored procedures
            foreach (var table in tables)
            {
                var storedProcedure = _cTableToCStoredProcedureUpdateConverter.Convert(table);

                var fileName = $"{storedProcedure.StoredProcedureName}.sql";
                //var filePath = System.IO.Path.Combine(System.IO.Path.Combine(OutputRootPath, dataProject.ProjectFolder), fileName);

                dataProject.ProjectContent.Add(new CProjectContent
                {
                    Content = storedProcedure,
                    BuildAction = CBuildAction.Build,
                    File = new CFile
                    {
                        Folder = $"{storedProcedure.Schema.SchemaName}\\Stored Procedures",
                        FileName = fileName
                    }
                });
            }
        }

        private bool AddInsertStoredProcedures(CProject dataProject, List<CTable> tables)
        {
            var first = true;
            foreach (var table in tables)
            {

                var converter3 = new CTableToSqlServerStoredProcedureAddConverter();
                //table.Schema.SchemaName = "changedschema";
                var storedProcedure = converter3.Convert(table);

                if (first)
                {
                    //create a schema for the stored procs
                    dataProject.ProjectContent.Add(new CProjectContent
                    {
                        Content = storedProcedure.Schema,
                        BuildAction = CBuildAction.Build,
                        File = new CFile {Folder = "Security", FileName = $"{storedProcedure.Schema.SchemaName}.sql"}
                    });
                    first = false;
                }
                var fileName = $"{storedProcedure.StoredProcedureName}.sql";
                //var filePath = System.IO.Path.Combine(System.IO.Path.Combine(OutputRootPath, dataProject.ProjectFolder), fileName);

                dataProject.ProjectContent.Add(new CProjectContent
                {
                    Content = storedProcedure,
                    BuildAction = CBuildAction.Build,
                    File = new CFile
                    {
                        Folder = $"{storedProcedure.Schema.SchemaName}\\Stored Procedures",
                        FileName = fileName
                    }
                });
            }

            return first;
        }

        private void AddInsertUpdateStoredProcedures(CProject dataProject, List<CTable> tables)
        {
            //merge version of stored procs
            //update stored procedures
            foreach (var table in tables)
            {
                var converter3 = new CTableToSqlServerStoredProcedureAddUpdateConverter();

                var storedProcedure = converter3.Convert(table);

                AddSchema(dataProject, $"{storedProcedure.Schema.SchemaName}");
                var fileName = $"{storedProcedure.StoredProcedureName}.sql";
                //var filePath = System.IO.Path.Combine(System.IO.Path.Combine(OutputRootPath, dataProject.ProjectFolder), fileName);

                dataProject.ProjectContent.Add(new CProjectContent
                {
                    Content = storedProcedure,
                    BuildAction = CBuildAction.Build,
                    File = new CFile
                    {
                        Folder = $"{storedProcedure.Schema.SchemaName}\\Stored Procedures",
                        FileName = fileName
                    }
                });
            }
        }

        private void AddDeleteStoredProcedures(CProject dataProject, List<CTable> tables)
        {
            foreach (var table in Enumerable.Reverse(tables))
            {
                var converter3 = new CTableToSStoredProcedureDeleteConverter();

                var storedProcedure = converter3.Convert(table);


                var fileName = $"{storedProcedure.StoredProcedureName}.sql";
                //var filePath = System.IO.Path.Combine(System.IO.Path.Combine(OutputRootPath, dataProject.ProjectFolder), fileName);

                dataProject.ProjectContent.Add(new CProjectContent
                {
                    Content = storedProcedure,
                    BuildAction = CBuildAction.Build,
                    File = new CFile
                    {
                        Folder = $"{storedProcedure.Schema.SchemaName}\\Stored Procedures",
                        FileName = fileName
                    }
                });
            }
        }

        private void AddTables(CProject dataProject, List<CTable> tables)
        {
            foreach (var table in tables)
            {
                //var converter3 = new STableToSqlServerTableConverter();
                //table.Schema.SchemaName = "changedschema";
                //var ddl = converter3.Convert(table);

                var fileName = $"{table.TableName}.sql";
                //var filePath = System.IO.Path.Combine(System.IO.Path.Combine(OutputRootPath, dataProject.ProjectFolder), fileName);

                dataProject.ProjectContent.Add(new CProjectContent
                {
                    Content = table,
                    BuildAction = CBuildAction.Build,
                    File = new CFile {Folder = $"{table.Schema.SchemaName}\\Tables", FileName = fileName}
                });
            }
        }

        private List<CTable> GetTables(IEnumerable<KView> views, bool isAudit)
        {
            var tables = new List<CTable>();
            foreach (var view in views)
            {
                var converter2 = new CViewToCTableConverter();

                CView view2 = view;
                if (view.GeneratedView != null)
                    view2 = view.GeneratedView;
                var table = converter2.Convert(view2);

                var schema = view.Schema.SchemaName;
                if (isAudit)
                    schema = $"{schema}{AuditSuffix}";
                table.Schema = new CSchema {SchemaName = schema};

                //temp hack
                table.Column.First().IsPrimaryKey = true;
                tables.Add(table);
            }
            return tables;
        }
    }
}