using Kickstart.Interface;
using Kickstart.Pass2.CModel.Code;
using Microsoft.Extensions.Logging;
namespace Kickstart.Pass3.VisualStudio2017
{
    public class CProjectContentVisitor : ICProjectContentVisitor
    {
        private readonly ILogger _logger;

        public CProjectContentVisitor(ILogger<CProjectContentVisitor> logger)
        {
            _logger = logger;
        }

        public void Visit(IVisitor visitor, CProjectContent projectContent)
        {
            if (projectContent.Content != null)
            {
                
                _logger.LogInformation($"Visiting {GetType()}, ProjectContent: Unknown");

                projectContent.Content.Accept(visitor);
                _logger.LogInformation($"Visted {GetType()}, ProjectContent: Unknown");
            }
            projectContent.File.Accept(visitor);
        }
    }
}