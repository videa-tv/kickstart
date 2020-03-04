namespace Kickstart.Vsix.Wizard
{
    partial class KickstartWizardDialogWeb
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
            this._panelBrowser = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // _panelBrowser
            // 
            this._panelBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this._panelBrowser.Location = new System.Drawing.Point(0, 0);
            this._panelBrowser.Name = "_panelBrowser";
            this._panelBrowser.Size = new System.Drawing.Size(670, 800);
            this._panelBrowser.TabIndex = 0;
            // 
            // KickstartWizardDialogWeb
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(670, 800);
            this.Controls.Add(this._panelBrowser);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "KickstartWizardDialogWeb";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "KickstartWizardDialogWeb";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel _panelBrowser;
    }
}