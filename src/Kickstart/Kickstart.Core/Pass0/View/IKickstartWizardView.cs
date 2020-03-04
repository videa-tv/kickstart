using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kickstart.Pass1.KModel;
using Kickstart.Vsix.Wizard;
using Kickstart.Wizard.Presenter;

namespace Kickstart.Wizard.View
{
    public interface IKickstartWizardView
    {
        event EventHandler Load;
        
        event EventHandler PreviousStep;


        Func<Object, EventArgs, Task> NextClicked { get; set; }
        IKickstartWizardPresenter Tag { get; set; }
        IProjectView ProjectView { get; }

        void DisableNext();
        void EnableFinish();
        void DisablePrevious();
        void EnableNext();
        void EnableGenerate();
        void CloseWizard();
        void AddView(IView menuView);
        int ShowDialog(IntPtr handle);
    }
}