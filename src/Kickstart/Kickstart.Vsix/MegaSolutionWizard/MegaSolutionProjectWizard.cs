using EnvDTE;
using Kickstart.Pass1;
using Kickstart.Pass1.Service;
using Kickstart.Pass2.DataLayerProject;
using Kickstart.Pass2.GrpcServiceProject;
using Kickstart.Pass3.gRPC;
using Kickstart.Utility;
using Kickstart.Vsix.Wizard;
using Kickstart.Vsix.Wizard.Service;
using Kickstart.Wizard.Presenter;
using Kickstart.Wizard.Service;
using Microsoft.VisualStudio.TemplateWizard;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Kickstart.Pass0.Model;
using Kickstart.Pass3;
using Kickstart.Pass3.VisualStudio2017;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using Kickstart.Vsix.MegaSolutionWizard;
using Kickstart.MegaSolutionWizard.Presenter;

namespace Kickstart.Vsix
{
    public class MegaSolutionProjectWizard : IWizard
    {
        public void BeforeOpeningFile(global::EnvDTE.ProjectItem projectItem)
        {

            int x = 1;
            //  throw new NotImplementedException();
        }

        public void ProjectFinishedGenerating(global::EnvDTE.Project project)
        {

            int x = 1;
            //throw new NotImplementedException();
        }

        public void ProjectItemFinishedGenerating(global::EnvDTE.ProjectItem projectItem)
        {
            int x = 1;
            //throw new NotImplementedException();
        }

        public void RunFinished()
        {

            //throw new NotImplementedException();
        }

        private DTE _dte;
        private string _projectDirectory = "";

        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
        private MegaSolutionWizardDialog _dialog;
        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
            

            _dte = (DTE)automationObject;
            
            _projectDirectory = replacementsDictionary["$destinationdirectory$"];

            

            var serviceCollection =new ServiceCollection();
            
            

            serviceCollection.AddLogging();
            //var logger = new LoggerConfiguration()
            //    .CreateLogger();

            var configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(AssemblyDirectory, "appsettings.json"), false)
                .Build();
            serviceCollection.AddSingleton(configuration);

            //serviceCollection.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

            //serviceCollection.AddSingleton(typeof(Serilog.ILogger), logger);
            var serviceProvider = serviceCollection.ConfigureContainer();
            
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            
            ConfigureLogger(loggerFactory);

            var prereqChecker = serviceProvider.GetService<IPrereqChecker>(); 
            if (!prereqChecker.CheckGrpcNugets())
            {
                MessageBox.Show("Grpc tools (Nugets) have not been installed");
                throw new WizardBackoutException();
            }
            var kickstartService = serviceProvider.GetRequiredService<IKickstartService>();
            
            
            //List<KSolutionGroup> unfiliteredSolutionGroupList = service.GetSolutionGroupListFromCode(); ;
            //pass1
            //read metadata from code/sql/proto files into K objects
            //unfiliteredSolutionGroupList = service.GetSolutionGroupListFromExcel();
            var kickstartWizard = new KickstartWizardModel()
            {
                SolutionName = replacementsDictionary["$specifiedsolutionname$"],
                CompanyName = "Company",
                ProjectName = replacementsDictionary["$specifiedsolutionname$"],
                // _dialog.AvailableTemplateSolutions = unfiliteredSolutionGroupList.SelectMany(g => g.Solution).ToList();
                
                ProjectDirectory = _projectDirectory
            };

            _dialog = new MegaSolutionWizardDialog();
            var menuView = new MenuStep();
            menuView.Tag = new MenuPresenter(kickstartWizard, menuView);
            /*
            _dialog.Tag = new MegaSolutionWizardPresenter(kickstartWizard, kickstartService,
                new KickstartWizardService( new ProtoToKProtoConverter(configuration), new SProtoFileToProtoFileConverter(), new KDataLayerProjectToKProtoFileConverter(), new DbToKSolutionConverter(), new DataLayerServiceProjectService()), 
                new MessageBoxDisplayService(), _dialog, menuView, _dialog.ProjectView, new ProtoFileStep(), new DatabaseSqlStep(), new GenerationStep(), new ProtoToKProtoConverter(configuration) );
        */

            var exclusiveProject = Boolean.Parse(replacementsDictionary["$exclusiveproject$"]);
            IWin32Window parentWindow = Control.FromHandle(new IntPtr(_dte.ActiveWindow.HWnd));
            var result = _dialog.ShowDialog(parentWindow);
            if (result == DialogResult.Cancel)
            {
                throw new WizardBackoutException();
            }
            replacementsDictionary["$specifiedsolutionname$"] = kickstartWizard.SolutionName;


            
            if (exclusiveProject)
            {
                if (kickstartWizard.SelectedTemplateSolutions.Count > 1 || kickstartWizard.SelectedTemplateSolutions.Count > 1)
                {
                    //open the root "master" solution
                    var solutionName = $"{kickstartWizard.SolutionName}.sln";

                    var fullSolutionPath = Path.Combine(kickstartWizard.ProjectDirectory, solutionName);

                    _dte.Solution.Open(fullSolutionPath);
                }
                else
                {
                    _dte.Solution.Open(kickstartWizard.SelectedTemplateSolutions.First().GeneratedSolution.SolutionPath);
                }
            }
            else
            {
                foreach (var solution2 in kickstartWizard.SelectedTemplateSolutions)
                    foreach (var project in solution2.GeneratedSolution.Project)
                    {
                        try
                        {
                            _dte.Solution.AddFromFile(project.Path);
                        }
                        catch
                        {

                        }
                    }
            }

            throw new WizardCancelledException();
        }

        private void ConfigureLogger(ILoggerFactory loggerFactory)
        {
            Log.Logger = new LoggerConfiguration()
               // .ReadFrom.Configuration(Configuration)
                .CreateLogger();

            loggerFactory.AddSerilog();

            var logger = loggerFactory.CreateLogger<ProjectWizard>();
            
            logger.LogInformation("Starting application");
            //Logger.LogInformation("Environment: {0}", EnvironmentName);
        }
        private Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var name = args.Name.Replace("Microsoft.Extensions.DependencyInjection.Abstractions, Version=1.1.1.0", "Microsoft.Extensions.DependencyInjection.Abstractions, Version=2.0.0.0");
            name = name.Replace("Microsoft.Extensions.Logging.Abstractions, Version=2.0.0.0",
                "Microsoft.Extensions.Logging.Abstractions, Version=2.0.1.0");
            name = name.Replace("Microsoft.Extensions.Options, Version=2.0.0.0",
                "Microsoft.Extensions.Options, Version=2.0.1.0");


            var requestedAssembly = new AssemblyName(name);
            /* if (requestedAssembly.Name != shortName)
                 return null;

             Debug.WriteLine("Redirecting assembly load of " + args.Name
                                                             + ",\tloaded by " + (args.RequestingAssembly == null ? "(unknown)" : args.RequestingAssembly.FullName));

             requestedAssembly.Version = targetVersion;
             requestedAssembly.SetPublicKeyToken(new AssemblyName("x, PublicKeyToken=" + publicKeyToken).GetPublicKeyToken());
             requestedAssembly.CultureInfo = CultureInfo.InvariantCulture;

             AppDomain.CurrentDomain.AssemblyResolve -= handler;
             */
            //AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomainOnAssemblyResolve;

            return Assembly.Load(requestedAssembly);
            
        }

        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }
    }
}
