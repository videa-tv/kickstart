namespace Kickstart.Vsix
{
    partial class SolutionOptionsControl
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
            this._textBoxSolutionName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this._textBoxCompanyName = new System.Windows.Forms.TextBox();
            this._tabControlProjects = new System.Windows.Forms.TabControl();
            this._checkBoxKickstart = new System.Windows.Forms.CheckBox();
            this._comboBoxSourceOfMetadata = new System.Windows.Forms.ComboBox();
            this._buttonViewProtoOrSql = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // _textBoxSolutionName
            // 
            this._textBoxSolutionName.Location = new System.Drawing.Point(123, 28);
            this._textBoxSolutionName.Name = "_textBoxSolutionName";
            this._textBoxSolutionName.Size = new System.Drawing.Size(100, 20);
            this._textBoxSolutionName.TabIndex = 0;
            this._textBoxSolutionName.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(120, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Solution Name:";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(241, 13);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(85, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Company Name:";
            // 
            // _textBoxCompanyName
            // 
            this._textBoxCompanyName.Location = new System.Drawing.Point(244, 29);
            this._textBoxCompanyName.Name = "_textBoxCompanyName";
            this._textBoxCompanyName.Size = new System.Drawing.Size(100, 20);
            this._textBoxCompanyName.TabIndex = 3;
            // 
            // _tabControlProjects
            // 
            this._tabControlProjects.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._tabControlProjects.Location = new System.Drawing.Point(20, 69);
            this._tabControlProjects.Multiline = true;
            this._tabControlProjects.Name = "_tabControlProjects";
            this._tabControlProjects.SelectedIndex = 0;
            this._tabControlProjects.Size = new System.Drawing.Size(631, 289);
            this._tabControlProjects.TabIndex = 4;
            // 
            // _checkBoxKickstart
            // 
            this._checkBoxKickstart.AutoSize = true;
            this._checkBoxKickstart.Location = new System.Drawing.Point(9, 31);
            this._checkBoxKickstart.Name = "_checkBoxKickstart";
            this._checkBoxKickstart.Size = new System.Drawing.Size(108, 17);
            this._checkBoxKickstart.TabIndex = 5;
            this._checkBoxKickstart.Text = "Kickstart Solution";
            this._checkBoxKickstart.UseVisualStyleBackColor = true;
            this._checkBoxKickstart.CheckedChanged += new System.EventHandler(this._checkBoxKickstart_CheckedChanged);
            // 
            // _comboBoxSourceOfMetadata
            // 
            this._comboBoxSourceOfMetadata.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._comboBoxSourceOfMetadata.FormattingEnabled = true;
            this._comboBoxSourceOfMetadata.Items.AddRange(new object[] {
            "N/A",
            "Proto First",
            "DB First"});
            this._comboBoxSourceOfMetadata.Location = new System.Drawing.Point(364, 29);
            this._comboBoxSourceOfMetadata.Name = "_comboBoxSourceOfMetadata";
            this._comboBoxSourceOfMetadata.Size = new System.Drawing.Size(121, 21);
            this._comboBoxSourceOfMetadata.TabIndex = 6;
            // 
            // _buttonViewProtoOrSql
            // 
            this._buttonViewProtoOrSql.Location = new System.Drawing.Point(491, 27);
            this._buttonViewProtoOrSql.Name = "_buttonViewProtoOrSql";
            this._buttonViewProtoOrSql.Size = new System.Drawing.Size(75, 23);
            this._buttonViewProtoOrSql.TabIndex = 7;
            this._buttonViewProtoOrSql.Text = "View..";
            this._buttonViewProtoOrSql.UseVisualStyleBackColor = true;
            this._buttonViewProtoOrSql.Click += new System.EventHandler(this._buttonViewProtoOrSql_Click);
            // 
            // SolutionOptionsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._buttonViewProtoOrSql);
            this.Controls.Add(this._comboBoxSourceOfMetadata);
            this.Controls.Add(this._checkBoxKickstart);
            this.Controls.Add(this._tabControlProjects);
            this.Controls.Add(this._textBoxCompanyName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._textBoxSolutionName);
            this.Name = "SolutionOptionsControl";
            this.Size = new System.Drawing.Size(652, 361);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox _textBoxSolutionName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox _textBoxCompanyName;
        private System.Windows.Forms.TabControl _tabControlProjects;
        private System.Windows.Forms.CheckBox _checkBoxKickstart;
        private System.Windows.Forms.ComboBox _comboBoxSourceOfMetadata;
        private System.Windows.Forms.Button _buttonViewProtoOrSql;
    }
}
