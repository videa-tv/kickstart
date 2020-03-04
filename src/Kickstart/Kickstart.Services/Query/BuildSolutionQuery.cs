using System;
using System.Collections.Generic;
using Kickstart.Pass0.Model;
using MediatR;

namespace Kickstart.Services.Query
{
    public class BuildSolutionQuery : IRequest<bool>
    {
        public KickstartWizardModel KickstartModel { get; set; } 
    }
}
