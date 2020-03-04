using System;
using System.Collections.Generic;
using System.Linq;
using Kickstart.Pass1.KModel;
using Kickstart.Utility;
using Kickstart.Wizard.View;
using System.Threading.Tasks;

namespace Kickstart.Pass0.Model
{
    public class KickstartWizardModel
    {
        public List<KSolution> AvailableTemplateSolutions { get; set; }
        public List<KSolution> SelectedTemplateSolutions { get; set; } = new List<KSolution>();

        public string ProjectDirectory { get;  set; }
        public bool ExclusiveProject { get;  set; }
        //public Solution Solution { get; internal set; }

        public enum Step { Menu, ProtoFile, DatabaseSql, GenerationStart, SolutionsSelect, SolutionsEdit, OpenGeneratedSolution }

        public Dictionary<Step, IView> Steps
        {
            get
            {
                return _steps;
            }
        }
        Dictionary<Step, IView> _steps;
        public void SetSteps(Dictionary<Step, IView> steps)
        {
           
            _steps = steps;
        }
        public Step CurrentStep = 0;

        public Step NextStep
        {

            get
            {
                if (_steps == null)
                    return CurrentStep;

                var array = _steps.Keys.ToArray();
                var currentStepIndex = Array.IndexOf(array, CurrentStep);
                var nextStepIndex = currentStepIndex + 1;
                if (nextStepIndex < array.Length)
                {
                    return array[nextStepIndex];
                }
                else
                    return CurrentStep;
            }
        }

        public string SolutionName { get;  set; }

        public Func<object, EventArgs, Task> ProjectNameChanged { get; set; }

        private string _projectName;
        public string ProjectName
        {
            get { return _projectName; }
            set
            {
                _projectName = value;
                ProjectNameChanged?.Invoke(null, null);
            }
        }
        public string CompanyName { get;  set; }
        public IView CurrentView
        {
            get
            {
                return Steps[CurrentStep];
            }
        }

        public MetadataSource MetadataSource { get; set; }
        private string _protoFileText;
        public string ProtoFileText { get;  set; }
        public bool CreateGrpcServiceTestClientProject { get; internal set; }
        public bool CreateGrpcUnitTestProject { get; set; }
        public bool CreateIntegrationTestProject { get; internal set; }
        public bool CreateGrpcServiceProject { get;  set; }
        public bool CreateDataLayerProject { get;  set; }
        public bool CreateWebAppProject { get;  set; }

        public DataStoreTypes DatabaseType { get;  set; }
        public bool CreateDatabaseProject { get; set; }
        public bool CreateDatabaseTestProject { get; set; } = true;

        public bool CreateDockerComposeProject { get; set; }

        public bool CreateGrpcClientProject { get; set; }
        public bool GenerateStoredProcAsEmbeddedQuery { get; set; } = true;
        public string SqlTableText { get; internal set; }
        public string SqlTableTypeText { get; internal set; }

        public string SqlStoredProcText { get; internal set; }
        public bool ConvertToSnakeCase { get; set; }
        public bool CreateDockerBuildProject { get; internal set; } = true;

        // public bool GenerateAsync { get; set; } = true;

        public void IncrementStep()
        {
            var array = _steps.Keys.ToArray();
            var currentStepIndex = Array.IndexOf(array, CurrentStep);
            currentStepIndex++;
            if (currentStepIndex < array.Length)
            {
                CurrentStep = array[currentStepIndex];
            }
        }

        public bool DecrementStep()
        {
            if (CurrentStep == Step.Menu)
            {
                return false;
            }
            var array = _steps.Keys.ToArray();
            var currentStepIndex = Array.IndexOf(array, CurrentStep);
            currentStepIndex--;
            if (currentStepIndex >= 0)
            {
                CurrentStep = array[currentStepIndex];
            }
            else
                CurrentStep = Step.Menu;

            return true;
        }

        internal bool HasSql()
        {
            if (!string.IsNullOrWhiteSpace(SqlStoredProcText))
                return true;
            if (!string.IsNullOrWhiteSpace(SqlTableText))
                return true;
            if (!string.IsNullOrWhiteSpace(SqlTableTypeText))
                return true;

            /*  if (!string.IsNullOrWhiteSpace(Sqsl))
                  return true;
              if (!string.IsNullOrWhiteSpace(SqlStoredProcText))
                  return true;
              if (!string.IsNullOrWhiteSpace(SqlStoredProcText))
                  return true;
                  */
            return false;
        }
    }
}
