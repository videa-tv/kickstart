using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Pass2.DataLayerProject.Table;
using Kickstart.Pass2.SqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kickstart.Utility;

namespace Kickstart.Pass2.DataLayerProject
{
    public class DataLayerServiceProjectServiceBase
    {

        protected CProject _dataProject;
        protected KDataStoreProject _dataStoreKProject;
        protected KDataLayerProject _dataLayerKProject;
        protected List<CStoredProcedure> _storedProcedures;
        protected IEnumerable<KTable> _tables;

        protected IEnumerable<KTableType> _tableTypes;

        public CClass BuildParameterEntityClass(CStoredProcedure storedProcedure, string parameterSetName)
        {
            if (string.IsNullOrEmpty(parameterSetName))
                return null;

            var converter = new CStoredProcedureToCClassConverter();
            var @class = converter.ConvertByParameterSet(storedProcedure);

            //overrite the default namespace logic
            @class.Namespace = new CNamespace
            {
                NamespaceName =
                    $"{_dataLayerKProject.CompanyName}.{_dataLayerKProject.ProjectName}{_dataLayerKProject.NamespaceSuffix}.Data.Entities"
            };
            return @class;
        }
        protected CClass GetParameterDto(KView kView, string parameterSetName)
        {
            if (string.IsNullOrEmpty(parameterSetName))
                return null;

            var converter = new KQueryToCClassConverter();
            var @class = converter.ConvertByParameterSet(kView);

            //overrite the default namespace logic
            @class.Namespace = new CNamespace
            {
                NamespaceName =
                    $"{_dataLayerKProject.CompanyName}.{_dataLayerKProject.ProjectName}{_dataLayerKProject.NamespaceSuffix}.Data.Entities"
            };
            return @class;
        }

        public CClass BuildResultEntityClass(CStoredProcedure storedProcedure, string resultSetName, KDataLayerProject dataLayerKProject)
        {
            if (string.IsNullOrEmpty(resultSetName))
                return null;

            var converter = new CStoredProcedureToCClassConverter();
            var @class = converter.ConvertByResultSet(storedProcedure);

            //overrite the default namespace logic
            @class.Namespace = new CNamespace
            {
                NamespaceName =
                    $"{dataLayerKProject.CompanyName}.{dataLayerKProject.ProjectName}{dataLayerKProject.NamespaceSuffix}.Data.Entities"
            };
            return @class;
        }

        protected List<CMethod> GetDataServiceMethods()
        {
            var methods = new List<CMethod>();
            if (_dataLayerKProject.KickstartBulkStore)
                methods.Add(BuildBulkStoreMethod());

            methods.AddRange(GetDataServiceMethodsFromStoredProcs());
            methods.AddRange(GetDataServiceMethodsFromQueries());
            return methods;
        }

