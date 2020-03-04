using System;
using System.Threading.Tasks;
using Kickstart.Pass1.KModel;

namespace Kickstart.Wizard.View
{
    public interface IProtoFileView : IView
    {
        Func<Object, EventArgs, Task> ProtoTextChanged { get; set; }
        
        string ProtoFileText { get; }

       
    }
}