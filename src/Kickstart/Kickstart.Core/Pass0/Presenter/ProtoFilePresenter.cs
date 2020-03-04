using Kickstart.Wizard.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickstart.Vsix.Wizard.Presenter
{
    class ProtoFilePresenter
    {
        private readonly IProtoFileView _protoFileView;

        public ProtoFilePresenter(IProtoFileView protoFileView)
        {
            _protoFileView = protoFileView;
        }
    }
}
