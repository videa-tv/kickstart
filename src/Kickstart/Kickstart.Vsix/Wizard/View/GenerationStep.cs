using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Kickstart.Wizard.View;

namespace Kickstart.Vsix.Wizard
{
    public partial class GenerationStep : UserControl, IGenerationView
    {
        public GenerationStep()
        {
            InitializeComponent();
            _progressMessage.Text = string.Empty;
        }

        public void IncrementProgress(int percentChange, string progressMessage)
        {
            if (InvokeRequired)
            {

                Invoke(new Action(() => IncrementProgress(percentChange, progressMessage)));
                return;
            }
            this._progressBar.Increment(percentChange);
            if (!string.IsNullOrEmpty(progressMessage))
            {
                _progressMessage.Text = progressMessage;
            }
        }

      

        public string ProgressStatus {
        set
            { _progressMessage.Text = value; } }
    }
}
