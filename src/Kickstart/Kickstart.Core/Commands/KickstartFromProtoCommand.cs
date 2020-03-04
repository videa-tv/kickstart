using System;
using Microsoft.Extensions.Logging;

namespace Kickstart.Commands
{
    public class KickstartFromProtoCommand : ICommand<KickstartFromProtoOptions>
    {
       
        private readonly ILogger<MetaRepoCreateCommand> _logger;

        public KickstartFromProtoCommand(
            ILogger<MetaRepoCreateCommand> logger)
        {
          
            _logger = logger;
           
        }

        public CommandResult Run(KickstartFromProtoOptions options)
        {

            throw new NotImplementedException();
        }
    }

    public class KickstartFromProtoOptions
    {
    }
}
