namespace Kickstart.Vsix
{
    partial class KickstartWizardDialog
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._buttonNext = new System.Windows.Forms.Button();
            this._buttonCancel = new System.Windows.Forms.Button();
            this._buttonBack = new System.Windows.Forms.Button();
            this._panelSteps = new System.Windows.Forms.Panel();
            this._projectControl = new Kickstart.Vsix.Wizard.ProjectControl();
            this.SuspendLayout();
            // 
            // _buttonNext
            // 
            this._buttonNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonNext.Location = new System.Drawing.Point(774, 501);
            this._buttonNext.Name = "_buttonNext";
            this._buttonNext.Size = new System.Drawing.Size(75, 23);
            this._buttonNext.TabIndex = 1;
            this._buttonNext.Text = "Next";
            this._buttonNext.UseVisualStyleBackColor = true;
            this._buttonNext.Click += new System.EventHandler(this._buttonNext_Click);
            // 
            // _buttonCancel
            // 
            this._buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._buttonCancel.Location = new System.Drawing.Point(853, 501);
            this._buttonCancel.Name = "_buttonCancel";
            this._buttonCancel.Size = new System.Drawing.Size(75, 23);
            this._buttonCancel.TabIndex = 2;
            this._buttonCancel.Text = "Cancel";
            this._buttonCancel.UseVisualStyleBackColor = true;
            this._buttonCancel.Click += new System.EventHandler(this._buttonCancel_Click);
            // 
            // _buttonBack
            // 
            this._buttonBack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._buttonBack.Location = new System.Drawing.Point(35, 501);
            this._buttonBack.Name = "_buttonBack";
            this._buttonBack.Size = new System.Drawing.Size(75, 23);
            this._buttonBack.TabIndex = 3;
            this._buttonBack.Text = "Back";
            this._buttonBack.UseVisualStyleBackColor = true;
            this._buttonBack.Click += new System.EventHandler(this._buttonBack_Click);
            // 
            // _panelSteps
            // 
            this._panelSteps.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._panelSteps.Location = new System.Drawing.Point(12, 48);
            this._panelSteps.Name = "_panelSteps";
            this._panelSteps.Size = new System.Drawing.Size(916, 447);
            this._panelSteps.TabIndex = 4;
            // 
            // _projectControl
            // 
            this._projectControl.Location = new System.Drawing.Point(12, -8);
            this._projectControl.Name = "_projectControl";
            this._projectControl.ProjectName = "";
            this._projectControl.ProjectNameChanged = null;
            this._projectControl.Size = new System.Drawing.Size(417, 50);
            this._projectControl.SolutionName = "";
            this._projectControl.TabIndex = 5;
            // 
            // KickstartWizardDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(940, 536);
            this.Controls.Add(this._projectControl);
            this.Controls.Add(this._panelSteps);
            this.Controls.Add(this._buttonBack);
            this.Controls.Add(this._buttonCancel);
            this.Controls.Add(this._buttonNext);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "KickstartWizardDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Kickstart Wizard";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.ProjectOptionsDialog_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button _buttonNext;
        private System.Windows.Forms.Button _buttonCancel;
        private System.Windows.Forms.Button _buttonBack;
        private System.Windows.Forms.Panel _panelSteps;
        private Wizard.ProjectControl _projectControl;
    }
}