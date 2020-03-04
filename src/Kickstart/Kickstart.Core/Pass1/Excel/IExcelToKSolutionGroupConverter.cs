using System.Collections.Generic;
using Kickstart.Pass1.KModel;

namespace Kickstart.Pass1.Excel
{
    public interface IExcelToKSolutionGroupConverter
    {
        List<KSolutionGroup> Convert(string excelFilePath);
    }
}