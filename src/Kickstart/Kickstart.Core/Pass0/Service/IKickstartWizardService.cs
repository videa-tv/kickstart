using Kickstart.Pass0.Model;
using Kickstart.Pass1.KModel;

namespace Kickstart.Wizard.Service
{
    public interface IKickstartWizardService
    {
        KSolution BuildSolution(KickstartWizardModel kickstartWizardModel);
        KDataStoreProject BuildDatabaseProject(KickstartWizardModel kickstartWizardModel, KProtoFile kProtoFile);


    }
}