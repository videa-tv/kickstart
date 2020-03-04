using Kickstart.Interface;
using Kickstart.Pass1;
using Kickstart.Pass1.KModel;
using Kickstart.Pass1.Service;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Pass2.DataLayerProject;
using Kickstart.Pass2.GrpcServiceProject;
using Kickstart.Pass3.gRPC;
using Kickstart.Utility;
using Kickstart.Vsix.Wizard.Service;
using Kickstart.Wizard.Service;
using Kickstart.Wizard.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Kickstart.Pass0.Model;
using static Kickstart.Pass0.Model.KickstartWizardModel;

namespace Kickstart.MegaSolutionWizard.Presenter
{
    public class MegaSolutionWizardPresenter
    {
        IKickstartWizardView _kickstartWizardView;
        IMessageBoxDisplayService _messageBoxDisplayService;
        IMenuView _menuView;
        IProjectView _projectView;
        IProtoFileView _protoFileView;
        IDatabaseSqlView _databaseSqlView;
        IGenerationView _generationView;

        KickstartWizardModel _kickstartWizardModel;
        IKickstartWizardService _kickstartWizardService;
        IKickstartService _kickstartService;
        private IProtoToKProtoConverter _protoToKProtoConverter;

        public MegaSolutionWizardPresenter(KickstartWizardModel kickstartWizard, IKickstartService kickstartService, IKickstartWizardService kickstartWizardService, IMessageBoxDisplayService messageBoxDisplayService, IKickstartWizardView kickstartWizardView, IMenuView menuView, IProjectView projectView, IProtoFileView protoFileView, IDatabaseSqlView databaseSqlView, IGenerationView generationView, IProtoToKProtoConverter protoToKProtoConverter)
        {
            
            _kickstartService = kickstartService;
            _kickstartWizardService = kickstartWizardService;
            _messageBoxDisplayService = messageBoxDisplayService;
            _kickstartWizardView = kickstartWizardView;
            _menuView = menuView;
            _protoFileView = protoFileView;
            _databaseSqlView = databaseSqlView;
            _generationView = generationView;
            _projectView = projectView;
            _protoToKProtoConverter = protoToKProtoConverter;

            kickstartWizardView.Load += KickstartWizardViewLoad;
            
            kickstartWizardView.NextClicked += KickstartWizardViewNextClicked;
            kickstartWizardView.PreviousStep += KickstartWizardViewPreviousStep;
            protoFileView.ProtoTextChanged += ProtoFileViewProtoTextChanged;
            databaseSqlView.GenerateStoredProcAsEmbeddedQueryChanged += (a, b) => 
            {
                _kickstartWizardModel.GenerateStoredProcAsEmbeddedQuery = _databaseSqlView.GenerateStoredProcAsEmbeddedQuery;
                return Task.CompletedTask;
            };

            databaseSqlView.SqlStoredProcTextChanged += (a, b) =>
            {
                _kickstartWizardModel.SqlStoredProcText = _databaseSqlView.SqlStoredProcText;
                return Task.CompletedTask;
            };

            databaseSqlView.SqlTableTextChanged += (a, b) =>
            {
                _kickstartWizardModel.SqlTableText = _databaseSqlView.SqlTableText;
                return Task.CompletedTask;
            };

            databaseSqlView.SqlTableTypeTextChanged += (a, b) =>
            {
                _kickstartWizardModel.SqlTableTypeText = _databaseSqlView.SqlTableTypeText;
                return Task.CompletedTask;
            };

            _kickstartWizardModel = kickstartWizard;

            kickstartWizardView.AddView(menuView);
            kickstartWizardView.AddView(protoFileView);
            kickstartWizardView.AddView(databaseSqlView);
            kickstartWizardView.AddView(generationView);

            //where should this go in MVP ?
            _projectView.SolutionName = _kickstartWizardModel.SolutionName;
            _projectView.CompanyName = _kickstartWizardModel.CompanyName;
            _projectView.ProjectName = _kickstartWizardModel.ProjectName;
        }

       

