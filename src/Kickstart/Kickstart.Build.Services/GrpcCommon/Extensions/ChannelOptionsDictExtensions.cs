using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kickstart.Build.Services.GrpcCommon.Extensions
{
    internal static class ChannelOptionsDictExtensions
    {
        public static void SetOption(this Dictionary<string, ChannelOption> opts, ChannelOption option)
        {
            option = option ?? throw new ArgumentNullException(nameof(option));
            opts[option.Name] = option;
        }
    }
}
