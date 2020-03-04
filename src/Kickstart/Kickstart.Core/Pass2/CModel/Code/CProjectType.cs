using System;

namespace Kickstart.Pass2.CModel.Code
{
    public enum CProjectType
    {
        CsProj,
        SqlProj,
        FlywayProj,
        DockerProj,
        MetaRepoProj
    }

    public static class CProjectTypeExtensions
    {
        public static string GetString(this CProjectType projectType)
        {
            switch (projectType)
            {
                case CProjectType.CsProj:
                    return "csproj";
                case CProjectType.SqlProj:
                    return "sqlproj"; 
                case CProjectType.FlywayProj:
                    return "csproj"; //todo: create custom .flywayproj VS project type
                case CProjectType.DockerProj:
                    return "dcproj";
                case CProjectType.MetaRepoProj:
                    return "csproj";
            }

            throw new NotImplementedException();
        }
    }
}