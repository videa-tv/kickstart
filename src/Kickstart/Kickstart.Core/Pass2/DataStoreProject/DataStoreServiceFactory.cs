using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kickstart.Pass2.SqlProject;
using Kickstart.Utility;

namespace Kickstart.Pass2.DataStoreProject
{
    public class DataStoreServiceFactory : IDataStoreServiceFactory
    {
        private readonly IContainer _container;
        public DataStoreServiceFactory(IContainer container)
        {
            _container = container;
        }
        public IDataStoreProjectService Create(DataStoreTypes dataStoreType)
        {
            switch (dataStoreType)
            {
                case DataStoreTypes.SqlServer:
                    return _container.GetInstance<ISqlServerProjectService>();
                case DataStoreTypes.MySql:
                case DataStoreTypes.Postgres:
                    return _container.GetInstance<IFlywayProjectService>();
                case DataStoreTypes.Kinesis:
                    return _container.GetInstance<IKinesisProjectService>();
                case DataStoreTypes.Tfs:
                    return null; //will build full data layer, later, if needed
                case DataStoreTypes.Okta:
                    return null;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
