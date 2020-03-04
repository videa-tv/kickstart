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

namespace Kickstart.Vsix
{
    public partial class SolutionOptionsControl : UserControl
    {
        KSolution _templateSolution;
        public SolutionOptionsControl()
        {
            InitializeComponent();
        }
        public void Bind(KSolution templateSolution)
        {
            _templateSolution = templateSolution;

            _textBoxSolutionName.Text = templateSolution.SolutionName;
            _textBoxCompanyName.Text = templateSolution.CompanyName;
            if (!string.IsNullOrEmpty(templateSolution.ProtoFileText))
            {
                _comboBoxSourceOfMetadata.SelectedIndex = 1;
            }
            else
            {
                _comboBoxSourceOfMetadata.SelectedIndex = 2;
            }

            foreach (var templateProject in _templateSolution.Project)
            {
                var projectControl = new ProjectOptionsControl();
                var tabPage = new TabPage();
                tabPage.Text = templateProject.ProjectShortName;
                _tabControlProjects.TabPages.Add(tabPage);
                tabPage.Controls.Add(projectControl);
                projectControl.Dock = DockStyle.Fill;
                projectControl.Bind(templateProject);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void _checkBoxKickstart_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void _buttonViewProtoOrSql_Click(object sender, EventArgs e)
        {
            MessageBox.Show(_templateSolution.ProtoFileText);
        }
    }
}                                 
