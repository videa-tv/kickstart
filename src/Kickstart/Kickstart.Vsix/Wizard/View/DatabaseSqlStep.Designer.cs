namespace Kickstart.Vsix.Wizard
{
    partial class DatabaseSqlStep
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.label3 = new System.Windows.Forms.Label();
            this._textBoxDatabaseSqlTableText = new System.Windows.Forms.TextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.label4 = new System.Windows.Forms.Label();
            this._checkBoxStoredProcsAsEmbeddedQueries = new System.Windows.Forms.CheckBox();
            this._textBoxSqlStoredProc = new System.Windows.Forms.TextBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.label5 = new System.Windows.Forms.Label();
            this._textBoxSqlMockView = new System.Windows.Forms.TextBox();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this._textBoxSqlView = new System.Windows.Forms.TextBox();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this._textBoxSqlQueries = new System.Windows.Forms.TextBox();
            this.tabPage6 = new System.Windows.Forms.TabPage();
            this._textBoxSqlTableType = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this._comboBoxSqlDialect = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this._buttonViewSqlTypeMapping = new System.Windows.Forms.Button();
            this._checkBoxConvertToSnakeCase = new System.Windows.Forms.CheckBox();
            this.button1 = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.tabPage5.SuspendLayout();
            this.tabPage6.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Controls.Add(this.tabPage5);
            this.tabControl1.Controls.Add(this.tabPage6);
            this.tabControl1.Location = new System.Drawing.Point(4, 42);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(936, 457);
            this.tabControl1.TabIndex = 2;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Controls.Add(this._textBoxDatabaseSqlTableText);
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPage1.Size = new System.Drawing.Size(928, 428);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Tables";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.RoyalBlue;
            this.label3.Location = new System.Drawing.Point(8, 15);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(351, 17);
            this.label3.TabIndex = 6;
            this.label3.Text = "Indexes should be added to the generated project files";
            // 
            // _textBoxDatabaseSqlTableText
            // 
            this._textBoxDatabaseSqlTableText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._textBoxDatabaseSqlTableText.Location = new System.Drawing.Point(4, 50);
            this._textBoxDatabaseSqlTableText.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this._textBoxDatabaseSqlTableText.MaxLength = 0;
            this._textBoxDatabaseSqlTableText.Multiline = true;
            this._textBoxDatabaseSqlTableText.Name = "_textBoxDatabaseSqlTableText";
            this._textBoxDatabaseSqlTableText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._textBoxDatabaseSqlTableText.Size = new System.Drawing.Size(912, 434);
            this._textBoxDatabaseSqlTableText.TabIndex = 2;
            this._textBoxDatabaseSqlTableText.TextChanged += new System.EventHandler(this._textBoxDatabaseSqlTableText_TextChanged_1);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.label4);
            this.tabPage2.Controls.Add(this._checkBoxStoredProcsAsEmbeddedQueries);
            this.tabPage2.Controls.Add(this._textBoxSqlStoredProc);
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPage2.Size = new System.Drawing.Size(928, 428);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Stored Procs";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.Color.RoyalBlue;
            this.label4.Location = new System.Drawing.Point(331, 22);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(460, 17);
            this.label4.TabIndex = 7;
            this.label4.Text = "Stored procs not used in DAL should be added to the generated project";
            // 
            // _checkBoxStoredProcsAsEmbeddedQueries
            // 
            this._checkBoxStoredProcsAsEmbeddedQueries.AutoSize = true;
            this._checkBoxStoredProcsAsEmbeddedQueries.Checked = true;
            this._checkBoxStoredProcsAsEmbeddedQueries.CheckState = System.Windows.Forms.CheckState.Checked;
            this._checkBoxStoredProcsAsEmbeddedQueries.Location = new System.Drawing.Point(8, 22);
            this._checkBoxStoredProcsAsEmbeddedQueries.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this._checkBoxStoredProcsAsEmbeddedQueries.Name = "_checkBoxStoredProcsAsEmbeddedQueries";
            this._checkBoxStoredProcsAsEmbeddedQueries.Size = new System.Drawing.Size(316, 21);
            this._checkBoxStoredProcsAsEmbeddedQueries.TabIndex = 5;
            this._checkBoxStoredProcsAsEmbeddedQueries.Text = "Transform stored procs to embedded queries";
            this._checkBoxStoredProcsAsEmbeddedQueries.UseVisualStyleBackColor = true;
            this._checkBoxStoredProcsAsEmbeddedQueries.CheckedChanged += new System.EventHandler(this._checkBoxStoredProcsAsEmbeddedQueries_CheckedChanged);
            // 
            // _textBoxSqlStoredProc
            // 
            this._textBoxSqlStoredProc.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._textBoxSqlStoredProc.Location = new System.Drawing.Point(4, 76);
            this._textBoxSqlStoredProc.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this._textBoxSqlStoredProc.MaxLength = 0;
            this._textBoxSqlStoredProc.Multiline = true;
            this._textBoxSqlStoredProc.Name = "_textBoxSqlStoredProc";
            this._textBoxSqlStoredProc.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._textBoxSqlStoredProc.Size = new System.Drawing.Size(912, 408);
            this._textBoxSqlStoredProc.TabIndex = 3;
            this._textBoxSqlStoredProc.TextChanged += new System.EventHandler(this._textBoxSqlStoredProc_TextChanged);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.label5);
            this.tabPage3.Controls.Add(this._textBoxSqlMockView);
            this.tabPage3.Location = new System.Drawing.Point(4, 25);
            this.tabPage3.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPage3.Size = new System.Drawing.Size(928, 428);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Views (Mock)";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.ForeColor = System.Drawing.Color.RoyalBlue;
            this.label5.Location = new System.Drawing.Point(8, 15);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(253, 17);
            this.label5.TabIndex = 7;
            this.label5.Text = "Mock views are transformed into tables";
            // 
            // _textBoxSqlMockView
            // 
            this._textBoxSqlMockView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._textBoxSqlMockView.Location = new System.Drawing.Point(4, 47);
            this._textBoxSqlMockView.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this._textBoxSqlMockView.MaxLength = 0;
            this._textBoxSqlMockView.Multiline = true;
            this._textBoxSqlMockView.Name = "_textBoxSqlMockView";
            this._textBoxSqlMockView.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._textBoxSqlMockView.Size = new System.Drawing.Size(853, 434);
            this._textBoxSqlMockView.TabIndex = 3;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this._textBoxSqlView);
            this.tabPage4.Location = new System.Drawing.Point(4, 25);
            this.tabPage4.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPage4.Size = new System.Drawing.Size(928, 428);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Views";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // _textBoxSqlView
            // 
            this._textBoxSqlView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._textBoxSqlView.Location = new System.Drawing.Point(4, 4);
            this._textBoxSqlView.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this._textBoxSqlView.MaxLength = 0;
            this._textBoxSqlView.Multiline = true;
            this._textBoxSqlView.Name = "_textBoxSqlView";
            this._textBoxSqlView.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._textBoxSqlView.Size = new System.Drawing.Size(872, 480);
            this._textBoxSqlView.TabIndex = 3;
            // 
            // tabPage5
            // 
            this.tabPage5.Controls.Add(this._textBoxSqlQueries);
            this.tabPage5.Location = new System.Drawing.Point(4, 25);
            this.tabPage5.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPage5.Size = new System.Drawing.Size(928, 428);
            this.tabPage5.TabIndex = 4;
            this.tabPage5.Text = "Queries";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // _textBoxSqlQueries
            // 
            this._textBoxSqlQueries.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._textBoxSqlQueries.Location = new System.Drawing.Point(4, 4);
            this._textBoxSqlQueries.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this._textBoxSqlQueries.MaxLength = 0;
            this._textBoxSqlQueries.Multiline = true;
            this._textBoxSqlQueries.Name = "_textBoxSqlQueries";
            this._textBoxSqlQueries.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._textBoxSqlQueries.Size = new System.Drawing.Size(872, 480);
            this._textBoxSqlQueries.TabIndex = 3;
            // 
            // tabPage6
            // 
            this.tabPage6.Controls.Add(this._textBoxSqlTableType);
            this.tabPage6.Location = new System.Drawing.Point(4, 25);
            this.tabPage6.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPage6.Name = "tabPage6";
            this.tabPage6.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPage6.Size = new System.Drawing.Size(928, 428);
            this.tabPage6.TabIndex = 5;
            this.tabPage6.Text = "Table Types";
            this.tabPage6.UseVisualStyleBackColor = true;
            // 
            // _textBoxSqlTableType
            // 
            this._textBoxSqlTableType.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._textBoxSqlTableType.Location = new System.Drawing.Point(4, 7);
            this._textBoxSqlTableType.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this._textBoxSqlTableType.MaxLength = 0;
            this._textBoxSqlTableType.Multiline = true;
            this._textBoxSqlTableType.Name = "_textBoxSqlTableType";
            this._textBoxSqlTableType.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._textBoxSqlTableType.Size = new System.Drawing.Size(912, 413);
            this._textBoxSqlTableType.TabIndex = 3;
            this._textBoxSqlTableType.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 11);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 17);
            this.label1.TabIndex = 3;
            this.label1.Text = "Sql Dialect:";
            // 
            // _comboBoxSqlDialect
            // 
            this._comboBoxSqlDialect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._comboBoxSqlDialect.FormattingEnabled = true;
            this._comboBoxSqlDialect.Items.AddRange(new object[] {
            "TSQL - Sql Server"});
            this._comboBoxSqlDialect.Location = new System.Drawing.Point(99, 7);
            this._comboBoxSqlDialect.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this._comboBoxSqlDialect.Name = "_comboBoxSqlDialect";
            this._comboBoxSqlDialect.Size = new System.Drawing.Size(183, 24);
            this._comboBoxSqlDialect.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.RoyalBlue;
            this.label2.Location = new System.Drawing.Point(340, 18);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(388, 17);
            this.label2.TabIndex = 5;
            this.label2.Text = "These have been inferred. Make any necessary corrections.";
            // 
            // _buttonViewSqlTypeMapping
            // 
            this._buttonViewSqlTypeMapping.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this._buttonViewSqlTypeMapping.Location = new System.Drawing.Point(433, 498);
            this._buttonViewSqlTypeMapping.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this._buttonViewSqlTypeMapping.Name = "_buttonViewSqlTypeMapping";
            this._buttonViewSqlTypeMapping.Size = new System.Drawing.Size(244, 28);
            this._buttonViewSqlTypeMapping.TabIndex = 13;
            this._buttonViewSqlTypeMapping.Text = "View Type Mapping";
            this._buttonViewSqlTypeMapping.UseVisualStyleBackColor = true;
            this._buttonViewSqlTypeMapping.Click += new System.EventHandler(this._buttonViewSqlTypeMapping_Click);
            // 
            // _checkBoxConvertToSnakeCase
            // 
            this._checkBoxConvertToSnakeCase.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._checkBoxConvertToSnakeCase.AutoSize = true;
            this._checkBoxConvertToSnakeCase.Checked = true;
            this._checkBoxConvertToSnakeCase.CheckState = System.Windows.Forms.CheckState.Checked;
            this._checkBoxConvertToSnakeCase.Location = new System.Drawing.Point(116, 505);
            this._checkBoxConvertToSnakeCase.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this._checkBoxConvertToSnakeCase.Name = "_checkBoxConvertToSnakeCase";
            this._checkBoxConvertToSnakeCase.Size = new System.Drawing.Size(266, 21);
            this._checkBoxConvertToSnakeCase.TabIndex = 15;
            this._checkBoxConvertToSnakeCase.Text = "Convert table/columns to snake_case";
            this._checkBoxConvertToSnakeCase.UseVisualStyleBackColor = true;
            this._checkBoxConvertToSnakeCase.CheckedChanged += new System.EventHandler(this._checkBoxConvertToSnakeCase_CheckedChanged);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(944, 130);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(244, 28);
            this.button1.TabIndex = 16;
            this.button1.Text = "Load Sample";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // DatabaseSqlStep
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.button1);
            this.Controls.Add(this._checkBoxConvertToSnakeCase);
            this.Controls.Add(this._buttonViewSqlTypeMapping);
            this.Controls.Add(this.label2);
            this.Controls.Add(this._comboBoxSqlDialect);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tabControl1);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "DatabaseSqlStep";
            this.Size = new System.Drawing.Size(1196, 533);
            this.Load += new System.EventHandler(this.DatabaseSqlStep_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            this.tabPage5.ResumeLayout(false);
            this.tabPage5.PerformLayout();
            this.tabPage6.ResumeLayout(false);
            this.tabPage6.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TextBox _textBoxDatabaseSqlTableText;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TextBox _textBoxSqlStoredProc;
        private System.Windows.Forms.TextBox _textBoxSqlMockView;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.TextBox _textBoxSqlView;
        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.TextBox _textBoxSqlQueries;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox _comboBoxSqlDialect;
        private System.Windows.Forms.CheckBox _checkBoxStoredProcsAsEmbeddedQueries;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TabPage tabPage6;
        private System.Windows.Forms.TextBox _textBoxSqlTableType;
        private System.Windows.Forms.Button _buttonViewSqlTypeMapping;
        private System.Windows.Forms.CheckBox _checkBoxConvertToSnakeCase;
        private System.Windows.Forms.Button button1;
    }
}
