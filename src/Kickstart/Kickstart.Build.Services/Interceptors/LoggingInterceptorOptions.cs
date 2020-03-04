using System;
using System.Collections.Generic;
using System.Text;

namespace Kickstart.Build.Services.Interceptors
{
    public class LoggingInterceptorOptions
    {
        /// <summary>
        /// Enable logging of request items from <see cref="IAsyncStreamReader{T}"/> request stream.
        /// </summary>
        public bool LogAsyncStreamItems { get; set; }
    }
}
