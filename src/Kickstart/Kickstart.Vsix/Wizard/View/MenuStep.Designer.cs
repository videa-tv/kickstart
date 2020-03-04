namespace Kickstart.Vsix.Wizard
{
    partial class MenuStep
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
            this._radioButtonMultiProject = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this._checkBoxCreateDataAccessLayer = new System.Windows.Forms.CheckBox();
            this._checkBoxCreateDatabaseProject = new System.Windows.Forms.CheckBox();
            this._comboBoxDatabaseType = new System.Windows.Forms.ComboBox();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this._checkBoxGrpcService = new System.Windows.Forms.CheckBox();
            this._radioButtonProtoFileMetadata = new System.Windows.Forms.RadioButton();
            this._radioButtonDatabaseMetadata = new System.Windows.Forms.RadioButton();
            this._radioButtonSqlScriptMetadata = new System.Windows.Forms.RadioButton();
            this._checkBoxGrpcTestClient = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this._radioButtonNoMetadata = new System.Windows.Forms.RadioButton();
            this._checkBoxGrpcUnitTestProject = new System.Windows.Forms.CheckBox();
            this._checkBoxIntegrationTest = new System.Windows.Forms.CheckBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this._checkBoxWebApp = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // _radioButtonMultiProject
            // 
            this._radioButtonMultiProject.AutoSize = true;
            this._radioButtonMultiProject.Enabled = false;
            this._radioButtonMultiProject.Location = new System.Drawing.Point(199, 322);
            this._radioButtonMultiProject.Name = "_radioButtonMultiProject";
            this._radioButtonMultiProject.Size = new System.Drawing.Size(141, 17);
            this._radioButtonMultiProject.TabIndex = 5;
            this._radioButtonMultiProject.Text = "Multi-Project (Advanced)";
            this._radioButtonMultiProject.UseVisualStyleBackColor = true;
            this._radioButtonMultiProject.Visible = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(344, 66);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(108, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Select Project Types:";
            // 
            // _checkBoxCreateDataAccessLayer
            // 
            this._checkBoxCreateDataAccessLayer.AutoSize = true;
            this._checkBoxCreateDataAccessLayer.Location = new System.Drawing.Point(364, 120);
            this._checkBoxCreateDataAccessLayer.Name = "_checkBoxCreateDataAccessLayer";
            this._checkBoxCreateDataAccessLayer.Size = new System.Drawing.Size(160, 17);
            this._checkBoxCreateDataAccessLayer.TabIndex = 11;
            this._checkBoxCreateDataAccessLayer.Text = "Data Access Layer (Dapper)";
            this._checkBoxCreateDataAccessLayer.UseVisualStyleBackColor = true;
            this._checkBoxCreateDataAccessLayer.CheckedChanged += new System.EventHandler(this._checkBoxCreateDataAccessLayer_CheckedChanged);
            // 
            // _checkBoxCreateDatabaseProject
            // 
            this._checkBoxCreateDatabaseProject.AutoSize = true;
            this._checkBoxCreateDatabaseProject.Location = new System.Drawing.Point(364, 97);
            this._checkBoxCreateDatabaseProject.Name = "_checkBoxCreateDatabaseProject";
            this._checkBoxCreateDatabaseProject.Size = new System.Drawing.Size(72, 17);
            this._checkBoxCreateDatabaseProject.TabIndex = 12;
            this._checkBoxCreateDatabaseProject.Text = "Database";
            this._checkBoxCreateDatabaseProject.UseVisualStyleBackColor = true;
            this._checkBoxCreateDatabaseProject.CheckedChanged += new System.EventHandler(this._checkBoxCreateDatabaseProject_CheckedChanged);
            // 
            // _comboBoxDatabaseType
            // 
            this._comboBoxDatabaseType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._comboBoxDatabaseType.FormattingEnabled = true;
            this._comboBoxDatabaseType.Items.AddRange(new object[] {
            "Postgres-AWS Aurora",
            "SqlServer2014",
            "MySql-AWS Aurora"});
            this._comboBoxDatabaseType.Location = new System.Drawing.Point(447, 93);
            this._comboBoxDatabaseType.Name = "_comboBoxDatabaseType";
            this._comboBoxDatabaseType.Size = new System.Drawing.Size(121, 21);
            this._comboBoxDatabaseType.TabIndex = 13;
            this._comboBoxDatabaseType.SelectedIndexChanged += new System.EventHandler(this._comboBoxDatabaseType_SelectedIndexChanged);
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Location = new System.Drawing.Point(510, 323);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(143, 17);
            this.radioButton1.TabIndex = 14;
            this.radioButton1.Text = "PowerBI Data Connector";
            this.radioButton1.UseVisualStyleBackColor = true;
            this.radioButton1.Visible = false;
            // 
            // _checkBoxGrpcService
            // 
            this._checkBoxGrpcService.AutoSize = true;
            this._checkBoxGrpcService.Location = new System.Drawing.Point(364, 143);
            this._checkBoxGrpcService.Name = "_checkBoxGrpcService";
            this._checkBoxGrpcService.Size = new System.Drawing.Size(88, 17);
            this._checkBoxGrpcService.TabIndex = 15;
            this._checkBoxGrpcService.Text = "Grpc Service";
            this._checkBoxGrpcService.UseVisualStyleBackColor = true;
            this._checkBoxGrpcService.CheckedChanged += new System.EventHandler(this._checkBoxGrpcService_CheckedChanged);
            // 
            // _radioButtonProtoFileMetadata
            // 
            this._radioButtonProtoFileMetadata.AutoSize = true;
            this._radioButtonProtoFileMetadata.Checked = true;
            this._radioButtonProtoFileMetadata.Location = new System.Drawing.Point(73, 96);
            this._radioButtonProtoFileMetadata.Name = "_radioButtonProtoFileMetadata";
            this._radioButtonProtoFileMetadata.Size = new System.Drawing.Size(69, 17);
            this._radioButtonProtoFileMetadata.TabIndex = 16;
            this._radioButtonProtoFileMetadata.TabStop = true;
            this._radioButtonProtoFileMetadata.Text = "Proto File";
            this._radioButtonProtoFileMetadata.UseVisualStyleBackColor = true;
            this._radioButtonProtoFileMetadata.CheckedChanged += new System.EventHandler(this._radioButtonProtoFileMetadata_CheckedChanged);
            // 
            // _radioButtonDatabaseMetadata
            // 
            this._radioButtonDatabaseMetadata.AutoSize = true;
            this._radioButtonDatabaseMetadata.Enabled = false;
            this._radioButtonDatabaseMetadata.Location = new System.Drawing.Point(73, 142);
            this._radioButtonDatabaseMetadata.Name = "_radioButtonDatabaseMetadata";
            this._radioButtonDatabaseMetadata.Size = new System.Drawing.Size(71, 17);
            this._radioButtonDatabaseMetadata.TabIndex = 18;
            this._radioButtonDatabaseMetadata.Text = "Database";
            this._radioButtonDatabaseMetadata.UseVisualStyleBackColor = true;
            this._radioButtonDatabaseMetadata.CheckedChanged += new System.EventHandler(this._radioButtonDatabaseMetadata_CheckedChanged);
            // 
            // _radioButtonSqlScriptMetadata
            // 
            this._radioButtonSqlScriptMetadata.AutoSize = true;
            this._radioButtonSqlScriptMetadata.Location = new System.Drawing.Point(73, 119);
            this._radioButtonSqlScriptMetadata.Name = "_radioButtonSqlScriptMetadata";
            this._radioButtonSqlScriptMetadata.Size = new System.Drawing.Size(75, 17);
            this._radioButtonSqlScriptMetadata.TabIndex = 20;
            this._radioButtonSqlScriptMetadata.Text = "Sql Scripts";
            this._radioButtonSqlScriptMetadata.UseVisualStyleBackColor = true;
            this._radioButtonSqlScriptMetadata.CheckedChanged += new System.EventHandler(this._radioButtonSqlScriptMetadata_CheckedChanged);
            // 
            // _checkBoxGrpcTestClient
            // 
            this._checkBoxGrpcTestClient.AutoSize = true;
            this._checkBoxGrpcTestClient.Location = new System.Drawing.Point(364, 193);
            this._checkBoxGrpcTestClient.Name = "_checkBoxGrpcTestClient";
            this._checkBoxGrpcTestClient.Size = new System.Drawing.Size(102, 17);
            this._checkBoxGrpcTestClient.TabIndex = 21;
            this._checkBoxGrpcTestClient.Text = "Grpc Test Client";
            this._checkBoxGrpcTestClient.UseVisualStyleBackColor = true;
            this._checkBoxGrpcTestClient.CheckedChanged += new System.EventHandler(this._checkBoxGrpcTestClient_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(52, 75);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(92, 13);
            this.label2.TabIndex = 22;
            this.label2.Text = "Metadata Source:";
            // 
            // _radioButtonNoMetadata
            // 
            this._radioButtonNoMetadata.AutoSize = true;
            this._radioButtonNoMetadata.Location = new System.Drawing.Point(73, 165);
            this._radioButtonNoMetadata.Name = "_radioButtonNoMetadata";
            this._radioButtonNoMetadata.Size = new System.Drawing.Size(51, 17);
            this._radioButtonNoMetadata.TabIndex = 23;
            this._radioButtonNoMetadata.Text = "None";
            this._radioButtonNoMetadata.UseVisualStyleBackColor = true;
            this._radioButtonNoMetadata.CheckedChanged += new System.EventHandler(this._radioButtonNoMetadata_CheckedChanged);
            // 
            // _checkBoxGrpcUnitTestProject
            // 
            this._checkBoxGrpcUnitTestProject.AutoSize = true;
            this._checkBoxGrpcUnitTestProject.Location = new System.Drawing.Point(364, 216);
            this._checkBoxGrpcUnitTestProject.Name = "_checkBoxGrpcUnitTestProject";
            this._checkBoxGrpcUnitTestProject.Size = new System.Drawing.Size(95, 17);
            this._checkBoxGrpcUnitTestProject.TabIndex = 24;
            this._checkBoxGrpcUnitTestProject.Text = "Grpc Unit Test";
            this._checkBoxGrpcUnitTestProject.UseVisualStyleBackColor = true;
            this._checkBoxGrpcUnitTestProject.CheckedChanged += new System.EventHandler(this._checkBoxGrpcUnitTestProject_CheckedChanged);
            // 
            // _checkBoxIntegrationTest
            // 
            this._checkBoxIntegrationTest.AutoSize = true;
            this._checkBoxIntegrationTest.Enabled = false;
            this._checkBoxIntegrationTest.Location = new System.Drawing.Point(364, 239);
            this._checkBoxIntegrationTest.Name = "_checkBoxIntegrationTest";
            this._checkBoxIntegrationTest.Size = new System.Drawing.Size(100, 17);
            this._checkBoxIntegrationTest.TabIndex = 25;
            this._checkBoxIntegrationTest.Text = "Integration Test";
            this._checkBoxIntegrationTest.UseVisualStyleBackColor = true;
            this._checkBoxIntegrationTest.CheckedChanged += new System.EventHandler(this._checkBoxIntegrationTest_CheckedChanged);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Enabled = false;
            this.checkBox1.Location = new System.Drawing.Point(364, 275);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(88, 17);
            this.checkBox1.TabIndex = 26;
            this.checkBox1.Text = "Configuration";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // _checkBoxWebApp
            // 
            this._checkBoxWebApp.AutoSize = true;
            this._checkBoxWebApp.Location = new System.Drawing.Point(364, 166);
            this._checkBoxWebApp.Name = "_checkBoxWebApp";
            this._checkBoxWebApp.Size = new System.Drawing.Size(201, 17);
            this._checkBoxWebApp.TabIndex = 27;
            this._checkBoxWebApp.Text = "Web App (React) --EXPERIMENTAL";
            this._checkBoxWebApp.UseVisualStyleBackColor = true;
            this._checkBoxWebApp.CheckedChanged += new System.EventHandler(this._checkBoxWebApp_CheckedChanged);
            // 
            // MenuStep
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._checkBoxWebApp);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this._checkBoxIntegrationTest);
            this.Controls.Add(this._checkBoxGrpcUnitTestProject);
            this.Controls.Add(this._radioButtonNoMetadata);
            this.Controls.Add(this.label2);
            this.Controls.Add(this._checkBoxGrpcTestClient);
            this.Controls.Add(this._radioButtonSqlScriptMetadata);
            this.Controls.Add(this._radioButtonDatabaseMetadata);
            this.Controls.Add(this._radioButtonProtoFileMetadata);
            this.Controls.Add(this._checkBoxGrpcService);
            this.Controls.Add(this.radioButton1);
            this.Controls.Add(this._comboBoxDatabaseType);
            this.Controls.Add(this._checkBoxCreateDatabaseProject);
            this.Controls.Add(this._checkBoxCreateDataAccessLayer);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._radioButtonMultiProject);
            this.Name = "MenuStep";
            this.Size = new System.Drawing.Size(673, 354);
            this.Load += new System.EventHandler(this.MenuStep_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.RadioButton _radioButtonMultiProject;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox _checkBoxCreateDataAccessLayer;
        private System.Windows.Forms.CheckBox _checkBoxCreateDatabaseProject;
        private System.Windows.Forms.ComboBox _comboBoxDatabaseType;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.CheckBox _checkBoxGrpcService;
        private System.Windows.Forms.RadioButton _radioButtonProtoFileMetadata;
        private System.Windows.Forms.RadioButton _radioButtonDatabaseMetadata;
        private System.Windows.Forms.RadioButton _radioButtonSqlScriptMetadata;
        private System.Windows.Forms.CheckBox _checkBoxGrpcTestClient;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RadioButton _radioButtonNoMetadata;
        private System.Windows.Forms.CheckBox _checkBoxGrpcUnitTestProject;
        private System.Windows.Forms.CheckBox _checkBoxIntegrationTest;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.CheckBox _checkBoxWebApp;
    }
}
