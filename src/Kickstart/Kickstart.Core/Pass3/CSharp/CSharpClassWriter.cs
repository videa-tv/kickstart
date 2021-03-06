using Kickstart.Interface;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass3.VisualStudio2017;
using Kickstart.Utility;
using Microsoft.Extensions.Logging;

namespace Kickstart.Pass3.CSharp
{
    public class CSharpClassWriter : IWriter
    {
        #region Methods

        private readonly ILogger _logger;
        private readonly ICVisualStudioVisitorBase _visualStudioVisitorBase;
        public CSharpClassWriter(ILogger<CSharpClassWriter> logger, ICVisualStudioVisitorBase visualStudioVisitorBase)
        {
            _logger = logger;
            _visualStudioVisitorBase = visualStudioVisitorBase;

        }
        public void Write(CClass value, ICodeWriter codeWriter)
        {
            
            var rootPath = @"c:\temp\";

            var fileWriter = new FileWriter(rootPath);

            var visitor = _visualStudioVisitorBase;


            visitor.VisitCClass(value);

            var code = codeWriter.ToString();
        }

        #endregion Methods

        #region Fields

        #endregion Fields

        #region Properties

        #endregion Properties

        #region Constructors

        #endregion Constructors
    }
}