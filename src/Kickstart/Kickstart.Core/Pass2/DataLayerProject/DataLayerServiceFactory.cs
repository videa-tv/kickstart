using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kickstart.Utility;

namespace Kickstart.Pass2.DataLayerProject
{
    public class DataLayerServiceFactory : IDataLayerServiceFactory
    {
        private readonly IContainer _container;
        public DataLayerServiceFactory(IContainer container)
        {
            _container = container;
        }
        public IDataLayerServiceProjectService Create(DataStoreTypes dataStoreType)
        {
            switch (dataStoreType)
            {
                case DataStoreTypes.Kinesis:
                case DataStoreTypes.Kafka:
                    {
                        var s = _container.GetInstance<StreamDataLayerServiceProjectService>();
                        s.ConnectsToDatabaseType = dataStoreType;
                        return s;
                    }

                case DataStoreTypes.SqlServer:
                case DataStoreTypes.Postgres:
                    {
                        var service = _container.GetInstance<RelationalDataLayerServiceProjectService>();
                        service.ConnectsToDatabaseType = dataStoreType;
                        return service;
                    }
                case DataStoreTypes.GenericResource:
                case DataStoreTypes.Tfs:
                case DataStoreTypes.Okta:
                    {
                        var s = _container.GetInstance<GenericResourceDataLayerServiceProjectService>();
                        s.ConnectsToDatabaseType = dataStoreType;
                        return s;
                    }
                default:
                    throw new NotImplementedException();
            }
           
        }
    }
}