        protected virtual CMethod BuildBulkStoreMethod()
        {
            throw new NotImplementedException();
        }
         protected List<CClass> BuildEntityClasses(IEnumerable<CStoredProcedure> storedProcedures, IEnumerable<KTable> tables,
           IEnumerable<KTableType> tableTypes, IEnumerable<CView> views, CProject dataProject)
        {
            var entityClasses = new List<CClass>();
            foreach (var storedProcedure in storedProcedures)
            {
                var entityParameterClass = BuildParameterEntityClass(storedProcedure, storedProcedure.ParameterSetName);
                if (entityParameterClass != null && !entityClasses.Exists(c => c.ClassName == entityParameterClass.ClassName))
                {
                    entityClasses.Add(entityParameterClass);
                    dataProject.ProjectContent.Add(new CProjectContent
                    {
                        BuildAction = CBuildAction.DoNotInclude,
                        Class = entityParameterClass,
                        File = new CFile { Folder = "Entities", FileName = $"{entityParameterClass.ClassName}.cs" }
                    });
                }
                var entityResultClass = BuildResultEntityClass(storedProcedure, storedProcedure.ResultSetName, _dataLayerKProject);
                if (entityResultClass != null && !entityClasses.Exists(c => c.ClassName == entityResultClass.ClassName))
                {
                    entityClasses.Add(entityResultClass);
                    dataProject.ProjectContent.Add(new CProjectContent
                    {
                        BuildAction = CBuildAction.DoNotInclude,
                        Class = entityResultClass,
                        File = new CFile { Folder = "Entities", FileName = $"{entityResultClass.ClassName}.cs" }
                    });
                }
            }
            foreach (var table in tables)
            {
                var dtoTableClass =
                    GetTableDto(tables,table, table.GeneratedTable.TableName);
                if (dtoTableClass != null && !entityClasses.Exists(c => c.ClassName == dtoTableClass.ClassName))
                {
                    entityClasses.Add(dtoTableClass);
                    dataProject.ProjectContent.Add(new CProjectContent
                    {
                        BuildAction = CBuildAction.DoNotInclude,
                        Class = dtoTableClass,
                        File = new CFile { Folder = "Entities", FileName = $"{dtoTableClass.ClassName}.cs" }
                    });
                }
            }

            /*
            foreach (var tableType in tableTypes)
            {
                var dtoTableTypeClass =
                    GetTableTypeDto(tableType.GeneratedTableType, tableType.GeneratedTableType.TableName);
                if (dtoTableTypeClass != null && !entityClasses.Exists(c => c.ClassName == dtoTableTypeClass.ClassName))
                {
                    entityClasses.Add(dtoTableTypeClass);
                    dataProject.ProjectContent.Add(new CProjectContent
                    {
                        BuildAction = CBuildAction.DoNotInclude,
                        Class = dtoTableTypeClass,
                        File = new CFile { Folder = "Entities", FileName = $"{dtoTableTypeClass.ClassName}.cs" }
                    });
                }
            }
            */

            foreach (var view in views)
            {
                var dtoViewClass =
                    GetViewDto(view);

                if (dtoViewClass != null && !entityClasses.Exists(c => c.ClassName == dtoViewClass.ClassName))
                {
                    entityClasses.Add(dtoViewClass);
                    dataProject.ProjectContent.Add(new CProjectContent
                    {
                        BuildAction = CBuildAction.DoNotInclude,
                        Class = dtoViewClass,
                        File = new CFile { Folder = "Entities\\Views", FileName = $"{dtoViewClass.ClassName}.cs" }
                    });
                }
            }

            return entityClasses;
        }
        protected CClass GetTableDto(IEnumerable<KTable> allTables, KTable table, string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
                return null;

            var converter = new CTableToCClassConverter();
            var @class = converter.Convert(table.GeneratedTable, allTables.Select(kt=>kt.GeneratedTable), false);
            @class.ClassName += "Dto";
            @class.DerivedFrom = table;
            //overrite the default namespace logic
            @class.Namespace = new CNamespace
            {
                NamespaceName =
                    $"{_dataLayerKProject.CompanyName}.{_dataLayerKProject.ProjectName}{_dataLayerKProject.NamespaceSuffix}.Data.Entities"
            };
            return @class;
        }


        protected CClass GetTableTypeDto(CTableType tableType, string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
                return null;

            var converter = new CTableTypeToCClassConverter();
            var @class = converter.Convert(tableType);
            @class.DerivedFrom = tableType;
            //overrite the default namespace logic
            @class.Namespace = new CNamespace
            {
                NamespaceName =
                    $"{_dataLayerKProject.CompanyName}.{_dataLayerKProject.ProjectName}{_dataLayerKProject.NamespaceSuffix}.Data.Entities"
            };
            return @class;
        }

        private CClass GetViewDto(CView view)
        {
            if (string.IsNullOrEmpty(view.ViewName))
                return null;

            var preConverter = new CViewToCTableConverter();
            var table = preConverter.Convert(view);

            var converter = new CTableToCClassConverter();
            var @class = converter.Convert(table, null, false);

            //overrite the default namespace logic
            @class.Namespace = new CNamespace
            {
                NamespaceName =
                    $"{_dataLayerKProject.CompanyName}.{_dataLayerKProject.ProjectName}{_dataLayerKProject.NamespaceSuffix}.Data.Entities"
            };
            return @class;
        }

