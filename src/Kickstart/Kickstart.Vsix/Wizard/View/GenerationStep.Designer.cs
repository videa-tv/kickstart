namespace Kickstart.Vsix.Wizard
{
    partial class GenerationStep
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
            this._progressBar = new System.Windows.Forms.ProgressBar();
            this._progressMessage = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // _progressBar
            // 
            this._progressBar.Location = new System.Drawing.Point(100, 96);
            this._progressBar.Name = "_progressBar";
            this._progressBar.Size = new System.Drawing.Size(413, 23);
            this._progressBar.TabIndex = 0;
            // 
            // _progressMessage
            // 
            this._progressMessage.AutoSize = true;
            this._progressMessage.Location = new System.Drawing.Point(100, 77);
            this._progressMessage.Name = "_progressMessage";
            this._progressMessage.Size = new System.Drawing.Size(35, 13);
            this._progressMessage.TabIndex = 1;
            this._progressMessage.Text = "label1";
            // 
            // GenerationStep
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._progressMessage);
            this.Controls.Add(this._progressBar);
            this.Name = "GenerationStep";
            this.Size = new System.Drawing.Size(595, 276);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar _progressBar;
        private System.Windows.Forms.Label _progressMessage;
    }
}
