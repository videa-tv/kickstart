using System;
using System.Collections.Generic;
using Kickstart.Interface;

namespace Kickstart.Pass2.CModel.Code
{
    public class CSolutionGroup : CPart
    {
        public List<CSolution> Solution { get; set; } = new List<CSolution>();
        public string SolutionGroupName { get; set; }

        public override void Accept(IVisitor visitor)
        {
            throw new NotImplementedException();
        }
    }
}