using System;
using System.Linq;
using Grpc.Core;
using Kickstart.Build.Services.Config;

namespace Kickstart.Build.Services.Auth
{
    public static class AuthExtensions
    {
        public static readonly string ApiKey = "api_key";
        private const string ErrorMessage = "Please provide a valid api_key";

        public static void CheckAuthenticated(this ServerCallContext context, AuthenticationSettings authSettings)
        {
            if (!authSettings.Enabled)
            {
                return;
            }

            var apiKey =
                context.RequestHeaders.FirstOrDefault(h => h.Key.Equals(ApiKey, StringComparison.OrdinalIgnoreCase));

            if (apiKey == null || !apiKey.Value.Equals(authSettings.ApiKey, StringComparison.OrdinalIgnoreCase))
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, ErrorMessage));
            }

        }

    }

}
