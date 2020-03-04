using System;
using System.Collections.Generic;
using Kickstart.Interface;

namespace Kickstart.Pass2.CModel.DataStore
{
    public class CStoredProcList : CPart
    {
        public IList<CStoredProcedure> List { get; set; } = new List<CStoredProcedure>();

        public override void Accept(IVisitor visitor)
        {
            throw new NotImplementedException();
        }
    }
}