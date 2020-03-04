using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickstart.Vsix.Wizard.Service
{
    public interface IMessageBoxDisplayService
    {
        void Show(string message);
    }
}
