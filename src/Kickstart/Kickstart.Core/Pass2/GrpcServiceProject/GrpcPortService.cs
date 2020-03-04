using System.Collections.Generic;

namespace Kickstart.Pass2.GrpcServiceProject
{
    public class GrpcPortService : IGrpcPortService
    {
        private readonly Dictionary<string, int> _portMap = new Dictionary<string, int>();
        private int _nextPort = 50095;

        public int GeneratePortNumber(string serviceName)
        {
            if (serviceName == null)
                serviceName = "null";

            if (_portMap.ContainsKey(serviceName))
                return _portMap[serviceName];
            var assignedPort = _nextPort;
            _portMap.Add(serviceName, assignedPort);
            _nextPort++;
            return assignedPort;
        }
    }
}