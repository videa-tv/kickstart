using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kickstart.Pass1.KModel;

namespace Kickstart
{
    public interface IKickstartService
    {
        event EventHandler<KickstartProgressChangedEventArgs> ProgressChanged;

         Task ExecuteAsync(string destinationDirectory, string solutionName, List<KSolutionGroup> solutionGroupList);
    }
}