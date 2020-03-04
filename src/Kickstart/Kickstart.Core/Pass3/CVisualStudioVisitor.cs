using Kickstart.Interface;
using Kickstart.Pass2.CModel;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Pass2.CModel.Proto;
using Kickstart.Pass2.SqlServer;
using Kickstart.Pass3.gRPC;
using Kickstart.Pass3.VisualStudio2017;
using System;
using Kickstart.Pass3.Docker;
using Kickstart.Pass3.Git;
using Microsoft.Extensions.Logging;
using Kickstart.Pass2.CModel.AWS.DMS;
using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Pass3
{
    public class CVisualStudioVisitor : CVisualStudioVisitorBase, ICVisualStudioVisitor
    {
      
        public CVisualStudioVisitor(ILogger<CVisualStudioVisitorBase> logger, ICodeWriter codeWriter, ICSolutionVisitor solutionVisitor = null, ICProjectVisitor projectVisitor = null, ICProjectFileVisitor fileVisitor = null, ICInterfaceVisitor interfaceVisitor = null, ICClassVisitor classVisitor = null, ICMethodVisitor methodVisitor = null, ICPropertyVisitor propertyVisitor = null, ICParameterVisitor parameterVisitor = null, ICFieldVisitor fieldVisitor = null, ICConstructorVisitor constructorVisitor = null, ICAssemblyInfoVisitor assemblyInfoVisitor = null, ICClassAttributeVisitor classAttributeVisitor = null, ICMethodAttributeVisitor methodAttributeVisitor = null, ICProjectContentVisitor projectContentVisitor = null, ICDockerComposeFileVisitor dockerComposeFileVisitor = null, ICMetaRepoVisitor metaRepoVisitor = null, ICRepoVisitor repoVisitor = null, ICDockerFileServiceVisitor dockerFileServiceVisitor = null, ICEnumVisitor enumVisitor = null) : base(logger, codeWriter, solutionVisitor, projectVisitor, fileVisitor, interfaceVisitor, classVisitor, methodVisitor, propertyVisitor, parameterVisitor, fieldVisitor, constructorVisitor, assemblyInfoVisitor, classAttributeVisitor, methodAttributeVisitor, projectContentVisitor, dockerComposeFileVisitor, metaRepoVisitor, repoVisitor, dockerFileServiceVisitor, enumVisitor)
        {
        }

        public override void Visit(CPart part)
        {
            
            if (part is CTableType)
            {
                var tableType = part as CTableType;
                //var ddl = (part as STableType).TableTypeBody;
                if (tableType.DatabaseType == Utility.DataStoreTypes.SqlServer)
                {
                    var converter = new CTableTypeToSqlServerTableTypeConverter();
                    var ddl = converter.Convert(part as CTableType);
                    _codeWriter.Write(ddl);
                }
                else if (tableType.DatabaseType == Utility.DataStoreTypes.Postgres)
                {
                    var converter = new CTableTypeToPostgresTableTypeConverter();
                    var ddl = converter.Convert(part as CTableType);
                    _codeWriter.Write(ddl);
                }
                else
                    throw new NotImplementedException();
            }
            else if (part is CTable)
            {
                var table = part as CTable;

                if (table.DatabaseType == Utility.DataStoreTypes.SqlServer)
                {
                    if (!string.IsNullOrEmpty(table.TableText))
                    {
                        _codeWriter.Write(table.TableText);
                    }
                    else
                    {
                        var converter = new CTableToSqlServerTableConverter();
                        var ddl = converter.Convert(table);

                        _codeWriter.Write(ddl);
                    }
                }
                else if (table.DatabaseType == Utility.DataStoreTypes.Postgres)
                {

                    var converter = new CTableToPostgresTableConverter();
                    var ddl = converter.Convert(table);

                    _codeWriter.Write(ddl);
                }
                else if (table.DatabaseType == Utility.DataStoreTypes.MySql)
                {
                    var converter = new CTableToMySqlTableConverter();
                    var ddl = converter.Convert(table);
                    _codeWriter.Write(ddl);
                }
                else
                {
                    throw new NotImplementedException();
                }

                
            }
            else if (part is CView)
            {
                var ddl = (part as CView).ViewText;

                _codeWriter.Write(ddl);
            }
            else if (part is CSchema)
            {
                var schema = part as CSchema;
                if (schema.DatabaseType == Utility.DataStoreTypes.SqlServer)
                {

                    var converter = new CSchemaToSqlServerSchemaConverter();
                    var ddl = converter.Convert(schema);
                    _codeWriter.Write(ddl);
                }
                else if (schema.DatabaseType == Utility.DataStoreTypes.Postgres)
                {
                    var converter = new CSchemaToPostgresSchemaConverter();
                    var ddl = converter.Convert(schema);
                    _codeWriter.Write(ddl);
                }
                else if (schema.DatabaseType == Utility.DataStoreTypes.MySql)
                {
                    var converter = new CSchemaToMySqlSchemaConverter();
                    var ddl = converter.Convert(schema);
                    _codeWriter.Write(ddl);
                }
                else
                    throw new NotImplementedException();

            }
            else if (part is CStoredProcedure)
            {
                var storedProc = part as CStoredProcedure;
                ICStoredProcedureToStoredProcedureConverter converter = null;
                if (storedProc.DatabaseType == Utility.DataStoreTypes.SqlServer)
                {
                    converter = new CStoredProcedureToSSqlServerStoredProcedureConverter();

                }
                else if (storedProc.DatabaseType == Utility.DataStoreTypes.Postgres)
                {
                    converter = new CStoredProcedureToPostgresStoredProcedureConverter();
                }
                else
                {
                    throw new NotImplementedException();
                }
                
                var ddl = converter.Convert(storedProc);
                _codeWriter.Write(ddl);

            }
            else if (part is CRules)
            {
                var rules = part as CRules;
                var json = rules.ToJson();
                _codeWriter.Write(json);
            }

            else if (part is CSeedScript)
            {
                var seedScript = part as CSeedScript;
                _codeWriter.Write(seedScript.SeedScriptBody);
            }
            else if (part is CProtoFile)
            {
                
                var protoFile = part as CProtoFile;

                if (!string.IsNullOrEmpty(protoFile.SourceProtoText))
                {
                    //source may have full comments
                    _codeWriter.Write(protoFile.SourceProtoText);
                }
                else
                {
                    var converter = new SProtoFileToProtoFileConverter();
                    _codeWriter.Write(converter.Convert(protoFile));
                }
            }
            else if (part is CProtoFileRef)
            {
                var protoFileRef = part as CProtoFileRef;
                var converter = new SProtoFileToProtoFileConverter();
                _codeWriter.Write(converter.Convert(protoFileRef.ProtoFile));
            }
            else if (part is CText)
            {
                var textFile = part as CText;
                _codeWriter.Write(textFile.Text);
            }
            else if (part is CBatchFile)
            {
                var batchFile = part as CBatchFile;
                _codeWriter.Write(batchFile.BatchFileContent);
            }
            else
            {
                base.Visit(part);
            }
        }

      
    }
}