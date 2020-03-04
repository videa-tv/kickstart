using System;
using System.Collections.Generic;
using System.Text;

namespace Kickstart.Build.Services.GrpcCommon
{
    public class GrpcServerOptions
    {
        /// <summary>
        /// Max IDLE connection timeout. After that server sends GOAWAY to the client and gracefully terminates connection.
        /// Clients would initiate new connection again.
        /// </summary>
        public int MaxConnectionIdleMs { get; set; }

        /// <summary>
        /// Max connection lifetime. After that server sends GOAWAY to the client and gracefully terminates connection.
        /// Clients would initiate new connection again
        /// </summary>
        public int MaxConnectionAgeMs { get; set; }
    }
}
