using Kickstart.Pass2.CModel.DataStore;

namespace Kickstart.Pass1.KModel
{
    public class KStoredProcedure : KPart
    {
        public string Schema { get; set; }
        public string StoredProcedureName { get; set; }
        public string StoredProcedureDescription { get; set; }

        public string StoredProcedureText { get; set; }
        public string ParameterSetName { get; set; }
        public string ResultSetName { get; set; }

        public bool ReturnsMultipleRows { get; set; }

        //public bool KickstartApi { get; set; } = false;
        public CStoredProcedure GeneratedStoredProcedure { get; set; }

        public KStoredProcedure()
        {
            int x = 1;
        }
    }
}