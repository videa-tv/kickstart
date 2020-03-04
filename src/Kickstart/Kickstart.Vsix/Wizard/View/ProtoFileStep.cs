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
using Kickstart.Pass1.Service;
using Kickstart.Wizard.View;

namespace Kickstart.Vsix.Wizard
{
    public partial class ProtoFileStep : UserControl, IProtoFileView
    {
        
        public ProtoFileStep()
        {
            InitializeComponent();
        }

        public string ProtoFileText
        {
            get
            {
                return _textBoxProtoFileText.Text;
            }
        }

        public Func<object, EventArgs, Task> ProtoTextChanged
        {
            get;set;
        }

        private void _buttonLoadSample_Click(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
        }

       
        internal void Bind(KProtoFile kProtoFile)
        {
            this._textBoxProtoFileText.Text = kProtoFile.ProtoFileText;
        }

        private void _textBoxProtoFileText_TextChanged(object sender, EventArgs e)
        {
          
            if (ProtoTextChanged != null)
                ProtoTextChanged(this, null);
        }

        private void _loadSampleProto3_Click(object sender, EventArgs e)
        {
        }

        private void _buttonLoadSampleProto4_Click(object sender, EventArgs e)
        {
        }

        private void _buttonLoadSampleProto5_Click(object sender, EventArgs e)
        {
            
        }

        private void _buttonSampleKickstart_Click(object sender, EventArgs e)
        {
        }
    }
}
