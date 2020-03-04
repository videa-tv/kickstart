using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Kickstart.Services.Infrastructure
{
    public static class LogEvents
    {
        public static readonly EventId Generic = new EventId(1000, "generic");
    }
}
