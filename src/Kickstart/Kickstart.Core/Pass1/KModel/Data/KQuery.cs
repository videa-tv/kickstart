using Kickstart.Pass2.CModel.DataStore;

namespace Kickstart.Pass1.KModel
{
    public class KQuery     {
        
        public string QueryName { get; set; }
        public string ParameterSetName { get; set; }
        public string ResultSetName { get; set; }

        public string QueryBody { get; set; }

        public CQuery GeneratedQuery { get; set; }

    }
}