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
    public partial class ProjectControl : UserControl, IProjectView
    {
        public Func<object, EventArgs, Task> ProjectNameChanged { get; set; }

        public string SolutionName
        {
            get
            {
                return _textBoxSolutionName.Text;
            }
            set
            {
                _textBoxSolutionName.Text = value;
            }
        }
        public string CompanyName
        {
            get
            {
                return _textBoxCompanyName.Text;
            }
            set
            {
                _textBoxCompanyName.Text = value;
            }
        }
        public string ProjectName
        {
            get
            {
                return _textBoxProjectName.Text ;
            }
            set
            {
                _textBoxProjectName.Text = value;
            }
        }
        public ProjectControl()
        {
            InitializeComponent();
        }

        private void _textBoxProjectName_TextChanged(object sender, EventArgs e)
        {
            ProjectNameChanged?.Invoke(null, null);
        }
    }
}
