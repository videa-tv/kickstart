using System;
using Kickstart.Pass1.KModel.Project;
using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Pass2
{
    internal class WebAppProjectService
    {
        internal CProject BuildProject(KWebUIProject webAppKProject)
        {
            var webProject = new CProject
            {
                ProjectName = webAppKProject.ProjectFullName,
                ProjectShortName = webAppKProject.ProjectShortName,
                ProjectFolder = webAppKProject.ProjectFolder,
                ProjectType = CProjectType.CsProj,
                ProjectIs = CProjectIs.Web
            };

            webProject.TemplateProjectPath =
                @"templates\RazorWebTemplate\RazorWebTemplate.csproj";
            webProject.TemplateProjectZip =
            @"templates\RazorWebTemplate.zip";
            
            return webProject;
        }
    }
}