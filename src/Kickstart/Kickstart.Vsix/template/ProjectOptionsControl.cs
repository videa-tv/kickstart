using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Vsix
{
    public partial class ProjectOptionsControl : UserControl
    {
        public ProjectOptionsControl()
        {
            InitializeComponent();
        }

        private void ProjectOptionsControl_Load(object sender, EventArgs e)
        {
        }

        private void loadProjectIsCombo()
        {
            this._comboBoxProjectIs.DataSource = Enum.GetValues(typeof(CProjectIs));
        }

        KProject _templateProject;
        public void Bind(KProject templateProject)
        {

            loadProjectIsCombo();

            _templateProject = templateProject;
            _checkBoxKickstart.Checked = _templateProject.Kickstart;
            _textBoxProjectName.Text = _templateProject.ProjectFullName;
            _textBoxProjectFolder.Text = _templateProject.ProjectFolder;
            _comboBoxProjectIs.SelectedItem = _templateProject.ProjectIs;
        }

        private void _textBoxProjectName_TextChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
    }
}
