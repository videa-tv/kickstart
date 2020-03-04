using System;
using Microsoft.Extensions.Logging;

namespace Kickstart.Commands
{
    public class MegaSolutionCreateCommand : ICommand<MetaRepoCreateOptions>
    {
       
        private readonly ILogger<MetaRepoCreateCommand> _logger;

        public MegaSolutionCreateCommand(
            ILogger<MetaRepoCreateCommand> logger)
        {
          
            _logger = logger;
           
        }

        public CommandResult Run(MetaRepoCreateOptions options)
        {

            throw new NotImplementedException();
        }
    }
}
