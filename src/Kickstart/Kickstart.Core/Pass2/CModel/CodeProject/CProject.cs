using System;
using System.Collections.Generic;
using Kickstart.Interface;
using Kickstart.Utility;

namespace Kickstart.Pass2.CModel.Code
{
    [Flags]
    public enum CProjectIs
    {
        DataBase = 1,
        DatabaseFlyway = 2,
        DataAccess = 4,
        Service = 8,
        Gui = 16,
        Grpc = 32,
        Integration = 64,
        Client = 128,
        Test = 256,
        DockerBuildScripts = 512, // doesn't create actual project file
        SolutionFiles = 1024, //used to add files into solution root folder, doesn't create an actual project file
        Web = 2048,
        DockerCompose = 4096,
        MetaRepo = 8192
    }
    public class CNullProject : CProject
    {   public CNullProject()
        {
            this.Kickstart = false;
        }
    }
    public class CProject : CPart
    {
        private string _assemblyName;
        private Guid _projectGuid;
        public CProjectIs ProjectIs { get; set; }
        public string TemplateProjectZip { get; set; }

        public string TemplateProjectPath { get; set; }
        public string SolutionFolder { get; set; }
        public string ProjectFolder { get; set; }
        public string ProjectID { get; set; }
        public string ProjectName { get; set; }
        public string ProjectShortName { get; set; }

        public CProjectType ProjectType { get; set; }

        public string FileName => $"{ProjectName}.{ProjectType.GetString()}";

        public Guid ProjectGuid
        {
            get
            {
                if (_projectGuid == Guid.Empty)
                    _projectGuid = ConsistentGuid.Generate(ProjectName); //todo: is this unique, enough?
                return _projectGuid;
            }
            set
            {
                _projectGuid = value;
            }
        }

        public List<CProjectContent> ProjectContent { get; set; } = new List<CProjectContent>();

        public List<CClass> Class
        {
            get
            {
                var classList = new List<CClass>();
                foreach (var pc in ProjectContent)
                {
                    if (pc.Class == null)
                        continue;
                    classList.Add(pc.Class);
                }
                return classList;
            }
        }

        public List<CInterface> Interface
        {
            get
            {
                var list = new List<CInterface>();
                foreach (var pc in ProjectContent)
                {
                    if (pc.Interface == null)
                        continue;
                    list.Add(pc.Interface);
                }
                return list;
            }
        }

        public string Path { get; set; }
        public string DefaultNamespace { get; set; }

        public string AssemblyName
        {
            get
            {
                if (_assemblyName == null)
                    return $"{ProjectName}.dll";
                return _assemblyName;
            }
            set => _assemblyName = value;
        }

        public string StartupObject { get; set; }
        public List<CProject> ProjectReference { get; set; } = new List<CProject>();

        public List<CNuGet> NuGetReference { get; set; } = new List<CNuGet>();
        public List<CPostBuildEventStep> PostBuildEventStep { get; set; } = new List<CPostBuildEventStep>();
        public bool HasDockerFile { get; internal set; }

        public override void Accept(IVisitor visitor)
        {
            visitor.VisitSProject(this);
        }

        public override string ToString()
        {
            return $"{base.ToString()} {this.ProjectName}";
        }
        public CNamespaceRef BuildNamespaceRefForType(string typeName)
        {
            foreach (var pc in ProjectContent)
                if (pc.Content is CClass)
                {
                    var @class = pc.Content as CClass;
                    if (@class.ClassName == typeName)
                        return new CNamespaceRef
                        {
                            ReferenceTo = new CNamespace {NamespaceName = @class.Namespace.NamespaceName}
                        };
                }
                else if (pc.Content is CInterface)
                {
                    var @interface = pc.Content as CInterface;
                    if (@interface.InterfaceName == typeName)
                        return new CNamespaceRef
                        {
                            ReferenceTo = new CNamespace {NamespaceName = @interface.Namespace.NamespaceName}
                        };
                }
            throw new Exception("class/interface name not found");
        }
    }
}