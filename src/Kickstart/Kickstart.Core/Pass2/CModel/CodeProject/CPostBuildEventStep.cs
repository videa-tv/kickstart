using System;
using Kickstart.Interface;

namespace Kickstart.Pass2.CModel.Code
{
    public class CPostBuildEventStep : CPart
    {
        public string Value { get; set; }

        public override void Accept(IVisitor visitor)
        {
            throw new NotImplementedException();
        }
    }
}