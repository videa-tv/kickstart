using Kickstart.Wizard.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kickstart.Pass0.Model;

namespace Kickstart.Wizard.Presenter
{
    public class MenuPresenter
    {
        KickstartWizardModel _kickstartWizardModel;
        IMenuView _menuView;
        public MenuPresenter(KickstartWizardModel kickstartWizardModel,  IMenuView menuView)
        {
            _kickstartWizardModel = kickstartWizardModel;
            _menuView = menuView;

            _menuView.MetadataSourceSelectionChanged += _menuView_MetadataSourceSelectionChanged;
            _menuView.DatabaseTypeChanged += _menuView_DatabaseTypeChanged;

            _menuView.CreateDatabaseProjectChanged += _menuView_CreateDatabaseProjectChanged;
            _menuView.CreateDataAccessLayerChanged += _menuView_CreateDataAccessLayerChanged;
            _menuView.CreateGrpcServiceChanged += _menuView_CreateGrpcProjectChanged;
            _menuView.CreateGrpcServiceTestClientProjectChanged += (a,b ) => 
            {
                _kickstartWizardModel.CreateGrpcServiceTestClientProject = _menuView.CreateGrpcServiceTestClientProject;
                return Task.CompletedTask;
            };

            _menuView.CreateGrpcUnitTestProjectChanged += (a, b) =>
            {
                _kickstartWizardModel.CreateGrpcUnitTestProject = _menuView.CreateGrpcUnitTestProject;
                return Task.CompletedTask;
            };

            _menuView.CreateIntegrationTestProjectChanged += (a, b) =>
            {
                _kickstartWizardModel.CreateIntegrationTestProject = _menuView.CreateIntegrationTestProject;
                return Task.CompletedTask;
            };

            _menuView.CreateWebAppProjectChanged += (a, b) =>
            {
                _kickstartWizardModel.CreateWebAppProject = _menuView.CreateWebAppProject;
                return Task.CompletedTask;
            };



        }

        private Task _menuView_DatabaseTypeChanged(object sender, EventArgs e)
        {
            _kickstartWizardModel.DatabaseType = _menuView.DatabaseType;
            return Task.CompletedTask;
        }

        private Task _menuView_CreateDatabaseProjectChanged(object sender, EventArgs e)
        {
            _kickstartWizardModel.CreateDatabaseProject = _menuView.CreateDatabaseProject;

            return Task.CompletedTask;
        }
        private Task _menuView_CreateDataAccessLayerChanged(object sender, EventArgs e)
        {
            _kickstartWizardModel.CreateDataLayerProject = _menuView.CreateDataLayerProject;

            return Task.CompletedTask;
        }

        private Task _menuView_CreateGrpcProjectChanged(object sender, EventArgs e)
        {
            _kickstartWizardModel.CreateGrpcServiceProject = _menuView.CreateGrpcServiceProject;

            return Task.CompletedTask;
        }


        private Task _menuView_MetadataSourceSelectionChanged(object sender, EventArgs e)
       {
            _kickstartWizardModel.MetadataSource = _menuView.MetadataSourceSelection;
            return Task.CompletedTask;
        }
    }
}
