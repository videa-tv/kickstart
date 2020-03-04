using ProtoAlias = Company.KickstartBuild.Services.Types;
using Google.Protobuf;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Collections;
using System;

namespace Kickstart.Build.Services
{
    public static class KickstartBuildServiceProtoExtensions
    {

        public static Kickstart.Build.Services.Model.ReleaseDefinition ToModel(this ProtoAlias.CreateReleaseDefinitionRequest source)
        {
            return source.ReleaseDefinition.ToModel();
        }
        public static Kickstart.Build.Services.Model.ServerLocation ToModel(this ProtoAlias.ServerLocation source)
        {
            if (source == ProtoAlias.ServerLocation.Aws)
                return Model.ServerLocation.AWS;
            if (source == ProtoAlias.ServerLocation.Onpremise)
                return Model.ServerLocation.OnPremise;

            throw new NotImplementedException();
        }
        public static Model.EnvironmentTag ToModel(this ProtoAlias.EnvironmentTag source)
        {
            switch (source)
            {
                case ProtoAlias.EnvironmentTag.Prod:
                    return Model.EnvironmentTag.Prod;
                case ProtoAlias.EnvironmentTag.Alpha:
                    return Model.EnvironmentTag.Alpha;
                case ProtoAlias.EnvironmentTag.Dev:
                    return Model.EnvironmentTag.Dev;
                case ProtoAlias.EnvironmentTag.Qa:
                    return Model.EnvironmentTag.QA;
            }

            throw new NotImplementedException();

           // return Enum.GetName(typeof(ProtoAlias.EnvironmentTag), source).ToLower();
        }
        public static Kickstart.Build.Services.Model.DbmsType ToModel(this ProtoAlias.DbmsType source)
        {
            if (source == ProtoAlias.DbmsType.Sqlserver)
                return Model.DbmsType.SqlServer;
            else if (source == ProtoAlias.DbmsType.Postgres)
                return Model.DbmsType.Postgresql;
            else if (source == ProtoAlias.DbmsType.Mysql)
                return Model.DbmsType.MySql;
            else
                throw new NotImplementedException();
        }
            public static Kickstart.Build.Services.Model.Database ToModel(this ProtoAlias.Database source)
        {
            return new Model.Database
            {
                DatabaseName = source.DatabaseName,
                ProjectFolder = source.ProjectFolder
            };
        }
        public static Kickstart.Build.Services.Model.DatabaseServer ToModel(this ProtoAlias.DatabaseServer source)
        {
            return new Model.DatabaseServer
            {
                DbmsType = source.DbmsType.ToModel(),
                Location = source.ServerLocation.ToModel(),
                Databases = source.Databases.ToModel()
            };

        }
        public static Kickstart.Build.Services.Model.Environment ToModel(this ProtoAlias.Environment source)
        {
            return new Model.Environment()
            {
                EnvironmentIdentifier = source.EnvironmentIdentifier,
                EnvironmentName = source.EnvironmentName,
                EnvironmentTag = source.EnvironmentTag.ToModel()
            };

        }
        public static Guid ProjectId = Guid.Parse("421987af-ee2d-4a53-99f3-b5b4e017cae9"); //Company
        public static string ProjectName = "Company"; //as opposed to Vida Git

        public static Kickstart.Build.Services.Model.BuildDefinition ToModel(this ProtoAlias.BuildDefinition source)
        {
            return new Kickstart.Build.Services.Model.BuildDefinition
            {
                BuildDefinitionIdentifier = source.BuildDefinitionIdentifier,
                BuildDefinitionName = source.BuildDefinitionName,
                ProjectId = ProjectId,
                ProjectName = ProjectName,
                RepoName = source.RepoName,
                RepoPath = source.RepoPath
                
            };


        }

        public static Kickstart.Build.Services.Model.Release ToModel(this ProtoAlias.CreateReleaseRequest source)
        {
            return new Kickstart.Build.Services.Model.Release
            {
                //ReleaseId = source.<unfound proto file>

            };


        }
        public static Kickstart.Build.Services.Model.ReleaseDefinition ToModel(this ProtoAlias.ReleaseDefinition source)
        {
            return new Kickstart.Build.Services.Model.ReleaseDefinition
            {
                //ReleaseDefinitionId = source.<unfound proto file>,
                ReleaseDefinitionName = source.ReleaseDefinitionName,
                ProjectId = ProjectId,
                //ReleaseDefinitionIdentifier = source.<unfound proto file>,
                BuildDefinitions = source.BuildDefinitions.ToModel(),
                Environments = source.Environments.ToModel()

            };
        }

        public static IEnumerable<Kickstart.Build.Services.Model.ReleaseDefinition> ToModel(this RepeatedField<ProtoAlias.ReleaseDefinition> source)
        {
            return source.Select(s => s.ToModel()).ToList();


        }

        public static IEnumerable<Kickstart.Build.Services.Model.BuildDefinition> ToModel(this RepeatedField<ProtoAlias.BuildDefinition> source)
        {
            return source.Select(s => s.ToModel()).ToList();


        }

        public static IEnumerable<Kickstart.Build.Services.Model.Environment> ToModel(this RepeatedField<ProtoAlias.Environment> source)
        {
            return source.Select(s => s.ToModel()).ToList();


        }

        public static IEnumerable<Kickstart.Build.Services.Model.DatabaseServer> ToModel(this RepeatedField<ProtoAlias.DatabaseServer> source)
        {
            return source.Select(s => s.ToModel()).ToList();


        }

        public static IEnumerable<Kickstart.Build.Services.Model.Database> ToModel(this RepeatedField<ProtoAlias.Database> source)
        {
            return source.Select(s => s.ToModel()).ToList();


        }

}

}
