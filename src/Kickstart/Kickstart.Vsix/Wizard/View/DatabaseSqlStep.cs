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
using Kickstart.Wizard.View;
using Kickstart.Sample;
using Kickstart.Utility;

namespace Kickstart.Vsix.Wizard
{
    public partial class DatabaseSqlStep : UserControl, IDatabaseSqlView
    {
        public DatabaseSqlStep()
        {
            InitializeComponent();
            _comboBoxSqlDialect.SelectedIndex = 0;
        }
        public bool GenerateStoredProcAsEmbeddedQuery
        {
            get
            {
                return _checkBoxStoredProcsAsEmbeddedQueries.Checked;
            }
            set
            {
                _checkBoxStoredProcsAsEmbeddedQueries.Checked = value;
            }
        }

        public string MockSqlViewText {
            get
            {
                return _textBoxSqlMockView.Text;    
            }
            set
            {
                _textBoxSqlMockView.Text = value;
            }
        }

        public string SqlTableText
        {
            get
            {
                return _textBoxDatabaseSqlTableText.Text;
            }
            set
            {
                _textBoxDatabaseSqlTableText.Text = value;
            }
        }
        public string SqlTableTypeText
        {
            get
            {
                return _textBoxSqlTableType.Text;
            }
            set
            {
                _textBoxSqlTableType.Text = value;
            }
        }

        public string SqlViewText
        {
            get
            {
                return _textBoxSqlView.Text;
            }
            set
            {
                _textBoxSqlView.Text = value;
            }
        }

        public string SqlStoredProcText
        {
            get
            {
                return _textBoxSqlStoredProc.Text;
            }
            set
            {
                _textBoxSqlStoredProc.Text = value;
            }
        }
        public bool ConvertToSnakeCase
        {
            get
            {
                return _checkBoxConvertToSnakeCase.Checked;
            }
            set
            {
                _checkBoxConvertToSnakeCase.Checked = value;
            }
              
        }

        public Func<object, EventArgs, Task> GenerateStoredProcAsEmbeddedQueryChanged { get; set; }
        public Func<object, EventArgs, Task> SqlTableTextChanged { get; set; }
        public Func<object, EventArgs, Task> SqlTableTypeTextChanged { get; set; }

        public Func<object, EventArgs, Task> SqlStoredProcTextChanged { get; set; }

        public Func<object, EventArgs, Task> ConvertToSnakeCaseChanged { get; set; }



        
        
        
        
        private void _checkBoxStoredProcsAsEmbeddedQueries_CheckedChanged(object sender, EventArgs e)
        {
            GenerateStoredProcAsEmbeddedQueryChanged?.Invoke(null, null);
        }

        private void DatabaseSqlStep_Load(object sender, EventArgs e)
        {
            GenerateStoredProcAsEmbeddedQueryChanged?.Invoke(null, null);
            ConvertToSnakeCaseChanged?.Invoke(null, null);

        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            SqlTableTypeTextChanged?.Invoke(null, null);
        }
        private void _textBoxDatabaseSqlTableText_TextChanged_1(object sender, EventArgs e)
        {
            SqlTableTextChanged?.Invoke(null, null);
        }

        private void _textBoxSqlStoredProc_TextChanged(object sender, EventArgs e)
        {

            SqlStoredProcTextChanged?.Invoke(null, null);
        }

        
        private void _buttonViewSqlTypeMapping_Click(object sender, EventArgs e)
        {
          
            var form = new Form() { Size = new Size(800,600) };
            var textBox = new TextBox();
            textBox.Multiline = true;
            textBox.Dock = DockStyle.Fill;
            form.Controls.Add(textBox);
            textBox.Font = new Font(FontFamily.GenericMonospace, textBox.Font.Size);
            textBox.Text = SqlMapper.PrintMappingFromSqlServer();
            form.StartPosition = FormStartPosition.CenterParent;
            form.ShowDialog(this);
        }

        
        private void _checkBoxConvertToSnakeCase_CheckedChanged(object sender, EventArgs e)
        {
            ConvertToSnakeCaseChanged?.Invoke(null, null);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _textBoxDatabaseSqlTableText.Text += SampleFileReader.ReadSampleFile("Sql", "Tables.sql");
            _checkBoxConvertToSnakeCase.Checked = false;
        }
    }
}
