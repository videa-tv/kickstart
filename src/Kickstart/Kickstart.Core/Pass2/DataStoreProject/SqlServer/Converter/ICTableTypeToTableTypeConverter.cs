using Kickstart.Pass2.CModel.DataStore;

namespace Kickstart.Pass2.SqlServer
{
    public interface ICTableTypeToTableTypeConverter
    {
        string Convert(CTableType tableType);
    }
}