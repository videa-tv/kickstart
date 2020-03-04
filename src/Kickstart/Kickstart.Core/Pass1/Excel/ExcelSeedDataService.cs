using System.Collections.Generic;
using Kickstart.Interface;
using Kickstart.Pass1.KModel;

namespace Kickstart.Pass1.Excel
{
    internal class ExcelSeedDataService : ISeedDataService
    {
        public void AddSeedData(IEnumerable<KView> views)
        {
            var converter = new ExcelToRowDataConverter();
            foreach (var kView in views)
            {
                if (string.IsNullOrEmpty(kView.SampleDataExcelFile))
                    continue;
                kView.GeneratedView.Row.AddRange(converter.Convert(kView));
            }
        }
    }
}