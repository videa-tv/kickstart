using Kickstart.Pass2.CModel.DataStore;

namespace Kickstart.Interface
{
    public interface ICStoredProcedureToStoredProcedureConverter
    {
        string Convert(CStoredProcedure storedProcedure);
    }
}