        private List<CMethod> GetDataServiceMethodsFromStoredProcs()
        {
            var methods = new List<CMethod>();
            foreach (var storedProcedure in _dataStoreKProject.StoredProcedure.Select(s => s.GeneratedStoredProcedure))
            {
                var dtoParameterClass = BuildParameterEntityClass(storedProcedure, storedProcedure.ParameterSetName);
                var dtoResultClass = BuildResultEntityClass(storedProcedure, storedProcedure.ResultSetName, _dataLayerKProject);

                var method = new CMethod
                {
                    AccessModifier = CAccessModifier.Public,
                    IsAsync = true,
                    ReturnType = storedProcedure.HasResultSet
                        ? $"Task<IEnumerable<{dtoResultClass.ClassName}>>"
                        : "Task<bool>",
                    MethodName = $"{storedProcedure.StoredProcedureName}Async",
                    DerivedFrom = storedProcedure
                };
                if (dtoParameterClass != null)
                {
                    var methodParameter = new CParameter
                    {
                        Type = dtoParameterClass.ClassName,
                        ParameterName = dtoParameterClass.ClassName.ToLower() //.ParameterNameCamelCase
                    };
                    method.Parameter.Add(methodParameter);
                }
                else
                {
                    foreach (var parameter in storedProcedure.Parameter)
                    {
                        var type = "object";
                        if (!parameter.ParameterTypeIsUserDefined)
                        {
                            type = SqlMapper.ParseValueAsSqlDbType(parameter.ParameterTypeRaw).ToClrTypeName();

                            if (parameter.IsCollection)
                            {
                                type = $"IEnumerable<{type}>";
                            }
                        }
                        else if (parameter.ParameterTypeIsUserDefined)
                        {
                            var parameterType = parameter.ParameterTypeRaw;
                            if (parameterType.StartsWith("tt"))
                            {
                                parameterType = parameterType.Substring(2, parameterType.Length - 2);
                            }
                            var tableDto = FindTableByParameterTypeRaw(parameterType);

                            type = GetTableDto(_tables, tableDto, tableDto.TableName).ClassName;
                            type = $"IEnumerable<{type}>";
                            /*
                            var tableType = FindTableType(parameter.ParameterTypeRaw);
                            
                            type = GetTableTypeDto(tableType.GeneratedTableType, tableType.TableTypeName).ClassName;
                            type = $"IEnumerable<{type}>";
                            */
                        }

                        var methodParameter = new CParameter
                        {
                            Type = type,
                            ParameterName = parameter.ParameterNameCamelCase
                        };
                        method.Parameter.Add(methodParameter);
                    }
                }

                var codeWriter = new CodeWriter();
                codeWriter.WriteLine($@"using var connection = DbProvider.GetConnection();");
                codeWriter.WriteLine("");
                codeWriter.WriteLine("var sqlParams = new DynamicParameters();");
                if (dtoParameterClass != null)
                    foreach (var property in dtoParameterClass.Property)
                    {
                        var propertyName = property.PropertyName;
                        if (storedProcedure.ConvertToSnakeCase)
                        {
                            propertyName = propertyName.ToSnakeCase();
                        }
                        var dbType = SqlMapper.GetDbType(property.Type);
                        codeWriter.WriteLine(
                            $@"sqlParams.Add(""@{propertyName}"", {dtoParameterClass.ClassName.ToLower()}.{
                                    property.PropertyName
                                }, DbType.{dbType});");
                    }
                else
                    foreach (var parameter in storedProcedure.Parameter)
                    {
                        var parameterName = parameter.ParameterName;
                        if (storedProcedure.ConvertToSnakeCase)
                            parameterName = parameterName.ToSnakeCase();

                        if (parameter.ParameterTypeIsUserDefined)
                        {
                            var tableType = FindTableType(parameter.ParameterTypeRaw);
                            var tableTypeClass = BuildTableTypeList(tableType);
                            if (tableTypeClass != null)
                            {
                                codeWriter.WriteLine(
                                    $@"sqlParams.Add(""@{parameterName}"", new {tableTypeClass.ClassName}({
                                            parameter.ParameterNameCamelCase
                                        }), DbType.Object);");
                            }
                            else
                            {
                                codeWriter.WriteLine("//todo");
                            }
                        }
                        else
                        {
                            var dbType = parameter.ParameterType.ToClrType().ToDbType();
                            codeWriter.WriteLine(
                                $@"sqlParams.Add(""@{parameterName}"", {
                                        parameter.ParameterNameCamelCase
                                    }, DbType.{dbType});");
                        }
                    }
                codeWriter.WriteLine("");
                var resultClassName = dtoResultClass.ClassName;
                var storedProcName = storedProcedure.StoredProcedureName;
                var storedProcNameOriginal = storedProcedure.StoredProcedureName;

                var schemaName = storedProcedure.Schema.SchemaName;

                if (storedProcedure.ConvertToSnakeCase)
                {
                    resultClassName = resultClassName.ToSnakeCase();
                    storedProcName = storedProcName.ToSnakeCase();
                    schemaName = schemaName.ToSnakeCase();
                }
                if (storedProcedure.GenerateAsEmbeddedQuery)
                {
                    codeWriter.WriteLine($@"var sqlQuery = Assembly.GetExecutingAssembly().GetEmbeddedQuery(""{storedProcNameOriginal}"");");
                }
                if (storedProcedure.HasResultSet)
                {

                    codeWriter.WriteLine($@"var result = await connection");
                    codeWriter.WriteLine($@"     .QueryAsync<{dtoResultClass.ClassName}>(");

                    if (storedProcedure.GenerateAsEmbeddedQuery)
                    {
                        codeWriter.WriteLine(
                        $@"     sqlQuery,");
                        codeWriter.WriteLine($@"        sqlParams,");

                        codeWriter.WriteLine($@"        commandType: CommandType.Text)");
                    }
                    else
                    {
                        codeWriter.WriteLine(
                        $@"     ""{schemaName}.{storedProcName}"",");
                        codeWriter.WriteLine($@"        sqlParams,");

                        codeWriter.WriteLine($@"        commandType: CommandType.StoredProcedure)");
                    }
                    codeWriter.WriteLine($@"     .ConfigureAwait(false);");
                    codeWriter.WriteLine("");
                    codeWriter.WriteLine($@"return result.ToList();");
                }
                else
                {
                    codeWriter.WriteLine($@"var result = await connection");
                    codeWriter.WriteLine($@"     .ExecuteAsync(");
                    if (storedProcedure.GenerateAsEmbeddedQuery)
                    {
                        codeWriter.WriteLine(
                            $@"     sqlQuery,");
                        codeWriter.WriteLine($@"        sqlParams,");

                        codeWriter.WriteLine($@"        commandType: CommandType.Text)");
                    }
                    else
                    {
                        codeWriter.WriteLine(
                            $@"     ""{schemaName}.{storedProcName}"",");
                        codeWriter.WriteLine($@"        sqlParams,");

                        codeWriter.WriteLine($@"        commandType: CommandType.StoredProcedure)");
                    }
                    codeWriter.WriteLine($@"     .ConfigureAwait(false);");
                    codeWriter.WriteLine("");
                    codeWriter.WriteLine($@"return result > 0;");
                }
                codeWriter.Write(@"");
                method.CodeSnippet = codeWriter.ToString();
                methods.Add(method);
            }
            return methods;
        }

        protected KTable FindTableByParameterTypeRaw(string parameterTypeRaw)
        {
            var foundTable = _tables.FirstOrDefault(t =>
                t.GeneratedTable.TableName == parameterTypeRaw);
            if (foundTable == null)
            {
                throw new ApplicationException($"Table not found: {parameterTypeRaw}");
            }

            return foundTable;
        }

        
        protected KTableType FindTableType(string parameterTypeRaw)
        {
            var tableType = _tableTypes.FirstOrDefault(t =>
                t.GeneratedTableType.TableName == parameterTypeRaw);

            if (tableType == null)
            {
                throw new ApplicationException($"TableType not found for: {parameterTypeRaw}");
            }

            return tableType;
        }

        protected virtual CClass BuildTableTypeList(KTableType kTableType)
        {
            throw new NotImplementedException();
        }

        public virtual CInterface BuildIDbDiagnosticsFactoryInterface(CProject dataProject)
        {
            return null;
        }

        private List<CMethod> GetDataServiceMethodsFromQueries()
        {
            var methods = new List<CMethod>();
            foreach (var kQuery in _dataStoreKProject.Query)
            {
                if (kQuery.GeneratedQuery == null)
                {
                    //todo: shouldn't have any null
                    continue;
                }
                var query = kQuery.GeneratedQuery;

                CClass dtoParameterClass = null;// GetParameterDto(query, query.ParameterSetName);
                CClass dtoResultClass = null;//GetResultDto(query, query.ResultSetName);

                var method = new CMethod
                {
                    AccessModifier = CAccessModifier.Public,
                    IsAsync = true,
                    ReturnType = query.HasResultSet
                        ? $"Task<IEnumerable<{dtoResultClass.ClassName}>>"
                        : "Task<bool>",
                    MethodName = $"{query.QueryName}Async",
                    //DerivedFrom = query
                };

                if (dtoParameterClass != null)
                {
                    var methodParameter = new CParameter
                    {
                        Type = dtoParameterClass.ClassName,
                        ParameterName = dtoParameterClass.ClassName.ToLower() //.ParameterNameCamelCase
                    };
                    method.Parameter.Add(methodParameter);
                }
                else
                {
                    foreach (var parameter in query.Parameter)
                    {
                        var type = "object";
                        if (!parameter.ParameterTypeIsUserDefined)
                        {
                            type = SqlMapper.ParseValueAsSqlDbType(parameter.ParameterTypeRaw).ToClrTypeName();

                            if (parameter.IsCollection)
                            {
                                type = $"IEnumerable<{type}>";
                            }
                        }
                        else if (parameter.ParameterTypeIsUserDefined)
                        {
                            var parameterType = parameter.ParameterTypeRaw;
                            if (parameterType.StartsWith("tt"))
                            {
                                parameterType = parameterType.Substring(2, parameterType.Length - 2);
                            }
                            var tableDto = FindTableByParameterTypeRaw(parameterType);

                            type = GetTableDto(_tables, tableDto, tableDto.TableName).ClassName;
                            type = $"IEnumerable<{type}>";
                            /*
                            var tableType = FindTableType(parameter.ParameterTypeRaw);
                            
                            type = GetTableTypeDto(tableType.GeneratedTableType, tableType.TableTypeName).ClassName;
                            type = $"IEnumerable<{type}>";
                            */
                        }


                        var methodParameter = new CParameter
                        {
                            Type = type,
                            ParameterName = parameter.ParameterNameCamelCase
                        };
                        method.Parameter.Add(methodParameter);
                    }
                }

                var codeWriter = new CodeWriter();
                codeWriter.WriteLine($@"using var connection = DbProvider.GetConnection();");
                codeWriter.WriteLine("");
                codeWriter.WriteLine("var sqlParams = new DynamicParameters();");

                if (dtoParameterClass != null)
                    foreach (var property in dtoParameterClass.Property)
                    {
                        var propertyName = property.PropertyName;
                        /*
                        if (ConnectsToDatabaseType == DatabaseTypes.Postgres)
                            propertyName = propertyName.ToSnakeCase();
                            */
                        var dbType = SqlMapper.GetDbType(property.Type);
                        codeWriter.WriteLine(
                            $@"sqlParams.Add(""@{propertyName}"",{dtoParameterClass.ClassName.ToLower()}.{
                                    property.PropertyName
                                }, DbType.{dbType});");
                    }
                else
                    foreach (var parameter in query.Parameter)
                    {
                        var parameterName = parameter.ParameterName;
                        /*
                        if (ConnectsToDatabaseType == DatabaseTypes.Postgres)
                            parameterName = parameterName.ToSnakeCase();
                            */
                        if (parameter.ParameterTypeIsUserDefined)
                        {
                            var tableType = FindTableType(parameter.ParameterTypeRaw);
                            var tableTypeClass = BuildTableTypeList(tableType);


                            codeWriter.WriteLine(
                                $@"sqlParams.Add(""@{parameterName}"",new {tableTypeClass.ClassName}({
                                        parameter.ParameterNameCamelCase
                                    }), DbType.Object);");
                        }
                        else
                        {
                            var dbType = parameter.ParameterType.ToClrType().ToDbType();
                            codeWriter.WriteLine(
                                $@"sqlParams.Add(""@{parameter.ParameterName}"",{
                                        parameter.ParameterNameCamelCase
                                    }, DbType.{dbType});");
                        }
                    }

                codeWriter.WriteLine("");
                if (query.HasResultSet)
                {
                    var schemaName = query.Schema.SchemaName;
                    var queryName = query.QueryName;
                    var resultClassName = dtoResultClass.ClassName;
                    /*
                    if (ConnectsToDatabaseType == DatabaseTypes.Postgres)
                    {
                        schemaName = schemaName.ToSnakeCase();
                        queryName = queryName.ToSnakeCase();
                        resultClassName = resultClassName.ToSnakeCase();
                    }*/
                    codeWriter.WriteLine($@"var result = await connection");
                    codeWriter.WriteLine($@"     .QueryAsync<{dtoResultClass.ClassName}>(");
                    codeWriter.WriteLine(
                        $@"     ""{schemaName}.{queryName}"",");
                    codeWriter.WriteLine($@"        sqlParams,");
                    codeWriter.WriteLine($@"        commandType: CommandType.StoredProcedure)");
                    codeWriter.WriteLine($@"     .ConfigureAwait(false);");
                    codeWriter.WriteLine("");
                    codeWriter.WriteLine($@"return result.ToList();");
                }
                else
                {
                    codeWriter.WriteLine($@"var result = await connection");
                    codeWriter.WriteLine($@"     .ExecuteAsync(");
                    codeWriter.WriteLine(
                        $@"     ""{query.QueryName}"",");
                    codeWriter.WriteLine($@"        sqlParams,");
                    codeWriter.WriteLine($@"        commandType: CommandType.StoredProcedure)");
                    codeWriter.WriteLine($@"     .ConfigureAwait(false);");
                    codeWriter.WriteLine("");
                    codeWriter.WriteLine($@"return result > 0;");
                }
                codeWriter.WriteLine(@"");
                method.CodeSnippet = codeWriter.ToString();
                methods.Add(method);

            }
            return methods;
        }


        protected CInterface BuildDataHealthCheckInterface()
        {
            var dataHealthCheckInterface = new CInterface();



            dataHealthCheckInterface.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "System.Threading.Tasks" }
            });

            dataHealthCheckInterface.Namespace = new CNamespace
            {
                NamespaceName =
                    $"{_dataLayerKProject.CompanyName}.{_dataLayerKProject.ProjectName}{_dataLayerKProject.NamespaceSuffix}.Data.DataServices"
            };

            dataHealthCheckInterface.InterfaceName = $"IDataHealthCheckService";

            dataHealthCheckInterface.Method.Add(new CMethod
            {
                SignatureOnly = true,
                MethodName = "Check",
                ReturnType = "Task<bool>"
            });

            return dataHealthCheckInterface;
        }

        protected CInterface BuildDataServiceInterface(List<CClass> dtoClasses)
        {
            var dbProviderInterface = new CInterface();
            dbProviderInterface.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "System" }
            });

            /*
            dbProviderInterface.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace {NamespaceName = "System.Data"}
            });
            */
            /*
            dbProviderInterface.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace {NamespaceName = "System.Data.SqlClient"}
            });*/
            dbProviderInterface.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "System.Collections.Generic" }
            });
            dbProviderInterface.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "System.Threading.Tasks" }
            });
            /*dbProviderInterface.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "System.Reflection" }
            });
            */

            dbProviderInterface.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace
                {
                    NamespaceName =
                    $"{_dataLayerKProject.CompanyName}.{_dataLayerKProject.ProjectName}{_dataLayerKProject.NamespaceSuffix}.Data.Entities"
                }
            });

            /*
            //todo: clean this up
            if (dtoClasses.Count > 0)
                dbProviderInterface.NamespaceRef.Add(
                    _dataProject.BuildNamespaceRefForType(dtoClasses.First().ClassName));
                    */

            dbProviderInterface.Namespace = new CNamespace
            {
                NamespaceName =
                    $"{_dataLayerKProject.CompanyName}.{_dataLayerKProject.ProjectName}{_dataLayerKProject.NamespaceSuffix}.Data.DataServices"
            };


            dbProviderInterface.InterfaceName =
                $"I{_dataLayerKProject.ProjectNameAsClassNameFriendly}{_dataLayerKProject.ProjectSuffix}Service";
            var methods = GetDataServiceMethods();
            foreach (var m in methods)
            {
                m.IsAsync = false;
                m.SignatureOnly = true;
            }

            dbProviderInterface.Method.AddRange(methods);


            return dbProviderInterface;
        }


    }

}
