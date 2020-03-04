using Kickstart.Interface;
using Kickstart.Pass2.CModel;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.CModel.Docker;
using Kickstart.Pass2.CModel.Git;
using Kickstart.Pass3.Docker;
using Kickstart.Pass3.Git;
using Microsoft.Extensions.Logging;
namespace Kickstart.Pass3.VisualStudio2017
{
    public class CVisualStudioVisitorBase : IVisitor, ICVisualStudioVisitorBase
    {
        private readonly ICAssemblyInfoVisitor _assemblyInfoVisitor;
        private readonly ICClassAttributeVisitor _classAttributeVisitor;
        private readonly ICClassVisitor _classVisitor;
        private readonly ICEnumVisitor _enumVisitor;

        protected readonly ICodeWriter _codeWriter;
        private readonly ICConstructorVisitor _constructorVisitor;
        private readonly ICFieldVisitor _fieldVisitor;
        private readonly ICProjectFileVisitor _fileVisitor;
        private readonly ICInterfaceVisitor _interfaceVisitor;
        private readonly ILogger _logger;
        private readonly ICMethodAttributeVisitor _methodAttributeVisitor;
        private readonly ICMethodVisitor _methodVisitor;
        private readonly ICParameterVisitor _parameterVisitor;
        private readonly ICProjectContentVisitor _projectContentVisitor;
        private readonly ICProjectVisitor _projectVisitor;
        private readonly ICPropertyVisitor _propertyVisitor;
        private readonly ICSolutionVisitor _solutionVisitor;
        private readonly ICDockerComposeFileVisitor _dockerComposeVisitor;
        private readonly ICMetaRepoVisitor _metaRepoVisitor;
        private readonly ICRepoVisitor _repoVisitor;
        private readonly ICDockerFileServiceVisitor _dockerFileServiceVisitor;

        public CVisualStudioVisitorBase(ILogger<CVisualStudioVisitorBase> logger, ICodeWriter codeWriter,
            ICSolutionVisitor solutionVisitor , ICProjectVisitor projectVisitor  ,
            ICProjectFileVisitor fileVisitor ,
            ICInterfaceVisitor interfaceVisitor ,
            ICClassVisitor classVisitor ,
            ICMethodVisitor methodVisitor , ICPropertyVisitor propertyVisitor ,
            ICParameterVisitor parameterVisitor ,
            ICFieldVisitor fieldVisitor , ICConstructorVisitor constructorVisitor ,
            ICAssemblyInfoVisitor assemblyInfoVisitor , ICClassAttributeVisitor classAttributeVisitor,
            ICMethodAttributeVisitor methodAttributeVisitor ,
            ICProjectContentVisitor projectContentVisitor ,
            ICDockerComposeFileVisitor dockerComposeFileVisitor,
            ICMetaRepoVisitor metaRepoVisitor ,
            ICRepoVisitor repoVisitor ,
            ICDockerFileServiceVisitor dockerFileServiceVisitor,
            ICEnumVisitor enumVisitor
            )
        {
            _logger = logger;
            _codeWriter = codeWriter;
            _solutionVisitor = solutionVisitor;
            _projectVisitor = projectVisitor;
            _fileVisitor = fileVisitor;
            _interfaceVisitor = interfaceVisitor;
            _classVisitor = classVisitor;
            _methodVisitor = methodVisitor;
            _propertyVisitor = propertyVisitor;
            _parameterVisitor = parameterVisitor;
            _fieldVisitor = fieldVisitor;
            _constructorVisitor = constructorVisitor;
            _assemblyInfoVisitor = assemblyInfoVisitor;
            _classAttributeVisitor = classAttributeVisitor;
            _methodAttributeVisitor = methodAttributeVisitor;
            _projectContentVisitor = projectContentVisitor;
            _dockerComposeVisitor = dockerComposeFileVisitor;
            _metaRepoVisitor = metaRepoVisitor;
            _repoVisitor = repoVisitor;
            _dockerFileServiceVisitor = dockerFileServiceVisitor;
            _enumVisitor = enumVisitor;

        }


        public void Visit(CSolution solution)
        {
            _logger.LogInformation($"Visiting {GetType()}");
            _solutionVisitor.Visit(this, solution);
            _logger.LogInformation($"Visited {GetType()}");
        }

        public void AddProjectToSolution(CProject project, string filePath)
        {
            if (_solutionVisitor != null)
                _solutionVisitor.AddProjectToSolution(project, filePath);
        }

        public void VisitSProject(CProject project)
        {
            _projectVisitor.Visit(this, project);
        }

        public void Visit(CFile file)
        {
            _fileVisitor.Visit(this, file);
        }

        public void VisitCClass(CClass cclass)
        {
            _classVisitor.CodeWriter.Clear();
            _classVisitor.Visit(this, cclass);
        }

        public void Visit(CEnum cenum)
        {
            _enumVisitor.CodeWriter.Clear();
            _enumVisitor.Visit(this, cenum);
        }


        public void VisitCMethod(CMethod method)
        {
            _methodVisitor.Visit(this, method);
        }

        public void Visit(CProperty property)
        {
            _propertyVisitor.Visit(property);
        }

        public void VisitCParameter(CParameter parameter)
        {
            _parameterVisitor.Visit(parameter);
        }

        public void VisitSProjectContent(CProjectContent projectContent)
        {
            _classVisitor.CodeWriter.Clear();
            _projectContentVisitor.Visit(this, projectContent);
        }

        public void VisitCInterface(CInterface sInterface)
        {
            _interfaceVisitor.Visit(this, sInterface);
        }

        public void Visit(CField field)
        {
            _fieldVisitor.Visit(field);
        }

        public void Visit(CConstructor constructor)
        {
            _constructorVisitor.Visit(this, constructor);
        }

        public void Visit(CAssemblyInfo assemblyInfo)
        {
            _classVisitor.CodeWriter.Clear();
            _assemblyInfoVisitor.Visit(this, assemblyInfo);
        }

        public virtual void Visit(CPart part)
        {
            //todo://why isn't it directly calling the method above?
            if (part is CClassAttribute)
                Visit(part as CClassAttribute);
            if (part is CMethodAttribute)
                Visit(part as CMethodAttribute);
            if (part is CDockerComposeFile)
            {
                Visit(part as CDockerComposeFile);
            }
            if (part is CMetaRepo)
            {
                Visit(part as CMetaRepo);
            }
            if (part is CRepo)
            {
                Visit(part as CRepo);
            }
            if (part is CDockerFileService)
            {
                Visit(part as CDockerFileService);
            }

            if (part is CEnum)
            {
                Visit(part as CEnum);
            }
        }

        public void Visit(CClassAttribute classAttribute)
        {
            _classAttributeVisitor.Visit(this, classAttribute);
        }

        public void Visit(CMethodAttribute methodAttribute)
        {
            _methodAttributeVisitor.Visit(this, methodAttribute);
        }

        public void Visit(CDockerComposeFile dockerComposeFile)
        {
            _dockerComposeVisitor.Visit(this, dockerComposeFile);
        }

        public void Visit(CMetaRepo metaRepo)
        {
            _metaRepoVisitor.Visit(this, metaRepo);
        }

        public void Visit(CRepo repo)
        {
            _repoVisitor.Visit(this, repo);
        }

        public void Visit(CDockerFileService service)
        {
            _dockerFileServiceVisitor.Visit(this, service);
        }
    }
}