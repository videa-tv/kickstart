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
    public partial class SolutionsEditStep : UserControl
    {
        public SolutionsEditStep()
        {
            InitializeComponent();
        }

        internal void Bind(List<KSolution> selectedTemplateSolutions)
        {
            foreach (var template in selectedTemplateSolutions)
            {
                addTabPage(template);
            }

        }
        private void addTabPage(KSolution templateSolution)
        {
            var tabPage = new TabPage();
            tabPage.Text = $"{templateSolution.SolutionName}";

            var control = new SolutionOptionsControl();
            tabPage.Controls.Add(control);
            control.Dock = DockStyle.Fill;
            control.Bind(templateSolution);
            _tabControl.TabPages.Add(tabPage);
        }
    }
}
