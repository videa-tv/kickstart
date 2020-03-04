using System;

namespace Kickstart.Pass1.Service
{
    public class ProtoCompileException : ApplicationException
    {
        public ProtoCompileException(string message) : base(message)
        {
        }
    }
}