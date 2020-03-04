using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Kickstart.Utility;
using Kickstart.Wizard.View;

namespace Kickstart.Vsix.Wizard
{
    public partial class MenuStep : UserControl, IMenuView
    {
     

        public MetadataSource MetadataSourceSelection
        {
            get
            {
                if (_radioButtonProtoFileMetadata.Checked)
                    return MetadataSource.Grpc;
                else if (_radioButtonSqlScriptMetadata.Checked)
                    return MetadataSource.SqlScripts;
                else if (_radioButtonDatabaseMetadata.Checked)
                    return MetadataSource.Database;
                else if (_radioButtonNoMetadata.Checked)
                    return MetadataSource.None;
                else
                    throw new NotImplementedException();
            }
            set
            {

            }
        }

        public bool CreateGrpcServiceProject
        {
            get
            {
                return _checkBoxGrpcService.Checked;
            }
        }
        public bool CreateDataLayerProject
        {
            get
            {
                return _checkBoxCreateDataAccessLayer.Checked;
            }
        }
        public bool CreateDatabaseProject 
        {
                get
                {
                return _checkBoxCreateDatabaseProject.Checked;
                }
        }
        public bool CreateGrpcUnitTestProject
        {
            get
            {
                return _checkBoxGrpcUnitTestProject.Checked;
            }
        }


        public DataStoreTypes DatabaseType
        {
            get
            {
                if (_comboBoxDatabaseType.SelectedIndex == 0)
                {
                    return DataStoreTypes.Postgres;
                }
                else if (_comboBoxDatabaseType.SelectedIndex == 1)
                {
                    return DataStoreTypes.SqlServer;
                }
                else if (_comboBoxDatabaseType.SelectedIndex == 2)
                {
                    return DataStoreTypes.MySql;
                }
                else
                    throw new NotImplementedException();
            }
        }

        public bool CreateGrpcServiceTestClientProject
        {
            get { return _checkBoxGrpcTestClient.Checked;
            }
        }
        public bool CreateIntegrationTestProject
        {
            get
            {
                return _checkBoxIntegrationTest.Checked;
            }
        }
        public bool CreateWebAppProject
        {
            get
            {
                return _checkBoxWebApp.Checked;
            }
        }

       
        public Func<object, EventArgs, Task> MetadataSourceSelectionChanged
        {
            get;set; }

        public Func<object, EventArgs, Task> DatabaseTypeChanged
        {
            get;set;
        }

        public Func<object, EventArgs, Task> CreateDatabaseProjectChanged
        {
            get; set;
        }
        public Func<object, EventArgs, Task> CreateDataAccessLayerChanged
        {
            get; set;
        }
        public Func<object, EventArgs, Task> CreateGrpcServiceChanged
        {
            get; set;
        }
        public Func<object, EventArgs, Task> CreateGrpcServiceTestClientProjectChanged { get; set; }
        public Func<object, EventArgs, Task> CreateGrpcUnitTestProjectChanged { get; set; }
        public Func<object, EventArgs, Task> CreateIntegrationTestProjectChanged { get; set; }
        public Func<object, EventArgs, Task> CreateWebAppProjectChanged { get; set; }
        public MenuStep()
        {
            InitializeComponent();
            this._comboBoxDatabaseType.SelectedIndex = 0;

        }

        
        private void _checkBoxCreateDatabaseProject_CheckedChanged(object sender, EventArgs e)
        {
            FireCreateDatabaseProjectChanged();
          
        }

        private void FireCreateDatabaseProjectChanged()
        {
            if (CreateDatabaseProjectChanged != null)
            {
                CreateDatabaseProjectChanged(this, null);
            }
        }

        private void _radioButtonProtoFileMetadata_CheckedChanged(object sender, EventArgs e)
        {
            FireMetaDataSourceSelectionChanged();
        }

        private void _radioButtonSqlScriptMetadata_CheckedChanged(object sender, EventArgs e)
        {
            FireMetaDataSourceSelectionChanged();
        }

        private void _radioButtonDatabaseMetadata_CheckedChanged(object sender, EventArgs e)
        {
            FireMetaDataSourceSelectionChanged();
        }

        private void _radioButtonNoMetadata_CheckedChanged(object sender, EventArgs e)
        {
            FireMetaDataSourceSelectionChanged();
        }

        private void FireMetaDataSourceSelectionChanged()
        {
            MetadataSourceSelectionChanged?.Invoke(null, null);
        }

        private void _comboBoxDatabaseType_SelectedIndexChanged(object sender, EventArgs e)
        {
            FireDatabaseTypeChanged();
        }

        private void FireDatabaseTypeChanged()
        {

            DatabaseTypeChanged?.Invoke(this, null);
        }

        private void MenuStep_Load(object sender, EventArgs e)
        {
            FireMetaDataSourceSelectionChanged();
            FireCreateDatabaseProjectChanged();
            FireDatabaseTypeChanged();
            FireCreateDataAccessLayerChanged();
            FireCreateGrpcServiceChanged();
            FireCreateDatabaseProjectChanged();
            FireCreateWebAppChanged();
        }

        private void _checkBoxCreateDataAccessLayer_CheckedChanged(object sender, EventArgs e)
        {
            FireCreateDataAccessLayerChanged();
        }

        private void FireCreateDataAccessLayerChanged()
        {
            if (CreateDataAccessLayerChanged !=null )
            {
                CreateDataAccessLayerChanged(this, null);
            }
        }

        private void _checkBoxGrpcService_CheckedChanged(object sender, EventArgs e)
        {
            FireCreateGrpcServiceChanged();
        }

        private void FireCreateGrpcServiceChanged()
        {
            if (CreateGrpcServiceChanged != null)
            {
                CreateGrpcServiceChanged(this, null);
            }
        }
        private void FireCreateWebAppChanged()
        {
            if (CreateWebAppProjectChanged != null)
            {
                CreateWebAppProjectChanged(this, null);
            }
        }
        private void _checkBoxGrpcTestClient_CheckedChanged(object sender, EventArgs e)
        {
            CreateGrpcServiceTestClientProjectChanged?.Invoke(null, null);
        }

        private void _checkBoxGrpcUnitTestProject_CheckedChanged(object sender, EventArgs e)
        {
            CreateGrpcUnitTestProjectChanged?.Invoke(null, null);
        }

        private void _checkBoxIntegrationTest_CheckedChanged(object sender, EventArgs e)
        {
            CreateIntegrationTestProjectChanged?.Invoke(null, null);
        }

        private void _checkBoxWebApp_CheckedChanged(object sender, EventArgs e)
        {
            CreateWebAppProjectChanged?.Invoke(null, null);
        }
    }
}
