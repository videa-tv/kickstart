using System;
using System.Threading.Tasks;
using Kickstart.Pass0.Model;
using Kickstart.Pass1.KModel;
using Kickstart.Pass1.Service;
using Kickstart.Pass3;
using Kickstart.Utility;
using Kickstart.Vsix.Wizard.Service;
using Kickstart.Wizard.Presenter;
using Kickstart.Wizard.Service;
using Kickstart.Wizard.View;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Kickstart.App.Tests
{
    [TestClass]
    public class SmokeTest
    {
        [TestMethod]
        public void IsThereSmoke()
        {
            //Arrange
            var kickstartWizard = new KickstartWizardModel()
            {
                ProjectDirectory = @"c:\temp\",
                SolutionName = "SmokeTest",
                ProjectName = "SmokeTest"
            };

            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient<IKickstartService, KickstartService>();
            //kickstartWizard.GenerateAsync = true;

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var kickstartService = serviceProvider.GetRequiredService<IKickstartService>();

            var kickstartWizardService = serviceProvider.GetRequiredService<IKickstartWizardService>();
            var messageBoxMock = new Mock<IMessageBoxDisplayService>();
            //messageBoxMock.Setup(x => x.Show(It.IsAny<string>())).Verifiable();

            var menuMock = new MockMenuView();
            var menuPresenter = new MenuPresenter(kickstartWizard, menuMock);
            menuMock.MetadataSourceSelection = MetadataSource.Grpc;
            menuMock.DatabaseType = DataStoreTypes.Postgres;

            var projectViewMock = new Mock<IProjectView>();
            var kickstartWizardMock = new MockKickstartWizardView();// new Mock<IKickstartWizardView>();
            var protoFileView = new Mock<MockProtoFileView>();
            var databaseSqlView = new Mock<IDatabaseSqlView>();
            var generationView = new Mock<IGenerationView>();
            var protoToKProtoConverter = new Mock<IProtoToKProtoConverter>();
            var presenter  = new KickstartWizardPresenter(kickstartWizard, kickstartService, kickstartWizardService,  messageBoxMock.Object, kickstartWizardMock, menuMock, 
                projectViewMock.Object, protoFileView.Object, databaseSqlView.Object, generationView.Object, protoToKProtoConverter.Object);

            //Act

            menuMock.MetadataSourceSelectionChanged(null, null);
            menuMock.DatabaseTypeChanged(null, null);
            menuMock.CreateDataAccessLayerChanged(null, null);
            menuMock.CreateDatabaseProjectChanged(null, null);
            menuMock.CreateGrpcServiceChanged(null, null);
            menuMock.CreateGrpcServiceTestClientProjectChanged(null, null);
            menuMock.CreateGrpcUnitTestProjectChanged(null, null);
            menuMock.CreateIntegrationTestProjectChanged(null, null);

            kickstartWizardMock.NextClicked(null, null).Wait();
            protoFileView.Object.ProtoTextChanged(null, null);
            kickstartWizardMock.NextClicked(null,null).Wait();
            kickstartWizardMock.NextClicked(null,null).Wait();
            
            //Assert
        }
        [TestMethod]
        public void IsThereSmoke2()
        {
            //Arrange
            var kickstartWizard = new KickstartWizardModel()
            {
                ProjectDirectory = @"c:\temp\",
                SolutionName = "SmokeTest2",
                ProjectName = "SmokeTest2"
            };

            //kickstartWizard.GenerateAsync = true;
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient<IKickstartService, KickstartService>();
            serviceCollection.AddTransient<IKickstartWizardService, KickstartWizardService>();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var kickstartService = serviceProvider.GetRequiredService<IKickstartService>();
            var kickstartWizardService = serviceProvider.GetRequiredService<IKickstartWizardService>();
            var messageBoxMock = new Mock<IMessageBoxDisplayService>();
            //messageBoxMock.Setup(x => x.Show(It.IsAny<string>())).Verifiable();

            var menuMock = new MockMenuView();
            var menuPresenter = new MenuPresenter(kickstartWizard, menuMock);
            menuMock.MetadataSourceSelection = MetadataSource.Grpc;
            menuMock.DatabaseType = DataStoreTypes.SqlServer;

            var projectViewMock = new Mock<IProjectView>();
            var kickstartWizardMock = new MockKickstartWizardView();// new Mock<IKickstartWizardView>();
            var protoFileView = new Mock<MockProtoFileView>();
            var databaseSqlView = new Mock<IDatabaseSqlView>();
            var generationView = new Mock<IGenerationView>();
            var protoToKProtoConverter = new Mock<IProtoToKProtoConverter>();

            var presenter = new KickstartWizardPresenter(kickstartWizard, kickstartService, kickstartWizardService, messageBoxMock.Object, kickstartWizardMock, menuMock,
                projectViewMock.Object, protoFileView.Object, databaseSqlView.Object, generationView.Object, protoToKProtoConverter.Object);

            //Act

            menuMock.MetadataSourceSelectionChanged(null, null);
            menuMock.DatabaseTypeChanged(null, null);
            menuMock.CreateDataAccessLayerChanged(null, null);
            menuMock.CreateDatabaseProjectChanged(null, null);
            menuMock.CreateGrpcServiceChanged(null, null);
            menuMock.CreateGrpcServiceTestClientProjectChanged(null, null);
            menuMock.CreateGrpcUnitTestProjectChanged(null, null);
            menuMock.CreateIntegrationTestProjectChanged(null, null);

            kickstartWizardMock.NextClicked(null, null).Wait();
            protoFileView.Object.ProtoTextChanged(null, null);
            kickstartWizardMock.NextClicked(null, null).Wait();
            kickstartWizardMock.NextClicked(null, null).Wait();

            //Assert
        }

        private string GetTextProtoText()
        {
            throw new NotImplementedException();
            //return new AgencyKSolution().ProtoFileText;
        }
    }
    /*
    public class ConsoleRegistry : Registry
    {
        public ConsoleRegistry()
        {
            Scan(scanner =>
            {
                scanner.TheCallingAssembly();
                scanner.AssemblyContainingType<IKickstartWizardService>();
                scanner.WithDefaultConventions();
            });
            // requires explicit registration; doesn't follow convention
            //For<ILog>().Use<ConsoleLogger>();
        }
    }*/
    public class MockKickstartWizardView : IKickstartWizardView
    {
        public event EventHandler Load;
        public event EventHandler PreviousStep;

        bool _nextEnabled = true;

        public Func<object, EventArgs, Task> NextClicked { get; set; }
        public IKickstartWizardPresenter Tag { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IProjectView ProjectView => throw new NotImplementedException();

        public void AddView(IView menuView)
        {
             
        }

        public void CloseWizard()
        {
             
        }

        public void DisableNext()
        {
            _nextEnabled = false;
        }

        public void DisablePrevious()
        {
             
        }

        public void EnableFinish()
        {
            _nextEnabled = true;
        }

        public void EnableNext()
        {
            _nextEnabled = true;
        }

        public void EnableGenerate()
        {
            throw new NotImplementedException();
        }

        public int ShowDialog(IntPtr handle)
        {
            throw new NotImplementedException();
        }
    }

    public class MockMenuView : IMenuView
    {
        public bool CreateDatabaseProject { get; set; } = true;

        public bool CreateDataLayerProject { get; set; } = true;

        public bool CreateGrpcServiceProject { get; set; } = true;

        public bool CreateGrpcServiceTestClientProject { get; set; } = true;

        public bool CreateGrpcUnitTestProject { get; set; } = true;

        public bool CreateIntegrationTestProject { get; set; } = true;

        public DataStoreTypes DatabaseType { get; set; }

    public MetadataSource MetadataSourceSelection { get; set; }
        public bool Visible { get; set; }
    public Func<object, EventArgs, Task> MetadataSourceSelectionChanged
        {
            get;set;
        }
        public Func<object, EventArgs, Task> DatabaseTypeChanged
        {
            get; set;
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

        public bool CreateWebAppProject => throw new NotImplementedException();

        public Func<object, EventArgs, Task> CreateWebAppProjectChanged { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }

    public class MockProtoFileView : IProtoFileView
    {
        public string ProtoFileText { get
            {

                throw new NotImplementedException();
                //return new AgencyKSolution().ProtoFileText;
            }
        }

        public bool Visible { get; set; }
        
        public Func<object, EventArgs, Task> ProtoTextChanged { get; set; }

        public void Bind(KProtoFile pf3)
        {
            throw new NotImplementedException();
        }
    }
}
