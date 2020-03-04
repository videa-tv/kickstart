using System.Collections.Generic;
using Kickstart.Pass2.CModel.AWS.DMS;
using Kickstart.Pass2.CModel.DataStore;

namespace Kickstart.Pass2.SqlServer
{
    public interface ICTableToDmsRuleConverter
    {
        List<Rule> Convert(CTable table);
    }
}