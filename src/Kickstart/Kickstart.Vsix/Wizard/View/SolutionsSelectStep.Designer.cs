namespace Kickstart.Vsix
{
    partial class SolutionsSelectStep
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
            this._checkedListBoxTemplateSolutions = new System.Windows.Forms.CheckedListBox();
            this._buttonCheckAll = new System.Windows.Forms.Button();
            this._buttonUncheckAll = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // _checkedListBoxTemplateSolutions
            // 
            this._checkedListBoxTemplateSolutions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._checkedListBoxTemplateSolutions.FormattingEnabled = true;
            this._checkedListBoxTemplateSolutions.Location = new System.Drawing.Point(14, 14);
            this._checkedListBoxTemplateSolutions.Name = "_checkedListBoxTemplateSolutions";
            this._checkedListBoxTemplateSolutions.Size = new System.Drawing.Size(298, 289);
            this._checkedListBoxTemplateSolutions.TabIndex = 0;
            // 
            // _buttonCheckAll
            // 
            this._buttonCheckAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonCheckAll.Location = new System.Drawing.Point(318, 39);
            this._buttonCheckAll.Name = "_buttonCheckAll";
            this._buttonCheckAll.Size = new System.Drawing.Size(75, 23);
            this._buttonCheckAll.TabIndex = 1;
            this._buttonCheckAll.Text = "Check All";
            this._buttonCheckAll.UseVisualStyleBackColor = true;
            this._buttonCheckAll.Click += new System.EventHandler(this._buttonCheckAll_Click);
            // 
            // _buttonUncheckAll
            // 
            this._buttonUncheckAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonUncheckAll.Location = new System.Drawing.Point(318, 68);
            this._buttonUncheckAll.Name = "_buttonUncheckAll";
            this._buttonUncheckAll.Size = new System.Drawing.Size(75, 23);
            this._buttonUncheckAll.TabIndex = 2;
            this._buttonUncheckAll.Text = "Uncheck All";
            this._buttonUncheckAll.UseVisualStyleBackColor = true;
            this._buttonUncheckAll.Click += new System.EventHandler(this._buttonUncheckAll_Click);
            // 
            // SolutionsSelectStep
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._buttonUncheckAll);
            this.Controls.Add(this._buttonCheckAll);
            this.Controls.Add(this._checkedListBoxTemplateSolutions);
            this.Name = "SolutionsSelectStep";
            this.Size = new System.Drawing.Size(404, 335);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckedListBox _checkedListBoxTemplateSolutions;
        private System.Windows.Forms.Button _buttonCheckAll;
        private System.Windows.Forms.Button _buttonUncheckAll;
    }
}
