using CefSharp.WinForms;
using Kickstart.Wizard.Presenter;
using Kickstart.Wizard.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kickstart.Vsix.Wizard
{
    public partial class KickstartWizardDialogWeb : Form, IKickstartWizardView
    {
        ChromiumWebBrowser _browser;
        public KickstartWizardDialogWeb()
        {
            InitializeComponent();
            if (!CefSharp.Cef.IsInitialized)
            {
                CefSharp.Cef.Initialize();
            }
            _browser = new ChromiumWebBrowser("http://localhost:56565/kickstartwizard") { Dock = DockStyle.Fill };
            //_browser = new ChromiumWebBrowser("http://localhost:24430/kickstartwizard/menu/") { Dock = DockStyle.Fill };
            _browser.LoadingStateChanged += Browser_LoadingStateChanged;
            _panelBrowser.Controls.Add(_browser);
        }

        private void Browser_LoadingStateChanged(object sender, CefSharp.LoadingStateChangedEventArgs e)
        {
            if (!e.IsLoading)
            {
                
            }
        }

        public Func<object, EventArgs, Task> NextClicked { get; set; }
        public IProjectView ProjectView { get; internal set; }
        IKickstartWizardPresenter IKickstartWizardView.Tag { get; set; }

        public event EventHandler PreviousStep;

        public void AddView(IView menuView)
        {
           // throw new NotImplementedException();
        }

        public void CloseWizard()
        {
           // throw new NotImplementedException();
        }

        public void DisableNext()
        {
            //throw new NotImplementedException();
        }

        public void DisablePrevious()
        {
            //throw new NotImplementedException();
        }

        public void EnableFinish()
        {
            //throw new NotImplementedException();
        }

        public void EnableGenerate()
        {
            //throw new NotImplementedException();
        }

        public void EnableNext()
        {
            //throw new NotImplementedException();
        }

        public int ShowDialog(IntPtr handle)
        {
            return (int)ShowDialog(new MyWin32Window() { Handle = handle });
        }
    }
    class MyWin32Window : IWin32Window
    {
        public IntPtr Handle { get; set; }
    }

}
