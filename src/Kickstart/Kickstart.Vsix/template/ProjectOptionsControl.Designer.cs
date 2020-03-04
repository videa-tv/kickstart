namespace Kickstart.Vsix
{
    partial class ProjectOptionsControl
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
            this.label1 = new System.Windows.Forms.Label();
            this._textBoxProjectName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this._textBoxProjectFolder = new System.Windows.Forms.TextBox();
            this._checkBoxKickstart = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this._comboBoxProjectIs = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 68);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(93, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Project Full Name:";
            // 
            // _textBoxProjectName
            // 
            this._textBoxProjectName.Location = new System.Drawing.Point(115, 65);
            this._textBoxProjectName.Name = "_textBoxProjectName";
            this._textBoxProjectName.ReadOnly = true;
            this._textBoxProjectName.Size = new System.Drawing.Size(281, 20);
            this._textBoxProjectName.TabIndex = 1;
            this._textBoxProjectName.TextChanged += new System.EventHandler(this._textBoxProjectName_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 100);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Project Folder:";
            // 
            // _textBoxProjectFolder
            // 
            this._textBoxProjectFolder.Location = new System.Drawing.Point(115, 97);
            this._textBoxProjectFolder.Name = "_textBoxProjectFolder";
            this._textBoxProjectFolder.ReadOnly = true;
            this._textBoxProjectFolder.Size = new System.Drawing.Size(281, 20);
            this._textBoxProjectFolder.TabIndex = 3;
            // 
            // _checkBoxKickstart
            // 
            this._checkBoxKickstart.AutoSize = true;
            this._checkBoxKickstart.Location = new System.Drawing.Point(115, 3);
            this._checkBoxKickstart.Name = "_checkBoxKickstart";
            this._checkBoxKickstart.Size = new System.Drawing.Size(103, 17);
            this._checkBoxKickstart.TabIndex = 4;
            this._checkBoxKickstart.Text = "Kickstart Project";
            this._checkBoxKickstart.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 40);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(54, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Project Is:";
            this.label3.Click += new System.EventHandler(this.label3_Click);
            // 
            // _comboBoxProjectIs
            // 
            this._comboBoxProjectIs.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._comboBoxProjectIs.FormattingEnabled = true;
            this._comboBoxProjectIs.Location = new System.Drawing.Point(115, 38);
            this._comboBoxProjectIs.Name = "_comboBoxProjectIs";
            this._comboBoxProjectIs.Size = new System.Drawing.Size(121, 21);
            this._comboBoxProjectIs.TabIndex = 6;
            // 
            // ProjectOptionsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._comboBoxProjectIs);
            this.Controls.Add(this.label3);
            this.Controls.Add(this._checkBoxKickstart);
            this.Controls.Add(this._textBoxProjectFolder);
            this.Controls.Add(this.label2);
            this.Controls.Add(this._textBoxProjectName);
            this.Controls.Add(this.label1);
            this.Name = "ProjectOptionsControl";
            this.Size = new System.Drawing.Size(417, 157);
            this.Load += new System.EventHandler(this.ProjectOptionsControl_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox _textBoxProjectName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox _textBoxProjectFolder;
        private System.Windows.Forms.CheckBox _checkBoxKickstart;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox _comboBoxProjectIs;
    }
}