        private Task ProtoFileViewProtoTextChanged(object sender, EventArgs e)
        {
            _kickstartWizardModel.ProtoFileText = _protoFileView.ProtoFileText;
            /*
            try
            {
                var kProtoFile = new ProtoToKProtoConverter().Convert("zzz", _protoFileView.ProtoFileText);

                if (kProtoFile == null || kProtoFile.GeneratedProtoFile == null || kProtoFile.GeneratedProtoFile.ProtoMessage.Count == 0)
                {

                    return;
                }
                _kickstartWizardModel.SolutionName = InferSolutionName(kProtoFile);
                _kickstartWizardModel.ProjectName = InferProjectName(kProtoFile);
            }
            catch (ProtoCompileException ex)
            {
                // MessageBox.Show(ex.Message);
            }*/
            return Task.CompletedTask;
        }

        private void KickstartWizardViewPreviousStep(object sender, EventArgs e)
        {   
            if (_kickstartWizardModel.DecrementStep())
                SwitchStep();
        }
        private async Task KickstartWizardViewNextClicked(object arg1, EventArgs arg2)
        {
            if (_kickstartWizardModel.CurrentStep == Step.Menu)
            {
                //CreateAllStepControls();
                BuildSteps();
            }
            await DoNextStep();
        }
      

        private  void BuildSteps()
        {
            var steps = new Dictionary<Step, IView>();
            if (_menuView.MetadataSourceSelection == MetadataSource.None)
            {
            }
            else if (_menuView.MetadataSourceSelection == MetadataSource.Grpc)
            {
                steps.Add(Step.ProtoFile, _protoFileView);
                if (_menuView.CreateDatabaseProject || _menuView.CreateDataLayerProject)
                {
                    steps.Add(Step.DatabaseSql, _databaseSqlView);
                }
            }
            else if (_menuView.MetadataSourceSelection == MetadataSource.SqlScripts)
            {
                steps.Add(Step.DatabaseSql, _databaseSqlView);
                steps.Add(Step.ProtoFile, _protoFileView);
            }

            steps.Add(Step.GenerationStart, _generationView);
            steps.Add(Step.OpenGeneratedSolution, _generationView);
            _kickstartWizardModel.SetSteps(steps);

           
        }

        private void KickstartWizardViewLoad(object sender, EventArgs e)
        {  
            SwitchStep();
        }

        private async Task DoNextStep()
        {
           
            await PerformStepActions();

            

        }



