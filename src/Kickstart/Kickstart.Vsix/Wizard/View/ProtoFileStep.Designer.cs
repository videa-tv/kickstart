namespace Kickstart.Vsix.Wizard
{
    partial class ProtoFileStep
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
            this._textBoxProtoFileText = new System.Windows.Forms.TextBox();
            this._buttonLoadSample = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this._loadSampleProto3 = new System.Windows.Forms.Button();
            this._buttonLoadSampleProto4 = new System.Windows.Forms.Button();
            this._buttonLoadSampleProto5 = new System.Windows.Forms.Button();
            this._buttonSampleKickstart = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // _textBoxProtoFileText
            // 
            this._textBoxProtoFileText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._textBoxProtoFileText.Location = new System.Drawing.Point(3, 34);
            this._textBoxProtoFileText.Multiline = true;
            this._textBoxProtoFileText.Name = "_textBoxProtoFileText";
            this._textBoxProtoFileText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._textBoxProtoFileText.Size = new System.Drawing.Size(692, 431);
            this._textBoxProtoFileText.TabIndex = 0;
            this._textBoxProtoFileText.TextChanged += new System.EventHandler(this._textBoxProtoFileText_TextChanged);
            // 
            // _buttonLoadSample
            // 
            this._buttonLoadSample.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonLoadSample.Location = new System.Drawing.Point(701, 76);
            this._buttonLoadSample.Name = "_buttonLoadSample";
            this._buttonLoadSample.Size = new System.Drawing.Size(235, 23);
            this._buttonLoadSample.TabIndex = 1;
            this._buttonLoadSample.Text = "Load Sample Proto File (Feature Flag)";
            this._buttonLoadSample.UseVisualStyleBackColor = true;
            this._buttonLoadSample.Click += new System.EventHandler(this._buttonLoadSample_Click);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(701, 105);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(235, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "Load Sample Proto File (Station Properties)";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // _loadSampleProto3
            // 
            this._loadSampleProto3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._loadSampleProto3.Location = new System.Drawing.Point(701, 134);
            this._loadSampleProto3.Name = "_loadSampleProto3";
            this._loadSampleProto3.Size = new System.Drawing.Size(232, 23);
            this._loadSampleProto3.TabIndex = 3;
            this._loadSampleProto3.Text = "Load Sample Proto File (Demo)";
            this._loadSampleProto3.UseVisualStyleBackColor = true;
            this._loadSampleProto3.Click += new System.EventHandler(this._loadSampleProto3_Click);
            // 
            // _buttonLoadSampleProto4
            // 
            this._buttonLoadSampleProto4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonLoadSampleProto4.Location = new System.Drawing.Point(701, 163);
            this._buttonLoadSampleProto4.Name = "_buttonLoadSampleProto4";
            this._buttonLoadSampleProto4.Size = new System.Drawing.Size(232, 23);
            this._buttonLoadSampleProto4.TabIndex = 4;
            this._buttonLoadSampleProto4.Text = "Load Sample Proto File (Razor)";
            this._buttonLoadSampleProto4.UseVisualStyleBackColor = true;
            this._buttonLoadSampleProto4.Click += new System.EventHandler(this._buttonLoadSampleProto4_Click);
            // 
            // _buttonLoadSampleProto5
            // 
            this._buttonLoadSampleProto5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonLoadSampleProto5.Location = new System.Drawing.Point(701, 192);
            this._buttonLoadSampleProto5.Name = "_buttonLoadSampleProto5";
            this._buttonLoadSampleProto5.Size = new System.Drawing.Size(232, 23);
            this._buttonLoadSampleProto5.TabIndex = 5;
            this._buttonLoadSampleProto5.Text = "Load Sample Proto File (Agency)";
            this._buttonLoadSampleProto5.UseVisualStyleBackColor = true;
            this._buttonLoadSampleProto5.Click += new System.EventHandler(this._buttonLoadSampleProto5_Click);
            // 
            // _buttonSampleKickstart
            // 
            this._buttonSampleKickstart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonSampleKickstart.Location = new System.Drawing.Point(701, 221);
            this._buttonSampleKickstart.Name = "_buttonSampleKickstart";
            this._buttonSampleKickstart.Size = new System.Drawing.Size(232, 23);
            this._buttonSampleKickstart.TabIndex = 6;
            this._buttonSampleKickstart.Text = "Load Sample Proto File (Kickstart)";
            this._buttonSampleKickstart.UseVisualStyleBackColor = true;
            this._buttonSampleKickstart.Click += new System.EventHandler(this._buttonSampleKickstart_Click);
            // 
            // ProtoFileStep
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._buttonSampleKickstart);
            this.Controls.Add(this._buttonLoadSampleProto5);
            this.Controls.Add(this._buttonLoadSampleProto4);
            this.Controls.Add(this._loadSampleProto3);
            this.Controls.Add(this.button1);
            this.Controls.Add(this._buttonLoadSample);
            this.Controls.Add(this._textBoxProtoFileText);
            this.Name = "ProtoFileStep";
            this.Size = new System.Drawing.Size(936, 464);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox _textBoxProtoFileText;
        private System.Windows.Forms.Button _buttonLoadSample;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button _loadSampleProto3;
        private System.Windows.Forms.Button _buttonLoadSampleProto4;
        private System.Windows.Forms.Button _buttonLoadSampleProto5;
        private System.Windows.Forms.Button _buttonSampleKickstart;
    }
}
