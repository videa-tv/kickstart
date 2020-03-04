using Kickstart.Utility;
using System;
using System.Threading.Tasks;

namespace Kickstart.Wizard.View
{
    public interface IMenuView : IView
    {
        bool CreateDatabaseProject { get; }
        bool CreateDataLayerProject { get; }
        bool CreateGrpcServiceProject { get; }
        bool CreateGrpcServiceTestClientProject { get; }
        bool CreateGrpcUnitTestProject { get; }
        bool CreateIntegrationTestProject { get; }
        bool CreateWebAppProject { get; }

        DataStoreTypes DatabaseType { get; }
        MetadataSource MetadataSourceSelection { get; set; }
        bool Visible { get; set; }

        Func<Object, EventArgs, Task> MetadataSourceSelectionChanged { get; set; }
        Func<Object, EventArgs, Task> DatabaseTypeChanged { get; set; }
        Func<Object, EventArgs, Task> CreateDatabaseProjectChanged { get; set; }
        Func<Object, EventArgs, Task> CreateDataAccessLayerChanged { get; set; }
        Func<Object, EventArgs, Task> CreateGrpcServiceChanged { get; set; }
        Func<Object, EventArgs, Task> CreateGrpcServiceTestClientProjectChanged { get; set; }
        Func<Object, EventArgs, Task> CreateGrpcUnitTestProjectChanged { get; set; }
        Func<Object, EventArgs, Task> CreateIntegrationTestProjectChanged { get; set; }

        Func<Object, EventArgs, Task> CreateWebAppProjectChanged { get; set; }

    }
}