        private async Task PerformStepActions()
        {
            if (_kickstartWizardModel.CurrentStep == Step.Menu)
            {
                _kickstartWizardModel.IncrementStep();

                SwitchStep();

            }
            else if (_kickstartWizardModel.CurrentStep == Step.ProtoFile && _menuView.MetadataSourceSelection == MetadataSource.Grpc)
            {
                if (!ValidateInput(_menuView.MetadataSourceSelection))
                {
                    return;
                }

                //this one gets used and thrown away
                var solution = _kickstartWizardService.BuildSolution(_kickstartWizardModel);
               
                

                var databaseProject = solution.Project.FirstOrDefault(p => p is KDataStoreProject) as KDataStoreProject;
                _databaseSqlView.SqlTableText = databaseProject.SqlTableText;
                _databaseSqlView.SqlStoredProcText = databaseProject.SqlStoredProcedureText;

                _kickstartWizardModel.IncrementStep();

                SwitchStep();

            }
            else if (_kickstartWizardModel.CurrentStep == Step.DatabaseSql && _menuView.MetadataSourceSelection == MetadataSource.SqlScripts)
            {
                var solution = _kickstartWizardService.BuildSolution(_kickstartWizardModel);
                _kickstartWizardModel.SelectedTemplateSolutions.Add(solution);
                

                _kickstartWizardModel.IncrementStep();

                SwitchStep();


            }
            else if (_kickstartWizardModel.CurrentStep == Step.DatabaseSql && _menuView.MetadataSourceSelection == MetadataSource.Grpc)
            {
                var solution = _kickstartWizardService.BuildSolution(_kickstartWizardModel);

                _kickstartWizardModel.SelectedTemplateSolutions.Add(solution);

               
                _kickstartWizardModel.IncrementStep();

                SwitchStep();

            }

            if (_kickstartWizardModel.CurrentStep == Step.GenerationStart)
            {
                _kickstartWizardView.DisableNext();

                _kickstartWizardModel.IncrementStep();

                SwitchStep();

                //if (_kickstartWizardModel.GenerateAsync)
                {
                    await ExecuteKickstartAsync();
                }
                /*
                else
                {
                    ExecuteKickstart().Wait();
                }*/
            }
            else if (_kickstartWizardModel.CurrentStep == Step.OpenGeneratedSolution)
            {
                CloseAndOpenSolution();
            }
            
        }
        private void SwitchStep()
        {
            if (_kickstartWizardModel.CurrentStep == Step.GenerationStart || _kickstartWizardModel.CurrentStep == Step.OpenGeneratedSolution)
            {
                _projectView.Visible = false;
            }
            else
                _projectView.Visible = true;

            if (_kickstartWizardModel.Steps != null)
            {
                foreach (var view in _kickstartWizardModel.Steps.Values)
                {
                    if (view == null)
                        continue;
                    view.Visible = false;
                }
            }
            if (_kickstartWizardModel.CurrentStep == Step.Menu)
            {
                _menuView.Visible = true;
            }
            else
            {
                _menuView.Visible = false;
                var currentStepControl = _kickstartWizardModel.CurrentView;
                if (currentStepControl != null)
                {
                    currentStepControl.Visible = true;
                }
            }
            if (_kickstartWizardModel.CurrentStep == Step.GenerationStart)
            {
                _kickstartWizardView.DisablePrevious();
                
            }

            if (_kickstartWizardModel.NextStep == Step.GenerationStart)
            {
                _kickstartWizardView.EnableGenerate();
            }
            else if (_kickstartWizardModel.NextStep == Step.OpenGeneratedSolution)
            {
                //_kickstartWizardView.EnableFinish();
            }
            else
                _kickstartWizardView.EnableNext();

        }

     
        private void CloseAndOpenSolution()
        {
            _kickstartWizardView.CloseWizard();
           


        }
        private bool ValidateInput(MetadataSource metadataSource)
        {
            if (metadataSource == MetadataSource.Grpc)
            {
                try
                {
                    var kProtoFile = metadataSource == MetadataSource.Grpc ? _protoToKProtoConverter.Convert("zzz", _protoFileView.ProtoFileText) : null;

                    if (kProtoFile == null || kProtoFile.GeneratedProtoFile == null || kProtoFile.GeneratedProtoFile.ProtoMessage.Count == 0)
                    {
                        _messageBoxDisplayService.Show( "Invalid proto");
                        return false;
                    }
                }
                catch (ProtoCompileException ex)
                {
                    _messageBoxDisplayService.Show( ex.Message);
                    return false;
                }
            }

            return true;
        }

        private async Task ExecuteKickstartAsync()
        {
            
            Task task = Task.Run(() => ExecuteKickstart());
            await task;
          
            _kickstartWizardView.EnableFinish();
        }
        private async Task ExecuteKickstart()
        { 
            List<KSolutionGroup> selectedSolutionGroups = new List<KSolutionGroup>();
            var selectedSolutionGroup = new KSolutionGroup();
            selectedSolutionGroup.Solution.AddRange(_kickstartWizardModel.SelectedTemplateSolutions);
            selectedSolutionGroups.Add(selectedSolutionGroup);


            _kickstartService.ProgressChanged += Service_ProgressChanged;
            _kickstartService.ExecuteAsync(_kickstartWizardModel.ProjectDirectory, _kickstartWizardModel.SolutionName, selectedSolutionGroups).Wait();

        }
        private void Service_ProgressChanged(object sender, KickstartProgressChangedEventArgs e)
        {
            ProgressChanged(e.ProgressPercentChange, e.ProgressMessage);
        }

        private void ProgressChanged(int percentChange, string progressMessage)
        {
             
            _generationView.IncrementProgress(percentChange, progressMessage);
            
        }

        /*
        private List<KSolution> scanAssembliesForTemplates()
        {
            var templates = new List<KSolution>();
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!a.FullName.Contains("Kickstart"))
                {
                    continue;
                }
                foreach (Type t in a.GetTypes())
                {
                    if (t == typeof(KSolution))
                    {
                        continue;
                    }
                    if (t == typeof(KApplicationSolution))
                    {
                        continue;
                    }

                    if (t.IsSubclassOf(typeof(KSolution)))
                    {
                        var solution = Activator.CreateInstance(t) as KSolution;
                        templates.Add(solution);
                    }
                }
            }

            return templates;
        }
        */
    }
}
