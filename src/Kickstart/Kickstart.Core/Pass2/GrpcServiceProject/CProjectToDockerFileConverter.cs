using Kickstart.Pass2.CModel;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Utility;

namespace Kickstart.Pass2.GrpcServiceProject
{
    public interface ICProjectToDockerFileConverter
    {
        CText Convert(CProject grpcProject);
    }

    internal class CProjectToDockerFileConverter : ICProjectToDockerFileConverter
    {
        public CText Convert(CProject grpcProject)
        {
            var dockerSnippet = new CodeWriter();

            dockerSnippet.WriteLine("FROM registry.company.com/dotnet:3.1.0-runtime");

            dockerSnippet.WriteLine();
            dockerSnippet.WriteLine("ENV LANG en_US.UTF-8");
            dockerSnippet.WriteLine("ENV LANGUAGE en_US.UTF-8");
            dockerSnippet.WriteLine();
            dockerSnippet.WriteLine("ARG GIT_COMMIT=unkown");
            dockerSnippet.WriteLine("ARG BUILD_NUMBER=unkown");
            dockerSnippet.WriteLine();
            dockerSnippet.WriteLine(@"LABEL git-commit=$GIT_COMMIT \ ");
            dockerSnippet.WriteLine("	  build-number=$BUILD_NUMBER ");
            dockerSnippet.WriteLine();
            dockerSnippet.WriteLine("ARG source=.");
            dockerSnippet.WriteLine("WORKDIR /app");
            dockerSnippet.WriteLine("COPY $source .");
            dockerSnippet.WriteLine();
            dockerSnippet.WriteLine($@"ENTRYPOINT [""dotnet"", ""{grpcProject.AssemblyName}""]");

            return new CText {Text = dockerSnippet.ToString()};
        }
    }
}