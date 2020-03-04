using Kickstart.Pass2.CModel.DataStore;

namespace Kickstart.Pass2.SqlServer
{
    public interface ICSchemaToSchemaConverter
    {
        string Convert(CSchema schema);
    }
}