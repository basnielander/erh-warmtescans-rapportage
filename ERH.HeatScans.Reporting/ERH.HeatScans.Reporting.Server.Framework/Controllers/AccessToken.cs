using System.Net.Http;

namespace ERH.HeatScans.Reporting.Server.Framework.Controllers
{
    internal static class AccessToken
    {
        internal static string Get(HttpRequestMessage request)
        {
            var authHeader = request.Headers.Authorization;
            if (authHeader == null || authHeader.Scheme != "Bearer")
            {
                return null;
            }

            return authHeader.Parameter;
        }
    }
}