using System.Collections.Generic;
using Kickstart.Pass1.KModel;

namespace Kickstart.Interface
{
    public interface ISeedDataService
    {
        void AddSeedData(IEnumerable<KView> views);
    }
}