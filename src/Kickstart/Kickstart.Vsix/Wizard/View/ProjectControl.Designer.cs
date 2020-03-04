namespace Kickstart.Vsix.Wizard
{
    partial class ProjectControl
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
            this._textBoxCompanyName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this._textBoxSolutionName = new System.Windows.Forms.TextBox();
            this._textBoxProjectName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // _textBoxCompanyName
            // 
            this._textBoxCompanyName.Location = new System.Drawing.Point(6, 27);
            this._textBoxCompanyName.Name = "_textBoxCompanyName";
            this._textBoxCompanyName.Size = new System.Drawing.Size(100, 20);
            this._textBoxCompanyName.TabIndex = 7;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 11);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(85, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Company Name:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(127, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(106, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Solution Name (root):";
            // 
            // _textBoxSolutionName
            // 
            this._textBoxSolutionName.Location = new System.Drawing.Point(130, 27);
            this._textBoxSolutionName.Name = "_textBoxSolutionName";
            this._textBoxSolutionName.Size = new System.Drawing.Size(142, 20);
            this._textBoxSolutionName.TabIndex = 4;
            // 
            // _textBoxProjectName
            // 
            this._textBoxProjectName.Location = new System.Drawing.Point(303, 27);
            this._textBoxProjectName.Name = "_textBoxProjectName";
            this._textBoxProjectName.Size = new System.Drawing.Size(100, 20);
            this._textBoxProjectName.TabIndex = 9;
            this._textBoxProjectName.TextChanged += new System.EventHandler(this._textBoxProjectName_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(300, 11);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(101, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Project Name (root):";
            // 
            // ProjectControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._textBoxProjectName);
            this.Controls.Add(this.label3);
            this.Controls.Add(this._textBoxCompanyName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._textBoxSolutionName);
            this.Name = "ProjectControl";
            this.Size = new System.Drawing.Size(426, 62);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox _textBoxCompanyName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox _textBoxSolutionName;
        private System.Windows.Forms.TextBox _textBoxProjectName;
        private System.Windows.Forms.Label label3;
    }
}
