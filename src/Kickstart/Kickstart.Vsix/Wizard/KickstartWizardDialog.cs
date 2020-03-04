using EnvDTE;
using Kickstart.Pass1;
using Kickstart.Pass1.KModel;
using Kickstart.Pass1.Service;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Pass2.DataLayerProject;
using Kickstart.Pass2.GrpcServiceProject;
using Kickstart.Pass3.gRPC;
using Kickstart.Utility;
using Kickstart.Vsix.Wizard;
using Kickstart.Wizard.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Kickstart.Wizard.Presenter;

namespace Kickstart.Vsix
{
    public partial class KickstartWizardDialog : Form, IKickstartWizardView
    {
        public Func<object, EventArgs, Task> NextClicked { get; set; }
        public KickstartWizardDialog( )
        {
            InitializeComponent();
        }
        
        public IProjectView ProjectView
        {
            get
            {
                return _projectControl;

            }
        }

        IKickstartWizardPresenter IKickstartWizardView.Tag { get; set; }

        public event EventHandler PreviousStep;
        private async void _buttonNext_Click(object sender, EventArgs e)
        {
            await FireNextStep();
        }
        public async Task<bool> FireNextStep()
        {
            if (!_buttonNext.Enabled)
                return false;
          
            if (NextClicked != null)
                await NextClicked(this, null);

            return true;
        }
        private void _buttonBack_Click(object sender, EventArgs e)
        {
            PreviousStep(this, null);
        }
        
        //private void CreateAllStepControls()
        //{
        //    foreach (var c in _panelSteps.Controls)
        //    {
        //        if (c is MenuStep)
        //            continue;

        //        (c as Control).Dispose();
        //    }

        //    _generationStep = new GenerationStep();
        //    _generationStep.Dock = DockStyle.Fill;
        //    _panelSteps.Controls.Add(_generationStep);
        //    /*
        //    _solutionsSelectStep = new SolutionsSelectStep();
        //    _solutionsSelectStep.Dock = DockStyle.Fill;
        //    _panelSteps.Controls.Add(_solutionsSelectStep);

        //    _solutionsEditStep = new SolutionsEditStep();
        //    _solutionsEditStep.Bind(_solutionsSelectStep.SelectedTemplateSolutions);
        //    _solutionsEditStep.Dock = DockStyle.Fill;
        //    _panelSteps.Controls.Add(_solutionsEditStep);
        //    */

        //    _protoFileStep = new ProtoFileStep();
        //    _protoFileStep.ProtoTextChanged += _protoFileStep_ProtoTextChanged;
        //    _protoFileStep.Dock = DockStyle.Fill;
        //    _panelSteps.Controls.Add(_protoFileStep);

        //    _databaseSqlStep = new DatabaseSqlStep();
        //    _databaseSqlStep.Dock = DockStyle.Fill;
        //    _panelSteps.Controls.Add(_databaseSqlStep);

        //    _finishedStep = new FinishedStep();
        //    _finishedStep.Dock = DockStyle.Fill;
        //    _panelSteps.Controls.Add(_finishedStep);

        //    foreach (var c in _panelSteps.Controls)
        //    {
        //        (c as Control).Visible = false;
        //    }


        //}
        /*
        private void _protoFileStep_ProtoTextChanged(object sender, EventArgs e)
        {
            try
            {
                var kProtoFile = new ProtoToKProtoConverter().Convert("zzz", _protoFileStep.ProtoFileText);

                if (kProtoFile == null || kProtoFile.GeneratedProtoFile == null || kProtoFile.GeneratedProtoFile.ProtoMessage.Count == 0)
                {

                    return;
                }
                SolutionName = InferSolutionName(kProtoFile);
                ProjectName = InferProjectName(kProtoFile);
            }
            catch (ProtoCompileException ex)
            {
               // MessageBox.Show(ex.Message);
            }
        }

       */
        
       

       
       
             
        private void _buttonCancel_Click(object sender, EventArgs e)
        {

            DialogResult = DialogResult.Cancel;
            this.Close();
        }
        


        private void ProjectOptionsDialog_Load(object sender, EventArgs e)
        {
            
        }

      

        public void DisableNext()
        {
            _buttonNext.Text = "Next";
            _buttonNext.Enabled = false;
        }

        public void EnableFinish()
        {
            _buttonNext.Text = "Finish";
            _buttonNext.Enabled = true;
        }

        public void DisablePrevious()
        {
            _buttonBack.Enabled = false;
        }

        public void EnableNext()
        {
            _buttonNext.Text = "Next";
            _buttonNext.Enabled = true;
        }

        public void CloseWizard()
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }

        public void AddView(IView view)
        {
            var control = view as Control;
            control.Visible = false;
            this._panelSteps.Controls.Add(control);
        }

        public void EnableGenerate()
        {
            _buttonNext.Text = "Generate";
            _buttonNext.Enabled = true;
        }

        public int ShowDialog(IntPtr handle)
        {
            return (int)ShowDialog(new MyWin32Window() {  Handle = handle });
        }
    }

    class MyWin32Window : IWin32Window
    {
        public IntPtr Handle { get; set; }
    }

}
