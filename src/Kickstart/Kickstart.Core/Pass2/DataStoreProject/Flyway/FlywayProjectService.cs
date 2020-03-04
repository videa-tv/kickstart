using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel;
using Kickstart.Pass2.CModel.AWS.DMS;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Pass2.SqlServer;
using Kickstart.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Kickstart.Pass2.DataStoreProject
{
    public class FlywayProjectService : IFlywayProjectService
    {
        KDataStoreProject _sqlKProject;

        private readonly ICTableToDmsRuleConverter _tableToDmsRuleConverter;
        private readonly IFlywayFileNameService _flywayFileNameService;
        public FlywayProjectService(IFlywayFileNameService flywayFileNameService, ICTableToDmsRuleConverter tableToDmsRuleConverter)
        {
            _flywayFileNameService = flywayFileNameService;
            _tableToDmsRuleConverter = tableToDmsRuleConverter;
        }

        public CProject BuildProject(KSolution kSolution, KDataStoreProject sqlKProject)
        {
            _sqlKProject = sqlKProject;

            var dbProject = new CProject
            {
                ProjectName = sqlKProject.ProjectFullName, 
                ProjectShortName = sqlKProject.ProjectName,  
                ProjectFolder = sqlKProject.ProjectFolder,
                ProjectType = CProjectType.FlywayProj
            };
            dbProject.TemplateProjectPath =
                @"templates\NetStandard20ClassLibrary.csproj";
            dbProject.ProjectIs = CProjectIs.DatabaseFlyway;

            addFlyway(dbProject);

            AddSchemas(dbProject, kSolution, sqlKProject);

            var tables2 = sqlKProject.Table.Select(t => t.GeneratedTable).ToList();
            AddTables(dbProject, tables2);

            AddTableTypes(dbProject, sqlKProject.TableType.Select(tt=> tt.GeneratedTableType).ToList());

            if (true) //todo: add separate setting for CRUD vs GET
                AddGetStoredProcedures(dbProject, sqlKProject.StoredProcedure.Select(s=>s.GeneratedStoredProcedure).ToList());

    
            AddDmsRules(dbProject, tables2);
            return dbProject;
        }

        private void AddTableTypes(CProject dataProject, IEnumerable<CTableType> tableTypes)
        {
            foreach (var tableType in tableTypes)
            {
                var fileName = _flywayFileNameService.GetFlywayFileName(tableType); // $"{FlywayVersionNumber}__Create_Type_{tableType.Schema.SchemaName}_{tableType.TableName}.sql";

                dataProject.ProjectContent.Add(new CProjectContent
                {
                    Content = tableType,
                    BuildAction = CBuildAction.Build,
                    File = new CFile { Folder = "migrations", FileName = fileName }
                });
            }
        }


        private void AddSchemas(CProject dataProject, KSolution mSolution, KDataStoreProject sqlKProject)
        {
            /*
            foreach (var mockView in sqlKProject.MockView)
            {
                AddSchema(dataProject, mockView.Schema.SchemaName);
                if (sqlKProject.KickstartAuditTables)
                    AddSchema(dataProject, $"{mockView.Schema.SchemaName}");

                AddSchema(dataProject, $"{mockView.Schema.SchemaName}Api");
            }*/
            foreach (var table in sqlKProject.Table)
                AddSchema(dataProject, $"{table.GeneratedTable.Schema.SchemaName}");

            foreach (var view in sqlKProject.View)
                AddSchema(dataProject, $"{view.GeneratedView.Schema.SchemaName}");

            foreach (var storeProcedure in sqlKProject.StoredProcedure)
                AddSchema(dataProject, $"{storeProcedure.GeneratedStoredProcedure.Schema.SchemaName}");
        }

        private void AddSchema(CProject dataProject, string schemaName)
        {
            //if (schemaName.ToLower() == "dbo")
            //    return;

            if (dataProject.ProjectContent.Exists(pc =>
                pc.Content is CSchema && (pc.Content as CSchema).SchemaName == schemaName))
                return;

            var fileName = _flywayFileNameService.GetFlywayFileName(new CSchema { SchemaName = schemaName });
            dataProject.ProjectContent.Add(new CProjectContent
            {
                Content = new CSchema { SchemaName = schemaName, DatabaseType = _sqlKProject.DataStoreType },
                BuildAction = CBuildAction.Build,
                File = new CFile { Folder = "migrations", FileName = fileName }
            });
        }

        private void AddTables(CProject dataProject, List<CTable> tables)
        {
            foreach (var table in tables)
            {
                var fileName = _flywayFileNameService.GetFlywayFileName(table);
                
                dataProject.ProjectContent.Add(new CProjectContent
                {
                    Content = table,
                    BuildAction = CBuildAction.Build,
                    File = new CFile { Folder = $"migrations", FileName = fileName }
                });
            }
        }
        private void AddDmsRules(CProject dataProject, List<CTable> tables)
        {
            var fileName = $"DmsRules.json";

            var rules = new CRules();

            foreach (var table in tables)
            {
                var rList= _tableToDmsRuleConverter.Convert(table);
                rules.Rule.AddRange(rList);
            }
                
            dataProject.ProjectContent.Add(new CProjectContent
            {
                Content = rules,
                BuildAction = CBuildAction.Build,
                File = new CFile { Folder = $"", FileName = fileName }
            });
        
        }
        private void AddGetStoredProcedures(CProject dbProject, List<CStoredProcedure> storedProcedures)
        {
            foreach (var storedProcedure in storedProcedures)
            {
                if (storedProcedure.GenerateAsEmbeddedQuery)
                    continue;

                var fileName = _flywayFileNameService.GetFlywayFileName(storedProcedure);
                dbProject.ProjectContent.Add(new CProjectContent
                {
                    Content = storedProcedure,
                    BuildAction = CBuildAction.Build,
                    File = new CFile
                    {
                        Folder = $"migrations",
                        FileName = fileName
                    }
                });
            }
        }

        private void addFlyway(CProject dbProject)
        {
           
            if (_sqlKProject.DataStoreType == Utility.DataStoreTypes.Postgres)
            {
                addBoilerplateFile(dbProject, "Postgres.clean.cmd", "clean.cmd");
                addBoilerplateFile(dbProject, "Postgres.clean.sh", "clean.sh");
                addBoilerplateFile(dbProject, "Postgres.migrate.cmd", "migrate.cmd");
                addBoilerplateFile(dbProject, "Postgres.migrate.sh", "migrate.sh");
                addBoilerplateFile(dbProject, "Postgres.repair.cmd", "repair.cmd");
                addBoilerplateFile(dbProject, "Postgres.repair.sh", "repair.sh");
            }
            else if (_sqlKProject.DataStoreType == Utility.DataStoreTypes.MySql)
            {
                addBoilerplateFile(dbProject, "MySql.clean.cmd", "clean.cmd");
                addBoilerplateFile(dbProject, "MySql.clean.sh", "clean.sh");
                addBoilerplateFile(dbProject, "MySql.migrate.cmd", "migrate.cmd");
                addBoilerplateFile(dbProject, "MySql.migrate.sh", "migrate.sh");
                addBoilerplateFile(dbProject, "MySql.repair.cmd", "repair.cmd");
                addBoilerplateFile(dbProject, "MySql.repair.sh", "repair.sh");
            }
            else
                throw new NotImplementedException();

         

        }

        void addBoilerplateFile(CProject dbProject, string fileName, string targetFileName)
        {
            var fileContent = readResourceFile($@"Flyway.{fileName}");

            fileContent = fileContent.Replace("##databasename##", $"{_sqlKProject.ProjectName.ToLower()}");

            dbProject.ProjectContent.Add(new CProjectContent
            {
                Content = new CBatchFile { BatchFileContent = fileContent },
                BuildAction = CBuildAction.None,
                File = new CFile { Folder = $@"", FileName = targetFileName, Encoding = Encoding.ASCII  }
            });
        }
        private string readResourceFile(string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"Kickstart.Core.NetStandard.Boilerplate.{fileName}";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

    }
}
