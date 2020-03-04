using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kickstart.Pass2.DataStoreProject.SqlServer;
using Kickstart.Pass2.SqlProject;
using Kickstart.Utility;

namespace Kickstart.Pass2.DataStoreProject
{
    public interface IDataStoreTestServiceFactory
    {
        IDataStoreTestProjectService Create(DataStoreTypes dataStoreType);
    }

    public class DataStoreTestServiceFactory : IDataStoreTestServiceFactory
    {
        private readonly IContainer _container;
        public DataStoreTestServiceFactory(IContainer container)
        {
            _container = container;
        }
        public IDataStoreTestProjectService Create(DataStoreTypes dataStoreType)
        {
            switch (dataStoreType)
            {
                case DataStoreTypes.SqlServer:
                    return _container.GetInstance<ISqlServerTestProjectService>();

                case DataStoreTypes.Postgres:
                    return null;
                case DataStoreTypes.Kinesis:
                    return null;
                case DataStoreTypes.Tfs:
                    return null;
                case DataStoreTypes.Okta:
                    return null;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
