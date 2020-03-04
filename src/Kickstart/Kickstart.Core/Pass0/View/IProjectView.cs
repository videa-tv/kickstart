using System;
using System.Threading.Tasks;

namespace Kickstart.Wizard.View
{
    public interface IProjectView : IView
    {
        Func<object, EventArgs, Task> ProjectNameChanged { get; set; }

        string CompanyName { get; set; }
        string ProjectName { get; set; }
        string SolutionName { get; set; }
        
    }
}