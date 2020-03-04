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
    public partial class SolutionsSelectStep : UserControl
    {
        public SolutionsSelectStep()
        {
            InitializeComponent();
        }

        public List<KSolution> SelectedTemplateSolutions
        {
            get
            {
                var list = new List<KSolution>();

                foreach (var item in _checkedListBoxTemplateSolutions.CheckedItems)
                {
                    list.Add(item as KSolution);

                }

                return list;
            }
        }

        internal void Bind(List<KSolution> templates)
        {
            foreach (var template in templates)
            {
                //    addListItem(template);
                _checkedListBoxTemplateSolutions.Items.Add(template);

            }
            (_checkedListBoxTemplateSolutions as ListBox).DisplayMember = "SolutionName";

            checkAll();


        }

        private void _buttonCheckAll_Click(object sender, EventArgs e)
        {
            checkAll();
        }

        private void checkAll()
        {
            for (int i = 0; i < _checkedListBoxTemplateSolutions.Items.Count; i++)
            {
                _checkedListBoxTemplateSolutions.SetItemChecked(i, true);
            }
        }

        private void _buttonUncheckAll_Click(object sender, EventArgs e)
        {
            uncheckAll();
        }

        private void uncheckAll()
        {
            for (int i = 0; i < _checkedListBoxTemplateSolutions.Items.Count; i++)
            {
                _checkedListBoxTemplateSolutions.SetItemChecked(i, false);
            }
        }

        /*
        private void addListItem(KSolution templateSolution)
        {
            int index = _checkedListBoxTemplateSolutions.Items.Add(templateSolution.SolutionName);
            _checkedListBoxTemplateSolutions.SetItemChecked(index, true);
        }*/

    }
}
