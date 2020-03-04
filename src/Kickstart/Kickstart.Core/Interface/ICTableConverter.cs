using Kickstart.Pass2.CModel.DataStore;

namespace Kickstart.Interface
{
    public interface ICTableToTableConverter
    {
        string Convert(CTable table);
    }
}